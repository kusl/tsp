using TravelingSalesman.Core.Solvers;

namespace TravelingSalesman.Cli.Tests;

public sealed class CommandTests
{
    public static TheoryData<string> Algorithms
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var key in SolverCatalog.Keys)
            {
                data.Add(key);
            }

            return data;
        }
    }

    [Fact]
    public async Task Algorithms_ListsEveryAlgorithm()
    {
        var result = await CliRunner.RunAsync("algorithms");

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        Assert.All(SolverCatalog.All, descriptor =>
        {
            Assert.Contains(descriptor.Key, result.Output, StringComparison.Ordinal);
            Assert.Contains(descriptor.DisplayName, result.Output, StringComparison.Ordinal);
        });
    }

    [Theory]
    [MemberData(nameof(Algorithms))]
    public async Task Solve_EachAlgorithm_PrintsSolution(string algorithm)
    {
        var result = await CliRunner.RunAsync("solve", "--cities", "12", "--seed", "7", "--algorithm", algorithm);

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        Assert.Equal(SolverCatalog.Find(algorithm).DisplayName, result.Value("Algorithm"));
        Assert.Equal("12", result.Value("Cities"));
        Assert.NotNull(result.Value("Distance"));
        Assert.StartsWith("C0 -> ", result.Value("Route"), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Solve_WithDefaults_Succeeds()
    {
        var result = await CliRunner.RunAsync("solve");

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        Assert.Equal(SolverCatalog.TwoOpt.DisplayName, result.Value("Algorithm"));
    }

    [Fact]
    public async Task Solve_SameSeed_IsReproducible()
    {
        string[] args = ["solve", "-n", "30", "-s", "3", "-a", "simulated-annealing", "--route"];

        var first = await CliRunner.RunAsync(args);
        var second = await CliRunner.RunAsync(args);

        Assert.Equal(first.Value("Distance"), second.Value("Distance"));
        Assert.Equal(first.Value("Route"), second.Value("Route"));
    }

    [Fact]
    public async Task Solve_RoutePreview_IsTruncatedUnlessRequested()
    {
        var preview = await CliRunner.RunAsync("solve", "-n", "25", "-s", "1");
        var full = await CliRunner.RunAsync("solve", "-n", "25", "-s", "1", "--route");

        Assert.Contains("(15 more)", preview.Value("Route"), StringComparison.Ordinal);
        Assert.DoesNotContain("more", full.Value("Route"), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("random")]
    [InlineData("circle")]
    [InlineData("grid")]
    [InlineData("Grid")]
    public async Task Solve_AcceptsEveryPattern(string pattern)
    {
        var result = await CliRunner.RunAsync("solve", "--pattern", pattern, "--cities", "9");

        Assert.Equal(ExitCodes.Success, result.ExitCode);
    }

    [Fact]
    public async Task Solve_Plot_DrawsStartingCity()
    {
        var result = await CliRunner.RunAsync("solve", "-n", "10", "-s", "2", "--plot");

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        Assert.Matches(@"(?m)^[ .o]*S[ .o]*\r?$", result.Output);
    }

    [Fact]
    public async Task Benchmark_ComparesAllAlgorithmsByDefault()
    {
        var result = await CliRunner.RunAsync("benchmark", "--cities", "15", "--seed", "42");

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        Assert.Contains("Benchmark of 4 algorithms on 15 cities", result.Output, StringComparison.Ordinal);
        Assert.All(SolverCatalog.All, descriptor => Assert.Contains(descriptor.DisplayName, result.Output, StringComparison.Ordinal));
        Assert.NotNull(result.Value("Best"));
    }

    [Fact]
    public async Task Benchmark_RunsOnlySelectedAlgorithms()
    {
        var result = await CliRunner.RunAsync("benchmark", "-n", "10", "-a", "nearest-neighbor", "two-opt");

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        Assert.Contains("Benchmark of 2 algorithms", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain(SolverCatalog.GeneticAlgorithm.DisplayName, result.Output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("solve", "--cities", "1")]
    [InlineData("solve", "--cities", "5001")]
    [InlineData("solve", "--cities", "many")]
    [InlineData("solve", "--pattern", "spiral")]
    [InlineData("solve", "--pattern", "1")]
    [InlineData("solve", "--algorithm", "brute-force")]
    [InlineData("benchmark", "--algorithm", "brute-force")]
    [InlineData("unknown")]
    public async Task InvalidArguments_FailWithoutCrashing(params string[] args)
    {
        var result = await CliRunner.RunAsync(args);

        Assert.NotEqual(ExitCodes.Success, result.ExitCode);
        Assert.NotEmpty(result.Output);
    }

    [Fact]
    public async Task NoCommand_FailsAndShowsHelp()
    {
        var result = await CliRunner.RunAsync();

        Assert.NotEqual(ExitCodes.Success, result.ExitCode);
        Assert.Contains("solve", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Help_DescribesCommands()
    {
        var result = await CliRunner.RunAsync("--help");

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        Assert.Contains("Traveling", result.Output, StringComparison.Ordinal);
        Assert.Contains("benchmark", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Version_PrintsVersion()
    {
        var result = await CliRunner.RunAsync("--version");

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        Assert.Matches(@"^\d+\.\d+\.\d+", result.Output.Trim());
    }
}
