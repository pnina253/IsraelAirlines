using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace IsraelAirlines.core.Interfaces.Services
{
    public interface IAircraftService
    {
        Task<AircraftResponse?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<AircraftResponse>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<AircraftResponse> CreateAsync(
            AircraftRequest request,
            CancellationToken cancellationToken = default);
    }
}
