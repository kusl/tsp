#!/usr/bin/env bash
source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"

require_command dotnet "install the .NET 10 SDK"

results="$TSP_ARTIFACTS/test-results"
rm -rf "$results"

log_step "Testing $TSP_CONFIGURATION"
dotnet test \
    --solution "$TSP_SOLUTION" \
    --configuration "$TSP_CONFIGURATION" \
    --no-build \
    --results-directory "$results" \
    --coverage \
    --coverage-output-format cobertura \
    "$@"

log_info "Test results and coverage: $results"
