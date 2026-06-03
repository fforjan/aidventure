using Microsoft.Data.Sqlite;

namespace Aidventure.Core;

public class StateManager : IDisposable
{
    private readonly SqliteConnection _db;

    public StateManager(string dbPath = "aidventure.db")
    {
        _db = new SqliteConnection($"Data Source={dbPath}");
        _db.Open();
        EnsureSchema();
    }

    private void EnsureSchema()
    {
        Execute(@"CREATE TABLE IF NOT EXISTS game_state (key TEXT PRIMARY KEY, value TEXT)");
    }

    public void ResetGame()
    {
        Execute("DELETE FROM game_state");
        var defaults = new Dictionary<string, string>
        {
            ["scene"]        = "S1",
            ["hunter_ally"]  = "false",
            ["cash"]         = "200",
            ["injured"]      = "false",
            ["spotted"]      = "false",
            ["status"]       = "alive",
            ["path_log"]     = "S1",
            ["s12_method"]   = ""
        };
        foreach (var (k, v) in defaults)
            Upsert(k, v);
    }

    public string Get(string key)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "SELECT value FROM game_state WHERE key = @k";
        cmd.Parameters.AddWithValue("@k", key);
        return cmd.ExecuteScalar() as string ?? "";
    }

    public void Set(string key, string value) => Upsert(key, value);

    public GameState LoadState() => new()
    {
        Scene       = Get("scene"),
        HunterAlly  = Get("hunter_ally") == "true",
        Cash        = int.TryParse(Get("cash"), out var c) ? c : 200,
        Injured     = Get("injured") == "true",
        Spotted     = Get("spotted") == "true",
        Status      = Get("status"),
        PathLog     = Get("path_log"),
        S12Method   = Get("s12_method") is { Length: > 0 } m ? m : null
    };

    public void ApplyRoute(SceneRoute route, GameState current)
    {
        var nextId = route.TargetSceneId;
        Set("scene", nextId);
        Set("path_log", current.PathLog + "→" + nextId);

        if (route.SetHunterAlly != null)
            Set("hunter_ally", route.SetHunterAlly);
        if (route.SetInjured)
            Set("injured", "true");
        if (route.SetSpotted)
            Set("spotted", "true");
        if (route.SetS12Method != null)
            Set("s12_method", route.SetS12Method);
    }

    private void Upsert(string key, string value)
    {
        Execute("INSERT OR REPLACE INTO game_state (key, value) VALUES (@k, @v)",
            ("@k", key), ("@v", value));
    }

    private void Execute(string sql, params (string, string)[] parameters)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (k, v) in parameters)
            cmd.Parameters.AddWithValue(k, v);
        cmd.ExecuteNonQuery();
    }

    public void Dispose() => _db.Dispose();
}
