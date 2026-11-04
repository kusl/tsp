namespace TravelingSalesman.Core.Solvers;

public sealed class TwoOptSolver(TwoOptOptions? options = null) : ITspSolver
{
    public const string DisplayName = "2-Opt";

    private readonly TwoOptOptions _options = options ?? new TwoOptOptions();

    public string Name => DisplayName;

    public Tour Solve(TspProblem problem, IProgress<SolverProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);

        var order = NearestNeighborSolver.BuildOrder(problem.Distances, cancellationToken);
        Optimize(order, problem.Distances, _options.MaxPasses, progress, cancellationToken);
        return new Tour(problem, order);
    }

    internal static void Optimize(
        Span<int> order,
        DistanceMatrix distances,
        int maxPasses,
        IProgress<SolverProgress>? progress,
        CancellationToken cancellationToken)
    {
        var count = order.Length;
        var length = TourMath.Length(order, distances);

        if (count < 4)
        {
            progress?.Report(new SolverProgress(0, length));
            return;
        }

        for (var pass = 1; pass <= maxPasses; pass++)
        {
            var improved = false;

            for (var start = 1; start < count - 1; start++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                for (var end = start + 1; end < count; end++)
                {
                    var delta = TourMath.TwoOptDelta(order, distances, start, end);
                    if (delta < -TourMath.Epsilon)
                    {
                        TourMath.Reverse(order, start, end);
                        length += delta;
                        improved = true;
                    }
                }
            }

            progress?.Report(new SolverProgress(pass, length));

            if (!improved)
            {
                return;
            }
        }
    }
}
