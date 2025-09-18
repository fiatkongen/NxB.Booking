using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Dto.Exceptions
{
    public class PdfGenerateException : Exception
    {
        public PdfGenerateException()
        {
        }

        public PdfGenerateException(string message)
            : base(message)
        {
        }

        public PdfGenerateException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public PdfGenerateException(Exception inner)
            : base("Kan ikke generere Pdf. Prøv igen senere", inner)
        {
        }
    }
}
