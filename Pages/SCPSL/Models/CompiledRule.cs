using System.Text.RegularExpressions;

namespace ghp_app.Pages.SCPSL.Models
{
    public class CompiledRule
    {
        public string Category { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public PlayerLogRule Rule { get; set; } = default!;
        public Regex CompiledRegex { get; set; } = default!;
    }
}