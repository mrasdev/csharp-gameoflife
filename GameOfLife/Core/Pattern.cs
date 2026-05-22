// Create random pattern or read RLE file.
// Random pattern: width and height from settings are used
// RLE file: width and height from file are used and stored in settings
// Naming: Cells = linear array, Grid = Cells + dimensions

using GameOfLife.Models;
using System.Text;

namespace GameOfLife.Core;

public record Placement(
    int Width = 0,      // dimensions of the whole grid
    int Height = 0,
    int StartX = 0,     // where to place the new grid from the RLE file
    int StartY = 0);

public record Grid(
    bool[] Cells,
    int Width,
    int Height);

internal static class Pattern
{
    public static bool[] GetCells(GameSettings settings)
    {
        if (settings.UseRandomPattern)
        {
            return CreateRandom(settings.Width, settings.Height, settings.Density);
        }
        Grid rleGrid = LoadFromRleFile(settings.RlePath);
        settings.Width = rleGrid.Width;
        settings.Height = rleGrid.Height;
        return rleGrid.Cells;
    }

    public static bool[] CreateRandom(int width, int height, double density)
    {
        bool[] cells = new bool[width * height];
        Random rnd = new();
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = rnd.NextDouble() < density;
        }
        return cells;
    }

    // Comment starts with #: e.g. #N Glider
    // Header starts with x or y: e.g. x = 3, y = 3, rule = B3/S23
    // Pattern: o: live, b: dead, $: line break, !: EOF and multiplicators like 42o
    // This method inserts an RLE grid into cells at place.
    public static Grid LoadFromRleFile(string filePath, bool[]? cells, Placement place)
    {
        string[] lines = File.ReadAllLines(filePath);
        StringBuilder sb = new();
        bool isHeaderParsed = false;
        int width = place.Width;
        int height = place.Height;

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith('#')) continue;
            if (!isHeaderParsed && line.StartsWith('x') && line.Contains('y'))
            {
                ParseHeaderDimensions(line, out int headerWidth, out int headerHeight);
                if (width == 0) width = headerWidth;
                if (height == 0) height = headerHeight;
                isHeaderParsed = true;
                continue;
            }
            sb.Append(line);
        }

        if (width == 0) throw new ArgumentException("Width must not be zero");
        if (height == 0) throw new ArgumentException("Height must not be zero");
        cells ??= new bool[width * height];
        if (cells.Length != width * height) throw new ArgumentException("Pattern size mismatch");
        Placement finalPlace = place with { Width = width, Height = height };
        return ExpandRleData(sb.ToString(), cells, finalPlace);
    }
    public static Grid LoadFromRleFile(string filePath)
    // build new pattern from scratch
    {
        return LoadFromRleFile(filePath, cells: null, new Placement());
    }

    private static Grid ExpandRleData(string rleText, bool[] cells, Placement place)
    {
        int currentX = place.StartX;
        int currentY = place.StartY;
        int repeatCount = 0;  // 0 means not yet set
        foreach (char symbol in rleText)
        {
            if (char.IsDigit(symbol))
            {
                repeatCount = repeatCount * 10 + (symbol - '0');  // convert multidigit number
                continue;
            }
            int length = repeatCount == 0 ? 1 : repeatCount;  // set to 1 if no digits have been detected
            repeatCount = 0;  // reset for next symbol
            switch (symbol)
            {
                case 'b':  // dead cell(s)
                    currentX += length;
                    if (currentX >= place.Width)
                        throw new IndexOutOfRangeException($"Dead cells at X = {currentX} exceed the grid width {place.Width}");
                    break;
                case 'o':// living cell(s)
                    for (int r = 0; r < length; r++)
                    {
                        if (currentX >= place.Width || currentY >= place.Height)
                            throw new IndexOutOfRangeException($"({currentX}, {currentY}) is outside [{place.Width}, {place.Height}]");
                        cells[currentY * place.Width + currentX] = true;
                        currentX++;
                    }
                    break;
                case '$':  // linefeed
                    currentY += length;  // there can be multiple linefeeds
                    if (currentY >= place.Height)
                        throw new IndexOutOfRangeException($"Line feeds at Y = {currentY} exceed the grid height {place.Height}");
                    currentX = place.StartX;
                    break;
                case '!': // end of file
                    return new Grid(cells, place.Width, place.Height);
                case '\r':
                case '\n':
                case ' ':
                    break;
                default:
                    throw new FormatException($"Invalid symbol '{symbol}' at position ({currentX}, {currentY})");
            }
        }
        throw new FormatException("No end of file symbol ('!') found");
    }

    private static void ParseHeaderDimensions(string headerLine, out int width, out int height)
    {
        width = 0;
        height = 0;
        string[] tokens = headerLine.Split(",");
        foreach (string token in tokens)
        {
            string clean = token.Replace(" ", "").ToLower();
            if (clean.StartsWith("x="))
            {
                _ = int.TryParse(clean[2..], out width);
            }
            else if (clean.StartsWith("y="))
            {
                _ = int.TryParse(clean[2..], out height);
            }
        }
    }
}
