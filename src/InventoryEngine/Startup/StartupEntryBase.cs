using System.Diagnostics;
using System.IO;
using InventoryEngine.Extensions;
using InventoryEngine.Shared;
using InventoryEngine.Tools;

namespace InventoryEngine.Startup
{
    public abstract class StartupEntryBase
    {
        /// <summary>
        ///     Command executed by the startup entry
        /// </summary>
        public virtual string Command { get; protected set; }

        /// <summary>
        ///     Full path to the executable pointed by the startup entry
        /// </summary>
        public virtual string CommandFilePath { get; protected set; }

        /// <summary>
        ///     Company info extracted from the target executable (if possible)
        /// </summary>
        public virtual string Company { get; protected set; }

        /// <summary>
        ///     True if the entry is not processed during startup. It is stored in the backup reg
        ///     key and optionally backup directory if it's a link file.
        /// </summary>
        public abstract bool Disabled { get; set; }

        /// <summary>
        ///     Name of the called program extracted from the link or the target executable (if possible)
        /// </summary>
        public virtual string ProgramName { get; protected set; }

        /// <summary>
        ///     Program name without version info and such
        /// </summary>
        public virtual string ProgramNameTrimmed { get; protected set; }

        /// <summary>
        ///     Custom name of the parent location
        /// </summary>
        public virtual string ParentShortName { get; protected set; }

        /// <summary>
        ///     Full name of the parent location
        /// </summary>
        public virtual string ParentLongName { get; protected set; }

        /// <summary>
        ///     Full name of the entry
        /// </summary>
        public virtual string EntryLongName { get; protected set; }

        /// <summary>
        ///     Combined ParentLongName and EntryLongName
        /// </summary>
        public virtual string FullLongName => ParentLongName != null && EntryLongName != null
            ? PathTools.GenerousCombine(ParentLongName, EntryLongName)
            : null;

        /// <summary>
        ///     Delete this startup entry from the system
        /// </summary>
        public abstract void Delete();

        /// <summary>
        ///     Check if this entry still exists in the system
        /// </summary>
        public abstract bool StillExists();

        /// <summary>
        ///     $"{ProgramName} | {Company} | {Command}"
        /// </summary>
        public virtual string ToLongString() => $"{ProgramName} | {Company} | {Command}";

        /// <summary>
        ///     Returns FullLongName, unless it's empty. In that case returns ProgramName, or
        ///     Command if that is empty too.
        /// </summary>
        public override string ToString() => FullLongName ?? ProgramName ?? Command;

        protected static string ProcessCommandString(string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                return null;
            }

            return ProcessStartCommand.TryParse(command, out var temp) ? temp.FileName : null;
        }

        /// <summary>
        ///     Fill in fields with version information from specified file
        /// </summary>
        protected void FillInformationFromFile(string commandFilename)
        {
            if (!File.Exists(commandFilename))
            {
                return;
            }

            try
            {
                var info = FileVersionInfo.GetVersionInfo(commandFilename);
                Company = info.CompanyName;

                var fileDesc = info.FileDescription.StripStringFromVersionNumber();
                if (!string.IsNullOrEmpty(fileDesc))
                {
                    ProgramNameTrimmed = fileDesc;
                }
                else
                {
                    ProgramNameTrimmed = !string.IsNullOrEmpty(info.ProductName)
                        ? info.ProductName
                        : ProgramName.StripStringFromVersionNumber();
                }
            }
            catch
            {
                // Ignore file access errors errors
            }
        }
    }
}