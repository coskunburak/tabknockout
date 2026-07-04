# AGENTS.md - Tap Knockout Codex Instructions

## Project

Tap Knockout is now a desktop-first Unity 3D arena survivor roguelike for PC/Steam-oriented development. The target loop is active arena survival: WASD movement, mouse aim, dash/evade, cooldown skills, XP pickups, level-up choices, escalating waves, elites, and boss milestones.

The previous mobile portrait, Archero-like room/chapter direction is legacy context only. Do not treat mobile touch controls, room clearing, ad-first monetization, or Android-first release planning as canonical for new work unless a future prompt explicitly asks to revisit them.

## Documentation Source

Primary production docs live here:

```text
Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/
```

The folder name is historical. The canonical product direction is documented in `00_README_INDEX.md` and `31_DESKTOP_SURVIVOR_PIVOT_PLAN.md`.

Before implementation tasks, read:

1. `Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/00_README_INDEX.md`
2. `Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/12_CODEX_AGENT_GUIDE.md`
3. `Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/31_DESKTOP_SURVIVOR_PIVOT_PLAN.md`
4. The task-specific docs listed in the sprint or prompt.

## Current Workflow Rule

Unity MCP is not approved for this project unless the user explicitly says it is working.

Default workflow:

- Work through repository/filesystem inspection.
- Do not inspect or modify the live Unity Editor.
- Do not directly edit `.unity` scene YAML.
- Prefer Editor tools and manual Unity setup instructions for future scene creation.

## Do Not

- Do not implement gameplay code during documentation/planning tasks.
- Do not add Unity packages without explicit approval.
- Do not import assets without explicit approval.
- Do not add real Ads, IAP, Analytics, crash, or remote config SDKs without explicit approval.
- Do not copy protected assets, UI, names, skill icons, balance tables, enemies, maps, store structure, or other protected content from reference games.
- Do not scan generated folders such as `Library/`, `Temp/`, `Logs/`, or build output unless the task requires a specific diagnostic.

## Do

- Keep implementation under `Assets/_Project/` once the production structure is created.
- Keep third-party assets under `Assets/ThirdParty/` after an approved migration.
- Use namespace prefix `TapKnockout.*`.
- Use data-driven ScriptableObject configs for gameplay, balance, progression, analytics, and optional future monetization.
- Use service abstractions for analytics, save, audio, remote config, and any future ads/IAP.
- Use object pooling for enemies, projectiles, VFX, pickups, and other runtime spawned objects.
- List changed files and validation steps in every implementation response.
