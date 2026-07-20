#!/usr/bin/env bash
# Decompiles the entire Oxide.Rust.zip Managed folder.
# Structure: RustDedicated_Data/Managed/*.dll  (game DLLs + Oxide DLLs, all flat)
set -uo pipefail

ZIP="$1"
WORK="/tmp/oxide-extract"
ILSPY="$HOME/.dotnet/tools/ilspycmd"

echo "=== Oxide: extracting ==="
rm -rf "$WORK" && mkdir -p "$WORK"
unzip -q "$ZIP" -d "$WORK"

# Find Managed folder (contains Oxide.Core.dll)
MANAGED=$(find "$WORK" -name "Oxide.Core.dll" 2>/dev/null | head -1 | xargs -I{} dirname {})
if [ -z "$MANAGED" ]; then
  echo "ERROR: Oxide.Core.dll not found in archive"
  unzip -l "$ZIP" | grep "\.dll" | head -20
  exit 1
fi
echo "Managed: $MANAGED"

# Skip pure framework / Unity internals — not useful for plugin authors
is_skip() {
  local n="$1"
  case "$n" in
    UnityEngine.*|Unity.*|Mono.*|mscorlib|netstandard) return 0 ;;
    System.*|I18N*|Microsoft.*|Azure.*) return 0 ;;
    BouncyCastle*|Ionic.*|LZ4*|ZString*|websocket-sharp) return 0 ;;
    Discord.*|SingularityGroup.*|GBG.*|RTLTMPro*) return 0 ;;
    *) return 1 ;;
  esac
}

# Route to output folder by DLL name prefix
out_dir() {
  local n="$1"
  case "$n" in
    Oxide.*)      echo "oxide/$n" ;;
    Facepunch.*)  echo "shared/$n" ;;
    Rust.*)       echo "shared/$n" ;;
    Assembly-*)   echo "shared/$n" ;;
    *)            echo "shared/$n" ;;
  esac
}

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

echo "=== Oxide: decompiling all DLLs in Managed ==="
while IFS= read -r DLL; do
  NAME=$(basename "$DLL" .dll)
  if is_skip "$NAME"; then
    echo "  skip (framework): $NAME.dll"
    continue
  fi
  OUT=$(out_dir "$NAME")
  decompile "$DLL" "$OUT"
done < <(find "$MANAGED" -maxdepth 1 -name "*.dll" | sort)

echo "=== Oxide: done ==="
