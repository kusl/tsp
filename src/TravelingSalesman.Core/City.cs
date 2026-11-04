namespace TravelingSalesman.Core;

public sealed record City(string Name, double X, double Y)
{
    public string Name { get; } = string.IsNullOrWhiteSpace(Name)
        ? throw new ArgumentException("A city name is required.", nameof(Name))
        : Name;

    public double X { get; } = double.IsFinite(X)
        ? X
        : throw new ArgumentOutOfRangeException(nameof(X), X, "Coordinates must be finite.");

    public double Y { get; } = double.IsFinite(Y)
        ? Y
        : throw new ArgumentOutOfRangeException(nameof(Y), Y, "Coordinates must be finite.");

    public double DistanceTo(City other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return double.Hypot(X - other.X, Y - other.Y);
    }
}
