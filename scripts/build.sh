#!/usr/bin/env bash
source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"

require_command dotnet "install the .NET 10 SDK"

version="$(tsp_version)"
log_step "Restoring $TSP_SOLUTION"
dotnet restore "$TSP_SOLUTION"

log_step "Building $TSP_CONFIGURATION (version $version)"
dotnet build "$TSP_SOLUTION" \
    --configuration "$TSP_CONFIGURATION" \
    --no-restore \
    -p:Version="$version" \
    "$@"
