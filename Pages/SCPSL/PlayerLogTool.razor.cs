using ghp_app.Models;

namespace ghp_app.Pages.SCPSL
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