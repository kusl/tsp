using System.CommandLine;
using TravelingSalesman.Cli.Commands;
using TravelingSalesman.Cli.Rendering;

namespace TravelingSalesman.Cli;

internal static class CliApplication
{
    public const string Description = "Solve Traveling Salesman Problem instances with classic heuristics and metaheuristics.";

    public static RootCommand CreateRootCommand(TextWriter output, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(timeProvider);

        var renderer = new ConsoleRenderer(output);
        var root = new RootCommand(Description);
        root.Subcommands.Add(SolveCommand.Create(renderer, timeProvider));
        root.Subcommands.Add(BenchmarkCommand.Create(renderer, timeProvider));
        root.Subcommands.Add(AlgorithmsCommand.Create(renderer));
        return root;
    }

    public static Task<int> Execute(ConsoleRenderer renderer, Action action, CancellationToken cancellationToken)
    {
        try
        {
            action();
            return Task.FromResult(ExitCodes.Success);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            renderer.Cancelled();
            return Task.FromResult(ExitCodes.Cancelled);
        }
    }
}
