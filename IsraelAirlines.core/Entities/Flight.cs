using System;
using System.Collections.Generic;
using System.Text;

namespace IsraelAirlines.core.Entities
{
    public class Flight
    {
        public int Id { get; set; }
        // מזהה ייחודי של הטיסה

        public string FlightNumber { get; set; } = string.Empty;
        // מספר הטיסה

        public int AircraftId {  get; set; }
        public Aircraft Aircraft { get; set; } = null!;
        // אובייקט המטוס שמבצע את הטיסה
      

        public string Origin { get; set; } = string.Empty;
        // עיר או שדה התעופה שממנו ממריאים

        public string Destination { get; set; } = string.Empty;
        // עיר או שדה התעופה שאליו טסים

        public DateTime DepartureTime { get; set; }
        // מועד ההמראה

        public DateTime ArrivalTime { get; set; }
        // מועד הנחיתה המתוכנן - מחושב על פי זמן הטיסה בדקות (שדה חישובי)

        public int DurationMinutes { get; set; }

        public decimal Price { get; set; }
        // מחיר כרטיס בשקלים


        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        public ICollection<FlightSeat> Seats { get; set; } = new List<FlightSeat>();
    }
}
