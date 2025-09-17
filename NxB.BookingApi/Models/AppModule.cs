using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.BookingApi.Models
{
    public abstract class AppModule
    {
        public Guid Id { get; set; }
        public string Name { get; set; } 
        public decimal Price { get; set; }

        public string DefaultSettingsJson { get; set; }
    }

    public class EmailModule : AppModule
    {
        public string EmailFriendlyName { get; set; }  
    }

    public class TenantAppModule
    {
        public Guid AppModuleId { get; set; }
        public AppModule AppModule { get; set; }

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; }

        public bool IsEnabled { get; set; }
        public decimal? OverridePrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal? DiscountPercent { get; set; }
        public string OverrideSettingsJson { get; set; }
    }

    //when sending dtos to ui, override the Module DefaultSettingsJson with OverrideSettingsJson. The values will not be
    //pr tenant. Do the reverse when recieving updates to the values
}