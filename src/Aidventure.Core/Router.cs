namespace Aidventure.Core;

/// <summary>
/// Hard-coded scene graph matching SKILL.md routing map.
/// Returns the set of valid routes for a given scene ID.
/// </summary>
public static class Router
{
    private static readonly Dictionary<string, List<SceneRoute>> _routes = new()
    {
        ["S1"] =
        [
            new(RouteKey.A, "S3"),
            new(RouteKey.B, "S2"),
            new(RouteKey.C, "S3", SetSpotted: true)
        ],
        ["S2"] =
        [
            new(RouteKey.A, "S4"),
            new(RouteKey.B, "S3"),
            new(RouteKey.C, "DEAD-1")
        ],
        ["S3"] =
        [
            new(RouteKey.A, "S5"),
            new(RouteKey.B, "S6")
        ],
        ["S4"] =
        [
            new(RouteKey.A, "S5"),
            new(RouteKey.B, "S7"),
            new(RouteKey.C, "DEAD-2")
        ],
        ["S5"] =
        [
            new(RouteKey.A, "S8"),
            new(RouteKey.B, "S9"),
            new(RouteKey.C, "S10")
        ],
        ["S6"] =
        [
            new(RouteKey.A, "S11"),
            new(RouteKey.B, "S8")
        ],
        ["S7"] =
        [
            new(RouteKey.A, "S11"),
            new(RouteKey.B, "DEAD-3")
        ],
        ["S8"] =
        [
            new(RouteKey.A, "S11", SetHunterAlly: "true"),
            new(RouteKey.B, "S9")
        ],
        ["S9"] =
        [
            new(RouteKey.A, "S11"),
            new(RouteKey.B, "S8")
        ],
        ["S10"] =
        [
            new(RouteKey.A, "S11", BurnMinutes: 12),
            new(RouteKey.B, "DEAD-4")
        ],
        ["S11"] =
        [
            new(RouteKey.A, "S12"),
            new(RouteKey.B, "S12"),
            new(RouteKey.C, "S13", SetInjured: true)
        ],
        ["S12"] =
        [
            new(RouteKey.A, "S14", SetS12Method: "blend"),
            new(RouteKey.B, "S14", SetS12Method: "distraction"),
            new(RouteKey.C, "S14", SetS12Method: "escort")
        ],
        ["S13"] =
        [
            new(RouteKey.A, "S14"),
            new(RouteKey.B, "DEAD-5")
        ],
        ["S14"] =
        [
            // Resolved at runtime based on hunter_ally flag
            new(RouteKey.A, "END-BEST"),
            new(RouteKey.B, "END-GOOD"),
            new(RouteKey.C, "END-NEUTRAL"),
            new(RouteKey.D, "END-HARD"),
        ]
    };

    public static List<SceneRoute>? GetRoutes(string sceneId) =>
        _routes.TryGetValue(sceneId, out var r) ? r : null;

    public static bool IsTerminal(string sceneId) =>
        sceneId.StartsWith("DEAD") || sceneId.StartsWith("END");
}
