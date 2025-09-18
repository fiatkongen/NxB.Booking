using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using NxB.Domain.Common.Model;
using NxB.Dto.AccountingApi;
using NxB.Dto.AutomationApi;
using NxB.Dto.DocumentApi;
using NxB.Dto.GuestBoxApi;
using NxB.Dto.TenantApi;

namespace NxB.Settings.Shared.Infrastructure
{
    public interface IAppSettingsRepository
    {
        decimal GetEurConversionRate(Guid? tenantId = null);
        string GetApplicationCurrency();
        string GetApplicationLanguage();
        string GetApplicationCountry();
        bool SendSmsAfterOnlineBooking();
        string GetDigitalGuestToken();
        string GetDigitalGuestProperty();
        bool IsIndividualTaxEnabled();
        decimal? TaxRate();
        bool IsBetaPriceCalculationEnabled(Guid? tenantId = null);
        bool GetIsTallyAutoActivationEnabled();
        string GetOnlineBookingMasterDataJson(Guid? tenantId = null);
        OnlineBookingMasterDataDto GetOnlineBookingMasterData(Guid? tenantId = null);
        OnlineBookingSearchSettings GetOnlineBookingSearchSettings(Guid? tenantId = null);
        Dictionary<string, int> GetOnlineContactInfoSettings(Guid? tenantId = null);
        Dictionary<string, int> GetKioskContactInfoSettings(Guid? tenantId = null);
        Dictionary<string, object> GetKioskOnlineSettings(Guid? tenantId = null);
        PaymentProvidersDto GetOnlineBookingPaymentProviders(Guid? tenantId = null);
        AutomationSettingsDto GetAutomationSettings(Guid? tenantId = null);
        VerifoneSettingsDto GetVerifoneSettings(Guid? tenantId = null);
        Dictionary<string, bool> GetOnlineBookingLanguages(Guid? tenantId = null);
        Dictionary<string, bool> GetGuestInfoLanguages(Guid? tenantId = null);
        Guid? GetSmsSettingSendOnArrival(Guid? tenantId = null);
        Guid? GetSmsSettingSendOnDeparture(Guid? tenantId = null);
        JObject GetOnlineBookingSettings(Guid? tenantId = null);
        int GetTallyLastAccessLogCrosscheckIndex();
        void SetTallyLastAccessLogCrosscheckIndex(int index);
        string GetSmsSenderName(Guid? tenantId = null);

        QuickPaySettings GetQuickPaySettings(Guid? tenantId = null);
        bool IsCtoutvertActivated(Guid? tenantId = null);
        string GetCtoutvertCompanyName(Guid? tenantId = null);
    }
}
