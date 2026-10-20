
using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.Entities;

namespace IsraelAirlines.core.Interfaces.Repository
{
    public interface IFlightRepository
    {
        Task<Flight?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken);

        // Tracked, for update and delete. Includes the aircraft and the seats.
        Task<Flight?> GetForUpdateAsync(
            int id,
            CancellationToken cancellationToken);

        Task<Flight?> GetByFlightNumberAsync(
            string flightNumber,
            CancellationToken cancellationToken);

        Task<(IReadOnlyList<Flight> Items, int TotalCount)> SearchAsync(
            FlightSearchQuery query,
            CancellationToken cancellationToken);

        Task<bool> HasReservationsAsync(
            int flightId,
            CancellationToken cancellationToken);

        Task AddAsync(
            Flight flight,
            CancellationToken cancellationToken);

        void Remove(Flight flight);
    }
}
