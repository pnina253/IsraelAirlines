using System.ComponentModel.DataAnnotations;

namespace IsraelAirlines.core.DTOs.Requests
{
    // Moves an existing reservation to another seat on the same flight.
    public class ReservationUpdateRequest
    {
        [Required]
        [MaxLength(5)]
        public string SeatNumber { get; set; } = string.Empty;
    }
}
