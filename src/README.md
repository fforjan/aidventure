# Aidventure CLI — Standalone C# Version

A standalone C# CLI that runs **The Last Flight** gamebook locally via [Ollama](https://ollama.ai/).
No internet, no Copilot, no cloud — pure local AI.

## Prerequisites

1. [.NET 9 SDK](https://dotnet.microsoft.com/download)
2. [Ollama](https://ollama.ai/) running locally
3. A model pulled:
   ```bash
   ollama pull llama3.1:8b
   ```

## Run

```bash
cd src/Aidventure.CLI
dotnet run
```

## Environment Variables

| Variable | Default | Description |
|---|---|---|
| `OLLAMA_URL` | `http://localhost:11434` | Ollama API base URL |
| `OLLAMA_MODEL` | `llama3.1:8b` | Model to use for narration and routing |
| `AIDVENTURE_DB` | `aidventure.db` | SQLite database path for game state |

Example with a different model:

```bash
OLLAMA_MODEL=mistral:7b dotnet run --project src/Aidventure.CLI
```

## Architecture

```
src/
├── Aidventure.Core/       # Engine (no UI dependency)
│   ├── Models.cs          # Scene, SceneRoute, GameState records
│   ├── StateManager.cs    # SQLite persistence
│   ├── Router.cs          # Hard-coded scene graph
│   ├── SceneLibrary.cs    # All 14 scenes, 6 dead ends, 4 endings
│   ├── OllamaService.cs   # Narrate (streaming) + Interpret (JSON routing)
│   └── GameEngine.cs      # Turn loop orchestration
└── Aidventure.CLI/
    └── Program.cs         # Spectre.Console UI, typewriter streaming
```

## AI Design

Two AI calls per turn:

1. **Narrate** — streaming, gold-coloured typewriter output
2. **Interpret** — non-streaming JSON `{"route":"A"}` to map freeform input to a scene route

Routing logic lives entirely in C# (`Router.cs`). The AI never controls game state.

## Recommended Models

| Model | Quality | Speed |
|---|---|---|
| `llama3.1:8b` | ⭐⭐⭐⭐ | Fast |
| `mistral:7b` | ⭐⭐⭐ | Fast |
| `llama3.2:3b` | ⭐⭐ | Very fast (low VRAM) |
| `llama3.3:70b` | ⭐⭐⭐⭐⭐ | Slow (needs high VRAM) |
