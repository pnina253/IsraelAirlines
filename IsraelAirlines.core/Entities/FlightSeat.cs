using IsraelAirlines.core.Enums;

namespace IsraelAirlines.core.Entities;

// One physical seat on one flight. This is the limited resource that passengers compete for.
public class FlightSeat
{
    public int Id { get; set; }

    public int FlightId { get; set; }
    public Flight Flight { get; set; } = null!;

    public string SeatNumber { get; set; } = string.Empty;
    // מספר המושב, למשל "12C"

    public SeatStatus Status { get; set; } = SeatStatus.Available;

    // Concurrency token. DataContext replaces it on every update, and EF adds the old
    // value to the UPDATE's WHERE clause, so a write based on a stale read updates no row.
    public Guid Version { get; set; } = Guid.NewGuid();
}
