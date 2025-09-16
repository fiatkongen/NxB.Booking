using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface IDiscountRepository
    {
        void Add(Discount discount);
        void MarkAsDeleted(Guid id);
        Discount FindSingle(Guid id);
        Discount FindSingleOrDefaultFromName(string name);
        void Update(Discount discount);
        Task<IList<Discount>> FindAll();
    }
}
