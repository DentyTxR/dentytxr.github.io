using ghp_app.Models;

namespace ghp_app.Services
{
    public class PageSettingsProvider
    {
        public List<PageSettingEntry>? CurrentSettings { get; set; }
        public Func<Task>? SaveSettingsAction { get; set; }
    }
}