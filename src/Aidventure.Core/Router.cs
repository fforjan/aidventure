namespace Aidventure.Core;

/// <summary>
/// Loads scene routing from the embedded gamedata.json (generated from SKILL.md).
/// </summary>
public static class Router
{
    private static readonly Dictionary<string, List<SceneRoute>> _routes;

    static Router()
    {
        var data = SceneLibrary.LoadGameData();
        _routes = data.Routes
            .GroupBy(r => r.SceneId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(ToSceneRoute).ToList());
    }

    private static SceneRoute ToSceneRoute(RouteData r) => new(
        Enum.Parse<RouteKey>(r.Key),
        r.TargetSceneId,
        r.Description,
        r.SetHunterAlly,
        r.SetInjured,
        r.SetSpotted,
        r.BurnMinutes,
        r.SetS12Method,
        r.RequiresHunterAlly);

    public static List<SceneRoute>? GetRoutes(string sceneId) =>
        _routes.TryGetValue(sceneId, out var r) ? r : null;

    public static bool IsTerminal(string sceneId) =>
        sceneId.StartsWith("DEAD") || sceneId.StartsWith("END");
}

