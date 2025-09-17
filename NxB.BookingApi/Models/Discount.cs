using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using Newtonsoft.Json;
using NxB.BookingApi.Infrastructure;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class Discount : ITenantEntity, IEntitySaved
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string Name { get; set; }
        public decimal? DiscountPercent { get; set; }
        public bool AllowMultiple { get; set; }
        public bool AllowPercentOverride { get; set; }
        public bool IsHidden { get; set; }
        public bool IsDeleted { get; set; }

        public DiscountGroupSelection RentalCategoriesSelection
        {
            get => this.IdSelections[nameof(RentalCategoriesSelection)];
            set
            {
                var key = nameof(RentalCategoriesSelection);
                this.IdSelections.Remove(key);
                this.IdSelections.Add(key, value ?? new DiscountGroupSelection());
            }
        }
        public DiscountGroupSelection GuestCategoriesSelection
        {
            get => this.IdSelections[nameof(GuestCategoriesSelection)];
            set
            {
                var key = nameof(GuestCategoriesSelection);
                this.IdSelections.Remove(key);
                this.IdSelections.Add(key, value ?? new DiscountGroupSelection());
            }
        }
        public DiscountGroupSelection ArticleCategoriesSelection
        {
            get => this.IdSelections[nameof(ArticleCategoriesSelection)];
            set
            {
                var key = nameof(ArticleCategoriesSelection);
                this.IdSelections.Remove(key);
                this.IdSelections.Add(key, value ?? new DiscountGroupSelection());
            }
        }
        public DiscountGroupSelection CustomerGroupsSelection
        {
            get => this.IdSelections[nameof(CustomerGroupsSelection)];
            set
            {
                var key = nameof(CustomerGroupsSelection);
                this.IdSelections.Remove(key);
                this.IdSelections.Add(key, value ?? new DiscountGroupSelection());
            }
        }
        private Dictionary<string, DiscountGroupSelection> IdSelections { get; set; }
        public string IdSelectionsJson { get; set; }

        public Discount(string idSelectionsJson = null)
        {
            if (idSelectionsJson != null)
            {
                this.IdSelectionsJson = idSelectionsJson;
                DeserializeFromJson();
            }
            else
            {
                InitializeWithDefaultValues();
            }
        }

        private void InitializeWithDefaultValues()
        {
            this.IdSelections = new Dictionary<string, DiscountGroupSelection>();
            this.IdSelections.Add(nameof(RentalCategoriesSelection), new DiscountGroupSelection());
            this.IdSelections.Add(nameof(GuestCategoriesSelection), new DiscountGroupSelection());
            this.IdSelections.Add(nameof(ArticleCategoriesSelection), new DiscountGroupSelection());
            this.IdSelections.Add(nameof(CustomerGroupsSelection), new DiscountGroupSelection());
        }

        private void SerializeToJson()
        {
            if (IdSelectionsContainsDefaultValues())
            {
                this.IdSelectionsJson = null;
            }
            else
            {
                this.IdSelectionsJson = JsonConvert.SerializeObject(this.IdSelections);
            }
        }

        private bool IdSelectionsContainsDefaultValues()
        {
            return IdSelectionContainsDefaultValues(this.RentalCategoriesSelection) &&
                   IdSelectionContainsDefaultValues(this.GuestCategoriesSelection) &&
                   IdSelectionContainsDefaultValues(this.ArticleCategoriesSelection) &&
                   IdSelectionContainsDefaultValues(this.CustomerGroupsSelection);
        }

        private bool IdSelectionContainsDefaultValues(DiscountGroupSelection discountGroupSelection)
        {
            return (discountGroupSelection == null  || (discountGroupSelection.FilterType == 1 && discountGroupSelection.Ids.Count == 0));
        }

        private void DeserializeFromJson()
        {
            this.IdSelections = JsonConvert.DeserializeObject<Dictionary<string, DiscountGroupSelection>>(this.IdSelectionsJson);
        }

        public void OnEntitySaved(EntityState entityState)
        {
            SerializeToJson();
        }
    }
}
