using OllamaSharp;
using OllamaSharp.Models.Chat;
using System.Text;
using System.Text.Json;

namespace Aidventure.Core;

public class OllamaService
{
    private readonly OllamaApiClient _client;
    private readonly string _model;

    private const string NarrationSystem = """
        You are the Game Master of a branching escape thriller called "The Last Flight".
        Narrate in second person present tense ("You push through the door...").
        Write 3–5 punchy cinematic paragraphs per scene.
        NEVER list choices A/B/C. End on a moment of tension or an open question.
        NEVER reveal routing, scene IDs, or any meta-text. Output only pure narrative.
        No inner monologue. Describe only externally observable events — what is seen, heard, smelled, physically happening.
        """;

    private const string InterpretSystem = """
        You are a routing engine for a branching gamebook. Given a scene context, a list of possible routes,
        and the player's freeform response, return ONLY valid JSON in this exact format:
        {"route":"A"} or {"route":"B"} or {"route":"C"} or {"route":"D"} or {"route":"INVALID"}

        Use "INVALID" only if the player's action is completely impossible or outside the scene's options.
        Otherwise pick the closest matching route. Return ONLY the JSON object, no other text.
        """;

    public OllamaService(string baseUrl = "http://localhost:11434", string model = "llama3.1:8b")
    {
        _client = new OllamaApiClient(new Uri(baseUrl));
        _model = model;
    }

    /// <summary>
    /// Streams narration for a scene to the provided callback (token by token).
    /// </summary>
    public async Task NarrateAsync(
        Scene scene,
        GameState state,
        Func<string, Task> onToken,
        CancellationToken ct = default)
    {
        var userPrompt = BuildNarrationPrompt(scene, state);

        var messages = new List<Message>
        {
            new() { Role = ChatRole.System, Content = NarrationSystem },
            new() { Role = ChatRole.User,   Content = userPrompt }
        };

        await foreach (var chunk in _client.ChatAsync(
            new OllamaSharp.Models.Chat.ChatRequest
            {
                Model = _model,
                Messages = messages,
                Stream = true
            }, ct))
        {
            var token = chunk?.Message?.Content;
            if (token != null)
                await onToken(token);
        }
    }

    /// <summary>
    /// Interprets freeform player input against valid routes. Returns the matched RouteKey or null for INVALID.
    /// </summary>
    public async Task<RouteKey?> InterpretAsync(
        Scene scene,
        List<SceneRoute> routes,
        GameState state,
        string playerInput,
        CancellationToken ct = default)
    {
        var routeDescriptions = BuildRouteDescriptions(routes, state);
        var prompt = $"""
            Scene: {scene.Id}
            Scene text: {scene.Text}
            State: hunter_ally={state.HunterAlly}, injured={state.Injured}, spotted={state.Spotted}

            Possible routes:
            {routeDescriptions}

            Player input: "{playerInput}"

            Respond with JSON only.
            """;

        var messages = new List<Message>
        {
            new() { Role = ChatRole.System, Content = InterpretSystem },
            new() { Role = ChatRole.User,   Content = prompt }
        };

        var sb = new StringBuilder();
        await foreach (var chunk in _client.ChatAsync(
            new OllamaSharp.Models.Chat.ChatRequest
            {
                Model    = _model,
                Messages = messages,
                Stream   = true
            }, ct))
        {
            sb.Append(chunk?.Message?.Content ?? "");
        }

        return ParseRouteKey(sb.ToString().Trim());
    }

    private static string BuildNarrationPrompt(Scene scene, GameState state)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Scene ID: {scene.Id}");
        sb.AppendLine($"Scene context: {scene.Text}");
        sb.AppendLine();

        if (state.HunterAlly)
            sb.AppendLine("Context: The city hunter is now the player's ally — they are present.");
        if (state.Injured)
            sb.AppendLine("Context: The player is injured — blood soaking through their sleeve.");
        if (state.S12Method == "distraction")
            sb.AppendLine("Context: A fire alarm is still echoing through the terminal.");
        if (state.S12Method == "escort")
            sb.AppendLine("Context: Security escorted the player — time is very tight.");

        sb.AppendLine();
        sb.AppendLine("Narrate this scene now. Do NOT list choices. End on tension or an open question.");
        return sb.ToString();
    }

    private static string BuildRouteDescriptions(List<SceneRoute> routes, GameState state)
    {
        var sb = new StringBuilder();
        foreach (var r in routes)
        {
            sb.AppendLine($"Route {r.Key}: {r.Description}");
        }
        return sb.ToString();
    }

    private static RouteKey? ParseRouteKey(string json)
    {
        try
        {
            // Handle possible markdown fences
            var start = json.IndexOf('{');
            var end   = json.LastIndexOf('}');
            if (start < 0 || end < 0) return null;

            json = json[start..(end + 1)];
            var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("route", out var routeEl))
                return null;

            var val = routeEl.GetString();
            if (val == "INVALID") return null;

            return Enum.TryParse<RouteKey>(val, ignoreCase: true, out var key) ? key : null;
        }
        catch
        {
            return null;
        }
    }
}
