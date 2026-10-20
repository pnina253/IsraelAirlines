using IsraelAirlines.core.Entities;

namespace IsraelAirlines.core.Interfaces.Repository
{
    public interface IFlightSeatRepository
    {
        // Tracked, because the caller is about to change the seat's status.
        Task<FlightSeat?> GetForUpdateAsync(
            int flightId,
            string seatNumber,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<string>> GetReservedSeatNumbersAsync(
            int flightId,
            CancellationToken cancellationToken);
    }
}
