using System;

namespace NxB.BookingApi.Models
{
    public class TextSectionUser
    {
        public Guid TextSectionId { get; set; }
        public Guid UserId { get; set; }
        public DateTime ReadDate { get; set; }

        public TextSectionUser(Guid textSectionId, Guid userId)
        {
            TextSectionId = textSectionId;
            UserId = userId;
            ReadDate = DateTime.Now.ToEuTimeZone();
        }
    }
}