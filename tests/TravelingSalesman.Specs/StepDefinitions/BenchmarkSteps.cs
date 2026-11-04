using TravelingSalesman.Core.Benchmarking;
using TravelingSalesman.Core.Solvers;
using TravelingSalesman.Specs.Drivers;

namespace TravelingSalesman.Specs.StepDefinitions;

[Binding]
public sealed class BenchmarkSteps(SolverWorld world)
{
    [When("I benchmark all algorithms")]
    public void WhenIBenchmarkAllAlgorithms()
    {
        var solvers = SolverCatalog.All.Select(descriptor => descriptor.Create(world.Problem.Count, seed: 1));
        world.BenchmarkResults = new BenchmarkRunner().Run(world.Problem, solvers, TestContext.Current.CancellationToken);
    }

    [Then("there is one result per algorithm")]
    public void ThenThereIsOneResultPerAlgorithm() =>
        Assert.Equal(
            SolverCatalog.All.Select(static descriptor => descriptor.DisplayName).Order(),
            world.BenchmarkResults.Select(static result => result.SolverName).Order());

    [Then("the results are ordered from shortest to longest")]
    public void ThenTheResultsAreOrdered() =>
        Assert.Equal(
            world.BenchmarkResults.Select(static result => result.Distance).Order(),
            world.BenchmarkResults.Select(static result => result.Distance));

    [Then("every result records its running time")]
    public void ThenEveryResultRecordsItsRunningTime() =>
        Assert.All(world.BenchmarkResults, static result => Assert.True(result.Elapsed >= TimeSpan.Zero));

    [Then("every result is no longer than the {string} result")]
    public void ThenEveryResultIsNoLongerThan(string algorithm)
    {
        var baseline = ResultFor(SolverCatalog.Find(algorithm).DisplayName).Distance;
        Assert.All(world.BenchmarkResults, result => Assert.True(result.Distance <= baseline + 1e-9, result.SolverName));
    }

    [Then("every result has a distance of {double} units")]
    public void ThenEveryResultHasADistanceOf(double expected) =>
        Assert.All(world.BenchmarkResults, result => Assert.Equal(expected, result.Distance, 9));

    private BenchmarkResult ResultFor(string solverName) =>
        world.BenchmarkResults.SingleOrDefault(result => result.SolverName == solverName)
        ?? throw new InvalidOperationException($"No benchmark result for '{solverName}'.");
}
