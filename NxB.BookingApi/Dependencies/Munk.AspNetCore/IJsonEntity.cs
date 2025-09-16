using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.Text.Common;

namespace Munk.AspNetCore
{
    public interface IJsonEntity
    {
        void Deserialize();
        void Serialize();
    }
}
