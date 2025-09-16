using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.AutomationApi
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public AuthUser User { get; set; }
    }

    public class AuthUser
    {
        public string Username { get; set; }
        public string Token { get; set; }
    }
}
