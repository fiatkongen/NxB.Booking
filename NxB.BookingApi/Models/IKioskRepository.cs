using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface IKioskRepository
    {
        void Add(Kiosk kiosk);
        void Update(Kiosk kiosk);
        void MarkAsDeleted(Kiosk kiosk);
        void MarkAsUndeleted(Kiosk kiosk);
        Task<List<Kiosk>> FindAll(bool includeDeleted);
        Task<Kiosk> FindSingleOrDefault(Guid id);
        Task<Kiosk> FindSingleOrDefaultFromHardwareSerialNo(string serialNo);
    }
}