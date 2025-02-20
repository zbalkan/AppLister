using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security;
using InventoryEngine.Extensions;
using InventoryEngine.Shared;
using InventoryEngine.Startup;
using InventoryEngine.Tools;

namespace InventoryEngine.Factory
{
    internal static partial class ServiceEntryFactory
    {
        public static bool CheckServiceEnabled(string serviceName)
        {
            var classInstance = GetServiceObject(serviceName);

            return GetEnabledState(classInstance);
        }

        public static void DeleteService(string serviceName)
        {
            try { EnableService(serviceName, false); }
            catch (ManagementException) { }

            var classInstance = GetServiceObject(serviceName);

            // Execute the method and obtain the return values.
            var outParams = classInstance.InvokeMethod("Delete", null, new InvokeMethodOptions { Timeout = TimeSpan.FromMinutes(1) });
            CheckReturnValue(outParams, 16); // 16 - Service Marked For Deletion
        }

        public static void EnableService(string serviceName, bool newState)
        {
            var classInstance = GetServiceObject(serviceName);

            // Obtain in-parameters for the method
            var inParams = classInstance.GetMethodParameters("ChangeStartMode");

            // Add the input parameters.
            inParams["StartMode"] = newState ? "Automatic" : "Disabled";

            // Execute the method and obtain the return values.
            var outParams = classInstance.InvokeMethod("ChangeStartMode", inParams, new InvokeMethodOptions { Timeout = TimeSpan.FromMinutes(1) });
            CheckReturnValue(outParams);
        }

        public static IEnumerable<ServiceEntry> GetServiceEntries()
        {
            var results = new List<ServiceEntry>();
            try
            {
                var searcher = new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_Service");

                foreach (var queryObj in searcher.Get())
                {
                    // Skip drivers and adapters
                    if (queryObj["ServiceType"] is string serviceType && serviceType.Contains("Process"))
                    {
                        // Don't show system services
                        if (queryObj["PathName"] is string filename && !filename.Contains(
                            WindowsTools.GetEnvironmentPath(Csidl.CSIDL_WINDOWS),
                            StringComparison.InvariantCultureIgnoreCase))
                        {
                            var e = new ServiceEntry((string)queryObj["Name"], queryObj["DisplayName"] as string, filename);
                            results.Add(e);
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is TypeInitializationException || ex is ManagementException || ex is ExternalException || ex is PlatformNotSupportedException)
            {
                Debug.WriteLine("Error while gathering services - " + ex);
            }

            return results.AsReadOnly();
        }

        private static void CheckReturnValue(ManagementBaseObject outParams, params uint[] ignoredCodes)
        {
            if (outParams == null)
            {
                return;
            }

            var exitCode = (uint)outParams["ReturnValue"];
            if (exitCode == 0 || ignoredCodes.Any(x => x == exitCode))
            {
                return;
            }

            if (exitCode == 2) // 2 - Access Denied
            {
                throw new SecurityException("The user does not have the necessary access.");
            }

            throw new ManagementException("Action failed with return value " + outParams["ReturnValue"] +
                ". Check return codes of Win32_Service class methods for more information.");
        }

        private static bool GetEnabledState(ManagementBaseObject queryObj) => queryObj["StartMode"] as string != nameof(StartMode.Auto);

        private static ManagementObject GetServiceObject(string serviceName) => new ManagementObject("root\\CIMV2",
                $"Win32_Service.Name='{serviceName}'", new ObjectGetOptions { Timeout = TimeSpan.FromMinutes(1) });
    }
}