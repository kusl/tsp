using System.CommandLine;
using TravelingSalesman.Cli.Rendering;
using TravelingSalesman.Core.Benchmarking;
using TravelingSalesman.Core.Solvers;

namespace TravelingSalesman.Cli.Commands;

internal static class BenchmarkCommand
{
    public static Command Create(ConsoleRenderer renderer, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(timeProvider);

        var problemOptions = new ProblemOptions();

        var algorithms = new Option<string[]>("--algorithm", "-a")
        {
            Description = "Algorithms to compare. Defaults to all of them.",
            AllowMultipleArgumentsPerToken = true,
            DefaultValueFactory = static _ => [.. SolverCatalog.Keys],
        };
        algorithms.AcceptOnlyFromAmong([.. SolverCatalog.Keys]);

        var command = new Command("benchmark", "Generate a problem and compare algorithms on it.");
        problemOptions.AddTo(command);
        command.Options.Add(algorithms);

        command.SetAction((parseResult, cancellationToken) => CliApplication.Execute(renderer, () =>
        {
            var problem = problemOptions.CreateProblem(parseResult);
            var seed = parseResult.GetValue(problemOptions.Seed);
            var keys = parseResult.GetValue(algorithms) is { Length: > 0 } selected ? selected : [.. SolverCatalog.Keys];
            var solvers = keys
                .Distinct(StringComparer.Ordinal)
                .Select(key => SolverCatalog.Find(key).Create(problem.Count, seed));

            var results = new BenchmarkRunner(timeProvider).Run(problem, solvers, cancellationToken);
            renderer.Benchmark(results, problem.Count);
        }, cancellationToken));

        return command;
    }
}
