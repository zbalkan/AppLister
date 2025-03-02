using System.IO;
using Engine.Extensions;
using Engine.Tools;

namespace Engine.Startup
{
    /// <summary>
    ///     Startup entries stored in Startup folders and Run/RunOnce registry keys
    /// </summary>
    public sealed class StartupEntry : StartupEntryBase
    {
        /// <summary>
        ///     True if this entry is executed during logon of all users, false if it is only for
        ///     the current user.
        /// </summary>
        public bool AllUsers
        {
            get { return AllUsersStore; }
            set
            {
                if (AllUsersStore != value)
                {
                    StartupEntryManager.SetAllUsers(this, value);
                }
            }
        }

        /// <summary>
        ///     True if the entry is not processed during startup. It is stored in the backup reg
        ///     key and optionally backup directory if it's a link file.
        /// </summary>
        public bool Disabled
        {
            get { return DisabledStore; }
            set
            {
                if (value == DisabledStore)
                {
                    return;
                }

                if (value)
                {
                    StartupEntryManager.Disable(this);
                }
                else
                {
                    StartupEntryManager.Enable(this);
                }

                DisabledStore = value;
            }
        }

        /// <summary>
        ///     Filename of the link (with extension), or name of the registry value.
        /// </summary>
        public override string EntryLongName { get; protected set; }

        /// <summary>
        ///     True if entry is a registry value, false if it's a link file
        /// </summary>
        public bool IsRegKey { get; internal set; }

        /// <summary>
        ///     True if the entry will be removed after running
        /// </summary>
        public bool IsRunOnce { get; internal set; }

        internal bool AllUsersStore;

        internal bool DisabledStore;

        internal StartupEntry(StartupPointData dataPoint, string fileName, string targetString)
        {
            AllUsersStore = dataPoint.AllUsers;
            IsRegKey = dataPoint.IsRegKey;
            IsRunOnce = dataPoint.IsRunOnce;
            EntryLongName = fileName;
            ParentShortName = dataPoint.Name;
            ParentLongName = dataPoint.Path?.TrimEnd('\\');

            Command = targetString ?? string.Empty;

            if (!string.IsNullOrEmpty(EntryLongName))
            {
                ProgramName = IsRegKey ? EntryLongName : Path.GetFileNameWithoutExtension(EntryLongName);
            }

            if (!string.IsNullOrEmpty(targetString))
            {
                CommandFilePath = ProcessCommandString(Command);

                if (CommandFilePath != null)
                {
                    FillInformationFromFile(CommandFilePath);
                }

                if (string.IsNullOrEmpty(ProgramName))
                {
                    ProgramName = ProgramNameTrimmed;
                }
            }

            if (string.IsNullOrEmpty(ProgramName))
            {
                ProgramName = targetString ?? dataPoint.Name ?? "Unknown";
            }

            if (string.IsNullOrEmpty(ProgramNameTrimmed))
            {
                ProgramNameTrimmed = ProgramName.StripStringFromVersionNumber();
            }
        }

        /// <summary>
        ///     Delete this startup entry from the system
        /// </summary>
        public void Delete() => StartupEntryManager.Delete(this);

        /// <summary>
        ///     Check if the startup entry still exists in registry or on disk. If the entry is
        ///     disabled, but it exists in the backup store, this method will return true.
        /// </summary>
        public override bool StillExists()
        {
            try
            {
                if (Disabled)
                {
                    return StartupEntryManager.DisableFunctions.StillExists(this);
                }

                if (!IsRegKey)
                {
                    return File.Exists(FullLongName);
                }

                using var key = RegistryTools.OpenRegistryKey(ParentLongName);
                return !string.IsNullOrEmpty(key.GetStringSafe(EntryLongName));
            }
            catch
            {
                return false;
            }
        }

        //TODO temporary hack
        internal void SetParentLongName(string newValue) => ParentLongName = newValue;
    }
}