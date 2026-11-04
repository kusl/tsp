#!/usr/bin/env bash
set -euo pipefail

TSP_SCRIPTS="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TSP_ROOT="$(cd "$TSP_SCRIPTS/.." && pwd)"
TSP_SOLUTION="$TSP_ROOT/tsp.slnx"
TSP_CLI_PROJECT="$TSP_ROOT/src/TravelingSalesman.Cli/TravelingSalesman.Cli.csproj"
TSP_ARTIFACTS="$TSP_ROOT/artifacts"
TSP_CONFIGURATION="${TSP_CONFIGURATION:-Release}"
TSP_FALLBACK_VERSION="2.0.0"
export TSP_ROOT TSP_SOLUTION TSP_CLI_PROJECT TSP_ARTIFACTS TSP_CONFIGURATION

export DOTNET_NOLOGO=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

if [ -t 1 ] && [ -z "${NO_COLOR:-}" ]; then
    TSP_BLUE=$'\033[1;34m'
    TSP_GREEN=$'\033[0;32m'
    TSP_YELLOW=$'\033[0;33m'
    TSP_RED=$'\033[0;31m'
    TSP_RESET=$'\033[0m'
else
    TSP_BLUE=''
    TSP_GREEN=''
    TSP_YELLOW=''
    TSP_RED=''
    TSP_RESET=''
fi

log_step() { printf '%s==> %s%s\n' "$TSP_BLUE" "$*" "$TSP_RESET"; }
log_info() { printf '%s%s%s\n' "$TSP_GREEN" "$*" "$TSP_RESET"; }
log_warn() { printf '%swarning: %s%s\n' "$TSP_YELLOW" "$*" "$TSP_RESET" >&2; }
log_error() { printf '%serror: %s%s\n' "$TSP_RED" "$*" "$TSP_RESET" >&2; }
die() { log_error "$*"; exit 1; }

has_command() { command -v "$1" >/dev/null 2>&1; }
require_command() { has_command "$1" || die "required command not found: $1${2:+ ($2)}"; }

in_git_repo() { git -C "$TSP_ROOT" rev-parse --is-inside-work-tree >/dev/null 2>&1; }

utc_date_of_epoch() {
    date -u -d "@$1" "$2" 2>/dev/null || date -u -r "$1" "$2"
}

tsp_version() {
    if [ -n "${TSP_VERSION:-}" ]; then
        printf '%s\n' "$TSP_VERSION"
        return
    fi
    if ! has_command git || ! in_git_repo || ! git -C "$TSP_ROOT" rev-parse --verify --quiet HEAD >/dev/null; then
        printf '%s\n' "$TSP_FALLBACK_VERSION"
        return
    fi
    local epoch year month day count
    epoch="$(git -C "$TSP_ROOT" show -s --format=%ct HEAD)"
    year="$(utc_date_of_epoch "$epoch" +%Y)"
    month="$(utc_date_of_epoch "$epoch" +%m)"
    day="$(utc_date_of_epoch "$epoch" +%d)"
    count="$(git -C "$TSP_ROOT" rev-list --count HEAD)"
    printf '%d.%d.%d.%d\n' "$year" "$((10#$month))" "$((10#$day))" "$((count % 65534))"
}

host_rid() {
    local os arch
    case "$(uname -s)" in
        Linux) os=linux ;;
        Darwin) os=osx ;;
        MINGW* | MSYS* | CYGWIN*) os=win ;;
        *) die "unsupported operating system: $(uname -s)" ;;
    esac
    case "$(uname -m)" in
        x86_64 | amd64) arch=x64 ;;
        aarch64 | arm64) arch=arm64 ;;
        *) die "unsupported architecture: $(uname -m)" ;;
    esac
    printf '%s-%s\n' "$os" "$arch"
}

tsp_cli() {
    dotnet run --project "$TSP_CLI_PROJECT" --configuration "$TSP_CONFIGURATION" --no-build -- "$@"
}
