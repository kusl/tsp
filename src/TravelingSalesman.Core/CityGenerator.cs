namespace TravelingSalesman.Core;

public static class CityGenerator
{
    public const double DefaultSize = 100;

    public static IReadOnlyList<City> Create(CityPattern pattern, int count, int? seed = null) => pattern switch
    {
        CityPattern.Random => Scattered(count, seed),
        CityPattern.Circle => Circle(count),
        CityPattern.Grid => GridFor(count),
        _ => throw new ArgumentOutOfRangeException(nameof(pattern), pattern, "Unknown city pattern."),
    };

    public static IReadOnlyList<City> Scattered(int count, int? seed = null, double width = DefaultSize, double height = DefaultSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        var random = seed is { } value ? new Random(value) : new Random();
        var cities = new City[count];
        for (var i = 0; i < count; i++)
        {
            cities[i] = new City(NameOf(i), random.NextDouble() * width, random.NextDouble() * height);
        }

        return cities;
    }

    public static IReadOnlyList<City> Circle(int count, double radius = DefaultSize / 2, double centerX = DefaultSize / 2, double centerY = DefaultSize / 2)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(radius);

        var cities = new City[count];
        for (var i = 0; i < count; i++)
        {
            var angle = 2 * Math.PI * i / count;
            cities[i] = new City(NameOf(i), centerX + (radius * Math.Cos(angle)), centerY + (radius * Math.Sin(angle)));
        }

        return cities;
    }

    public static IReadOnlyList<City> GridFor(int count, double spacing = 10)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        var columns = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(count)));
        return [.. Grid(DivideRoundingUp(count, columns), columns, spacing).Take(count)];
    }

    public static IReadOnlyList<City> Grid(int rows, int columns, double spacing = 10)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(rows);
        ArgumentOutOfRangeException.ThrowIfNegative(columns);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(spacing);

        var cities = new City[rows * columns];
        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                var index = (row * columns) + column;
                cities[index] = new City(NameOf(index), column * spacing, row * spacing);
            }
        }

        return cities;
    }

    private static int DivideRoundingUp(int value, int divisor) => (value + divisor - 1) / divisor;

    private static string NameOf(int index) => $"C{index}";
}
