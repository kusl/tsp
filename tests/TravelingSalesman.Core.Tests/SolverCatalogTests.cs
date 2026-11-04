using TravelingSalesman.Core.Solvers;

namespace TravelingSalesman.Core.Tests;

public sealed class SolverCatalogTests
{
    [Fact]
    public void Keys_AreUniqueAndMatchDescriptors()
    {
        Assert.Equal(SolverCatalog.All.Count, SolverCatalog.Keys.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(SolverCatalog.All.Select(descriptor => descriptor.Key), SolverCatalog.Keys);
    }

    [Theory]
    [InlineData("two-opt")]
    [InlineData("TWO-OPT")]
    public void Find_IsCaseInsensitive(string key) => Assert.Same(SolverCatalog.TwoOpt, SolverCatalog.Find(key));

    [Fact]
    public void Find_UnknownKey_Throws() => Assert.Throws<ArgumentException>(() => SolverCatalog.Find("brute-force"));

    [Fact]
    public void TryFind_UnknownKey_ReturnsFalse()
    {
        Assert.False(SolverCatalog.TryFind("brute-force", out var descriptor));
        Assert.Null(descriptor);
    }

    [Fact]
    public void Create_ReturnsNewInstances() =>
        Assert.NotSame(SolverCatalog.GeneticAlgorithm.Create(10), SolverCatalog.GeneticAlgorithm.Create(10));
}
