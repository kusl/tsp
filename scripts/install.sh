#!/usr/bin/env bash
set -euo pipefail

repository="${TSP_REPOSITORY:-kusl/tsp}"
install_dir="${TSP_INSTALL_DIR:-/opt/kusl-tsp}"
link_path="${TSP_LINK_PATH:-/usr/local/bin/kusl-tsp}"

die() { printf 'error: %s\n' "$*" >&2; exit 1; }

[ "$(uname -s)" = Linux ] || die "this installer supports Linux only"
case "$(uname -m)" in
    x86_64 | amd64) rid=linux-x64 ;;
    aarch64 | arm64) rid=linux-arm64 ;;
    *) die "unsupported architecture: $(uname -m)" ;;
esac

for command in curl sha256sum install; do
    command -v "$command" >/dev/null 2>&1 || die "required command not found: $command"
done

[ -w "$(dirname "$install_dir")" ] || [ "$(id -u)" -eq 0 ] || die "run as root (sudo) or set TSP_INSTALL_DIR to a writable location"

base_url="https://github.com/$repository/releases/latest/download"
work_dir="$(mktemp -d)"
trap 'rm -rf "$work_dir"' EXIT

printf 'Downloading tsp-%s from %s\n' "$rid" "$repository"
curl --fail --location --silent --show-error --output "$work_dir/tsp-$rid" "$base_url/tsp-$rid"
curl --fail --location --silent --show-error --output "$work_dir/SHA256SUMS" "$base_url/SHA256SUMS"

(cd "$work_dir" && grep " tsp-$rid\$" SHA256SUMS | sha256sum --check --status) || die "checksum verification failed"

if [ -f "$install_dir" ]; then
    rm -f "$install_dir"
fi
install -d "$install_dir"
install -m 0755 "$work_dir/tsp-$rid" "$install_dir/tsp"

if [ -d "$(dirname "$link_path")" ]; then
    ln -sfn "$install_dir/tsp" "$link_path"
fi

printf 'Installed %s (%s)\n' "$install_dir/tsp" "$("$install_dir/tsp" --version)"
if [ -L "$link_path" ]; then
    printf 'Run it with: %s --help\n' "$(basename "$link_path")"
fi
