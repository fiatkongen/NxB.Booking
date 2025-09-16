using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Controllers.Accounting
{
    [Produces("application/json")]
    [Route("country")]
    [Authorize]
    [ApiValidationFilter]
    public class CountryController : BaseController
    {
        private readonly ICountryRepository _countryRepository;
        private readonly AppDbContext _appDbContext;

        public CountryController(ICountryRepository countryRepository, AppDbContext appDbContext)
        {
            _countryRepository = countryRepository;
            _appDbContext = appDbContext;
        }

        [HttpGet]
        [Route("")]
        public async Task<ObjectResult> FindCountry(string countryId)
        {
            if (countryId == "aq") countryId = "dk";
            var countries = await this._countryRepository.FindSingle(countryId);
            return new ObjectResult(countries);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("list/all")]
        public async Task<ObjectResult> FindCountries(bool includeHidden = true)
        {
            var countries = await this._countryRepository.FindAll(includeHidden);
            return new ObjectResult(countries);
        }

        [HttpPut]
        [Route("ishidden")]
        public async Task<ObjectResult> ModifyIsHidden(string countryId, bool isHidden)
        {
            var country = await this._countryRepository.FindSingle(countryId);
            country.IsHidden = isHidden;
            await _appDbContext.SaveChangesAsync();
            return new ObjectResult(country);
        }

        [HttpPut]
        [Route("language")]
        public async Task<ObjectResult> ModifyLanguage(string countryId, string language)
        {
            var country = await this._countryRepository.FindSingle(countryId);
            country.Language = string.IsNullOrWhiteSpace(language) ? null : language.Replace(" ", "");
            await _appDbContext.SaveChangesAsync();
            return new ObjectResult(country);
        }

        [HttpPut]
        [Route("flag")]
        public async Task<ObjectResult> ModifyFlag(string countryId, string flag)
        {
            var country = await this._countryRepository.FindSingle(countryId);
            country.Flag = string.IsNullOrWhiteSpace(flag) ? null : flag;
            await _appDbContext.SaveChangesAsync();
            return new ObjectResult(country);
        }

        //[HttpPost]
        //[AllowAnonymous]
        //[Route("translate")]
        //public async Task<IActionResult> Translate()
        //{
        //    var countries = await this._countryRepository.FindAll(true);

        //    TranslationClient client = TranslationClient.CreateFromApiKey("AIzaSyC04eMk_L9DQyU1QvQFbXUUWeK84N4AYc8");

        //    var languages = new List<string> { "en", "sv", "no", "nl", "de" };
        //    foreach (var country in countries)
        //    {
        //        country.TextTranslator.AddTranslation("s", "da", country.Text);
        //    }
        //    foreach (var language in languages)
        //    {
        //        foreach (var country in countries)
        //        {
        //            var response = await client.TranslateTextAsync(
        //                text: country.Text,
        //                targetLanguage: language,
        //                sourceLanguage: "da");
        //            country.TextTranslator.AddTranslation("s", language, response.TranslatedText);
        //            _countryRepository.Update(country);
        //        }
        //    }
        //    await _appDbContext.SaveChangesAsync();
        //    return Ok();
        //}
    }
}
