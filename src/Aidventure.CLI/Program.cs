using Aidventure.Core;
using Spectre.Console;

// ── Configuration ──────────────────────────────────────────────────────────────
var ollamaUrl   = Environment.GetEnvironmentVariable("OLLAMA_URL")   ?? "http://localhost:11434";
var ollamaModel = Environment.GetEnvironmentVariable("OLLAMA_MODEL") ?? "llama3.1:8b";
var dbPath      = Environment.GetEnvironmentVariable("AIDVENTURE_DB") ?? "aidventure.db";

// ── Boot ───────────────────────────────────────────────────────────────────────
using var stateManager = new StateManager(dbPath);
var ollama             = new OllamaService(ollamaUrl, ollamaModel);
var engine             = new GameEngine(stateManager, ollama);

AnsiConsole.Clear();
PrintTitle();

await RunGameLoop();

// ── Main loop ─────────────────────────────────────────────────────────────────
async Task RunGameLoop()
{
    engine.NewGame();

    while (true)
    {
        var gs    = stateManager.LoadState();
        var scene = SceneLibrary.Scenes.GetValueOrDefault(gs.Scene);

        if (scene == null)
        {
            AnsiConsole.MarkupLine("[red]Unknown scene: {0}[/]", gs.Scene);
            break;
        }

        // Narrate current scene
        await NarrateScene(gs);

        if (scene.IsDead || scene.IsEnding)
        {
            PrintSummary(engine.GetSummary());

            if (!AskPlayAgain()) break;

            AnsiConsole.Clear();
            PrintTitle();
            engine.NewGame();
            continue;
        }

        // Player input
        var input = ReadPlayerInput();
        if (string.IsNullOrWhiteSpace(input)) continue;
        if (input.Equals("quit", StringComparison.OrdinalIgnoreCase)) break;

        AnsiConsole.WriteLine();

        // Process and route
        using var cts = new CancellationTokenSource();
        var (result, next) = await engine.ProcessInputAsync(input, cts.Token);

        if (result == TurnResult.Invalid)
        {
            AnsiConsole.MarkupLine("");
            AnsiConsole.MarkupLine("[italic dim]The universe doesn't cooperate. Try something else.[/]");
            AnsiConsole.WriteLine();
            continue;
        }

        // Advance state is already done by engine — loop will narrate next scene
    }
}

// ── Narration ─────────────────────────────────────────────────────────────────
async Task NarrateScene(GameState gs)
{
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine($"[dim]── {gs.Scene} ──────────────────────────────────────────────[/]");
    AnsiConsole.WriteLine();

    // Stream tokens directly — typewriter effect
    await ollama.NarrateAsync(
        SceneLibrary.Scenes[gs.Scene],
        gs,
        async token =>
        {
            AnsiConsole.Markup($"[gold1]{EscapeMarkup(token)}[/]");
            await Task.CompletedTask;
        });

    AnsiConsole.WriteLine();
    AnsiConsole.WriteLine();
}

// ── Input ─────────────────────────────────────────────────────────────────────
string ReadPlayerInput()
{
    AnsiConsole.Markup("[cyan]❯ [/]");
    Console.ForegroundColor = ConsoleColor.Cyan;
    var input = Console.ReadLine() ?? "";
    Console.ResetColor();
    return input.Trim();
}

// ── Summary ───────────────────────────────────────────────────────────────────
void PrintSummary(GameSummary summary)
{
    AnsiConsole.WriteLine();
    var table = new Table()
        .Border(TableBorder.Rounded)
        .BorderColor(Color.Grey)
        .AddColumn(new TableColumn("[dim]Stat[/]"))
        .AddColumn(new TableColumn("[dim]Value[/]"));

    table.AddRow("Scenes visited",  string.Join(" → ", summary.ScenesVisited));
    table.AddRow("Ending",          summary.FinalScene);
    table.AddRow("Hunter ally",     summary.HunterAlly ? "[green]Yes[/]" : "[dim]No[/]");
    table.AddRow("Injured",         summary.Injured    ? "[red]Yes[/]"   : "[dim]No[/]");
    table.AddRow("Epitaph",         $"[italic]{EscapeMarkup(summary.Epitaph)}[/]");

    AnsiConsole.Write(table);
    AnsiConsole.WriteLine();
}

bool AskPlayAgain()
{
    var choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[dim]Play again?[/]")
            .AddChoices("Yes, from the start", "No, quit"));
    return choice.StartsWith("Yes");
}

// ── Title ─────────────────────────────────────────────────────────────────────
void PrintTitle()
{
    AnsiConsole.Write(new FigletText("AIDVENTURE").Color(Color.Gold1));
    AnsiConsole.MarkupLine("[dim]The Last Flight   •   Powered by Ollama[/]");
    AnsiConsole.MarkupLine($"[dim]Model: {ollamaModel}   •   Type 'quit' to exit[/]");
    AnsiConsole.WriteLine();
}

string EscapeMarkup(string text) =>
    text.Replace("[", "[[").Replace("]", "]]");
