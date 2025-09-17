using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class TextSectionRepository : ITextSectionRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IClaimsProvider _claimsProvider;
        private readonly ITextSectionUserRepository _textSectionUserRepository;

        public TextSectionRepository(AppDbContext appDbContext, IClaimsProvider claimsProvider, ITextSectionUserRepository textSectionUserRepository)
        {
            _appDbContext = appDbContext;
            _claimsProvider = claimsProvider;
            _textSectionUserRepository = textSectionUserRepository;
        }

        public void Add(TextSection textSection)
        {
            _appDbContext.Add(textSection);
        }

        public void Update(TextSection textSection)
        {
            _appDbContext.Update(textSection);
        }

        public void Delete(Guid id)
        {
            var textSection = this.FindSingle(id);
            _appDbContext.Remove(textSection);
        }

        public TextSection FindSingle(Guid id)
        {
            return _appDbContext.TextSections.Single(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<List<TextSection>> FindAll(TextSectionType textSectionType, bool filterOnlyUnread)
        {
            var textSections = await _appDbContext.TextSections.Where(x => !x.IsDeleted && x.Type == textSectionType).ToListAsync();
            return await SortAndFilterTextSections(filterOnlyUnread, textSections);
        }

        private async Task<List<TextSection>> SortAndFilterTextSections(bool filterOnlyUnread, List<TextSection> textSections)
        {
            var unpublished = textSections.Where(x => x.PublishDate == null).OrderBy(x => x.Sort).ThenByDescending(x => x.CreateDate);
            var published = textSections.Where(x => x.PublishDate != null).OrderBy(x => x.Sort).ThenByDescending(x => x.PublishDate);
            textSections = unpublished.Concat(published).ToList();

            await SetIsReadOnTextSections(textSections);

            if (filterOnlyUnread)
            {
                textSections = textSections.Where(x => !x.IsRead).ToList();
            }

            return textSections;
        }

        public async Task<List<TextSection>> FindAllMinimum(TextSectionType textSectionType, bool filterOnlyUnread)
        {
            string commandText = $@"SELECT
                                Id,
                                Title,
                                [Version],
                                [Type],
                                PublishDate,
                                CreateDate,
                                IsDeleted,
                                KeyWords,
                                null AS Text,
                                Summary,
                                VideoUrl,
                                Sort,
                                HelpUrlMatch
                                FROM TextSection WHERE IsDeleted = 0 AND [Type]=" + (int)textSectionType;

            var textSections = await _appDbContext.TextSections.FromSqlRaw(commandText).ToListAsync();

            return await SortAndFilterTextSections(filterOnlyUnread, textSections);
        }

        private async Task SetIsReadOnTextSections(List<TextSection> textSections)
        {
            var textSectionsRead = await _textSectionUserRepository.FindSectionsReadByCurrentUser();
            textSections.Where(x => textSectionsRead.Contains(x.Id)).ToList().ForEach(x => x.IsRead = true);
        }

        public async Task<List<TextSection>> FindAllPublished(TextSectionType textSectionType, bool filterOnlyUnread)
        {
            var textSections = (await this.FindAll(textSectionType, filterOnlyUnread)).Where(x => x.PublishDate != null && !x.IsDeleted).OrderByDescending(x => x.PublishDate).ToList();
            return textSections;
        }

        public async Task<int> GetUnreadCount(TextSectionType textSectionType)
        {
            await using var dbConnection = _appDbContext.Database.GetDbConnection();
            string commandText = $@"SELECT COUNT(*) FROM TextSection WHERE PublishDate is not null AND IsDeleted = 0 AND [Type]={(int)textSectionType} AND Id NOT IN (
                                    SELECT TextSectionId FROM TextSectionUser WHERE TextSectionUser.UserId = '{_claimsProvider.GetUserId()}')";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = commandText;
            cmd.CommandType = CommandType.Text;
            cmd.Connection = dbConnection;

            await dbConnection.OpenAsync();

            object result = await cmd.ExecuteScalarAsync();
            await dbConnection.CloseAsync();

            if (result != null && DBNull.Value != result)
            {
                return (int)result;
            }

            return 0;
        }
    }
}