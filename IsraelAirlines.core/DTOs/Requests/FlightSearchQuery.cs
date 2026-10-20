using IsraelAirlines.core.Enums;
using System.ComponentModel.DataAnnotations;

namespace IsraelAirlines.core.DTOs.Requests
{
    public class FlightSearchQuery
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        [MaxLength(100)]
        public string? Origin { get; set; }

        [MaxLength(100)]
        public string? Destination { get; set; }

        // Inclusive range of departure dates (UTC days).
        public DateOnly? FromDate { get; set; }

        public DateOnly? ToDate { get; set; }

        public FlightSortField SortBy { get; set; } = FlightSortField.DepartureTime;

        public bool SortDescending { get; set; }
    }
}
