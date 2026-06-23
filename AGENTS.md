# AGENTS.md - Tap Knockout Codex Instructions

## Project

Tap Knockout is a production Unity 3D portrait mobile action roguelite. The reference genre structure is Archero-style room/chapter progression, but the game identity must be original and centered on dash-impact combat.

## Documentation Source

Primary production docs live here:

```text
Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/
```

Before implementation tasks, read:

1. `Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/00_README_INDEX.md`
2. `Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/12_CODEX_AGENT_GUIDE.md`
3. The task-specific docs listed in the sprint or prompt.

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
- Do not copy Archero assets, UI, names, skill icons, balance tables, enemies, maps, store structure, or protected content.
- Do not scan generated folders such as `Library/`, `Temp/`, `Logs/`, or build output unless the task requires a specific diagnostic.

## Do

- Keep implementation under `Assets/_Project/` once the production structure is created.
- Keep third-party assets under `Assets/ThirdParty/` after an approved migration.
- Use namespace prefix `TapKnockout.*`.
- Use data-driven ScriptableObject configs for gameplay, economy, monetization, and balance.
- Use service abstractions for analytics, ads, IAP, save, audio, and remote config.
- Use object pooling for combat runtime objects.
- List changed files and validation steps in every implementation response.

