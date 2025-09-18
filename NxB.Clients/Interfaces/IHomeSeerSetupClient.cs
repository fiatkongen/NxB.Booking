using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Clients.Interfaces
{
    public interface IHomeSeerSetupClient : IAuthorizeClient
    {
        public Task<int> SynchronizeDevices();
    }
}
