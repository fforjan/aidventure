using System.Text;
using System.Text.RegularExpressions;
using Aidventure.Core;

namespace GameDataGenerator;

/// <summary>
/// Parses SKILL.md into a GameData object.
/// </summary>
public static class SkillParser
{
    // ── Public entry point ────────────────────────────────────────────────────

    public static GameData Parse(string content)
    {
        // Normalize to Unix line endings so all regex $ anchors work consistently
        content = content.Replace("\r\n", "\n").Replace("\r", "\n");

        var data = new GameData();

        int sceneLibIdx = content.IndexOf("## 📖 Scene Library");
        int deadEndsIdx = content.IndexOf("## 💀 Dead Ends");
        int endingsIdx  = content.IndexOf("## 🏆 Endings");

        if (sceneLibIdx < 0) throw new InvalidOperationException("Scene Library section not found");
        if (deadEndsIdx < 0) throw new InvalidOperationException("Dead Ends section not found");
        if (endingsIdx  < 0) throw new InvalidOperationException("Endings section not found");

        // Parse S1–S14
        var librarySection = content[sceneLibIdx..deadEndsIdx];
        data.Scenes.AddRange(ParseMainScenes(librarySection, data.Routes));

        // Parse dead ends (table only — text generated from cause)
        var deadSection = content[deadEndsIdx..endingsIdx];
        data.Scenes.AddRange(ParseDeadEnds(deadSection));

        // Parse endings
        var endingsSection = content[endingsIdx..];
        data.Scenes.AddRange(ParseEndings(endingsSection));

        Validate(data);
        return data;
    }

    // ── Scene Library (S1–S14) ────────────────────────────────────────────────

    private static List<SceneData> ParseMainScenes(string section, List<RouteData> routes)
    {
        var scenes = new List<SceneData>();

        // Find every ### S{n} header
        var headerRegex = new Regex(@"^### (S\d+) — (.+)$", RegexOptions.Multiline);
        var headers = headerRegex.Matches(section);

        for (int i = 0; i < headers.Count; i++)
        {
            var m     = headers[i];
            var id    = m.Groups[1].Value.Trim();
            var title = m.Groups[2].Value.Trim();

            // Content from after the header line to the next header (or end)
            int contentStart = m.Index + m.Length;
            int contentEnd   = i + 1 < headers.Count ? headers[i + 1].Index : section.Length;
            var body = section[contentStart..contentEnd];

            var (text, sceneRoutes) = ParseSceneBody(id, body);

            scenes.Add(new SceneData { Id = id, Title = title, Text = text });
            routes.AddRange(sceneRoutes);
        }

        return scenes;
    }

    private static (string text, List<RouteData> routes) ParseSceneBody(string sceneId, string body)
    {
        var lines = body.Split('\n');

        var textLines   = new List<string>();
        var routeLines  = new List<string>();
        bool inRouting  = false;

        foreach (var line in lines)
        {
            if (line.TrimStart().StartsWith("> **Routing (hidden):**"))
            {
                inRouting = true;
                continue;
            }

            if (inRouting)
            {
                routeLines.Add(line);
            }
            else
            {
                // Keep all non-header lines as text (including narrative blockquotes like > *"..."*)
                textLines.Add(line);
            }
        }

        var text   = CleanText(textLines);
        var routes = ParseRoutingBlock(sceneId, routeLines);
        return (text, routes);
    }

    // ── Routing block parser ──────────────────────────────────────────────────

    private static readonly Regex RouteLineRegex =
        new(@"^\s*>\s*-\s+(.+?)\s*→\s*(.+)$", RegexOptions.Compiled);

    private static List<RouteData> ParseRoutingBlock(string sceneId, List<string> lines)
    {
        var routes = new List<RouteData>();
        bool? currentHunterAllyCondition = null; // null = no condition
        var keyCounter = 0; // 0=A, 1=B, 2=C, …

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();

            // Detect conditional blocks (S14 only)
            if (line.Contains("If `hunter_ally = true`"))
            {
                currentHunterAllyCondition = true;
                keyCounter = 0; // A, B for hunter_ally=true block
                continue;
            }
            if (line.Contains("If `hunter_ally = false`"))
            {
                currentHunterAllyCondition = false;
                keyCounter = 2; // C, D, E for hunter_ally=false block
                continue;
            }

            var m = RouteLineRegex.Match(line);
            if (!m.Success) continue;

            var description = m.Groups[1].Value.Trim();
            var rest        = m.Groups[2].Value.Trim();

            // Split target from flags (target is first token(s) before spaces/parens)
            var targetRaw = ExtractTarget(rest);
            var targetId  = MapTarget(targetRaw);
            if (targetId == null) continue;

            var route = new RouteData
            {
                SceneId              = sceneId,
                Key                  = KeyLetter(keyCounter++),
                TargetSceneId        = targetId,
                Description          = description,
                RequiresHunterAlly   = currentHunterAllyCondition,
            };

            ParseFlags(rest, route);
            routes.Add(route);
        }

        return routes;
    }

    private static string ExtractTarget(string rest)
    {
        // Target is everything before the first space+paren or end, stripped of emojis handled in MapTarget
        // Examples: "S3", "S11", "💀 DEAD-3", "🏆 GOOD ENDING", "S14 (method: blend)"
        var trimmed = rest.Trim();

        // If it starts with an emoji, take up to end of word/number
        var emojiTarget = Regex.Match(trimmed, @"^[💀🏆]\s*(DEAD-\d+|BEST ENDING|GOOD ENDING|NEUTRAL ENDING|HARD ENDING)");
        if (emojiTarget.Success)
            return emojiTarget.Groups[1].Value;

        // Plain scene ID like "S3", "S11", "S14"
        var sceneId = Regex.Match(trimmed, @"^(S\d+)");
        if (sceneId.Success)
            return sceneId.Groups[1].Value;

        return trimmed.Split(' ')[0];
    }

    private static string? MapTarget(string raw) => raw.Trim() switch
    {
        "S1"  or "S2"  or "S3"  or "S4"  or "S5"  or "S6"  or "S7"  => raw,
        "S8"  or "S9"  or "S10" or "S11" or "S12" or "S13" or "S14" => raw,
        "DEAD-1" or "DEAD-2" or "DEAD-3" or "DEAD-4" or "DEAD-5" or "DEAD-6" => raw,
        "BEST ENDING"    => "END-BEST",
        "GOOD ENDING"    => "END-GOOD",
        "NEUTRAL ENDING" => "END-NEUTRAL",
        "HARD ENDING"    => "END-HARD",
        _                => null
    };

    private static void ParseFlags(string rest, RouteData route)
    {
        if (Regex.IsMatch(rest, @"spotted=true"))         route.SetSpotted    = true;
        if (Regex.IsMatch(rest, @"hunter_ally=true"))     route.SetHunterAlly = "true";
        if (Regex.IsMatch(rest, @"injured=true"))         route.SetInjured    = true;

        var method = Regex.Match(rest, @"method:\s*(blend|distraction|escort)");
        if (method.Success) route.SetS12Method = method.Groups[1].Value;

        var minutes = Regex.Match(rest, @"(\d+)\s*(?:min|mins|minute|minutes)\s+burned");
        if (minutes.Success) route.BurnMinutes = int.Parse(minutes.Groups[1].Value);
    }

    private static string KeyLetter(int index) => index switch
    {
        0 => "A", 1 => "B", 2 => "C", 3 => "D", 4 => "E",
        _ => ((char)('A' + index)).ToString()
    };

    // ── Dead Ends ─────────────────────────────────────────────────────────────

    private static List<SceneData> ParseDeadEnds(string section)
    {
        var scenes = new List<SceneData>();

        // | DEAD-1 | Froze in the lobby | S2 |
        var rowRegex = new Regex(@"\|\s*(DEAD-\d+)\s*\|\s*(.+?)\s*\|\s*(S\d+)\s*\|");
        foreach (Match m in rowRegex.Matches(section))
        {
            var id       = m.Groups[1].Value.Trim();
            var cause    = m.Groups[2].Value.Trim();
            var lastSafe = m.Groups[3].Value.Trim();

            scenes.Add(new SceneData
            {
                Id           = id,
                Title        = cause,
                Text         = cause,  // AI uses cause to narrate death dramatically
                IsDead       = true,
                DeadLastSafe = lastSafe,
            });
        }

        return scenes;
    }

    // ── Endings ───────────────────────────────────────────────────────────────

    private static List<SceneData> ParseEndings(string section)
    {
        var scenes  = new List<SceneData>();
        var headers = new Regex(@"^### (BEST|GOOD|NEUTRAL|HARD) ENDING — \*(.+?)\*$", RegexOptions.Multiline);
        var matches = headers.Matches(section);

        for (int i = 0; i < matches.Count; i++)
        {
            var m     = matches[i];
            var key   = m.Groups[1].Value; // BEST / GOOD / NEUTRAL / HARD
            var title = m.Groups[2].Value;

            int start = m.Index + m.Length;
            int end   = i + 1 < matches.Count ? matches[i + 1].Index : section.Length;
            var body  = section[start..end];

            // Strip condition line *(hunter_ally=true, ...)* and clean
            var text = CleanText(body.Split('\n')
                .Where(l => !l.TrimStart().StartsWith("*("))
                .ToList());

            scenes.Add(new SceneData
            {
                Id        = $"END-{key}",
                Title     = title,
                Text      = text,
                IsEnding  = true,
            });
        }

        return scenes;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string CleanText(IEnumerable<string> lines)
    {
        var result = new StringBuilder();
        foreach (var line in lines)
        {
            var clean = line.TrimEnd();
            // Strip blockquote marker but keep content (e.g. > *"GET OUT NOW..."*)
            if (clean.StartsWith("> ")) clean = clean[2..];
            else if (clean == ">")      continue;

            // Skip separator lines
            if (clean == "---") continue;

            result.AppendLine(clean);
        }
        return result.ToString().Trim();
    }

    // ── Validation ────────────────────────────────────────────────────────────

    private static void Validate(GameData data)
    {
        var sceneIds  = data.Scenes.Select(s => s.Id).ToHashSet();
        var errors    = new List<string>();

        var mainScenes = data.Scenes.Where(s => !s.IsDead && !s.IsEnding).ToList();
        var deadEnds   = data.Scenes.Where(s => s.IsDead).ToList();
        var endings    = data.Scenes.Where(s => s.IsEnding).ToList();

        if (mainScenes.Count != 14) errors.Add($"Expected 14 main scenes, got {mainScenes.Count}");
        if (deadEnds.Count != 6)   errors.Add($"Expected 6 dead ends, got {deadEnds.Count}");
        if (endings.Count != 4)    errors.Add($"Expected 4 endings, got {endings.Count}");

        // Validate all route targets resolve
        foreach (var route in data.Routes)
        {
            if (!sceneIds.Contains(route.TargetSceneId))
                errors.Add($"Route {route.SceneId}:{route.Key} → unknown target '{route.TargetSceneId}'");
        }

        if (errors.Any())
            throw new InvalidOperationException("Validation failed:\n" + string.Join("\n", errors));

        Console.WriteLine($"✅ Validation passed: {mainScenes.Count} scenes, {deadEnds.Count} dead ends, {endings.Count} endings, {data.Routes.Count} routes");
    }
}
