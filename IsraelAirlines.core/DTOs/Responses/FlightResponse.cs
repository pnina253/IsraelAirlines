using System;
using System.Collections.Generic;
using System.Text;

namespace IsraelAirlines.core.DTOs.Responses
{
    public class FlightResponse
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; } = string.Empty;

        public string Origin { get; set; } = string.Empty;

        public string Destination { get; set; } = string.Empty;

        public DateTime DepartureTime { get; set; }

        public DateTime ArrivalTime { get; set; }
        //מחושב אוטמטית על פי זמן הטיסה
        public int DurationMinutes { get; set; }

        public int AircraftId { get; set; }

        public string AircraftModel { get; set; } = string.Empty;

        public string RegistrationNumber { get; set; } = string.Empty;

        public decimal Price { get; set; }

    }
}
