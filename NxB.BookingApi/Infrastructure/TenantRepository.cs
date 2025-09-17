using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore.Sql;
using NxB.Domain.Common.Constants;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class TenantRepository : ITenantRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IDbConnectionFactory _connectionFactory;

        public TenantRepository(AppDbContext appDbContext, IDbConnectionFactory connectionFactory)
        {
            _appDbContext = appDbContext;
            _connectionFactory = connectionFactory;
        }

        public void Add(Tenant tenant)
        {
            this._appDbContext.Tenants.Add(tenant);
        }

        public void Delete(Guid id)
        {
            var tenant = this._appDbContext.Tenants.FirstOrDefault(x => x.Id == id);
            if (tenant != null) this._appDbContext.Tenants.Remove(tenant);
        }

        public Tenant FindSingle(Guid id)
        {
            var tenant = this._appDbContext.Tenants.Single(x => x.Id == id);
            return tenant;
        }

        public Tenant FindSingleOrDefault(Guid id)
        {
            var tenant = this._appDbContext.Tenants.SingleOrDefault(x => x.Id == id);
            return tenant;
        }

        public Tenant FindSingleFromClientId(string clientId)
        {
            var tenant = this._appDbContext.Tenants.FirstOrDefault(x => x.ClientId == clientId);
            return tenant;
        }

        public Tenant FindSingleFromLegacyId(string legacyId)
        {
            var tenant = this._appDbContext.Tenants.OrderByDescending(x => x.UseForLegacyOnline).FirstOrDefault(x => x.LegacyId == legacyId);
            return tenant;
        }

        public Tenant FindSingleFromSubDomain(string subDomain)
        {
            var tenant = this._appDbContext.Tenants.SingleOrDefault(x => x.SubDomain == subDomain);
            if (tenant == null)
            {
                var tenants = this._appDbContext.Tenants.Where(x => x.SubDomain != null && x.SubDomain.Contains(subDomain)).ToList();
                foreach (var item in tenants)
                {
                    var match = item.SubDomain.Split(" ").Any(x => x == subDomain);
                    if (match) return item;
                }
            }
            return tenant;
        }

        public Tenant FindSingleFromKioskId(string kioskId)
        {
            var kiosk = this._appDbContext.Kiosks.SingleOrDefault(x => x.HardwareSerialNo == kioskId);
            if (kiosk == null) return null;
            var tenant = FindSingleOrDefault(kiosk.TenantId);
            return tenant;
        }

        public void Update(Tenant tenant)
        {
            this._appDbContext.Tenants.Update(tenant);
        }

        public async Task DeleteBookings(Guid tenantId, bool filterImported)
        {
            string filterImportedSql = "";

            if (filterImported)
            {
                filterImportedSql = " AND NOT [order].importTimeStamp is null ";
            }


            await using var sqlConnection = _connectionFactory.Create();
            var commandText = $@"
                    delete from InvoiceLine WHERE InvoiceLine.InvoiceSubOrderId IN (SELECT InvoiceSubOrder.Id FROM InvoiceSubOrder WHERE InvoiceSubOrder.VoucherId IN (SELECT [Voucher].Id FROM [Order],[Voucher] WHERE [Order].Id=[Voucher].OrderId AND [Voucher].TenantId = '{tenantId}' {filterImportedSql} ))
                    delete from InvoiceSubOrder WHERE InvoiceSubOrder.VoucherId IN (SELECT [Voucher].Id FROM [Order],[Voucher] WHERE [Order].Id=[Voucher].OrderId AND [Voucher].TenantId = '{tenantId}' {filterImportedSql} )
                    delete from [Voucher] WHERE Id IN (SELECT [Voucher].Id FROM [Order],[Voucher] WHERE [Order].Id=[Voucher].OrderId AND [Voucher].TenantId = '{tenantId}' {filterImportedSql} )

                    delete from subOrderDiscount WHERE Id IN (SELECT SubOrderDiscount.Id FROM [order], SubOrder WHERE [order].Id = SubOrder.OrderId AND SubOrderDiscount.SubOrderId = SubOrder.Id AND [Order].TenantId = '{tenantId}' {filterImportedSql}  )
                    delete from availabilitymatrix WHERE TenantId = '{tenantId}'
                    delete from allocation where [End] != '1/1/2050' AND TenantId = '{tenantId}' AND Id IN (SELECT OrderLine.AllocationId FROM [order], SubOrder, OrderLine WHERE [order].Id = SubOrder.OrderId AND SubOrder.Id = OrderLine.SubOrderId AND [Order].TenantId = '{tenantId}' {filterImportedSql} AND OrderLine.Type = 'allocation' )
                    delete from orderline  WHERE Id IN (SELECT OrderLine.Id FROM [order], SubOrder WHERE [order].Id = SubOrder.OrderId AND SubOrder.Id = OrderLine.SubOrderId AND [Order].TenantId = '{tenantId}' {filterImportedSql} )
                    delete from subOrderSection WHERE Id IN (SELECT SubOrderSection.Id FROM [order], SubOrder WHERE [order].Id = SubOrder.OrderId AND SubOrderSection.SubOrderId = SubOrder.Id AND [Order].TenantId = '{tenantId}' {filterImportedSql} )
                    delete from subOrder  WHERE Id IN (SELECT SubOrder.Id FROM [order] WHERE [order].Id = SubOrder.OrderId AND TenantId = '{tenantId}' {filterImportedSql} )
                    delete from [order] WHERE TenantId = '{tenantId}' {filterImportedSql}
                    delete from allocationState WHERE SubOrderId IN (SELECT SubOrder.Id FROM [order], [SubOrder] WHERE [order].Id = SubOrder.OrderId AND [Order].TenantId = '{tenantId}' {filterImportedSql})
                    delete from [Message] WHERE Id IN (SELECT [Message].Id FROM [Order],[Message] WHERE [Order].Id=[Message].OrderId AND [Message].TenantId = '{tenantId}' {filterImportedSql} )
                ";

            var cmd = new SqlCommand
            {
                CommandText =
                    commandText,
                CommandType = CommandType.Text,
                Connection = sqlConnection,
                CommandTimeout = AppConstants.LONG_RUNNING_SQL_TIMEOUT_SECONDS

            };

            sqlConnection.Open();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteSingleBooking(Guid tenantId, string friendlyBookingId)
        {

            string filterBookingIdSql = $" AND [order].friendlyId = {friendlyBookingId} ";

            await using var sqlConnection = _connectionFactory.Create();
            var commandText = $@"
                    delete from InvoiceLine WHERE InvoiceLine.InvoiceSubOrderId IN (SELECT InvoiceSubOrder.Id FROM InvoiceSubOrder WHERE InvoiceSubOrder.VoucherId IN (SELECT [Voucher].Id FROM [Order],[Voucher] WHERE [Order].Id=[Voucher].OrderId AND [Voucher].TenantId = '{tenantId}' {filterBookingIdSql}))
                    delete from InvoiceSubOrder WHERE InvoiceSubOrder.VoucherId IN (SELECT [Voucher].Id FROM [Order],[Voucher] WHERE [Order].Id=[Voucher].OrderId AND [Voucher].TenantId = '{tenantId}' {filterBookingIdSql})
                    delete from [Voucher] WHERE Id IN (SELECT [Voucher].Id FROM [Order],[Voucher] WHERE [Order].Id=[Voucher].OrderId AND [Voucher].TenantId = '{tenantId}' {filterBookingIdSql})

                    delete from subOrderDiscount WHERE Id IN (SELECT SubOrderDiscount.Id FROM [order], SubOrder WHERE [order].Id = SubOrder.OrderId AND SubOrderDiscount.SubOrderId = SubOrder.Id AND[Order].TenantId = '{tenantId}' {filterBookingIdSql} )
                    delete from availabilitymatrix WHERE TenantId = '{tenantId}'
                    delete from allocation where [End] != '1/1/2050' AND TenantId = '{tenantId}' AND Id IN (SELECT OrderLine.AllocationId FROM [order], SubOrder, OrderLine WHERE [order].Id = SubOrder.OrderId AND SubOrder.Id = OrderLine.SubOrderId AND [Order].TenantId = '{tenantId}' {filterBookingIdSql} AND OrderLine.Type = 'allocation')
                    delete from orderline  WHERE Id IN (SELECT OrderLine.Id FROM [order], SubOrder WHERE [order].Id = SubOrder.OrderId AND SubOrder.Id = OrderLine.SubOrderId AND [Order].TenantId = '{tenantId}' {filterBookingIdSql})
                    delete from subOrderSection WHERE Id IN (SELECT SubOrderSection.Id FROM [order], SubOrder WHERE [order].Id = SubOrder.OrderId AND SubOrderSection.SubOrderId = SubOrder.Id AND [Order].TenantId = '{tenantId}' {filterBookingIdSql})
                    delete from subOrder  WHERE Id IN (SELECT SubOrder.Id FROM [order] WHERE [order].Id = SubOrder.OrderId AND TenantId = '{tenantId}' {filterBookingIdSql})
                    delete from [order] WHERE TenantId = '{tenantId}'  {filterBookingIdSql}
                    delete from allocationState WHERE SubOrderId IN (SELECT SubOrder.Id FROM [order], [SubOrder] WHERE [order].Id = SubOrder.OrderId AND [Order].TenantId = '{tenantId}' {filterBookingIdSql})
                    delete from [Message] WHERE Id IN (SELECT [Message].Id FROM [Order],[Message] WHERE [Order].Id=[Message].OrderId AND [Message].TenantId = '{tenantId}' {filterBookingIdSql})
                ";

            var cmd = new SqlCommand
            {
                CommandText =
                    commandText,
                CommandType = CommandType.Text,
                Connection = sqlConnection,
                CommandTimeout = AppConstants.LONG_RUNNING_SQL_TIMEOUT_SECONDS

            };

            sqlConnection.Open();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteVouchers(Guid tenantId)
        {
            await using var sqlConnection = _connectionFactory.Create();
            var commandText = $@"
                    delete from Voucher WHERE TenantId = '{tenantId}'
                    delete from InvoiceSubOrder WHERE Id IN (SELECT InvoiceSubOrder.Id FROM InvoiceSubOrder, Voucher WHERE Voucher.Id = InvoiceSubOrder.VoucherId AND Voucher.TenantId = '{tenantId}')
                    delete from InvoiceLine WHERE TenantId = '{tenantId}'
                ";

            var cmd = new SqlCommand
            {
                CommandText =
                    commandText,
                CommandType = CommandType.Text,
                Connection = sqlConnection,
                CommandTimeout = AppConstants.LONG_RUNNING_SQL_TIMEOUT_SECONDS

            };

            sqlConnection.Open();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteMessages(Guid tenantId)
        {
            await using var sqlConnection = _connectionFactory.Create();
            var commandText = $@"
                    delete from Message WHERE TenantId = '{tenantId}'
                ";

            var cmd = new SqlCommand
            {
                CommandText =
                    commandText,
                CommandType = CommandType.Text,
                Connection = sqlConnection,
                CommandTimeout = AppConstants.LONG_RUNNING_SQL_TIMEOUT_SECONDS

            };

            sqlConnection.Open();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteCustomers(Guid tenantId)
        {
            await using var sqlConnection = _connectionFactory.Create();
            var commandText = $@"
                    delete from Customer WHERE TenantId = '{tenantId}'
                ";

            var cmd = new SqlCommand
            {
                CommandText =
                    commandText,
                CommandType = CommandType.Text,
                Connection = sqlConnection,
                CommandTimeout = AppConstants.LONG_RUNNING_SQL_TIMEOUT_SECONDS

            };

            sqlConnection.Open();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Tenant>> FindAll()
        {
            var tenants = await this._appDbContext.Tenants.OrderBy(x => x.CompanyName).ToListAsync();
            return tenants;
        }

        public async Task<List<Tenant>> FindAllActive()
        {
            var tenants = await this._appDbContext.Tenants.Where(x => !x.IsDeleted).OrderBy(x => x.CompanyName).ToListAsync();
            return tenants;
        }
    }
}