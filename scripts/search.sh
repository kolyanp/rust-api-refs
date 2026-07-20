#!/usr/bin/env bash
# Search a symbol in decompiled Rust API sources.
# Usage: ./scripts/search.sh <SymbolName> [oxide|carbon|shared|all]
QUERY="$1"
SCOPE="${2:-all}"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"

if [ -z "$QUERY" ]; then
  echo "Usage: $0 <SymbolName> [oxide|carbon|shared|all]"
  exit 1
fi

search_dir() {
  local dir="$ROOT/$1" label="$1"
  [ -d "$dir" ] || return
  local results
  results=$(grep -rn "$QUERY" "$dir" --include="*.cs" -l 2>/dev/null)
  [ -z "$results" ] && return
  echo ""
  echo "╔══ $label ══"
  while IFS= read -r f; do
    echo "│ $(basename $f)"
    grep -n "$QUERY" "$f" | sed 's/^/│    /'
  done <<< "$results"
}

echo "🔍 '$QUERY' (scope: $SCOPE)"
case "$SCOPE" in
  oxide)  search_dir oxide ;;
  carbon) search_dir carbon ;;
  shared) search_dir shared ;;
  *)      search_dir shared; search_dir oxide; search_dir carbon ;;
esac
echo ""
