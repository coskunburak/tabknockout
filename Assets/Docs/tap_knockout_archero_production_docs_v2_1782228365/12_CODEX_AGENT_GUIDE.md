# Codex Agent Guide

## Role

Codex is a production engineering assistant for a Unity mobile roguelite action game.

## Required Reading

Before changes:

1. Root `AGENTS.md`
2. `00_README_INDEX.md`
3. `12_CODEX_AGENT_GUIDE.md`
4. `22_PRODUCTION_SPRINT_PLAN.md` for sprint execution
5. Task-specific document

For implementation prompts, also check:

- `21_VERTICAL_SLICE_SPEC.md`
- `24_DATA_CONFIG_SCHEMA.md`
- `25_PREFAB_AND_SCENE_CONTRACTS.md`
- `27_QA_PERFORMANCE_SOFT_LAUNCH_PLAN.md`

## MCP

Official Unity MCP may not be available due to Unity CapacityLimit.

Default:

```text
Do not use Unity MCP unless user confirms it works.
```

Use filesystem edits and Editor scripts.

## Scope Control

Before large implementation, Codex must state:

```text
Task understanding
Files to inspect
Files to change/create
Out of scope
Implementation plan
Manual Unity steps
Waiting for approval
```

## File Access Rules

Do not scan:

- Library
- Temp
- Build outputs
- Entire ThirdParty folders unless asset task

## Edit Rules

Do not:

- Modify ThirdParty source assets
- Add SDKs without approval
- Copy protected game content
- Directly edit `.unity` YAML unless approved
- Create giant managers
- Hardcode balance everywhere

Do:

- Use `Assets/_Project`
- Use namespaces `TapKnockout.*`
- Use ScriptableObjects/configs
- Use interfaces/events
- Use pooling
- Explain Unity manual setup
- List changed files, validation steps, and any manual Unity steps
- Keep branch/commit scope aligned with `22_PRODUCTION_SPRINT_PLAN.md`

## First Implementation Order

1. Folder structure
2. Core combat models/interfaces
3. Player movement
4. Auto-attack
5. Dash
6. Enemy
7. Wave/room
8. Ability selection
9. HUD
10. Editor scene builder
11. Save/meta stubs
12. Analytics/ads/IAP stubs
