#!/usr/bin/env bash
set -euo pipefail
ZIP="$1"
WORK="/tmp/carbon-extract"
ILSPY="$HOME/.dotnet/tools/ilspycmd"

echo "=== Carbon: extracting $ZIP ==="
rm -rf "$WORK" && mkdir -p "$WORK"
unzip -q "$ZIP" -d "$WORK"

decompile() {
  local DLL="$1" OUT="$2"
  [ -f "$DLL" ] || { echo "SKIP (missing): $DLL"; return; }
  local HASH_FILE="$OUT/.dll_hash"
  local CURR_HASH; CURR_HASH=$(sha256sum "$DLL" | cut -d' ' -f1)
  if [ -f "$HASH_FILE" ] && [ "$(cat "$HASH_FILE")" = "$CURR_HASH" ]; then
    echo "SKIP (unchanged): $(basename $DLL)"; return
  fi
  echo "Decompiling: $(basename $DLL) → $OUT"
  rm -rf "$OUT" && mkdir -p "$OUT"
  "$ILSPY" "$DLL" -p -o "$OUT" --no-dead-code 2>/dev/null || "$ILSPY" "$DLL" -p -o "$OUT" 2>&1 | tail -5
  echo "$CURR_HASH" > "$HASH_FILE"
}

CARBON_DLL=$(find "$WORK" -name "Carbon.dll"        | head -1)
API_DLL=$(find    "$WORK" -name "Carbon.API.dll"    | head -1)
COMPAT_DLL=$(find "$WORK" -name "Carbon.Compat.dll" | head -1)
CSHARP_DLL=$(find "$WORK" -name "Assembly-CSharp.dll" | head -1)

[ -n "$CARBON_DLL"  ] && decompile "$CARBON_DLL"  "carbon/Carbon"
[ -n "$API_DLL"     ] && decompile "$API_DLL"      "carbon/Carbon.API"
[ -n "$COMPAT_DLL"  ] && decompile "$COMPAT_DLL"   "carbon/Carbon.Compat"
[ -n "$CSHARP_DLL"  ] && decompile "$CSHARP_DLL"   "carbon/Assembly-CSharp"

echo "=== Carbon: done ==="
