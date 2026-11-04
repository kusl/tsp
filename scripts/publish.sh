#!/usr/bin/env bash
source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"

require_command dotnet "install the .NET 10 SDK"

rid="${1:-$(host_rid)}"
version="$(tsp_version)"
publish_dir="$TSP_ARTIFACTS/publish/$rid"
release_dir="$TSP_ARTIFACTS/release"

case "$rid" in
    win-*) extension=".exe" ;;
    *) extension="" ;;
esac

if [[ "$rid" == linux-* && "$(host_rid)" == linux-* ]]; then
    require_command clang "Native AOT needs clang and zlib headers; on Fedora run: sudo dnf install clang zlib-devel"
fi

log_step "Publishing Native AOT binary for $rid (version $version)"
rm -rf "$publish_dir"
dotnet publish "$TSP_CLI_PROJECT" \
    --configuration Release \
    --runtime "$rid" \
    --output "$publish_dir" \
    -p:Version="$version"

mkdir -p "$release_dir"
artifact="$release_dir/tsp-$rid$extension"
cp "$publish_dir/tsp$extension" "$artifact"
log_info "Created $artifact ($(du -h "$artifact" | cut -f1))"

if [ "$rid" = "$(host_rid)" ]; then
    bash "$TSP_SCRIPTS/smoke.sh" "$artifact"
else
    log_warn "skipping smoke test: $rid binaries cannot run on $(host_rid)"
fi
