namespace TravelingSalesman.Core.Tests;

public sealed class TourTests
{
    [Fact]
    public void Distance_IncludesReturnToStart()
    {
        var tour = new Tour(TestProblems.Square(), [0, 1, 2, 3]);

        Assert.Equal(4.0, tour.Distance, 10);
    }

    [Fact]
    public void Distance_CrossingRoute_IsLonger()
    {
        var problem = TestProblems.Square();

        Assert.True(new Tour(problem, [0, 2, 1, 3]).Distance > new Tour(problem, [0, 1, 2, 3]).Distance);
    }

    [Fact]
    public void Cities_FollowOrder()
    {
        var tour = new Tour(TestProblems.Square(), [0, 3, 2, 1]);

        Assert.Equal(new[] { "A", "D", "C", "B" }, tour.Cities.Select(city => city.Name));
    }

    [Fact]
    public void EmptyProblem_HasZeroDistance()
    {
        var tour = new Tour(TestProblems.Empty(), []);

        Assert.Equal(0.0, tour.Distance);
        Assert.Empty(tour.Cities);
        Assert.Equal("(empty tour)", tour.ToString());
    }

    [Fact]
    public void SingleCity_HasZeroDistance()
    {
        var tour = new Tour(new TspProblem([new City("A", 5, 5)]), [0]);

        Assert.Equal(0.0, tour.Distance);
    }

    [Theory]
    [InlineData(new[] { 0, 1, 2 })]
    [InlineData(new[] { 0, 1, 2, 2 })]
    [InlineData(new[] { 0, 1, 2, 4 })]
    [InlineData(new[] { 0, 1, 2, -1 })]
    [InlineData(new[] { 0, 1, 2, 3, 0 })]
    public void Constructor_InvalidOrder_Throws(int[] order) =>
        Assert.Throws<ArgumentException>(() => new Tour(TestProblems.Square(), order));

    [Fact]
    public void Constructor_CopiesOrder()
    {
        int[] order = [0, 1, 2, 3];
        var tour = new Tour(TestProblems.Square(), order);
        order[1] = 3;

        Assert.Equal(new[] { 0, 1, 2, 3 }, tour.Order);
    }

    [Fact]
    public void ToString_ListsRouteAndReturnsToStart() =>
        Assert.Equal("A -> B -> C -> D -> A", new Tour(TestProblems.Square(), [0, 1, 2, 3]).ToString());
}
