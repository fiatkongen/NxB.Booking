using System;
using System.Collections.Generic;
using System.Text;
using NxB.Dto.AccountingApi;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class EmailEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Index { get; set; }
        public ContactPriority ContactPriority { get; set; }
        public string Email { get; set; }
        public bool SuggestForEmails { get; set; }
        public bool IsDeleted { get; set; }

        private EmailEntry() { }

        public EmailEntry(string email, ContactPriority contactPriority, bool suggestForEmails)
        {
            ContactPriority = contactPriority;
            Email = email;
            SuggestForEmails = suggestForEmails;
        }
    }
}
