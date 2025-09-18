using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NxB.Domain.Common.Dto;

namespace NxB.Dto.AccountingApi
{
    public class BaseCustomerDto
    {
        public NameDto Fullname { get; set; }
        public string CompanyName { get; set; }
        public NameDto Att { get; set; }
        [Required]
        public AddressDto Address { get; set; }
        public string Cvr { get; set; }

        public string Passport { get; set; }
        public string Note { get; set; }
        public bool NoteState { get; set; }
        public bool IsCompany { get; set; }
        public bool IsImported { get; set; }
    }

    public class CreateCustomerDto : BaseCustomerDto
    {
        public List<CreateCustomerAccountDto> NewAccounts { get; set; } = new();
        public List<CreatePhoneEntryDto> NewPhoneEntries { get; set; } = new();
        public List<CreateEmailEntryDto> NewEmailEntries { get; set; } = new();
        public List<CreateLicensePlateEntryDto> NewLicensePlateEntries { get; set; } = new();
        public List<CustomerGroupCustomerDto> NewCustomerGroupCustomers { get; set; } = new();
        public List<CreateFamilyMemberDto> NewFamilyMembers { get; set; } = new();
    }

    public class ModifyCustomerDto : CreateCustomerDto
    {
        public Guid Id { get; set; }
        public List<PhoneEntryDto> ModifiedPhoneEntries { get; set; } = new();
        public List<EmailEntryDto> ModifiedEmailEntries { get; set; } = new();
        public List<LicensePlateEntryDto> ModifiedLicensePlateEntries { get; set; } = new();
        public List<CustomerGroupCustomerDto> DeletedCustomerGroupCustomers { get; set; } = new();
        public List<FamilyMemberDto> ModifiedFamilyMembers { get; set; } = new();
    }

    public class CustomerDto : BaseCustomerDto
    {
        public Guid Id { get; set; }
        public long FriendlyId { get; set; }
        public bool IsDeleted { get; set; }
        public string DisplayString { get; set; }

        public List<AccountDto> Accounts { get; set; } = new();
        public List<PhoneEntryDto> PhoneEntries { get; set; } = new();
        public List<EmailEntryDto> EmailEntries { get; set; } = new();
        public List<LicensePlateEntryDto> LicensePlateEntries { get; set; } = new();
        public List<CustomerGroupCustomerDto> CustomerGroupCustomers { get; set; } = new();
        public List<FamilyMemberDto> FamilyMembers { get; set; } = new();

        public AccountDto GetAccount(Guid accountId)
        {
            return this.Accounts.Single(x => x.Id == accountId);
        }

        public List<PhoneEntryDto> GetSuggestedPhoneEntries()
        {
            if (this.PhoneEntries.Count == 0) return new List<PhoneEntryDto>();
            var suggestedPhoneEntries = this.PhoneEntries.Where(x => x.SuggestForSms).ToList();
            if (this.PhoneEntries.Count == 1 || suggestedPhoneEntries.Count == 0) return new List<PhoneEntryDto> { this.PhoneEntries.First() };
            return suggestedPhoneEntries;
        }

        public List<EmailEntryDto> GetSuggestedEmailEntries()
        {
            if (this.EmailEntries.Count == 0) return new List<EmailEntryDto>();
            var suggestedEmailEntries = this.EmailEntries.Where(x => x.SuggestForEmails).ToList();
            if (this.EmailEntries.Count == 1 || suggestedEmailEntries.Count == 0) return new List<EmailEntryDto> { this.EmailEntries.First() };
            return suggestedEmailEntries;
        }

        public List<string> GetSuggestedEmails()
        {
            return GetSuggestedEmailEntries().Select(ee => ee.Email).ToList();
        }

        public List<string> GetSuggestedPhoneNumbers()
        {
            return GetSuggestedPhoneEntries().Select(p => p.TotalNumber).ToList();
        }

        public string GetSuggestedEmailsAsString()
        {
            return string.Join("; ", this.GetSuggestedEmails());
        }

        public string GetSuggestedPhoneNumbersAsString()
        {
            return string.Join("; ", this.GetSuggestedPhoneNumbers());
        }
    }

    public class ModifyCustomerNoteDto
    {
        public Guid CustomerId { get; set; }
        public string Note { get; set; }
        public bool? NoteState { get; set; } = null;
    }
}
