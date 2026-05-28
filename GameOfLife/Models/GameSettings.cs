// Read from JSON file and provide settings for game. If file does not exist a new one is created.
// Return default settings in case of error. IRL you should replace WriteLines by any external handling.

using GameOfLife.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameOfLife.Models;

internal class GameSettings
{
    // set default values for all properties, so that the user doesn't have to specify all of them in the JSON file
    public int Width
    {  // not relevant if grid is loaded from RLE file
        get => _width;
        set => _width = Math.Abs(value); 
    }  
    public int Height
    {  // not relevant if grid is loaded from RLE file
        get => _height; 
        set => _height = Math.Abs(value);
    } 
    public bool Toroidal { get; set; } = true;
    public CellularRuleType RuleType { get; set; } = CellularRuleType.Conway;
    public NeighbourhoodType NeighbourType { get; set; } = NeighbourhoodType.Moore;
    public bool UseRandomPattern { get; set; } = true;  // If false, RLE file must be given
    public double Density
    {  // not relevant if grid is loaded from RLE file
        get => _density;
        set => _density = Math.Clamp(value, 0.0, 1.0);
    }
    public string RlePath { get; set; } = String.Empty;  // not used if UseRandomPattern = true
    public int FpsRate
    {  // not relevant if grid is loaded from RLE file
        get => _fpsRate;
        set => _fpsRate = Math.Max(1, value);
    }
    public SimulationMode StartupMode { get; set; } = SimulationMode.Fast;
    public bool ShowHelpScreen { get; set; } = true;

    private int _width = 160;
    private int _height = 40;
    private double _density = 0.3;
    private int _fpsRate = 10;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        AllowTrailingCommas = true,
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

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
}