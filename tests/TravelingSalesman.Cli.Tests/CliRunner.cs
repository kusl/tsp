using System.CommandLine;

namespace TravelingSalesman.Cli.Tests;

internal static class CliRunner
{
    public static async Task<CliResult> RunAsync(params string[] args)
    {
        using var output = new StringWriter();
        var configuration = new InvocationConfiguration
        {
            Output = output,
            Error = output,
            ProcessTerminationTimeout = null,
        };

        var exitCode = await CliApplication
            .CreateRootCommand(output, TimeProvider.System)
            .Parse(args)
            .InvokeAsync(configuration, TestContext.Current.CancellationToken);

        return new CliResult(exitCode, output.ToString());
    }
}
