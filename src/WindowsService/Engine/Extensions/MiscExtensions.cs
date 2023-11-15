using System;

namespace WindowsService.Extensions
{
    public static class MiscExtensions
    {
        /// <summary>
        ///     Check if this struct is equal to the default value for this type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        public static bool IsDefault<T>(this T value)
            where T : struct
        {
            var isDefault = value.Equals(default(T));

            return isDefault;
        }

        public static bool IsEmpty(this Guid obj)
        {
            return Guid.Empty.Equals(obj);
        }

        public static bool IsZeroOrNull(this Version obj)
        {
            return obj == null || obj.Equals(new Version(0, 0, 0, 0))
                || obj.Equals(new Version(0, 0, 0)) || obj.Equals(new Version(0, 0));
        }
    }
}
