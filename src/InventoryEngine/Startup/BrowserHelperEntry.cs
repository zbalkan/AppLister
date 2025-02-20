using System.IO;
using System.Linq;
using InventoryEngine.Factory;
using InventoryEngine.Tools;

namespace InventoryEngine.Startup
{
    internal sealed class BrowserHelperEntry : StartupEntryBase
    {
        public bool Disabled
        {
            get { return _disabled; }
            set
            {
                if (_disabled == value)
                {
                    return;
                }

                using (var key = RegistryTools.OpenRegistryKey(GetRealParentPath(), true))
                using (var newKey = RegistryTools.CreateSubKeyRecursively(GetRealParentPath(true)))
                {
                    key.MoveSubKey(EntryLongName, newKey, EntryLongName);
                }

                _disabled = value;
            }
        }

        public override string ParentShortName
        {
            get { return "Startup_Shortname_BrowserHelper" + (IsWow ? " (Wow)" : string.Empty); }
            protected set { }
        }

        private bool IsWow { get; }
        private bool _disabled;

        public BrowserHelperEntry(string programName, string command, string parentKeyPath,
            string className, bool disabled, bool isWow)
        {
            ProgramName = programName;
            Command = command;

            ParentLongName = parentKeyPath;
            EntryLongName = className;
            _disabled = disabled;
            IsWow = isWow;

            CommandFilePath = ProcessCommandString(Command);

            if (CommandFilePath != null)
            {
                FillInformationFromFile(CommandFilePath);
            }
        }

        public void Delete()
        {
            using var key = RegistryTools.OpenRegistryKey(GetRealParentPath(), true);
            key?.DeleteSubKey(EntryLongName);
        }

        public string GetRealParentPath(bool opposite = false) => (opposite ? !_disabled : _disabled)
                ? Path.Combine(ParentLongName, BrowserEntryFactory.AutorunsDisabledKeyName)
                : ParentLongName;

        public override bool StillExists()
        {
            using var key = RegistryTools.OpenRegistryKey(GetRealParentPath());
            return key?.GetSubKeyNames().Contains(EntryLongName) == true;
        }
    }
}