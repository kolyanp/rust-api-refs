#!/usr/bin/env bash
# Checks whether key game/Oxide/Carbon API symbols used by RustForge plugins
# still exist in the freshly-decompiled refs.
# Exits 0 = all good, 1 = symbols missing (caller should open a GitHub Issue).
set -uo pipefail

MISSING=()

check() {
  local SYMBOL="$1"
  local DIR="$2"
  if grep -rq "$SYMBOL" "$DIR/" 2>/dev/null; then
    echo "  OK: $SYMBOL"
  else
    echo "  MISSING: $SYMBOL in $DIR/"
    MISSING+=("$SYMBOL ($DIR)")
  fi
}

echo "=== RustForge API compat check ==="

echo "--- Game types (shared/) ---"
check "class BasePlayer"           shared
check "enum PlayerFlags"           shared
check "class BaseEntity"           shared
check "class BaseNetworkable"      shared
check "class Item "                shared
check "class ItemContainer"        shared
check "class LootContainer"        shared
check "class StorageContainer"     shared
check "class BaseProjectile"       shared
check "class BuildingBlock"        shared
check "class Construction"         shared
check "class ConVar"               shared

echo "--- Oxide API (oxide/) ---"
check "class CSharpPlugin"         oxide
check "class RustPlugin"           oxide
check "interface IPlayer"          oxide
check "class PluginManager"        oxide

echo "--- Carbon API (carbon/) ---"
check "class CSharpPlugin"         carbon
check "interface IPlayer"          carbon

if [ ${#MISSING[@]} -eq 0 ]; then
  echo ""
  echo "=== All symbols present — no breaking changes detected ==="
  exit 0
fi

echo ""
echo "=== BREAKING CHANGES DETECTED: ${#MISSING[@]} symbol(s) missing ==="
for s in "${MISSING[@]}"; do
  echo "  - $s"
done
exit 1
