namespace TravelingSalesman.Core.Tests;

internal sealed class SteppingTimeProvider(TimeSpan step) : TimeProvider
{
    private long _ticks;

    public override long TimestampFrequency => TimeSpan.TicksPerSecond;

    public override long GetTimestamp() => _ticks += step.Ticks;
}
