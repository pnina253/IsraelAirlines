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

    public ICollection<Flight> Flights { get; set; } = new List<Flight>();
}
