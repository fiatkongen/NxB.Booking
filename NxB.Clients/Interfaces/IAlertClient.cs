using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Clients.Interfaces
{
    public interface IAlertClient
    {
        public Task SendSmsToSupport(string text);
    }
}
