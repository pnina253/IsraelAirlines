using System.Text.RegularExpressions;
using IsraelAirlines.core.Entities;

namespace IsraelAirlines.core.Constants;

// Seat numbers are a row number followed by a letter from the aircraft layout, e.g. "12C".
public static partial class SeatMap
{
    [GeneratedRegex("^([1-9][0-9]?)([A-Z])$")]
    private static partial Regex SeatPattern();

    public static bool IsValidSeat(Aircraft aircraft, string seatNumber)
    {
        var match = SeatPattern().Match(seatNumber);

        if (!match.Success)
            return false;

        var row = int.Parse(match.Groups[1].Value);
        var letter = match.Groups[2].Value[0];

        return row <= aircraft.Rows && aircraft.SeatLayout.Contains(letter);
    }
}
