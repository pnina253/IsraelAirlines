
using AutoMapper;
using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.DTOs.Responses;
using IsraelAirlines.core.Entities;
using IsraelAirlines.core.Enums;
using IsraelAirlines.core.Exceptions;
using IsraelAirlines.core.Interfaces.Repository;
using IsraelAirlines.Service.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace IsraelAirlines.Tests.Services;

public class ReservationServiceTests
{
    private const int UserId = 7;
    private const int OtherUserId = 8;

    private readonly Mock<IReservationRepository> _reservationRepository = new();
    private readonly Mock<IFlightRepository> _flightRepository = new();
    private readonly Mock<IFlightSeatRepository> _seatRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly ReservationService _service;

    public ReservationServiceTests()
    {
        _service = new ReservationService(
            _reservationRepository.Object,
            _flightRepository.Object,
            _seatRepository.Object,
            _unitOfWork.Object,
            _mapper.Object,
            NullLogger<ReservationService>.Instance);

        _mapper
            .Setup(m => m.Map<ReservationResponse>(It.IsAny<Reservation>()))
            .Returns((Reservation r) => new ReservationResponse
            {
                Id = r.Id,
                UserId = r.UserId,
                SeatNumber = r.SeatNumber
            });
    }

    private static Flight CreateFlight() => new()
    {
        Id = 1,
        FlightNumber = "FL100",
        Aircraft = new Aircraft { Rows = 30, SeatLayout = "ABC DEF" }
    };

    private static ReservationRequest Request(string seat = "12A") => new()
    {
        FlightNumber = "FL100",
        SeatNumber = seat
    };

    private void SetupFlight()
    {
        _flightRepository
            .Setup(r => r.GetByFlightNumberAsync("FL100", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateFlight());
    }

    private FlightSeat SetupSeat(string seatNumber, SeatStatus status)
    {
        var seat = new FlightSeat { Id = 50, FlightId = 1, SeatNumber = seatNumber, Status = status };

        _seatRepository
            .Setup(r => r.GetForUpdateAsync(1, seatNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(seat);

        return seat;
    }

    private Reservation SetupReservation(int ownerId, string seatNumber = "12A")
    {
        var reservation = new Reservation
        {
            Id = 3,
            UserId = ownerId,
            FlightId = 1,
            Flight = CreateFlight(),
            SeatNumber = seatNumber
        };

        _reservationRepository
            .Setup(r => r.GetForUpdateAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        return reservation;
    }

    private void VerifyNothingSaved()
    {
        _unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ---------- Create: the limited resource ----------

    [Fact]
    public async Task CreateAsync_WhenSeatIsAvailable_ReservesSeatAndSavesOnce()
    {
        SetupFlight();
        var seat = SetupSeat("12A", SeatStatus.Available);
        Reservation? added = null;

        _reservationRepository
            .Setup(r => r.Add(It.IsAny<Reservation>()))
            .Callback<Reservation>(r => added = r);

        var result = await _service.CreateAsync(UserId, Request(" 12a "));

        Assert.Equal(SeatStatus.Reserved, seat.Status);
        Assert.NotNull(added);
        Assert.Equal(UserId, added.UserId);
        Assert.Equal(1, added.FlightId);
        Assert.Equal("12A", added.SeatNumber);
        Assert.Equal("FL100", result.FlightNumber);
        Assert.Equal("12A", result.SeatNumber);

        // The seat change and the reservation are written together.
        _unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenSeatIsAlreadyReserved_ThrowsConflictAndSavesNothing()
    {
        SetupFlight();
        SetupSeat("12A", SeatStatus.Reserved);

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => _service.CreateAsync(UserId, Request()));

        Assert.IsNotType<ConcurrencyConflictException>(exception);
        _reservationRepository.Verify(r => r.Add(It.IsAny<Reservation>()), Times.Never);
        VerifyNothingSaved();
    }

    // Both requests saw the seat as available; the other one saved first.
    [Fact]
    public async Task CreateAsync_WhenAnotherRequestTookTheSeatFirst_ThrowsConcurrencyConflict()
    {
        SetupFlight();
        SetupSeat("12A", SeatStatus.Available);

        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConcurrencyConflictException("stale version"));

        var exception = await Assert.ThrowsAsync<ConcurrencyConflictException>(
            () => _service.CreateAsync(UserId, Request()));

        Assert.Equal(ReservationService.SeatTakenMessage, exception.Message);
    }

    // The unique (flight, seat) index is the second line of defence.
    [Fact]
    public async Task CreateAsync_WhenUniqueIndexRejectsSecondReservation_ThrowsConcurrencyConflict()
    {
        SetupFlight();
        SetupSeat("12A", SeatStatus.Available);

        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("unique violation"));

        var exception = await Assert.ThrowsAsync<ConcurrencyConflictException>(
            () => _service.CreateAsync(UserId, Request()));

        Assert.Equal(ReservationService.SeatTakenMessage, exception.Message);
    }

    [Fact]
    public async Task CreateAsync_WhenFlightDoesNotExist_ThrowsNotFoundException()
    {
        _flightRepository
            .Setup(r => r.GetByFlightNumberAsync("FL100", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Flight?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.CreateAsync(UserId, Request()));

        VerifyNothingSaved();
    }

    [Theory]
    [InlineData("31A")]
    [InlineData("12G")]
    [InlineData("0A")]
    [InlineData("A12")]
    public async Task CreateAsync_WhenSeatDoesNotExistOnAircraft_ThrowsBadRequestException(
        string seatNumber)
    {
        SetupFlight();

        await Assert.ThrowsAsync<BadRequestException>(
            () => _service.CreateAsync(UserId, Request(seatNumber)));

        VerifyNothingSaved();
    }

    [Fact]
    public async Task CreateAsync_WhenSeatRowIsMissing_ThrowsBadRequestException()
    {
        SetupFlight();

        _seatRepository
            .Setup(r => r.GetForUpdateAsync(1, "12A", It.IsAny<CancellationToken>()))
            .ReturnsAsync((FlightSeat?)null);

        await Assert.ThrowsAsync<BadRequestException>(
            () => _service.CreateAsync(UserId, Request()));

        VerifyNothingSaved();
    }

    [Fact]
    public async Task CreateAsync_PassesCancellationTokenToEveryCall()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        SetupFlight();
        SetupSeat("12A", SeatStatus.Available);

        await _service.CreateAsync(UserId, Request(), token);

        _flightRepository.Verify(r => r.GetByFlightNumberAsync("FL100", token), Times.Once);
        _seatRepository.Verify(r => r.GetForUpdateAsync(1, "12A", token), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(token), Times.Once);
    }

    // ---------- Change seat ----------

    [Fact]
    public async Task ChangeSeatAsync_WhenNewSeatIsAvailable_MovesTheReservation()
    {
        var reservation = SetupReservation(UserId, "12A");
        var oldSeat = SetupSeat("12A", SeatStatus.Reserved);
        var newSeat = SetupSeat("14C", SeatStatus.Available);

        var result = await _service.ChangeSeatAsync(
            3, UserId, isAdmin: false, new ReservationUpdateRequest { SeatNumber = "14c" });

        Assert.Equal(SeatStatus.Available, oldSeat.Status);
        Assert.Equal(SeatStatus.Reserved, newSeat.Status);
        Assert.Equal("14C", reservation.SeatNumber);
        Assert.Equal("14C", result.SeatNumber);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangeSeatAsync_WhenNewSeatIsReserved_ThrowsConflictAndKeepsOldSeat()
    {
        var reservation = SetupReservation(UserId, "12A");
        var oldSeat = SetupSeat("12A", SeatStatus.Reserved);
        SetupSeat("14C", SeatStatus.Reserved);

        await Assert.ThrowsAsync<ConflictException>(() => _service.ChangeSeatAsync(
            3, UserId, isAdmin: false, new ReservationUpdateRequest { SeatNumber = "14C" }));

        Assert.Equal(SeatStatus.Reserved, oldSeat.Status);
        Assert.Equal("12A", reservation.SeatNumber);
        VerifyNothingSaved();
    }

    [Fact]
    public async Task ChangeSeatAsync_ByAnotherUser_ThrowsForbidden()
    {
        SetupReservation(OtherUserId);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.ChangeSeatAsync(
            3, UserId, isAdmin: false, new ReservationUpdateRequest { SeatNumber = "14C" }));

        VerifyNothingSaved();
    }

    // ---------- Cancel ----------

    [Fact]
    public async Task CancelAsync_ByOwner_ReleasesSeatAndRemovesReservation()
    {
        var reservation = SetupReservation(UserId);
        var seat = SetupSeat("12A", SeatStatus.Reserved);

        await _service.CancelAsync(3, UserId, isAdmin: false);

        Assert.Equal(SeatStatus.Available, seat.Status);
        _reservationRepository.Verify(r => r.Remove(reservation), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_ByAnotherUser_ThrowsForbidden()
    {
        SetupReservation(OtherUserId);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => _service.CancelAsync(3, UserId, isAdmin: false));

        _reservationRepository.Verify(r => r.Remove(It.IsAny<Reservation>()), Times.Never);
        VerifyNothingSaved();
    }

    [Fact]
    public async Task CancelAsync_ByAdmin_CanCancelAnyReservation()
    {
        var reservation = SetupReservation(OtherUserId);
        SetupSeat("12A", SeatStatus.Reserved);

        await _service.CancelAsync(3, UserId, isAdmin: true);

        _reservationRepository.Verify(r => r.Remove(reservation), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_WhenReservationDoesNotExist_ThrowsNotFound()
    {
        _reservationRepository
            .Setup(r => r.GetForUpdateAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.CancelAsync(3, UserId, isAdmin: false));
    }

    // ---------- Queries ----------

    [Fact]
    public async Task GetByIdAsync_WhenReservationDoesNotExist_ReturnsNull()
    {
        _reservationRepository
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation?)null);

        var result = await _service.GetByIdAsync(999);

        Assert.Null(result);
        _mapper.Verify(m => m.Map<ReservationResponse>(It.IsAny<Reservation>()), Times.Never);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsReservationsForUser()
    {
        var reservations = new List<Reservation> { new() { Id = 1, UserId = UserId, SeatNumber = "12A" } };
        var expected = new List<ReservationResponse> { new() { Id = 1, SeatNumber = "12A" } };

        _reservationRepository
            .Setup(r => r.GetByUserIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservations);
        _mapper
            .Setup(m => m.Map<IReadOnlyList<ReservationResponse>>(reservations))
            .Returns(expected);

        var result = await _service.GetByUserIdAsync(UserId);

        Assert.Single(result);
        Assert.Equal("12A", result[0].SeatNumber);
    }
}
