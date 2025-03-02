using System.IO;
using System.Management;
using Engine.Factory;
using Engine.Shared;

namespace Engine.Startup
{
    internal sealed class ServiceEntry : StartupEntryBase
    {
        public bool Disabled
        {
            get
            {
                try
                {
                    return ServiceEntryFactory.CheckServiceEnabled(ProgramName);
                }
                catch (ManagementException)
                {
                    return false;
                }
            }
            set { ServiceEntryFactory.EnableService(ProgramName, !value); }
        }

        public override string ParentLongName
        {
            get { return "Startup_ShortName_Service"; }
            protected set { }
        }

        public override string ParentShortName
        {
            get { return "Startup_ShortName_Service"; }
            protected set { }
        }

        public ServiceEntry(string serviceName, string displayName, string command)
        {
            ProgramName = serviceName;
            EntryLongName = displayName;

            Command = command;

            if (ProcessStartCommand.TryParse(command, out var pc))
            {
                CommandFilePath = pc.FileName;
            }
            else if (File.Exists(command))
            {
                CommandFilePath = command;
            }

            FillInformationFromFile(CommandFilePath);
        }

        public override bool StillExists()
        {
            try
            {
                ServiceEntryFactory.CheckServiceEnabled(ProgramName);
                return true;
            }
            catch (ManagementException)
            {
                return false;
            }
        }
    }
}