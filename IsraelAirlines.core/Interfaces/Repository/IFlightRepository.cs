
using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.Entities;

namespace IsraelAirlines.core.Interfaces.Repository
{
    public interface IFlightRepository
    {
        Task<Flight?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken);

        Task<Flight?> GetByFlightNumberAsync(
            string flightNumber,
            CancellationToken cancellationToken);

        Task<(IReadOnlyList<Flight> Items, int TotalCount)> SearchAsync(
            FlightSearchQuery query,
            CancellationToken cancellationToken);

        Task AddAsync(
            Flight flight,
            CancellationToken cancellationToken);
    }
}
