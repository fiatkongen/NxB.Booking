using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public class UserSession
    {
        public string SessionId { get; set; }
        public string UserJson { get; set; }
    }
}
