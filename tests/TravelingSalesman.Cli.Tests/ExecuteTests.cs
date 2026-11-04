using TravelingSalesman.Cli.Rendering;

namespace TravelingSalesman.Cli.Tests;

public sealed class ExecuteTests
{
    [Fact]
    public async Task Execute_Success_ReturnsZero()
    {
        using var output = new StringWriter();
        var ran = false;

        var exitCode = await CliApplication.Execute(new ConsoleRenderer(output), () => ran = true, TestContext.Current.CancellationToken);

        Assert.True(ran);
        Assert.Equal(ExitCodes.Success, exitCode);
    }

    [Fact]
    public async Task Execute_Cancelled_ReturnsCancelledExitCode()
    {
        using var output = new StringWriter();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var exitCode = await CliApplication.Execute(
            new ConsoleRenderer(output),
            () => cancellation.Token.ThrowIfCancellationRequested(),
            cancellation.Token);

        Assert.Equal(ExitCodes.Cancelled, exitCode);
        Assert.Contains("Cancelled", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Execute_OtherExceptions_Propagate()
    {
        using var output = new StringWriter();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CliApplication.Execute(new ConsoleRenderer(output), static () => throw new InvalidOperationException(), TestContext.Current.CancellationToken));
    }
}
