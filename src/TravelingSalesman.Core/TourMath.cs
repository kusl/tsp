namespace TravelingSalesman.Core;

internal static class TourMath
{
    public const double Epsilon = 1e-10;

    public static double Length(ReadOnlySpan<int> order, DistanceMatrix distances)
    {
        if (order.Length < 2)
        {
            return 0;
        }

        var total = 0.0;
        for (var i = 1; i < order.Length; i++)
        {
            total += distances[order[i - 1], order[i]];
        }

        return total + distances[order[^1], order[0]];
    }

    public static double TwoOptDelta(ReadOnlySpan<int> order, DistanceMatrix distances, int start, int end)
    {
        var before = order[start - 1];
        var first = order[start];
        var last = order[end];
        var after = order[(end + 1) % order.Length];

        return distances[before, last] + distances[first, after]
             - distances[before, first] - distances[last, after];
    }

    public static void Reverse(Span<int> order, int start, int end) => order[start..(end + 1)].Reverse();
}
