using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IsraelAirlines.core.DTOs.Requests
{
    public class AircraftRequest
    {
        [Required]
        [MaxLength(100)]
        public string Model { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string RegistrationNumber { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        [Range(1, 80)]
        public int Rows { get; set; } = 30;

        // Seat letters per row; a space marks an aisle, e.g. "ABC DEF".
        [Required]
        [MaxLength(20)]
        [RegularExpression("^[A-Za-z]+( [A-Za-z]+)*$",
            ErrorMessage = "Seat layout must contain letters, with single spaces for aisles.")]
        public string SeatLayout { get; set; } = "ABC DEF";

        [Range(0, 80)]
        public int BusinessRows { get; set; }
    }
}
