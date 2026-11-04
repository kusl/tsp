namespace TravelingSalesman.Cli.Tests;

internal sealed record CliResult(int ExitCode, string Output)
{
    public string? Value(string label) =>
        Output.Split('\n')
            .Select(static line => line.TrimEnd('\r'))
            .Where(line => line.StartsWith(label, StringComparison.Ordinal))
            .Select(line => line[(line.IndexOf(':', StringComparison.Ordinal) + 1)..].Trim())
            .FirstOrDefault();
}
