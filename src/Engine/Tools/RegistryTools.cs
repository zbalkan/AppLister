using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace Engine.Tools
{
    internal static class RegistryTools
    {
        private const string HkccRootName = "HKEY_CURRENT_CONFIG";

        private const string HkccShortRootName = "HKCC";

        private const string HkcrRootName = "HKEY_CLASSES_ROOT";

        private const string HkcrShortRootName = "HKCR";

        private const string HkcuRootName = "HKEY_CURRENT_USER";

        private const string HkcuShortRootName = "HKCU";

        private const string HklmRootName = "HKEY_LOCAL_MACHINE";

        private const string HklmShortRootName = "HKLM";

        private const string HkuRootName = "HKEY_USERS";

        private const string HkuShortRootName = "HKUS";

        private const string HkuShortRootName2 = "HKU";

        /// <summary>
        ///     Return registry key at supplied path. If the key or its parents don't exist, create
        ///     them before returning. The returned RegistryKey is writable.
        /// </summary>
        /// <param name="fullPath">
        ///     Path of the key to open or create. Not case-sensitive.
        /// </param>
        internal static RegistryKey CreateSubKeyRecursively(string fullPath)
        {
            if (fullPath == null)
            {
                throw new ArgumentNullException(nameof(fullPath));
            }

            if (fullPath.Length < 4)
            {
                throw new ArgumentException("Path is too short/invalid");
            }

            var previousKey = GetRootHive(fullPath);

            var parts = StripKeyRoot(fullPath).Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < parts.Length; i++)
            {
                var newKey = previousKey.CreateSubKey(parts[i]);

                // Don't try to close the root key
                if (i > 0)
                {
                    previousKey.Close();
                }

                previousKey = newKey;
            }

            return previousKey;
        }

        /// <summary>
        ///     Move subkey under a new parent.
        /// </summary>
        internal static void MoveSubKey(this RegistryKey parentKey,
            string subKeyName, RegistryKey newParentKey, string newSubKeyName)
        {
            CopySubKey(parentKey, subKeyName, newParentKey, newSubKeyName);
            parentKey.DeleteSubKeyTree(subKeyName);
        }

        /// <summary>
        ///     Open registry key using its fully qualified path. The key is opened read-only. Root
        ///     key can be named by either its long or short name. (long: "HKEY_LOCAL_MACHINE",
        ///     short: "HKLM")
        /// </summary>
        /// <param name="fullPath">
        ///     Full path of the requested registry key
        /// </param>
        internal static RegistryKey OpenRegistryKey(string fullPath) => OpenRegistryKey(fullPath, false);

        /// <summary>
        ///     Open registry key using its fully qualified path. Root key can be named by either
        ///     its long or short name. (long: "HKEY_LOCAL_MACHINE", short: "HKLM")
        /// </summary>
        /// <param name="fullPath">
        ///     Full path of the requested registry key
        /// </param>
        /// <param name="writable">
        ///     If false, key is opened read-only
        /// </param>
        /// <param name="ignoreAccessExceptions">
        ///     If true, return null instead of throwing an exception if the key is inaccessible
        /// </param>
        internal static RegistryKey OpenRegistryKey(string fullPath, bool writable, bool ignoreAccessExceptions = false)
        {
            if (!ignoreAccessExceptions)
            {
                return OpenRegistryKey(fullPath, writable);
            }

            try
            {
                return OpenRegistryKey(fullPath, writable);
            }
            catch (Exception ex)
            {
                if (!(ex is UnauthorizedAccessException) && !(ex is System.Security.SecurityException) &&
                    !(ex is IOException))
                {
                    throw;
                }

                Debug.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        ///     Check if entry has not been uninstalled already (check registry key)
        /// </summary>
        /// <returns>
        ///     True if key exists
        /// </returns>
        internal static bool RegKeyStillExists(string registryPath)
        {
            if (string.IsNullOrEmpty(registryPath))
            {
                return false;
            }

            try
            {
                using var key = OpenRegistryKey(registryPath);
                return key != null;
            }
            catch
            {
                return false;
            }
        }

        internal static void RemoveRegistryValue(string fullRegistryPath, string valueName)
        {
            if (string.IsNullOrEmpty(fullRegistryPath))
            {
                throw new ArgumentException("RegistryTools_RemoveRegistryKey_PathEmptyNull",
                    nameof(fullRegistryPath));
            }

            if (string.IsNullOrEmpty(valueName))
            {
                throw new ArgumentException("RegistryTools_RemoveRegistryKey_RemoveDefault",
                    nameof(valueName));
            }

            if (fullRegistryPath.Count(x => x.Equals('\\')) < 2)
            {
                throw new ArgumentException("RegistryTools_RemoveRegistryKey_PointsAtRoot",
                    nameof(fullRegistryPath));
            }

            using var key = OpenRegistryKey(fullRegistryPath, true);
            key?.DeleteValue(valueName);
        }

        internal static string StripKeyRoot(string fullPath)
        {
            if (fullPath == null)
            {
                throw new ArgumentNullException(nameof(fullPath));
            }

            if (fullPath.Length < 4)
            {
                throw new ArgumentException("Path is too short/invalid");
            }

            var firstSplitter = fullPath.IndexOf('\\');
            if (firstSplitter < 0)
            {
                firstSplitter = fullPath.Length;
            }

            return firstSplitter >= fullPath.Length - 1
                ? string.Empty
                : fullPath.Substring(firstSplitter + 1);
        }

        /// <summary>
        ///     Copy subkey to a different parent key.
        /// </summary>
        private static void CopySubKey(this RegistryKey parentKey,
            string subKeyName, RegistryKey newParentKey, string newSubKeyName)
        {
            using var destinationKey = newParentKey.CreateSubKey(newSubKeyName);
            using var sourceKey = parentKey.OpenSubKey(subKeyName, true);
            RecurseCopyKey(sourceKey, destinationKey);
        }

        private static string GetKeyRoot(string fullPath, bool shortStyle)
        {
            if (fullPath == null)
            {
                throw new ArgumentNullException(nameof(fullPath));
            }

            if (fullPath.Length < 3)
            {
                throw new ArgumentException("Path is too short/invalid");
            }

            var firstSplitter = fullPath.IndexOf('\\');
            if (firstSplitter < 0)
            {
                firstSplitter = fullPath.Length;
            }

            switch (fullPath.Substring(0, firstSplitter).ToUpperInvariant())
            {
                case HklmRootName:
                case HklmShortRootName:
                    return shortStyle ? HklmShortRootName : HklmRootName;

                case HkcrRootName:
                case HkcrShortRootName:
                    return shortStyle ? HkcrShortRootName : HkcrRootName;

                case HkcuRootName:
                case HkcuShortRootName:
                    return shortStyle ? HkcuShortRootName : HkcuRootName;

                case HkuRootName:
                case HkuShortRootName:
                case HkuShortRootName2:
                    return shortStyle ? HkuShortRootName : HkuRootName;

                case HkccRootName:
                case HkccShortRootName:
                    return shortStyle ? HkccShortRootName : HkccRootName;

                default:
                    throw new ArgumentException("Path root is invalid or missing");
            }
        }

        private static RegistryKey GetRootHive(string fullPath)
        {
            switch (GetKeyRoot(fullPath, true))
            {
                case HklmShortRootName:
                    return Registry.LocalMachine;

                case HkcrShortRootName:
                    return Registry.ClassesRoot;

                case HkcuShortRootName:
                    return Registry.CurrentUser;

                case HkuShortRootName:
                case HkuShortRootName2:
                    return Registry.Users;

                case HkccShortRootName:
                    return Registry.CurrentConfig;

                default:
                    throw new ArgumentException("Path root is invalid or missing");
            }
        }

        /// <summary>
        ///     Open registry key using its fully qualified path. Root key can be named by either
        ///     its long or short name. (long: "HKEY_LOCAL_MACHINE", short: "HKLM")
        /// </summary>
        /// <param name="fullPath">
        ///     Full path of the requested registry key
        /// </param>
        /// <param name="writable">
        ///     If false, key is opened read-only
        /// </param>
        private static RegistryKey OpenRegistryKey(string fullPath, bool writable)
        {
            if (fullPath == null)
            {
                throw new ArgumentNullException(nameof(fullPath));
            }

            if (fullPath.Length < 4)
            {
                throw new ArgumentException("Path is too short/invalid");
            }

            var rootKey = GetRootHive(fullPath);

            return rootKey.OpenSubKey(StripKeyRoot(fullPath), writable);
        }

        private static void RecurseCopyKey(RegistryKey sourceKey, RegistryKey destinationKey)
        {
            foreach (var valueName in sourceKey.GetValueNames())
            {
                var valueData = sourceKey.GetValue(valueName);
                var valueKind = sourceKey.GetValueKind(valueName);
                destinationKey.SetValue(valueName, valueData, valueKind);
            }

            foreach (var sourceSubKeyName in sourceKey.GetSubKeyNames())
            {
                using var destSubKey = destinationKey.CreateSubKey(sourceSubKeyName);
                using var sourceSubKey = sourceKey.OpenSubKey(sourceSubKeyName, true);
                RecurseCopyKey(sourceSubKey, destSubKey);
            }
        }
    }
}