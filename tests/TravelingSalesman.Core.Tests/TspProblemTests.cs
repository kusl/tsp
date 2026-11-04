namespace TravelingSalesman.Core.Tests;

public sealed class TspProblemTests
{
    [Fact]
    public void Constructor_CopiesCities()
    {
        var source = new List<City> { new("A", 0, 0), new("B", 1, 1) };
        var problem = new TspProblem(source);
        source.Add(new City("C", 2, 2));

        Assert.Equal(2, problem.Count);
        Assert.Equal(2, problem.Distances.Size);
    }

    [Fact]
    public void Constructor_NullSequence_Throws() => Assert.Throws<ArgumentNullException>(() => new TspProblem(null!));

    [Fact]
    public void Constructor_NullCity_Throws() =>
        Assert.Throws<ArgumentException>(() => new TspProblem([new City("A", 0, 0), null!]));

    [Fact]
    public void Constructor_EmptySequence_IsAllowed() => Assert.Empty(TestProblems.Empty().Cities);
}
