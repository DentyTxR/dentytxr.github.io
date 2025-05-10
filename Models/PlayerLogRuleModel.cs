using System.Text.Json.Serialization;

namespace ghp_app.Models
{
    public class PlayerLogRuleModel
    {
        [JsonPropertyName("pattern")]
        public string Pattern { get; set; } = string.Empty;

        [JsonPropertyName("display")]
        public string Display { get; set; } = string.Empty;

        [JsonPropertyName("response")]
        public string Response { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;
    }
}