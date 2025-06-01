using Microsoft.AspNetCore.Components;

namespace ghp_app.Models
{
    public class PageSettingEntry
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public RenderFragment Editor { get; set; } = default!;
    }
}