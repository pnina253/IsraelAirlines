using IsraelAirlines.core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace IsraelAirlines.core.DTOs.Responses
{

    public class AircraftResponse
    {

        public int Id { get; set; }
        public string Model { get; set; } = string.Empty;

        public string RegistrationNumber { get; set; } = string.Empty;
        //מזהה ייחודי פיזי הצרוב על המטוס


        public bool IsActive { get; set; }
    }
}
