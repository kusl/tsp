#!/usr/bin/env bash
source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"

usage() {
    cat <<USAGE
Usage: bash scripts/ci.sh [options]

Runs every verification step: lint, build, test, smoke, run, publish, container and export.

Options:
  --clean             Delete the artifacts directory first
  --skip-publish      Skip the Native AOT publish of the host binary
  --skip-container    Skip the container image build and smoke test
  --skip-export       Skip writing docs/llm/dump.txt
  -h, --help          Show this help
USAGE
}

clean=false
skip_publish=false
skip_container=false
skip_export=false

while [ "$#" -gt 0 ]; do
    case "$1" in
        --clean) clean=true ;;
        --skip-publish) skip_publish=true ;;
        --skip-container) skip_container=true ;;
        --skip-export) skip_export=true ;;
        -h | --help) usage; exit 0 ;;
        *) usage >&2; die "unknown option: $1" ;;
    esac
    shift
done

summary=()

run_step() {
    local name="$1" started
    shift
    started=$SECONDS
    log_step "[$name]"
    bash "$TSP_SCRIPTS/$1" "${@:2}"
    summary+=("$(printf '%-10s %4ss' "$name" "$((SECONDS - started))")")
}

skip_step() {
    log_warn "skipping $1: $2"
    summary+=("$(printf '%-10s skipped (%s)' "$1" "$2")")
}

if "$clean"; then
    log_step "Cleaning $TSP_ARTIFACTS"
    rm -rf "$TSP_ARTIFACTS"
fi

run_step lint lint.sh
run_step build build.sh
run_step test test.sh
run_step smoke smoke.sh
run_step run run.sh

if "$skip_publish"; then
    skip_step publish "--skip-publish"
elif [[ "$(host_rid)" == linux-* ]] && ! has_command clang; then
    skip_step publish "clang not installed"
else
    run_step publish publish.sh
fi

if "$skip_container"; then
    skip_step container "--skip-container"
elif ! has_command podman && ! has_command docker; then
    skip_step container "no container engine"
else
    run_step container container.sh
fi

if "$skip_export"; then
    skip_step export "--skip-export"
else
    run_step export export.sh
fi

log_step "Summary"
printf '  %s\n' "${summary[@]}"
log_info "All steps completed successfully"
