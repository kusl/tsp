namespace TravelingSalesman.Core;

public sealed class Tour
{
    public Tour(TspProblem problem, IEnumerable<int> order)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(order);

        int[] indices = [.. order];
        if (!IsPermutation(indices, problem.Count))
        {
            throw new ArgumentException("The order must visit every city of the problem exactly once.", nameof(order));
        }

        Problem = problem;
        Order = Array.AsReadOnly(indices);
        Cities = Array.AsReadOnly(Array.ConvertAll(indices, index => problem.Cities[index]));
        Distance = TourMath.Length(indices, problem.Distances);
    }

    public TspProblem Problem { get; }

    public IReadOnlyList<int> Order { get; }

    public IReadOnlyList<City> Cities { get; }

    public double Distance { get; }

    public override string ToString() =>
        Cities.Count == 0
            ? "(empty tour)"
            : string.Join(" -> ", Cities.Select(static city => city.Name).Append(Cities[0].Name));

    private static bool IsPermutation(int[] order, int count)
    {
        if (order.Length != count)
        {
            return false;
        }

        var seen = new bool[count];
        foreach (var index in order)
        {
            if ((uint)index >= (uint)count || seen[index])
            {
                return false;
            }

            seen[index] = true;
        }

        return true;
    }
}
