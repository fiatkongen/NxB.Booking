using Microsoft.EntityFrameworkCore;

namespace NxB.Domain.Common.Interfaces
{
    public interface IEntitySaved
    {
        void OnEntitySaved(EntityState entityState);
    }
}