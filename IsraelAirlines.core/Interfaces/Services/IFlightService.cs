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

        Task<PagedResult<FlightResponse>> SearchAsync(
            FlightSearchQuery query,
            CancellationToken cancellationToken = default);

        // Returns null when the flight does not exist.
        Task<IReadOnlyList<string>?> GetReservedSeatsAsync(
            int flightId,
            CancellationToken cancellationToken = default);

        Task<FlightResponse> CreateAsync(
            FlightRequest request,
            CancellationToken cancellationToken = default);
    }
}
