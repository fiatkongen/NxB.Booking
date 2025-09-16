using Munk.AspNetCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NxB.Domain.Common.Constants;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;
using NxB.Dto.AccountingApi;
using NxB.Dto.AutomationApi;
using NxB.Dto.DocumentApi;
using NxB.Dto.TenantApi;
using ServiceStack.Messaging;
using NxB.Domain.Common.Enums;
using NxB.Dto.GuestBoxApi;

namespace NxB.Settings.Shared.Infrastructure
{
    public class SettingsRepository<TAppDbContext> : TenantFilteredRepository<SettingsItem, TAppDbContext>,
        ISettingsRepository where TAppDbContext : DbContext
    {
        private static string CONTEXT_TENANT = "tenant";
        private static string CONTEXT_USER = "user";
        private readonly Dictionary<(Guid, string), SettingsItem> _cachedSettingsItems = new();

        public SettingsRepository(IClaimsProvider claimsProvider, TAppDbContext appDbContext) : base(claimsProvider,
            appDbContext)
        {
        }

        public JObject GetTenantJsonSettings(string path = null, Guid? tenantId = null)
        {
            return this.GetJsonSettings(tenantId ?? ClaimsProvider.GetTenantId(), CONTEXT_TENANT, path);
        }

        public void SetTenantJsonSettings(JObject settings, string path = null)
        {
            SetJsonSettings(ClaimsProvider.GetTenantId(), CONTEXT_TENANT, settings, path);
        }

        public void DeleteTenantJsonSettings(string path)
        {
            DeleteSettings(ClaimsProvider.GetTenantId(), CONTEXT_TENANT, path);
        }

        public JObject GetUserJsonSettings(string path = null)
        {
            return this.GetJsonSettings(ClaimsProvider.GetTenantId(), CONTEXT_USER, path);
        }

        public void SetUserJsonSettings(JObject settings, string path = null)
        {
            SetJsonSettings(ClaimsProvider.GetTenantId(), CONTEXT_USER, settings, path);
        }

        public void DeleteUserJsonSettings(string path)
        {
            DeleteSettings(ClaimsProvider.GetTenantId(), CONTEXT_USER, path);
        }

        public void CacheTenantSettings(Guid tenantId)
        {
            var settingsItem = FindSettingsOrDefault(tenantId, CONTEXT_TENANT);
            if (settingsItem != null)
            {
                this._cachedSettingsItems.Add((tenantId, CONTEXT_TENANT), settingsItem);
            }
        }

        protected JObject GetJsonSettings(Guid id, string context, string path = null)
        {
            var settingsItem = FindSettingsOrDefault(id, context);
            dynamic currentSettings = settingsItem?.Value;
            JObject currentSettingsParent = null;

            if (path != null && currentSettings != null)
            {
                var paths = path.Split('/');
                for (var i = 0; i < paths.Length; i++)
                {
                    if (currentSettings[paths[i]] != null)
                    {
                        currentSettingsParent = currentSettings;
                        currentSettings = currentSettings[paths[i]];
                    }
                    else
                    {
                        currentSettings = null;
                        break;
                    }
                }
            }

            if (currentSettings == null) return new JObject();
            if (!(currentSettings is JObject)) return currentSettingsParent;
            return currentSettings;
        }

        private void SetJsonSettings(Guid id, string context, JObject settings, string path = null)
        {
            var settingsItem = FindSettingsOrDefault(id, context);
            if (settingsItem == null)
            {
                settingsItem = new SettingsItem
                {
                    Id = id,
                    Context = context,
                    TenantId = ClaimsProvider.GetTenantId(),
                    Value = new JObject(),
                };
                SetSettings(settings, path, settingsItem);
                AppDbContext.Add(settingsItem);
            }
            else
            {
                SetSettings(settings, path, settingsItem);
                AppDbContext.Update(settingsItem);
            }

            Serialize(settingsItem);
        }

        private void DeleteSettings(Guid id, string context, string path)
        {
            var settings = FindSettingsOrDefault(id, context);
            if (path != null)
            {
                dynamic currentSettings = settings.Value;
                var paths = path.Split('/');
                for (var i = 0; i < paths.Length; i++)
                {
                    if (i == paths.Length - 1)
                    {
                        ((JObject)currentSettings).Remove(paths[i]);
                    }
                    else
                    {
                        if (currentSettings[paths[i]] == null)
                        {
                            return;
                        }

                        currentSettings = currentSettings[paths[i]];
                    }
                }

                Serialize(settings);
                AppDbContext.Update(settings);
            }
        }

        private void SetSettings(dynamic settings, string path, SettingsItem settingsItem)
        {
            if (path != null)
            {
                dynamic currentSettings = settingsItem.Value;
                var paths = path.Split('/');
                for (var i = 0; i < paths.Length; i++)
                {
                    if (i == paths.Length - 1)
                    {
                        currentSettings[paths[i]] = settings.simpleValue == null ? settings : settings.simpleValue;
                    }
                    else
                    {
                        if (currentSettings[paths[i]] == null)
                        {
                            currentSettings[paths[i]] = new JObject();
                        }

                        currentSettings = currentSettings[paths[i]];
                    }
                }
            }
            else
            {
                settingsItem.Value = settings;
            }
        }

        public async Task CacheAllActiveTenantsSettings()
        {
            var settingsItems = await AppDbContext.Set<SettingsItem>().AsNoTracking().Where(x => x.Context == CONTEXT_TENANT)
                .ToListAsync();
            foreach (var settingsItem in settingsItems)
            {
                Deserialize(settingsItem);
                this._cachedSettingsItems.Add((settingsItem.TenantId, settingsItem.Context), settingsItem);
            }
        }

        private SettingsItem FindSettingsOrDefault(Guid tenantId, string context)
        {
            if (!this._cachedSettingsItems.TryGetValue((tenantId, context), out var settingsItem))
            {
                settingsItem = AppDbContext.Set<SettingsItem>().AsNoTracking().SingleOrDefault(x => x.Id == tenantId && x.Context == context);
            }
            if (settingsItem != null)
            {
                Deserialize(settingsItem);
            }

            return settingsItem;
        }

        private void Serialize(SettingsItem settingsItem)
        {
            var json = JsonConvert.SerializeObject(settingsItem.Value);
            settingsItem.JsonSettingsItem = json;
        }

        private void Deserialize(SettingsItem settingsItem)
        {
            var value = JsonConvert.DeserializeObject<JObject>(settingsItem.JsonSettingsItem);
            settingsItem.Value = value;
        }

        public decimal GetEurConversionRate(Guid? tenantId = null)
        {
            try
            {
                var setting =
                    this.GetTenantJsonSettings("setup/applicationCurrencyEurConversion", tenantId)["applicationCurrencyEurConversion"];

                if (setting == null)
                {
                    return 7.1m;
                }

                return decimal.Parse(setting.ToString(), CultureInfo.InvariantCulture);
            }
            catch
            {
                throw new SettingsException(
                    "Kunne ikke anvende EUR-kursen. Tjek at værdien er korrekt under Opsætning -> System");
            }
        }

        public string GetApplicationCurrency()
        {
            try
            {
                var setting =
                    this.GetTenantJsonSettings("setup/applicationCurrency")[
                        "applicationCurrency"];
                if (setting == null)
                {
                    return "DKK";
                }

                return setting.ToString().ToUpper();
            }
            catch
            {
                throw new SettingsException(
                    "Kunne ikke anvende valuta. Tjek at værdien er korrekt under Opsætning -> System");
            }
        }

        public string GetApplicationLanguage()
        {
            try
            {
                var setting = this.GetTenantJsonSettings("setup/applicationLanguage")["applicationLanguage"];
                if (setting == null)
                {
                    return "da";
                }

                return setting.ToString();
            }
            catch
            {
                throw new SettingsException(
                    "Kunne ikke anvende sprog. Tjek at værdien er korrekt under Opsætning -> System");
            }
        }

        public string GetApplicationCountry()
        {
            try
            {
                var setting = this.GetTenantJsonSettings("setup/applicationCountry")["applicationCountry"];
                if (setting == null)
                {
                    return "dk";
                }

                return setting.ToString();
            }
            catch
            {
                throw new SettingsException(
                    "Kunne ikke anvende land. Tjek at værdien er korrekt under Opsætning -> System");
            }
        }

        public bool SendSmsAfterOnlineBooking()
        {
            try
            {
                var setting = this.GetTenantJsonSettings("setup/sendSmsAfterOnlineBooking")["sendSmsAfterOnlineBooking"];
                if (setting == null)
                {
                    return false;
                }

                return bool.Parse(setting.ToString());
            }
            catch
            {
                throw new SettingsException("Kunne finde setup/sendSmsAfterOnlineBooking.");
            }

        }

        public string GetDigitalGuestToken()
        {
            try
            {
                var setting = this.GetTenantJsonSettings("setup/isDigitalGuestIntegrationEnabled")["isDigitalGuestIntegrationEnabled"];
                if (setting == null || !bool.Parse(setting.ToString()))
                {
                    return null;
                }
                setting = this.GetTenantJsonSettings("setup/digitalGuestToken")["digitalGuestToken"];
                if (setting == null) return null;

                return setting.ToString();
            }
            catch
            {
                throw new SettingsException("Kunne ikke finde setup/digitalGuestToken.");
            }
        }

        public string GetDigitalGuestProperty()
        {
            try
            {
                var setting = this.GetTenantJsonSettings("setup/isDigitalGuestIntegrationEnabled")["isDigitalGuestIntegrationEnabled"];
                if (setting == null || !bool.Parse(setting.ToString()))
                {
                    return null;
                }
                setting = this.GetTenantJsonSettings("setup/digitalGuestProperty")["digitalGuestProperty"];
                if (setting == null) return null;

                return setting.ToString();
            }
            catch
            {
                throw new SettingsException("Kunne finde setup/GetDigitalGuestProperty.");
            }
        }

        public bool IsIndividualTaxEnabled()
        {
            var setting = this.GetTenantJsonSettings("setup/tax")["isIndividualTaxEnabled"];
            if (setting == null || !bool.Parse(setting.ToString()))
            {
                return false;
            }
            return bool.Parse(setting.ToString());
        }

        public decimal? TaxRate()
        {
            var setting = this.GetTenantJsonSettings("setup/tax")["taxRate"];
            if (setting == null || !decimal.TryParse(setting.ToString(), out var result))
            {
                return null;
            }
            return result;
        }

        public string GetCtoutvertCompanyName(Guid? tenantId = null)
        {
            try
            {
                var setting = this.GetTenantJsonSettings("setup/ctoutvertSettings", tenantId)["companyName"];
                return setting?.ToString();
            }
            catch
            {
                throw new SettingsException("Kunne finde setup/ctoutvertSettings/companyName");
            }
        }

        public bool IsCtoutvertActivated(Guid? tenantId = null)
        {
            var setting = this.GetTenantJsonSettings("setup/ctoutvertSettings", tenantId)["isEnabled"];
            if (setting == null || !bool.Parse(setting.ToString()))
            {
                return false;
            }
            return bool.Parse(setting.ToString());
        }

        public bool IsBetaPriceCalculationEnabled(Guid? tenantId = null)
        {
            var setting = this.GetTenantJsonSettings("setup/isBetaPriceCalculationEnabled", tenantId)["isBetaPriceCalculationEnabled"];
            if (setting == null || !bool.Parse(setting.ToString()))
            {
                return false;
            }
            return bool.Parse(setting.ToString());
        }

        public bool GetIsTallyAutoActivationEnabled()
        {
            var setting = this.GetTenantJsonSettings("setup/isTallyAutoActivationEnabled")["isTallyAutoActivationEnabled"];
            if (setting == null || !bool.Parse(setting.ToString()))
            {
                return false;
            }
            return bool.Parse(setting.ToString());
        }

        public string GetOnlineBookingMasterDataJson(Guid? tenantId = null)
        {
            var setting = this.GetTenantJsonSettings("setup/onlineBookingMasterData", tenantId);

            return setting?.ToString();
        }

        public OnlineBookingMasterDataDto GetOnlineBookingMasterData(Guid? tenantId = null)
        {
            var onlineBookingMasterDataJson = this.GetOnlineBookingMasterDataJson(tenantId);
            var masterData = JsonConvert.DeserializeObject<OnlineBookingMasterDataDto>(onlineBookingMasterDataJson);
            return masterData;
        }

        public Dictionary<string, int> GetOnlineContactInfoSettings(Guid? tenantId = null)
        {
            Dictionary<string, int> defaultInfoSettings;
            var setting = this.GetTenantJsonSettings("setup/contactInfoSettingsOnline", tenantId).ToString();
            if (setting != "{}")
            {
                defaultInfoSettings = JsonConvert.DeserializeObject<Dictionary<string, int>>(setting);
            }
            else
            {
                defaultInfoSettings = new Dictionary<string, int>();
            }

            var isLicensePlateAutomationEnabled = this.GetAutomationSettings(tenantId).IsLicensePlateAutomationEnabled;
            if (!isLicensePlateAutomationEnabled && !defaultInfoSettings.ContainsKey("licensePlate"))
            {
                defaultInfoSettings.Add("licensePlate", this.GetOnlineBookingMasterData(tenantId).IsLicensePlateVisible ? (int)FormInputDemand.Show : (int)FormInputDemand.Hidden);

            }

            ApplyDefaultContactInfoSetting(defaultInfoSettings);

            return defaultInfoSettings;
        }

        private void ApplyDefaultContactInfoSetting(Dictionary<string, int> defaultInfoSettings)
        {
            defaultInfoSettings.TryAdd("licensePlate", (int)FormInputDemand.WarnIfEmpty);
            defaultInfoSettings.TryAdd("address", (int)FormInputDemand.Show);
            defaultInfoSettings.TryAdd("zip", (int)FormInputDemand.Show);
            defaultInfoSettings.TryAdd("city", (int)FormInputDemand.Show);
            defaultInfoSettings.TryAdd("arrivalTime", (int)FormInputDemand.Show);
        }

        public Dictionary<string, int> GetKioskContactInfoSettings(Guid? tenantId = null)
        {
            Dictionary<string, int> defaultInfoSettings;
            var setting = this.GetTenantJsonSettings("setup/contactInfoSettingsKiosk", tenantId).ToString();
            if (setting != "{}")
            {
                defaultInfoSettings = JsonConvert.DeserializeObject<Dictionary<string, int>>(setting);
                ApplyDefaultContactInfoSetting(defaultInfoSettings);
                return defaultInfoSettings;
            }

            return GetOnlineContactInfoSettings(tenantId);
        }

        public Dictionary<string, object> GetKioskOnlineSettings(Guid? tenantId = null)
        {
            var defaultSettings = new Dictionary<string, object>();
            defaultSettings.Add("showWelcomeOnlineInTop", true);
            defaultSettings.Add("showWelcomeOnlineInPauseScreenTop", true);
            defaultSettings.Add("hideWelcomeOnline", true);
            defaultSettings.Add("hideAboutOnline", true);
            defaultSettings.Add("searchOnShow", true);
            defaultSettings.Add("searchDays", 1);
            defaultSettings.Add("refreshAfterInactiveMinutes", 3);
            defaultSettings.Add("activatePingTest", false);
            defaultSettings.Add("hideLanguageSelection", false);
            defaultSettings.Add("isStartDateSelectionEnabled", false);
            defaultSettings.Add("isBookingEnabled", true);
            defaultSettings.Add("isCheckInEnabled", true);

            var setting = this.GetTenantJsonSettings("setup/kioskSettingsOnline", tenantId).ToString();
            var dto = setting != "{}" ? JsonConvert.DeserializeObject<Dictionary<string, object>>(setting) : defaultSettings;

            foreach (var defaultSettingKey in defaultSettings.Keys)
            {
                if (dto != null && !dto.ContainsKey(defaultSettingKey))
                {
                    dto.Add(defaultSettingKey, defaultSettings[defaultSettingKey]);
                }
            }
            return dto;
        }

        public T GetDeserializedSetting<T>(string path, Guid? tenantId = null) where T : class, new()
        {
            var setting = this.GetTenantJsonSettings(path, tenantId);
            var dto = setting != null ? JsonConvert.DeserializeObject<T>(setting.ToString()) : new T();
            return dto;
        }

        public OnlineBookingSearchSettings GetOnlineBookingSearchSettings(Guid? tenantId = null)
        {
            var setting = this.GetDeserializedSetting<OnlineBookingSearchSettings>("setup/onlineBookingSearchSettings", tenantId);
            return setting;
        }

        public PaymentProvidersDto GetOnlineBookingPaymentProviders(Guid? tenantId = null)
        {
            var setting = this.GetTenantJsonSettings("setup/onlineBookingPaymentProviders", tenantId);
            var dto = setting != null ? JsonConvert.DeserializeObject<PaymentProvidersDto>(setting.ToString()) : new PaymentProvidersDto();
            return dto;
        }

        public AutomationSettingsDto GetAutomationSettings(Guid? tenantId = null)
        {
            tenantId ??= ClaimsProvider.GetTenantId();

            var setting = this.GetTenantJsonSettings("setup/automationSettings", tenantId);

            var dto = setting != null ? JsonConvert.DeserializeObject<AutomationSettingsDto>(setting.ToString()) : new AutomationSettingsDto();

            if (tenantId == AppConstants.DEMONSTRATION_TENANT_ID)
            {
                dto.Login = "test";
                dto.Password = "test";
            }

            return dto;
        }

        public VerifoneSettingsDto GetVerifoneSettings(Guid? tenantId = null)
        {
            tenantId ??= ClaimsProvider.GetTenantId();

            var setting = this.GetTenantJsonSettings("setup/verifoneSettings", tenantId);

            var dto = setting != null ? JsonConvert.DeserializeObject<VerifoneSettingsDto>(setting.ToString()) : new VerifoneSettingsDto();

            return dto;
        }

        public Dictionary<string, bool> GetOnlineBookingLanguages(Guid? tenantId = null)
        {
            var defaultLanguages = new Dictionary<string, bool>();
            defaultLanguages.Add("isDanishEnabled", true);
            defaultLanguages.Add("isEnglishEnabled", true);
            defaultLanguages.Add("isGermanEnabled", true);
            defaultLanguages.Add("isDutchEnabled", false);

            var setting = this.GetTenantJsonSettings("setup/onlineLanguages", tenantId).ToString();
            var dto = setting != "{}" ? JsonConvert.DeserializeObject<Dictionary<string, bool>>(setting) : defaultLanguages;
            return dto;
        }

        public Dictionary<string, bool> GetGuestInfoLanguages(Guid? tenantId = null)
        {
            var defaultLanguages = new Dictionary<string, bool>();
            defaultLanguages.Add("isDanishEnabled", true);
            defaultLanguages.Add("isEnglishEnabled", true);
            defaultLanguages.Add("isGermanEnabled", true);
            defaultLanguages.Add("isDutchEnabled", false);
            defaultLanguages.Add("isSwedishEnabled", false);
            defaultLanguages.Add("isNorwegianEnabled", false);

            var setting = this.GetTenantJsonSettings("setup/guestInfo/languages", tenantId).ToString();
            var dto = setting != "{}" ? JsonConvert.DeserializeObject<Dictionary<string, bool>>(setting) : defaultLanguages;
            return dto;
        }

        public Guid? GetSmsSettingSendOnArrival(Guid? tenantId = null)
        {
            var setting = this.GetTenantJsonSettings("setup/sendSmsOnArrival", tenantId)["sendSmsOnArrival"];
            if (setting == null || !Guid.TryParse(setting.ToString(), out _))
            {
                return null;
            }
            return Guid.Parse(setting.ToString());
        }

        public Guid? GetSmsSettingSendOnDeparture(Guid? tenantId = null)
        {
            var setting = this.GetTenantJsonSettings("setup/sendSmsOnDeparture", tenantId)["sendSmsOnDeparture"];
            if (setting == null || !Guid.TryParse(setting.ToString(), out _))
            {
                return null;
            }
            return Guid.Parse(setting.ToString());
        }

        public JObject GetOnlineBookingSettings(Guid? tenantId = null)
        {
            var setting = this.GetTenantJsonSettings("setup/online", tenantId);
            return setting;
        }

        public int GetTallyLastAccessLogCrosscheckIndex()
        {
            var setting = this.GetTenantJsonSettings("setup/tallyLastAccessLogCrosscheckIndex")["tallyLastAccessLogCrosscheckIndex"];
            if (setting == null || !int.TryParse(setting.ToString(), out _))
            {
                return 0;
            }
            return int.Parse(setting.ToString());
        }
        public void SetTallyLastAccessLogCrosscheckIndex(int index)
        {
            throw new NotImplementedException();
        }

        public string GetSmsSenderName(Guid? tenantId = null)
        {
            var setting = this.GetTenantJsonSettings("setup/smsSettings", tenantId)["senderName"];
            return setting?.ToString();
        }

        public QuickPaySettings GetQuickPaySettings(Guid? tenantId = null)
        {
            try
            {
                var quickpayMerchantId = this.GetTenantJsonSettings("setup/quickpayMerchantId", tenantId)["quickpayMerchantId"] ?? "";
                var quickpayNewMerchantId = this.GetTenantJsonSettings("setup/quickpayNewMerchantId", tenantId)["quickpayNewMerchantId"] ?? "";
                var quickpayMD5 = this.GetTenantJsonSettings("setup/quickpayMD5", tenantId)["quickpayMD5"] ?? "";
                var quickpayPrivateKey = this.GetTenantJsonSettings("setup/quickpayPrivateKey", tenantId)["quickpayPrivateKey"] ?? "";
                var quickpayApiUser = this.GetTenantJsonSettings("setup/quickpayApiUser", tenantId)["quickpayApiUser"] ?? "";
                var quickpayUserKey = this.GetTenantJsonSettings("setup/quickpayUserKey", tenantId)["quickpayUserKey"] ?? "";
                var isQuickpayPaymentLinkAutoCaptured = bool.Parse((this.GetTenantJsonSettings("setup/isQuickpayPaymentLinkAutoCaptured", tenantId)["isQuickpayPaymentLinkAutoCaptured"] ?? "true").ToString());
                var isQuickpayPaymentLinkAutoFee = bool.Parse((this.GetTenantJsonSettings("setup/isQuickpayPaymentLinkAutoFee", tenantId)["isQuickpayPaymentLinkAutoFee"] ?? "true").ToString());
                var isQuickpayOnlineLinkAutoCaptured = bool.Parse((this.GetTenantJsonSettings("setup/isQuickpayOnlineLinkAutoCaptured", tenantId)["isQuickpayOnlineLinkAutoCaptured"] ?? "false").ToString());
                var isQuickpayOnlineLinkAutoFee = bool.Parse((this.GetTenantJsonSettings("setup/isQuickpayOnlineLinkAutoFee", tenantId)["isQuickpayOnlineLinkAutoFee"] ?? "true").ToString());

                return new QuickPaySettings
                {
                    QuickPayMerchantId = quickpayMerchantId.ToString(),
                    QuickPayMD5 = quickpayMD5.ToString(),
                    QuickPayPrivateKey = quickpayPrivateKey.ToString(),
                    QuickPayApiUser = quickpayApiUser.ToString(),
                    QuickPayUserKey = quickpayUserKey.ToString(),
                    IsQuickPayPaymentLinkAutoCaptured = isQuickpayPaymentLinkAutoCaptured,
                    IsQuickPayPaymentLinkAutoFee = isQuickpayPaymentLinkAutoFee,
                    IsQuickPayOnlineLinkAutoCaptured = isQuickpayOnlineLinkAutoCaptured,
                    IsQuickPayOnlineLinkAutoFee = isQuickpayOnlineLinkAutoFee,
                    QuickPayNewMerchantId = quickpayNewMerchantId.ToString()
                };
            }
            catch (Exception exception)
            {
                throw new SettingsException("Fejl ved GetQuickPaySettings: " + exception);
            }
        }
    }
}

