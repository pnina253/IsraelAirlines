using IsraelAirlines.core.Entities;
using IsraelAirlines.core.Enums;
using IsraelAirlines.core.Exceptions;
using IsraelAirlines.data.Context;
using IsraelAirlines.data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IsraelAirlines.Tests.Integration;

// Proves the optimistic concurrency token on FlightSeat, the resource passengers compete for.
// Each test builds its own aircraft, flight and seat, and deletes them afterwards.
[Trait("Category", "Integration")]
public class FlightSeatConcurrencyTests : IAsyncLifetime
{
    private readonly DbContextOptions<DataContext> _options = TestDatabase.Options();
    private int _aircraftId;
    private int _flightId;
    private int _seatId;

    public async Task InitializeAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

        await using var setup = new DataContext(_options);

        var aircraft = new Aircraft
        {
            Model = "Concurrency Test",
            RegistrationNumber = $"T-{suffix}",
            IsActive = true,
            Rows = 1,
            SeatLayout = "A"
        };

        var flight = new Flight
        {
            FlightNumber = $"T{suffix}",
            Aircraft = aircraft,
            Origin = "Test A",
            Destination = "Test B",
            DepartureTime = DateTime.UtcNow.AddDays(30),
            ArrivalTime = DateTime.UtcNow.AddDays(30).AddHours(1),
            DurationMinutes = 60,
            Price = 1m
        };

        var seat = new FlightSeat { Flight = flight, SeatNumber = "1A" };

        setup.FlightSeats.Add(seat);
        await setup.SaveChangesAsync();

        _aircraftId = aircraft.Id;
        _flightId = flight.Id;
        _seatId = seat.Id;
    }

    public async Task DisposeAsync()
    {
        await using var cleanup = new DataContext(_options);

        // Seats are deleted with the flight (cascade).
        await cleanup.Flights.Where(f => f.Id == _flightId).ExecuteDeleteAsync();
        await cleanup.Aircrafts.Where(a => a.Id == _aircraftId).ExecuteDeleteAsync();
    }

    [Fact]
    public async Task TwoContextsReserveTheSameSeat_FirstSaveWins_SecondThrows()
    {
        await using var first = new DataContext(_options);
        await using var second = new DataContext(_options);

        // Both passengers read the seat while it is still available.
        var seatSeenByFirst = await first.FlightSeats.SingleAsync(s => s.Id == _seatId);
        var seatSeenBySecond = await second.FlightSeats.SingleAsync(s => s.Id == _seatId);

        Assert.Equal(SeatStatus.Available, seatSeenByFirst.Status);
        Assert.Equal(SeatStatus.Available, seatSeenBySecond.Status);

        seatSeenByFirst.Status = SeatStatus.Reserved;
        seatSeenBySecond.Status = SeatStatus.Reserved;

        // The first save matches the original Version and changes it.
        await first.SaveChangesAsync();

        // The second UPDATE still carries the old Version, so it matches no row.
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => second.SaveChangesAsync());

        await using var check = new DataContext(_options);
        var stored = await check.FlightSeats.AsNoTracking().SingleAsync(s => s.Id == _seatId);
        Assert.Equal(SeatStatus.Reserved, stored.Status);
    }

    [Fact]
    public async Task UnitOfWork_TranslatesTheLostRaceIntoConcurrencyConflictException()
    {
        await using var first = new DataContext(_options);
        await using var second = new DataContext(_options);

        var seatSeenByFirst = await first.FlightSeats.SingleAsync(s => s.Id == _seatId);
        var seatSeenBySecond = await second.FlightSeats.SingleAsync(s => s.Id == _seatId);

        seatSeenByFirst.Status = SeatStatus.Reserved;
        seatSeenBySecond.Status = SeatStatus.Reserved;

        await new UnitOfWork(first).SaveChangesAsync(CancellationToken.None);

        // This is what the service layer receives, and the API turns into 409.
        await Assert.ThrowsAsync<ConcurrencyConflictException>(
            () => new UnitOfWork(second).SaveChangesAsync(CancellationToken.None));
    }
}
