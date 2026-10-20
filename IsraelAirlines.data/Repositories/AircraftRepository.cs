using IsraelAirlines.core.Entities;
using IsraelAirlines.core.Exceptions;
using IsraelAirlines.core.Interfaces.Repository;
using IsraelAirlines.data.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace IsraelAirlines.data.Repositories
{
    public class AircraftRepository : IAircraftRepository
    {
        private readonly DataContext _context;

        public AircraftRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Aircraft?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Aircrafts
                .AsNoTracking()
                .Include(a => a.Amenities)
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        public async Task<Aircraft?> GetForUpdateAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Aircrafts
                .Include(a => a.Amenities)
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Aircraft>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Aircrafts
                .AsNoTracking()
                .Include(a => a.Amenities)
                .OrderBy(a => a.Id)
                .ToListAsync(cancellationToken);
        }

        // Tracked on purpose: a new flight references the returned aircraft.
        public async Task<Aircraft?> GetByRegistrationNumberAsync(
            string registrationNumber,
            CancellationToken cancellationToken = default)
        {
            return await _context.Aircrafts
                .Include(a => a.Amenities)
                .FirstOrDefaultAsync(a => a.RegistrationNumber == registrationNumber, cancellationToken);
        }

        public async Task<bool> HasFlightsAsync(
            int aircraftId,
            CancellationToken cancellationToken)
        {
            return await _context.Flights
                .AnyAsync(f => f.AircraftId == aircraftId, cancellationToken);
        }

        public async Task AddAsync(
            Aircraft aircraft,
            CancellationToken cancellationToken = default)
        {
            await _context.Aircrafts.AddAsync(aircraft, cancellationToken);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is PostgresException
                {
                    SqlState: PostgresErrorCodes.UniqueViolation
                } postgresException
                && postgresException.ConstraintName ==
                    "IX_Aircrafts_RegistrationNumber")
            {
                throw new ConflictException(
                    "An aircraft with this registration number already exists.");
            }
        }

        public void Remove(Aircraft aircraft)
        {
            _context.Aircrafts.Remove(aircraft);
        }
    }
}
