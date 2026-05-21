using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace GameOfLife;

internal class PatternFactory
{
    public static bool[] CreateRandom(int width, int height, double density)
    {
        bool[] cells = new bool[width * height];
        Random rnd = new Random();
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = rnd.NextDouble() < density;
        }
        return cells;
    }

    public static bool[] LoadFromRleFile(string filePath, bool[]? cells, int width, int height, int startX, int startY)
    // Comment starts with #: #N Glider
    // Header starts with x or y: x = 3, y = 3, rule = B3/S23
    // Pattern: o: live, b: dead, $: line break, !: EOF and multiplicators like 42o
    {
        string[] lines = File.ReadAllLines(filePath);
        StringBuilder sb = new();
        bool headerFound = false;

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith('#')) continue;
            if (!headerFound && line.Contains('x') && line.Contains('y'))
            {
                ParseRleHeader(line, out width, out height);
                headerFound = true;
                continue;
            }
            sb.Append(line);
        }
        if (width == 0) throw new ArgumentNullException(nameof(width));
        if (height == 0) throw new ArgumentNullException(nameof(height));
        cells ??= new bool[width * height];
        if (cells.Length != width * height) throw new ArgumentException("Pattern size mismatch");
        return ParseRleGrid(sb.ToString(), cells, width, height, startX, startY);
    }
    public static bool[] LoadFromRleFile(string filePath)
    // build new pattern from scratch
    {
        return LoadFromRleFile(filePath, cells: null, width: 0, height: 0, startX: 0, startY: 0);
    }

    public static bool[] LoadFromRleFile(string filePath, bool[] cells, int width, int height)
    // replace existing pattern
    {
        return LoadFromRleFile(filePath, cells, width, height, startX: 0, startY: 0);
    }

    private static bool[] ParseRleGrid(string gridData, bool[] cells, int width, int height, int startX, int startY)
    {
        int currentX = startX;
        int currentY = startY;
        int runCount = 0;  // 0 means not yet set
        for (int i = 0; i < gridData.Length; i++)
        {
            char c = gridData[i];
            if (char.IsDigit(c))
            {
                runCount = runCount * 10 + (c - '0');  // convert multidigit number
                continue;
            }
            int actualRun = runCount == 0 ? 1 : runCount;
            runCount = 0;  // reset for next character
            if (c == 'b') currentX += actualRun;  // dead cell(s)
            else if (c == 'o')  // living cell(s)
            {
                for (int r = 0; r < actualRun; r++)
                {
                    if (currentX < width && currentY < height)
                        cells[currentY * width + currentX] = true;
                    else throw new IndexOutOfRangeException($"({currentX}, {currentY}) is outside [{width}, {height}]");
                    currentX++;
                }
            }
            else if (c == '$')  // linefeed
            {
                currentY += actualRun;  // there can be multiple linefeeds
                currentX = startX;
            }
            else if (c == '!')  // end of file
                break;
        }
        return cells;
    }

    private static void ParseRleHeader(string headerLine, out int width, out int height)
    {
        width = 0;
        height = 0;
        string[] parts = headerLine.Split(",");
        foreach (string part in parts)
        {
            string clean = part.Replace(" ", "").ToLower();
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
