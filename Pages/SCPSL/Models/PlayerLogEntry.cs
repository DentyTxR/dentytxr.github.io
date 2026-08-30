using ghp_app.Models;
using MudBlazor;

namespace ghp_app.Pages.SCPSL.Models
{
    public class PlayerLogEntry
    {
        public string Category { get; set; } = "Uncategorized";
        public string DisplayName { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public string Solution { get; set; } = string.Empty;
        public int Order { get; set; } = 0;
        public Color HighlightText { get; set; } = Color.Primary;
        public string BadgeText { get; set; } = string.Empty;
        public Color BadgeColor { get; set; } = Color.Default;
    }
}