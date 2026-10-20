using IsraelAirlines.core.Exceptions;
using IsraelAirlines.core.Interfaces.Repository;
using IsraelAirlines.data.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace IsraelAirlines.data.Repositories
{
    // Translates EF failures into Core exceptions, so the Service layer never references EF Core.
    public class UnitOfWork : IUnitOfWork
    {
        private static readonly Dictionary<string, string> UniqueConstraintMessages = new()
        {
            ["IX_Reservations_FlightId_SeatNumber"] = "This seat is already reserved for the selected flight.",
            ["IX_FlightSeats_FlightId_SeatNumber"] = "This seat already exists on the flight.",
            ["IX_Flights_FlightNumber"] = "A flight with this number already exists.",
            ["IX_Aircrafts_RegistrationNumber"] = "An aircraft with this registration number already exists.",
            ["IX_Users_Email"] = "A user with this email already exists."
        };

        private readonly DataContext _context;

        public UnitOfWork(DataContext context)
        {
            _context = context;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                // One SaveChanges is one transaction: all staged changes are written, or none.
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                // A row we read was changed by another request before we saved.
                throw new ConcurrencyConflictException(
                    "The data was changed by another user. Refresh and try again.");
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is PostgresException
                {
                    SqlState: PostgresErrorCodes.UniqueViolation
                } postgresException)
            {
                var message = postgresException.ConstraintName is { } name
                    && UniqueConstraintMessages.TryGetValue(name, out var known)
                    ? known
                    : "The record conflicts with an existing one.";

                throw new ConflictException(message);
            }
        }
    }
}
