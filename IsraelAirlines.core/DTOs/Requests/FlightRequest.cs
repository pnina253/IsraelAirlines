using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IsraelAirlines.core.DTOs.Requests
{
    public class FlightRequest
    {
        [Required]
        [MaxLength(20)]
        public string FlightNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Origin { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Destination { get; set; } = string.Empty;

        [Required]
        public DateTime? DepartureTime { get; set; }

        [Required]
        [Range(1,3000)]
        public int DurationMinutes { get; set; }

        [Required]
        [MaxLength(20)]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required]
        [Range(typeof(decimal), "0.01", "100000")]
        public decimal? Price { get; set; }

    }
}
