#!/usr/bin/env bash
source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"

engine="${TSP_CONTAINER_ENGINE:-}"
if [ -z "$engine" ]; then
    if has_command podman; then
        engine=podman
    elif has_command docker; then
        engine=docker
    else
        die "neither podman nor docker is installed; on Fedora run: sudo dnf install podman"
    fi
fi

image="${TSP_IMAGE:-localhost/tsp:dev}"
version="$(tsp_version)"

log_step "Building $image with $engine (version $version)"
"$engine" build \
    --file "$TSP_ROOT/Containerfile" \
    --build-arg "VERSION=$version" \
    --tag "$image" \
    "$TSP_ROOT"

bash "$TSP_SCRIPTS/smoke.sh" "$engine" run --rm "$image"
