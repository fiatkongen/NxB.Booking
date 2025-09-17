using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NxB.Domain.Common.Enums;

namespace NxB.BookingApi.Models
{
    public interface ITextSectionRepository
    {
        void Add(TextSection textSection);
        void Update(TextSection textSection);
        void Delete(Guid id);
        TextSection FindSingle(Guid id);
        Task<List<TextSection>> FindAll(TextSectionType textSectionType, bool filterOnlyUnread);
        Task<List<TextSection>> FindAllMinimum(TextSectionType textSectionType, bool filterOnlyUnread);
        Task<List<TextSection>> FindAllPublished(TextSectionType textSectionType, bool filterOnlyUnread);
        Task<int> GetUnreadCount(TextSectionType textSectionType);
    }
}