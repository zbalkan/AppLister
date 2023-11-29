using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using InventoryEngine.Extensions;

namespace InventoryEngine.Tools
{
    internal static class MsiTools
    {
        private static readonly int[] GuidRegistryFormatPattern = { 8, 4, 4, 2, 2, 2, 2, 2, 2, 2, 2 };

        public static string MsiGetProductInfo(Guid productCode, MsiWrapper.INSTALLPROPERTY property)
        {
            var propertyLen = 512;
            var sbProperty = new StringBuilder(propertyLen);

            var code = MsiGetProductInfo(productCode.ToString("B"), property.PropertyName, sbProperty,
                ref propertyLen);

            //If code is 0 prevent returning junk
            return code != 0 ? null : sbProperty.ToString();
        }

        internal static Guid ConvertBetweenUpgradeAndProductCode(Guid from) => new Guid(from.ToString("N").Reverse(GuidRegistryFormatPattern));

        internal static IEnumerable<Guid> MsiEnumProducts()
        {
            var sbProductCode = new StringBuilder(39);
            var iIdx = 0;

            while (MsiEnumProducts(iIdx++, sbProductCode) == 0)
            {
                var guidString = sbProductCode.ToString();
                if (GuidTools.GuidTryParse(guidString, out var guid))
                {
                    yield return guid;
                }
                else
                {
                    Debug.WriteLine($"Invalid MSI guid in MsiEnumProducts: {guidString}");
                }
            }
        }

        [DllImport("msi.dll", SetLastError = true)]
        internal static extern int MsiEnumProducts(int iProductIndex, StringBuilder lpProductBuf);

        // Return product info
        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        private static extern int MsiGetProductInfo(string product, string property, [Out] StringBuilder valueBuf,
            ref int len);
    }
}