using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace NxB.BookingApi.Models
{
    public interface IAccessRepository
    {
        void Add(Access access);
        Task Remove(Guid id);
        Task<Access> FindAccess(Guid id);
        Task<Access> FindSingleOrDefaultAccess(Guid id);
        Task<Access> FindActiveAccessFromCode(int code);
        Task<Access> FindAutoActivationAccessFromCode(int code);
        Task<Access> FindAccessOrDefault(Guid id);
        Task<Access> MarkAsDeleted(Guid id);
        Task<Access> Deactivate(Guid id);
        Task<Access> DeactivateFromCode(int code);
        Task<Access> Reactivate(Guid id);
        Task<List<Access>> FindAllActive();
        Task<List<Access>> FindAllInActive(DateTime deactivationDate);
        Task<List<Access>> FindFromSubOrderId(Guid subOrderId, bool? isKeyCode = null); 
    }
}
