using System.Text.Json.Serialization;

namespace ghp_app.Pages.SCPSL.Models
{
    public class PassmarkData
    {
        [JsonPropertyName("cpu")]
        public Dictionary<string, int> Cpu { get; set; } = new();

        [JsonPropertyName("gpu")]
        public Dictionary<string, int> Gpu { get; set; } = new();
    }
}