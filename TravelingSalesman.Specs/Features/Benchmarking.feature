Feature: Algorithm Benchmarking
    As a researcher or developer
    I want to compare different TSP algorithms
    So that I can choose the best algorithm for my use case

@benchmark
Scenario: Run benchmark on small problem
    Given I have 10 randomly generated cities
    When I benchmark all available algorithms
    Then I should receive benchmark results for each algorithm
    And the results should be sorted by distance (best first)
    And each result should include execution time

@benchmark
Scenario: Benchmark identifies best solution
    Given I have the following simple cities:
        | Name | X | Y |
        | A    | 0 | 0 |
        | B    | 1 | 0 |
        | C    | 1 | 1 |
        | D    | 0 | 1 |
    When I benchmark all available algorithms
    Then the best solution should have a distance of 4.0 units
    And all algorithms should find the optimal solution

@benchmark @performance
Scenario: Algorithm performance ranking
    Given I have 15 randomly generated cities
    When I benchmark the following algorithms:
        | Algorithm           |
        | Nearest Neighbor    |
        | 2-Opt              |
        | Simulated Annealing |
        | Genetic Algorithm   |
    Then Nearest Neighbor should be the fastest
    And Genetic Algorithm should typically find the best solution
    And 2-Opt should improve upon Nearest Neighbor
