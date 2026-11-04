# tsp

[![CI](https://github.com/kusl/tsp/actions/workflows/ci.yml/badge.svg)](https://github.com/kusl/tsp/actions/workflows/ci.yml)
[![Release](https://github.com/kusl/tsp/actions/workflows/release.yml/badge.svg)](https://github.com/kusl/tsp/actions/workflows/release.yml)
[![License: AGPL v3](https://img.shields.io/badge/License-AGPL_v3-blue.svg)](LICENSE.txt)

A Traveling Salesman Problem solver for the command line, written in C# 14 on .NET 10 and shipped as a single Native AOT binary.

It generates a problem (random, circle or grid layouts), solves it with one of four algorithms and prints the route, or benchmarks every algorithm against the same problem.

## Quick start

Install the latest release on Linux (x64 or arm64):

```bash
curl -fsSL https://raw.githubusercontent.com/kusl/tsp/master/scripts/install.sh | sudo bash
kusl-tsp benchmark --cities 30 --seed 42
```

The installer verifies the binary against the release `SHA256SUMS`, installs it to `/opt/kusl-tsp/tsp` and links it as `/usr/local/bin/kusl-tsp`. Binaries for Linux, macOS and Windows are attached to every [release](https://github.com/kusl/tsp/releases/latest).

Or run it in a container with Podman:

```bash
podman build --file Containerfile --tag localhost/tsp .
podman run --rm localhost/tsp solve --cities 20 --algorithm genetic --plot
```

## Usage

```text
tsp solve      [--cities N] [--pattern random|circle|grid] [--seed S] [--algorithm KEY] [--route] [--plot]
tsp benchmark  [--cities N] [--pattern random|circle|grid] [--seed S] [--algorithm KEY ...]
tsp algorithms
tsp --help | --version
```

Running `tsp` (or the container image) without a command prints the help and exits with code 1.

| Option | Default | Description |
|---|---|---|
| `-n`, `--cities` | `20` | Number of cities, 2 to 5000 |
| `-p`, `--pattern` | `random` | City layout |
| `-s`, `--seed` | random | Seed for reproducible layouts and solver runs |
| `-a`, `--algorithm` | `two-opt` (solve), all (benchmark) | Algorithm key, see below |
| `--route` | off | Print the whole route instead of the first ten stops |
| `--plot` | off | Draw the route as an ASCII plot |

Example:

```text
$ tsp benchmark --cities 25 --seed 42
Benchmark of 4 algorithms on 25 cities

Rank  Algorithm                  Distance   vs best   Time (ms)
---------------------------------------------------------------
1     Simulated Annealing          402.13    0.00 %        41.7
2     Genetic Algorithm            402.13    0.00 %        35.2
3     2-Opt                        419.66    4.36 %         0.3
4     Nearest Neighbor             478.25   18.93 %         0.1

Best: Simulated Annealing (402.13)
```

The numbers above are illustrative; exact values depend on the seed.

## Algorithms

| Key | Algorithm | Cost | Notes |
|---|---|---|---|
| `nearest-neighbor` | Nearest Neighbor | O(n²) | Greedy construction, deterministic |
| `two-opt` | 2-Opt | O(n²) per pass | Starts from Nearest Neighbor, reverses segments until no improving move remains |
| `simulated-annealing` | Simulated Annealing | O(steps × iterations) | Random 2-opt moves with a geometric cooling schedule |
| `genetic` | Genetic Algorithm | O(generations × population × n) | Tournament selection, order crossover, inversion mutation, elitism, early stop on stagnation |

Every solver starts from city 0, honours cancellation (Ctrl+C exits with code 130) and is reproducible when a seed is given. 2-Opt, Simulated Annealing and the Genetic Algorithm are seeded with the Nearest Neighbor tour, so they never return anything longer than it.

## Development

Requirements: the .NET 10 SDK (`sudo dnf install dotnet-sdk-10.0` on Fedora). Optional: `clang zlib-ng-compat-devel` for Native AOT, `podman` for the container image, `ShellCheck` for script linting.

Everything runs through bash scripts in `scripts/`, which need no execute bit. One command runs the whole pipeline locally:

```bash
time bash scripts/ci.sh
```

| Script | Purpose |
|---|---|
| `ci.sh` | Lint, build, test, smoke test, run, publish, container and export, in that order. Accepts `--clean`, `--skip-publish`, `--skip-container`, `--skip-export` |
| `lint.sh` | `bash -n` and ShellCheck over every script |
| `build.sh` | Restore and build the solution in Release |
| `test.sh` | Run every test project on Microsoft Testing Platform with Cobertura coverage into `artifacts/test-results` |
| `smoke.sh` | Exercise the CLI end to end; pass a command (a binary or `podman run --rm image`) to smoke test something else |
| `run.sh` | Run the CLI; defaults to `benchmark --cities 25 --pattern random --seed 42` |
| `publish.sh [rid]` | Native AOT publish to `artifacts/release/tsp-<rid>` and smoke test it when it runs on this machine |
| `container.sh` | Build the `Containerfile` with Podman (or Docker) and smoke test the image |
| `export.sh` | Write every tracked source file to `docs/llm/dump.txt` |
| `install.sh` | Install the latest release binary |
| `version.sh` | Print the version derived from the commit date and count |
| `checksums.sh [dir]` | Write `SHA256SUMS` for release binaries |

The GitHub workflows only call these scripts, so a green local `ci.sh` means a green CI.

## Layout

```text
src/TravelingSalesman.Core      Domain model, solvers, generators and benchmark runner (no dependencies)
src/TravelingSalesman.Cli       The tsp command line, built on System.CommandLine
tests/TravelingSalesman.Core.Tests   xUnit v3 unit tests
tests/TravelingSalesman.Cli.Tests    xUnit v3 tests that run the CLI in-process
tests/TravelingSalesman.Specs        Reqnroll scenarios on xUnit v3
scripts/                        Build, test, release and maintenance scripts
```

Design notes:

- `ITspSolver` is the only abstraction the CLI and benchmark depend on. Algorithms register in `SolverCatalog`; adding one does not touch any command.
- Solvers work on index permutations over a precomputed `DistanceMatrix`, report progress through `IProgress<SolverProgress>` and keep no state between calls.
- `Tour` and `TspProblem` validate their input and are immutable.
- Time is measured through `TimeProvider`, so benchmarks are testable.
- Options are validated records (C# 14 `field` accessors). Formatting uses C# 14 extension members.
- The build uses `tsp.slnx`, central package management, `UseArtifactsOutput`, `TreatWarningsAsErrors` and the latest analyzers. `global.json` pins the .NET 10 SDK band and opts `dotnet test` into Microsoft Testing Platform.

## License

[GNU Affero General Public License v3.0](LICENSE.txt).

This project includes code written with the help of large language models, including Claude. All of it is reviewed and covered by tests.
