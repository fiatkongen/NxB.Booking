using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NxB.Dto.DocumentApi;

namespace NxB.Clients.Interfaces
{
    public interface ITranslatorClient : IAuthorizeClient
    {
        Task<TranslationResultDto> TranslateText(TranslationRequestDto translationRequestDto);
        Task<TranslationResultDto> TryTranslateText(TranslationRequestDto translationRequestDto);
        Task<TranslationResultDto> TranslateHtml(TranslationRequestDto translationRequestDto);
    }
}
