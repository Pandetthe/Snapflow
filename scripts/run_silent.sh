#!/usr/bin/env bash

# Helper functions for running dotnet commands with clean output.
# Source this file; do not execute it directly.
#
# Usage:
#   source scripts/run_silent.sh
#   run_silent "Build" "dotnet build ./Server.slnx --configuration Release --nologo"
#   run_silent_with_test_count "Unit Tests" "dotnet test ./Server.slnx --configuration Release --nologo"
#
# Set VERBOSE=1 to see full command output.

GREEN='\033[0;32m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m'

VERBOSE=${VERBOSE:-0}

# Run command silently; print ✓ on success or ✗ + full output on failure.
run_silent() {
    local description="$1"
    local command="$2"

    if [ "$VERBOSE" = "1" ]; then
        echo "  → $command"
        eval "$command"
        return $?
    fi

    local tmp=$(mktemp)
    if eval "$command" > "$tmp" 2>&1; then
        printf "  ${GREEN}✓${NC} %s\n" "$description"
        rm -f "$tmp"
        return 0
    else
        local code=$?
        printf "  ${RED}✗${NC} %s\n" "$description"
        printf "${RED}Command: %s${NC}\n" "$command"
        cat "$tmp"
        rm -f "$tmp"
        return $code
    fi
}

# Like run_silent but also parses and prints dotnet test pass/skip/fail counts.
run_silent_with_test_count() {
    local description="$1"
    local command="$2"

    if [ "$VERBOSE" = "1" ]; then
        echo "  → $command"
        eval "$command"
        return $?
    fi

    local tmp=$(mktemp)
    if eval "$command" > "$tmp" 2>&1; then
        # Summary lines end with ".dll (netX.Y)" — works for any locale.
        # Extract passed count: "Passed: 42" (EN) or "powodzenie:    42" (PL)
        local total_passed
        total_passed=$(grep -E '\.dll \(net' "$tmp" \
            | grep -oE '(powodzenie|Passed):[[:space:]]*[0-9]+' \
            | grep -oE '[0-9]+$' \
            | awk '{s+=$1} END {print s}' || true)
        if [ -n "$total_passed" ] && [ "$total_passed" -gt 0 ] 2>/dev/null; then
            printf "  ${GREEN}✓${NC} %s — %s tests passed\n" "$description" "$total_passed"
        else
            printf "  ${GREEN}✓${NC} %s\n" "$description"
        fi
        rm -f "$tmp"
        return 0
    else
        local code=$?
        printf "  ${RED}✗${NC} %s\n" "$description"
        printf "${RED}Command: %s${NC}\n" "$command"
        cat "$tmp"
        rm -f "$tmp"
        return $code
    fi
}

print_header() {
    printf "\n${BLUE}[%s]${NC} %s:\n" "$1" "$2"
}
