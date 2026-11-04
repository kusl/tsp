namespace TravelingSalesman.Core.Tests;

internal static class TestProblems
{
    public static TspProblem Square(double side = 1) => new(
    [
        new City("A", 0, 0),
        new City("B", side, 0),
        new City("C", side, side),
        new City("D", 0, side),
    ]);

    public static TspProblem Random(int count, int seed = 42) => new(CityGenerator.Scattered(count, seed));

    public static TspProblem Circle(int count) => new(CityGenerator.Circle(count));

    public static TspProblem Empty() => new([]);

    public static bool IsPermutation(IReadOnlyList<int> order, int count) =>
        order.Count == count && order.Order().SequenceEqual(Enumerable.Range(0, count));
}
