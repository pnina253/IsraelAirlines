
using IsraelAirlines.core.Entities;
using IsraelAirlines.data.Contex;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace IsraelAirlines.Tests.Integration;

public class DatabaseConnectionTests
{
    [Fact]
    public async Task CanConnectToPostgreSql()
    {
        var configuration = new ConfigurationBuilder()
        .AddUserSecrets<DatabaseConnectionTests>()
        .Build();

        var connectionString =
            configuration.GetConnectionString("DefaultConnection");

        Assert.False(
            string.IsNullOrWhiteSpace(connectionString),
            "Connection string was not found.");

        var options = new DbContextOptionsBuilder<DataContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var context = new DataContext(options);

        var canConnect = await context.Database.CanConnectAsync();

        Assert.True(
            canConnect,
            "Could not connect to PostgreSQL.");
    }

    [Fact]
    public async Task ConcurrentUpdates_ShouldThrowConcurrencyException()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<DatabaseConnectionTests>()
            .Build();

        var connectionString =
            configuration.GetConnectionString("DefaultConnection");

        Assert.False(string.IsNullOrWhiteSpace(connectionString));

        var options = new DbContextOptionsBuilder<DataContext>()
            .UseNpgsql(connectionString)
            .Options;

        int userId = 0;
        int reservationId = 0;

        try
        {
            // Create temporary test data
            await using (var setup = new DataContext(options))
            {
                var flightId = await setup.Flights
                    .Select(f => f.Id)
                    .FirstAsync();

                var user = new User
                {
                    FullName = "Concurrency Test",
                    Email = $"concurrency-{Guid.NewGuid():N}@test.com",
                    PasswordHash = "test-hash",
                    Role = "User"
                };

                setup.Users.Add(user);
                await setup.SaveChangesAsync();

                userId = user.Id;

                var reservation = new Reservation
                {
                    UserId = userId,
                    FlightId = flightId,
                    SeatNumber = Guid.NewGuid()
                        .ToString("N")[..5]
                        .ToUpperInvariant(),
                    CreatedAt = DateTime.UtcNow
                };

                setup.Reservations.Add(reservation);
                await setup.SaveChangesAsync();

                reservationId = reservation.Id;
            }

            // Load the same reservation through two separate contexts
            await using var context1 = new DataContext(options);
            await using var context2 = new DataContext(options);

            var reservation1 = await context1.Reservations
                .SingleAsync(r => r.Id == reservationId);

            var reservation2 = await context2.Reservations
                .SingleAsync(r => r.Id == reservationId);

            // The first context updates successfully
            reservation1.CreatedAt = DateTime.UtcNow.AddMinutes(1);
            await context1.SaveChangesAsync();

            // The second context still holds the old Version
            reservation2.CreatedAt = DateTime.UtcNow.AddMinutes(2);

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                () => context2.SaveChangesAsync());
        }
        finally
        {
            // Remove only the temporary test records
            await using var cleanup = new DataContext(options);

            if (reservationId != 0)
            {
                var reservation = await cleanup.Reservations
                    .FindAsync(reservationId);

                if (reservation is not null)
                    cleanup.Reservations.Remove(reservation);
            }

            if (userId != 0)
            {
                var user = await cleanup.Users.FindAsync(userId);

                if (user is not null)
                    cleanup.Users.Remove(user);
            }

            await cleanup.SaveChangesAsync();
        }
    }
}