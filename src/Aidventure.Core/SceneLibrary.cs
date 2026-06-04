using System.Text.Json;

namespace Aidventure.Core;

/// <summary>
/// Loads scene text from the embedded gamedata.json (generated from SKILL.md).
/// </summary>
public static class SceneLibrary
{
    public static readonly Dictionary<string, Scene> Scenes;

    static SceneLibrary()
    {
        var data = LoadGameData();
        Scenes = data.Scenes.ToDictionary(
            s => s.Id,
            s => new Scene(s.Id, s.Text, s.IsDead, s.IsEnding, s.DeadLastSafe));
    }

    internal static GameData LoadGameData()
    {
        var assembly     = typeof(SceneLibrary).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .First(n => n.EndsWith("gamedata.json", StringComparison.OrdinalIgnoreCase));
        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        return JsonSerializer.Deserialize<GameData>(stream)!;
    }
}

