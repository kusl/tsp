namespace TravelingSalesman.Core.Tests;

public sealed class CityGeneratorTests
{
    [Fact]
    public void Scattered_StaysWithinBoundsWithUniqueNames()
    {
        var cities = CityGenerator.Scattered(200, seed: 42, width: 30, height: 10);

        Assert.Equal(200, cities.Count);
        Assert.All(cities, city => Assert.InRange(city.X, 0, 30));
        Assert.All(cities, city => Assert.InRange(city.Y, 0, 10));
        Assert.Equal(cities.Count, cities.Select(city => city.Name).Distinct().Count());
    }

    [Fact]
    public void Scattered_SameSeed_IsDeterministic() =>
        Assert.Equal(CityGenerator.Scattered(10, seed: 123), CityGenerator.Scattered(10, seed: 123));

    [Fact]
    public void Scattered_DifferentSeeds_Differ() =>
        Assert.NotEqual(CityGenerator.Scattered(10, seed: 1), CityGenerator.Scattered(10, seed: 2));

    [Fact]
    public void Circle_PlacesCitiesOnRadius()
    {
        var center = new City("center", 50, 50);

        Assert.All(CityGenerator.Circle(12, radius: 10), city => Assert.Equal(10.0, city.DistanceTo(center), 9));
    }

    [Fact]
    public void Grid_UsesRegularSpacing()
    {
        var cities = CityGenerator.Grid(3, 4, spacing: 5);

        Assert.Equal(12, cities.Count);
        Assert.Equal(new City("C5", 5, 5), cities[5]);
        Assert.Equal(new City("C11", 15, 10), cities[11]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(10)]
    [InlineData(16)]
    public void GridFor_ProducesRequestedCountOfDistinctPoints(int count)
    {
        var cities = CityGenerator.GridFor(count);

        Assert.Equal(count, cities.Count);
        Assert.Equal(count, cities.Select(city => (city.X, city.Y)).Distinct().Count());
    }

    [Theory]
    [InlineData(CityPattern.Random)]
    [InlineData(CityPattern.Circle)]
    [InlineData(CityPattern.Grid)]
    public void Create_SupportsEveryPattern(CityPattern pattern) =>
        Assert.Equal(9, CityGenerator.Create(pattern, 9, seed: 1).Count);

    [Fact]
    public void Create_UnknownPattern_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => CityGenerator.Create((CityPattern)42, 3));

    [Fact]
    public void NegativeCount_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => CityGenerator.Scattered(-1));
}
