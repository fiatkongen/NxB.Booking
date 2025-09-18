using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Domain.Common.Exceptions
{
    public class LegacyImporterException : Exception
    {
        public LegacyImporterException()
        {
        }

        public LegacyImporterException(string message)
            : base(message)
        {
        }

        public LegacyImporterException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
