using FuzzySharp;
using System.Text.RegularExpressions;

namespace ghp_app.Utils
{
    public static class CompareHardwareHelper
    {
        public static bool Debug { get; set; } = false;

        private static readonly Dictionary<string, int?> _scoreCache = new();
        private static Dictionary<string, HashSet<string>>? _cpuTokenIndex;
        private static Dictionary<string, HashSet<string>>? _gpuTokenIndex;

        private static Dictionary<string, HashSet<string>> BuildTokenIndex(IEnumerable<string> keys)
        {
            var index = new Dictionary<string, HashSet<string>>();
            foreach (var key in keys)
            {
                foreach (var token in Normalize(key).Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!index.TryGetValue(token, out var set))
                        index[token] = set = new HashSet<string>();
                    set.Add(key);
                }
            }
            return index;
        }

        public static void Prime(Dictionary<string, int> cpuScores, Dictionary<string, int> gpuScores)
        {
            EnsureTokenIndex(cpuScores, isCpu: true);
            EnsureTokenIndex(gpuScores, isCpu: false);
        }

        private static void EnsureTokenIndex(Dictionary<string, int> scores, bool isCpu)
        {
            if (isCpu)
            {
                if (_cpuTokenIndex == null || _cpuTokenIndex.Count == 0)
                    _cpuTokenIndex = BuildTokenIndex(scores.Keys);
            }
            else
            {
                if (_gpuTokenIndex == null || _gpuTokenIndex.Count == 0)
                    _gpuTokenIndex = BuildTokenIndex(scores.Keys);
            }
        }

        public static string CompareCpu(string userCpu, string minCpu, Dictionary<string, int> cpuScores)
        {
            EnsureTokenIndex(cpuScores, isCpu: true);
            return CompareHardware(userCpu, minCpu, cpuScores, _cpuTokenIndex!, "CPU");
        }

        public static string CompareGpu(string userGpu, string minGpu, Dictionary<string, int> gpuScores)
        {
            EnsureTokenIndex(gpuScores, isCpu: false);
            return CompareHardware(userGpu, minGpu, gpuScores, _gpuTokenIndex!, "GPU");
        }

        private static string CompareHardware(string user, string min, Dictionary<string, int> scores, Dictionary<string, HashSet<string>> tokenIndex, string type)
        {
            int? userScore = FindScore(user, scores, tokenIndex, type, "User");
            int? minScore = FindScore(min, scores, tokenIndex, type, "Minimum");

            if (userScore == null || minScore == null)
                return "[Unknown]";

            return userScore >= minScore ? $"[Above Minimum]({minScore} <= {userScore})" : $"[Below Minimum]({minScore} <= {userScore})";
        }

        private static int? FindScore(string name, Dictionary<string, int> scores, Dictionary<string, HashSet<string>> tokenIndex, string type, string label)
        {
            if (string.IsNullOrWhiteSpace(name) || scores == null || scores.Count == 0)
                return null;

            var normalizedName = Normalize(name);

            if (_scoreCache.TryGetValue(normalizedName, out var cached))
                return cached;

            // 1. Exact match (normalized)
            var exact = scores.Keys.FirstOrDefault(k => Normalize(k) == normalizedName);
            if (exact != null)
            {
                if (Debug)
                    Console.WriteLine($"[{type}][{label}] Exact match: \"{name}\" -> \"{exact}\"");
                _scoreCache[normalizedName] = scores[exact];
                return scores[exact];
            }

            // 2. Prefix or substring match (normalized)
            var substring = scores.Keys
                .FirstOrDefault(k => Normalize(k).StartsWith(normalizedName) || normalizedName.StartsWith(Normalize(k)));
            if (substring != null)
            {
                if (Debug)
                    Console.WriteLine($"[{type}][{label}] Substring match: \"{name}\" -> \"{substring}\"");
                _scoreCache[normalizedName] = scores[substring];
                return scores[substring];
            }

            // 3. Model number extraction and direct match
            var modelMatch = Regex.Match(normalizedName, @"\b(\d{3,5})\b");
            if (modelMatch.Success)
            {
                var modelNumber = modelMatch.Groups[1].Value;
                // Find all PassMark keys with the same model number
                var modelCandidates = scores.Keys
                    .Where(k => Regex.IsMatch(Normalize(k), $@"\b{modelNumber}\b"))
                    .ToList();

                if (modelCandidates.Count > 0)
                {
                    // Prefer those with the most tokens in common
                    var bestModel = modelCandidates
                        .OrderByDescending(k => TokenOverlap(Normalize(k), normalizedName))
                        .ThenByDescending(k => Normalize(k).Contains("rtx") ? 1 : 0)
                        .First();

                    if (Debug)
                        Console.WriteLine($"[{type}][{label}] Model number match: \"{name}\" -> \"{bestModel}\"");
                    _scoreCache[normalizedName] = scores[bestModel];
                    return scores[bestModel];
                }
            }

            // 4. Token-based candidate reduction
            var nameTokens = normalizedName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var candidates = new HashSet<string>();
            foreach (var token in nameTokens)
                if (tokenIndex.TryGetValue(token, out var set))
                    candidates.UnionWith(set);

            if (candidates.Count == 0)
                candidates.UnionWith(scores.Keys.Take(25)); // fallback

            // 5. Fuzzy match only on candidates
            var allScores = candidates
                .Select(key => new
                {
                    Key = key,
                    Score = Math.Max(
                        Fuzz.TokenSetRatio(Normalize(key), normalizedName),
                        Fuzz.PartialRatio(Normalize(key), normalizedName)
                    )
                })
                .OrderByDescending(x => x.Score)
                .ToList();

            var best = allScores.FirstOrDefault();
            var second = allScores.Skip(1).FirstOrDefault();

            if (Debug)
            {
                Console.WriteLine($"[{type}][{label}] Fuzzy matching \"{name}\" (normalized: \"{normalizedName}\") against PassMark keys:");
                foreach (var entry in allScores.Take(10))
                    Console.WriteLine($"  - {entry.Key} : {entry.Score}");
                if (best != null)
                    Console.WriteLine($"  => Best match: \"{best.Key}\" (Score: {best.Score})");
                if (second != null)
                    Console.WriteLine($"  => Second best: \"{second.Key}\" (Score: {second.Score})");
            }

            // 6. Accept only if the best match is unique and strong
            if (best != null && best.Score == 100)
            {
                _scoreCache[normalizedName] = scores[best.Key];
                return scores[best.Key];
            }
            if (best != null && best.Score >= 88 &&
                (second == null || best.Score - second.Score >= 5))
            {
                _scoreCache[normalizedName] = scores[best.Key];
                return scores[best.Key];
            }

            // 7. Otherwise, do not risk a false positive
            _scoreCache[normalizedName] = null;
            return null;
        }

        // Helper: count token overlap between two normalized strings
        private static int TokenOverlap(string a, string b)
        {
            var setA = new HashSet<string>(a.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var setB = new HashSet<string>(b.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            setA.IntersectWith(setB);
            return setA.Count;
        }

        private static string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var s = input.ToLowerInvariant();
            s = Regex.Replace(s, @"\((r|tm|c)\)", "", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"[^\w\d]+", " ");
            s = Regex.Replace(s, @"\s+", " ");
            return s.Trim();
        }
    }
}