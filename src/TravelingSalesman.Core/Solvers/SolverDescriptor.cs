namespace TravelingSalesman.Core.Solvers;

public sealed record SolverDescriptor(
    string Key,
    string DisplayName,
    string Complexity,
    string Summary,
    Func<int, int?, ITspSolver> Factory)
{
    public ITspSolver Create(int cityCount, int? seed = null) => Factory(cityCount, seed);
}
