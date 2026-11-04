kushal@fedora:~/src/dotnet/tsp$ cd ~/src/dotnet/tsp; time git rm -r TSP.sln Dockerfile .dockerignore TravelingSalesman.ConsoleApp TravelingSalesman.Core TravelingSalesman.Specs TravelingSalesman.Tests .github/workflows/build.yml docs/build.txt docs/results.html scripts/PowerShell.txt scripts/bash.txt scripts/cat.ps1 scripts/push.ps1 scripts/linux-installation-command.txt scripts/install-linux.sh scripts/install-kusl-tsp-simple.sh
rm '.dockerignore'
rm '.github/workflows/build.yml'
rm 'Dockerfile'
rm 'TSP.sln'
rm 'TravelingSalesman.ConsoleApp/Program.cs'
rm 'TravelingSalesman.ConsoleApp/Properties/launchSettings.json'
rm 'TravelingSalesman.ConsoleApp/TravelingSalesman.ConsoleApp.csproj'
rm 'TravelingSalesman.Core/TravelingSalesman.Core.csproj'
rm 'TravelingSalesman.Core/TravelingSalesmanLibrary.cs'
rm 'TravelingSalesman.Specs/Features/Benchmarking.feature'
rm 'TravelingSalesman.Specs/Features/CityOperations.feature'
rm 'TravelingSalesman.Specs/Features/DataGeneration.feature'
rm 'TravelingSalesman.Specs/Features/TspSolvers.feature'
rm 'TravelingSalesman.Specs/StepDefinitions/BenchmarkingSteps.cs'
rm 'TravelingSalesman.Specs/StepDefinitions/CityOperationSteps.cs'
rm 'TravelingSalesman.Specs/StepDefinitions/DataGenerationSteps.cs'
rm 'TravelingSalesman.Specs/StepDefinitions/TspSolverSteps.cs'
rm 'TravelingSalesman.Specs/Support/Hooks.cs'
rm 'TravelingSalesman.Specs/TravelingSalesman.Specs.csproj'
rm 'TravelingSalesman.Specs/reqnroll.json'
rm 'TravelingSalesman.Tests/Tests.cs'
rm 'TravelingSalesman.Tests/TravelingSalesman.Tests.csproj'
rm 'docs/build.txt'
rm 'docs/results.html'
rm 'scripts/PowerShell.txt'
rm 'scripts/bash.txt'
rm 'scripts/cat.ps1'
rm 'scripts/install-kusl-tsp-simple.sh'
rm 'scripts/install-linux.sh'
rm 'scripts/linux-installation-command.txt'
rm 'scripts/push.ps1'

real	0m0.005s
user	0m0.001s
sys	0m0.003s
kushal@fedora:~/src/dotnet/tsp$ cd ~/src/dotnet/tsp/; time rm -rf TravelingSalesman.ConsoleApp TravelingSalesman.Core TravelingSalesman.Specs TravelingSalesman.Tests

real	0m0.001s
user	0m0.001s
sys	0m0.000s
kushal@fedora:~/src/dotnet/tsp$ cd ~/src/dotnet/tsp/; time bash scripts/export.sh > docs/llm/vendor/claude/terminal.txt

real	0m1.452s
user	0m0.734s
sys	0m0.866s
kushal@fedora:~/src/dotnet/tsp$ time dnf info clang zlib-devel
Updating and loading repositories:
 Fedora 44 - x86_64 - Updates                                                                                                                                               100% |  24.6 KiB/s |  19.1 KiB |  00m01s
 RPM Fusion for Fedora 44 - Nonfree - Updates                                                                                                                               100% |   7.3 KiB/s |   5.7 KiB |  00m01s
 RPM Fusion for Fedora 44 - Nonfree - Steam                                                                                                                                 100% |   6.5 KiB/s |   5.1 KiB |  00m01s
 RPM Fusion for Fedora 44 - Nonfree - NVIDIA Driver                                                                                                                         100% |   7.9 KiB/s |   5.3 KiB |  00m01s
 RPM Fusion for Fedora 44 - Nonfree                                                                                                                                         100% |  15.4 KiB/s |   5.6 KiB |  00m00s
 RPM Fusion for Fedora 44 - Free - Updates                                                                                                                                  100% |  10.5 KiB/s |   3.8 KiB |  00m00s
 RPM Fusion for Fedora 44 - Free                                                                                                                                            100% |  11.2 KiB/s |   3.4 KiB |  00m00s
 google-chrome                                                                                                                                                              100% |  10.0 KiB/s |   1.3 KiB |  00m00s
 Fedora 44 openh264 (From Cisco) - x86_64                                                                                                                                   100% |   4.0 KiB/s | 986.0   B |  00m00s
 Fedora 44 - x86_64                                                                                                                                                         100% |  96.1 KiB/s |  22.4 KiB |  00m00s
Repositories loaded.
No matches found for "zlib-devel".
Available packages
Name           : clang
Epoch          : 0
Version        : 22.1.8
Release        : 4.fc44
Architecture   : i686
Download size  : 151.3 KiB
Installed size : 351.8 KiB
Source         : llvm-22.1.8-4.fc44.src.rpm
Repository     : updates
Summary        : A C language family front-end for LLVM
URL            : http://llvm.org
License        : Apache-2.0 WITH LLVM-exception OR NCSA
Description    : clang: noun
               :     1. A loud, resonant, metallic sound.
               :     2. The strident call of a crane or goose.
               :     3. C-language family front-end toolkit.
               : 
               : The goal of the Clang project is to create a new C, C++, Objective C
               : and Objective C++ front-end for the LLVM compiler. Its tools are built
               : as libraries and designed to be loosely-coupled and extensible.
               : 
               : Install compiler-rt if you want the Blocks C language extension or to
               : enable sanitization and profiling options when building, and
               : libomp-devel to enable -fopenmp.
Vendor         : Fedora Project

Name           : clang
Epoch          : 0
Version        : 22.1.8
Release        : 4.fc44
Architecture   : x86_64
Download size  : 7.1 MiB
Installed size : 61.5 MiB
Source         : llvm-22.1.8-4.fc44.src.rpm
Repository     : updates
Summary        : A C language family front-end for LLVM
URL            : http://llvm.org
License        : Apache-2.0 WITH LLVM-exception OR NCSA
Description    : clang: noun
               :     1. A loud, resonant, metallic sound.
               :     2. The strident call of a crane or goose.
               :     3. C-language family front-end toolkit.
               : 
               : The goal of the Clang project is to create a new C, C++, Objective C
               : and Objective C++ front-end for the LLVM compiler. Its tools are built
               : as libraries and designed to be loosely-coupled and extensible.
               : 
               : Install compiler-rt if you want the Blocks C language extension or to
               : enable sanitization and profiling options when building, and
               : libomp-devel to enable -fopenmp.
Vendor         : Fedora Project

real	0m1.872s
user	0m0.837s
sys	0m0.292s
kushal@fedora:~/src/dotnet/tsp$ dnf search zlib
Updating and loading repositories:
Repositories loaded.
Matched fields: name, summary
 erlang-ezlib.x86_64	Native zlib driver for Erlang
 gambas3-gb-compress-bzlib2.x86_64	Gambas3 component package for compress-bzlib2
 gambas3-gb-compress-zlib.x86_64	Gambas3 component package for compress-zlib
 ghc-zlib.x86_64	Compression and decompression in the gzip and zlib formats
 ghc-zlib-bindings.x86_64	Low-level bindings to the zlib package
 ghc-zlib-bindings-devel.x86_64	Haskell zlib-bindings library development files
 ghc-zlib-bindings-doc.noarch	Haskell zlib-bindings library documentation
 ghc-zlib-bindings-prof.x86_64	Haskell zlib-bindings profiling library
 ghc-zlib-devel.x86_64	Haskell zlib library development files
 ghc-zlib-doc.noarch	Haskell zlib library documentation
 ghc-zlib-prof.x86_64	Haskell zlib profiling library
 gnatcoll-zlib.i686	The GNAT Components Collection – ZLib binding
 gnatcoll-zlib.x86_64	The GNAT Components Collection – ZLib binding
 gnatcoll-zlib-devel.i686	Development files for the GNAT Components Collection – ZLib binding
 gnatcoll-zlib-devel.x86_64	Development files for the GNAT Components Collection – ZLib binding
 jzlib.noarch	Re-implementation of zlib in pure Java
 jzlib-demo.noarch	Examples for jzlib
 libknet1-compress-zlib-plugin.x86_64	Provides libknet1 zlib support
 lua-zlib.x86_64	Simple streaming interface to zlib for Lua
 mingw32-zlib.noarch	MinGW Windows zlib compression library for the win32 target
 mingw32-zlib-static.noarch	Static libraries for mingw32-zlib development.
 mingw64-zlib.noarch	MinGW Windows zlib compression library for the win64 target
 mingw64-zlib-static.noarch	Static libraries for mingw64-zlib development
 perl-Archive-Extract-Z-Compress-Zlib.noarch	Z decompressor for perl-Archive-Extract via Compress::Zlib
 perl-Archive-Extract-gz-Compress-Zlib.noarch	Gzip decompressor for perl-Archive-Extract via Compress::Zlib
 perl-Archive-Extract-tgz-Archive-Tar-Compress-Zlib.noarch	Gzipped-tar decompressor for perl-Archive-Extract via Archive::Tar and Compress::Zlib
 perl-Archive-Extract-tgz-Archive-Tar-IO-Zlib.noarch	Gzipped-tar decompressor for perl-Archive-Extract via Archive::Tar and IO::Zlib
 perl-Compress-Raw-Zlib.x86_64	Low-level interface to the zlib compression library
 perl-Compress-Raw-Zlib-tests.x86_64	Tests for perl-Compress-Raw-Zlib
 perl-IO-Zlib.noarch	Perl IO:: style interface to Compress::Zlib
 perl-IO-Zlib-tests.noarch	Tests for perl-IO-Zlib
 perl-POE-Filter-Zlib.noarch	POE filter wrapped around Compress::Zlib
 python3-zlib-ng.x86_64	Drop-in replacement for zlib and gzip modules using zlib-ng
 rust-libz-sys+stock-zlib-devel.noarch	Low-level bindings to the system libz library (also known as zlib)
 rust-zlib-rs+__internal-api-devel.noarch	Memory-safe zlib implementation written in rust
 rust-zlib-rs+avx512-devel.noarch	Memory-safe zlib implementation written in rust
 rust-zlib-rs+c-allocator-devel.noarch	Memory-safe zlib implementation written in rust
 rust-zlib-rs+default-devel.noarch	Memory-safe zlib implementation written in rust
 rust-zlib-rs+lsx-devel.noarch	Memory-safe zlib implementation written in rust
 rust-zlib-rs+rust-allocator-devel.noarch	Memory-safe zlib implementation written in rust
 rust-zlib-rs+std-devel.noarch	Memory-safe zlib implementation written in rust
 rust-zlib-rs+vpclmulqdq-devel.noarch	Memory-safe zlib implementation written in rust
 rust-zlib-rs-devel.noarch	Memory-safe zlib implementation written in rust
 ucrt64-zlib.noarch	MinGW Windows zlib compression library for the ucrt64 target
 ucrt64-zlib-static.noarch	Static libraries for ucrt64-zlib development
 zlib-ada.i686	Zlib for Ada
 zlib-ada.x86_64	Zlib for Ada
 zlib-ada-devel.i686	Development files for Zlib-Ada
 zlib-ada-devel.x86_64	Development files for Zlib-Ada
 zlib-ng.i686	Zlib replacement with optimizations
 zlib-ng.x86_64	Zlib replacement with optimizations
 zlib-ng-compat.i686	Zlib implementation provided by zlib-ng
 zlib-ng-compat.x86_64	Zlib implementation provided by zlib-ng
 zlib-ng-compat-devel.i686	Development files for zlib-ng-compat
 zlib-ng-compat-devel.x86_64	Development files for zlib-ng-compat
 zlib-ng-compat-static.i686	Static libraries for zlib-ng-compat
 zlib-ng-compat-static.x86_64	Static libraries for zlib-ng-compat
 zlib-ng-devel.i686	Development files for zlib-ng
 zlib-ng-devel.x86_64	Development files for zlib-ng
 zlibrary-devel.x86_64	Development files for zlibrary
Matched fields: name
 rust-async-compression+zlib-devel.noarch	Adaptors between compression crates and Rust's modern asynchronous IO types
 rust-blazesym+zlib-devel.noarch	Library for address symbolization and related tasks
 rust-flate2+any_c_zlib-devel.noarch	DEFLATE compression and decompression exposed as Read/BufRead/Write streams
 rust-flate2+any_zlib-devel.noarch	DEFLATE compression and decompression exposed as Read/BufRead/Write streams
 rust-flate2+zlib-default-devel.noarch	DEFLATE compression and decompression exposed as Read/BufRead/Write streams
 rust-flate2+zlib-devel.noarch	DEFLATE compression and decompression exposed as Read/BufRead/Write streams
 rust-flate2+zlib-ng-devel.noarch	DEFLATE compression and decompression exposed as Read/BufRead/Write streams
 rust-flate2+zlib-rs-devel.noarch	DEFLATE compression and decompression exposed as Read/BufRead/Write streams
 rust-gix+zlib-ng-devel.noarch	Interact with git repositories just like git would
 rust-gix+zlib-rs-devel.noarch	Interact with git repositories just like git would
 rust-gix+zlib-stock-devel.noarch	Interact with git repositories just like git would
 rust-gix-features+zlib-devel.noarch	Integrate various capabilities using compile-time feature flags
 rust-gzp+any_zlib-devel.noarch	Parallel Compression
 rust-gzp+deflate_zlib_ng-devel.noarch	Parallel Compression
 rust-libbpf-sys+static-zlib-devel.noarch	Rust bindings to libbpf from the Linux kernel
 rust-libbpf-sys+vendored-zlib-devel.noarch	Rust bindings to libbpf from the Linux kernel
 rust-libcramjam+zlib-devel.noarch	Compression library combining a plethora of algorithms
 rust-libcramjam+zlib-shared-devel.noarch	Compression library combining a plethora of algorithms
 rust-libcramjam0.8+zlib-devel.noarch	Compression library combining a plethora of algorithms
 rust-libcramjam0.8+zlib-shared-devel.noarch	Compression library combining a plethora of algorithms
 rust-png+zlib-rs-devel.noarch	PNG decoding and encoding library in pure Rust
 rust-rustls+zlib-devel.noarch	Rustls is a modern TLS library written in Rust
 rust-tokio-rustls+zlib-devel.noarch	Asynchronous TLS/SSL streams for Tokio using Rustls
 rust-zip+deflate-flate2-zlib-devel.noarch	Library to support the reading and writing of zip files
 rust-zip+deflate-flate2-zlib-ng-devel.noarch	Library to support the reading and writing of zip files
 rust-zip+deflate-flate2-zlib-rs-devel.noarch	Library to support the reading and writing of zip files
 rust-zip0.6+deflate-zlib-devel.noarch	Library to support the reading and writing of zip files
 rust-zip2+deflate-zlib-devel.noarch	Library to support the reading and writing of zip files
 rust-zip2+deflate-zlib-ng-devel.noarch	Library to support the reading and writing of zip files
 rust-zopfli+zlib-devel.noarch	Rust implementation of the Zopfli compression algorithm
 rust-zune-inflate+zlib-devel.noarch	Heavily optimized deflate decompressor in Pure Rust
 zlibrary.x86_64	Cross-platform GUI library
Matched fields: summary
 golang-modernc-z.x86_64	Zlib for Go without cgo
 golang-modernc-z-devel.noarch	Zlib for Go without cgo
 libdeflate.i686	Fast implementation of DEFLATE, gzip, and zlib
 libdeflate.x86_64	Fast implementation of DEFLATE, gzip, and zlib
 mingw32-libidn.noarch	MinGW Windows zlib compression library for the win32 target
 mingw64-libidn.noarch	MinGW Windows zlib compression library for the win64 target
 miniz.i686	Compression library implementing the zlib and Deflate
 miniz.x86_64	Compression library implementing the zlib and Deflate
 minizip-ng.i686	Minizip-ng contrib in zlib-ng with the latest bug fixes and advanced features
 minizip-ng.x86_64	Minizip-ng contrib in zlib-ng with the latest bug fixes and advanced features
 ocaml-decompress.x86_64	Pure OCaml implementation of Zlib
 python3-isal.x86_64	Faster zlib and gzip compatible (de)compression using the ISA-L library
 rust-libz-ng-sys+default-devel.noarch	Low-level bindings to zlib-ng
 rust-libz-ng-sys-devel.noarch	Low-level bindings to zlib-ng
 rust-libz-rs-sys+c-allocator-devel.noarch	Memory-safe zlib implementation written in rust
 rust-libz-rs-sys+custom-prefix-devel.noarch	Memory-safe zlib implementation written in rust
 rust-libz-rs-sys+default-devel.noarch	Memory-safe zlib implementation written in rust
 rust-libz-rs-sys+export-symbols-devel.noarch	Memory-safe zlib implementation written in rust
 rust-libz-rs-sys+gz-devel.noarch	Memory-safe zlib implementation written in rust
 rust-libz-rs-sys+rust-allocator-devel.noarch	Memory-safe zlib implementation written in rust
 rust-libz-rs-sys+semver-prefix-devel.noarch	Memory-safe zlib implementation written in rust
 rust-libz-rs-sys+std-devel.noarch	Memory-safe zlib implementation written in rust
 rust-libz-rs-sys+testing-prefix-devel.noarch	Memory-safe zlib implementation written in rust
 rust-libz-rs-sys-devel.noarch	Memory-safe zlib implementation written in rust
 rust-libz-sys+default-devel.noarch	Low-level bindings to the system libz library (also known as zlib)
 rust-libz-sys+libc-devel.noarch	Low-level bindings to the system libz library (also known as zlib)
 rust-libz-sys-devel.noarch	Low-level bindings to the system libz library (also known as zlib)
 zopfli.i686	Zlib compatible better compressor
 zopfli.x86_64	Zlib compatible better compressor
 zstr-devel.noarch	A C++ header-only ZLib wrapper
kushal@fedora:~/src/dotnet/tsp$ git status
On branch master
Your branch is ahead of 'origin/master' by 8 commits.
  (use "git push" to publish your local commits)

nothing to commit, working tree clean
kushal@fedora:~/src/dotnet/tsp$ cd ~/src/dotnet/tsp/; time bash scripts/export.sh > docs/llm/vendor/claude/terminal.txt

real	0m1.333s
user	0m0.742s
sys	0m0.859s
kushal@fedora:~/src/dotnet/tsp$ 
