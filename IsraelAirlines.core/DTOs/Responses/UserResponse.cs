using IsraelAirlines.core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace IsraelAirlines.core.DTOs.Responses
{


    public class UserResponse
    {
      
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        //public UserRole Role { get; set; }   - אין צורך למשתמש לדעת את זה רק המערכת צריכה לדעת את זה
    }


}
