using System.CommandLine;
using TravelingSalesman.Cli.Rendering;
using TravelingSalesman.Core.Solvers;

namespace TravelingSalesman.Cli.Commands;

internal static class SolveCommand
{
    public static Command Create(ConsoleRenderer renderer, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(timeProvider);

        var problemOptions = new ProblemOptions();

        var algorithm = new Option<string>("--algorithm", "-a")
        {
            Description = "Algorithm used to solve the problem.",
            DefaultValueFactory = static _ => SolverCatalog.TwoOpt.Key,
        };
        algorithm.AcceptOnlyFromAmong([.. SolverCatalog.Keys]);

        var route = new Option<bool>("--route")
        {
            Description = "Print the full route instead of a preview.",
        };

        var plot = new Option<bool>("--plot")
        {
            Description = "Draw the route as an ASCII plot.",
        };

        var command = new Command("solve", "Generate a problem and solve it with one algorithm.");
        problemOptions.AddTo(command);
        command.Options.Add(algorithm);
        command.Options.Add(route);
        command.Options.Add(plot);

        command.SetAction((parseResult, cancellationToken) => CliApplication.Execute(renderer, () =>
        {
            var problem = problemOptions.CreateProblem(parseResult);
            var descriptor = SolverCatalog.Find(parseResult.GetValue(algorithm) ?? SolverCatalog.TwoOpt.Key);
            var solver = descriptor.Create(problem.Count, parseResult.GetValue(problemOptions.Seed));

            var started = timeProvider.GetTimestamp();
            var tour = solver.Solve(problem, progress: null, cancellationToken);
            var elapsed = timeProvider.GetElapsedTime(started);

            renderer.Solution(tour, solver.Name, elapsed, parseResult.GetValue(route), parseResult.GetValue(plot));
        }, cancellationToken));

        return command;
    }
}
