using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace InventoryEngine.Factory
{
    internal sealed partial class ScoopFactory
    {
        private sealed class AppManifest
        {
            public IDictionary<string, AppManifestArchitecture> Architecture { get; set; }

            [JsonConverter(typeof(DynamicStringArrayConverter))]
            public string[] Bin { get; set; }

            [JsonPropertyName("env_add_path"), JsonConverter(typeof(DynamicStringArrayConverter))]
            public string[] EnvAddPath { get; set; }

            public string Homepage { get; set; }

            public string[][] Shortcuts { get; set; }
        }
    }
}