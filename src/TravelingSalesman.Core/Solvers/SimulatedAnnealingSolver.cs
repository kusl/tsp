namespace TravelingSalesman.Core.Solvers;

public sealed class SimulatedAnnealingSolver(SimulatedAnnealingOptions? options = null) : ITspSolver
{
    public const string DisplayName = "Simulated Annealing";

    private readonly SimulatedAnnealingOptions _options = options ?? new SimulatedAnnealingOptions();

    public string Name => DisplayName;

    public Tour Solve(TspProblem problem, IProgress<SolverProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);

        var distances = problem.Distances;
        var current = NearestNeighborSolver.BuildOrder(distances, cancellationToken);
        var count = current.Length;

        if (count < 4)
        {
            var trivial = new Tour(problem, current);
            progress?.Report(new SolverProgress(0, trivial.Distance));
            return trivial;
        }

        var random = _options.Seed is { } seed ? new Random(seed) : new Random();
        var best = (int[])current.Clone();
        var currentLength = TourMath.Length(current, distances);
        var bestLength = currentLength;
        var step = 0;

        for (var temperature = _options.InitialTemperature;
             temperature > _options.FinalTemperature;
             temperature *= _options.CoolingRate)
        {
            cancellationToken.ThrowIfCancellationRequested();

            for (var iteration = 0; iteration < _options.IterationsPerTemperature; iteration++)
            {
                var start = random.Next(1, count - 1);
                var end = random.Next(start + 1, count);
                var delta = TourMath.TwoOptDelta(current, distances, start, end);

                if (delta > 0 && random.NextDouble() >= Math.Exp(-delta / temperature))
                {
                    continue;
                }

                TourMath.Reverse(current, start, end);
                currentLength += delta;

                if (currentLength < bestLength - TourMath.Epsilon)
                {
                    bestLength = currentLength;
                    current.CopyTo(best, 0);
                }
            }

            progress?.Report(new SolverProgress(++step, bestLength));
        }

        return new Tour(problem, best);
    }
}
