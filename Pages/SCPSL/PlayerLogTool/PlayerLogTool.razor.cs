using ghp_app.Models;
using ghp_app.Pages.SCPSL.Models;

namespace ghp_app.Pages.SCPSL.PlayerLogTool
{
    public class PlayerLogToolSettings
    {
        public bool EnableHardwareCompare { get; set; } = true;
        public RuleModes CurrentRuleMode { get; set; } = RuleModes.Disabled;
        public List<PlayerLogRule> CustomRules { get; set; } = new();
    }

    public enum RuleModes
    {
        Disabled = 0,
        Addition = 1,
        Override = 2
    }
}