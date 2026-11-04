#!/usr/bin/env bash
source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"

require_command dotnet "install the .NET 10 SDK"

if [ "$#" -eq 0 ]; then
    set -- benchmark --cities 25 --pattern random --seed 42
fi

log_step "tsp $*"
tsp_cli "$@"
