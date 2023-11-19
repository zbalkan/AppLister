﻿using System.Collections;
using System.EnterpriseServices.Internal;
using System.Management;
using System.Management.Instrumentation;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: WmiConfiguration(@"root\Inventory", HostingModel = ManagementHostingModel.Decoupled)]

namespace WmiProvider
{
    [System.ComponentModel.RunInstaller(true)]
    public class WMIProviderInstaller : DefaultManagementInstaller
    {
        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            var asm = Assembly.GetExecutingAssembly();
            var RS = new RegistrationServices();

            if (RS.RegisterAssembly(asm, AssemblyRegistrationFlags.SetCodeBase))
            {
                try
                {
                    new Publish().GacInstall(Assembly.GetExecutingAssembly().Location);
                }
                catch { }
            }
        }

        public override void Uninstall(IDictionary savedState)
        {
            try
            {
                var MC = new ManagementClass(@"root\Inventory:Win32_Package");
                MC.Delete();
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