using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NetScope.Models
{
    public class MacPolicyResult
    {
        [JsonPropertyName("mac")]
        public string Mac { get; set; } = string.Empty;

        [JsonPropertyName("isAllowed")]
        public bool IsAllowed { get; set; }

        [JsonPropertyName("scopes")]
        public List<string> Scopes { get; set; } = new List<string>();
    }
}
