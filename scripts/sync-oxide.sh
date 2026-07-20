#!/usr/bin/env bash
set -euo pipefail
ZIP="$1"
WORK="/tmp/oxide-extract"
ILSPY="$HOME/.dotnet/tools/ilspycmd"

echo "=== Oxide: extracting $ZIP ==="
rm -rf "$WORK" && mkdir -p "$WORK"
unzip -q "$ZIP" -d "$WORK"

MANAGED=$(find "$WORK" -name "Assembly-CSharp.dll" -not -path "*/Carbon/*" | head -1 | xargs dirname)
if [ -z "$MANAGED" ]; then
  echo "ERROR: Assembly-CSharp.dll not found in Oxide archive"
  exit 1
fi
echo "Managed path: $MANAGED"

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

decompile "$MANAGED/Assembly-CSharp.dll"           "shared/Assembly-CSharp"
decompile "$MANAGED/Assembly-CSharp-firstpass.dll" "shared/Assembly-CSharp-firstpass"
decompile "$MANAGED/Facepunch.System.dll"          "shared/Facepunch.System"
decompile "$MANAGED/Rust.Global.dll"               "shared/Rust.Global"

OXIDE_DIR="$MANAGED/Oxide"
[ -d "$OXIDE_DIR" ] || OXIDE_DIR=$(find "$WORK" -type d -name "Oxide" | head -1)
decompile "$OXIDE_DIR/Oxide.Core.dll"       "oxide/Oxide.Core"
decompile "$OXIDE_DIR/Oxide.CSharp.dll"     "oxide/Oxide.CSharp"
decompile "$OXIDE_DIR/Oxide.Ext.Rust.dll"   "oxide/Oxide.Ext.Rust"
decompile "$OXIDE_DIR/Oxide.Common.dll"     "oxide/Oxide.Common"
decompile "$OXIDE_DIR/Oxide.References.dll" "oxide/Oxide.References"

echo "=== Oxide: done ==="
