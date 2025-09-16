using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface IFriendlyAccountingIdProvider
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        long GenerateNextFriendlyCustomerId();

        [MethodImpl(MethodImplOptions.Synchronized)]
        long GenerateNextFriendlyInvoiceId();

        [MethodImpl(MethodImplOptions.Synchronized)]
        long GenerateNextFriendlyPaymentId();

        [MethodImpl(MethodImplOptions.Synchronized)]
        long GenerateNextFriendlyDueDepositId();
    }
}
