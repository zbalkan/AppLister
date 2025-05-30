using System;
using System.Collections.Generic;
using System.Globalization;

namespace Engine.Factory
{
    internal partial class DriverFactory
    {
        /// <summary>
        ///     Data fields retrieved from Driver store for each driver
        /// </summary>
        public class DriverStoreEntry : IEquatable<DriverStoreEntry>
        {
            /// <summary>
            ///     Associated device Id (device instance path).
            /// </summary>
            public string DeviceId { get; set; }

            /// <summary>
            ///     Associated device name.
            /// </summary>
            public string DeviceName { get; set; }

            /// <summary>
            ///     Whether the associated device present in the system or not.
            /// </summary>
            public bool? DevicePresent { get; set; }

            public NativeDriverStore.ProcessorArchitecture DriverArchitecture { get; set; }

            /// <summary>
            ///     Driver class (ex., "System Devices")
            /// </summary>
            public string DriverClass { get; set; }

            /// <summary>
            ///     Sys file date
            /// </summary>
            public DateTime DriverDate { get; set; }

            /// <summary>
            ///     Driver Original INF Name
            /// </summary>
            public string DriverInfPath { get; set; }

            /// <summary>
            ///     Driver package provider
            /// </summary>
            public string DriverPkgProvider { get; set; }

            /// <summary>
            ///     Name of the OEM INF in driver store
            /// </summary>
            public string DriverPublishedName { get; set; }

            /// <summary>
            ///     Signer name. Empty if not WHQLd.
            /// </summary>
            public string DriverSignerName { get; set; }

            /// <summary>
            ///     Sys file version
            /// </summary>
            public Version DriverVersion { get; set; }

            public int? OemId
            {
                get
                {
                    var oemInfName = DriverPublishedName;

                    if (!string.IsNullOrEmpty(oemInfName))
                    {
                        if (oemInfName.StartsWith("oem", StringComparison.OrdinalIgnoreCase))
                        {
                            oemInfName = oemInfName.Substring(3);
                        }

                        if (oemInfName.EndsWith(".inf", StringComparison.OrdinalIgnoreCase))
                        {
                            oemInfName = oemInfName.Substring(0, oemInfName.Length - 4);
                        }

                        if (int.TryParse(oemInfName, out var id))
                        {
                            return id;
                        }
                    }

                    return null;
                }
            }

            private static readonly Dictionary<long, string> SizeRangeToName = new Dictionary<long, string>
        {
            { 10 * 1024, "0 - 10 KB" },
            { 100 * 1024, "10 - 100 KB" },
            { 1024 * 1024, "100 KB - 1 MB" },
            { 16 * 1024 * 1024, "1 - 16 MB" },
            { 128 * 1024 * 1024, "16 - 128 MB" },
            { long.MaxValue, "> 128 MB" },
        };

            // Returns the human-readable file size for an arbitrary, 64-bit file size The default
            // format is "0.### XB", e.g. "4 KB" or "1.4 GB"
            public static string GetBytesReadable(long i)
            {
                // Get absolute value
                var absolute_i = (i < 0 ? -i : i);

                // Determine the format of the readable value
                string format;
                double readable;

                if (absolute_i >= 0x40000000) // Gigabyte
                {
                    format = "0.0 \\GB";
                    readable = (i >> 20);
                }
                else if (absolute_i >= 0x100000) // Megabyte
                {
                    format = "0 \\MB";
                    readable = (i >> 10);
                }
                else if (absolute_i >= 0x400) // Kilobyte
                {
                    format = "0 \\KB";
                    readable = i;
                }
                else
                {
                    return "1 KB";
                }

                // Divide by 1024 to get fractional value
                readable /= 1024;

                // Return formatted number with suffix
                return readable.ToString(format);
            }

            public static string[] GetFieldNames()
            {
                return new[] {
                "OEM INF",
                "INF",
                "Package Provider",
                "Driver Class",
                "Driver Date",
                "Driver Version",
                "Driver Signer",
                "Driver Size",
                "Driver Folder",
                "Device Id",
                "Device Name",
                "Device Present",
            };
            }

            public static long GetSizeRange(long size)
            {
                foreach (var item in SizeRangeToName)
                {
                    if (size < item.Key)
                    {
                        return item.Key;
                    }
                }

                return -1;
            }

            public static string GetSizeRangeName(long size)
            {
                if (SizeRangeToName.TryGetValue(size, out var name))
                {
                    return name;
                }
                else
                {
                    return string.Empty;
                }
            }

            public string[] GetFieldValues()
            {
                return new[]
                {
                DriverPublishedName ?? string.Empty,
                DriverInfPath ?? string.Empty,
                DriverPkgProvider ?? string.Empty,
                DriverClass ?? string.Empty,
                DriverDate.ToString("d"),
                DriverVersion?.ToString() ?? string.Empty,
                DriverSignerName ?? string.Empty,
                DeviceId ?? string.Empty,
                DeviceName ?? string.Empty,
                DevicePresent?.ToString() ?? string.Empty,
            };
            }

            public void SetDriverDateAndVersion(string value)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var dateAndVersion = value.Trim().Split(new char[] { ' ' }, 2);
                    if (dateAndVersion.Length == 2)
                    {
                        DriverDate = default;
                        DriverVersion = null;

                        if (DateTime.TryParse(dateAndVersion[0].Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var driverDate))
                        {
                            DriverDate = driverDate;
                        }

                        if (Version.TryParse(dateAndVersion[1].Trim(), out var driverVersion))
                        {
                            DriverVersion = driverVersion;
                        }
                    }
                }
            }

            public override string ToString()
            {
                return $"PublishedName: {DriverPublishedName}, InfName: {DriverInfPath}, Class: {DriverClass}, Version: {DriverVersion}, DeviceName: {DeviceName}";
            }

            bool IEquatable<DriverStoreEntry>.Equals(DriverStoreEntry other)
            {
                return other != null &&
                       string.Equals(DriverPublishedName, other.DriverPublishedName, StringComparison.InvariantCultureIgnoreCase) &&
                       string.Equals(DriverInfPath, other.DriverInfPath, StringComparison.InvariantCultureIgnoreCase) &&
                       string.Equals(DriverPkgProvider, other.DriverPkgProvider, StringComparison.InvariantCultureIgnoreCase) &&
                       string.Equals(DriverClass, other.DriverClass, StringComparison.InvariantCultureIgnoreCase) &&
                       DriverDate.Equals(other.DriverDate) &&
                       DriverVersion.Equals(other.DriverVersion) &&
                       string.Equals(DriverSignerName, other.DriverSignerName, StringComparison.InvariantCultureIgnoreCase) &&
                       string.Equals(DeviceId, other.DeviceId, StringComparison.InvariantCultureIgnoreCase) &&
                       string.Equals(DeviceName, other.DeviceName, StringComparison.InvariantCultureIgnoreCase) &&
                       DevicePresent.Equals(other.DevicePresent) &&
                       DriverArchitecture.Equals(other.DriverArchitecture);
            }
        };
    }
}