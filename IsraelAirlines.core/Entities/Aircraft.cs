using System;
using System.Collections.Generic;
using System.Text;

namespace IsraelAirlines.core.Entities;

public class Aircraft
{
    public int Id { get; set; }
    // מזהה ייחודי של המטוס

    public string Model { get; set; } = string.Empty;
    // דגם המטוס, למשל Boeing 737

    public string RegistrationNumber { get; set; } = string.Empty;
    // מספר הרישום הייחודי של המטוס

    public bool IsActive { get; set; }
    // האם המטוס פעיל וזמין לשימוש

    public int Rows { get; set; }
    // מספר שורות המושבים במטוס

    public string SeatLayout { get; set; } = string.Empty;
    // אותיות המושבים בכל שורה, רווח מסמן מעבר. למשל "ABC DEF"

    public int BusinessRows { get; set; }
    // מספר השורות הראשונות שהן מחלקת עסקים

    public ICollection<Flight> Flights { get; set; } = new List<Flight>();

    public ICollection<Amenity> Amenities { get; set; } = new List<Amenity>();
}
