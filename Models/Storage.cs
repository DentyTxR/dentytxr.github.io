using System.Text.Json;

namespace ghp_app.Models
{
    public class Storage
    {
        public string Color { get; set; } = string.Empty;
        public Dictionary<string, JsonElement> PageSettings { get; set; } = new();
        public JsonElement? GlobalSettings { get; set; }
    }
}