using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.Shared
{
    public class ToastDto
    {
        public string Text { get; set; }
        public int DurationSeconds { get; set; }
        public string Style { get; set; }
    }
}
