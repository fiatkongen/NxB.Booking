using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Munk.Utils.Object
{
    public static class DictionaryExtensions
    {
        public static string DefaultName(this Dictionary<string, string> nameTranslations)
        {
            string languages = ",en,da,no";

            var arrLanguages = languages.Split(",").Distinct().ToArray();

            return TranslateWithFallback(nameTranslations, arrLanguages);
        }

        public static string TranslateWithFallback(this Dictionary<string, string> nameTranslations, string languages)
        {
            languages += ",en,da,no";

            var arrLanguages = languages.Split(",").Distinct().ToArray();

            return TranslateWithFallback(nameTranslations, arrLanguages);
        }

        public static string TranslateWithFallback(this Dictionary<string, string> nameTranslations, string[] languages)
        {
            foreach (var language in languages)
            {
                var translation = GetTranslation(nameTranslations, language);
                if (!string.IsNullOrWhiteSpace(translation))
                {
                    return translation;
                }
            }

            return "N/A";
        }

        private static string GetTranslation(Dictionary<string, string> nameTranslations, string languageId)
        {
            if (!languageId.Contains("s_", StringComparison.InvariantCultureIgnoreCase))
            {
                languageId = "s_" + languageId;
            }
            return nameTranslations.ContainsKey(languageId) && !string.IsNullOrWhiteSpace(nameTranslations[languageId]) ? nameTranslations[languageId] : null;
        }
    }
}
