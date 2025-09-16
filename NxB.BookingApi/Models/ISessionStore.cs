using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface ISessionStore
    {
        bool DoesIdExist(string sessionId);
        User FetchUser(string sessionId);
        void Save(string sessionId, User user);
        void Remove(string sessionId);
        long GetCount();
    }
}