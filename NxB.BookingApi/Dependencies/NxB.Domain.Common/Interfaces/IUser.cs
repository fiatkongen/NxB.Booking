using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Domain.Common.Interfaces
{
    public interface IUser
    {
        Guid Id { get; set; }
        string Login { get; set; }
        string Username { get; set; }
    }
}
