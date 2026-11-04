Feature: Solving the traveling salesman problem
  As a user of the solver
  I want every algorithm to produce a valid round trip
  So that I can pick the trade-off between speed and quality

  Background:
    Given the following cities:
      | Name | X | Y |
      | A    | 0 | 0 |
      | B    | 3 | 0 |
      | C    | 3 | 4 |
      | D    | 0 | 4 |

  @solver
  Scenario Outline: Every algorithm finds the optimal rectangle tour
    When I solve the problem with the "<algorithm>" algorithm
    Then the "<algorithm>" tour visits every city exactly once
    And the "<algorithm>" tour starts at city "A"
    And the "<algorithm>" tour distance is 14.0 units

    Examples:
      | algorithm           |
      | nearest-neighbor    |
      | two-opt             |
      | simulated-annealing |
      | genetic             |

  @solver @quality
  Scenario: 2-opt never makes the greedy tour worse
    Given 60 random cities with seed 7
    When I solve the problem with the "nearest-neighbor" algorithm
    And I solve the problem with the "two-opt" algorithm
    Then the "two-opt" tour is no longer than the "nearest-neighbor" tour

  @solver @deterministic
  Scenario Outline: Seeded solvers are reproducible
    Given 30 random cities with seed 5
    When I solve the problem twice with the "<algorithm>" algorithm and seed 11
    Then both tours are identical

    Examples:
      | algorithm           |
      | simulated-annealing |
      | genetic             |
