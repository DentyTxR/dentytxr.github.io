using System.Text.Json.Serialization;

namespace ghp_app.Pages.SCPSL.Models
{
    public class MinRequirements
    {
        [JsonPropertyName("cpu")]
        public string MinCpu { get; set; } = string.Empty;

        [JsonPropertyName("gpu")]
        public string MinGpu { get; set; } = string.Empty;
    }
}