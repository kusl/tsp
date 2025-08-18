using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reqnroll;
using TravelingSalesman.Core;
using Xunit;

namespace TravelingSalesman.Specs.StepDefinitions
{
    [Binding]
    public class BenchmarkingSteps
    {
        private List<City> _cities = new();
        private IReadOnlyList<TspBenchmark.BenchmarkResult>? _benchmarkResults;
        private List<ITspSolver> _solversToTest = new();

        [Given(@"I have (.*) randomly generated cities for benchmarking")]
        public void GivenIHaveRandomlyGeneratedCities(int count)
        {
            var generator = new TspDataGenerator(seed: 42);
            _cities = generator.GenerateRandomCities(count).ToList();
        }

        [Given(@"I have the following simple cities:")]
        public void GivenIHaveTheFollowingSimpleCities(Table table)
        {
            _cities.Clear();
            int id = 0;
            
            foreach (var row in table.Rows)
            {
                var name = row["Name"];
                var x = double.Parse(row["X"]);
                var y = double.Parse(row["Y"]);
                
                _cities.Add(new City(id++, name, x, y));
            }
        }

        [When(@"I benchmark all available algorithms")]
        public async Task WhenIBenchmarkAllAvailableAlgorithms()
        {
            var benchmark = new TspBenchmark();
            var solvers = TspSolverFactory.CreateAllSolvers().ToList();
            
            _benchmarkResults = await benchmark.RunBenchmarkAsync(_cities, solvers);
        }

        [When(@"I benchmark the following algorithms:")]
        public async Task WhenIBenchmarkTheFollowingAlgorithms(Table table)
        {
            _solversToTest.Clear();
            
            foreach (var row in table.Rows)
            {
                var algorithmName = row["Algorithm"];
                var solver = CreateSolver(algorithmName);
                _solversToTest.Add(solver);
            }
            
            var benchmark = new TspBenchmark();
            _benchmarkResults = await benchmark.RunBenchmarkAsync(_cities, _solversToTest);
        }

        [Then(@"I should receive benchmark results for each algorithm")]
        public void ThenIShouldReceiveBenchmarkResultsForEachAlgorithm()
        {
            Assert.NotNull(_benchmarkResults);
            Assert.Equal(4, _benchmarkResults.Count); // We have 4 algorithms
            
            Assert.All(_benchmarkResults, result =>
            {
                Assert.NotNull(result.SolverName);
                Assert.True(result.Distance > 0);
                Assert.True(result.ExecutionTime.TotalMilliseconds >= 0);
                Assert.NotNull(result.Tour);
            });
        }

        [Then(@"the results should be sorted by distance \(best first\)")]
        public void ThenTheResultsShouldBeSortedByDistanceBestFirst()
        {
            Assert.NotNull(_benchmarkResults);
            
            for (int i = 1; i < _benchmarkResults.Count; i++)
            {
                Assert.True(_benchmarkResults[i].Distance >= _benchmarkResults[i - 1].Distance,
                    $"Results not sorted: {_benchmarkResults[i].Distance} < {_benchmarkResults[i - 1].Distance}");
            }
        }

        [Then(@"each result should include execution time")]
        public void ThenEachResultShouldIncludeExecutionTime()
        {
            Assert.NotNull(_benchmarkResults);
            Assert.All(_benchmarkResults, result =>
            {
                Assert.True(result.ExecutionTime.TotalMilliseconds >= 0);
            });
        }

        [Then(@"the best solution should have a distance of (.*) units")]
        public void ThenTheBestSolutionShouldHaveADistanceOfUnits(double expectedDistance)
        {
            Assert.NotNull(_benchmarkResults);
            Assert.NotEmpty(_benchmarkResults);
            
            var bestResult = _benchmarkResults.First();
            Assert.Equal(expectedDistance, bestResult.Distance, 1);
        }

        [Then(@"all algorithms should find the optimal solution")]
        public void ThenAllAlgorithmsShouldFindTheOptimalSolution()
        {
            Assert.NotNull(_benchmarkResults);
            Assert.NotEmpty(_benchmarkResults);
            
            var optimalDistance = _benchmarkResults.First().Distance;
            
            // For simple 4-city square, optimal is 4.0
            Assert.All(_benchmarkResults, result =>
            {
                Assert.Equal(optimalDistance, result.Distance, 0.1);
            });
        }

        [Then(@"Nearest Neighbor should be the fastest")]
        public void ThenNearestNeighborShouldBeTheFastest()
        {
            Assert.NotNull(_benchmarkResults);
            
            var nnResult = _benchmarkResults.FirstOrDefault(r => r.SolverName == "Nearest Neighbor");
            Assert.NotNull(nnResult);
            
            var fastestTime = _benchmarkResults.Min(r => r.ExecutionTime);
            Assert.Equal(fastestTime, nnResult.ExecutionTime);
        }

        [Then(@"Genetic Algorithm should typically find the best solution")]
        public void ThenGeneticAlgorithmShouldTypicallyFindTheBestSolution()
        {
            Assert.NotNull(_benchmarkResults);
            
            var gaResult = _benchmarkResults.FirstOrDefault(r => r.SolverName == "Genetic Algorithm");
            Assert.NotNull(gaResult);
            
            var bestDistance = _benchmarkResults.Min(r => r.Distance);
            
            // GA should be within 10% of the best solution
            Assert.True(gaResult.Distance <= bestDistance * 1.1,
                $"GA distance {gaResult.Distance} is not within 10% of best {bestDistance}");
        }

        [Then(@"2-Opt should improve upon Nearest Neighbor")]
        public void Then2OptShouldImproveUponNearestNeighbor()
        {
            Assert.NotNull(_benchmarkResults);
            
            var nnResult = _benchmarkResults.FirstOrDefault(r => r.SolverName == "Nearest Neighbor");
            var twoOptResult = _benchmarkResults.FirstOrDefault(r => r.SolverName == "2-Opt");
            
            Assert.NotNull(nnResult);
            Assert.NotNull(twoOptResult);
            
            Assert.True(twoOptResult.Distance <= nnResult.Distance,
                $"2-Opt ({twoOptResult.Distance}) should improve upon NN ({nnResult.Distance})");
        }

        private ITspSolver CreateSolver(string algorithmName)
        {
            return algorithmName switch
            {
                "Nearest Neighbor" => new NearestNeighborSolver(),
                "2-Opt" => new TwoOptSolver(maxIterations: 100),
                "Simulated Annealing" => new SimulatedAnnealingSolver(
                    initialTemperature: 1000,
                    coolingRate: 0.95,
                    iterationsPerTemperature: 50,
                    seed: 42),
                "Genetic Algorithm" => new GeneticAlgorithmSolver(
                    populationSize: 50,
                    generations: 100,
                    mutationRate: 0.05,
                    elitismRate: 0.2,
                    seed: 42),
                _ => throw new NotSupportedException($"Algorithm '{algorithmName}' is not supported")
            };
        }
    }
}
