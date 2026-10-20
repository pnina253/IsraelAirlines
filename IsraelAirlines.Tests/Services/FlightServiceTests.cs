using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.Entities;
using IsraelAirlines.core.Exceptions;
using IsraelAirlines.core.Interfaces.Repository;
using IsraelAirlines.Service.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace IsraelAirlines.Tests.Services;

public class FlightServiceTests
{
    private readonly Mock<IFlightRepository> _flightRepository = new();
    private readonly Mock<IAircraftRepository> _aircraftRepository = new();
    private readonly Mock<IFlightSeatRepository> _seatRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly FlightService _service;

    public FlightServiceTests()
    {
        _service = new FlightService(
            _flightRepository.Object,
            _aircraftRepository.Object,
            _seatRepository.Object,
            _unitOfWork.Object,
            TestMapper.Create(),
            NullLogger<FlightService>.Instance);
    }

    private static FlightRequest Request(
        DateTime? departure = null,
        string origin = "Tel Aviv",
        string destination = "Rome",
        string registration = "4X-AAA") => new()
    {
        FlightNumber = " IA500 ",
        Origin = origin,
        Destination = destination,
        DepartureTime = departure ?? DateTime.UtcNow.AddDays(30),
        DurationMinutes = 120,
        RegistrationNumber = registration,
        Price = 500m
    };

    private Aircraft SetupAircraft(string registration = "4X-AAA", bool isActive = true)
    {
        var aircraft = new Aircraft
        {
            Id = 1,
            Model = "Boeing 737",
            RegistrationNumber = registration,
            IsActive = isActive,
            Rows = 30,
            SeatLayout = "ABC DEF",
            Amenities = [new Amenity { Name = "Wi-Fi" }]
        };

        _aircraftRepository
            .Setup(r => r.GetByRegistrationNumberAsync(registration, It.IsAny<CancellationToken>()))
            .ReturnsAsync(aircraft);

        return aircraft;
    }

    [Fact]
    public async Task CreateAsync_BuildsOneSeatPerRowAndLetter()
    {
        SetupAircraft();
        Flight? added = null;

        _flightRepository
            .Setup(r => r.AddAsync(It.IsAny<Flight>(), It.IsAny<CancellationToken>()))
            .Callback<Flight, CancellationToken>((f, _) => added = f);

        var result = await _service.CreateAsync(Request());

        Assert.NotNull(added);
        Assert.Equal(30 * 6, added.Seats.Count);
        Assert.Contains(added.Seats, s => s.SeatNumber == "1A");
        Assert.Contains(added.Seats, s => s.SeatNumber == "30F");
        Assert.Equal("IA500", result.FlightNumber);
        Assert.Equal("Boeing 737", result.AircraftModel);
        Assert.Equal(["Wi-Fi"], result.Amenities);
        Assert.Equal(added.DepartureTime.AddMinutes(120), added.ArrivalTime);
    }

    [Fact]
    public async Task CreateAsync_WhenDepartureIsInThePast_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(
            () => _service.CreateAsync(Request(departure: DateTime.UtcNow.AddHours(-1))));
    }

    [Fact]
    public async Task CreateAsync_WhenOriginEqualsDestination_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(
            () => _service.CreateAsync(Request(origin: "Rome", destination: " rome ")));
    }

    [Fact]
    public async Task CreateAsync_WhenAircraftIsInactive_ThrowsBadRequest()
    {
        SetupAircraft(isActive: false);

        await Assert.ThrowsAsync<BadRequestException>(
            () => _service.CreateAsync(Request()));
    }

    [Fact]
    public async Task CreateAsync_WhenFlightNumberExists_ThrowsConflict()
    {
        _flightRepository
            .Setup(r => r.GetByFlightNumberAsync("IA500", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Flight { Id = 9, FlightNumber = "IA500" });

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.CreateAsync(Request()));
    }

    [Fact]
    public async Task DeleteAsync_WhenFlightHasReservations_ThrowsConflict()
    {
        _flightRepository
            .Setup(r => r.GetForUpdateAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Flight { Id = 5 });
        _flightRepository
            .Setup(r => r.HasReservationsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteAsync(5));

        _flightRepository.Verify(r => r.Remove(It.IsAny<Flight>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenFlightDoesNotExist_ReturnsFalse()
    {
        _flightRepository
            .Setup(r => r.GetForUpdateAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Flight?)null);

        Assert.False(await _service.DeleteAsync(5));
    }

    [Fact]
    public async Task UpdateAsync_MovingAFlightWithReservationsToAnotherAircraft_ThrowsConflict()
    {
        var aircraft = SetupAircraft();
        _flightRepository
            .Setup(r => r.GetForUpdateAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Flight { Id = 5, FlightNumber = "IA500", Aircraft = aircraft });
        _flightRepository
            .Setup(r => r.HasReservationsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.UpdateAsync(5, Request(registration: "4X-BBB")));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
