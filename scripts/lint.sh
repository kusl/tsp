#!/usr/bin/env bash
source "$(dirname "${BASH_SOURCE[0]}")/lib.sh"

log_step "Checking shell scripts"
for script in "$TSP_SCRIPTS"/*.sh; do
    bash -n "$script" || die "syntax error in $script"
done

if has_command shellcheck; then
    (cd "$TSP_ROOT" && shellcheck scripts/*.sh)
    log_info "shellcheck passed"
else
    log_warn "shellcheck not installed; only syntax was checked (on Fedora: sudo dnf install ShellCheck)"
fi
