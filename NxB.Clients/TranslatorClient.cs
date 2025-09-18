using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Clients.Interfaces;
using NxB.Dto.DocumentApi;
using NxB.Dto.LogApi;
using ServiceStack;

namespace NxB.Clients
{
    public class TranslatorClient : NxBAdministratorClient, ITranslatorClient
    {
        public static string SERVICEURL = "/NxB.Services.App/NxB.DocumentApi";

        public TranslatorClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<TranslationResultDto> TranslateText(TranslationRequestDto translationRequestDto)
        {
            if (string.IsNullOrEmpty(translationRequestDto.Text)) return new TranslationResultDto { Language = translationRequestDto.TargetLanguage, Text = "" };
            var url = $"{SERVICEURL}/translator/text";
            var result = await this.PostAsync<TranslationResultDto>(url, translationRequestDto);
            return result;
        }

        public async Task<TranslationResultDto> TryTranslateText(TranslationRequestDto translationRequestDto)
        {
            try
            {
                return await this.TranslateText(translationRequestDto);
            }
            catch
            {
                return new TranslationResultDto
                {
                    Language = translationRequestDto.SourceLanguage,
                    Text = translationRequestDto.Text
                };
            }
        }

        public async Task<TranslationResultDto> TranslateHtml(TranslationRequestDto translationRequestDto)
        {
            if (string.IsNullOrEmpty(translationRequestDto.Text)) return new TranslationResultDto { Language = translationRequestDto.TargetLanguage, Text = "" };
            var url = $"{SERVICEURL}/translator/html";
            var result = await this.PostAsync<TranslationResultDto>(url, translationRequestDto);
            return result;
        }
    }
}
