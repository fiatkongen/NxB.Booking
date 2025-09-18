using System;
using Newtonsoft.Json.Linq;
using NxB.Domain.Common.Interfaces;

namespace NxB.Settings.Shared.Infrastructure
{
    public class SettingsItem : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string Context { get; set; }
        public JObject Value { get; set; }
        public string JsonSettingsItem { get; set; }
    }
}
