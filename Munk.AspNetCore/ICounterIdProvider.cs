using System;
using System.Runtime.CompilerServices;

namespace Munk.AspNetCore
{
    public interface ICounterIdProvider
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        long Next(string id, long seed = 0);

        [MethodImpl(MethodImplOptions.Synchronized)]
        long Next_Shared(string id, long seed = 0);

        long Get(string id, long seed = 0, Guid? tenantId = null);
        void Set(string id, long count, Guid? tenantId = null);
    }
}