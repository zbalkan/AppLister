using System;
using System.Text.Json.Serialization;
using Engine.Extensions;

namespace Engine.Factory
{
    internal sealed partial class ScoopFactory
    {
        private sealed class ExportAppEntry
        {
            public string Info { get; set; }

            [JsonIgnore] public bool IsPublic => Info?.Contains("Global install", StringComparison.InvariantCultureIgnoreCase) == true;

            public string Name { get; set; }

            public string Source { get; set; }

            public DateTimeOffset Updated { get; set; }

            public string Version { get; set; }
        }
    }
}