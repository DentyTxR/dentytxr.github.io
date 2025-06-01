using System.Text.Json.Serialization;

namespace ghp_app.Models
{
    public class PlayerLogRule
    {
        [JsonPropertyName("pattern")]
        public string Pattern { get; set; } = string.Empty;

        [JsonPropertyName("response")]
        public string Response { get; set; } = string.Empty;

        [JsonPropertyName("solution")]
        public string? Solution { get; set; }

        [JsonPropertyName("order")]
        public int Order { get; set; } = 0;

        [JsonPropertyName("latest")]
        public bool Latest { get; set; } = false;
    }
}