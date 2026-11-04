#!/usr/bin/env bash
source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"

require_command dotnet "install the .NET 10 SDK"

TSP_SMOKE_COMMAND=("$@")
if [ "${#TSP_SMOKE_COMMAND[@]}" -eq 0 ]; then
    TSP_SMOKE_COMMAND=(tsp_cli)
fi

failures=0

run_tsp() { "${TSP_SMOKE_COMMAND[@]}" "$@" 2>&1 | tr -d '\r'; }

pass() { log_info "  ok    $*"; }
fail() { log_error "  FAIL  $*"; failures=$((failures + 1)); }

expect_output() {
    local pattern="$1" output
    shift
    if output="$(run_tsp "$@")" && grep -Eq -- "$pattern" <<<"$output"; then
        pass "tsp $*"
    else
        fail "tsp $* (expected /$pattern/)"
        printf '%s\n' "$output" >&2
    fi
}

expect_failure() {
    local output
    if output="$(run_tsp "$@")"; then
        fail "tsp $* (expected a non-zero exit code)"
        printf '%s\n' "$output" >&2
    else
        pass "tsp $* (rejected)"
    fi
}

log_step "Smoke testing the CLI"

expect_output '^[0-9]+\.[0-9]+\.[0-9]+' --version
expect_output 'benchmark' --help
expect_output 'genetic' algorithms

for algorithm in nearest-neighbor two-opt simulated-annealing genetic; do
    expect_output '^Distance +: [0-9]+\.[0-9]{2}$' solve --cities 15 --seed 7 --algorithm "$algorithm"
done

expect_output '^Route +: C0 -> ' solve --cities 9 --pattern grid --route
expect_output '^[ .o]*S[ .o]*$' solve --cities 12 --pattern circle --plot
expect_output '^Best: ' benchmark --cities 15 --seed 42

expect_failure solve --cities 1
expect_failure solve --pattern spiral
expect_failure solve --algorithm brute-force
expect_failure

if [ "$failures" -gt 0 ]; then
    die "$failures smoke check(s) failed"
fi

log_info "All smoke checks passed"
