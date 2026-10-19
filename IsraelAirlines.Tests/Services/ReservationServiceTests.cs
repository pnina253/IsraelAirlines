
using AutoMapper;
using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.DTOs.Responses;
using IsraelAirlines.core.Entities;
using IsraelAirlines.core.Exceptions;
using IsraelAirlines.core.Interfaces.Repository;
using IsraelAirlines.Service.Services;
using Moq;
using Xunit;

namespace IsraelAirlines.Tests.Services;

public class ReservationServiceTests
{
    private readonly Mock<IReservationRepository> _reservationRepository;
    private readonly Mock<IFlightRepository> _flightRepository;
    private readonly Mock<IMapper> _mapper;
    private readonly ReservationService _service;

    public ReservationServiceTests()
    {
        _reservationRepository = new Mock<IReservationRepository>();
        _flightRepository = new Mock<IFlightRepository>();
        _mapper = new Mock<IMapper>();

        _service = new ReservationService(
            _reservationRepository.Object,
            _flightRepository.Object,
            _mapper.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenFlightDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var request = new ReservationRequest
        {
            FlightNumber = "FL100",
            SeatNumber = "12A"
        };

        _flightRepository
            .Setup(r => r.GetByFlightNumberAsync(
                request.FlightNumber,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Flight?)null);

        // Act
        var action = () => _service.CreateAsync(request);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(action);

        _reservationRepository.Verify(
            r => r.AddAsync(
                It.IsAny<Reservation>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenSeatIsAlreadyReserved_ThrowsConflictException()
    {
        // Arrange
        var request = new ReservationRequest
        {
            FlightNumber = "FL100",
            SeatNumber = "12A"
        };

        var flight = new Flight
        {
            Id = 1,
            FlightNumber = "FL100"
        };

        _flightRepository
            .Setup(r => r.GetByFlightNumberAsync(
                request.FlightNumber,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(flight);

        _reservationRepository
            .Setup(r => r.IsSeatReservedAsync(
                flight.Id,
                "12A",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var action = () => _service.CreateAsync(request);

        // Assert
        await Assert.ThrowsAsync<ConflictException>(action);

        _reservationRepository.Verify(
            r => r.AddAsync(
                It.IsAny<Reservation>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenSeatIsAvailable_CreatesReservation()
    {
        // Arrange
        var request = new ReservationRequest
        {
            FlightNumber = "FL100",
            SeatNumber = " 12a "
        };

        var flight = new Flight
        {
            Id = 1,
            FlightNumber = "FL100"
        };

        var expectedResponse = new ReservationResponse
        {
            Id = 5,
            FlightNumber = "FL100",
            SeatNumber = "12A",
            CreatedAt = DateTime.UtcNow
        };

        Reservation? savedReservation = null;

        _flightRepository
            .Setup(r => r.GetByFlightNumberAsync(
                request.FlightNumber,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(flight);

        _reservationRepository
            .Setup(r => r.IsSeatReservedAsync(
                flight.Id,
                "12A",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _reservationRepository
            .Setup(r => r.AddAsync(
                It.IsAny<Reservation>(),
                It.IsAny<CancellationToken>()))
            .Callback<Reservation, CancellationToken>(
                (reservation, _) => savedReservation = reservation)
            .Returns(Task.CompletedTask);

        _mapper
            .Setup(m => m.Map<ReservationResponse>(
                It.IsAny<Reservation>()))
            .Returns(expectedResponse);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.NotNull(savedReservation);
        Assert.Equal(1, savedReservation.FlightId);
        Assert.Equal("12A", savedReservation.SeatNumber);

        Assert.Equal("FL100", result.FlightNumber);
        Assert.Equal("12A", result.SeatNumber);

        _reservationRepository.Verify(
            r => r.AddAsync(
                It.IsAny<Reservation>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_PassesCancellationTokenToRepositories()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        var request = new ReservationRequest
        {
            FlightNumber = "FL100",
            SeatNumber = "12A"
        };

        var flight = new Flight
        {
            Id = 1,
            FlightNumber = "FL100"
        };

        _flightRepository
            .Setup(r => r.GetByFlightNumberAsync(
                request.FlightNumber,
                token))
            .ReturnsAsync(flight);

        _reservationRepository
            .Setup(r => r.IsSeatReservedAsync(
                flight.Id,
                "12A",
                token))
            .ReturnsAsync(true);

        // Act
        await Assert.ThrowsAsync<ConflictException>(
            () => _service.CreateAsync(request, token));

        // Assert
        _flightRepository.Verify(
            r => r.GetByFlightNumberAsync(
                request.FlightNumber,
                token),
            Times.Once);

        _reservationRepository.Verify(
            r => r.IsSeatReservedAsync(
                flight.Id,
                "12A",
                token),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenReservationExists_ReturnsReservationResponse()
    {
        // Arrange
        var reservation = new Reservation
        {
            Id = 1,
            FlightId = 1,
            SeatNumber = "12A",
            CreatedAt = DateTime.UtcNow
        };

        var expectedResponse = new ReservationResponse
        {
            Id = 1,
            FlightNumber = "FL100",
            SeatNumber = "12A",
            CreatedAt = reservation.CreatedAt
        };

        _reservationRepository
            .Setup(r => r.GetByIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        _mapper
            .Setup(m => m.Map<ReservationResponse>(reservation))
            .Returns(expectedResponse);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("FL100", result.FlightNumber);
        Assert.Equal("12A", result.SeatNumber);
    }

    [Fact]
    public async Task GetByIdAsync_WhenReservationDoesNotExist_ReturnsNull()
    {
        // Arrange
        _reservationRepository
            .Setup(r => r.GetByIdAsync(
                999,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);

        _mapper.Verify(
            m => m.Map<ReservationResponse>(
                It.IsAny<Reservation>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllReservations()
    {
        // Arrange
        var reservations = new List<Reservation>
    {
        new()
        {
            Id = 1,
            FlightId = 1,
            SeatNumber = "12A",
            CreatedAt = DateTime.UtcNow
        },
        new()
        {
            Id = 2,
            FlightId = 1,
            SeatNumber = "12B",
            CreatedAt = DateTime.UtcNow
        }
    };

        var expectedResponses = new List<ReservationResponse>
    {
        new() { Id = 1, FlightNumber = "FL100", SeatNumber = "12A" },
        new() { Id = 2, FlightNumber = "FL100", SeatNumber = "12B" }
    };

        _reservationRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservations);

        _mapper
            .Setup(m => m.Map<IReadOnlyList<ReservationResponse>>(reservations))
            .Returns(expectedResponses);

        // Act
        var result = await _service.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("12A", result[0].SeatNumber);
        Assert.Equal("12B", result[1].SeatNumber);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsReservationsForUser()
    {
        // Arrange
        const int userId = 2;

        var reservations = new List<Reservation>
    {
        new()
        {
            Id = 1,
            UserId = userId,
            FlightId = 1,
            SeatNumber = "12A",
            CreatedAt = DateTime.UtcNow
        }
    };

        var expectedResponses = new List<ReservationResponse>
    {
        new() { Id = 1, FlightNumber = "FL100", SeatNumber = "12A" }
    };

        _reservationRepository
            .Setup(r => r.GetByUserIdAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservations);

        _mapper
            .Setup(m => m.Map<IReadOnlyList<ReservationResponse>>(reservations))
            .Returns(expectedResponses);

        // Act
        var result = await _service.GetByUserIdAsync(userId);

        // Assert
        Assert.Single(result);
        Assert.Equal("12A", result[0].SeatNumber);

        _reservationRepository.Verify(
            r => r.GetByUserIdAsync(
                userId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task GetAllAsync_WhenNoReservationsExist_ReturnsEmptyList()
    {
        // Arrange
        var reservations = new List<Reservation>();

        var expectedResponses = new List<ReservationResponse>();

        _reservationRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservations);

        _mapper
            .Setup(m => m.Map<IReadOnlyList<ReservationResponse>>(reservations))
            .Returns(expectedResponses);

        // Act
        var result = await _service.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_PassesCancellationTokenToRepository()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _reservationRepository
            .Setup(r => r.GetByIdAsync(1, token))
            .ReturnsAsync((Reservation?)null);

        // Act
        var result = await _service.GetByIdAsync(1, token);

        // Assert
        Assert.Null(result);

        _reservationRepository.Verify(
            r => r.GetByIdAsync(1, token),
            Times.Once);
    }

}
