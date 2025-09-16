using System;
using System.Collections.Generic;
using System.Text;
using Munk.Utils.Object;
using Newtonsoft.Json;

namespace NxB.Domain.Common.Model
{
    [Serializable]
    public class Country 
    {

        public string Id { get; set; }
        public string Text { get; set; }
        public string PhonePrefix { get; set; }
        public string ISO3166_2 { get; set; }
        public string ISO3166_3 { get; set; }
        public string ISO3166_Group { get; set; }
        public int? ISO3166_Num { get; set; }
        public string Language { get; set; }
        public bool IsHidden { get; set; }
        public string Flag { get; set; }


        [JsonIgnore]
        public string TextTranslationsJson { get; set; }
        [JsonIgnore]
        public NameTranslator TextTranslator { get; private set; } = new NameTranslator();
        public Dictionary<string, string> TextTranslations => TextTranslator.Translations;


        public Country() { }

        public virtual void Deserialize()
        {
            if (!string.IsNullOrEmpty(TextTranslationsJson))
            {
                TextTranslator = NameTranslator.DeSerializeFromJson(TextTranslationsJson);
            }
            else
            {
                TextTranslator = new NameTranslator();
            }
        }

        public virtual void Serialize()
        {
            TextTranslationsJson = TextTranslator.SerializeToJson();
        }
    }
}
