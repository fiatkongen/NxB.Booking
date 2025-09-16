using Microsoft.EntityFrameworkCore;

namespace Munk.AspNetCore
{
    public interface IAppDbContextFactory<out TAppDbContext> where TAppDbContext : DbContext
    {
        TAppDbContext Create(string inMemoryId);
    }
}