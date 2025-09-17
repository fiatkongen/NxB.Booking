using NxB.Dto.DocumentApi;

public interface ITranslatorService
{
    Task<TranslationResultDto> TranslateText(TranslationRequestDto translationRequestDto);
    Task<TranslationResultDto> TranslateHtml(TranslationRequestDto translationRequestDto);

    Task<string> TryTranslateArticle(Guid resourceId, string[] languages);
    Task<string> TryTranslateRentalUnit(Guid resourceId, string[] languages);
    Task<string> TryTranslateRentalCategory(Guid resourceId, string[] languages);
    Task<string> TryTranslateRentalUnitCategory(Guid rentalUnitResourceId, string[] languages);
    Task<string> TryTranslateGuestCategory(Guid resourceId, string[] languages);
    public string[] CleanUpLanguages(string[] languages);
}