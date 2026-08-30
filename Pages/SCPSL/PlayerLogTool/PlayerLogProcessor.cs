using ghp_app.Pages.SCPSL.Models;
using ghp_app.Utils;
using MudBlazor;
using System.Text.RegularExpressions;

namespace ghp_app.Pages.SCPSL.PlayerLogTool
{
    public static class PlayerLogProcessor
    {
        public static List<PlayerLogEntry> Process(
                string content,
                List<CompiledRule> rules,
                string minCpu,
                string minGpu,
                Dictionary<string, int> cpuScores,
                Dictionary<string, int> gpuScores)
        {
            var results = new List<PlayerLogEntry>();

            if (rules.Count == 0)
                return results;

            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var ruleMatches = new Dictionary<CompiledRule, List<(int Index, Match Match)>>();

            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                var line = lines[lineIndex];
                foreach (var rule in rules)
                {
                    var match = rule.CompiledRegex.Match(line);
                    if (match.Success)
                    {
                        if (!ruleMatches.ContainsKey(rule))
                            ruleMatches[rule] = new List<(int, Match)>();

                        ruleMatches[rule].Add((lineIndex, match));
                    }
                }
            }

            foreach (var rule in rules)
            {
                if (!ruleMatches.TryGetValue(rule, out var matches) || matches.Count == 0)
                    continue;

                var selectedMatch = rule.Rule.Latest ? matches.Last().Match : matches.First().Match;
                var data = selectedMatch.Groups[1].Value;

                string response = rule.Rule.Response.Replace("${data}", data);

                var entry = new PlayerLogEntry
                {
                    Category = rule.Category,
                    DisplayName = rule.DisplayName,
                    Solution = rule.Rule.Solution,
                    Order = rule.Rule.Order
                };

                switch (rule.DisplayName)
                {
                    case "OS":
                        response = response.Replace("${status}", GetOsStatus(data));
                        break;

                    case "CPU":
                        var cpuStatus = CompareHardwareHelper.CompareCpu(data, minCpu, cpuScores, true);
                        response = response.Replace("${status}", cpuStatus);

                        if (cpuStatus.Contains("Below Minimum"))
                            entry.HighlightText = Color.Error;
                        else if (cpuStatus.Contains("Above Minimum"))
                            entry.HighlightText = Color.Success;
                        else
                            entry.HighlightText = Color.Default;
                        break;

                    case "GPU":
                        var gpuStatus = CompareHardwareHelper.CompareGpu(data, minGpu, gpuScores, true);
                        response = response.Replace("${status}", gpuStatus);

                        if (gpuStatus.Contains("Below Minimum"))
                            entry.HighlightText = Color.Error;
                        else if (gpuStatus.Contains("Above Minimum"))
                            entry.HighlightText = Color.Success;
                        else
                            entry.HighlightText = Color.Default;
                        break;
                }

                if (rule.Category == "Errors")
                {
                    entry.HighlightText = Color.Error;
                    entry.BadgeText = "!";
                    entry.BadgeColor = Color.Error;
                }

                entry.Response = response;

                results.Add(entry);
            }

            return results;
        }

        private static string GetOsStatus(string osLine)
        {
            if (osLine.Contains("Windows 10", StringComparison.OrdinalIgnoreCase) || osLine.Contains("Windows 11", StringComparison.OrdinalIgnoreCase))
                return $"[Supported] {osLine}";
            else
                return $"[Unsupported] {osLine} (game might be running in compatibility mode)";
        }
    }
}