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

    // Every seat number of the aircraft, row by row: "1A", "1B", ... "30F".
    public static IEnumerable<string> AllSeats(Aircraft aircraft)
    {
        var letters = aircraft.SeatLayout.Replace(" ", string.Empty);

        for (var row = 1; row <= aircraft.Rows; row++)
        {
            foreach (var letter in letters)
                yield return $"{row}{letter}";
        }
    }
}
