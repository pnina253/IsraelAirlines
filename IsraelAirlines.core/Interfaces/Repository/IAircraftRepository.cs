using IsraelAirlines.core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace IsraelAirlines.core.Interfaces.Repository
{
    public interface IAircraftRepository
    {
        Task<Aircraft?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<IReadOnlyList<Aircraft>> GetAllAsync(CancellationToken cancellationToken);
        Task AddAsync(Aircraft aircraft, CancellationToken cancellationToken);
        Task <Aircraft?> GetByRegistrationNumberAsync(string registrationNumber, CancellationToken cancellationToken = default);
    }
}
