using System;
using System.Collections.Generic;
using System.Text;

namespace IsraelAirlines.core.DTOs.Responses
{
    public class ReservationResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;

        public string SeatNumber { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
