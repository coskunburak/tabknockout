# Codex Agent Guide

## Canonical Product Direction

Tap Knockout is now a desktop-first 3D arena survivor roguelike. Future work should optimize for PC/Steam, WASD movement, mouse aim, active skills, dash/evade, XP pickups, level-up choices, wave/timer progression, elite milestones, boss encounters, and performance under enemy density.

The previous mobile room-based direction is legacy. Do not use it as the default implementation target.

## Required Reading

Before implementation tasks, read:

1. Root `AGENTS.md`
2. `00_README_INDEX.md`
3. This guide
4. `31_DESKTOP_SURVIVOR_PIVOT_PLAN.md`
5. `22_PRODUCTION_SPRINT_PLAN.md`
6. Task-specific docs

For implementation tasks involving configs or prefabs, also read:

- `24_DATA_CONFIG_SCHEMA.md`
- `25_PREFAB_AND_SCENE_CONTRACTS.md`

## Workflow Rules

- Do not use Unity MCP unless the user explicitly says it is working.
- Do not inspect or operate the live Unity Editor by default.
- Do not directly edit `.unity` scene YAML.
- Do not add packages without approval.
- Do not import or migrate assets without approval.
- Do not add real Ads, IAP, Analytics, crash, or remote config SDKs without approval.
- Do not implement gameplay code during documentation/planning tasks.
- Do not scan generated folders unless a specific diagnostic requires it.

Generated folders include:

```text
Library/
Temp/
Logs/
Build/
Builds/
UserSettings/
```

## Implementation Style

- Keep production code under `Assets/_Project`.
- Use namespace prefix `TapKnockout.*`.
- Prefer small focused components over monolithic managers.
- Use ScriptableObject configs with stable IDs.
- Use object pooling for runtime spawned enemies, projectiles, VFX, and pickups.
- Use service interfaces for analytics, save, remote config, audio, and future optional monetization.
- Use Editor tools or manual Unity setup for scene creation.
- Keep changes scoped to the requested sprint.

## Documentation Language

When implementation is not verified, use:

- `planned`
- `target`
- `MVP requirement`
- `TODO`

Do not claim a system exists in code unless repository inspection confirms it.

## Final Response Checklist

Every implementation response should include:

- Changed files.
- Validation steps.
- Tests run or why tests were not run.
- Manual Unity steps if needed.
- Known TODOs or unresolved decisions.

For documentation-only tasks, explicitly state that no gameplay code or `.meta` files were intentionally changed.
