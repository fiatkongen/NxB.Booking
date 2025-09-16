using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface IAutoAddRepository
    {
        void Add(AutoAdd autoAdd);
        void Delete(Guid id);
        AutoAdd FindSingle(Guid id);
        Task<List<AutoAdd>> FindAll();
    }
}
