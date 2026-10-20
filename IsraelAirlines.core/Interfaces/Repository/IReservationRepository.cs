using IsraelAirlines.core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace IsraelAirlines.core.Interfaces.Repository
{
    public interface IReservationRepository
    {
        Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<IReadOnlyList<Reservation>> GetByUserIdAsync(int userId, CancellationToken cancellationToken);
        Task<bool> IsSeatReservedAsync(
            int flightId,
            string seatNumber,
            CancellationToken cancellationToken);
        Task<IReadOnlyList<string>> GetReservedSeatsAsync(
            int flightId,
            CancellationToken cancellationToken);
        Task AddAsync(Reservation reservation, CancellationToken cancellationToken);
        Task<IReadOnlyList<Reservation>> GetAllAsync(
        CancellationToken cancellationToken);
    }

}
