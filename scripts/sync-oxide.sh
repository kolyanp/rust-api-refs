#!/usr/bin/env bash
set -euo pipefail
ZIP="$1"
WORK="/tmp/oxide-extract"
ILSPY="$HOME/.dotnet/tools/ilspycmd"

echo "=== Oxide: extracting $ZIP ==="
rm -rf "$WORK" && mkdir -p "$WORK"
unzip -q "$ZIP" -d "$WORK"

MANAGED=$(find "$WORK" -name "Oxide.Core.dll" | head -1 | xargs dirname)
if [ -z "$MANAGED" ]; then
  echo "ERROR: Oxide.Core.dll not found"
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
# Пропускаємо лише чисто Unity-внутрішні модулі (не потрібні для плагінів Rust)
SKIP_PATTERN="^(UnityEngine\.|Unity\.|Mono\.|netstandard|mscorlib|System\.|I18N|Microsoft\.|Azure\.|BouncyCastle|Ionic\.|LZ4|websocket-sharp|Discord\.)"

find "$MANAGED" -maxdepth 1 -name "*.dll" | sort | while read DLL; do
  NAME=$(basename "$DLL" .dll)
  if echo "$NAME" | grep -qE "$SKIP_PATTERN"; then
    echo "  skip (framework): $NAME.dll"
    continue
  fi
  # Визначаємо вихідну папку за префіксом
  if   echo "$NAME" | grep -qE "^Oxide\."; then
    OUT="oxide/$NAME"
  elif echo "$NAME" | grep -qE "^(Assembly-CSharp|Facepunch\.|Rust\.)"; then
    OUT="shared/$NAME"
  else
    OUT="shared/$NAME"
  fi
  decompile "$DLL" "$OUT"
done

echo "=== Oxide: done ==="
