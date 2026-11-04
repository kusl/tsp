#!/usr/bin/env bash
source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"

directory="${1:-$TSP_ARTIFACTS/release}"
[ -d "$directory" ] || die "directory not found: $directory"

cd "$directory"
shopt -s nullglob
files=(tsp-*)
[ "${#files[@]}" -gt 0 ] || die "no release binaries found in $directory"

if has_command sha256sum; then
    sha256sum "${files[@]}" > SHA256SUMS
else
    shasum -a 256 "${files[@]}" > SHA256SUMS
fi

log_info "Wrote $directory/SHA256SUMS"
cat SHA256SUMS
