using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using Munk.Utils.Object;
using NxB.Dto.TallyWebIntegrationApi;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Controllers.TallyWebIntegration
{
    [Produces("application/json")]
    [Route("accessgroup")]
    [Authorize]
    [ApiValidationFilter]
    public class AccessGroupController : BaseController
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IAccessGroupRepository _accessGroupRepository;
        private readonly AccessGroupFactory _accessGroupFactory;

        public AccessGroupController(AppDbContext appDbContext, IMapper mapper, IAccessGroupRepository accessGroupRepository, AccessGroupFactory accessGroupFactory)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _accessGroupRepository = accessGroupRepository;
            _accessGroupFactory = accessGroupFactory;
        }

        [HttpGet]
        [Route("")]
        public async Task<ObjectResult> FindSingleAccess([NoEmpty]Guid id)
        {
            var accessGroup = await _accessGroupRepository.FindSingle(id);
            var accessGroupDto = _mapper.Map<AccessGroupDto>(accessGroup);
            return new OkObjectResult(accessGroupDto);
        }

        [HttpPost]
        [Route("")]
        public async Task<ObjectResult> CreateAccessGroup([FromBody] CreateAccessGroupDto createAccessGroupDto)
        {
            var accessGroup = _accessGroupFactory.Create();
            _mapper.Map(createAccessGroupDto, accessGroup);

            _accessGroupRepository.Add(accessGroup);
            await _appDbContext.SaveChangesAsync();

            var accessGroupDto = _mapper.Map<AccessGroupDto>(accessGroup);
            return new CreatedResult(new Uri("?id=" + accessGroupDto.Id, UriKind.Relative), accessGroupDto);
        }

        [HttpPut]
        [Route("")]
        public async Task<ObjectResult> ModifyAccessGroup([FromBody] ModifyAccessGroupDto modifyAccessGroupDto)
        {
            var accessGroup = await _accessGroupRepository.FindSingle(modifyAccessGroupDto.Id);
            _mapper.Map(modifyAccessGroupDto, accessGroup);

            _accessGroupRepository.Update(accessGroup);
            await _appDbContext.SaveChangesAsync();

            var accessGroupDto = _mapper.Map<AccessGroupDto>(accessGroup);
            return new OkObjectResult(accessGroupDto);
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAllAccesses()
        {
            var accessGroups = await _accessGroupRepository.FindAll();
            var accessGroupDtos = accessGroups.Select(x => _mapper.Map<AccessGroupDto>(x)).ToList();
            return new OkObjectResult(accessGroupDtos);
        }

        [HttpPut]
        [Route("markasdeleted")]
        public async Task<ObjectResult> MarkAccessAsDeleted(Guid id)
        {
            var accessGroup = await _accessGroupRepository.MarkAsDeleted(id);
            await _appDbContext.SaveChangesAsync();
            var accessGroupDto = _mapper.Map<AccessGroupDto>(accessGroup);
            return new OkObjectResult(accessGroupDto);
        }

    }
}
