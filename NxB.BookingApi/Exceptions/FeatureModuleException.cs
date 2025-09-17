using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NxB.BookingApi.Exceptions
{
    public class FeatureModuleException : Exception
    {
        public FeatureModuleException() : base()
        {
        }

        public FeatureModuleException(string message) : base(message)
        {
        }

        public FeatureModuleException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}