# Production Sprint Plan

## Sprint 1 - Desktop Survivor Pivot — Prototype Foundation

Status: Planned. Do not claim implementation complete until the sprint is actually executed and validated.

Recommended duration: 1 week.

## Objective

Prepare the project for a desktop 3D arena survivor prototype without implementing the full game loop prematurely.

## Required Reading

- Root `AGENTS.md`
- `00_README_INDEX.md`
- `12_CODEX_AGENT_GUIDE.md`
- `21_VERTICAL_SLICE_SPEC.md`
- `23_TECHNICAL_DECISIONS_ADR.md`
- `24_DATA_CONFIG_SCHEMA.md`
- `25_PREFAB_AND_SCENE_CONTRACTS.md`
- `31_DESKTOP_SURVIVOR_PIVOT_PLAN.md`

## Goals

- Create the `DesktopSurvivorPrototype` scene plan.
- Define input/camera architecture.
- Define arena run director.
- Define spawn director.
- Define XP/level-up loop.
- Document which existing mobile systems are reused, deprecated, or replaced.
- Keep implementation safe for future scene creation by manual setup or approved Editor tools.

## Tasks

### Documentation and Audit

- Search current docs and code for mobile, touch, room, Android, ad, and daily economy assumptions.
- List systems that can be preserved.
- List systems that should be isolated as legacy.
- Confirm no `.meta` or scene YAML edits are needed for the planning step.

### Input and Camera Design

- Define `DesktopInputController`.
- Define `MouseAimController`.
- Define `SurvivorCameraRig`.
- Define camera bounds, zoom, follow, and shake hooks.
- Document manual scene setup requirements.

### Arena Run Design

- Define `ArenaRunDirector` responsibilities.
- Define run states: loading, playing, level_up_paused, boss_active, complete, failed.
- Define run timer and result summary.
- Define interaction with UI and analytics interfaces.

### Spawn and Wave Design

- Define `SpawnDirector`.
- Define `WaveDirector`.
- Define spawn ring/safety radius.
- Define live enemy budget.
- Define elite and boss milestone hooks.

### XP and Level-Up Design

- Define `XPOrb` and pickup pooling requirements.
- Define XP curve ownership.
- Define `LevelUpSelectionController`.
- Define ability offer rules.
- Define safe pause/resume behavior.

### Reuse, Deprecate, Replace

Reuse:

- Damage/health contracts.
- Projectile logic where compatible.
- Ability configs and tags after migration.
- Enemy/boss configs after schema update.
- Wave config concepts.
- Object pooling principles.
- QA/performance planning.

Deprecate:

- Touch joystick as primary input.
- Room clear as primary progression.
- Android-first build gates.
- Rewarded-ad-first economy.

Replace:

- Chapter/room runner with arena run director.
- Room spawn points as the primary model with spawn rings/budgets.
- Mobile HUD with desktop HUD.
- Room reward flow with XP/drop/run result flow.

## Deliverables

- Updated implementation notes if gaps are found.
- Optional skeleton code only if a future prompt explicitly requests implementation.
- Manual Unity setup instructions for prototype scene.
- Validation checklist for the first playable prototype.

## Acceptance Criteria

- Future implementation tasks have a clear desktop survivor target.
- Legacy mobile/room assumptions are identified.
- No gameplay code is implemented during documentation-only work.
- No `.unity` scene YAML is directly edited.
- No packages or SDKs are added.

## Suggested Branch

`codex/desktop-survivor-pivot-foundation`

## Suggested Commits

- `docs: migrate project docs to desktop survivor`
- `docs: add desktop survivor pivot plan`
- `docs: define prototype foundation sprint`
