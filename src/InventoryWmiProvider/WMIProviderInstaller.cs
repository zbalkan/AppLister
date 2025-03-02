﻿using System.Collections;
using System.EnterpriseServices.Internal;
using System.Management;
using System.Management.Instrumentation;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: WmiConfiguration(@"root\ZB", HostingModel = ManagementHostingModel.Decoupled)]

namespace InventoryWmiProvider
{
    [System.ComponentModel.RunInstaller(true)]
    public class WmiProviderInstaller : DefaultManagementInstaller
    {
        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            var asm = Assembly.GetExecutingAssembly();
            var rs = new RegistrationServices();

            if (!rs.RegisterAssembly(asm, AssemblyRegistrationFlags.SetCodeBase))
            {
                return;
            }

            try
            {
                new Publish().GacInstall(Assembly.GetExecutingAssembly().Location);
            }
            catch { }
        }

        public override void Uninstall(IDictionary savedState)
        {
            try
            {
                var mc = new ManagementClass(@"root\ZB:ZB_App");
                mc.Delete();
            }
            catch { }

            try
            {
                base.Uninstall(savedState);
            }
            catch { }

            try
            {
                new Publish().GacRemove(Assembly.GetExecutingAssembly().Location);
            }
            catch { }
        }
    }
}