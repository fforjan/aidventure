using System.Text.Json;
using Aidventure.Core;
using GameDataGenerator;

var skillPath  = args.Length > 0 ? args[0]
    : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", ".github", "skills", "aidventure", "SKILL.md");
var outputPath = args.Length > 1 ? args[1]
    : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "Aidventure.Core", "gamedata.json");

skillPath  = Path.GetFullPath(skillPath);
outputPath = Path.GetFullPath(outputPath);

Console.WriteLine($"📖 Reading: {skillPath}");
Console.WriteLine($"📄 Writing: {outputPath}");
Console.WriteLine();

if (!File.Exists(skillPath))
{
    Console.Error.WriteLine($"ERROR: SKILL.md not found at {skillPath}");
    return 1;
}

var content = await File.ReadAllTextAsync(skillPath);
var data    = SkillParser.Parse(content);

var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
{
    WriteIndented          = true,
    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
});

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
await File.WriteAllTextAsync(outputPath, json);

Console.WriteLine();
Console.WriteLine($"✅ Written {new FileInfo(outputPath).Length / 1024} KB → {outputPath}");
return 0;
