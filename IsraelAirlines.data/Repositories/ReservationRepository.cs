using IsraelAirlines.core.Entities;
using IsraelAirlines.core.Interfaces.Repository;
using IsraelAirlines.data.Contex;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

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



        public async Task<IReadOnlyList<Reservation>> GetByUserIdAsync(
           int userId,
           CancellationToken cancellationToken = default)
        {
            return await _context.Reservations
               .AsNoTracking()
               .Include(r => r.Flight)
               .Where(r => r.UserId == userId)
               .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsSeatReservedAsync(
            int flightId,
            string seatNumber,
            CancellationToken cancellationToken = default)
        {
            return await _context.Reservations
                .AnyAsync(
                    r => r.FlightId == flightId &&
                         r.SeatNumber == seatNumber,
                    cancellationToken);
        }

        public async Task AddAsync(
            Reservation reservation,
            CancellationToken cancellationToken = default)
        {
            await _context.Reservations.AddAsync(
                reservation, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Reservation>> GetAllAsync(
            CancellationToken cancellationToken)
        {
            return await _context.Reservations
                .AsNoTracking()
                .Include(r => r.Flight)
                .ToListAsync(cancellationToken);
        }

    }
}
