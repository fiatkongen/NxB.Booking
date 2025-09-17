using NxB.Dto.Clients;
using NxB.Dto.OrderingApi;

namespace NxB.BookingApi.Infrastructure
{
    public class OrderClient : NxBAdministratorClient, IOrderClient
    {
        private readonly Dictionary<Guid, OrderDto> _orderDtosCache = new();
        public static string SERVICEURL = "/NxB.Services.App/NxB.OrderingApi";

        public async Task<OrderDto> FindOrderFromSubOrderId(Guid subOrderId)
        {
            var url = $"{SERVICEURL}/order/suborderid?subOrderId={subOrderId}";
            var orderDto = await this.GetAsync<OrderDto>(url);
            return orderDto;
        }

        public async Task<OrderDto> FindOrder(long id)
        {
            var orderDto = _orderDtosCache.FirstOrDefault(x => x.Value.FriendlyId == id).Value;

            if (orderDto != null)
            {
                return orderDto;
            }
            var url = $"{SERVICEURL}/order?orderId={id}";
            orderDto = await this.GetAsync<OrderDto>(url);
            this._orderDtosCache.Add(orderDto.Id, orderDto);
            return orderDto;
        }

        public async Task<bool> ExistsOrder(Guid id)
        {
            var url = $"{SERVICEURL}/order/exists?orderId={id}";
            var result = await this.GetAsync<bool>(url);
            return result;
        }

        public async Task<OrderDto> FindOrder(string orderId)
        {
            bool isFriendly = int.TryParse(orderId, out var friendlyId);
            OrderDto orderDto;

            if (isFriendly)
            {
                orderDto = await FindOrder(friendlyId);
            }
            else
            {
                orderDto = await FindOrder(Guid.Parse(orderId));
            }
            return orderDto;
        }

        public async Task<OrderDto> CreateOrder(CreateOrderDto createOrderDto)
        {
            var url = $"{SERVICEURL}/order";
            var orderDto = await this.PostAsync<OrderDto>(url, createOrderDto);
            return orderDto;
        }

        public async Task<OrderDto> CreateOnlineOrder(CartDto cartDto, string language)
        {
            var url = $"{SERVICEURL}/orderonline?language={language}";
            var orderDto = await this.PostAsync<OrderDto>(url, cartDto);
            return orderDto;
        }

        public async Task AppendToOrderNote(ModifyOrderNoteDto modifyOrderNoteDto)
        {
            var url = $"{SERVICEURL}/order/note/append";
            await this.PostAsync(url, modifyOrderNoteDto);
        }

        public async Task<decimal> CalculateOrderTotal(Guid id)
        {
            var url = $"{SERVICEURL}/order/calculate/total?id={id}";
            var result = await this.GetAsync<Dictionary<string, decimal>>(url);
            return result["result"];
        }

        public async Task<decimal> CalculateAccountTotal(Guid accountId)
        {
            var url = $"{SERVICEURL}/order/account/calculate/total?accountId={accountId}";
            var result = await this.GetAsync<Dictionary<string, decimal>>(url);
            return result["result"];
        }

        public async Task<OrderDto> FindSingleOrDefaultOrderIdFromOrderLineId(Guid orderLineId)
        {
            var url = $"{SERVICEURL}/order?orderlineid={orderLineId}";
            var orderDto = await this.GetAsync<OrderDto>(url);
            return orderDto;
        }

        public async Task<Guid?> FindTenantIdFromExternalOrderId(string externalOrderId)
        {
            var url = $"{SERVICEURL}/order/tenantidfromexternalorderid?externalOrderId={externalOrderId}";
            var tenantId = await this.GetAsync<Guid?>(url);
            return tenantId;
        }

        public async Task<OrderDto> FindOrderFromExternalOrderId(string externalOrderId)
        {
            var url = $"{SERVICEURL}/order/orderfromexternalorderid?externalOrderId={externalOrderId}";
            var orderDto = await this.GetAsync<OrderDto>(url);
            return orderDto;
        }

        public async Task<OrderDto> FindOrder(Guid id)
        {
            if (_orderDtosCache.ContainsKey(id))
            {
                return _orderDtosCache[id];
            }
            var url = $"{SERVICEURL}/order?orderId={id}";
            var orderDto = await this.GetAsync<OrderDto>(url);
            this._orderDtosCache.Add(orderDto.Id, orderDto);
            return orderDto;
        }

        public async Task UpdateOrderOnlineTransactionDetails(ModifyOrderOnlineTransactionDetails modifyOrderOnlineTransactionDetails)
        {
            var url = $"{SERVICEURL}/order/onlinetransactiondetails";
            await this.PutAsync(url, modifyOrderOnlineTransactionDetails);
        }

        public OrderClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }
    }
}