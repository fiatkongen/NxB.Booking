using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface ITextSectionUserRepository
    {
        Task MarkSectionAsReadByCurrentUser(Guid textSectionId);
        Task<List<Guid>> FindSectionsReadByCurrentUser();
    }
}