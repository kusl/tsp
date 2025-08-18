Feature: Algorithm Benchmarking
    As a researcher or developer
    I want to compare different TSP algorithms
    So that I can choose the best algorithm for my use case

@benchmark
Scenario: Run benchmark on small problem
    Given I have 10 randomly generated cities for benchmarking
    When I benchmark all available algorithms
    Then I should receive benchmark results for each algorithm
    And the results should be sorted by distance (best first)
    And each result should include execution time

@benchmark
Scenario: Benchmark identifies best solution on simple problem
    Given I have the following simple cities:
        | Name | X | Y |
        | A    | 0 | 0 |
        | B    | 1 | 0 |
        | C    | 1 | 1 |
        | D    | 0 | 1 |
    When I benchmark all available algorithms
    Then the best solution should have a distance of 4.0 units
    And all algorithms should find the optimal solution

@benchmark @algorithm-characteristics
Scenario: Algorithm characteristics validation
    Given I have 15 randomly generated cities for benchmarking
    When I benchmark the following algorithms:
        | Algorithm           |
        | Nearest Neighbor    |
        | 2-Opt              |
        | Simulated Annealing |
        | Genetic Algorithm   |
    Then Nearest Neighbor should be among the fastest algorithms
    And 2-Opt should produce same or better solution than Nearest Neighbor
    And advanced algorithms should produce competitive solutions

@benchmark @performance @small-problem
Scenario: Quick algorithm comparison on small dataset
    Given I have 8 randomly generated cities for benchmarking
    When I benchmark all available algorithms
    Then all algorithms should find good solutions within 20% of optimal
    And Nearest Neighbor should complete in under 10 milliseconds
    And all algorithms should complete in under 1 second

@benchmark @quality @larger-problem
Scenario: Solution quality on moderate problem
    Given I have 25 randomly generated cities for benchmarking
    When I benchmark the following algorithms:
        | Algorithm           |
        | 2-Opt              |
        | Simulated Annealing |
        | Genetic Algorithm   |
    Then each algorithm should find a valid tour
    And the best solution should be better than a random tour