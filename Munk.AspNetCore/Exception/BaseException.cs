using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Munk.AspNetCore.Ex
{
    public class BaseException : Exception
    {
        public bool SkipExceptionLoggingInMiddleware { get; set; }

        public BaseException() { }

        public BaseException(string message) : base(message) { }

        public BaseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
