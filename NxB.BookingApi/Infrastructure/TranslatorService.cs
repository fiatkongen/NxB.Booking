using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Translation.V2;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using NxB.Clients.Interfaces;
using NxB.Dto.DocumentApi;
using System.Web;
using Munk.Utils.Object;
using NxB.Settings.Shared.Infrastructure;
using System.Net;

namespace NxB.BookingApi.Infrastructure
{
    public class TranslatorService: ITranslatorService
    {
        private readonly IArticleClientCached _articleClientCached;
        private readonly IRentalUnitClientCached _rentalUnitClientCached;
        private readonly IRentalCategoryClientCached _rentalCategoryClientCached;
        private readonly IGuestCategoryClientCached _guestCategoryClientCached;
        private readonly ISettingsRepository _settingsRepository;

        public TranslatorService(IArticleClientCached articleClientCached, IRentalUnitClientCached rentalUnitClientCached, IRentalCategoryClientCached rentalCategoryClientCached, IGuestCategoryClientCached guestCategoryClientCached, ISettingsRepository settingsRepository)
        {
            _articleClientCached = articleClientCached;
            _rentalUnitClientCached = rentalUnitClientCached;
            _rentalCategoryClientCached = rentalCategoryClientCached;
            _guestCategoryClientCached = guestCategoryClientCached;
            _settingsRepository = settingsRepository;
        }

        public async Task<TranslationResultDto> TranslateText(TranslationRequestDto translationRequestDto)
        {
            var client = CreateTranslationClient();
            TranslationResultDto translationResultDto = null;

            var sourceLanguage = await DetectSourceLanguage(translationRequestDto, client);
            if (sourceLanguage == translationRequestDto.TargetLanguage)
            {
                translationResultDto = new TranslationResultDto
                {
                    Language = sourceLanguage,
                    Text = translationRequestDto.Text
                };
                return translationResultDto;
            }

            var response = await client.TranslateTextAsync(
                text: translationRequestDto.Text,
                targetLanguage: translationRequestDto.TargetLanguage,
                sourceLanguage: sourceLanguage);

            translationResultDto = new TranslationResultDto
            {
                Language = response.TargetLanguage,
                Text = response.TranslatedText
            };

            return translationResultDto;
        }

        public async Task<TranslationResultDto> TranslateHtml(TranslationRequestDto translationRequestDto)
        {
            TranslationClient client = CreateTranslationClient();
            TranslationResultDto translationResultDto = null;

            var sourceLanguage = await DetectSourceLanguage(translationRequestDto, client);
            if (sourceLanguage == translationRequestDto.TargetLanguage)
            {
                translationResultDto = new TranslationResultDto
                {
                    Language = sourceLanguage,
                    Text = translationRequestDto.Text
                };
                return translationResultDto;
            }

            var response = await client.TranslateHtmlAsync(
                html: translationRequestDto.Text,
                targetLanguage: translationRequestDto.TargetLanguage,
                sourceLanguage: sourceLanguage);

            translationResultDto = new TranslationResultDto
            {
                Language = response.TargetLanguage,
                Text = response.TranslatedText
            };

            return translationResultDto;
        }

        public async Task<string> TryTranslateArticle(Guid resourceId, string[] languages)
        {
            try
            {
                var article = await _articleClientCached.FindSingleOrDefault(resourceId);
                if (article != null)
                {
                    foreach (var language in languages)
                    {
                        if (article.NameTranslations.ContainsKey("s_" + language))
                        {
                            return article.NameTranslations["s_" + language];
                        }
                    }
                }
            }
            catch
            {
                return "";
            }
            return "";
        }

        public async Task<string> TryTranslateGuestCategory(Guid resourceId, string[] languages)
        {
            try
            {
                var guestType = await _guestCategoryClientCached.FindSingleOrDefault(resourceId);
                if (guestType != null)
                {
                    foreach (var language in languages)
                    {
                        if (guestType.NameTranslations.ContainsKey("s_" + language))
                        {
                            return guestType.NameTranslations["s_" + language];
                        }
                    }
                }
            }
            catch
            {
                return "";
            }
            return "";
        }

        public async Task<string> TryTranslateRentalUnit(Guid resourceId, string[] languages)
        {
            try
            {
                var rentalUnit = await _rentalUnitClientCached.FindSingleOrDefault(resourceId);
                if (rentalUnit != null)
                {
                    return rentalUnit.NameTranslations.TranslateWithFallback(languages);
                }
            }
            catch
            {
                return "";
            }
            return "";
        }

        public async Task<string> TryTranslateRentalCategory(Guid resourceId, string[] languages)
        {
            try
            {

                var rentalCategoryDto = await _rentalCategoryClientCached.FindSingleOrDefault(resourceId);
                if (rentalCategoryDto != null)
                {
                    foreach (var language in languages)
                    {
                        if (rentalCategoryDto.NameTranslations.ContainsKey("s_" + language))
                        {
                            var name = rentalCategoryDto.NameTranslations["s_" + language];
                            return name;
                        }
                    }
                }
            }
            catch
            {
                return "";
            }
            return "";
        }

        public async Task<string> TryTranslateRentalUnitCategory(Guid rentalUnitResourceId, string[] languages)
        {
            try
            {
                var rentalUnit = await _rentalUnitClientCached.FindSingleOrDefault(rentalUnitResourceId);
                if (rentalUnit == null) return "";
                var rentalCategoryDto = await _rentalCategoryClientCached.FindSingleOrDefault(rentalUnit.RentalCategoryId);
                if (rentalCategoryDto != null)
                {
                    var name = rentalCategoryDto.NameTranslations.TranslateWithFallback(languages);
                    return name;
                }
            }
            catch
            {
                return "";
            }
            return "";
        }

        private TranslationClient CreateTranslationClient()
        {
            TranslationClient client = TranslationClient.CreateFromApiKey("AIzaSyC04eMk_L9DQyU1QvQFbXUUWeK84N4AYc8");
            return client;
        }

        private async Task<string> DetectSourceLanguage(TranslationRequestDto translationRequestDto, TranslationClient client)
        {
            if (!string.IsNullOrEmpty(translationRequestDto.SourceLanguage)) return translationRequestDto.SourceLanguage;

            var text = translationRequestDto.Text;
            if (translationRequestDto.DetectSourceLanguageFromFirstSentence)
            {
                var detectText = HtmlUtilities.ConvertToPlainText(translationRequestDto.Text);
                var noText = new string(detectText.TakeWhile(c => !char.IsLetter(c)).ToArray());
                detectText = detectText.Substring(noText.Length);
                var index = detectText.IndexOf('\n');
                while (index < 50)
                {
                    index = detectText.IndexOf('\n', index + 1);
                    if (index == -1) break;
                }
                if (index > -1)
                {
                    text = detectText.Substring(0, index);
                }
            }
            if (!translationRequestDto.DetectSourceLanguageFromFirstSentence && string.IsNullOrEmpty(translationRequestDto.DetectSourceLanguageText)) return null;

            var detection = await client.DetectLanguageAsync(text);
            return detection.Language;
        }

        public static string[] CleanUpLanguages(string[] languages, ISettingsRepository settingsRepository)
        {
            if (languages == null || languages.Length == 0)
            {
                throw new DocumentBuilderException("Fejl ved generering af tekst. Ingen sprog er valgt.");
            }

            var fallbackLanguages = new List<string> { "en", settingsRepository.GetApplicationLanguage() };
            return new List<string>(languages.Concat(fallbackLanguages)).Distinct().ToArray();
        }

        public string[] CleanUpLanguages(string[] languages)
        {
            return CleanUpLanguages(languages, _settingsRepository);
        }
    }
}
