using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.BookingApi.Models
{
    public class TermsAndConditions
    {
        public Dictionary<string, string> Header { get; set; }
        public Dictionary<string, string> Description { get; set; }
    }
}