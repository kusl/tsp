# TSP Solver

[![Build Status](https://github.com/kusl/tsp/actions/workflows/build.yml/badge.svg)](https://github.com/kusl/tsp/actions/workflows/build.yml)
[![Release](https://github.com/kusl/tsp/actions/workflows/release.yml/badge.svg)](https://github.com/kusl/tsp/actions/workflows/release.yml)
[![License: AGPL v3](https://img.shields.io/badge/License-AGPL%20v3-blue.svg)](https://www.gnu.org/licenses/agpl-3.0)
[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/9.0)

A high-performance, production-ready Traveling Salesman Problem (TSP) solver written in .NET 9, featuring multiple optimization algorithms, comprehensive logging, native AOT compilation, and cross-platform support.

## 🌟 Key Features

### Core Capabilities
- **🧠 Multiple Algorithms**: Four distinct TSP solving approaches with different trade-offs
- **⚡ Native AOT**: Single-file executable with no runtime dependencies
- **📊 Comprehensive Logging**: Structured logging with Serilog (console + rotating file logs)
- **🔄 Cross-Platform**: Windows, Linux, macOS (x64 & ARM64)
- **🐳 Docker Ready**: Multi-stage optimized container builds
- **✅ Well-Tested**: 99 unit tests + BDD scenarios with Reqnroll

### Solver Features
- **Interactive Mode**: User-friendly console interface
- **Benchmark Suite**: Side-by-side algorithm comparison
- **Visual Demonstration**: Step-by-step algorithm visualization
- **Progress Tracking**: Real-time algorithm progress reporting
- **Cancellation Support**: Graceful interruption of long-running operations
- **Multiple City Patterns**: Random, circular, and grid generation

## 🚀 Quick Start

### Option 1: Download Pre-built Binaries

Get the latest release from [GitHub Releases](https://github.com/kusl/tsp/releases/latest):

| Platform | Binary | Size |
|----------|--------|------|
| Windows x64 | `TSP-win-x64-{sha}.exe` | ~3-5 MB |
| Linux x64 | `TSP-linux-x64-{sha}` | ~3-5 MB |
| macOS Intel | `TSP-osx-x64-{sha}` | ~3-5 MB |
| macOS ARM64 | `TSP-osx-arm64-{sha}` | ~3-5 MB |

```bash
# Linux/macOS
chmod +x TSP-linux-x64-*
./TSP-linux-x64-*

# Windows
TSP-win-x64-*.exe
```

### Option 2: Docker

```bash
# Build and run
docker build -t tsp-solver .
docker run --rm -it tsp-solver

# With persistent logs
docker run --rm -it -v $(pwd)/logs:/app/logs tsp-solver

# Benchmark mode (non-interactive)
echo -e "2\n15\n5\n" | docker run --rm -i tsp-solver
```

### Option 3: Build from Source

```bash
# Clone
git clone https://github.com/kusl/tsp.git
cd tsp

# Build & Test
dotnet build TSP.sln
dotnet test TSP.sln

# Run
dotnet run --project TravelingSalesman.ConsoleApp

# Publish native AOT
dotnet publish TravelingSalesman.ConsoleApp \
  -c Release \
  -r linux-x64 \
  --self-contained \
  -p:PublishAot=true
```

## 📊 Algorithm Comparison

| Algorithm | Time Complexity | Solution Quality | Speed | Best For |
|-----------|----------------|------------------|-------|----------|
| **Nearest Neighbor** | O(n²) | ★★★☆☆ | ⚡⚡⚡⚡⚡ | Real-time, initial solutions |
| **2-Opt** | O(n²) per iteration | ★★★★☆ | ⚡⚡⚡⚡ | Quick improvements |
| **Simulated Annealing** | O(n × iterations) | ★★★★☆ | ⚡⚡⚡ | Escaping local optima |
| **Genetic Algorithm** | O(pop × gen × n) | ★★★★★ | ⚡ | Best quality, large problems |

### Performance Guidelines

- **< 20 cities**: Nearest Neighbor + 2-Opt (< 100ms)
- **20-50 cities**: Simulated Annealing (< 5s)
- **50-100 cities**: Tuned Genetic Algorithm (< 30s)
- **> 100 cities**: Scaled Genetic Algorithm with early stopping

## 🎮 Usage

### Interactive Mode

```
╔═══════════════════════════════════════════════════════════════╗
║      TRAVELING SALESMAN PROBLEM SOLVER v1.2.0                 ║
║                   .NET 9 Implementation                       ║
╚═══════════════════════════════════════════════════════════════╝

📍 Main Menu:

  1. Interactive Solver - Solve custom TSP instances
  2. Algorithm Benchmark - Compare all algorithms
  3. Visual Demonstration - See algorithms in action
  4. Algorithm Information - Learn about each algorithm
  5. Exit

➤ Select an option (1-5):
```

### Command Line

```bash
# Version information
./TSP-linux-x64-* --version

# Help
./TSP-linux-x64-* --help

# View logs
tail -f logs/tsp-solver-$(date +%Y-%m-%d).log
```

## 🏗️ Architecture

```
TSP/
├── TravelingSalesman.Core/          # Core algorithms & data structures
│   ├── City                        # Immutable city record
│   ├── Tour                        # Tour with distance caching
│   ├── ITspSolver                  # Solver interface
│   ├── TspSolverBase               # Base solver with common logic
│   ├── NearestNeighborSolver       # Greedy heuristic
│   ├── TwoOptSolver                # Local search improvement
│   ├── SimulatedAnnealingSolver    # Metaheuristic with cooling
│   ├── GeneticAlgorithmSolver      # Population-based evolution
│   ├── TspDataGenerator            # Test data generation
│   └── TspBenchmark                # Performance comparison
│
├── TravelingSalesman.ConsoleApp/    # Interactive console UI
│   └── Program.cs                   # Main entry point with menus
│
├── TravelingSalesman.Tests/         # xUnit tests (99 tests)
│   └── Tests.cs                     # Comprehensive test coverage
│
├── TravelingSalesman.Specs/         # BDD tests with Reqnroll
│   ├── Features/                    # Gherkin scenarios
│   └── StepDefinitions/             # Test implementations
│
├── Directory.Build.props            # Centralized MSBuild settings
├── Directory.Packages.props         # Central package management
├── Dockerfile                       # Multi-stage container build
└── .github/workflows/               # CI/CD automation
```

## 🔧 Development

### Prerequisites

- .NET 9 SDK ([Download](https://dotnet.microsoft.com/download/dotnet/9.0))
- Docker (optional)
- Git

### Testing

```bash
# Run all tests
dotnet test TSP.sln

# With coverage
dotnet test TSP.sln \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage

# Run specific tests
dotnet test --filter "FullyQualifiedName~TspBenchmark"

# Run BDD tests only
dotnet test TravelingSalesman.Specs
```

### Code Quality

The project enforces:
- ✅ `TreatWarningsAsErrors: true`
- ✅ `AnalysisLevel: latest`
- ✅ Nullable reference types
- ✅ Latest C# language features
- ✅ Central package management

## 📈 Performance Characteristics

### Memory Usage
- Native AOT reduces memory footprint by ~50%
- Tour distance caching prevents recalculation
- Efficient distance matrix for O(1) lookups

### Execution Speed (10 cities, Release build)
- Nearest Neighbor: < 1ms
- 2-Opt: < 10ms
- Simulated Annealing: < 100ms
- Genetic Algorithm: < 500ms

## 🐳 Docker Details

Multi-stage Dockerfile with:
- Native AOT compilation in build stage
- Minimal runtime dependencies
- Log directory auto-creation
- Support for VS debugging
- Optimized layer caching

```dockerfile
# Build
docker build -t tsp-solver .

# Run with resource limits
docker run --rm -it \
  --memory="512m" \
  --cpus="1.0" \
  tsp-solver
```

## 📝 Logging

Structured logging with Serilog:

```
logs/
├── tsp-solver-2025-01-17.log    # Today's detailed logs
├── tsp-solver-2025-01-16.log    # Yesterday's logs
└── ...                           # 30-day retention
```

Log levels:
- **Information**: User actions, results
- **Debug**: Algorithm internals
- **Trace**: Detailed execution flow
- **Warning**: Performance issues
- **Error**: Exceptions and failures

## 🚀 CI/CD Pipeline

GitHub Actions workflow:
1. **Build & Test**: Every push/PR
2. **Docker validation**: Container testing
3. **Native AOT**: Multi-platform compilation
4. **Release automation**: Tagged releases
5. **Artifact upload**: Binaries & logs

## ⚖️ License

This project is licensed under the **GNU Affero General Public License v3.0 (AGPL-3.0)**.

### What this means:

- ✅ **You CAN**: Use, modify, distribute, use privately
- ✅ **You CAN**: Use commercially (with source disclosure)
- ⚠️ **You MUST**: Disclose source code
- ⚠️ **You MUST**: Include copyright & license notices
- ⚠️ **You MUST**: State changes made
- ⚠️ **You MUST**: Share under same license (AGPL-3.0)
- ❌ **You CANNOT**: Hold liable or remove warranty disclaimers

**Important**: If you use this software as a network service, you must provide source code to users.

See [LICENSE.txt](LICENSE.txt) for full terms.

## 🤝 Contributing

We welcome contributions! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Write tests for new functionality
4. Ensure all tests pass (`dotnet test`)
5. Commit with clear messages (`git commit -m 'Add amazing feature'`)
6. Push to your fork (`git push origin feature/amazing-feature`)
7. Open a Pull Request

### Coding Standards
- Follow existing code style
- Add XML documentation for public APIs
- Write unit tests for new features
- Update README for significant changes

## 🐛 Known Issues

1. **Simulated Annealing Performance**: May take >20s for small problems with default parameters
   - **Workaround**: Reduce `initialTemperature` and `iterationsPerTemperature`
   - **Fix**: Adaptive parameter tuning based on city count (planned)

## 📚 Resources

- [TSP on Wikipedia](https://en.wikipedia.org/wiki/Travelling_salesman_problem)
- [TSPLIB Benchmark Sets](http://comopt.ifi.uni-heidelberg.de/software/TSPLIB95/)
- [.NET Native AOT](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
- [Serilog Documentation](https://serilog.net/)

## 👤 Author

**Kushal Hada**
- GitHub: [@kusl](https://github.com/kusl)
- Repository: [github.com/kusl/tsp](https://github.com/kusl/tsp)

## 🙏 Acknowledgments

- .NET team for excellent Native AOT support
- Serilog contributors for robust logging
- xUnit and Reqnroll teams for testing frameworks
- All contributors and issue reporters

---

**⚠️ AI Disclosure**: This project includes code generated with assistance from Large Language Models (LLMs) including Claude. All generated code has been reviewed, tested, and validated. Use at your own discretion.

**🔬 Academic Use**: If you use this software in research, please cite:
```
Hada, K. (2025). TSP Solver: A Multi-Algorithm Approach. 
GitHub. https://github.com/kusl/tsp
```
