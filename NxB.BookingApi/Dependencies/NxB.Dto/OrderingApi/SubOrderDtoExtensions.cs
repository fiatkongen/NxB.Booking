using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Munk.Utils.Object;
using NxB.Dto.Clients;

namespace NxB.Dto.OrderingApi
{
    public static class SubOrderDtoExtensions
    {
        public static string GetAllocationUnitsCombinedText(this SubOrderDto subOrderDto)
        {
            var validAllocationOrderLineTexts = subOrderDto.AllocationOrderLines.Where(x => !x.IsEqualized).Select(x => x.Text.Replace("  ", " ")).Distinct().ToList();

            if (!validAllocationOrderLineTexts.Any()) { return ""; }
            return string.Join(", ", validAllocationOrderLineTexts);
        }

        public static string GetAllocationUnitsCombinedHtml(this SubOrderDto subOrderDto)
        {
            return HttpUtility.HtmlEncode(GetAllocationUnitsCombinedText(subOrderDto));
        }


        public static async Task<string> GetAllocationTypesCombinedText(this SubOrderDto subOrderDto, IRentalCategoryClient rentalCategoryClient, IRentalUnitClient rentalUnitClient, string[] languages)
        {
            var validAllocationOrderLineRentalUnitIds = GetDistinctAllocationOrderLineRentalUnitIds(subOrderDto);

            if (!validAllocationOrderLineRentalUnitIds.Any()) { return ""; }
            var rentalCategoryNames = new List<string>();

            foreach (var categoryId in validAllocationOrderLineRentalUnitIds)
            {
                var rentalUnit = await rentalUnitClient.FindSingleOrDefault(categoryId);
                if (rentalUnit == null) continue;
                var rentalCategory = await rentalCategoryClient.FindSingleOrDefault(rentalUnit.RentalCategoryId);

                rentalCategoryNames.Add(rentalCategory.NameTranslations.TranslateWithFallback(languages));
            }
            var groupedResources = rentalCategoryNames.GroupBy(x => x);
            var resourcesString = string.Join(',', groupedResources.Select(x => x.Count() + " x " + x.Key));
            return resourcesString;
        }

        public static List<Guid> GetDistinctAllocationOrderLineRentalUnitIds(this SubOrderDto subOrderDto)
        {
            return subOrderDto.AllocationOrderLines.Where(x => !x.IsEqualized).Select(x => x.ResourceId).Distinct().ToList();
        }

        public static async Task<string> GetAllocationTypesCombinedHtml(this SubOrderDto subOrderDto, IRentalCategoryClient rentalCategoryClient, IRentalUnitClient rentalUnitClient, string[] languages)
        {
            return HttpUtility.HtmlEncode(await GetAllocationTypesCombinedText(subOrderDto, rentalCategoryClient, rentalUnitClient, languages));
        }
    }
}
