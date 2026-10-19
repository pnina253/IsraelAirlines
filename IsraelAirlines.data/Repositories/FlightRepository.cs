using IsraelAirlines.core.Entities;
using IsraelAirlines.core.Interfaces.Repository;
using IsraelAirlines.data.Contex;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IsraelAirlines.data.Repositories
{
    public class FlightRepository : IFlightRepository
    {
        private readonly DataContext _context;

        public FlightRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Flight?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Flights
            .Include(f => f.Aircraft)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Flight>> SearchAsync(
       string? origin,
       string? destination,
       CancellationToken cancellationToken = default)
        {
            var query = _context.Flights
    .AsNoTracking()
    .Include(f => f.Aircraft)
    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(origin))
            {
                query = query.Where(f => f.Origin == origin);
            }

            if (!string.IsNullOrWhiteSpace(destination))
            {
                query = query.Where(f => f.Destination == destination);
            }

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<Flight?> GetByFlightNumberAsync(
            string flightNumber,
            CancellationToken cancellationToken)
        {
            return await _context.Flights
                .Include(f => f.Aircraft)
                .FirstOrDefaultAsync(
                    f => f.FlightNumber == flightNumber,
                    cancellationToken);
        }

        public async Task AddAsync(
            Flight flight,
            CancellationToken cancellationToken = default)
        {
            await _context.Flights.AddAsync(flight, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
