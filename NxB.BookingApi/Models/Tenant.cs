using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using NxB.Dto.TenantApi;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class Tenant
    {
        private Tenant() { }

        public Tenant(Guid id, string clientId)
        {
            Id = id;
            ClientId = clientId;
        }

        public Guid Id { get; private set; }
        public DateTime? CreateDate { get; set; } = DateTime.Now.ToEuTimeZone();
        public string ClientId { get; private set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string Address { get; set; }
        public string Zip { get; set; }
        public string City { get; set; }
        public string CountryId { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string LegacyId { get; set; }
        public bool UseForLegacyOnline { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsLocked { get; set; }
        public string Cvr { get; set; }
        public string Note { get; set; }
        public Guid? LogoFileId { get; set; }
        public string ExtrasJson { get; set; }
        public string SubDomain { get; set; }
        public string HomePage { get; set; }
        public string BankName { get; set; }
        public string BankRegNo { get; set; }
        public string BankAccount { get; set; }
        public string BankIBAN { get; set; }
        public string BankSWIFT { get; set; }
        public CompanySegment CompanySegment { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}