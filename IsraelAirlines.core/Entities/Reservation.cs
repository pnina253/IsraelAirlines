using System;
using System.Collections.Generic;
using System.Text;

namespace IsraelAirlines.core.Entities
{
    public class Reservation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FlightId { get; set; }
        public string SeatNumber { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;

        public Flight Flight { get; set; } = null!;
    }
}
