using System;

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
                       string.Equals(DeviceName, other.DeviceName, StringComparison.InvariantCultureIgnoreCase) &&
                       DriverArchitecture.Equals(other.DriverArchitecture);
            }
        };
    }
}