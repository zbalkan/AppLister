using System;
using System.Collections.Generic;
using System.Diagnostics;
using Engine.Extensions;
using Engine.Startup;
using Engine.Tools;
using Microsoft.Win32;

namespace Engine.Factory
{
    internal static class BrowserEntryFactory
    {
        internal const string AutorunsDisabledKeyName = "AutorunsDisabled";

        private const string ClsidPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID";

        private static readonly string[] RegistryStartupPoints =
                {
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Browser Helper Objects",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\explorer\Browser Helper Objects"
        };

        public static IEnumerable<BrowserHelperEntry> GetBrowserHelpers()
        {
            using var clsidKey = RegistryTools.OpenRegistryKey(ClsidPath);
            foreach (var registryStartupPoint in RegistryStartupPoints)
            {
                RegistryKey mainKey;

                try
                {
                    mainKey = RegistryTools.CreateSubKeyRecursively(registryStartupPoint);
                }
                catch (UnauthorizedAccessException e)
                {
                    Debug.WriteLine($"Failed to read reg key {registryStartupPoint}: " + e);
                    continue;
                }

                using (mainKey)
                {
                    foreach (var browserHelperEntry in
                             GatherBrowserHelpersFromKey(mainKey, clsidKey, registryStartupPoint, false))
                    {
                        yield return browserHelperEntry;
                    }

                    using (var disabledKey = mainKey.OpenSubKey(AutorunsDisabledKeyName))
                    {
                        if (disabledKey == null)
                        {
                            continue;
                        }

                        foreach (var browserHelperEntry in
                                 GatherBrowserHelpersFromKey(disabledKey, clsidKey, registryStartupPoint, true))
                        {
                            yield return browserHelperEntry;
                        }
                    }
                }
            }
        }

        private static IEnumerable<BrowserHelperEntry> GatherBrowserHelpersFromKey(RegistryKey workingKey,
            RegistryKey clsidKey,
            string registryStartupPoint, bool disabled)
        {
            foreach (var registryKey in workingKey.GetSubKeyNames())
            {
                using var classKey = clsidKey.OpenSubKey(registryKey);
                var name = classKey?.GetStringSafe(null);
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                string command;
                using (var runKey = classKey.OpenSubKey("InProcServer32") ?? classKey.OpenSubKey("InProcServer"))
                {
                    command = runKey?.GetStringSafe(null);
                }

                yield return new BrowserHelperEntry(name, command,
                    registryStartupPoint, registryKey, disabled, workingKey.Name.Contains("Wow6432Node"));
            }
        }
    }
}