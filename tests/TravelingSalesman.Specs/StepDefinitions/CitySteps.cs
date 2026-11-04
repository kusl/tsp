using System.Globalization;
using TravelingSalesman.Specs.Drivers;

namespace TravelingSalesman.Specs.StepDefinitions;

[Binding]
public sealed class CitySteps(SolverWorld world)
{
    private const int Precision = 9;

    [Given("a city {string} at {double}, {double}")]
    public void GivenACity(string name, double x, double y) => world.UseCities([.. world.Cities, new City(name, x, y)]);

    [Given("the following cities:")]
    public void GivenTheFollowingCities(DataTable table) =>
        world.UseCities(table.Rows.Select(static row => new City(
            row["Name"],
            double.Parse(row["X"], CultureInfo.InvariantCulture),
            double.Parse(row["Y"], CultureInfo.InvariantCulture))));

    [Given("no cities")]
    public void GivenNoCities() => world.UseCities([]);

    [Given("{int} random cities with seed {int}")]
    public void GivenRandomCities(int count, int seed) => world.UseCities(CityGenerator.Scattered(count, seed));

    [When("I measure the distance from {string} to {string}")]
    public void WhenIMeasureTheDistance(string from, string to) => world.Distance = world.FindCity(from).DistanceTo(world.FindCity(to));

    [When("I visit the cities in the order {string}")]
    public void WhenIVisitTheCitiesInOrder(string names)
    {
        var order = names
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(name => world.Cities.IndexOf(world.FindCity(name)));

        world.Tour = new Tour(world.Problem, order);
    }

    [Then("the distance is {double} units")]
    public void ThenTheDistanceIs(double expected) =>
        Assert.Equal(expected, world.Distance ?? throw new InvalidOperationException("No distance was measured."), Precision);

    [Then("the tour distance is {double} units")]
    public void ThenTheTourDistanceIs(double expected) => Assert.Equal(expected, RequiredTour().Distance, Precision);

    [Then("the tour distance is greater than {double} units")]
    public void ThenTheTourDistanceIsGreaterThan(double threshold) => Assert.True(RequiredTour().Distance > threshold);

    private Tour RequiredTour() => world.Tour ?? throw new InvalidOperationException("No tour has been created.");
}
