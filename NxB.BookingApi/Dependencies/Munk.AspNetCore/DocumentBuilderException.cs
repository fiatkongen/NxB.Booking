using System;
using Newtonsoft.Json;

namespace Munk.AspNetCore
{
    public class DocumentBuilderException : Exception
    {
        public DocumentBuilderException()
        {
        }

        public DocumentBuilderException(string message)
            : base(message)
        {
        }

        public DocumentBuilderException(string message, Exception inner, object documentDataContext)
                    : base(message + ("documentDataContext: " + JsonConvert.SerializeObject(documentDataContext)), inner)
        {

        }

        public DocumentBuilderException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
