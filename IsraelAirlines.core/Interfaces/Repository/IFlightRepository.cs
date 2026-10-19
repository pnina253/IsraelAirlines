
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

        Task<IReadOnlyList<Flight>> SearchAsync(
            string? origin,
            string? destination,
            CancellationToken cancellationToken);

        Task AddAsync(
            Flight flight,
            CancellationToken cancellationToken);
    }
}
