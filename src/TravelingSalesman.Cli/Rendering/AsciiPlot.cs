using TravelingSalesman.Core;

namespace TravelingSalesman.Cli.Rendering;

internal static class AsciiPlot
{
    public const char Start = 'S';
    public const char Stop = 'o';
    public const char Path = '.';

    public static IReadOnlyList<string> Render(Tour tour, int width = 60, int height = 20)
    {
        ArgumentNullException.ThrowIfNull(tour);
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 2);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 2);

        var canvas = new char[height][];
        for (var row = 0; row < height; row++)
        {
            canvas[row] = new string(' ', width).ToCharArray();
        }

        var cities = tour.Cities;
        if (cities.Count == 0)
        {
            return [.. canvas.Select(static line => new string(line))];
        }

        var minX = cities.Min(static city => city.X);
        var maxX = cities.Max(static city => city.X);
        var minY = cities.Min(static city => city.Y);
        var maxY = cities.Max(static city => city.Y);
        var points = cities
            .Select(city => (Column: Scale(city.X, minX, maxX, width), Row: height - 1 - Scale(city.Y, minY, maxY, height)))
            .ToArray();

        for (var i = 1; i <= points.Length; i++)
        {
            var from = points[i - 1];
            var to = points[i % points.Length];
            DrawLine(canvas, from.Column, from.Row, to.Column, to.Row);
        }

        foreach (var (column, row) in points)
        {
            canvas[row][column] = Stop;
        }

        canvas[points[0].Row][points[0].Column] = Start;
        return [.. canvas.Select(static line => new string(line).TrimEnd())];
    }

    private static int Scale(double value, double minimum, double maximum, int cells)
    {
        var span = maximum - minimum;
        return span <= 0 ? cells / 2 : (int)Math.Round((value - minimum) / span * (cells - 1));
    }

    private static void DrawLine(char[][] canvas, int x0, int y0, int x1, int y1)
    {
        var dx = Math.Abs(x1 - x0);
        var dy = -Math.Abs(y1 - y0);
        var stepX = x0 < x1 ? 1 : -1;
        var stepY = y0 < y1 ? 1 : -1;
        var error = dx + dy;

        while (true)
        {
            canvas[y0][x0] = Path;
            if (x0 == x1 && y0 == y1)
            {
                return;
            }

            var doubled = 2 * error;
            if (doubled >= dy)
            {
                error += dy;
                x0 += stepX;
            }

            if (doubled <= dx)
            {
                error += dx;
                y0 += stepY;
            }
        }
    }
}
