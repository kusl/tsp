using System.CommandLine;
using TravelingSalesman.Cli.Rendering;
using TravelingSalesman.Core.Solvers;

namespace TravelingSalesman.Cli.Commands;

internal static class AlgorithmsCommand
{
    public static Command Create(ConsoleRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);

        var command = new Command("algorithms", "List the available algorithms.");
        command.SetAction(_ => renderer.Algorithms(SolverCatalog.All));
        return command;
    }
}
