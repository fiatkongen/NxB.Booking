using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Munk.Utils.Object;

namespace NxB.Dto.TenantApi
{
    public class CreateTenantDto : CreateTenantPublicDto
    {
        [Required]
        public string ClientId { get; set; }

        public string LegacyId { get; set; }

        public bool IsLocked { get; set; }

        public string Note { get; set; }

        public string SubDomain { get; set; }

        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }


    public class TenantPublicDto : CreateTenantPublicDto
    {
        [NoEmpty]
        public Guid Id { get; set; }

        [Required]
        public string ClientId { get; set; }

        public string LegacyId { get; set; }

        public string SubDomain { get; set; }

    }

    public class ModifyTenantPublicDto : CreateTenantPublicDto
    {
        [NoEmpty]
        public Guid Id { get; set; }
    }

    public class CreateTenantPublicDto
    {
        public string Address { get; set; }

        public string Zip { get; set; }

        public string City { get; set; }

        public string CountryId { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string ContactName { get; set; }

        [NoEmpty]
        public string CompanyName { get; set; }

        public string Cvr { get; set; }

        public Guid? LogoFileId { get; set; }

        public string HomePage { get; set; }

        public string ExtrasJson { get; set; }

        public string BankName { get; set; }
        public string BankRegNo { get; set; }
        public string BankAccount { get; set; }
        public string BankIBAN { get; set; }
        public string BankSWIFT { get; set; }

        public CompanySegment CompanySegment { get; set; }

        public string Latitude { get; set; }
        public string Longitude { get; set; }

        public Dictionary<string, string> MiscValues { get; set; }
    }

    public class TenantDto : CreateTenantDto
    {
        [NoEmpty]
        public Guid Id { get; set; }
    }

    public enum CompanySegment
    {
        Camping,
        Marina
    }
}
