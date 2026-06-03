namespace Aidventure.Core;

public enum TurnResult
{
    Continue,
    Dead,
    Ending,
    Invalid
}

public record NarrationContext(
    Scene Scene,
    GameState State,
    TurnResult PreviousResult = TurnResult.Continue
);

public class GameEngine
{
    private readonly StateManager _state;
    private readonly OllamaService _ollama;

    public GameEngine(StateManager state, OllamaService ollama)
    {
        _state  = state;
        _ollama = ollama;
    }

    public void NewGame() => _state.ResetGame();

    public NarrationContext GetCurrentContext()
    {
        var gs     = _state.LoadState();
        var scene  = SceneLibrary.Scenes[gs.Scene];
        return new NarrationContext(scene, gs);
    }

    public async Task NarrateCurrentAsync(GameState gs, Func<string, Task> onToken, CancellationToken ct = default)
    {
        var scene = SceneLibrary.Scenes[gs.Scene];
        await _ollama.NarrateAsync(scene, gs, onToken, ct);
    }

    /// <summary>
    /// Processes freeform player input. Returns the turn result and the next scene (if any).
    /// </summary>
    public async Task<(TurnResult result, Scene? next)> ProcessInputAsync(
        string playerInput,
        CancellationToken ct = default)
    {
        var gs = _state.LoadState();

        if (Router.IsTerminal(gs.Scene))
            return (gs.Scene.StartsWith("DEAD") ? TurnResult.Dead : TurnResult.Ending, null);

        var routes = Router.GetRoutes(gs.Scene);
        if (routes == null || routes.Count == 0)
            return (TurnResult.Dead, null);

        // S14 final: filter routes by hunter_ally flag
        if (gs.Scene == "S14")
            routes = FilterS14Routes(routes, gs);

        var scene = SceneLibrary.Scenes[gs.Scene];
        var routeKey = await _ollama.InterpretAsync(scene, routes, gs, playerInput, ct);

        if (routeKey == null)
            return (TurnResult.Invalid, null);

        var chosen = routes.FirstOrDefault(r => r.Key == routeKey);
        if (chosen == null)
            return (TurnResult.Invalid, null);

        _state.ApplyRoute(chosen, gs);

        var nextGs    = _state.LoadState();
        var nextScene = SceneLibrary.Scenes[nextGs.Scene];

        var result = nextScene.IsDead    ? TurnResult.Dead
                   : nextScene.IsEnding  ? TurnResult.Ending
                   : TurnResult.Continue;

        return (result, nextScene);
    }

    public GameSummary GetSummary()
    {
        var gs = _state.LoadState();
        var scenes = gs.PathLog.Split('→');
        var epitaph = scenes.Length <= 3 ? "Moved like smoke. They never had a chance."
                    : gs.HunterAlly && gs.Scene.Contains("BEST") ? "Never had to go it alone — and was smart enough to know it."
                    : scenes.Any(s => s.StartsWith("DEAD")) ? "Learned the hard way. Aren't they all."
                    : "Stubborn. Reckless. Alive.";

        return new GameSummary(scenes, gs.Scene, epitaph, gs.HunterAlly, gs.Injured);
    }

    private static List<SceneRoute> FilterS14Routes(List<SceneRoute> routes, GameState gs)
    {
        if (gs.HunterAlly)
            return routes.Where(r => r.Key is RouteKey.A or RouteKey.B).ToList();
        else
            return routes.Where(r => r.Key is RouteKey.C or RouteKey.D).ToList();
    }
}

public record GameSummary(
    string[] ScenesVisited,
    string FinalScene,
    string Epitaph,
    bool HunterAlly,
    bool Injured
);
