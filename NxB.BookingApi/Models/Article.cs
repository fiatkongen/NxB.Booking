using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using Munk.AspNetCore;
using Newtonsoft.Json;
using NxB.Domain.Common.Dto;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class Article : BaseTranslatedEntity, ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public int Sort { get; set; }
        public Guid? DefaultPriceProfileId { get; set; }
        public Guid? DefaultOnlinePriceProfileId { get; set; }
        public long? DefaultLegacyPriceProfileId { get; set; }
        public long? DefaultLegacyOnlinePriceProfileId { get; set; }
        public int DepositType { get; set; }
        public decimal? Deposit { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsAvailableOnline { get; set; }
        public bool IsAvailablePOS { get; set; }
        public bool IsAvailableExtras { get; set; }
        public BookingAvailability KioskAvailability { get; set; }
        public bool AllowCustomText { get; set; }
        public string ImageUrl { get; set; }
        public string Caption { get; set; }
        public string IconName { get; set; }
        public string CssJson { get; set; }
        public string BarCode { get; set; }

        public string ArticleRentalCategoriesJson { get; set; }
        public List<ArticleRentalCategory> ArticleRentalCategories { get; private set; } = new();
        public bool ExcemptFromOnlineSearchTotal { get; set; }

        public decimal? Tax { get; set; }

        public Article(Guid id, Guid tenantId, string nameTranslationsJson = null, string descriptionTranslationsJson = null, string articleRentalCategoriesJson = null) : base(nameTranslationsJson, descriptionTranslationsJson)
        {
            Id = id;
            TenantId = tenantId;

            bool isRunByUnitTestHack = nameTranslationsJson != null && nameTranslationsJson.StartsWith(nameof(nameTranslationsJson));

            if (!isRunByUnitTestHack && nameTranslationsJson != null)
            {
                ArticleRentalCategoriesJson = articleRentalCategoriesJson;
                Deserialize();
            }
        }


        /* Use when EF can decide to use specific constructor
         // to be used by EF
        public Article(Guid id, Guid tenantId, long legacyId, string nameTranslationsJson, string articleRentalCategoriesJson) : this(id, tenantId, legacyId)
        {
            NameTranslationsJson = nameTranslationsJson;
            ArticleRentalCategoriesJson = articleRentalCategoriesJson;
            Deserialize();
        }

        //For now, only a constructor exists that demands a legacyId.
        public Article(Guid id, Guid tenantId, long legacyId)
        {
            Id = id;
            TenantId = tenantId;
            LegacyId = legacyId;
        }*/


        public override void Deserialize()
        {
            base.Deserialize();
            if (!string.IsNullOrEmpty(ArticleRentalCategoriesJson))
            {
                ArticleRentalCategories = JsonConvert.DeserializeObject<List<ArticleRentalCategory>>(ArticleRentalCategoriesJson);
            }
        }

        public override void Serialize()
        {
            base.Serialize();
            ArticleRentalCategoriesJson = JsonConvert.SerializeObject(ArticleRentalCategories);
        }


        public void AddRentalCategory(ArticleRentalCategory guestRentalCategory)
        {
            ArticleRentalCategories.Add(guestRentalCategory);
        }
    }
}
