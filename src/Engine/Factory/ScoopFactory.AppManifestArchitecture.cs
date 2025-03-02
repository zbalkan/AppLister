using System.Text.Json.Serialization;

namespace Engine.Factory
{
    internal sealed partial class ScoopFactory
    {
        private sealed class AppManifestArchitecture
        {
            [JsonConverter(typeof(DynamicStringArrayConverter))]
            public string[] Bin { get; set; }

            public string[] EnvAddPath { get; set; }

            public string[][] Shortcuts { get; set; }
        }
    }
}