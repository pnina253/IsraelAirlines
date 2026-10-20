namespace IsraelAirlines.core.Interfaces.Repository
{
    // Saves every pending change of the current request in one database transaction.
    public interface IUnitOfWork
    {
        // Throws ConcurrencyConflictException when another request changed the same row first,
        // and ConflictException when a unique constraint is violated.
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
