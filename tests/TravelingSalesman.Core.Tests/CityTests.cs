namespace TravelingSalesman.Core.Tests;

public sealed class CityTests
{
    [Fact]
    public void DistanceTo_ReturnsEuclideanDistance()
    {
        var origin = new City("A", 0, 0);
        var other = new City("B", 3, 4);

        Assert.Equal(5.0, origin.DistanceTo(other), 10);
    }

    [Fact]
    public void DistanceTo_IsSymmetric()
    {
        var first = new City("A", -2.5, 7);
        var second = new City("B", 11, -3.25);

        Assert.Equal(first.DistanceTo(second), second.DistanceTo(first), 12);
    }

    [Fact]
    public void DistanceTo_SameLocation_IsZero()
    {
        var city = new City("A", 10.5, 20.3);

        Assert.Equal(0.0, city.DistanceTo(city with { }));
    }

    [Fact]
    public void DistanceTo_Null_Throws() =>
        Assert.Throws<ArgumentNullException>(() => new City("A", 0, 0).DistanceTo(null!));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_BlankName_Throws(string name) =>
        Assert.Throws<ArgumentException>(() => new City(name, 0, 0));

    [Theory]
    [InlineData(double.NaN, 0)]
    [InlineData(0, double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity, 0)]
    public void Constructor_NonFiniteCoordinate_Throws(double x, double y) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new City("A", x, y));

    [Fact]
    public void Equality_IsValueBased()
    {
        Assert.Equal(new City("A", 1, 2), new City("A", 1, 2));
        Assert.NotEqual(new City("A", 1, 2), new City("B", 1, 2));
    }
}
