using System.Collections.Generic;
using System.IO;
using System.Linq;
using Engine.Junk.Confidence;
using Engine.Junk.Containers;

namespace Engine.Junk.Finders.Registry
{
    internal class TracingScanner : IJunkCreator
    {
        public string CategoryName => "Junk_Tracing_GroupName";

        private const string FullTracingKey = @"HKEY_LOCAL_MACHINE\" + TracingKey;

        private const string TracingKey = @"SOFTWARE\Microsoft\Tracing";

        private ICollection<ApplicationUninstallerEntry> _allEntries;

        public IEnumerable<IJunkResult> FindJunk(ApplicationUninstallerEntry target)
        {
            var results = new List<RegistryKeyJunk>();
            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(TracingKey))
            {
                if (key != null)
                {
                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        var i = subKeyName.LastIndexOf('_');
                        if (i <= 0)
                        {
                            continue;
                        }

                        var str = subKeyName.Substring(0, i);

                        var conf = ConfidenceGenerators.GenerateConfidence(str, Path.Combine(FullTracingKey, subKeyName), 0, target).ToList();
                        if (!conf.Any())
                        {
                            continue;
                        }

                        var node = new RegistryKeyJunk(Path.Combine(FullTracingKey, subKeyName), target, this);
                        node.Confidence.AddRange(conf);
                        results.Add(node);
                    }
                }
            }

            ConfidenceGenerators.TestForSimilarNames(target, _allEntries, results.ConvertAll(x => new KeyValuePair<JunkResultBase, string>(x, x.RegKeyName)));

            return results;
        }

        public void Setup(ICollection<ApplicationUninstallerEntry> allUninstallers) => _allEntries = allUninstallers;
    }
}