Feature: TSP Solver Algorithms
    As a user of the TSP solver system
    I want to solve traveling salesman problems using different algorithms
    So that I can find optimal or near-optimal routes through cities

Background:
    Given I have the following cities:
        | Name | X  | Y  |
        | A    | 0  | 0  |
        | B    | 3  | 0  |
        | C    | 3  | 4  |
        | D    | 0  | 4  |

@smoke @solver
Scenario: Nearest Neighbor solver finds a valid tour
    When I solve the TSP using Nearest Neighbor algorithm
    Then the tour should visit all 4 cities
    And the tour should return to the starting city
    And the total distance should be greater than 0

@solver
Scenario Outline: Different algorithms produce valid tours
    When I solve the TSP using <Algorithm> algorithm
    Then the tour should visit all 4 cities
    And the tour should return to the starting city
    And the total distance should be between 10 and 20 units

    Examples:
        | Algorithm           |
        | Nearest Neighbor    |
        | 2-Opt              |
        | Simulated Annealing |
        | Genetic Algorithm   |

@solver @performance
Scenario: 2-Opt improves upon Nearest Neighbor solution
    Given I have solved the TSP using Nearest Neighbor algorithm
    When I apply 2-Opt optimization
    Then the optimized tour distance should be less than or equal to the initial distance

@solver @large
Scenario: Solvers handle large problem instances
    Given I have 50 randomly generated cities for the TSP solver
    When I solve the TSP using Nearest Neighbor algorithm
    Then the solution should complete within 1 second
    And the tour should visit all 50 cities

@solver @deterministic
Scenario: Nearest Neighbor produces deterministic results
    When I solve the TSP using Nearest Neighbor algorithm
    And I solve the same problem again using Nearest Neighbor algorithm
    Then both solutions should have the same total distance
    And both solutions should have the same route
