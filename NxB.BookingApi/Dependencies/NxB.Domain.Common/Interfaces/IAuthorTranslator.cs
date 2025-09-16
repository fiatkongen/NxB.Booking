using System;
using Microsoft.EntityFrameworkCore;

namespace NxB.Domain.Common.Interfaces
{
    public interface IAuthorTranslator<in TAppDbContext> where TAppDbContext : DbContext
    {
        string GetName(Guid userId, TAppDbContext appDbContext);
    }
}
