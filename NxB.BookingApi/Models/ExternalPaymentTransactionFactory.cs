using System;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    public class ExternalPaymentTransactionFactory
    {
        private readonly IClaimsProvider _claimsProvider;

        public ExternalPaymentTransactionFactory(IClaimsProvider claimsProvider)
        {
            _claimsProvider = claimsProvider;
        }

        public ExternalPaymentTransaction Create(string transactionId, string transactionType, decimal amount, string status, Guid voucherId, string jsonResponse, string saleId)
        {
            return new ExternalPaymentTransaction(Guid.NewGuid(), _claimsProvider.GetTenantId(), transactionId, transactionType, amount, status, voucherId, jsonResponse, saleId, _claimsProvider.GetUserId());
        }
    }
}