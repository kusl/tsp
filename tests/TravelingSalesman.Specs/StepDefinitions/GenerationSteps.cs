using TravelingSalesman.Specs.Drivers;

namespace TravelingSalesman.Specs.StepDefinitions;

[Binding]
public sealed class GenerationSteps(SolverWorld world)
{
    private const int Precision = 9;

    [When("I generate {int} random cities with seed {int}")]
    [When("I generate {int} random cities again with seed {int}")]
    public void WhenIGenerateRandomCities(int count, int seed) => world.GeneratedSets.Add(CityGenerator.Scattered(count, seed));

    [When("I generate {int} cities on a circle of radius {double}")]
    public void WhenIGenerateACircle(int count, double radius) => world.GeneratedSets.Add(CityGenerator.Circle(count, radius));

    [When("I generate a {int} by {int} grid with spacing {double}")]
    public void WhenIGenerateAGrid(int rows, int columns, double spacing) =>
        world.GeneratedSets.Add(CityGenerator.Grid(rows, columns, spacing));

    [Then("there are {int} cities")]
    public void ThenThereAreCities(int expected) => Assert.Equal(expected, world.LatestGenerated.Count);

    [Then("every city lies between {double} and {double} on both axes")]
    public void ThenEveryCityLiesWithin(double minimum, double maximum) =>
        Assert.All(world.LatestGenerated, city =>
        {
            Assert.InRange(city.X, minimum, maximum);
            Assert.InRange(city.Y, minimum, maximum);
        });

    [Then("every city has a unique name")]
    public void ThenEveryCityHasAUniqueName() =>
        Assert.Equal(world.LatestGenerated.Count, world.LatestGenerated.Select(static city => city.Name).Distinct().Count());

    [Then("both city sets are identical")]
    public void ThenBothCitySetsAreIdentical()
    {
        Assert.True(world.GeneratedSets.Count >= 2, "Two city sets must be generated.");
        Assert.Equal(world.GeneratedSets[^2], world.GeneratedSets[^1]);
    }

    [Then("every city is {double} units from {double}, {double}")]
    public void ThenEveryCityIsAtDistance(double distance, double x, double y)
    {
        var center = new City("center", x, y);
        Assert.All(world.LatestGenerated, city => Assert.Equal(distance, city.DistanceTo(center), Precision));
    }

    [Then("the closest pair of cities is {double} units apart")]
    public void ThenTheClosestPairIs(double expected)
    {
        var cities = world.LatestGenerated;
        var closest = cities
            .SelectMany((city, index) => cities.Skip(index + 1).Select(other => city.DistanceTo(other)))
            .Min();

        Assert.Equal(expected, closest, Precision);
    }
}
