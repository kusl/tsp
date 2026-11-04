Feature: Benchmarking algorithms
  As a researcher
  I want to compare algorithms on the same problem
  So that I can choose the right one for my use case

  @benchmark
  Scenario: Benchmark ranks every algorithm
    Given 25 random cities with seed 42
    When I benchmark all algorithms
    Then there is one result per algorithm
    And the results are ordered from shortest to longest
    And every result records its running time

  @benchmark @quality
  Scenario: Advanced algorithms match or beat the greedy baseline
    Given 40 random cities with seed 2026
    When I benchmark all algorithms
    Then every result is no longer than the "nearest-neighbor" result

  @benchmark
  Scenario: Every algorithm is optimal on a unit square
    Given the following cities:
      | Name | X | Y |
      | A    | 0 | 0 |
      | B    | 1 | 0 |
      | C    | 1 | 1 |
      | D    | 0 | 1 |
    When I benchmark all algorithms
    Then every result has a distance of 4.0 units
