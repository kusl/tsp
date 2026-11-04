using TravelingSalesman.Core.Solvers;

namespace TravelingSalesman.Core.Tests;

public sealed class SolverOptionsTests
{
    [Fact]
    public void TwoOpt_RejectsNonPositivePasses() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new TwoOptOptions { MaxPasses = 0 });

    [Theory]
    [InlineData(0, 0.01, 0.9, 10)]
    [InlineData(100, 0, 0.9, 10)]
    [InlineData(100, 0.01, 0, 10)]
    [InlineData(100, 0.01, 1, 10)]
    [InlineData(100, 0.01, 0.9, 0)]
    public void SimulatedAnnealing_RejectsInvalidValues(double initial, double final, double cooling, int iterations) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new SimulatedAnnealingOptions
        {
            InitialTemperature = initial,
            FinalTemperature = final,
            CoolingRate = cooling,
            IterationsPerTemperature = iterations,
        });

    [Theory]
    [InlineData(1, 10, 0.1, 0.1, 2, 10)]
    [InlineData(10, 0, 0.1, 0.1, 2, 10)]
    [InlineData(10, 10, -0.1, 0.1, 2, 10)]
    [InlineData(10, 10, 1.1, 0.1, 2, 10)]
    [InlineData(10, 10, 0.1, 1, 2, 10)]
    [InlineData(10, 10, 0.1, 0.1, 0, 10)]
    [InlineData(10, 10, 0.1, 0.1, 2, 0)]
    public void GeneticAlgorithm_RejectsInvalidValues(int population, int generations, double mutation, double elite, int tournament, int stagnation) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeneticAlgorithmOptions
        {
            PopulationSize = population,
            Generations = generations,
            MutationRate = mutation,
            EliteFraction = elite,
            TournamentSize = tournament,
            StagnationLimit = stagnation,
        });

    [Theory]
    [InlineData(1, 100)]
    [InlineData(50, 1_000)]
    [InlineData(10_000, 20_000)]
    public void SimulatedAnnealing_ForCityCount_ClampsIterations(int cityCount, int expected) =>
        Assert.Equal(expected, SimulatedAnnealingOptions.ForCityCount(cityCount).IterationsPerTemperature);

    [Theory]
    [InlineData(1, 50, 100)]
    [InlineData(60, 120, 1_200)]
    [InlineData(5_000, 200, 2_000)]
    public void GeneticAlgorithm_ForCityCount_ClampsParameters(int cityCount, int population, int generations)
    {
        var options = GeneticAlgorithmOptions.ForCityCount(cityCount, seed: 3);

        Assert.Equal(population, options.PopulationSize);
        Assert.Equal(generations, options.Generations);
        Assert.Equal(3, options.Seed);
    }

    [Fact]
    public void Options_SupportValueEquality() =>
        Assert.Equal(new GeneticAlgorithmOptions { Seed = 1 }, new GeneticAlgorithmOptions { Seed = 1 });
}
