using System;

namespace WindowsService.Extensions
{
    internal static class ByteExtensions
    {
        public static string ToHexString(this byte[] ba)
        {
            var hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }
    }
}