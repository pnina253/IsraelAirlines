using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace IsraelAirlines.core.Interfaces.Services
{
    public interface IReservationService
    {
        Task<IReadOnlyList<ReservationResponse>> GetAllAsync(
    CancellationToken cancellationToken);
        Task<ReservationResponse?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ReservationResponse>> GetByUserIdAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<ReservationResponse> CreateAsync(
            int userId,
            ReservationRequest request,
            CancellationToken cancellationToken = default);
    }
}
