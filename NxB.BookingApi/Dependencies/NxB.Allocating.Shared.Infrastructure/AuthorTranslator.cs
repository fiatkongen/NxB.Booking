using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Interfaces;

namespace NxB.Allocating.Shared.Infrastructure
{
    public class AuthorTranslator<TAppDbContext> : IAuthorTranslator<TAppDbContext> where TAppDbContext : DbContext
    {
        private readonly Dictionary<Guid, string> _nameCache = new();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string GetName(Guid userId, TAppDbContext appDbContext)
        {
            if (_nameCache.ContainsKey(userId))
            {
                return _nameCache[userId];
            }

            var user = appDbContext.Set<Author>().SingleOrDefault(x => x.Id == userId);
            if (user != null)
            {
                _nameCache.Add(userId, user.Username);
                return user.Username;
            }

            return "N/A";
        }
    }
}
