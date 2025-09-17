using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NxB.Domain.Common.Interfaces;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class TextSectionUserRepository : ITextSectionUserRepository
    {
        private readonly IClaimsProvider _claimsProvider;
        private readonly AppDbContext _appDbContext;

        public TextSectionUserRepository(AppDbContext appDbContext, IClaimsProvider claimsProvider)
        {
            _appDbContext = appDbContext;
            _claimsProvider = claimsProvider;
        }

        public async Task MarkSectionAsReadByCurrentUser(Guid textSectionId)
        {
            var userId = _claimsProvider.GetUserId();
            var existingTextSectionUser = await _appDbContext.TextSectionUsers.FindAsync(textSectionId, userId);
            if (existingTextSectionUser != null) return;
            var textSectionUser = new TextSectionUser(textSectionId, userId);

            await this._appDbContext.AddAsync(textSectionUser);
        }

        public Task<List<Guid>> FindSectionsReadByCurrentUser()
        {
            var textSectionIdsRead = _appDbContext.TextSectionUsers.Where(x => x.UserId == this._claimsProvider.GetUserId()).Select(x => x.TextSectionId).ToListAsync();
            return textSectionIdsRead;
        }
    }
}