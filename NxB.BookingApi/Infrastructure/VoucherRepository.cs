using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;

namespace NxB.BookingApi.Infrastructure
{
    public class VoucherRepository : TenantFilteredRepository<Voucher, AppDbContext>, IVoucherRepository
    {
        public VoucherRepository(IClaimsProvider claimsProvider, AppDbContext appDbContext) : base(claimsProvider, appDbContext)
        {
        }

        public void Add(Voucher invoice)
        {
            AppDbContext.Add(invoice);
        }

        public void Update(Voucher invoice)
        {
            AppDbContext.Update(invoice);
        }

        public void DeleteLegacyInvoice(Voucher invoice)
        {
            if (invoice.FriendlyId > 0) throw new VoucherException("Kan ikke slette faktura. Faktura skal have negativt(gammelt) fakturanummer for at den kan slettes");
            AppDbContext.Remove(invoice);
        }

        public Payment FindSinglePayment(Guid id)
        {
            return this.TenantFilteredEntitiesQuery.Single(x => x.Id == id) as Payment;
        }

        public TInvoiceType FindSingleInvoiceBase<TInvoiceType>(Guid id) where TInvoiceType : InvoiceBase
        {
            var invoice = this.GetInvoicesQueryable<TInvoiceType>(false).Single(x => x.Id == id);
            return invoice;
        }

        public TVoucherType FindSingleVoucher<TVoucherType>(Guid id) where TVoucherType : Voucher
        {
            var voucher = this.GetVouchersQueryable<TVoucherType>().SingleOrDefault(x => x.Id == id);
            return voucher;
        }

        public TVoucherType FindSingleVoucherFromFriendlyId<TVoucherType>(long friendlyId, VoucherType voucherType) where TVoucherType : Voucher
        {
            var voucher = this.GetVouchersQueryable<TVoucherType>().Single(x => x.FriendlyId == friendlyId && x.VoucherType == voucherType);
            return voucher;
        }

        public TInvoiceType FindSingleInvoiceFromFriendlyId<TInvoiceType>(long friendlyId) where TInvoiceType : InvoiceBase
        {
            var invoice = this.GetInvoicesQueryable<TInvoiceType>(false).Single(x => x.FriendlyId == friendlyId);
            return invoice;
        }

        public TVoucherType FindSingleOrDefaultVoucherFromFriendlyId<TVoucherType>(long friendlyId, VoucherType voucherType) where TVoucherType : Voucher
        {
            var voucher = this.GetVouchersQueryable<TVoucherType>().SingleOrDefault(x => x.FriendlyId == friendlyId && x.VoucherType == voucherType);
            return voucher;
        }

        public Task<List<TVoucher>> FindAll<TVoucher>(DateInterval dateInterval, bool? isClosed = null) where TVoucher : Voucher
        {
            return this.GetVouchersQueryable<TVoucher>().Where(x => (isClosed == null || x.IsClosed == isClosed)
                && (x.VoucherDate >= dateInterval.Start && x.VoucherDate <= dateInterval.End)).ToListAsync();
        }

        public async Task<List<TDueVoucher>> FindAllDueVouchers<TDueVoucher>(DateInterval dateInterval, bool? isClosed = null, bool? isDue = null) where TDueVoucher : DueVoucher
        {
            var dueVouchers = await this.GetVouchersQueryable<TDueVoucher>().Where(x => (isClosed == null || x.IsClosed == isClosed)
                                                                       && (x.VoucherDate >= dateInterval.Start && x.VoucherDate <= dateInterval.End)).ToListAsync();

            if (isDue != null)  //Forced ToList because isDue is not a DB property
                dueVouchers = dueVouchers.Where(x => isDue.Value == x.IsDue).ToList();

            return dueVouchers;
        }

        public async Task<TVoucher> FindSingleFromDocumentId<TVoucher>(Guid documentId) where TVoucher : Voucher
        {
            var voucher = await this.GetVouchersQueryable<TVoucher>().SingleOrDefaultAsync(x => x.DocumentId.HasValue && x.DocumentId == documentId);
            if (voucher is InvoiceBase invoiceBase)
            {
                voucher = this.GetInvoicesQueryable<InvoiceBase>(false).Cast<TVoucher>().Single(x => x.Id == voucher.Id);
            }

            return voucher;
        }

        public async Task<List<TInvoiceType>> FindInvoiceBases<TInvoiceType>(DateInterval dateInterval, bool ignoreSubItems, bool? isClosed, bool? isDue) where TInvoiceType : InvoiceBase
        {
            var invoices = await this.GetInvoicesQueryable<TInvoiceType>(ignoreSubItems)
                .Where(x => (x.VoucherDate >= dateInterval.Start && x.VoucherDate <= dateInterval.End)
                    && (isClosed == null || isClosed.Value == x.IsClosed)
                    ).ToListAsync();

            if (isDue != null)  //Forced ToList because isDue is not a DB property
                invoices = invoices.Where(x => isDue.Value == x.IsDue).ToList();

            return invoices;
        }

        public async Task<List<TInvoiceType>> FindInvoiceBasesFromIds<TInvoiceType>(List<Guid> invoiceIds, bool? isClosed) where TInvoiceType : InvoiceBase
        {
            var invoices = await this.GetInvoicesQueryable<TInvoiceType>(false).Where(x => invoiceIds.Any(i => i == x.Id) && (isClosed == null || x.IsClosed == isClosed)).ToListAsync();
            return invoices.ToList();
        }

        public async Task<List<TVoucherType>> FindVouchersFromIds<TVoucherType>(List<Guid> invoiceIds, bool? isClosed) where TVoucherType : Voucher
        {
            var vouchers = await this.GetVouchersQueryable<TVoucherType>().Where(x => invoiceIds.Any(i => i == x.Id) && (isClosed == null || x.IsClosed == isClosed)).ToListAsync();
            return vouchers.ToList();
        }

        public Task<List<TInvoiceType>> FindInvoiceBasesFromOrderId<TInvoiceType>(Guid orderId, bool ignoreSubItems) where TInvoiceType : InvoiceBase
        {
            var invoices = GetInvoicesQueryable<TInvoiceType>(ignoreSubItems);
            invoices = invoices.Where(x => x.OrderId == orderId);
            return invoices.ToListAsync();
        }

        public Task<List<TInvoiceType>> FindInvoiceBasesFromFriendlyOrderId<TInvoiceType>(long friendlyOrderId, bool ignoreSubItems) where TInvoiceType : InvoiceBase
        {
            var invoices = GetInvoicesQueryable<TInvoiceType>(ignoreSubItems);
            invoices = invoices.Where(x => x.FriendlyOrderId == friendlyOrderId);
            return invoices.ToListAsync();
        }

        public Task<List<Payment>> FindPaymentsFromOrderId(Guid orderId)
        {
            var invoices = GetVouchersQueryable<Payment>();
            invoices = invoices.Where(x => x.OrderId == orderId);
            return invoices.ToListAsync();
        }

        public Task<List<Payment>> FindPaymentsFromFriendlyOrderId(long friendlyOrderId)
        {
            var invoices = GetVouchersQueryable<Payment>();
            invoices = invoices.Where(x => x.FriendlyOrderId == friendlyOrderId);
            return invoices.ToListAsync();
        }

        public Task<List<Voucher>> FindPaymentsOrCreditNotesFromOrder(Guid orderId, bool? isClosed = null)
        {
            var vouchers = GetVouchersQueryable<Voucher>();
            vouchers = vouchers.Where(x => x.OrderId == orderId && (isClosed == null || x.IsClosed == isClosed) && (x.VoucherType == VoucherType.Payment || x.VoucherType == VoucherType.CreditNote));
            return vouchers.ToListAsync();
        }

        public async Task<List<Payment>> FindSpecificPaymentsFromInvoiceId(Guid invoiceId, bool? isClosed)
        {
            var payments = await this.GetVouchersQueryable<Payment>().Where(x => x.SpecificInvoiceId.HasValue && x.SpecificInvoiceId == invoiceId && (isClosed == null || x.IsClosed == isClosed)).ToListAsync();
            return payments.ToList();
        }

        public async Task<List<Payment>> FindPaymentsFromClosedTransactionId(Guid closeTransactionId)
        {
            var payments = await this.GetVouchersQueryable<Payment>().Where(x => x.IsClosed && x.VoucherCloseTransactionId.Value == closeTransactionId).ToListAsync();
            return payments.ToList();
        }

        public async Task<List<Payment>> FindPaymentsFromIds(List<Guid> paymentIds, bool? isClosed)
        {
            var payments = await this.GetVouchersQueryable<Payment>().Where(x => paymentIds.Any(p => p == x.Id) && (isClosed == null || x.IsClosed == isClosed)).ToListAsync();
            return payments.ToList();
        }

        public Task<List<TVoucher>> FindFromAccountId<TVoucher>(Guid accountId, bool? isClosed) where TVoucher : Voucher
        {
            return this.GetVouchersQueryable<TVoucher>().Where(x => x.AccountId == accountId && (isClosed == null || isClosed.Value == x.IsClosed)).ToListAsync();
        }

        public Task<List<TInvoiceType>> FindFromOrderId<TInvoiceType>(Guid orderId, bool? isClosed) where TInvoiceType : Voucher
        {
            var invoices = GetVouchersQueryable<TInvoiceType>();
            invoices = invoices.Where(x => x.OrderId == orderId && (isClosed == null || isClosed.Value == x.IsClosed));
            return invoices.ToListAsync();
        }

        public Task<List<TInvoiceType>> FindFromFriendlyOrderId<TInvoiceType>(long friendlyOrderId, bool? isClosed) where TInvoiceType : Voucher
        {
            var invoices = GetVouchersQueryable<TInvoiceType>();
            invoices = invoices.Where(x => x.FriendlyOrderId == friendlyOrderId && (isClosed == null || isClosed.Value == x.IsClosed));
            return invoices.ToListAsync();
        }

        private IQueryable<TInvoiceType> GetInvoicesQueryable<TInvoiceType>(bool ignoreSubItems) where TInvoiceType : InvoiceBase
        {
            var invoices = ignoreSubItems
                ? this.AppDbContext.Vouchers.OfType<TInvoiceType>().Where(x => x.TenantId == this.TenantId)
                : this.AppDbContext.Vouchers.OfType<TInvoiceType>().Where(x => x.TenantId == this.TenantId).Include(x => x.InvoiceSubOrders).ThenInclude(x => x.InvoiceLines).AsSingleQuery();
            return invoices.OrderByDescending(x => x.VoucherDate).ThenByDescending(x => x.CreateDate);
        }

        private IQueryable<TVoucher> GetVouchersQueryable<TVoucher>() where TVoucher : Voucher
        {
            return this.AppDbContext.Vouchers.OfType<TVoucher>().Where(x => x.TenantId == this.TenantId).OrderByDescending(x => x.VoucherDate);
        }

        public async Task<List<Guid>> RemoveInvoicedOrderLineIds(List<Guid> orderLineIds)
        {
            var invoicedOrderLineIds = await FilterInvoicedOrderLineIds(orderLineIds);
            var orderLineIdsNotInvoiced = orderLineIds.Except(invoicedOrderLineIds).ToList();
            return orderLineIdsNotInvoiced;
        }

        public async Task<Dictionary<Guid, string>> GetInvoicedOrderLineIds(Guid orderId)
        {
            var orderLines = await this.AppDbContext.InvoiceLines.OfType<InvoiceOrderLine>().Where(x => (x.InvoiceSubOrder.Voucher.VoucherType == VoucherType.Invoice || x.InvoiceSubOrder.Voucher.VoucherType == VoucherType.Deposit) && ((Invoice)x.InvoiceSubOrder.Voucher).OrderId == orderId).OrderByDescending(x => x.InvoiceSubOrder.Voucher.VoucherDate).Select(x => new { orderLineId = x.OrderLineId, invoiceId = x.InvoiceSubOrder.Voucher.Id }).ToListAsync();
            var invoicedOrderLineIds = await FilterInvoicedOrderLineIds(orderLines.Select(x => x.orderLineId).ToList());
            orderLines = orderLines.Where(x => invoicedOrderLineIds.Contains(x.orderLineId)).ToList();

            var distinctColors = new DistinctColors(1);
            var invoiceColors = orderLines.Select(x => x.invoiceId).Distinct().ToDictionary(x => x, x => distinctColors.NextColor());

            var orderLinesIdsAndColor = new Dictionary<Guid, string>();

            //Just add first instance, all other instances should be because of crediting
            orderLines.ForEach(x =>
            {
                if (!orderLinesIdsAndColor.ContainsKey(x.orderLineId))
                {
                    orderLinesIdsAndColor.Add(x.orderLineId, invoiceColors[x.invoiceId]);
                }
            });

            return orderLinesIdsAndColor;
        }

        public async Task<List<InvoicedOrderLineInfo>> GetInvoicedOrderLinesInfo(Guid orderId)
        {
            var orderLines = await this.AppDbContext.InvoiceLines.OfType<InvoiceOrderLine>().Where(x => (x.InvoiceSubOrder.Voucher.VoucherType == VoucherType.Invoice || x.InvoiceSubOrder.Voucher.VoucherType == VoucherType.Deposit) && ((Invoice)x.InvoiceSubOrder.Voucher).OrderId == orderId).OrderByDescending(x => x.InvoiceSubOrder.Voucher.VoucherDate).Select(x => new { orderLineId = x.OrderLineId, invoiceId = x.InvoiceSubOrder.Voucher.Id }).ToListAsync();
            var invoicedOrderLineInfos = await FilterInvoicedOrderLineIds2(orderLines.Select(x => x.orderLineId).ToList());
            orderLines = orderLines.Where(x => invoicedOrderLineInfos.Any(iol => iol.OrderLineId == x.orderLineId)).ToList();

            var distinctColors = new DistinctColors(1);
            var invoiceColors = orderLines.Select(x => x.invoiceId).Distinct().ToDictionary(x => x, x => distinctColors.NextColor());

            var invoicedOrderLineInfosFiltered = new List<InvoicedOrderLineInfo>();

            //Just add first instance, all other instances should be because of crediting
            orderLines.ForEach(x =>
            {
                if (invoicedOrderLineInfosFiltered.None(oil => oil.OrderLineId == x.orderLineId))
                {
                    var info = invoicedOrderLineInfos.First(info => info.OrderLineId == x.orderLineId);
                    info.Color = invoiceColors[x.invoiceId];
                    invoicedOrderLineInfosFiltered.Add(info);
                }
            });

            return invoicedOrderLineInfosFiltered;
        }

        private async Task<List<Guid>> FilterInvoicedOrderLineIds(List<Guid> orderLineIds)
        {
            var invoiceOrderLines = await this.AppDbContext.InvoiceLines.Include(x => x.InvoiceSubOrder)
                .ThenInclude(x => x.Voucher).OfType<InvoiceOrderLine>()
                .Where(x => x.TenantId == this.TenantId && orderLineIds.Contains(x.OrderLineId)).ToListAsync();

            var groupedInvoiceOrderLines = invoiceOrderLines.GroupBy(x => x.OrderLineId,
                    x => x,
                    (key, g) => new
                    {
                        orderLineId = key,
                        orderLines = g.OrderByDescending(x => x.InvoiceSubOrder.Voucher.CreateDate).ToList()
                    }).ToList();
            var invoicedOrderLineIds = groupedInvoiceOrderLines
                .Where(x => x.orderLines.First().InvoiceSubOrder.Voucher is Invoice).Select(x => x.orderLineId)
                .ToList();
            return invoicedOrderLineIds;
        }

        private async Task<List<InvoicedOrderLineInfo>> FilterInvoicedOrderLineIds2(List<Guid> orderLineIds)
        {
            var invoiceOrderLines = await this.AppDbContext.InvoiceLines
                .Include(x => x.InvoiceSubOrder)
                .ThenInclude(x => x.Voucher).OfType<InvoiceOrderLine>()
                .Where(x => x.TenantId == this.TenantId && orderLineIds.Contains(x.OrderLineId)).ToListAsync();

            var groupedInvoiceOrderLines = invoiceOrderLines.GroupBy(x => x.OrderLineId,
                x => x,
                (key, g) => new
                {
                    orderLineId = key,
                    orderLines = g.OrderByDescending(x => x.InvoiceSubOrder.Voucher.CreateDate).ToList()
                }).ToList();
            var invoicedOrderLineIds = groupedInvoiceOrderLines
                .Where(x => x.orderLines.First().InvoiceSubOrder.Voucher is Invoice).Select(x =>
                    new InvoicedOrderLineInfo(x.orderLineId, x.orderLines[0].InvoiceSubOrder.Voucher.Id, x.orderLines[0].InvoiceSubOrder.Voucher.FriendlyId, x.orderLines[0].InvoiceSubOrder.Voucher.DocumentId.Value, null))
                .ToList();
            return invoicedOrderLineIds;
        }

        public CreditNote FindSingleOrDefaultCreditNoteFromInvoiceId(Guid invoiceId)
        {
            return this.GetInvoicesQueryable<CreditNote>(false).SingleOrDefault(x => x.InvoiceId == invoiceId);
        }

        public Task<List<Voucher>> FindVouchersFromClosedTransactionId(Guid closeTransactionId)
        {
            return this.GetVouchersQueryable<Voucher>().Where(x => x.VoucherCloseTransactionId == closeTransactionId).ToListAsync();
        }

        public async Task<decimal> CalculateTotalFromOrderId(Guid orderId)
        {
            var subTotal = await this.GetInvoicesQueryable<InvoiceBase>(true).Where(x => x.OrderId == orderId).Select(x => x.Total).SumAsync();
            return subTotal;
        }

        public async Task<decimal> CalculateTotalFromAccountId(Guid accountId)
        {
            var subTotal = await this.GetInvoicesQueryable<InvoiceBase>(true).Where(x => x.AccountId == accountId).Select(x => x.Total).SumAsync();
            return subTotal;
        }
    }
}
