namespace Aidventure.Core;

public enum RouteKey { A, B, C, D }

public record Scene(
    string Id,
    string Text,
    bool IsDead = false,
    bool IsEnding = false,
    string? DeadLastSafe = null
);

public record SceneRoute(
    RouteKey Key,
    string TargetSceneId,
    string? SetHunterAlly = null,
    bool SetInjured = false,
    bool SetSpotted = false,
    int BurnMinutes = 0,
    string? SetS12Method = null
);

public class GameState
{
    public string Scene { get; set; } = "S1";
    public bool HunterAlly { get; set; }
    public int Cash { get; set; } = 200;
    public bool Injured { get; set; }
    public bool Spotted { get; set; }
    public string Status { get; set; } = "alive";
    public string PathLog { get; set; } = "S1";
    public string? S12Method { get; set; }
}
