using TravelingSalesman.Core.Solvers;

namespace TravelingSalesman.Core.Tests;

public sealed class SolverContractTests
{
    public static TheoryData<string> Algorithms
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var key in SolverCatalog.Keys)
            {
                data.Add(key);
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(Algorithms))]
    public void Solve_ReturnsPermutationStartingAtFirstCity(string algorithm)
    {
        var problem = TestProblems.Random(30);

        var tour = Create(algorithm, problem).Solve(problem, null, TestContext.Current.CancellationToken);

        Assert.True(TestProblems.IsPermutation(tour.Order, problem.Count));
        Assert.Equal(0, tour.Order[0]);
        Assert.Same(problem, tour.Problem);
    }

    [Theory]
    [MemberData(nameof(Algorithms))]
    public void Solve_Square_FindsPerimeter(string algorithm)
    {
        var problem = TestProblems.Square();

        var tour = Create(algorithm, problem).Solve(problem, null, TestContext.Current.CancellationToken);

        Assert.Equal(4.0, tour.Distance, 9);
    }

    [Theory]
    [MemberData(nameof(Algorithms))]
    public void Solve_TinyProblems_ReturnsValidTours(string algorithm)
    {
        for (var count = 0; count <= 3; count++)
        {
            var problem = TestProblems.Random(count);

            var tour = Create(algorithm, problem).Solve(problem, null, TestContext.Current.CancellationToken);

            Assert.True(TestProblems.IsPermutation(tour.Order, count));
        }
    }

    [Theory]
    [MemberData(nameof(Algorithms))]
    public void Solve_NeverWorseThanNearestNeighbor(string algorithm)
    {
        var problem = TestProblems.Random(40, seed: 2026);
        var greedy = new NearestNeighborSolver().Solve(problem, null, TestContext.Current.CancellationToken);

        var tour = Create(algorithm, problem).Solve(problem, null, TestContext.Current.CancellationToken);

        Assert.True(tour.Distance <= greedy.Distance + 1e-9, $"{algorithm}: {tour.Distance} > {greedy.Distance}");
    }

    [Theory]
    [MemberData(nameof(Algorithms))]
    public void Solve_SameSeed_IsDeterministic(string algorithm)
    {
        var problem = TestProblems.Random(25);

        var first = Create(algorithm, problem).Solve(problem, null, TestContext.Current.CancellationToken);
        var second = Create(algorithm, problem).Solve(problem, null, TestContext.Current.CancellationToken);

        Assert.Equal(first.Order, second.Order);
        Assert.Equal(first.Distance, second.Distance);
    }

    [Theory]
    [MemberData(nameof(Algorithms))]
    public void Solve_ReportsProgressThatNeverGetsWorse(string algorithm)
    {
        var problem = TestProblems.Random(20);
        var progress = new RecordingProgress();

        var tour = Create(algorithm, problem).Solve(problem, progress, TestContext.Current.CancellationToken);

        Assert.NotEmpty(progress.Reports);
        Assert.Equal(tour.Distance, progress.Reports[^1].BestDistance, 1e-6);
    }

    [Theory]
    [MemberData(nameof(Algorithms))]
    public void Solve_CancelledToken_Throws(string algorithm)
    {
        var problem = TestProblems.Random(30);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.ThrowsAny<OperationCanceledException>(() => Create(algorithm, problem).Solve(problem, null, cancellation.Token));
    }

    [Theory]
    [MemberData(nameof(Algorithms))]
    public void Solve_NullProblem_Throws(string algorithm) =>
        Assert.Throws<ArgumentNullException>(() => Create(algorithm, TestProblems.Square()).Solve(null!, null, TestContext.Current.CancellationToken));

    [Theory]
    [MemberData(nameof(Algorithms))]
    public void Name_MatchesCatalogDisplayName(string algorithm)
    {
        var descriptor = SolverCatalog.Find(algorithm);

        Assert.Equal(descriptor.DisplayName, descriptor.Create(10).Name);
    }

    private static ITspSolver Create(string algorithm, TspProblem problem) => SolverCatalog.Find(algorithm).Create(problem.Count, seed: 1);
}
