using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Dto.DocumentApi
{
    public class TranslationRequestDto
    {
        public string Text { get; set; }
        public string SourceLanguage { get; set; }
        public string TargetLanguage { get; set; }
        public string DetectSourceLanguageText { get; set; }
        public bool DetectSourceLanguageFromFirstSentence { get; set; } = false;
    }

    public class TranslationResultDto
    {
        public string Text { get; set; }
        public string Language { get; set; }
    }
}
