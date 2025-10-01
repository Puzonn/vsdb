#!/usr/bin/env bash
set -euo pipefail

DLL_PATH="$(dirname "$0")/bin/Debug/net9.0/vsbd-core.dll"

DEST="$(dirname "$0")/../vsbd/Libraries"

mkdir -p "$DEST"
cp "$DLL_PATH" "$DEST/"

echo "Copied to $DEST"
