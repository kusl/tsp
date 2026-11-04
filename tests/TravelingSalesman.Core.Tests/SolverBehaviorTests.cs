using TravelingSalesman.Core.Solvers;

namespace TravelingSalesman.Core.Tests;

public sealed class SolverBehaviorTests
{
    [Fact]
    public void NearestNeighbor_BreaksTiesByLowestIndex()
    {
        var tour = new NearestNeighborSolver().Solve(TestProblems.Square(), null, TestContext.Current.CancellationToken);

        Assert.Equal(new[] { 0, 1, 2, 3 }, tour.Order);
    }

    [Fact]
    public void TwoOpt_ResultIsLocallyOptimal()
    {
        var problem = TestProblems.Random(60, seed: 11);
        var tour = new TwoOptSolver().Solve(problem, null, TestContext.Current.CancellationToken);
        var order = tour.Order.ToArray();

        for (var start = 1; start < order.Length - 1; start++)
        {
            for (var end = start + 1; end < order.Length; end++)
            {
                Assert.True(TourMath.TwoOptDelta(order, problem.Distances, start, end) >= -1e-9);
            }
        }
    }

    [Fact]
    public void TwoOpt_ConvexPoints_FindsOptimalTour()
    {
        const int count = 16;
        var shuffled = CityGenerator.Circle(count).ToArray();
        new Random(5).Shuffle(shuffled);
        var problem = new TspProblem(shuffled);
        var perimeter = count * 2 * 50 * Math.Sin(Math.PI / count);

        var tour = new TwoOptSolver().Solve(problem, null, TestContext.Current.CancellationToken);

        Assert.Equal(perimeter, tour.Distance, 6);
    }

    [Fact]
    public void TwoOpt_SinglePass_StillReturnsValidTour()
    {
        var problem = TestProblems.Random(40);

        var tour = new TwoOptSolver(new TwoOptOptions { MaxPasses = 1 }).Solve(problem, null, TestContext.Current.CancellationToken);

        Assert.True(TestProblems.IsPermutation(tour.Order, problem.Count));
    }

    [Fact]
    public void SimulatedAnnealing_FinalTemperatureAboveInitial_ReturnsGreedyTour()
    {
        var problem = TestProblems.Random(20);
        var options = new SimulatedAnnealingOptions { InitialTemperature = 1, FinalTemperature = 2, Seed = 3 };

        var tour = new SimulatedAnnealingSolver(options).Solve(problem, null, TestContext.Current.CancellationToken);
        var greedy = new NearestNeighborSolver().Solve(problem, null, TestContext.Current.CancellationToken);

        Assert.Equal(greedy.Order, tour.Order);
    }

    [Fact]
    public void GeneticAlgorithm_StopsEarlyWhenStagnant()
    {
        var problem = TestProblems.Circle(8);
        var progress = new RecordingProgress();
        var options = new GeneticAlgorithmOptions { Generations = 1_000, StagnationLimit = 5, Seed = 9 };

        new GeneticAlgorithmSolver(options).Solve(problem, progress, TestContext.Current.CancellationToken);

        Assert.True(progress.Reports.Count < 1_000);
    }

    [Fact]
    public void GeneticAlgorithm_ImprovesOnRandomRoutes()
    {
        var problem = TestProblems.Random(30, seed: 99);
        var options = GeneticAlgorithmOptions.ForCityCount(problem.Count, seed: 4);

        var tour = new GeneticAlgorithmSolver(options).Solve(problem, null, TestContext.Current.CancellationToken);
        var identity = new Tour(problem, Enumerable.Range(0, problem.Count));

        Assert.True(tour.Distance < identity.Distance);
    }
}
