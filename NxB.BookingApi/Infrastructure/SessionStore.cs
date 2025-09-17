using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class SessionStore : ISessionStore
    {
        private readonly AppDbContext _appDbContext;

        public SessionStore(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public bool DoesIdExist(string sessionId)
        {
            bool hasKey = _appDbContext.UserSessions.FirstOrDefault(x => x.SessionId == sessionId) != null;
            return hasKey;
        }

        public User FetchUser(string sessionId)
        {
            var userSession = this._appDbContext.UserSessions.FirstOrDefault(x => x.SessionId == sessionId);
            if (userSession == null) return null;
            User user = JsonConvert.DeserializeObject<User>(userSession.UserJson);
            return user;
        }

        public void Save(string sessionId, User user)
        {
            var userSession = new UserSession();
            userSession.SessionId = sessionId;
            userSession.UserJson = JsonConvert.SerializeObject(user);
            this._appDbContext.UserSessions.Add(userSession);
        }

        public void Remove(string sessionId)
        {
            if (this.DoesIdExist(sessionId))
            {
                var userSession = this._appDbContext.UserSessions.First(x => x.SessionId == sessionId);
                this._appDbContext.UserSessions.Remove(userSession);
            }
        }

        public long GetCount()
        {
            long count = this._appDbContext.UserSessions.Count();
            return count;
        }
    }
}
