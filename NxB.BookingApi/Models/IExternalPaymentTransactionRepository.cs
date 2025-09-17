using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface IExternalPaymentTransactionRepository
    {
        void Add(ExternalPaymentTransaction externalPaymentTransaction);
        Task<ExternalPaymentTransaction> FindSingleOrDefault(Guid id);
        Task<List<ExternalPaymentTransaction>> FindAll();
        Task<List<ExternalPaymentTransaction>> FindAllFromVoucherId(Guid voucherId);
    }
}