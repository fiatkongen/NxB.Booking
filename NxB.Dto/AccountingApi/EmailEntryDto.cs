using System;

namespace NxB.Dto.AccountingApi
{
    public class CreateEmailEntryDto
    {
        public ContactPriority ContactPriority { get; set; }
        public string Email { get; set; }
        public bool SuggestForEmails { get; set; }
    }

    public class EmailEntryDto : CreateEmailEntryDto
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
    }
}
