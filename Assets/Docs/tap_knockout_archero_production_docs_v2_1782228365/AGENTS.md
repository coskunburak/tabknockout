# AGENTS.md - Codex Package Instructions

Note: a root `AGENTS.md` also exists at the project root so future Codex runs can find these rules automatically. This file remains the documentation package copy.

## Project

Tap Knockout is now a desktop-first Unity 3D arena survivor roguelike. The target platform is PC/Steam first. The core loop is arena survival with WASD movement, mouse aim, dash/evade, active skills, XP pickups, level-up choices, escalating wave pressure, elites, and boss milestones.

The older mobile portrait room/chapter direction is deprecated legacy context. Future work should not add mobile touch, room-first progression, rewarded-ad-first economy, or Android release assumptions unless a new prompt explicitly scopes that support.

## Default Rule

Unity MCP may be unavailable. Unless the user explicitly says MCP works:

```text
Do not use Unity MCP.
Work through filesystem edits, approved Editor scripts, and manual Unity setup instructions only.
```

## Required Reading

Before changing code, read:

1. `00_README_INDEX.md`
2. `12_CODEX_AGENT_GUIDE.md`
3. `31_DESKTOP_SURVIVOR_PIVOT_PLAN.md`
4. `22_PRODUCTION_SPRINT_PLAN.md` for sprint execution
5. The task-specific document
6. `24_DATA_CONFIG_SCHEMA.md` and `25_PREFAB_AND_SCENE_CONTRACTS.md` for implementation tasks

## Do Not

- Do not scan `Library/`, `Temp/`, `Builds/`, or huge generated folders.
- Do not modify `Assets/ThirdParty` source files directly.
- Do not copy protected assets, UI, names, skills, icons, balance, or store design from reference games.
- Do not add real Ads/IAP/Analytics SDKs without explicit approval.
- Do not directly edit `.unity` scene YAML unless explicitly approved.
- Do not create giant all-purpose managers.
- Do not hardcode balance values across multiple scripts.
- Do not implement gameplay code during documentation-only tasks.

## Do

- Use `Assets/_Project`.
- Use namespaces under `TapKnockout`.
- Use small components.
- Use ScriptableObject configs.
- Use interfaces for analytics, save, remote config, audio, and future optional monetization.
- Use object pooling for runtime spawned enemies, projectiles, VFX, and pickups.
- Provide exact Unity Editor manual setup steps.
- Keep changes scoped and reviewable.
