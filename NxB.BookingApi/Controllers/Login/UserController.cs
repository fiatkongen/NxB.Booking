using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using Munk.Utils.Object;
using Newtonsoft.Json.Linq;
using NxB.Domain.Common.Dto;
using NxB.Domain.Common.Interfaces;
using NxB.Clients.Interfaces;
using NxB.Dto.LoginApi;
using NxB.Dto.TenantApi;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;
using NxB.Settings.Shared.Infrastructure;

namespace NxB.BookingApi.Controllers.Login
{
    [Produces("application/json")]
    [Route("user")]
    [Authorize]
    [ApiValidationFilter]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _appDbContext;
        private readonly IClaimsProvider _claimsProvider;
        private readonly ITenantClient _tenantClient;
        private readonly IMapper _mapper;

        public UserController(IUserRepository userRepository, AppDbContext appDbContext, IClaimsProvider claimsProvider, IMapper mapper, ITenantClient tenantClient)
        {
            _userRepository = userRepository;
            _appDbContext = appDbContext;
            _claimsProvider = claimsProvider;
            _mapper = mapper;
            _tenantClient = tenantClient;
        }

        [HttpPost]
        [Route("")]
        public async Task<ObjectResult> CreateUser([FromBody]CreateUserDto createUserDto)
        {
            var user = new User(createUserDto.Username, createUserDto.Login, createUserDto.Password,
                _claimsProvider.GetTenantId(), "dk");
            //Hack: for now user is always a simple user
            createUserDto.Roles = null;

            _mapper.Map(createUserDto, user);
            _userRepository.Add(user);
            await _appDbContext.SaveChangesAsync();

            user = _userRepository.FindSingle(user.Id);
            var userDto = _mapper.Map<UserDto>(user);
            return new CreatedResult(new Uri("?id=" + userDto.Id, UriKind.Relative), userDto);
        }

        [HttpPut]
        [Route("")]
        public async Task<ObjectResult> ModifyUser([FromBody]UserDto modifyUserDto)
        {
            var user = _userRepository.FindSingle(modifyUserDto.Id);
            _mapper.Map(modifyUserDto, user);
            _userRepository.Update(user);
            await _appDbContext.SaveChangesAsync();

            user = _userRepository.FindSingle(user.Id);
            var userDto = _mapper.Map<UserDto>(user);
            return new OkObjectResult(userDto);
        }

        [HttpGet]
        [Route("")]
        public ObjectResult FindUser(Guid id)
        {
            var user = _userRepository.FindSingle(id);
            var userDto = _mapper.Map<UserDto>(user);
            return new OkObjectResult(userDto);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("login")]
        public ObjectResult FindUserFromLogin(Guid tenantId, string login)
        {
            var user = _userRepository.FindFromLogin(tenantId, login);
            var userDto = _mapper.Map<UserDto>(user);
            return new OkObjectResult(userDto);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        [Route("administrator/tenant")]
        public IActionResult AddTenantToAdministrator([NoEmpty] Guid tenantId)
        {
            _userRepository.AddTenantToAdministrator(tenantId);
            this._appDbContext.SaveChanges();
            return new OkResult();
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        [Route("onlineuser/tenant")]
        public IActionResult AddTenantToOnlineUser([NoEmpty] Guid tenantId)
        {
            _userRepository.AddTenantToOnlineUser(tenantId);
            this._appDbContext.SaveChanges();
            return new OkResult();
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        [Route("tenant")]
        public IActionResult AddTenantToUser([NoEmpty] Guid userId, [NoEmpty] Guid tenantId)
        {
            _userRepository.AddTenantToUser(userId, tenantId);
            this._appDbContext.SaveChanges();
            return new OkResult();
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAllUsers(bool includeDeleted = false)
        {
            var customers = includeDeleted ? await _userRepository.FindAllIncludeDeleted() : await _userRepository.FindAll();
            var customerDtos = customers.Select(x => _mapper.Map<UserDto>(x));
            return new OkObjectResult(customerDtos);
        }

        [HttpDelete]
        [Route("")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteUserPermanently([NoEmpty]Guid id)
        {
            _userRepository.Delete(id);
            _appDbContext.SaveChanges();

            return new OkResult();
        }

        [HttpPut]
        [Route("markasdeleted")]
        public IActionResult MarkUserAsDeleted([NoEmpty]Guid id)
        {
            var user = _userRepository.FindSingle(id);

            user.MarkAsDeleted();
            _userRepository.Update(user);
            _appDbContext.SaveChanges();

            return new OkResult();
        }

        [HttpPut]
        [Route("markasdisabled")]
        public IActionResult MarkUserAsDisabled([NoEmpty]Guid id)
        {
            var user = _userRepository.FindSingle(id);

            user.MarkAsDisabled();
            _userRepository.Update(user);
            _appDbContext.SaveChanges();

            return new OkResult();
        }

        [HttpPut]
        [Route("markasenabled")]
        public IActionResult MarkUserAsEnabled([NoEmpty] Guid id)
        {
            var user = _userRepository.FindSingle(id);

            user.MarkAsEnabled();
            _userRepository.Update(user);
            _appDbContext.SaveChanges();

            return new OkResult();
        }

        [HttpPut]
        [Route("changepassword")]
        public IActionResult ChangePasswordForUser([FromBody]ChangePasswordDto changePasswordDto)
        {
            var user = _userRepository.FindSingle(changePasswordDto.UserId);

            user.ChangePassword(changePasswordDto.OldPassword, changePasswordDto.NewPassword);
            _userRepository.Update(user);
            _appDbContext.SaveChanges();

            return new OkResult();
        }

        [HttpPut]
        [AllowAnonymous]
        [Route("changepassword2")]
        public async Task<IActionResult> ChangePassword2ForUser([FromBody] ChangePassword2Dto changePassword2Dto)
        {
            var tenantDto = await _tenantClient.FindTenantFromClientId(changePassword2Dto.ClientId);
            var user = _userRepository.FindFromLogin(tenantDto.Id, changePassword2Dto.Login);

            user.ChangePassword(changePassword2Dto.OldPassword, changePassword2Dto.NewPassword);
            _userRepository.Update(user);
            await _appDbContext.SaveChangesAsync();

            return new OkResult();
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        [Route("resetpassword")]
        public async Task<IActionResult> ResetPasswordForUser([FromBody] ResetPasswordDto resetPasswordDto)
        {
            var user = _userRepository.FindSingle(resetPasswordDto.UserId);

            user.Password = resetPasswordDto.NewPassword;
            _userRepository.Update(user);
            await _appDbContext.SaveChangesAsync();

            return new OkResult();
        }
    }
}
