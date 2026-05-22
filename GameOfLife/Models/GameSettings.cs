using GameOfLife.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameOfLife.Models;

internal class GameSettings
{
    // set default values for all properties, so that the user doesn't have to specify all of them in the JSON file
    public int Width { get; set; } = 80;  // not used if loaded from RLE file
    public int Height { get; set; } = 40;  // not used if loaded from RLE file
    public bool Toroidal { get; set; } = true;
    public CellularRuleType RuleType { get; set; } = CellularRuleType.Conway;
    public NeighbourhoodType NeighbourType { get; set; } = NeighbourhoodType.Moore;
    public bool UseRandomPattern { get; set; } = true;  // If false, RLE file must be given
    public double Density { get; set; } = 0.3;  // not used if loaded from RLE file
    public string RlePath { get; set; } = "";  // not used if UseRandomPattern = true
    public int FpsRate { get; set; } = 5;
    public SimulationMode StartupMode { get; set; } = SimulationMode.Step;
    public bool ShowHelpScreen { get; set; } = true;

    public static GameSettings LoadFromJson(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Settings file '{filePath}' not found. Using default settings.");
            GameSettings settings = new();
            settings.SaveToJson(filePath);
            return settings;
        }
        return ReadJsonFile(filePath);
    }

    private static GameSettings ReadJsonFile(string filePath)
    {
        try
        {
            Console.WriteLine($"Load settings from '{filePath}'.");
            string content = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<GameSettings>(content, JsonOptions) ?? new GameSettings();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}.\n--> Using default settings.");
            return new GameSettings();
        }
    }

    public void SaveToJson(string filePath)
    {
        try
        {
            Console.WriteLine($"Save settings to '{filePath}'.");
            string jsonText = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(filePath, jsonText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
        }
    }

    public void PrintSettings()
    {
        foreach (var prop in typeof(GameSettings).GetProperties())
        {
            Console.WriteLine($"{prop.Name,-20} {prop.GetValue(this)}");
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        AllowTrailingCommas = true,
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };
}