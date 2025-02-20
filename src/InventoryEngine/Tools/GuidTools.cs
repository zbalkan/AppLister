using System;
using System.Text.RegularExpressions;

namespace InventoryEngine.Tools
{
    internal static class GuidTools
    {
        private const string GuidMatchPattern =
            "^[A-Fa-f0-9]{32}$|^({|\\()?[A-Fa-f0-9]{8}-([A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{12}(}|\\))?$|^({)?[0xA-Fa-f0-9]{3,10}(, {0,1}[0xA-Fa-f0-9]{3,6}){2}, {0,1}({)([0xA-Fa-f0-9]{3,4}, {0,1}){7}[0xA-Fa-f0-9]{3,4}(}})$";

        private static readonly Regex GuidMatchRegex = new Regex(GuidMatchPattern, RegexOptions.Compiled);

        /// <summary>
        ///     Try to parse the supplied string into a guid. Faster than catching exceptions.
        /// </summary>
        internal static bool GuidTryParse(string s, out Guid result)
        {
            result = Guid.Empty;
            if (string.IsNullOrEmpty(s) || !GuidMatchRegex.IsMatch(s))
            {
                return false;
            }
            result = new Guid(s);
            return true;
        }

        /// <summary>
        ///     Try to extract and parse a guid from the supplied string. result = Guid.Empty if
        ///     operation fails.
        /// </summary>
        internal static bool TryExtractGuid(string source, out Guid result)
        {
            result = Guid.Empty;
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }

            var braceIndex = source.IndexOfAny(new[] { '{', '(' });
            if (braceIndex < 0)
            {
                return GuidTryParse(source, out result);
            }

            var endingBraceIndex = source.IndexOfAny(new[] { '}', ')' });
            source = endingBraceIndex > braceIndex
                ? source.Substring(braceIndex, endingBraceIndex - braceIndex + 1)
                : source.Substring(braceIndex + 1);

            return GuidTryParse(source, out result);
        }
    }
}