using IsraelAirlines.core.Entities;

namespace IsraelAirlines.core.Interfaces.Repository
{
    public interface IReservationRepository
    {
        Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken);

        // Tracked, for update and delete.
        Task<Reservation?> GetForUpdateAsync(int id, CancellationToken cancellationToken);

        Task<IReadOnlyList<Reservation>> GetByUserIdAsync(int userId, CancellationToken cancellationToken);

        Task<IReadOnlyList<Reservation>> GetAllAsync(CancellationToken cancellationToken);

        // Add and Remove only stage the change; IUnitOfWork.SaveChangesAsync writes it.
        void Add(Reservation reservation);

        void Remove(Reservation reservation);
    }

}
