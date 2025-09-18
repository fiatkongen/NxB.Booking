using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using Newtonsoft.Json;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;

namespace NxB.BookingApi.Models
{
    public class AccessGroup : TallyRadiosFilter, ITenantEntity, IEntitySaved, IJsonEntity
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string Name { get; set; }
        public int Option { get; set; }
        public bool IsDeleted { get; set; }
        public int Sort { get; set; }

        public string SwitchRadiosJson { get; set; }
        public string SocketRadiosJson { get; set; }

        private AccessGroup()
        {
        }

        public AccessGroup(Guid id, Guid tenantId)
        {
            Id = id;
            TenantId = tenantId;
        }

        public void MarkAsDeleted()
        {
            this.IsDeleted = true;
        }

        public void OnEntitySaved(EntityState entityState)
        {
            Serialize();
        }

        public void Serialize()
        {
            if (SwitchRadios.Count == 0)
            {
                SwitchRadiosJson = null;
            }
            else
            {
                SwitchRadiosJson = JsonConvert.SerializeObject(SwitchRadios);
            }

            if (SocketRadios.Count == 0)
            {
                SocketRadiosJson = null;
            }
            else
            {
                SocketRadiosJson = JsonConvert.SerializeObject(SocketRadios);
            }
        }

        public void Deserialize()
        {
            if (!string.IsNullOrEmpty(SwitchRadiosJson))
            {
                SwitchRadios = JsonConvert.DeserializeObject<List<RadioAccessUnit>>(SwitchRadiosJson);
            }
            else
            {
                SwitchRadios = new List<RadioAccessUnit>();
            }

            if (!string.IsNullOrEmpty(SocketRadiosJson))
            {
                SocketRadios = JsonConvert.DeserializeObject<List<RadioAccessUnit>>(SocketRadiosJson);
            }
            else
            {
                SocketRadios = new List<RadioAccessUnit>();
            }
        }
    }
}