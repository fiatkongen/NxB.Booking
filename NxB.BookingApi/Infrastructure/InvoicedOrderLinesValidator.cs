using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NxB.Domain.Common.Interfaces;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class InvoicedOrderLinesValidator : IInvoicedOrderLinesValidator
    {
        private readonly AppDbContext _appDbContext;
        private readonly IClaimsProvider _claimsProvider;

        public InvoicedOrderLinesValidator(IClaimsProvider claimsProvider, AppDbContext appDbContext)
        {
            _claimsProvider = claimsProvider;
            _appDbContext = appDbContext;
        }

        public async Task ValidateInvoicedOrderLines(Order order)
        {
            var tenantId = _claimsProvider.GetTenantId();
            var dbConnection = _appDbContext.Database.GetDbConnection();
            string commandText = $@"SELECT COUNT(*) FROM OrderLine
                            INNER JOIN SubOrder ON SubOrder.Id = OrderLine.SubOrderId AND OrderLine.IsEqualized=1 AND SubOrder.IsEqualized=0 AND OrderLine.TenantId='{tenantId}'
                            INNER JOIN [Order] ON [Order].Id=SubOrder.OrderId AND [Order].Id='{order.Id}'
                            AND (SELECT TOP(1) Voucher.[Type] FROM InvoiceLine INNER JOIN Voucher ON InvoiceLine.InvoiceBaseId = Voucher.Id AND InvoiceLine.OrderLineId=OrderLine.Id ORDER BY CreateDate DESC) != 'CreditNote'";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = commandText;
            cmd.CommandType = CommandType.Text;
            cmd.Connection = dbConnection;

            await dbConnection.OpenAsync();

            int linesBothEqualizedAndInvoiced = (int)await cmd.ExecuteScalarAsync();

            await dbConnection.CloseAsync();
            if (linesBothEqualizedAndInvoiced > 0)
            {
                throw new CreateOrderException("Kan ikke gemme ordre. Kan ikke udligne en faktureret linie");
            }
        }
    }
}
