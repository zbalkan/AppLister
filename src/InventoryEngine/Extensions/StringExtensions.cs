using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace InventoryEngine.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        ///     Check if base string starts with any of the supplied strings.
        /// </summary>
        /// <param name="s">
        /// </param>
        /// <param name="items">
        ///     Items to be compared to the base string
        /// </param>
        /// <param name="comparisonType">
        ///     Rules of the comparison
        /// </param>
        /// <returns>
        ///     True if any of the items were found in the base string, else false
        /// </returns>
        public static bool StartsWithAny(this string s, IEnumerable<string> items, StringComparison comparisonType)
        {
            return items.Any(item => s.StartsWith(item, comparisonType));
        }

        /// <summary>
        ///     Reverse the string using the specified pattern. The string is split into parts
        ///     corresponding to the pattern's values, then each of the parts is reversed and
        ///     finally they are joined back.
        ///     Example: String("Tester") Pattern(1,3,2) -&gt; T est er -&gt; T tse re -&gt; Result("Ttsere")
        /// </summary>
        /// <param name="value">
        ///     String to reverse
        /// </param>
        /// <param name="pattern">
        ///     Pattern used to reverse the string.
        ///     Warning: The pattern has to have identical total length to the length of the string.
        /// </param>
        public static string Reverse(this string value, int[] pattern)
        {
            if (value == null)
                throw new NullReferenceException();
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));
            if (value.Length != pattern.Sum())
                throw new ArgumentException(
                    "Pattern doesn't match the string. Sum of the pattern's parts has to have length equal to the length the string.");

            var returnString = new StringBuilder();

            var index = 0;

            // Iterate over the reversal pattern
            foreach (var length in pattern)
            {
                // Reverse the sub-string and append it
                returnString.Append(value.Substring(index, length).Reverse().ToArray());

                // Increment our posistion in the string
                index += length;
            }

            return returnString.ToString();
        }

        /// <summary>
        ///     Strip version number from the end of a string. "MyApp 1.023.1" -&gt; "MyApp" If
        ///     string is null or empty, string.Empty is returned.
        /// </summary>
        /// <param name="input">
        /// </param>
        /// <returns>
        /// </returns>
        public static string StripStringFromVersionNumber(this string input)
        {
            if (input == null)
                return string.Empty;

            int previousLen;
            do
            {
                previousLen = input.Length;

                input = input.Trim();

                if (input.Length == 0)
                    return string.Empty;

                if (input.EndsWith(")", StringComparison.Ordinal))
                {
                    var bracketLocation = input.LastIndexOf('(');
                    if (bracketLocation > 4)
                    {
                        input = input.Substring(0, bracketLocation).TrimEnd();
                    }
                }

                input = input.ExtendedTrimEndAny(
                    new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", ".", ",", "-", "_" },
                    StringComparison.InvariantCultureIgnoreCase).TrimEnd();

                if (input.EndsWith(" v", StringComparison.InvariantCultureIgnoreCase))
                    input = input.Substring(0, input.Length - 2);

                input = input.ExtendedTrimEndAny(new[] { "Application", "Helper", " v", " CE" },
                    StringComparison.InvariantCultureIgnoreCase);

                input = input.TrimEnd();
            } while (previousLen != input.Length);

            return input;
        }

        public static ReadOnlySpan<char> AsSpan(this string s)
        {
            return s.ToCharArray().AsSpan();
        }

        public static bool Contains(this string s, string value, StringComparison comparisonType)
        {
            return s.IndexOf(value, comparisonType) >= 0;
        }

        /// <summary>
        ///     Check if base char array contains any of the supplied chars.
        /// </summary>
        /// <param name="s">
        /// </param>
        /// <param name="items">
        ///     Chars to be compared to the base array
        /// </param>
        /// <returns>
        ///     True if any of the items were found in the base string, else false
        /// </returns>
        public static bool ContainsAny(this IEnumerable<char> s, IEnumerable<char> items)
        {
            return items.Any(s.Contains);
        }

        /// <summary>
        ///     Check if base string contains any of the supplied strings.
        /// </summary>
        /// <param name="s">
        /// </param>
        /// <param name="items">
        ///     Items to be compared to the base string
        /// </param>
        /// <param name="comparisonType">
        ///     Rules of the comparison
        /// </param>
        /// <returns>
        ///     True if any of the items were found in the base string, else false
        /// </returns>
        public static bool ContainsAny(this string s, IEnumerable<string> items, StringComparison comparisonType)
        {
            return items.Any(item => s.Contains(item, comparisonType));
        }

        /// <summary>
        ///     Check if base string contains all of the supplied strings.
        /// </summary>
        /// <param name="s">
        /// </param>
        /// <param name="items">
        ///     Items to be compared to the base string
        /// </param>
        /// <param name="comparisonType">
        ///     Rules of the comparison
        /// </param>
        /// <returns>
        ///     True if any of the items were found in the base string, else false
        /// </returns>
        public static bool ContainsAll(this string s, IEnumerable<string> items, StringComparison comparisonType)
        {
            return items.All(item => s.Contains(item, comparisonType));
        }

        /// <summary>
        ///     Trim this string from all whitespaces and ending pronounciations (eg. '.' ','), then
        ///     remove any of the supplied items from the end of the resulting string. This method
        ///     is greedy, it will remove the same item multiple times if possible. After every
        ///     successful removal whitespaces and ending pronounciations are trimmed again.
        /// </summary>
        /// <param name="s">
        /// </param>
        /// <param name="trimmers">
        ///     Items to be trimmed from base string
        /// </param>
        /// <param name="comparisonType">
        ///     How the items are compared to the base string
        /// </param>
        /// <returns>
        ///     Trimmed version of the base string
        /// </returns>
        public static string ExtendedTrimEndAny(this string s, IEnumerable<string> trimmers,
            StringComparison comparisonType)
        {
            if (String.IsNullOrEmpty(s))
                return String.Empty;

            var trimmerList = trimmers as IList<string> ?? trimmers.ToList();
            var resultStr = s.Trim().Trim(',', '.', ' ');
            var rerun = true;
            while (rerun)
            {
                rerun = false;
                foreach (var trimmer in trimmerList)
                {
                    if (!resultStr.EndsWith(trimmer, comparisonType)) continue;

                    var cutNum = resultStr.Length - trimmer.Length;

                    // Exit the loop quickly if resultStr contains only the trimmer. Also checks for
                    // negative lenght.
                    if (cutNum <= 0)
                        return String.Empty;

                    resultStr = resultStr.Substring(0, cutNum);
                    resultStr = resultStr.Trim().Trim(',', '.', ' ');
                    rerun = true;
                    break;
                }
            }
            return resultStr;
        }

        /// <summary>
        ///     Safe version of normalize that doesn't crash on invalid code points in string.
        ///     Instead the points are replaced with question marks.
        /// </summary>
        public static string SafeNormalize(this string input, NormalizationForm normalizationForm = NormalizationForm.FormC)
        {
            try
            {
                return ReplaceNonCharacters(input, '?').Normalize(normalizationForm);
            }
            catch (ArgumentException e)
            {
                throw new InvalidDataException("String contains invalid characters. Data: " + Encoding.UTF32.GetBytes(input).ToHexString(), e);
            }
        }

        #region Private helpers

        private static string ReplaceNonCharacters(string input, char replacement)
        {
            var sb = new StringBuilder(input.Length);
            for (var i = 0; i < input.Length; i++)
            {
                if (char.IsSurrogatePair(input, i))
                {
                    int c = char.ConvertToUtf32(input, i);
                    i++;
                    if (IsValidCodePoint(c))
                        sb.Append(char.ConvertFromUtf32(c));
                    else
                        sb.Append(replacement);
                }
                else
                {
                    char c = input[i];
                    if (IsValidCodePoint(c))
                        sb.Append(c);
                    else
                        sb.Append(replacement);
                }
            }
            return sb.ToString();
        }

        private static bool IsValidCodePoint(int point)
        {
            return point < 0xfdd0 || point >= 0xfdf0 && (point & 0xffff) != 0xffff && (point & 0xfffe) != 0xfffe && point <= 0x10ffff;
        }

        #endregion Private helpers
    }
}