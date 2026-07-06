# Tap Knockout Documentation Index

## Canonical Direction

Tap Knockout is now a desktop-first Unity 3D arena survivor roguelike for PC/Steam-oriented development.

Core loop:

```text
enter arena -> survive wave pressure -> kill enemies -> collect XP/drops -> level up -> choose abilities -> fight elites/bosses -> complete or fail the run
```

The target experience combines fast arena survival, build crafting, active skill mastery, readable 3D combat density, and escalating enemy pressure. Reference genres include Vampire Survivors-style escalation, Soulstone Survivors-style 3D arena combat, and Brotato-like upgrade pacing. These are design references only; assets, names, UI, balance, and content must remain original.

## Migration Note

This documentation package was originally created for a mobile portrait room-based action roguelite. As of the desktop survivor pivot, those ideas are legacy unless a document explicitly marks them as future optional support. The folder name still contains historical wording, but the canonical direction is desktop 3D survivor roguelike with active skills and wave-based progression.

## Required Reading

For any future implementation task, read in this order:

1. Root `AGENTS.md`
2. This index
3. `12_CODEX_AGENT_GUIDE.md`
4. `31_DESKTOP_SURVIVOR_PIVOT_PLAN.md`
5. `22_PRODUCTION_SPRINT_PLAN.md`
6. Task-specific docs below

## Documentation Map

| Doc | Canonical Purpose |
|---|---|
| `01_PRODUCT_VISION.md` | Product vision, player fantasy, audience, MVP goal. |
| `02_GDD_PRODUCTION.md` | Arena survivor game design, run loop, combat, progression, results. |
| `03_TECH_ARCHITECTURE_UNITY.md` | Desktop Unity architecture and runtime system plan. |
| `04_COMBAT_AND_ABILITIES.md` | Survivor combat, active skills, passives, modifiers, stacking. |
| `05_LEVEL_ROOM_WAVE.md` | Arena/run/wave timeline, spawn pressure, elite and boss milestones. |
| `06_META_ECONOMY.md` | Demo-friendly progression and future meta economy. |
| `07_MONETIZATION_LIVEOPS.md` | Future optional monetization/liveops after Steam demo validation. |
| `08_UI_UX_CONTROLS.md` | WASD, mouse aim, skill hotkeys, HUD, level-up UX. |
| `09_ASSET_PIPELINE.md` | 3D arena, enemy, VFX, pickup, icon, and license pipeline. |
| `10_ANALYTICS_REMOTE_CONFIG.md` | Run-based analytics and tuning keys. |
| `11_QA_PERFORMANCE_RELEASE.md` | Desktop survivor QA, performance, release gates. |
| `12_CODEX_AGENT_GUIDE.md` | Codex workflow and repository safety rules. |
| `13_ROADMAP.md` | Pivot roadmap from docs to Steam-facing demo. |
| `14_RISK_REGISTER.md` | Product, tech, performance, balance, and migration risks. |
| `15_STORE_COMPLIANCE.md` | Steam/demo compliance and future platform gates. |
| `16_CODEX_PROMPTS.md` | Future task prompts aligned to the desktop survivor pivot. |
| `17_CREDITS_TEMPLATE.md` | Asset and SDK credit tracking template. |
| `18_REPOSITORY_DISCOVERY_AND_DOCUMENTATION_AUDIT.md` | Repository and documentation audit after pivot. |
| `19_MISSING_DOCUMENTATION_PLAN.md` | Documentation coverage plan and consolidation choices. |
| `20_BACKLOG_MASTER.md` | Backlog grouped by survivor systems. |
| `21_VERTICAL_SLICE_SPEC.md` | MVP vertical slice scope and acceptance criteria. |
| `22_PRODUCTION_SPRINT_PLAN.md` | First pivot sprint: prototype foundation. |
| `23_TECHNICAL_DECISIONS_ADR.md` | Durable technical decisions. |
| `24_DATA_CONFIG_SCHEMA.md` | ScriptableObject/config schema for arena survivor systems. |
| `25_PREFAB_AND_SCENE_CONTRACTS.md` | Scene and prefab contracts for desktop survivor prototype. |
| `26_MONETIZATION_ANALYTICS_REMOTE_CONFIG_SPEC.md` | Detailed telemetry, remote config, and future monetization spec. |
| `27_QA_PERFORMANCE_SOFT_LAUNCH_PLAN.md` | Desktop demo QA and performance validation plan. |
| `28_CONTENT_PIPELINE_BALANCING_EDITOR_TOOLS.md` | Survivor balancing sheets and Editor tool plan. |
| `29_RELEASE_BRANCHING_AND_GIT_WORKFLOW.md` | Git, branch, release, and Steam demo workflow. |
| `30_ABILITY_AND_ENEMY_DESIGN_CATALOG.md` | Initial survivor ability and enemy catalog. |
| `31_DESKTOP_SURVIVOR_PIVOT_PLAN.md` | Pivot summary, migration phases, risks, acceptance. |

## Deprecated Legacy Concepts

- Mobile-first portrait orientation.
- Touch joystick as the primary control model.
- Room-clearing as the main gameplay unit.
- Chapter maps as the primary run structure.
- Rewarded-ad-first progression.
- Android-first store and soft-launch planning.

These concepts may return only as explicitly scoped future options. They are not MVP requirements.
