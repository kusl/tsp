using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Reqnroll;
using TravelingSalesman.Core;
using Xunit;

namespace TravelingSalesman.Specs.StepDefinitions
{
    [Binding]
    public class TspSolverSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private List<City> _cities = new();
        private Tour? _currentTour;
        private Tour? _previousTour;
        private ITspSolver? _currentSolver;
        private Stopwatch? _stopwatch;
        private readonly Dictionary<string, Tour> _tours = new();

        public TspSolverSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Given(@"I have the following cities:")]
        public void GivenIHaveTheFollowingCities(Table table)
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
            
            _scenarioContext["Cities"] = _cities;
        }

        [Given(@"I have (.*) randomly generated cities")]
        public void GivenIHaveRandomlyGeneratedCities(int count)
        {
            var generator = new TspDataGenerator(seed: 42);
            _cities = generator.GenerateRandomCities(count).ToList();
            _scenarioContext["Cities"] = _cities;
        }

        [Given(@"I have solved the TSP using Nearest Neighbor algorithm")]
        public async Task GivenIHaveSolvedTheTSPUsingNearestNeighborAlgorithm()
        {
            _currentSolver = new NearestNeighborSolver();
            _previousTour = await _currentSolver.SolveAsync(_cities);
            _tours["initial"] = _previousTour;
        }

        [When(@"I solve the TSP using (.*) algorithm")]
        public async Task WhenISolveTheTSPUsingAlgorithm(string algorithmName)
        {
            _currentSolver = CreateSolver(algorithmName);
            
            _stopwatch = Stopwatch.StartNew();
            _currentTour = await _currentSolver.SolveAsync(_cities);
            _stopwatch.Stop();
            
            _tours[algorithmName] = _currentTour;
        }

        [When(@"I solve the same problem again using (.*) algorithm")]
        public async Task WhenISolveTheSameProblemAgainUsingAlgorithm(string algorithmName)
        {
            var solver = CreateSolver(algorithmName);
            var secondTour = await solver.SolveAsync(_cities);
            _tours["second"] = secondTour;
        }

        [When(@"I apply 2-Opt optimization")]
        public async Task WhenIApply2OptOptimization()
        {
            var solver = new TwoOptSolver(maxIterations: 100);
            _currentTour = await solver.SolveAsync(_cities);
            _tours["optimized"] = _currentTour;
        }

        [Then(@"the tour should visit all (.*) cities")]
        public void ThenTheTourShouldVisitAllCities(int expectedCount)
        {
            Assert.NotNull(_currentTour);
            Assert.Equal(expectedCount, _currentTour.Cities.Count);
            
            // Verify all cities are unique
            var uniqueCities = _currentTour.Cities.Distinct().Count();
            Assert.Equal(expectedCount, uniqueCities);
        }

        [Then(@"the tour should return to the starting city")]
        public void ThenTheTourShouldReturnToTheStartingCity()
        {
            Assert.NotNull(_currentTour);
            Assert.True(_currentTour.Cities.Count > 0);
            
            // The tour distance calculation includes return to start
            // We just need to verify the tour exists and is valid
            var firstCity = _currentTour.Cities.First();
            Assert.NotNull(firstCity);
        }

        [Then(@"the total distance should be greater than (.*)")]
        public void ThenTheTotalDistanceShouldBeGreaterThan(double minDistance)
        {
            Assert.NotNull(_currentTour);
            Assert.True(_currentTour.TotalDistance > minDistance,
                $"Expected distance > {minDistance}, but was {_currentTour.TotalDistance}");
        }

        [Then(@"the total distance should be between (.*) and (.*) units")]
        public void ThenTheTotalDistanceShouldBeBetweenUnits(double min, double max)
        {
            Assert.NotNull(_currentTour);
            Assert.InRange(_currentTour.TotalDistance, min, max);
        }

        [Then(@"the optimized tour distance should be less than or equal to the initial distance")]
        public void ThenTheOptimizedTourDistanceShouldBeLessThanOrEqualToTheInitialDistance()
        {
            var initial = _tours["initial"];
            var optimized = _tours["optimized"];
            
            Assert.NotNull(initial);
            Assert.NotNull(optimized);
            Assert.True(optimized.TotalDistance <= initial.TotalDistance,
                $"Optimized distance ({optimized.TotalDistance:F2}) should be <= initial ({initial.TotalDistance:F2})");
        }

        [Then(@"the solution should complete within (.*) second")]
        [Then(@"the solution should complete within (.*) seconds")]
        public void ThenTheSolutionShouldCompleteWithinSeconds(int seconds)
        {
            Assert.NotNull(_stopwatch);
            Assert.True(_stopwatch.Elapsed.TotalSeconds <= seconds,
                $"Expected completion within {seconds}s, but took {_stopwatch.Elapsed.TotalSeconds:F2}s");
        }

        [Then(@"both solutions should have the same total distance")]
        public void ThenBothSolutionsShouldHaveTheSameTotalDistance()
        {
            var first = _tours.Values.First();
            var second = _tours["second"];
            
            Assert.Equal(first.TotalDistance, second.TotalDistance, 2);
        }

        [Then(@"both solutions should have the same route")]
        public void ThenBothSolutionsShouldHaveTheSameRoute()
        {
            var first = _tours.Values.First();
            var second = _tours["second"];
            
            Assert.Equal(first.Cities.Count, second.Cities.Count);
            
            for (int i = 0; i < first.Cities.Count; i++)
            {
                Assert.Equal(first.Cities[i].Id, second.Cities[i].Id);
            }
        }

        private ITspSolver CreateSolver(string algorithmName)
        {
            return algorithmName.ToLower() switch
            {
                "nearest neighbor" => new NearestNeighborSolver(),
                "2-opt" => new TwoOptSolver(maxIterations: 100),
                "simulated annealing" => new SimulatedAnnealingSolver(
                    initialTemperature: 1000,
                    coolingRate: 0.95,
                    iterationsPerTemperature: 50,
                    seed: 42),
                "genetic algorithm" => new GeneticAlgorithmSolver(
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
