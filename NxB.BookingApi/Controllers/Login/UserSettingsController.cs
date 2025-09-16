using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using NxB.BookingApi.Infrastructure;
using NxB.Settings.Shared.Infrastructure;

namespace NxB.BookingApi.Controllers.Login
{
    [Produces("application/json")]
    [Authorize]
    [Route("user/settings")]
    public class UserSettingsController
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly AppDbContext _appDbContext;

        public UserSettingsController(ISettingsRepository settingsRepository, AppDbContext appDbContext)
        {
            _settingsRepository = settingsRepository;
            _appDbContext = appDbContext;
        }

        [HttpPost]
        [Route("")]
        public ObjectResult SetSettings([FromBody]JObject jsonSettings, [FromQuery] string path = null)
        {
            this._settingsRepository.SetUserJsonSettings(jsonSettings, path);
            _appDbContext.SaveChanges();
            var settings = this._settingsRepository.GetUserJsonSettings(null);

            return new OkObjectResult(settings);
        }

        [HttpDelete]
        [Route("")]
        public ObjectResult DeleteSettings(string path)
        {
            this._settingsRepository.DeleteUserJsonSettings(path);
            _appDbContext.SaveChanges();
            var settings = this._settingsRepository.GetUserJsonSettings(null);

            return new OkObjectResult(settings);
        }

        [HttpGet]
        [Route("")]
        public ObjectResult FindSettings(string path = null)
        {
            var settings = this._settingsRepository.GetUserJsonSettings(path);
            return new OkObjectResult(settings);
        }
    }
}
