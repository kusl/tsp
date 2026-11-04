using TravelingSalesman.Cli.Rendering;
using TravelingSalesman.Core;

namespace TravelingSalesman.Cli.Tests;

public sealed class RenderingTests
{
    private static readonly TspProblem Square = new(
    [
        new City("A", 0, 0),
        new City("B", 10, 0),
        new City("C", 10, 10),
        new City("D", 0, 10),
    ]);

    [Fact]
    public void FormatRoute_FullRoute_ReturnsToStart() =>
        Assert.Equal("A -> B -> C -> D -> A", new Tour(Square, [0, 1, 2, 3]).FormatRoute());

    [Fact]
    public void FormatRoute_Preview_ReportsHiddenStops() =>
        Assert.Equal("A -> B -> ... (2 more) -> A", new Tour(Square, [0, 1, 2, 3]).FormatRoute(2));

    [Fact]
    public void FormatRoute_EmptyTour() =>
        Assert.Equal("(empty)", new Tour(new TspProblem([]), []).FormatRoute());

    [Fact]
    public void AsciiPlot_HasRequestedSizeAndMarksCorners()
    {
        var rows = AsciiPlot.Render(new Tour(Square, [0, 1, 2, 3]), width: 11, height: 5);

        Assert.Equal(5, rows.Count);
        Assert.All(rows, row => Assert.True(row.Length <= 11));
        Assert.Equal(AsciiPlot.Start, rows[4][0]);
        Assert.Equal(AsciiPlot.Stop, rows[4][10]);
        Assert.Equal(AsciiPlot.Stop, rows[0][0]);
        Assert.Equal(AsciiPlot.Stop, rows[0][10]);
        Assert.Equal(AsciiPlot.Path, rows[4][5]);
    }

    [Fact]
    public void AsciiPlot_SingleCity_IsCentered()
    {
        var rows = AsciiPlot.Render(new Tour(new TspProblem([new City("A", 3, 3)]), [0]), width: 9, height: 3);

        Assert.Equal(AsciiPlot.Start, rows[1][4]);
    }
}
