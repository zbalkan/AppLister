# InventoryService

A Windows service and a WMI provider to allow anyone to have a working inventory implementation.

*Beware that this project is in alpha stage. Breaking changes will occur.*

## Rationale

The `Win32_Product` has always been [problematic](https://gregramsey.net/2012/02/20/win32_product-is-evil/), especially when it comes to WMI filters. The alternative is to use file or registry paths in your queries, which may or may not work for your use case. For local scenarios, PowerShell can save you but not for remote use cases or WMI filtering for Group Policies.

Also, the developer sometimes invent new ways of doing some tasks, which may not fit what Windows expects it. Therefore, it may not be reliable to use those commands, cmdlets or tools.

Here, I used Bulk Crap Uninstaller as an engine due to its amazing discovery capabilities.

## Usage

Install the service and WMI provider using the installer. It will start discovery as soon as possible. You can then run queries against it:
```powershell
Get-CimInstance -Class "ZB_App" |
        Select-Object * -ExcludeProperty PSComputerName, Scope, Path, Options, ClassPath, Properties, SystemProperties, Qualifiers, Site, Container, __*
```

One of the good things is that you can use WMI filters for your Group Policies. For instance, you can query if Firefox installed with this PowerShell command, and you can use the same query for WMI filters.

```powershell
Get-CimInstance -Query "SELECT Name FROM ZB_App WHERE Name LIKE 'Mozilla Firefox'"
```

## Architecture

The folder structure clearly shows the architecture of the software. The Windows service is split into two projects: the `WindowsServiceProxy` and `InventoryService`. The `InventoryService` consists of the business logic, which can be imported and unit-tested. The `WindowsServiceProxy`, on the other hand, is almost a generic Windows service executable which initiates the process.

All of the code is based on .NET Framework 4.8.1 due to the dependencies.

```yml
/root:
  /installer: Inno setup code to compile the installer
  /src:
    /InventoryEngine: Bulk Crap Installer-based discovery engine.
    /InventoryService: The service class and utilities which include the business logic
    /InventoryWmiProvider: The WMI Provider class `Package`, the core object populated and published to WMI.
    /Tests: Unit tests
    /WindowsServiceProxy: A reusable, skeleton Windows service executable that initiates the InventoryService.
```

## Thanks

The scan engine is ripped off from [Bulk Crap Uninstaller](https://github.com/Klocman/Bulk-Crap-Uninstaller). This project minimized the engine to a list-only implementation by excluding many capacilities including uninstallation. This project would not happen without [Marcin Szeniak](https://github.com/Klocman)'s work.
