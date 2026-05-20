using GameOfLife.CellurlarRules;
using GameOfLife.Neighbourhoods;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameOfLife;

internal class GameSettings
{
    // set default values for all properties, so that the user doesn't have to specify all of them in the JSON file
    public int Width { get; set; } = 80;
    public int Height { get; set; } = 40;
    public bool Toroidal { get; set; } = true;
    public CellularRuleType RuleType { get; set; } = CellularRuleType.Conway;
    public NeighbourhoodType NeighbourType { get; set; } = NeighbourhoodType.Moore;

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
            string jsonText = File.ReadAllText(filePath);
            Console.WriteLine($"Settings loaded from '{filePath}'.");
            return JsonSerializer.Deserialize<GameSettings>(jsonText, JsonOptions) ?? new GameSettings();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading settings from '{filePath}': {ex.Message}. Using default settings.");
            return new GameSettings();
        }
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