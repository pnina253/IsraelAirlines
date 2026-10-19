using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace IsraelAirlines.core.Interfaces.Services
{
    public interface IFlightService
    {
        Task<FlightResponse?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<FlightResponse>> SearchAsync(
            string? origin,
            string? destination,
            CancellationToken cancellationToken = default);

        Task<FlightResponse> CreateAsync(
            FlightRequest request,
            CancellationToken cancellationToken = default);
    }
}
