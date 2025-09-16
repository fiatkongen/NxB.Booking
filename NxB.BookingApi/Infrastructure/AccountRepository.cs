using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using Munk.AspNetCore.Sql;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Infrastructure
{
    public class AccountRepository : TenantFilteredRepository<Account, AppDbContext>, IAccountRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public AccountRepository(IClaimsProvider claimsProvider, AppDbContext appDbContext, IDbConnectionFactory dbConnectionFactory) : base(claimsProvider, appDbContext)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public void Add(Account account)
        {
            AppDbContext.Add(account);
        }

        public void MarkAsDeleted(Guid id)
        {
            throw new NotImplementedException();
        }

        public Account FindSingle(Guid id)
        {
            var account = this.TenantFilteredEntitiesQuery.Single(x => !x.IsDeleted && x.Id == id);
            return account;
        }

        public Account FindSingleOrDefault(Guid id)
        {
            var account = this.TenantFilteredEntitiesQuery.SingleOrDefault(x => !x.IsDeleted && x.Id == id);
            return account;
        }

        public async Task<IList<Account>> FindAll()
        {
            var accounts = await this.TenantFilteredEntitiesQuery.Where(x => !x.IsDeleted).ToListAsync();
            return accounts;
        }

        public async Task<IList<Account>> FindAllForCustomer(Guid customerId)
        {
            var accounts = await this.TenantFilteredEntitiesQuery.Where(x => !x.IsDeleted && x.CustomerId == customerId).ToListAsync();
            return accounts;
        }

        public async Task<IList<Account>> FindAllForCustomerIncludeDeleted(Guid customerId)
        {
            var accounts = await this.TenantFilteredEntitiesQuery.Where(x => x.CustomerId == customerId).ToListAsync();
            return accounts;
        }

        //public async Task<decimal> CalculateNotInvoicedForOrder(Guid orderId)
        //{
        //    var tenantId = ClaimsProvider.GetTenantId();

        //    using (var sqlConnection = _dbConnectionFactory.Create())
        //    {
        //        var commandText = $@"
        //            SELECT SUM(OrderLine.PricePcs * OrderLine.Number) FROM OrderLine 

        //            INNER JOIN SubOrder ON SubOrder.Id = OrderLine.SubOrderId
        //            INNER JOIN [Order] ON [Order].Id = SubOrder.OrderId AND [Order].TenantId = '{tenantId}'
        //            AND [Order].Id = '{orderId}'
        //            AND OrderLine.Id NOT IN (
	       //             SELECT InvoiceLine.OrderLineId FROM InvoiceLine 
	       //             INNER JOIN InvoiceSubOrder ON OrderLine.SubOrderId = InvoiceSubOrder.Id 
	       //             INNER JOIN Voucher ON Voucher.Id = InvoiceSubOrder.VoucherId AND Voucher.TenantId = '{tenantId}'
        //                AND Voucher.OrderId = '{orderId}'
        //            )";

        //        var cmd = new SqlCommand
        //        {
        //            CommandText =
        //                commandText,
        //            CommandType = CommandType.Text,
        //            Connection = sqlConnection
        //        };

        //        sqlConnection.Open();
        //        var result = (decimal) await cmd.ExecuteScalarAsync();

        //        return result;
        //    }
        //}

        //public Task<decimal> CalculateNotInvoicedForAccount(Guid accountId)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
