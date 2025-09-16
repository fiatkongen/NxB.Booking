using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.Exceptions
{
    public class TenantException : Exception
    {
        public TenantException()
        {
        }

        public TenantException(string message)
            : base(message)
        {
        }

        public TenantException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
