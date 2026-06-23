# AGENTS.md — Codex Package Instructions

Note: a root `AGENTS.md` also exists at the project root so future Codex runs can find these rules automatically. This file remains as the production documentation package copy.

## Project

Tap Knockout is a Unity 3D portrait mobile action roguelite with Archero-style room progression and an original dash-impact combat identity.

## Default Rule

Official Unity MCP may be unavailable due to Unity plan CapacityLimit. Unless user explicitly says MCP works:

```text
Do not use Unity MCP.
Work through filesystem edits and Editor scripts only.
```

## Required Reading

Before changing code, read:

1. `00_README_INDEX.md`
2. `12_CODEX_AGENT_GUIDE.md`
3. `22_PRODUCTION_SPRINT_PLAN.md` for sprint execution
4. The task-specific document
5. `24_DATA_CONFIG_SCHEMA.md` and `25_PREFAB_AND_SCENE_CONTRACTS.md` for implementation tasks

## Do Not

- Do not scan `Library/`, `Temp/`, `Builds/`, or huge generated folders.
- Do not modify `Assets/ThirdParty` source files directly.
- Do not copy Archero assets, UI, names, skills, icons, balance, or store design.
- Do not add real Ads/IAP/Analytics SDKs without explicit approval.
- Do not directly edit `.unity` scene YAML unless explicitly approved.
- Do not create giant all-purpose managers.
- Do not hardcode balance values across multiple scripts.

## Do

- Use `Assets/_Project`.
- Use namespaces under `TapKnockout`.
- Use small components.
- Use ScriptableObject configs.
- Use interfaces for analytics, ads, IAP, save, remote config.
- Use object pooling for runtime spawned objects.
- Provide exact Unity Editor manual setup steps.
- Keep changes scoped and reviewable.
