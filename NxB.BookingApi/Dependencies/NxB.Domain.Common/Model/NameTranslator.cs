using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NxB.Domain.Common.Model
{
    [Serializable]
    public class NameTranslator
    {
        [JsonProperty]//do not rename to _translations
        private Dictionary<string, string> translations = new Dictionary<string, string>();

        public Dictionary<string, string> Translations => translations;

        public void AddTranslation(string id, string languageIso, string translation)
        {
            var translationKey = BuildTranslationKey(id, languageIso);
            if (!translations.ContainsKey(translationKey))
            {
                translations.Add(BuildTranslationKey(id, languageIso), translation);
            }
            else
            {
                translations[translationKey] = translation;
            }
        }

        private static string BuildTranslationKey(string id, string languageIso)
        {
            return id + "_" + languageIso;
        }

        public string GetTranslation(string id, string languageIso)
        {
            var translationKey = BuildTranslationKey(id, languageIso);
            if (!translations.ContainsKey(translationKey)) return "";

            var translation = translations[translationKey];
            return translation;
        }

        public string SerializeToJson()
        {
            string json = JsonConvert.SerializeObject(this.translations);
            return json;
        }

        public static NameTranslator DeSerializeFromJson(string json)
        {
            var nameTranslator = new NameTranslator();
            if (!string.IsNullOrEmpty(json))
            {
                nameTranslator.translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            return nameTranslator;
        }
    }
}