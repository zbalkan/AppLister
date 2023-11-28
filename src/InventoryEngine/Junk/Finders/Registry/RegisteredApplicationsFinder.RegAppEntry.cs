using System;
using System.IO;

namespace InventoryEngine.Junk.Finders.Registry
{
    internal partial class RegisteredApplicationsFinder
    {
        private readonly struct RegAppEntry
        {
            public RegAppEntry(string valueName, string rootKeyName, string targetSubKeyPath)
            {
                ValueName = valueName;
                RootKeyName = rootKeyName;
                TargetSubKeyPath = targetSubKeyPath;

                AppName = AppKey = null;

                if (valueName.Length == 36 && valueName.StartsWith("App", StringComparison.Ordinal))
                {
                    var pathParts = targetSubKeyPath.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    for (var i = pathParts.Length - 2; i >= 8; i--)
                    {
                        if (pathParts[i] == "Packages")
                        {
                            AppName = pathParts[i + 1];
                            AppKey = string.Join("\\", pathParts, 0, i + 2);
                            break;
                        }
                    }
                }
            }

            public string ValueName { get; }
            public string TargetSubKeyPath { get; }
            public string RootKeyName { get; }
            public string TargetFullPath => Path.Combine(RootKeyName, TargetSubKeyPath);
            public string RegAppFullPath => Path.Combine(RootKeyName, RegAppsSubKeyPath);
            public string AppName { get; }
            public string AppKey { get; }
        }
    }
}