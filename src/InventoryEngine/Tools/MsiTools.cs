using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InventoryEngine.Extensions;

namespace InventoryEngine.Tools
{
    internal static class MsiTools
    {
        private static readonly int[] GuidRegistryFormatPattern = { 8, 4, 4, 2, 2, 2, 2, 2, 2, 2, 2 };

        internal static Guid ConvertBetweenUpgradeAndProductCode(Guid from) => new Guid(from.ToString("N").Reverse(GuidRegistryFormatPattern));

        internal static IEnumerable<Guid> MsiEnumProducts()
        {
            var sbProductCode = new StringBuilder(39);
            var iIdx = 0;

            while (MsiWrapper.MsiEnumProducts(iIdx++, sbProductCode) == 0)
            {
                var guidString = sbProductCode.ToString();
                if (GuidTools.GuidTryParse(guidString, out var guid))
                {
                    yield return guid;
                }
                else
                {
                    Console.WriteLine($"Invalid MSI guid in MsiEnumProducts: {guidString}");
                }
            }
        }
    }
}