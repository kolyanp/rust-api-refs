# rust-api-refs

Auto-updated decompiled Rust Game API sources for **Oxide** and **Carbon** plugin development.

Updates daily at 08:00 UTC via GitHub Actions.

## Structure

```
shared/    ← Assembly-CSharp (game) — PlayerFlags, BasePlayer, etc.
oxide/     ← Oxide/uMod API
carbon/    ← Carbon API
```

## Search a symbol

```bash
./scripts/search.sh PlayerFlags
./scripts/search.sh "IPlayer" oxide
./scripts/search.sh "HookResult" carbon
```

## Manual update trigger

GitHub → Actions → "Sync Rust API References" → Run workflow
