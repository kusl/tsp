namespace TravelingSalesman.Core.Tests;

public sealed class TourMathTests
{
    [Fact]
    public void TwoOptDelta_MatchesLengthChangeOfReversal()
    {
        var problem = TestProblems.Random(15, seed: 7);
        var order = Enumerable.Range(0, problem.Count).ToArray();

        for (var start = 1; start < order.Length - 1; start++)
        {
            for (var end = start + 1; end < order.Length; end++)
            {
                var before = TourMath.Length(order, problem.Distances);
                var delta = TourMath.TwoOptDelta(order, problem.Distances, start, end);
                var reversed = (int[])order.Clone();
                TourMath.Reverse(reversed, start, end);

                Assert.Equal(before + delta, TourMath.Length(reversed, problem.Distances), 9);
            }
        }
    }

    [Fact]
    public void Reverse_ReversesInclusiveSegment()
    {
        int[] order = [0, 1, 2, 3, 4, 5];

        TourMath.Reverse(order, 1, 4);

        Assert.Equal(new[] { 0, 4, 3, 2, 1, 5 }, order);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Length_FewerThanTwoCities_IsZero(int count)
    {
        var problem = TestProblems.Random(count);

        Assert.Equal(0.0, TourMath.Length(Enumerable.Range(0, count).ToArray(), problem.Distances));
    }
}
