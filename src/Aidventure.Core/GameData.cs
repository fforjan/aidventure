using System.Text.Json.Serialization;

namespace Aidventure.Core;

public class GameData
{
    [JsonPropertyName("scenes")]
    public List<SceneData> Scenes { get; set; } = new();

    [JsonPropertyName("routes")]
    public List<RouteData> Routes { get; set; } = new();
}

public class SceneData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("text")]
    public string Text { get; set; } = "";

    [JsonPropertyName("isDead")]
    public bool IsDead { get; set; }

    [JsonPropertyName("isEnding")]
    public bool IsEnding { get; set; }

    [JsonPropertyName("deadLastSafe")]
    public string? DeadLastSafe { get; set; }
}

public class RouteData
{
    [JsonPropertyName("sceneId")]
    public string SceneId { get; set; } = "";

    [JsonPropertyName("key")]
    public string Key { get; set; } = "";

    [JsonPropertyName("targetSceneId")]
    public string TargetSceneId { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("requiresHunterAlly")]
    public bool? RequiresHunterAlly { get; set; }

    [JsonPropertyName("setHunterAlly")]
    public string? SetHunterAlly { get; set; }

    [JsonPropertyName("setInjured")]
    public bool SetInjured { get; set; }

    [JsonPropertyName("setSpotted")]
    public bool SetSpotted { get; set; }

    [JsonPropertyName("burnMinutes")]
    public int BurnMinutes { get; set; }

    [JsonPropertyName("setS12Method")]
    public string? SetS12Method { get; set; }
}
