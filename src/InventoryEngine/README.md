# InventoryEngine

This library is a subset of [Bulk Crap Uninstaller](https://github.com/Klocman/Bulk-Crap-Uninstaller). The classes are extracted one by one until the application scan and list functionality is fulfilled.

The actual code is written by [Marcin Szeniak](https://github.com/Klocman). The related pieces of code are copied as is yet it is expected to differentiate in time.


## License
```
    Copyright (c) 2017 Marcin Szeniak (https://github.com/Klocman/)
    Apache License Version 2.0
```

You can find the original license in this project's folder.

## Changes

| Class in InventoryEngine | Original | Remarks |
|--------------------------|----------|---------|
| InventoryEngine.ApplicationEntryTools | XXX.ApplicationEntryTools | |
| InventoryEngine.ApplicationUninstallerEntry | XXX.ApplicationUninstallerEntry | |
| InventoryEngine.ApplicationUninstallerFactoryCache | XXX.ApplicationUninstallerFactoryCache | |
| InventoryEngine.BrowserHelperEntry | XXX.BrowserHelperEntry | |
| InventoryEngine.CompiledPropertyInfo | XXX.CompiledPropertyInfo | |
| InventoryEngine.CSIDL | XXX.CSIDL | |
| InventoryEngine.Inventory | XXX.Inventory | |
| InventoryEngine.MachineType | XXX.MachineType | |
| InventoryEngine.ProcessStartCommand | XXX.ProcessStartCommand | |
| InventoryEngine.ReflectionTools | XXX.ReflectionTools | |
| InventoryEngine.ServiceEntry | XXX.ServiceEntry | |
| InventoryEngine.Sift4 | XXX.Sift4 | |
| InventoryEngine.ThreadedWorkSpreader | XXX.ThreadedWorkSpreader | |
| InventoryEngine.UninstallerType | XXX.UninstallerType | |
| InventoryEngine.UninstallToolsGlobalConfig | XXX.UninstallToolsGlobalConfig | |
| InventoryEngine.Extensions.ByteExtensions | XXX.Extensions.ByteExtensions | |
| InventoryEngine.Extensions.CollectionExtensions | XXX.Extensions.CollectionExtensions | |
| InventoryEngine.Extensions.EnumerableExtensions | XXX.Extensions.EnumerableExtensions | |
| InventoryEngine.Extensions.MiscExtensions | XXX.Extensions.MiscExtensions | |
| InventoryEngine.Extensions.ParentProcessUtilities | XXX.Extensions.ParentProcessUtilities | |
| InventoryEngine.Extensions.ProcessExtensions | XXX.Extensions.ProcessExtensions | |
| InventoryEngine.Extensions.RegistryKeyExtensions | XXX.Extensions.RegistryKeyExtensions | |
| InventoryEngine.Extensions.StringExtensions | XXX.Extensions.StringExtensions | |
| InventoryEngine.Factory.ApplicationUninstallerFactory | XXX.Factory.ApplicationUninstallerFactory | |
| InventoryEngine.Factory.BrowserEntryFactory | XXX.Factory.BrowserEntryFactory | |
| InventoryEngine.Factory.ConcurrentApplicationFactory | XXX.Factory.ConcurrentApplicationFactory | |
| InventoryEngine.Factory.DirectoryFactory | XXX.Factory.DirectoryFactory | |
| InventoryEngine.Factory.FactoryThreadedHelpers | XXX.Factory.FactoryThreadedHelpers | |
| InventoryEngine.Factory.FactoryTools | XXX.Factory.FactoryTools | |
| InventoryEngine.Factory.IUninstallerFactory | XXX.Factory.IUninstallerFactory | |
| InventoryEngine.Factory.RegistryFactory | XXX.Factory.RegistryFactory | |
| InventoryEngine.Factory.ServiceEntryFactory | XXX.Factory.ServiceEntryFactory | |
| InventoryEngine.InfoAdders.AppExecutablesSearcher | XXX.InfoAdders.AppExecutablesSearcher | |
| InventoryEngine.InfoAdders.CertificateGetter | XXX.InfoAdders.CertificateGetter | |
| InventoryEngine.InfoAdders.ExecutableAttributeExtractor | XXX.InfoAdders.ExecutableAttributeExtractor | |
| InventoryEngine.InfoAdders.IMissingInfoAdder | XXX.InfoAdders.IMissingInfoAdder | |
| InventoryEngine.InfoAdders.InfoAdderManager | XXX.InfoAdders.InfoAdderManager | |
| InventoryEngine.InfoAdders.InfoAdderPriority | XXX.InfoAdders.InfoAdderPriority | |
| InventoryEngine.InfoAdders.UninstallerTypeAdder | XXX.InfoAdders.UninstallerTypeAdder | |
| InventoryEngine.Junk.IJunkCreator | XXX.Junk.IJunkCreator | |
| InventoryEngine.Junk.JunkManager | XXX.Junk.JunkManager | |
| InventoryEngine.Junk.ProgramFilesOrphans | XXX.Junk.ProgramFilesOrphans | |
| InventoryEngine.Junk.Confidence.ConfidenceCollection | XXX.Junk.Confidence.ConfidenceCollection | |
| InventoryEngine.Junk.Confidence.ConfidenceGenerators | XXX.Junk.Confidence.ConfidenceGenerators | |
| InventoryEngine.Junk.Confidence.ConfidenceLevel | XXX.Junk.Confidence.ConfidenceLevel | |
| InventoryEngine.Junk.Confidence.ConfidenceRecord | XXX.Junk.Confidence.ConfidenceRecord | |
| InventoryEngine.Junk.Confidence.ConfidenceRecords | XXX.Junk.Confidence.ConfidenceRecords | |
| InventoryEngine.Junk.Containers.FileSystemJunk | XXX.Junk.Containers.FileSystemJunk | |
| InventoryEngine.Junk.Containers.IJunkResult | XXX.Junk.Containers.IJunkResult | |
| InventoryEngine.Junk.Containers.JunkResultBase | XXX.Junk.Containers.JunkResultBase | |
| InventoryEngine.Junk.Containers.RegistryKeyJunk | XXX.Junk.Containers.RegistryKeyJunk | |
| InventoryEngine.Junk.Containers.RegistryValueJunk | XXX.Junk.Containers.RegistryValueJunk | |
| InventoryEngine.Junk.Containers.RunProcessJunk | XXX.Junk.Containers.RunProcessJunk | |
| InventoryEngine.Junk.Containers.StartupJunkNode | XXX.Junk.Containers.StartupJunkNode | |
| InventoryEngine.Junk.Finders.JunkCreatorBase | XXX.Junk.Finders.JunkCreatorBase | |
| InventoryEngine.Junk.Finders.Drive.CommonDriveJunkScanner | XXX.Junk.Finders.Drive.CommonDriveJunkScanner | |
| InventoryEngine.Junk.Finders.Drive.InstallLocationScanner | XXX.Junk.Finders.Drive.InstallLocationScanner | |
| InventoryEngine.Junk.Finders.Drive.PrefetchScanner | XXX.Junk.Finders.Drive.PrefetchScanner | |
| InventoryEngine.Junk.Finders.Drive.SpecificUninstallerKindScanner | XXX.Junk.Finders.Drive.SpecificUninstallerKindScanner | |
| InventoryEngine.Junk.Finders.Drive.UninstallerLocationScanner | XXX.Junk.Finders.Drive.UninstallerLocationScanner | |
| InventoryEngine.Junk.Finders.Drive.WerScanner | XXX.Junk.Finders.Drive.WerScanner | |
| InventoryEngine.Junk.Finders.Misc.ShortcutJunk | XXX.Junk.Finders.Misc.ShortcutJunk | |
| InventoryEngine.Junk.Finders.Misc.StartupJunk | XXX.Junk.Finders.Misc.StartupJunk | |
| InventoryEngine.Junk.Finders.Registry.AppCompatFlagScanner | XXX.Junk.Finders.Registry.AppCompatFlagScanner | |
| InventoryEngine.Junk.Finders.Registry.AudioPolicyConfigScanner | XXX.Junk.Finders.Registry.AudioPolicyConfigScanner | |
| InventoryEngine.Junk.Finders.Registry.ComScanner | XXX.Junk.Finders.Registry.ComScanner | |
| InventoryEngine.Junk.Finders.Registry.DebugTracingScanner | XXX.Junk.Finders.Registry.DebugTracingScanner | |
| InventoryEngine.Junk.Finders.Registry.EventLogScanner | XXX.Junk.Finders.Registry.EventLogScanner | |
| InventoryEngine.Junk.Finders.Registry.FirewallRuleScanner | XXX.Junk.Finders.Registry.FirewallRuleScanner | |
| InventoryEngine.Junk.Finders.Registry.HeapLeakDetectionScanner | XXX.Junk.Finders.Registry.HeapLeakDetectionScanner | |
| InventoryEngine.Junk.Finders.Registry.InstallerFoldersScanner | XXX.Junk.Finders.Registry.InstallerFoldersScanner | |
| InventoryEngine.Junk.Finders.Registry.RegisteredApplicationsFinder | XXX.Junk.Finders.Registry.RegisteredApplicationsFinder | |
| InventoryEngine.Junk.Finders.Registry.SoftwareRegKeyScanner | XXX.Junk.Finders.Registry.SoftwareRegKeyScanner | |
| InventoryEngine.Junk.Finders.Registry.TracingScanner | XXX.Junk.Finders.Registry.TracingScanner | |
| InventoryEngine.Junk.Finders.Registry.UninstallerKeySearcher | XXX.Junk.Finders.Registry.UninstallerKeySearcher | |
| InventoryEngine.Junk.Finders.Registry.UserAssistScanner | XXX.Junk.Finders.Registry.UserAssistScanner | |
| InventoryEngine.Properties.AssemblyInfo | XXX.Properties.AssemblyInfo | |
| InventoryEngine.Startup.IStartupDisable | XXX.Startup.IStartupDisable | |
| InventoryEngine.Startup.NewStartupDisable | XXX.Startup.NewStartupDisable | |
| InventoryEngine.Startup.OldStartupDisable | XXX.Startup.OldStartupDisable | |
| InventoryEngine.Startup.StartupEntry | XXX.Startup.StartupEntry | |
| InventoryEngine.Startup.StartupEntryBase | XXX.Startup.StartupEntryBase | |
| InventoryEngine.Startup.StartupEntryFactory | XXX.Startup.StartupEntryFactory | |
| InventoryEngine.Startup.StartupEntryManager | XXX.Startup.StartupEntryManager | |
| InventoryEngine.Startup.StartupManager | XXX.Startup.StartupManager | |
| InventoryEngine.Startup.StartupPointData | XXX.Startup.StartupPointData | |
| InventoryEngine.Startup.TaskEntry | XXX.Startup.TaskEntry | |
| InventoryEngine.Startup.TaskEntryFactory | XXX.Startup.TaskEntryFactory | |
| InventoryEngine.Tools.GuidTools | XXX.Tools.GuidTools | |
| InventoryEngine.Tools.MsiTools | XXX.Tools.MsiTools | |
| InventoryEngine.Tools.MsiWrapper | XXX.Tools.MsiWrapper | |
| InventoryEngine.Tools.PathTools | XXX.Tools.PathTools | |
| InventoryEngine.Tools.ProcessTools | XXX.Tools.ProcessTools | |
| InventoryEngine.Tools.RegistryTools | XXX.Tools.RegistryTools | |
| InventoryEngine.Tools.WindowsTools | XXX.Tools.WindowsTools | |