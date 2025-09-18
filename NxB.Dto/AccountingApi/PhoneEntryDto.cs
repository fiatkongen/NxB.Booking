using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Dto.AccountingApi
{
    public class CreatePhoneEntryDto
    {
        public ContactPriority ContactPriority { get; set; }
        public string Prefix { get; set; }
        public string Number { get; set; }
        public bool SuggestForSms { get; set; }
    }

    public class PhoneEntryDto : CreatePhoneEntryDto
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }

        public string TotalNumber => (Prefix ?? "") + (Number ?? "");
    }
}
