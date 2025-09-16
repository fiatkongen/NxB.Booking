using System;
using System.Collections.Generic;
using System.Text;
using NxB.Dto.AccountingApi;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class PhoneEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Index { get; set; }
        public ContactPriority ContactPriority { get; set; }
        public string Prefix { get; set; }
        public string Number { get; set; }
        public bool SuggestForSms { get; set; }
        public bool IsDeleted { get; set; }

        private PhoneEntry() { }

        public PhoneEntry(string prefix, string number, ContactPriority contactPriority, bool suggestForSms)
        {
            Number = number;
            Prefix = prefix;
            ContactPriority = contactPriority;
            SuggestForSms = suggestForSms;
        }
    }
}
