namespace IsraelAirlines.core.Entities;

// A service available on board, e.g. Wi-Fi. Many-to-many with Aircraft.
public class Amenity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Aircraft> Aircraft { get; set; } = new List<Aircraft>();
}
