namespace Engine.Tools
{
    internal static class MsiWrapper
    {
        internal sealed class Installproperty
        {
            public string PropertyName { get; }

            public static Installproperty Assignmenttype = new Installproperty("AssignmentType");

            public static Installproperty Helplink = new Installproperty("HelpLink");

            public static Installproperty Helptelephone = new Installproperty("HelpTelephone");

            public static Installproperty Installdate = new Installproperty("InstallDate");

            // Product info attributes: installed information
            public static Installproperty Installedproductname = new Installproperty("InstalledProductName");

            public static Installproperty Installlocation = new Installproperty("InstallLocation");

            public static Installproperty Installsource = new Installproperty("InstallSource");

            public static Installproperty Language = new Installproperty("Language");

            public static Installproperty Localpackage = new Installproperty("LocalPackage");

            public static Installproperty Packagecode = new Installproperty("PackageCode");

            // Product info attributes: advertised information
            public static Installproperty Packagename = new Installproperty("PackageName");

            public static Installproperty Producticon = new Installproperty("ProductIcon");

            public static Installproperty Productname = new Installproperty("ProductName");

            public static Installproperty Publisher = new Installproperty("Publisher");

            public static Installproperty Transforms = new Installproperty("Transforms");

            public static Installproperty Urlinfoabout = new Installproperty("URLInfoAbout");

            public static Installproperty Urlupdateinfo = new Installproperty("URLUpdateInfo");

            public static Installproperty Version = new Installproperty("Version");

            public static Installproperty Versionmajor = new Installproperty("VersionMajor");

            public static Installproperty Versionminor = new Installproperty("VersionMinor");

            public static Installproperty Versionstring = new Installproperty("VersionString");

            private Installproperty(string name)
            {
                PropertyName = name;
            }

            public override string ToString() => PropertyName;
        }
    }
}