#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname "$0")" && pwd)"
DLL_PATH="$SCRIPT_DIR/bin/Debug/net9.0/vsbd-core.dll"
PDB_PATH="$SCRIPT_DIR/bin/Debug/net9.0/vsbd-core.pdb"

DEST="$SCRIPT_DIR/../vsbd/Libraries"

mkdir -p "$DEST"

# remove previous copies
rm -f "$DEST/vsbd-core.dll" "$DEST/vsbd-core.pdb"

# copy fresh build
cp "$DLL_PATH" "$DEST/"
[ -f "$PDB_PATH" ] && cp "$PDB_PATH" "$DEST/"
