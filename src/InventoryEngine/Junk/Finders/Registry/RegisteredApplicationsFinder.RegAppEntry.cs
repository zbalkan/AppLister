using System;
using System.IO;

namespace InventoryEngine.Junk.Finders.Registry
{
    internal partial class RegisteredApplicationsFinder
    {
        private readonly struct RegAppEntry
        {
            public string AppKey { get; }

            public string AppName { get; }

            public string RegAppFullPath => Path.Combine(_rootKeyName, RegAppsSubKeyPath);

            public string TargetFullPath => Path.Combine(_rootKeyName, TargetSubKeyPath);

            public string TargetSubKeyPath { get; }

            public string ValueName { get; }

            private readonly string _rootKeyName;

            public RegAppEntry(string valueName, string rootKeyName, string targetSubKeyPath)
            {
                ValueName = valueName;
                _rootKeyName = rootKeyName;
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
        }
    }
}