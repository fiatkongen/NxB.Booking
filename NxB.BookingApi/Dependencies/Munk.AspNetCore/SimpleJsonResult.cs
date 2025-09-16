using System;
using System.Collections.Generic;
using System.Text;

namespace Munk.AspNetCore
{
    public class SimpleJsonResult
    {
        public object Result { get; set; }

        public SimpleJsonResult(object result)
        {
            Result = result;
        }
    }
}
