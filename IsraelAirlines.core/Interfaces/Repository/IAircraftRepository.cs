using IsraelAirlines.core.Entities;

namespace IsraelAirlines.core.Interfaces.Repository
{
    public interface IAircraftRepository
    {
        Task<Aircraft?> GetByIdAsync(int id, CancellationToken cancellationToken);

        // Tracked, for update and delete.
        Task<Aircraft?> GetForUpdateAsync(int id, CancellationToken cancellationToken);

        Task<IReadOnlyList<Aircraft>> GetAllAsync(CancellationToken cancellationToken);

        // Tracked, because a new flight is attached to the returned aircraft.
        Task<Aircraft?> GetByRegistrationNumberAsync(string registrationNumber, CancellationToken cancellationToken);

        Task<bool> HasFlightsAsync(int aircraftId, CancellationToken cancellationToken);

        Task AddAsync(Aircraft aircraft, CancellationToken cancellationToken);

        void Remove(Aircraft aircraft);
    }
}
