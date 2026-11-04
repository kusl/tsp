using TravelingSalesman.Core.Solvers;

namespace TravelingSalesman.Core.Benchmarking;

public sealed class BenchmarkRunner(TimeProvider? timeProvider = null)
{
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    public IReadOnlyList<BenchmarkResult> Run(TspProblem problem, IEnumerable<ITspSolver> solvers, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(solvers);

        var results = new List<BenchmarkResult>();
        foreach (var solver in solvers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var started = _timeProvider.GetTimestamp();
            var tour = solver.Solve(problem, progress: null, cancellationToken);
            results.Add(new BenchmarkResult(solver.Name, tour, _timeProvider.GetElapsedTime(started)));
        }

        return [.. results.OrderBy(static result => result.Distance).ThenBy(static result => result.Elapsed)];
    }
}
