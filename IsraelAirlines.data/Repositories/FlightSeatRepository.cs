using IsraelAirlines.core.Entities;
using IsraelAirlines.core.Enums;
using IsraelAirlines.core.Interfaces.Repository;
using IsraelAirlines.data.Context;
using Microsoft.EntityFrameworkCore;

namespace IsraelAirlines.data.Repositories
{
    public class FlightSeatRepository : IFlightSeatRepository
    {
        private readonly DataContext _context;

        public FlightSeatRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<FlightSeat?> GetForUpdateAsync(
            int flightId,
            string seatNumber,
            CancellationToken cancellationToken)
        {
            return await _context.FlightSeats
                .FirstOrDefaultAsync(
                    s => s.FlightId == flightId && s.SeatNumber == seatNumber,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<string>> GetReservedSeatNumbersAsync(
            int flightId,
            CancellationToken cancellationToken)
        {
            return await _context.FlightSeats
                .AsNoTracking()
                .Where(s => s.FlightId == flightId && s.Status == SeatStatus.Reserved)
                .OrderBy(s => s.SeatNumber)
                .Select(s => s.SeatNumber)
                .ToListAsync(cancellationToken);
        }
    }
}
