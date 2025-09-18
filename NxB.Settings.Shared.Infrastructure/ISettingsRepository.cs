using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace NxB.Settings.Shared.Infrastructure
{
    public interface ISettingsRepository : IAppSettingsRepository
    {
        JObject GetTenantJsonSettings(string path = null, Guid? tenantId = null);
        void SetTenantJsonSettings(JObject settings, string path = null);
        void DeleteTenantJsonSettings(string path);
        void CacheTenantSettings(Guid tenantId);
        T GetDeserializedSetting<T>(string path, Guid? tenantId = null) where T : class, new();

        JObject GetUserJsonSettings(string path = null);
        void SetUserJsonSettings(JObject settings, string path = null);
        void DeleteUserJsonSettings(string path);
        
        Task CacheAllActiveTenantsSettings();
    }
}
