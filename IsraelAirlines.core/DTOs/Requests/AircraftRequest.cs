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
    }
}
