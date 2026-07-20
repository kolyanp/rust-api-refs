#!/usr/bin/env bash
set -euo pipefail
ZIP="$1"
WORK="/tmp/carbon-extract"
ILSPY="$HOME/.dotnet/tools/ilspycmd"

echo "=== Carbon: extracting $ZIP ==="
rm -rf "$WORK" && mkdir -p "$WORK"
unzip -q "$ZIP" -d "$WORK"

# Знаходимо папку з DLL Carbon
MANAGED=$(find "$WORK" -name "Carbon.dll" 2>/dev/null | head -1 | xargs dirname 2>/dev/null || true)
if [ -z "$MANAGED" ]; then
  # Спробуємо знайти за Assembly-CSharp
  MANAGED=$(find "$WORK" -name "Assembly-CSharp.dll" 2>/dev/null | head -1 | xargs dirname 2>/dev/null || true)
fi
if [ -z "$MANAGED" ]; then
  echo "ERROR: no managed DLLs found. Archive structure:"
  unzip -l "$ZIP" | grep "\.dll" | head -20
  exit 1
fi
echo "Managed: $MANAGED"
echo "Total DLLs: $(find "$MANAGED" -maxdepth 1 -name '*.dll' | wc -l)"

decompile() {
  local DLL="$1" OUT="$2"
  [ -f "$DLL" ] || return
  local HASH_FILE="$OUT/.dll_hash"
  local CURR_HASH; CURR_HASH=$(sha256sum "$DLL" | cut -d' ' -f1)
  if [ -f "$HASH_FILE" ] && [ "$(cat "$HASH_FILE")" = "$CURR_HASH" ]; then
    echo "  skip (unchanged): $(basename $DLL)"; return
  fi
  echo "  decompile: $(basename $DLL)"
  rm -rf "$OUT" && mkdir -p "$OUT"
  "$ILSPY" "$DLL" -p -o "$OUT" --no-dead-code 2>/dev/null \
    || "$ILSPY" "$DLL" -p -o "$OUT" 2>&1 | tail -3
  echo "$CURR_HASH" > "$HASH_FILE"
}

# ── Деомпілювати ВСЮ папку Managed ─────────────────────────────────────────
SKIP_PATTERN="^(UnityEngine\.|Unity\.|Mono\.|netstandard|mscorlib|System\.|I18N|Microsoft\.|Azure\.|BouncyCastle|Ionic\.|LZ4|websocket-sharp|Discord\.)"

find "$MANAGED" -maxdepth 1 -name "*.dll" | sort | while read DLL; do
  NAME=$(basename "$DLL" .dll)
  if echo "$NAME" | grep -qE "$SKIP_PATTERN"; then
    echo "  skip (framework): $NAME.dll"
    continue
  fi
  # Визначаємо вихідну папку за префіксом
  if   echo "$NAME" | grep -qE "^Carbon\."; then
    OUT="carbon/$NAME"
  elif echo "$NAME" | grep -qE "^(Assembly-CSharp|Facepunch\.|Rust\.)"; then
    OUT="shared/$NAME"
  elif echo "$NAME" | grep -qE "^Oxide\."; then
    OUT="oxide/$NAME"
  else
    OUT="shared/$NAME"
  fi
  decompile "$DLL" "$OUT"
done

echo "=== Carbon: done ==="
