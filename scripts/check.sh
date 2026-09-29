#!/usr/bin/env bash
set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(dirname "$SCRIPT_DIR")"
source "$SCRIPT_DIR/run_silent.sh"
cd "$ROOT"

TARGET="${1:-all}"
case "$TARGET" in
    all|server|web) ;;
    *) echo "Usage: scripts/check.sh [all|server|web]"; exit 1 ;;
esac

if [ "$TARGET" != "web" ]; then
    print_header "Snapflow" "Server"
    run_silent "Restore" "dotnet restore ./server/Snapflow.slnx --nologo -v minimal"
    run_silent "Build"   "dotnet build   ./server/Snapflow.slnx --configuration Release --no-restore --nologo -v minimal"
    run_silent "Format"  "dotnet format  ./server/Snapflow.slnx --verify-no-changes --no-restore"
    run_silent_with_test_count "All tests" \
        "dotnet test ./server/Snapflow.slnx --configuration Release --no-build --nologo -v minimal"
fi

if [ "$TARGET" != "server" ]; then
    print_header "Snapflow" "Web"
    [ -d web/node_modules ] || run_silent "Install" "npm ci --prefix web"
    run_silent "Lint"       "npm run lint --prefix web"
    run_silent "Check"      "npm run check --prefix web"
    run_silent "Unit tests" "npm run test:unit --prefix web"
fi
