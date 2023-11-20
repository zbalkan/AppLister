# InventoryService

A Windows service and a WMI provider to allow anyone to have a working inventory implementation.

*Beware that this project is in alpha stage. Breaking changes will occur.*

## Rationale

The `Win32_Product` has always been [problematic](https://gregramsey.net/2012/02/20/win32_product-is-evil/), especially when it comes to WMI filters. For local scenarios, PowerShell can save you but not for remote use cases or WMI filtering for Group Policies.

## Usage

Install the service and WMI provider using the installer. It will start discovery as soon as possible.

You can then run queries against it. The name

```powershell
$namespace = "ROOT\Inventory"
$classname = "Win32_Package"

Get-WmiObject -Class $Class -Namespace $Namespace |
        Select-Object * -ExcludeProperty PSComputerName, Scope, Path, Options, ClassPath, Properties, SystemProperties, Qualifiers, Site, Container, __*
```

## Thanks

The scan engine is ripped off from [Bulk Crap Uninstaller](https://github.com/Klocman/Bulk-Crap-Uninstaller). This project minimized the backend to a list-only implementation. This project would not happen without [Marcin Szeniak](https://github.com/Klocman)'s work.