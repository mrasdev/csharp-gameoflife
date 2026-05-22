using GameOfLife.CellurlarRules;
using GameOfLife.Neighbourhoods;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameOfLife;

public enum SimulationMode
{
    Step,
    Slow,  // 1 Hz
    Fast,  // 100 Hz
    Max
}

internal class GameSettings
{
    // set default values for all properties, so that the user doesn't have to specify all of them in the JSON file
    public int Width { get; set; } = 80;
    public int Height { get; set; } = 40;
    public bool Toroidal { get; set; } = true;
    public CellularRuleType RuleType { get; set; } = CellularRuleType.Conway;
    public NeighbourhoodType NeighbourType { get; set; } = NeighbourhoodType.Moore;
    public bool UseRandomPattern { get; set; } = true;  // If false, Rle file must be given
    public double Density { get; set; } = 0.3;
    public string RlePath { get; set; } = "";
    public int FpsRate { get; set; } = 5;
    public SimulationMode StartupMode { get; set; } = SimulationMode.Step;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        AllowTrailingCommas = true,
        WriteIndented = true
    };

    public static GameSettings LoadFromJson(string filePath)
    {
        try
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
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading settings from '{filePath}': {ex.Message}. Using default settings.");
            return new GameSettings();
        }
    }

    private static GameSettings ReadJsonFile(string filePath)
    {
        string content = File.ReadAllText(filePath);
        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNameCaseInsensitive = true
        };
        Console.WriteLine($"Settings loaded from '{filePath}'.");
        return JsonSerializer.Deserialize<GameSettings>(content, options) ?? new GameSettings();
    }

    public void SaveToJson(string filePath)
    {
        try
        {
            string jsonText = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(filePath, jsonText);
            Console.WriteLine($"Settings saved to '{filePath}'.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving settings to '{filePath}': {ex.Message}");
        }
    }

    public void PrintSettings()
    {
        foreach (var prop in typeof(GameSettings).GetProperties())
        {
            Console.WriteLine($"{prop.Name,-20} {prop.GetValue(this)}");
        }
    }
}