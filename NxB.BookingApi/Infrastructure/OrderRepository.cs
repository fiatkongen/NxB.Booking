using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Munk.AspNetCore;
using Munk.AspNetCore.Sql;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;
using NxB.BookingApi.Infrastructure;
using ServiceStack;
using Z.EntityFramework.Plus;

namespace NxB.BookingApi.Infrastructure
{
    public class OrderRepository : TenantFilteredRepository<Order, AppDbContext>, IOrderRepository
    {
        public OrderRepository(IClaimsProvider claimsProvider, AppDbContext appDbContext) : base(claimsProvider,
            appDbContext)
        {
        }

        public IOrderRepository CloneWithCustomClaimsProvider(IClaimsProvider overrideClaimsProvider)
        {
            return new OrderRepository(overrideClaimsProvider, AppDbContext);
        }

        protected override IQueryable<Order> TenantFilteredEntitiesQuery
        {
            get
            {
                return base.TenantFilteredEntitiesQuery
                    .Include(x => x.SubOrders).ThenInclude(x => x.SubOrderSections)
                    .Include(x => x.SubOrders).ThenInclude(x => x.SubOrderDiscounts)
                    .IncludeFilter(x => x.SubOrders.SelectMany(ol => ol.OrderLines).Where(ol => !ol.IsEqualized))
                    .IncludeFilter(x => x.SubOrders.SelectMany(ol => ol.OrderLines).Where(ol => !ol.IsEqualized)
                        .OfType<AllocationOrderLine>().Select(aol => aol.Allocation)).AsSingleQuery();
            }
        }

        protected IQueryable<Order> TenantFilteredEntitiesQueryIncludeIsEqualized
        {
            get
            {
                return base.TenantFilteredEntitiesQuery
                    .Include(x => x.SubOrders).ThenInclude(x => x.SubOrderSections)
                    .Include(x => x.SubOrders).ThenInclude(x => x.SubOrderDiscounts)
                    .Include(x => x.SubOrders).ThenInclude(x => x.OrderLines)
                    .ThenInclude(aol => (aol as AllocationOrderLine).Allocation).AsSingleQuery();
            }
        }

        protected IQueryable<Order> GetTenantFilteredEntitiesQuery(bool includeIsEqualized)
        {
            return includeIsEqualized ? TenantFilteredEntitiesQueryIncludeIsEqualized : TenantFilteredEntitiesQuery;
        }

        public void Add(Order order)
        {
            this.AppDbContext.Add(order);
        }

        public async Task<Order> FindSingle(string id, bool includeIsEqualized)
        {
            bool isFriendly = int.TryParse(id, out var friendlyId);
            Order order = null;
            if (isFriendly)
            {
                order = await FindSingleFromFriendlyId(long.Parse(id), includeIsEqualized);
            }
            else
            {
                order = await FindSingle(Guid.Parse(id), includeIsEqualized);
            }

            return order;
        }

        public async Task<Order> FindSingleOrDefault(string id, bool includeIsEqualized)
        {
            bool isFriendly = int.TryParse(id, out var friendlyId);
            Order order = null;
            if (isFriendly)
            {
                order = await FindSingleOrDefaultFromFriendlyId(long.Parse(id), includeIsEqualized);
            }
            else
            {
                order = await FindSingleOrDefault(Guid.Parse(id), includeIsEqualized);
            }

            return order;
        }

        public Task<Order> FindSingle(Guid id, bool includeIsEqualized)
        {
            var order = this.GetTenantFilteredEntitiesQuery(includeIsEqualized).SingleAsync(x => !x.IsDeleted && x.Id == id);
            return order;
        }

        public Task<Order> FindSingleOrDefault(Guid id, bool includeIsEqualized)
        {
            var order = this.GetTenantFilteredEntitiesQuery(includeIsEqualized).SingleOrDefaultAsync(x => !x.IsDeleted && x.Id == id);
            return order;
        }

        public Task<bool> Exists(Guid id)
        {
            return this.AppDbContext.Orders.AnyAsync(x => x.Id == id);
        }

        public Task<Order> FindSingleFromSubOrderId(Guid id, bool includeIsEqualized)
        {
            var order = this.GetTenantFilteredEntitiesQuery(includeIsEqualized).SingleAsync(x => !x.IsDeleted && x.SubOrders.Any(so => so.Id == id));
            return order;
        }

        public async Task<Order> FindSingleFromExternalOrderId(string externalOrderId)
        {
            var order = await this.GetTenantFilteredEntitiesQuery(false).SingleOrDefaultAsync(x => !x.IsDeleted && x.ExternalId == externalOrderId);

            if (order == null)
                order = await this.GetTenantFilteredEntitiesQuery(false).SingleAsync(x => !x.IsDeleted && x.CreateNote.Contains(externalOrderId));

            return order;
        }

        public Task<Order> FindSingleOrDefaultOrderIdFromAllocationId(Guid allocationId)
        {
            var allocationOrderLine = AppDbContext.OrderLines.OfType<AllocationOrderLine>()
                .SingleOrDefault(ol => ol.AllocationId == allocationId);
            if (allocationOrderLine == null) return null;

            var order = this.TenantFilteredEntitiesQuery.SingleOrDefaultAsync(x => !x.IsDeleted && x.SubOrders.Any(so => so.Id == allocationOrderLine.SubOrderId));
            return order;
        }

        public Task<Order> FindSingleOrDefaultOrderIdFromOrderLineId(Guid orderLineId)
        {
            var allocationOrderLine = AppDbContext.OrderLines
                .SingleOrDefault(ol => ol.Id == orderLineId);
            if (allocationOrderLine == null) return null;

            var order = this.TenantFilteredEntitiesQuery.SingleOrDefaultAsync(x => !x.IsDeleted && x.SubOrders.Any(so => so.Id == allocationOrderLine.SubOrderId));
            return order;
        }

        public Task<Order> FindSingleOrDefaultFromSubOrderId(Guid subOrderId, bool includeIsEqualized)
        {
            var order = this.GetTenantFilteredEntitiesQuery(includeIsEqualized).SingleOrDefaultAsync(x => !x.IsDeleted && x.SubOrders.Any(so => so.Id == subOrderId));
            return order;
        }

        public Task<Order> FindSingleFromFriendlyId(long id, bool includeIsEqualized)
        {
            var order = this.GetTenantFilteredEntitiesQuery(includeIsEqualized).SingleAsync(x => !x.IsDeleted && x.FriendlyId == id);
            return order;
        }

        public Task<Order> FindSingleOrDefaultFromFriendlyId(long id, bool includeIsEqualized)
        {
            var order = this.GetTenantFilteredEntitiesQuery(includeIsEqualized).SingleOrDefaultAsync(x => !x.IsDeleted && x.FriendlyId == id);
            return order;
        }

        public async Task<IList<Order>> FindAll(DateInterval dateInterval)
        {
            var orders = await TenantFilteredEntitiesQuery.Where(x => !x.IsDeleted && x.SubOrders.Any(su => su.Start >= dateInterval.Start && su.End <= dateInterval.End)).ToListAsync();
            return orders;
        }

        public Task DeleteImportedOrder(Guid id, DateTime importTimeStamp)
        {
            throw new NotImplementedException();
        }

        public Task<decimal> CalculateOrderTotal(int friendlyOrderId, Guid tenantId)
        {
            // var result = this.AppDbContext.OrderLines.Where(x => x.SubOrder.Order.TenantId == TenantId && x.SubOrder.Order.Id == id).Select(x => x.Number * x.PricePcs).SumAsync();
            var whereClause = $"[Order].FriendlyId = {friendlyOrderId}";
            return CalculateTotal(whereClause, tenantId);
        }

        public Task<decimal> CalculateOrderTotal(Guid id)
        {
            // var result = this.AppDbContext.OrderLines.Where(x => x.SubOrder.Order.TenantId == TenantId && x.SubOrder.Order.Id == id).Select(x => x.Number * x.PricePcs).SumAsync();
            var whereClause = $"[Order].Id = '{id}'";
            return CalculateTotal(whereClause, TenantId);
        }

        private async Task<decimal> CalculateTotal(string whereClause, Guid tenantId)
        {
            await using var dbConnection = AppDbContext.Database.GetDbConnection();
            string commandText = $@"SELECT SUM(OrderLine.Number * OrderLine.PricePcs) FROM OrderLine
                            INNER JOIN SubOrder ON SubOrder.Id = OrderLine.SubOrderId AND OrderLine.IsEqualized=0 
                            INNER JOIN [Order] ON [Order].Id=SubOrder.OrderId AND {whereClause} AND [Order].TenantId='{tenantId}' ";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = commandText;
            cmd.CommandType = CommandType.Text;
            cmd.Connection = dbConnection;

            await dbConnection.OpenAsync();

            object result = await cmd.ExecuteScalarAsync();
            await dbConnection.CloseAsync();

            if (result != null && DBNull.Value != result)
            {
                return (decimal)result;
            }

            return new decimal(0);
        }

        public Task<decimal> CalculateAccountTotal(Guid accountId)
        {
            //var result = this.AppDbContext.OrderLines.Where(x => x.SubOrder.Order.TenantId == TenantId && x.SubOrder.Order.AccountId == accountId).Select(x => x.Number * x.PricePcs).SumAsync();
            var whereClause = $"[Order].AccountId = '{accountId}'";
            return CalculateTotal(whereClause, TenantId);
        }

        public void UpdateOrderNote(Guid orderId, string note, bool? noteState)
        {
            var order = base.TenantFilteredEntitiesQuery.Single(x => x.Id == orderId);
            if (noteState == null && note == order.Note) return;

            order.Note = string.IsNullOrWhiteSpace(note) ? null : note;
            if (noteState != null)
            {
                order.NoteState = noteState.Value;
            }

            if (string.IsNullOrWhiteSpace(order.Note))
            {
                order.NoteState = false;
            }

            this.AppDbContext.Update(order);
        }

        public void AppendToOrderNote(Guid orderId, string note)
        {
            var order = base.TenantFilteredEntitiesQuery.Single(x => x.Id == orderId);
            if (string.IsNullOrWhiteSpace(order.Note) && string.IsNullOrWhiteSpace(note)) { return; }
            order.Note = string.IsNullOrWhiteSpace(order.Note) ? "" : order.Note;
            order.Note += string.IsNullOrWhiteSpace(note) ? "" : note;
        }

        public void UpdateSubOrderNote(Guid subOrderId, string note, bool? noteState)
        {
            var subOrder = this.AppDbContext.SubOrders.Single(x => x.Id == subOrderId);
            if (noteState == null && note == subOrder.Note) return;

            subOrder.Note = string.IsNullOrWhiteSpace(note) ? null : note;
            if (noteState != null)
            {
                subOrder.NoteState = noteState.Value;
            }

            if (string.IsNullOrWhiteSpace(subOrder.Note))
            {
                subOrder.NoteState = false;
            }
            this.AppDbContext.Update(subOrder);
        }

        public async Task<MeterReading> GetLastMeterReading(Guid subOrderId, Guid rentalUnitId)
        {
            var powerMeteredLine = this.AppDbContext.OrderLines.OfType<ArticleOrderLine>().Where(x => x.SubOrderId == subOrderId && x.MeterReference == rentalUnitId && !x.IsEqualized).OrderByDescending(x => x.Index).FirstOrDefault();
            if (powerMeteredLine == null) return null;
            return new MeterReading(powerMeteredLine.MeterEnd ?? powerMeteredLine.MeterStart.Value, powerMeteredLine.CreateDate);
        }

        public async Task<Guid?> FindTenantIdFromOrderId(Guid orderId)
        {
            var order = await AppDbContext.Orders.SingleOrDefaultAsync(x => x.Id == orderId);
            return order?.TenantId;
        }

        public async Task<Guid?> FindTenantIdFromExternalOrderId(string externalOrderId)
        {
            var order = await AppDbContext.Orders.SingleOrDefaultAsync(x => x.ExternalId == externalOrderId);

            if (order == null) 
                order = await AppDbContext.Orders.SingleOrDefaultAsync(x => x.CreateNote.Contains(externalOrderId));
            return order?.TenantId;
        }

        public OrderLine DeleteOrderLine(Guid id)
        {
            var orderLine = AppDbContext.OrderLines.Single(x => x.Id == id);
            this.AppDbContext.OrderLines.Remove(orderLine);
            return orderLine;
        }

        public async Task<Guid?> FindTenantIdFromSubOrderId(Guid subOrderId)
        {
            var subOrder = await AppDbContext.SubOrders.SingleOrDefaultAsync(x => x.Id == subOrderId);
            if (subOrder == null) return null;
            var order = await AppDbContext.Orders.SingleOrDefaultAsync(x => x.Id == subOrder.OrderId && !x.IsDeleted);
            return order.TenantId;
        }
        
    }
}
