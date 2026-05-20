using GameOfLife.CellurlarRules;
using GameOfLife.Interfaces;
using GameOfLife.Neighbourhoods;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameOfLife
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                AllowTrailingCommas = true
            };

            // 2. JSON-Datei einlesen
            string jsonText = File.ReadAllText("settings.json");
            GameSettings loadedSettings = JsonSerializer.Deserialize<GameSettings>(jsonText, jsonOptions) ?? new GameSettings();

            // 3. Engine mit den geladenen Einstellungen füttern
            SimulationEngine engine = new SimulationEngine(loadedSettings);

            // 4. Spiel starten
            engine.UpdatePattern();
        }
    }
}
