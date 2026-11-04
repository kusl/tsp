namespace TravelingSalesman.Core;

public sealed class DistanceMatrix
{
    private readonly double[] _values;

    private DistanceMatrix(int size, double[] values)
    {
        Size = size;
        _values = values;
    }

    public int Size { get; }

    public double this[int from, int to]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)from, (uint)Size, nameof(from));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)to, (uint)Size, nameof(to));
            return _values[(from * Size) + to];
        }
    }

    public static DistanceMatrix Create(IReadOnlyList<City> cities)
    {
        ArgumentNullException.ThrowIfNull(cities);

        var size = cities.Count;
        var values = new double[checked(size * size)];

        for (var i = 0; i < size; i++)
        {
            for (var j = i + 1; j < size; j++)
            {
                var distance = cities[i].DistanceTo(cities[j]);
                values[(i * size) + j] = distance;
                values[(j * size) + i] = distance;
            }
        }

        return new DistanceMatrix(size, values);
    }
}
