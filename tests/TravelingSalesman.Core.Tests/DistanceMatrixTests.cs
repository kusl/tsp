namespace TravelingSalesman.Core.Tests;

public sealed class DistanceMatrixTests
{
    [Fact]
    public void Create_ProducesSymmetricMatrixWithZeroDiagonal()
    {
        var problem = TestProblems.Random(12);
        var matrix = problem.Distances;

        Assert.Equal(12, matrix.Size);
        for (var i = 0; i < matrix.Size; i++)
        {
            Assert.Equal(0.0, matrix[i, i]);
            for (var j = 0; j < matrix.Size; j++)
            {
                Assert.Equal(matrix[i, j], matrix[j, i]);
                Assert.Equal(problem.Cities[i].DistanceTo(problem.Cities[j]), matrix[i, j], 12);
            }
        }
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, 4)]
    [InlineData(4, 0)]
    public void Indexer_OutOfRange_Throws(int from, int to)
    {
        var matrix = TestProblems.Square().Distances;

        Assert.Throws<ArgumentOutOfRangeException>(() => matrix[from, to]);
    }

    [Fact]
    public void Create_Null_Throws() => Assert.Throws<ArgumentNullException>(() => DistanceMatrix.Create(null!));
}
