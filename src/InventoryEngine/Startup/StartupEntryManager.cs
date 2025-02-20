using System.IO;
using System.Linq;
using InventoryEngine.Tools;
using Microsoft.Win32;

namespace InventoryEngine.Startup
{
    public static class StartupEntryManager
    {
        // 6.2 is windows 8 and 2012, they are using a new startup disable scheme
        internal static IStartupDisable DisableFunctions => _disableFunctions ?? (_disableFunctions = new StartupDisable());

        private static IStartupDisable _disableFunctions;

        /// <summary>
        ///     Delete startup entry data from registry and file system. Only needed items are
        ///     removed, for example if entry is disabled the entry from "Run" key is not removed if
        ///     it exists, same for the "Startup" folder. To remove them change the Disabled
        ///     property and run this command again.
        /// </summary>
        /// <param name="startupEntry"> Entry to delete </param>
        public static void Delete(StartupEntry startupEntry)
        {
            if (startupEntry.Disabled)
            {
                DisableFunctions.Enable(startupEntry);
            }

            if (startupEntry.IsRegKey)
            {
                RegistryTools.RemoveRegistryValue(startupEntry.ParentLongName, startupEntry.EntryLongName);
            }
            else
            {
                File.Delete(startupEntry.FullLongName);
            }
        }

        /// <summary>
        ///     Disable startup entry to stop it from being processed at startup. It is stored in
        ///     the backup store.
        /// </summary>
        /// <param name="startupEntry"> </param>
        public static void Disable(StartupEntry startupEntry)
        {
            if (startupEntry.DisabledStore)
            {
                return;
            }

            DisableFunctions.Disable(startupEntry);
        }

        /// <summary>
        ///     Restore the entry from the backup store, so that it can be executed again.
        /// </summary>
        /// <param name="startupEntry"> </param>
        public static void Enable(StartupEntry startupEntry)
        {
            if (!startupEntry.DisabledStore)
            {
                return;
            }

            DisableFunctions.Enable(startupEntry);
        }

        /// <summary>
        ///     Set if this startup entry should run for all users or only for the current user.
        /// </summary>
        public static void SetAllUsers(StartupEntry startupEntry, bool allUsers)
        {
            // Find the suitable replacement
            var target = StartupEntryFactory.RunLocations.First(x => x.IsRegKey == startupEntry.IsRegKey
                                                                     && x.IsRunOnce == startupEntry.IsRunOnce &&
                                                                     x.AllUsers == allUsers && !x.IsWow);

            // Don't want to deal with the disable wizardry
            var wasDisabled = startupEntry.Disabled;
            if (wasDisabled)
            {
                Enable(startupEntry);
            }

            // Remove old entry or move the link to the new directory.
            if (startupEntry.IsRegKey)
            {
                try
                {
                    // Can't do this with links as they would get deleted
                    startupEntry.Delete();
                }
                catch
                {
                    // Key doesn't exist
                }
            }
            else
            {
                if (File.Exists(startupEntry.FullLongName))
                {
                    var newPath = Path.Combine(target.Path, startupEntry.EntryLongName);
                    File.Delete(newPath);
                    File.Move(startupEntry.FullLongName, newPath);
                }
            }

            // Plug in new data
            startupEntry.SetParentLongName(target.Path);
            startupEntry.AllUsersStore = allUsers;

            // Update registry stuff
            if (startupEntry.IsRegKey)
            {
                CreateRegValue(startupEntry);
            }

            // Restore disable status
            if (wasDisabled)
            {
                Disable(startupEntry);
            }
        }

        /// <summary>
        ///     Create a registry value for the specified entry. Works for drive links as well.
        /// </summary>
        /// <param name="startupEntry"> </param>
        internal static void CreateRegValue(StartupEntry startupEntry)
        {
            if (string.IsNullOrEmpty(startupEntry.Command))
            {
                return;
            }

            using (var runKey = RegistryTools.CreateSubKeyRecursively(startupEntry.ParentLongName))
            {
                runKey.SetValue(startupEntry.EntryLongName, startupEntry.Command, RegistryValueKind.String);
            }
        }
    }
}