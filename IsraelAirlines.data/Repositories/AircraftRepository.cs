using IsraelAirlines.core.Entities;
using IsraelAirlines.core.Interfaces.Repository;
using IsraelAirlines.data.Contex;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

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
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Aircraft>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Aircrafts
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            Aircraft aircraft,
            CancellationToken cancellationToken = default)
        {
            await _context.Aircrafts.AddAsync(aircraft, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public  async Task<Aircraft?> GetByRegistrationNumberAsync(string registrationNumber, CancellationToken cancellationToken = default)
        {
            return await _context.Aircrafts.FirstOrDefaultAsync(r=>r.RegistrationNumber == registrationNumber, cancellationToken);
        }
    }
}
