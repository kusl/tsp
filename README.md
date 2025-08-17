# TSP Solver

[![Build Status](https://github.com/kusl/tsp/actions/workflows/build.yml/badge.svg)](https://github.com/kusl/tsp/actions/workflows/build.yml)
[![Release](https://github.com/kusl/tsp/actions/workflows/release.yml/badge.svg)](https://github.com/kusl/tsp/actions/workflows/release.yml)

A high-performance Traveling Salesman Problem (TSP) solver written in .NET 9 with multiple algorithms, comprehensive logging, and cross-platform native AOT compilation.

## ✨ Features

- 🧠 **Multiple Algorithms**: Nearest Neighbor, 2-Opt, Simulated Annealing, Genetic Algorithm
- 📊 **Comprehensive Logging**: Console + file logging with Serilog (daily rotation, 30-day retention)
- 🎯 **Interactive Mode**: Solve custom TSP instances with different city patterns (random, circular, grid)
- 🏆 **Benchmark Mode**: Compare all algorithms performance side-by-side
- 📈 **Performance Tracking**: Detailed metrics and algorithm progress reporting
- 🐳 **Docker Support**: Containerized execution environment
- ⚡ **Native AOT**: Fast startup, single-file deployment, no .NET runtime required
- 🔄 **Cross-Platform**: Windows, Linux, macOS (Intel & ARM64)

## 🚀 Quick Start

### Download Pre-built Binaries

Download the latest release for your platform from [Releases](https://github.com/kusl/tsp/releases/latest):

- **Windows**: `TSP-win-x64-{sha}.exe`
- **Linux**: `TSP-linux-x64-{sha}`
- **macOS Intel**: `TSP-osx-x64-{sha}`
- **macOS Apple Silicon**: `TSP-osx-arm64-{sha}`

```bash
# Linux/macOS example
chmod +x TSP-linux-x64-{sha}
./TSP-linux-x64-{sha}
```

### Run with Docker

```bash
# Interactive mode
docker build -t tsp-solver .
docker run --rm -it tsp-solver

# With log persistence
docker run --rm -it -v $(pwd)/logs:/app/logs tsp-solver
```

### Build from Source

```bash
# Clone repository
git clone https://github.com/kusl/tsp.git
cd tsp

# Build solution
dotnet build TSP.sln

# Run console application
dotnet run --project TravelingSalesman.ConsoleApp

# Run tests
dotnet test TSP.sln

# Publish native AOT binary
dotnet publish TravelingSalesman.ConsoleApp -c Release -r linux-x64 --self-contained -p:PublishAot=true
```

## 📋 Usage Examples

### Interactive Mode
```bash
./TSP-linux-x64-{sha}

# Follow the interactive menu:
# 1. Interactive Solver - Solve custom TSP instances
# 2. Algorithm Benchmark - Compare all algorithms  
# 3. Visual Demonstration - See algorithms in action
# 4. Algorithm Information - Learn about each algorithm
# 5. Exit
```

### Command Line Options
```bash
# Show version
./TSP-linux-x64-{sha} --version

# Show help
./TSP-linux-x64-{sha} --help
```

### View Logs
```bash
# Logs are written to logs/ directory
ls -la logs/
tail -f logs/tsp-solver-*.log
```

## 🧠 Algorithm Comparison

| Algorithm | Best For | Time Complexity | Quality | Speed |
|-----------|----------|-----------------|---------|-------|
| **Nearest Neighbor** | Quick results, real-time | O(n²) | Good | ⚡⚡⚡ |
| **2-Opt** | Improved solutions | O(n²) per iteration | Better | ⚡⚡ |
| **Simulated Annealing** | Avoiding local optima | O(n) per iteration | Very Good | ⚡ |
| **Genetic Algorithm** | Large problems, best quality | O(p×g×n) | Best | 🐌 |

### Recommendations:
- **Small problems (< 20 cities)**: Nearest Neighbor + 2-Opt
- **Medium problems (20-100 cities)**: Simulated Annealing
- **Large problems (> 100 cities)**: Genetic Algorithm
- **Real-time requirements**: Nearest Neighbor

## 📊 Project Structure

```
TSP/
├── TravelingSalesman.Core/           # Core TSP algorithms and logic
├── TravelingSalesman.ConsoleApp/     # Interactive console application
├── TravelingSalesman.Tests/          # Comprehensive unit tests
├── scripts/                          # Build and utility scripts
├── .github/workflows/                # CI/CD automation
├── Directory.Build.props             # Centralized project settings
├── Directory.Packages.props          # Centralized package versions
├── Dockerfile                        # Container configuration
└── TSP.sln                          # Solution file
```

## 🔧 Development

### Prerequisites
- .NET 9 SDK
- Docker (optional)
- Git

### Running Tests
```bash
# Run all tests
dotnet test TSP.sln --verbosity normal

# Run with coverage
dotnet test TSP.sln --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "TspBenchmarkTests"
```

### Code Analysis
```bash
# Build with full analysis
dotnet build TSP.sln -c Release --verbosity normal

# The project uses:
# - TreatWarningsAsErrors: true
# - AnalysisLevel: latest
# - Nullable reference types enabled
```

## 🚀 Version Management

When upgrading to a new .NET version, update these lines in `Directory.Build.props`:

```xml
<!-- Current: .NET 9 -->
<TargetFramework>net9.0</TargetFramework>
<DotNetVersion>9.0</DotNetVersion>
<DotNetChannel>9.0</DotNetChannel>

<!-- Future: .NET 10 -->
<!-- <TargetFramework>net10.0</TargetFramework> -->
<!-- <DotNetVersion>10.0</DotNetVersion> -->
<!-- <DotNetChannel>10.0</DotNetChannel> -->
```

And update the Dockerfile ARGs:
```dockerfile
ARG DOTNET_VERSION=10.0
ARG DOTNET_VERSION_EXACT=10.0
```

## 🏗️ CI/CD Pipeline

The project uses GitHub Actions for automated:

- **Build & Test**: Every push/PR
- **Docker Testing**: Multi-scenario validation
- **Native AOT Compilation**: Windows, Linux, macOS (x64 & ARM64)
- **Automated Releases**: Tagged releases with binaries
- **Log Validation**: Ensures logging works in all environments

## 📝 Logging

The application uses Serilog for structured logging:

- **Console**: User-friendly output during execution
- **File**: Detailed logs in `logs/` directory
  - Daily rotation (`tsp-solver-YYYY-MM-DD.log`)
  - 30-day retention
  - Debug-level algorithm internals
  - Performance metrics and benchmarking data

Log levels:
- **Information**: User actions, results, benchmarks
- **Debug**: Algorithm progress, internal state
- **Trace**: Detailed execution flow (distance calculations, swaps)

## 🐳 Docker

The project includes a multi-stage Dockerfile optimized for:
- ✅ Native AOT compilation
- ✅ Minimal runtime dependencies
- ✅ Development debugging support
- ✅ Log directory creation
- ✅ Cross-platform compatibility

## 📄 License

AGPLv3 License - see [LICENSE.txt](LICENSE.txt)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Run tests (`dotnet test TSP.sln`)
5. Commit changes (`git commit -m 'Add amazing feature'`)
6. Push to branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

## 🐛 Issues & Support

- 🐛 **Bug Reports**: [GitHub Issues](https://github.com/kusl/tsp/issues)
- 💡 **Feature Requests**: [GitHub Discussions](https://github.com/kusl/tsp/discussions)
- 📚 **Documentation**: Check the source code - it's heavily commented!

---

**⚠️ LLM Notice**: This project contains code generated by Large Language Models (Claude, etc.). All code is experimental and thoroughly tested.