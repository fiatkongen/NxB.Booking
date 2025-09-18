using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using Munk.Utils.Object;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.Clients.Interfaces;
using NxB.Dto.TenantApi;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Controllers.Tenant
{
    [Produces("application/json")]
    [Authorize]
    [Route("textsection")]
    [ApiValidationFilter]
    public class TextSectionController : BaseController
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly ITextSectionRepository _textSectionRepository;
        private readonly ITextSectionUserRepository _textSectionUserRepository;
        private readonly IGroupedBroadcasterClient _groupedBroadcasterClient;
        private readonly TelemetryClient _telemetryClient;

        public TextSectionController(AppDbContext appDbContext, IMapper mapper, ITextSectionRepository textSectionRepository, ITextSectionUserRepository textSectionUserRepository, IGroupedBroadcasterClient groupedBroadcasterClient, TelemetryClient telemetryClient)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _textSectionRepository = textSectionRepository;
            _textSectionUserRepository = textSectionUserRepository;
            _groupedBroadcasterClient = groupedBroadcasterClient;
            _telemetryClient = telemetryClient;
        }

        [HttpGet]
        [Route("")]
        public ObjectResult FindSingleTextSection([NoEmpty] Guid id)
        {
            var textSection = _textSectionRepository.FindSingle(id);
            var textSectionDto = _mapper.Map<TextSection, TextSectionDto>(textSection);
            return new OkObjectResult(textSectionDto);
        }

        [HttpPost]
        [Route("")]
        [Authorize(Roles = "Admin")]
        public async Task<ObjectResult> CreateTextSection([FromBody] CreateTextSectionDto createTextSectionDto)
        {
            var textSection = new TextSection(Guid.NewGuid());
            _mapper.Map(createTextSectionDto, textSection);
            _textSectionRepository.Add(textSection);
            await _appDbContext.SaveChangesAsync();
            var textSectionDto = _mapper.Map<TextSection, TextSectionDto>(textSection);
            await TryBroadcastTriggerUnreadMessagesCountRefresh();
            return new CreatedResult(new Uri("?id=" + textSectionDto.Id, UriKind.Relative), textSectionDto);
        }

        [HttpPut]
        [Route("")]
        [Authorize(Roles = "Admin")]
        public async Task<ObjectResult> ModifyTextSection([FromBody] ModifyTextSectionDto modifyTextSectionDto)
        {
            var textSection = _textSectionRepository.FindSingle(modifyTextSectionDto.Id);
            _mapper.Map(modifyTextSectionDto, textSection);
            _textSectionRepository.Update(textSection);
            await _appDbContext.SaveChangesAsync();
            var textSectionDto = _mapper.Map<TextSection, TextSectionDto>(textSection);
            await TryBroadcastTriggerUnreadMessagesCountRefresh();
            return new OkObjectResult(textSectionDto);
        }

        [HttpGet]
        [Route("list/all")]
        [Authorize(Roles = "Admin")]
        public async Task<ObjectResult> FindAllTextSections(TextSectionType textSectionType, bool filterOnlyUnread = false)
        {
            var textSections = await _textSectionRepository.FindAll(textSectionType, filterOnlyUnread);
            var textSectionDtos = textSections.Select(x => _mapper.Map<TextSection, TextSectionDto>(x));
            return new OkObjectResult(textSectionDtos);
        }

        [HttpGet]
        [Route("list/all/minimum")]
        public async Task<ObjectResult> FindAllTextSectionsMinimum(TextSectionType textSectionType, bool filterOnlyUnread = false)
        {
            var textSections = await _textSectionRepository.FindAllMinimum(textSectionType, filterOnlyUnread);
            var textSectionDtos = textSections.Select(x => _mapper.Map<TextSection, TextSectionDto>(x));
            return new OkObjectResult(textSectionDtos);
        }

        [HttpGet]
        [Route("list/all/published")]
        public async Task<ObjectResult> FindAllPublishedTextSections(TextSectionType textSectionType, bool filterOnlyUnread = false)
        {
            var textSections = await _textSectionRepository.FindAllPublished(textSectionType, filterOnlyUnread);
            var textSectionDtos = textSections.Select(x => _mapper.Map<TextSection, TextSectionDto>(x));
            return new OkObjectResult(textSectionDtos);
        }

        [HttpDelete]
        [Route("")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTextSection(Guid id)
        {
            var textSection = _textSectionRepository.FindSingle(id);
            _textSectionRepository.Delete(id);
            await _appDbContext.SaveChangesAsync();
            await TryBroadcastTriggerUnreadMessagesCountRefresh();
            return new OkResult();
        }

        [HttpPut]
        [Route("publish")]
        [Authorize(Roles = "Admin")]
        public async Task<ObjectResult> PublishTextSection(Guid id)
        {
            var textSection = _textSectionRepository.FindSingle(id);
            textSection.Publish();
            await _appDbContext.SaveChangesAsync();
            var textSectionDto = _mapper.Map<TextSection, TextSectionDto>(textSection);
            await TryBroadcastTriggerUnreadMessagesCountRefresh();
            return new OkObjectResult(textSectionDto);
        }

        [HttpPut]
        [Route("unpublish")]
        [Authorize(Roles = "Admin")]
        public async Task<ObjectResult> UnPublishTextSection(Guid id)
        {
            var textSection = _textSectionRepository.FindSingle(id);
            textSection.Unpublish();
            await _appDbContext.SaveChangesAsync();
            var textSectionDto = _mapper.Map<TextSection, TextSectionDto>(textSection);
            await TryBroadcastTriggerUnreadMessagesCountRefresh();
            return new OkObjectResult(textSectionDto);
        }

        [HttpPut]
        [Route("markasdeleted")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkTextSectionAsDeleted(Guid id)
        {
            var textSection = _textSectionRepository.FindSingle(id);
            textSection.Delete();
            await _appDbContext.SaveChangesAsync();
            await TryBroadcastTriggerUnreadMessagesCountRefresh();
            return new OkResult();
        }

        [HttpGet]
        [Route("unread/count")]
        public async Task<ObjectResult> GetUnreadCount(TextSectionType textSectionType)
        {
            var unreadCount = await _textSectionRepository.GetUnreadCount(textSectionType);
            var result = new Dictionary<string, int>();
            result.Add("unreadCount", unreadCount);
            return new OkObjectResult(result);
        }

        public async Task TryBroadcastTriggerUnreadMessagesCountRefresh()
        {
            try
            {
                await _groupedBroadcasterClient.TryTriggerRefreshCounter("triggerUnreadMessages");
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackException(exception);
            }
        }

        [HttpPut]
        [Route("markasread")]
        public async Task<IActionResult> MarkTextSectionAsRead(Guid id)
        {
            await _textSectionUserRepository.MarkSectionAsReadByCurrentUser(id);
            await _appDbContext.SaveChangesAsync();
            return new OkResult();
        }
    }
}