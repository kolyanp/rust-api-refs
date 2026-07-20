#!/usr/bin/env bash
# Decompiles Carbon.Windows.Release.zip.
#
# Carbon zip structure:
#   carbon/managed/*.dll        — Carbon core DLLs (10)  → carbon/
#   carbon/managed/hooks/*.dll  — Carbon hooks (3)       → carbon/hooks/
#   carbon/managed/modules/*.dll— Carbon modules (1)     → carbon/modules/
#   carbon/managed/lib/*.dll    — Carbon deps (58)       → SKIP (Mono.Cecil, System.*, etc.)
#
# NOTE: Carbon does NOT ship game DLLs (Assembly-CSharp, Facepunch.*, Rust.*).
#       Those come from the Oxide sync (shared/ folder).
set -uo pipefail

ZIP="$1"
WORK="/tmp/carbon-extract"
ILSPY="$HOME/.dotnet/tools/ilspycmd"

echo "=== Carbon: extracting ==="
rm -rf "$WORK" && mkdir -p "$WORK"
unzip -q "$ZIP" -d "$WORK"

MANAGED="$WORK/carbon/managed"
if [ ! -d "$MANAGED" ]; then
  echo "ERROR: carbon/managed/ not found in archive"
  unzip -l "$ZIP" | head -20
  exit 1
fi
echo "Managed: $MANAGED"

decompile() {
  local DLL="$1" OUT="$2"
  [ -f "$DLL" ] || return 0
  local HASH_FILE="$OUT/.dll_hash"
  local CURR_HASH
  CURR_HASH=$(sha256sum "$DLL" | cut -d' ' -f1)
  if [ -f "$HASH_FILE" ] && [ "$(cat "$HASH_FILE")" = "$CURR_HASH" ]; then
    echo "  skip (unchanged): $(basename "$DLL")"; return 0
  fi
  echo "  decompile: $(basename "$DLL") → $OUT"
  rm -rf "$OUT" && mkdir -p "$OUT"
  "$ILSPY" "$DLL" -p -o "$OUT" --no-dead-code 2>/dev/null \
    || "$ILSPY" "$DLL" -p -o "$OUT" 2>&1 | tail -3 || true
  echo "$CURR_HASH" > "$HASH_FILE"
}

# ── carbon/managed/*.dll → carbon/ ──────────────────────────────────────────
echo "=== Carbon core DLLs ==="
while IFS= read -r DLL; do
  NAME=$(basename "$DLL" .dll)
  decompile "$DLL" "carbon/$NAME"
done < <(find "$MANAGED" -maxdepth 1 -name "*.dll" | sort)

# ── carbon/managed/hooks/*.dll → carbon/hooks/ ──────────────────────────────
echo "=== Carbon hooks ==="
while IFS= read -r DLL; do
  NAME=$(basename "$DLL" .dll)
  decompile "$DLL" "carbon/hooks/$NAME"
done < <(find "$MANAGED/hooks" -maxdepth 1 -name "*.dll" 2>/dev/null | sort)

# ── carbon/managed/modules/*.dll → carbon/modules/ ──────────────────────────
echo "=== Carbon modules ==="
while IFS= read -r DLL; do
  NAME=$(basename "$DLL" .dll)
  decompile "$DLL" "carbon/modules/$NAME"
done < <(find "$MANAGED/modules" -maxdepth 1 -name "*.dll" 2>/dev/null | sort)

# ── carbon/managed/lib/ — SKIP (Carbon's own deps, not game API) ────────────
echo "  skip carbon/managed/lib/ (Carbon internal dependencies — not game API)"

echo "=== Carbon: done ==="
