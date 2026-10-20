using System;
using System.Collections.Generic;
using System.Text;

namespace IsraelAirlines.core.DTOs.Responses
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserResponse User { get; set; } = new();

        // The role the server assigned; read-only information for the client UI.
        public string Role { get; set; } = string.Empty;
    }
}
