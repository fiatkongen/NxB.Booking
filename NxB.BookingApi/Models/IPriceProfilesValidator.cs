namespace NxB.BookingApi.Models
{
    public interface IPriceProfilesValidator
    {
        Task<List<Guid>> ValidatePriceProfileIds(List<Guid> priceProfileIds, Guid tenantId);
    }
}