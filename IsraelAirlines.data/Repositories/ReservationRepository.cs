using IsraelAirlines.core.Entities;
using IsraelAirlines.core.Interfaces.Repository;
using IsraelAirlines.data.Context;
using Microsoft.EntityFrameworkCore;

namespace IsraelAirlines.data.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly DataContext _context;

        public ReservationRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Reservation?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Reservations
                .AsNoTracking()
                .Include(r => r.Flight)
                .FirstOrDefaultAsync(
                    r => r.Id == id,
                    cancellationToken);
        }

        public async Task<Reservation?> GetForUpdateAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Reservations
                .Include(r => r.Flight)
                .FirstOrDefaultAsync(
                    r => r.Id == id,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<Reservation>> GetByUserIdAsync(
           int userId,
           CancellationToken cancellationToken = default)
        {
            return await _context.Reservations
               .AsNoTracking()
               .Include(r => r.Flight)
               .Where(r => r.UserId == userId)
               .OrderBy(r => r.Flight.DepartureTime)
               .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Reservation>> GetAllAsync(
            CancellationToken cancellationToken)
        {
            return await _context.Reservations
                .AsNoTracking()
                .Include(r => r.Flight)
                .OrderBy(r => r.Id)
                .ToListAsync(cancellationToken);
        }

        public void Add(Reservation reservation)
        {
            _context.Reservations.Add(reservation);
        }

        public void Remove(Reservation reservation)
        {
            _context.Reservations.Remove(reservation);
        }
    }
}
