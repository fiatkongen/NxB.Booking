using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NxB.Domain.Common.Interfaces;

namespace NxB.Domain.Common.Model
{
    public class BaseTranslatedEntity : IEntitySaved
    {
        public string NameTranslationsJson { get; set; }    //just include for now
        public string DescriptionTranslationsJson { get; set; }    //just include for now
        public NameTranslator NameTranslator { get; private set; } = new NameTranslator();
        public NameTranslator DescriptionTranslator { get; private set; } = new NameTranslator();
        public Dictionary<string, string> NameTranslations => NameTranslator.Translations;
        public Dictionary<string, string> DescriptionTranslations => DescriptionTranslator.Translations;
        
        public BaseTranslatedEntity(string nameTranslationsJson = null, string descriptionTranslationsJson = null)
        {
            bool isRunByUnitTestHack = nameTranslationsJson != null && nameTranslationsJson.StartsWith(nameof(nameTranslationsJson));

            if (!isRunByUnitTestHack && nameTranslationsJson != null)
            {
                NameTranslationsJson = nameTranslationsJson;
                DescriptionTranslationsJson = descriptionTranslationsJson;
                Deserialize();
            }
        }

        public void AddDefaultNameTranslation(string name)
        {
            this.NameTranslator.AddTranslation("s", "da", name);
        }

        public void AddNameTranslation(string id, string isoLanguage, string name)
        {
            this.NameTranslator.AddTranslation(id, isoLanguage, name);
        }

        public virtual void Deserialize()
        {
            if (!string.IsNullOrEmpty(NameTranslationsJson))
            {
                NameTranslator = NameTranslator.DeSerializeFromJson(NameTranslationsJson);
            }
            else
            {
                NameTranslator = new NameTranslator();
            }

            if (!string.IsNullOrEmpty(DescriptionTranslationsJson))
            {
                DescriptionTranslator = NameTranslator.DeSerializeFromJson(DescriptionTranslationsJson);
            }
            else
            {
                DescriptionTranslator = new NameTranslator();
            }
        }

        public virtual void Serialize()
        {
            NameTranslationsJson = NameTranslator.SerializeToJson();
            DescriptionTranslationsJson = DescriptionTranslator.SerializeToJson();
        }

        public virtual void OnEntitySaved(EntityState entityState)
        {
            Serialize();
        }
    }
}
