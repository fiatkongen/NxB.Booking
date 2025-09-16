using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using Newtonsoft.Json;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;
using NxB.Dto.OrderingApi;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class Access : ITenantEntity, IEntitySaved
    {
        public Guid Id { get; set; }
        public int Code { get; set; }
        public Guid TenantId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ActivationDate { get; set; }
        public DateTime? DeactivationDate { get; set; }
        public bool IsActive => this.DeactivationDate == null;
        public DateTime? AutoActivationDate { get; set; }
        public DateTime? AutoDeactivationDate { get; set; }
        public AccessType AccessType { get; set; }
        public bool IsKeyCode { get; set; }
        public bool HasError { get; set; }
        public bool IsDeleted { get; set; }
        public Guid SubOrderId { get; set; }
        public string AccessNames { get; set; }
        public string AccessibleItemsJson { get; set; }

        public AccessibleItems AccessibleItems { get; set; }

        private Access(string accessibleItemsJson)
        {
            AccessibleItemsJson = accessibleItemsJson;

            this.Deserialize();
        }

        public Access(Guid id, Guid tenantId, int code, bool isKeyCode, string accessibleItemsJson = null) : this(accessibleItemsJson)
        {
            Id = id;
            Code = code;
            IsKeyCode = isKeyCode;
            TenantId = tenantId;
            CreateDate = DateTime.Now.ToEuTimeZone();
        }

        public void Activate()
        {
            AutoActivationDate = null;
            ActivationDate = DateTime.Now.ToEuTimeZone();
        }

        public void Deactivate()
        {
            this.DeactivationDate = DateTime.Now.ToEuTimeZone();
            this.AutoActivationDate = null;
            this.AutoDeactivationDate = null;
        }

        public void Reactivate()
        {
            this.DeactivationDate = null;
        }

        public void MarkAsDeleted()
        {
            this.IsDeleted = true;
        }

        public void Serialize()
        {
            AccessibleItemsJson = JsonConvert.SerializeObject(AccessibleItems);
            AccessNames = AccessibleItems != null ? AccessibleItems.GetAccessNames() : "Standard";
        }

        public void Deserialize()
        {
            if (!string.IsNullOrEmpty(AccessibleItemsJson))
            {
                AccessibleItems = JsonConvert.DeserializeObject<AccessibleItems>(AccessibleItemsJson);
            }
        }

        public Access Clone(Guid id)
        {
            var access = new Access(id, TenantId, Code, IsKeyCode);
            access.AccessNames = AccessNames;
            access.SubOrderId = SubOrderId;
            return access;
        }

        public void OnEntitySaved(EntityState entityState)
        {
            Serialize();
        }
    }
}
