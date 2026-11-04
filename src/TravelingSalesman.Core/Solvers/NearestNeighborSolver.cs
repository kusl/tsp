namespace TravelingSalesman.Core.Solvers;

public sealed class NearestNeighborSolver : ITspSolver
{
    public const string DisplayName = "Nearest Neighbor";

    public string Name => DisplayName;

    public Tour Solve(TspProblem problem, IProgress<SolverProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);

        var tour = new Tour(problem, BuildOrder(problem.Distances, cancellationToken));
        progress?.Report(new SolverProgress(1, tour.Distance));
        return tour;
    }

    internal static int[] BuildOrder(DistanceMatrix distances, CancellationToken cancellationToken)
    {
        var count = distances.Size;
        var order = new int[count];
        if (count == 0)
        {
            return order;
        }

        var visited = new bool[count];
        visited[0] = true;

        for (var step = 1; step < count; step++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var current = order[step - 1];
            var nearest = -1;
            var nearestDistance = double.PositiveInfinity;

            for (var candidate = 0; candidate < count; candidate++)
            {
                if (visited[candidate])
                {
                    continue;
                }

                var distance = distances[current, candidate];
                if (distance < nearestDistance)
                {
                    nearest = candidate;
                    nearestDistance = distance;
                }
            }

            order[step] = nearest;
            visited[nearest] = true;
        }

        return order;
    }
}
