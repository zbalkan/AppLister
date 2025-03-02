using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Engine.Extensions
{
    internal static class StringExtensions
    {
        internal static bool Contains(this string s, string value, StringComparison comparisonType) => s.IndexOf(value, comparisonType) >= 0;

        /// <summary>
        ///     Check if base string contains all the supplied strings.
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
        internal static bool ContainsAll(this string s, IEnumerable<string> items, StringComparison comparisonType) => items.All(item => s.Contains(item, comparisonType));

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
        internal static bool ContainsAny(this IEnumerable<char> s, IEnumerable<char> items) => items.Any(s.Contains);

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
        internal static bool ContainsAny(this string s, IEnumerable<string> items, StringComparison comparisonType) => items.Any(item => s.Contains(item, comparisonType));

        /// <summary>
        ///     Trim this string from all whitespaces and ending pronunciations (e.g., '.' ','),
        ///     then remove any of the supplied items from the end of the resulting string. This
        ///     method is greedy, it will remove the same item multiple times if possible. After
        ///     every successful removal whitespaces and ending pronunciations are trimmed again.
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
        internal static string ExtendedTrimEndAny(this string s, IEnumerable<string> trimmers,
            StringComparison comparisonType)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            var trimmerList = trimmers as IList<string> ?? trimmers.ToList();
            var resultStr = s.Trim().Trim(',', '.', ' ');
            var rerun = true;
            while (rerun)
            {
                rerun = false;
                foreach (var trimmer in trimmerList)
                {
                    if (!resultStr.EndsWith(trimmer, comparisonType))
                    {
                        continue;
                    }

                    var cutNum = resultStr.Length - trimmer.Length;

                    // Exit the loop quickly if resultStr contains only the trimmer. Also checks for
                    // negative length.
                    if (cutNum <= 0)
                    {
                        return string.Empty;
                    }

                    resultStr = resultStr.Substring(0, cutNum);
                    resultStr = resultStr.Trim().Trim(',', '.', ' ');
                    rerun = true;
                    break;
                }
            }
            return resultStr;
        }

        /// <summary>
        ///     Reverse the string using the specified pattern. The string is split into parts
        ///     corresponding to the pattern's values, then each of the parts is reversed, and
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
        internal static string Reverse(this string value, int[] pattern)
        {
            if (value == null)
            {
                throw new NullReferenceException();
            }

            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            if (value.Length != pattern.Sum())
            {
                throw new ArgumentException(
                    "Pattern doesn't match the string. Sum of the pattern's parts has to have length equal to the length the string.");
            }

            var returnString = new StringBuilder();

            var index = 0;

            // Iterate over the reversal pattern
            foreach (var length in pattern)
            {
                // Reverse the sub-string and append it
                returnString.Append(value.Substring(index, length).Reverse().ToArray());

                // Increment our position in the string
                index += length;
            }

            return returnString.ToString();
        }

        /// <summary>
        ///     Safe version of normalize that doesn't crash on invalid code points in string.
        ///     Instead, the points are replaced with question marks.
        /// </summary>
        internal static string SafeNormalize(this string input, NormalizationForm normalizationForm = NormalizationForm.FormC)
        {
            try
            {
                return ReplaceNonCharacters(input, '?').Normalize(normalizationForm);
            }
            catch (ArgumentException e)
            {
                throw new InvalidDataException("String contains invalid characters. Data: " + ConvertToHexString(Encoding.UTF32.GetBytes(input)), e);
            }
        }

        /// <summary>
        ///     Split string on newlines
        /// </summary>
        internal static string[] SplitNewlines(this string value, StringSplitOptions options) => value.Split(new[] { "\r\n", "\n" }, options);

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
        internal static bool StartsWithAny(this string s, IEnumerable<string> items, StringComparison comparisonType) => items.Any(item => s.StartsWith(item, comparisonType));

        /// <summary>
        ///     Strip version number from the end of a string. "MyApp 1.023.1" -&gt; "MyApp" If
        ///     string is null or empty, string.Empty is returned.
        /// </summary>
        /// <param name="input">
        /// </param>
        /// <returns>
        /// </returns>
        internal static string StripStringFromVersionNumber(this string input)
        {
            if (input == null)
            {
                return string.Empty;
            }

            int previousLen;
            do
            {
                previousLen = input.Length;

                input = input.Trim();

                if (input.Length == 0)
                {
                    return string.Empty;
                }

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
                {
                    input = input.Substring(0, input.Length - 2);
                }

                input = input.ExtendedTrimEndAny(new[] { "Application", "Helper", " v", " CE" },
                    StringComparison.InvariantCultureIgnoreCase);

                input = input.TrimEnd();
            } while (previousLen != input.Length);

            return input;
        }

        /// <summary>
        ///     Convert to - PascalCase.
        /// </summary>
        internal static string ToPascalCase(this string baseStr)
        {
            baseStr = baseStr?.Trim();
            if (string.IsNullOrEmpty(baseStr))
            {
                return string.Empty;
            }

            if (!baseStr.Contains(" "))
            {
                return baseStr;
            }

            baseStr = CultureInfo.GetCultureInfo("en-US").TextInfo.ToTitleCase(baseStr);
            return string.Concat(baseStr.Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        ///     Convert to - Title Case Capitalize the first character and add a space before each
        ///     capitalized letter (except the first character).
        /// </summary>
        internal static string ToTitleCase(this string baseStr)
        {
            if (string.IsNullOrEmpty(baseStr))
            {
                return string.Empty;
            }

            const string pattern = @"(?<=\w)(?=[A-Z])";
            baseStr = Regex.Replace(baseStr.ToPascalCase(), pattern, " ", RegexOptions.None);
            return baseStr.Substring(0, 1).ToUpperInvariant() + baseStr.Substring(1);
        }

        #region Private helpers

        private static string ConvertToHexString(this byte[] ba) => BitConverter.ToString(ba).Replace("-", "");

        private static bool IsValidCodePoint(int point) => point < 0xfdd0 || (point >= 0xfdf0 && (point & 0xffff) != 0xffff && (point & 0xfffe) != 0xfffe && point <= 0x10ffff);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Control flow", "TI6101:Do not change a loop variable inside a for loop block", Justification = "<Pending>")]
        private static string ReplaceNonCharacters(string input, char replacement)
        {
            var sb = new StringBuilder(input.Length);
            for (var i = 0; i < input.Length; i++)
            {
                if (char.IsSurrogatePair(input, i))
                {
                    var c = char.ConvertToUtf32(input, i);
                    i++;
                    if (IsValidCodePoint(c))
                    {
                        sb.Append(char.ConvertFromUtf32(c));
                    }
                    else
                    {
                        sb.Append(replacement);
                    }
                }
                else
                {
                    var c = input[i];
                    sb.Append(IsValidCodePoint(c) ? c : replacement);
                }
            }
            return sb.ToString();
        }

        #endregion Private helpers
    }
}