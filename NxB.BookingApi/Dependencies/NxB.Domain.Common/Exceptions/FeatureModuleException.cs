using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Domain.Common.Exceptions
{
    public class FeatureModuleException : Exception
    {
        public FeatureModuleException() { }

        public FeatureModuleException(string message)
            : base(message)
        {
        }

        public FeatureModuleException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
