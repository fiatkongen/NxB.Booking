using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NxB.BookingApi.Infrastructure;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class Customer : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public long FriendlyId { get; set; }
        public Name Fullname { get; set; }
        public string CompanyName { get; set; }
        public Name Att { get; set; }
        public Address Address { get; set; }
        public string Cvr { get; set; }
        public string Passport { get; set; }
        public string Note { get; set; }
        public bool NoteState { get; set; }
        public long? BookingTagId { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsCompany { get; set; }
        public bool IsImported { get; set; }
        public string EmailEntriesJson { get; set; }
        public string PhoneEntriesJson { get; set; }
        public string LicensePlateEntriesJson { get; set; }
        public string FamilyMembersJson { get; set; }

        public List<Account> Accounts { get; set; } = new();
        public List<EmailEntry> EmailEntries { get; set; } = new();
        public List<PhoneEntry> PhoneEntries { get; set; } = new();
        public List<LicensePlateEntry> LicensePlateEntries { get; set; } = new();
        public List<CustomerGroupCustomer> CustomerGroupCustomers { get; set; } = new();
        public List<FamilyMember> FamilyMembers { get; set; } = new();

        public string DisplayString()
        {
            if (IsCompany)
            {
                return CompanyName + (Att != null ? (!string.IsNullOrWhiteSpace(Att.ToString()) ? " / att: " : "") + (Att.ToString()) : "");
            }
            else
            {
                return Fullname.ToString();
            }
        }

        private Customer() { }

        public Customer(Guid id, Guid tenantId, long friendlyId)
        {
            Id = id;
            TenantId = tenantId;
            FriendlyId = friendlyId;
            IsImported = false;
        }

        public void AddCustomerGroup(Guid customerGroupId)
        {
            if (!CustomerGroupCustomers.Exists(x => x.CustomerGroupId == customerGroupId))
            {
                CustomerGroupCustomers.Add(new CustomerGroupCustomer(this.Id, customerGroupId));
            }
        }
    }
}
