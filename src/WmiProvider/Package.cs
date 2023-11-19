using System;
using System.Diagnostics;
using System.Management.Instrumentation;

namespace WmiProvider
{
    [ManagementEntity(Name = "Win32_Package", Singleton = false)]
    [ManagementQualifier("Description", Value = "Software inventory: A read-only Win32_Product alternative")]
    [DebuggerDisplay("Package = {Id}")]
    public class Package : IEquatable<Package>
    {
        [ManagementKey]
        [ManagementQualifier("Description", Value = "Unique identifier: <Name>_<Version>")]
        public string Id { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "Package Name")]
        public string Name { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "Package Version")]
        public string Version { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "Package Publisher information")]
        public string Publisher { get; set; }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Package))
            {
                return false;
            }
            return Id == ((Package)obj).Id;
        }

        public bool Equals(Package other) => Id == ((Package)other).Id;

        public override int GetHashCode() => Id.GetHashCode();
    }
}