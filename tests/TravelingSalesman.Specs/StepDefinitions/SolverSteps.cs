using TravelingSalesman.Core.Solvers;
using TravelingSalesman.Specs.Drivers;

namespace TravelingSalesman.Specs.StepDefinitions;

[Binding]
public sealed class SolverSteps(SolverWorld world)
{
    private const int DefaultSeed = 1;

    [When("I solve the problem with the {string} algorithm")]
    public void WhenISolve(string algorithm) => world.Tours[algorithm] = Solve(algorithm, DefaultSeed);

    [When("I solve the problem twice with the {string} algorithm and seed {int}")]
    public void WhenISolveTwice(string algorithm, int seed)
    {
        world.RepeatedTours.Add(Solve(algorithm, seed));
        world.RepeatedTours.Add(Solve(algorithm, seed));
    }

    [Then("the {string} tour visits every city exactly once")]
    public void ThenTheTourVisitsEveryCity(string algorithm) =>
        Assert.Equal(Enumerable.Range(0, world.Problem.Count), TourFor(algorithm).Order.Order());

    [Then("the {string} tour starts at city {string}")]
    public void ThenTheTourStartsAt(string algorithm, string city) => Assert.Equal(city, TourFor(algorithm).Cities[0].Name);

    [Then("the {string} tour distance is {double} units")]
    public void ThenTheTourDistanceIs(string algorithm, double expected) => Assert.Equal(expected, TourFor(algorithm).Distance, 9);

    [Then("the {string} tour is no longer than the {string} tour")]
    public void ThenTheTourIsNoLongerThan(string candidate, string baseline) =>
        Assert.True(TourFor(candidate).Distance <= TourFor(baseline).Distance + 1e-9);

    [Then("both tours are identical")]
    public void ThenBothToursAreIdentical()
    {
        Assert.Equal(2, world.RepeatedTours.Count);
        Assert.Equal(world.RepeatedTours[0].Order, world.RepeatedTours[1].Order);
    }

    private Tour Solve(string algorithm, int seed) =>
        SolverCatalog.Find(algorithm)
            .Create(world.Problem.Count, seed)
            .Solve(world.Problem, null, TestContext.Current.CancellationToken);

    private Tour TourFor(string algorithm) =>
        world.Tours.TryGetValue(algorithm, out var tour)
            ? tour
            : throw new InvalidOperationException($"The problem has not been solved with '{algorithm}'.");
}
