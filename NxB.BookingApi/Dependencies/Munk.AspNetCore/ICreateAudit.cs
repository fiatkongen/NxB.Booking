using System;

namespace Munk.AspNetCore
{
    public interface ICreateAudit
    {
        Guid CreateAuthorId { get; }
        DateTime CreateDate { get; }
    }
}