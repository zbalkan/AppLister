namespace InventoryEngine.Tools
{
    internal static class MsiWrapper
    {
        internal sealed class INSTALLPROPERTY
        {
            public string PropertyName { get; }

            public static INSTALLPROPERTY ASSIGNMENTTYPE = new INSTALLPROPERTY("AssignmentType");

            public static INSTALLPROPERTY HELPLINK = new INSTALLPROPERTY("HelpLink");

            public static INSTALLPROPERTY HELPTELEPHONE = new INSTALLPROPERTY("HelpTelephone");

            public static INSTALLPROPERTY INSTALLDATE = new INSTALLPROPERTY("InstallDate");

            // Product info attributes: installed information
            public static INSTALLPROPERTY INSTALLEDPRODUCTNAME = new INSTALLPROPERTY("InstalledProductName");

            public static INSTALLPROPERTY INSTALLLOCATION = new INSTALLPROPERTY("InstallLocation");

            public static INSTALLPROPERTY INSTALLSOURCE = new INSTALLPROPERTY("InstallSource");

            public static INSTALLPROPERTY LANGUAGE = new INSTALLPROPERTY("Language");

            public static INSTALLPROPERTY LOCALPACKAGE = new INSTALLPROPERTY("LocalPackage");

            public static INSTALLPROPERTY PACKAGECODE = new INSTALLPROPERTY("PackageCode");

            // Product info attributes: advertised information
            public static INSTALLPROPERTY PACKAGENAME = new INSTALLPROPERTY("PackageName");

            public static INSTALLPROPERTY PRODUCTICON = new INSTALLPROPERTY("ProductIcon");
            public static INSTALLPROPERTY PRODUCTNAME = new INSTALLPROPERTY("ProductName");
            public static INSTALLPROPERTY PUBLISHER = new INSTALLPROPERTY("Publisher");
            public static INSTALLPROPERTY TRANSFORMS = new INSTALLPROPERTY("Transforms");
            public static INSTALLPROPERTY URLINFOABOUT = new INSTALLPROPERTY("URLInfoAbout");
            public static INSTALLPROPERTY URLUPDATEINFO = new INSTALLPROPERTY("URLUpdateInfo");
            public static INSTALLPROPERTY VERSION = new INSTALLPROPERTY("Version");
            public static INSTALLPROPERTY VERSIONMAJOR = new INSTALLPROPERTY("VersionMajor");
            public static INSTALLPROPERTY VERSIONMINOR = new INSTALLPROPERTY("VersionMinor");
            public static INSTALLPROPERTY VERSIONSTRING = new INSTALLPROPERTY("VersionString");

            private INSTALLPROPERTY(string name)
            {
                PropertyName = name;
            }

            public override string ToString() => PropertyName;
        }
    }
}