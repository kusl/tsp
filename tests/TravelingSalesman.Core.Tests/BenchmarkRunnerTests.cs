using TravelingSalesman.Core.Benchmarking;
using TravelingSalesman.Core.Solvers;

namespace TravelingSalesman.Core.Tests;

public sealed class BenchmarkRunnerTests
{
    [Fact]
    public void Run_ReturnsOneResultPerSolverSortedByDistance()
    {
        var problem = TestProblems.Random(25);
        var solvers = SolverCatalog.All.Select(descriptor => descriptor.Create(problem.Count, seed: 1)).ToList();
        var runner = new BenchmarkRunner(new SteppingTimeProvider(TimeSpan.FromMilliseconds(3)));

        var results = runner.Run(problem, solvers, TestContext.Current.CancellationToken);

        Assert.Equal(solvers.Select(solver => solver.Name).Order(), results.Select(result => result.SolverName).Order());
        Assert.Equal(results.OrderBy(result => result.Distance).Select(result => result.Distance), results.Select(result => result.Distance));
        Assert.All(results, result => Assert.Equal(TimeSpan.FromMilliseconds(3), result.Elapsed));
        Assert.All(results, result => Assert.Equal(result.Tour.Distance, result.Distance));
    }

    [Fact]
    public void Run_NoSolvers_ReturnsEmpty() =>
        Assert.Empty(new BenchmarkRunner().Run(TestProblems.Square(), [], TestContext.Current.CancellationToken));

    [Fact]
    public void Run_CancelledToken_Throws()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.ThrowsAny<OperationCanceledException>(() =>
            new BenchmarkRunner().Run(TestProblems.Square(), [new NearestNeighborSolver()], cancellation.Token));
    }

    [Fact]
    public void Run_NullArguments_Throw()
    {
        var runner = new BenchmarkRunner();

        Assert.Throws<ArgumentNullException>(() => runner.Run(null!, [], TestContext.Current.CancellationToken));
        Assert.Throws<ArgumentNullException>(() => runner.Run(TestProblems.Square(), null!, TestContext.Current.CancellationToken));
    }
}
