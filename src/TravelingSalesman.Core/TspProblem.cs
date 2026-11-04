namespace TravelingSalesman.Core;

public sealed class TspProblem
{
    public TspProblem(IEnumerable<City> cities)
    {
        ArgumentNullException.ThrowIfNull(cities);

        City[] items = [.. cities];
        if (Array.Exists(items, static city => city is null))
        {
            throw new ArgumentException("Cities must not contain null entries.", nameof(cities));
        }

        Cities = Array.AsReadOnly(items);
        Distances = DistanceMatrix.Create(Cities);
    }

    public IReadOnlyList<City> Cities { get; }

    public DistanceMatrix Distances { get; }

    public int Count => Cities.Count;
}
