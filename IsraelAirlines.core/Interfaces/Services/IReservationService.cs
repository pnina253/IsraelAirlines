using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.DTOs.Responses;

namespace IsraelAirlines.core.Interfaces.Services
{
    public interface IReservationService
    {
        Task<IReadOnlyList<ReservationResponse>> GetAllAsync(
            CancellationToken cancellationToken = default);

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

        // Only the owner or an admin may change or cancel a reservation.
        Task<ReservationResponse> ChangeSeatAsync(
            int reservationId,
            int userId,
            bool isAdmin,
            ReservationUpdateRequest request,
            CancellationToken cancellationToken = default);

        Task CancelAsync(
            int reservationId,
            int userId,
            bool isAdmin,
            CancellationToken cancellationToken = default);
    }
}
