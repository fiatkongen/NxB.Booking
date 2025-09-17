using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml.Linq;
using AutoMapper;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.Utils.Object;
using Newtonsoft.Json;
using NxB.Domain.Common.Constants;
using NxB.Dto;
using NxB.Dto.Clients;
using NxB.Dto.LoginApi;
using NxB.Dto.TenantApi;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;

//https://www.devtrends.co.uk/blog/handling-errors-in-asp.net-core-web-api
//https://www.youtube.com/watch?v=TUXZujOkJGo
namespace NxB.BookingApi.Controllers.Login
{
    [Produces("application/json")]
    public class LoginController : BaseController
    {
        private readonly ISessionStore _sessionStore;
        private readonly IUserRepository _userRepository;
        private readonly ITenantClient _tenantClient;
        private readonly TelemetryClient _telemetry;
        private readonly IMapper _mapper;

        public LoginController(ISessionStore sessionStore, IUserRepository userRepository, ITenantClient tenantClient, TelemetryClient telemetry, IMapper mapper)
        {
            _sessionStore = sessionStore;
            _userRepository = userRepository;
            _tenantClient = tenantClient;
            _telemetry = telemetry;
            this._mapper = mapper;
        }

        [HttpGet]
        [Route("login/session/verify")]
        public bool VerifySession()
        {
            string sessionId;
            try
            {
                sessionId = GetSessionId();
                if (sessionId == null)
                    return false;
            }
            catch
            {
                return false;
            }
            var doesSessionExist = _sessionStore.DoesIdExist(sessionId);
            return doesSessionExist;
        }

        [HttpGet]
        [Authorize]
        [Route("login/session/credentials")]
        public async Task<ObjectResult> GetCredentials()
        {
            try
            {
                if (!this.HttpContext.User.Identity?.IsAuthenticated ?? false) return new ObjectResult(null);
                var userDto = await ValidateUser();

                AddUserRoles(userDto);
                return new OkObjectResult(userDto);

            }
            catch (Exception exception)
            {
                _telemetry.TrackTrace("LoginController.GetCredentials Could not build user: " + exception);
                _telemetry.TrackException(exception);
                throw;
            }
        }

        [HttpGet]
        [Authorize]
        [Route("login/session/user")]
        public async Task<ObjectResult> GetCurrentUser()
        {
            try
            {
                if (!this.HttpContext.User.Identity?.IsAuthenticated ?? false) return new ObjectResult(null);
                var credentials = BuildCredentialsDtoFromPrincipal();
                var user = _userRepository.FindSingleOrDefault(credentials.Id);
                var userDto = _mapper.Map<UserDto>(user);

                return new OkObjectResult(userDto);

            }
            catch (Exception exception)
            {
                _telemetry.TrackTrace("LoginController.GetCurrentUser Could not build user: " + exception);
                _telemetry.TrackException(exception);
                throw;
            }
        }

        private async Task<CredentialsDto> ValidateUser()
        {
            try
            {
                var userDto = BuildCredentialsDtoFromPrincipal();
                if (!await this._userRepository.IsValid(userDto.Id))
                {
                    await SignOutAndRejectUser();
                    throw new ValidateUserException("Bruger er ikke valid. Måske er bruger slettet eller inaktiveret.");
                }
                return userDto;
            }
            catch (Exception exception)
            {
                await SignOutAndRejectUser();
                throw new ValidateUserException("Kan ikke validere bruger: " + exception.Message);
            }
        }

        private async Task SignOutAndRejectUser()
        {
            try
            {
                await SignOutPrincipal();
            }
            catch { }

            Unauthorized();
        }

        private string GetSessionId()
        {
            return HttpContext.User.Claims.FirstOrDefault(c => c.Type == Claims.CLAIM_SESSIONID_NAME)?.Value;
        }

        [HttpPut]
        [Authorize(Roles = Claims.CLAIMS_ROLE_ADMIN)]
        [Route("login/astenant")]
        public async void LoginAsTenant([NoEmpty] Guid tenantId)
        {
            Guid userId = Guid.Parse(GetClaim(Claims.CLAIM_USERID_NAME));
            string sessionId = GetClaim(Claims.CLAIM_SESSIONID_NAME);
            var user = this._userRepository.FindSingle(userId);
            await SignOutPrincipal();
            throw new NotImplementedException();
            // await SignInPrincipal(sessionId, tenantId, user);
        }

        [HttpPost]
        [Route("login/session")]
        public async Task<IActionResult> CreateSession([FromBody] LoginDto loginDto)
        {
            return await CreateSession(loginDto.Login, loginDto.Password, loginDto.AccessCardId, loginDto.ClientId, Unauthorized, sessionId => Ok(new JsonResult("{message = 'Login successful', sessionId='" + sessionId + "'}")));
        }

        [HttpPost]
        [Route("login/switch/session")]
        public async Task<IActionResult> SwitchSession([FromBody] LoginDto loginDto)
        {
            var tenantDto = await _tenantClient.FindTenant(loginDto.ClientId);

            User user;
            if (loginDto.AccessCardId == null)
            {
                user = _userRepository.FindFromLogin(tenantDto.Id, loginDto.Login);
                if (user == null) throw new Exception("Bruger findes ikke");
                if (user.Password != loginDto.Password) throw new Exception("Forkert password");
            }
            else
            {
                user = _userRepository.FindFromAccessCardId(tenantDto.Id, loginDto.AccessCardId);
                if (user == null) throw new Exception("Bruger med adgangskort findes ikke");
            }
            var sessionId = GetSessionId();
            var newSession = await CreateSession(loginDto.Login, loginDto.Password, loginDto.AccessCardId, loginDto.ClientId, Unauthorized, sessionId => Ok(new JsonResult("{message = 'Login successful', sessionId='" + sessionId + "'}")));
            _sessionStore.Remove(sessionId);
            return newSession;
        }

        [HttpGet]
        [Route("login/session/json")]
        public async Task<IActionResult> CreateSessionJson(string login, string password, string accessCardId, string clientId)
        {
            try
            {
                return await CreateSession(login, password, accessCardId, clientId, Unauthorized,
                    sessionId => Ok(new JsonResult("{'Login successful'}")));
            }
            catch (ValidateUserException exception)
            {
                return Unauthorized(exception);
            }
        }

        [HttpGet]
        [Route("login/session/js")]
        public async Task<IActionResult> CreateSessionJs(string login, string password, string clientId, string accessCardId)
        {

            return await CreateSession(login, password, accessCardId, clientId, () => new JavaScriptResult($"alert('Problem ved login, session er ikke oprettet. Luk alle alle browservinduer og forsøg igen.');"), sessionId => new JavaScriptResult($"window.top.nxb_sessionId='{sessionId}';"));
        }

        public async Task<IActionResult> CreateSession(string login, string password, string accessCardId, string clientId, Func<IActionResult> onUnauthorized, Func<string, IActionResult> onOk)
        {
            // _telemetry.TrackTrace("LoginController.GetCredentials.CreateSession");
            if (login == null && accessCardId == null) throw new ArgumentNullException(nameof(login));
            if (password == null && accessCardId == null) throw new ArgumentNullException(nameof(password));
            if (onUnauthorized == null) throw new ArgumentNullException(nameof(onUnauthorized));
            if (onOk == null) throw new ArgumentNullException(nameof(onOk));

            var requestHost = this.HttpContext.Request.Host;
            var isIntegrationTesting = requestHost.Host == "localhost" && requestHost.Port == null;

            if (!isIntegrationTesting)    // hack
                await SignOutPrincipal();

            var sessionId = Guid.NewGuid().ToString();
            var doesSessionExists = _sessionStore.DoesIdExist(sessionId);
            var tenantDto = await _tenantClient.FindTenant(clientId);

            if (tenantDto == null)
            {
                _telemetry.TrackTrace($"LoginController.GetCredentials.CreateSession - login {login} for tenant with clientId/legacyId {clientId} was unauthorized" + login);
                return onUnauthorized();
            }

            if (!doesSessionExists)
            {
                try
                {
                    User user = null;
                    if (accessCardId == null)
                    {
                        user = _userRepository.FindFromLogin(tenantDto.Id, login);
                    }
                    else
                    {
                        user = _userRepository.FindFromAccessCardId(tenantDto.Id, accessCardId);
                    }
                    if (user == null)
                    {
                        //_telemetry.TrackTrace($"LoginController.VerifyCredentials Could not verify credentials for user [login={login}, clientid={clientId}]" + sessionId, SeverityLevel.Information);
                        return onUnauthorized();
                    }

                    if (!await this._userRepository.IsValid(user.Id))
                    {
                        return onUnauthorized();
                    }
                    _sessionStore.Save(sessionId, user);
                    if (!isIntegrationTesting)    // hack
                        await SignInPrincipal(sessionId, tenantDto.Id, user, tenantDto.ClientId, tenantDto.LegacyId);
                }
                catch (Exception exception)
                {
                    _telemetry.TrackTrace($"LoginController.VerifyCredentials Error verifying credentials for user [login={login}, clientid={clientId}]" + sessionId, SeverityLevel.Information);
                    _telemetry.TrackException(exception);
                    throw;
                }
            }
            else
            {
                await ValidateUser();
            }

            return onOk(sessionId);
        }

        [HttpDelete]
        [Route("login/session")]
        public async Task<IActionResult> RemoveSession()
        {
            var sessionId = GetSessionId();
            if (sessionId == null) return Ok(new JsonResult("{'Logout successful - session did not exist'}"));
            _sessionStore.Remove(sessionId);
            await SignOutPrincipal();
            return Ok(new JsonResult("{'Logout successful'}"));
        }
    }

    public class JavaScriptResult : ContentResult
    {
        public JavaScriptResult(string script)
        {
            this.Content = script;
            this.ContentType = "application/javascript";
        }
    }
}