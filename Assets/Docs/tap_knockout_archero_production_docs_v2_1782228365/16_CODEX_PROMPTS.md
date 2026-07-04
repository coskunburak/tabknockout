# Codex Task Prompts

These prompts are for future work after the desktop survivor documentation pivot. They assume Unity MCP is unavailable unless the user explicitly says otherwise.

## Prompt 1 - Inspect Project After Pivot

```text
Read AGENTS.md, 00_README_INDEX.md, 12_CODEX_AGENT_GUIDE.md, 18_REPOSITORY_DISCOVERY_AND_DOCUMENTATION_AUDIT.md, 22_PRODUCTION_SPRINT_PLAN.md, and 31_DESKTOP_SURVIVOR_PIVOT_PLAN.md.

Inspect the repository through the filesystem only. Do not use Unity MCP. Do not modify files.

Return:
1. Current project structure
2. Existing Assets/_Project systems
3. Remaining mobile/room legacy references in code or docs
4. Existing scene/prefab/config assets relevant to desktop survivor
5. Immediate risks before implementation
6. Recommended first safe implementation branch
```

## Prompt 2 - Desktop Survivor Prototype Foundation

```text
Implement only the foundation requested by 22_PRODUCTION_SPRINT_PLAN.md.

Read AGENTS.md, 03_TECH_ARCHITECTURE_UNITY.md, 21_VERTICAL_SLICE_SPEC.md, 22_PRODUCTION_SPRINT_PLAN.md, 24_DATA_CONFIG_SCHEMA.md, 25_PREFAB_AND_SCENE_CONTRACTS.md, and 31_DESKTOP_SURVIVOR_PIVOT_PLAN.md.

Scope:
- Define or implement safe foundations for DesktopInputController, MouseAimController, SurvivorCameraRig, ArenaRunDirector, SpawnDirector/WaveDirector, XP/level-up contracts as requested by the sprint.
- Keep code under Assets/_Project.
- Do not directly edit .unity YAML.
- Do not add packages or SDKs.

Return changed files, tests, manual Unity setup, and validation.
```

## Prompt 3 - WASD Movement and Mouse Aim

```text
Implement desktop player input and aim.

Read 02_GDD_PRODUCTION.md, 03_TECH_ARCHITECTURE_UNITY.md, 08_UI_UX_CONTROLS.md, 24_DATA_CONFIG_SCHEMA.md, and 25_PREFAB_AND_SCENE_CONTRACTS.md.

Scope:
- WASD movement input.
- Mouse world aim direction.
- Dash input binding.
- No mobile joystick work.
- No scene YAML edits.

Return changed files, tests, and manual prefab setup.
```

## Prompt 4 - Arena Run and Spawn Director

```text
Implement the arena run director and spawn director foundation.

Read 05_LEVEL_ROOM_WAVE.md, 21_VERTICAL_SLICE_SPEC.md, 24_DATA_CONFIG_SCHEMA.md, 25_PREFAB_AND_SCENE_CONTRACTS.md, and 30_ABILITY_AND_ENEMY_DESIGN_CATALOG.md.

Scope:
- Run timer.
- Wave timeline evaluation.
- Spawn group selection.
- Spawn safety radius.
- Enemy live budget.
- Elite and boss milestone hooks.

Out of scope:
- Full boss implementation.
- Final balance.
- Scene YAML edits.
```

## Prompt 5 - XP, Pickups, and Level-Up Choices

```text
Implement survivor XP and level-up foundation.

Read 04_COMBAT_AND_ABILITIES.md, 08_UI_UX_CONTROLS.md, 10_ANALYTICS_REMOTE_CONFIG.md, 21_VERTICAL_SLICE_SPEC.md, and 24_DATA_CONFIG_SCHEMA.md.

Scope:
- XP orb pickup contract.
- XP curve.
- Level-up trigger.
- Weighted 3-choice ability offer.
- Apply selected ability to run state.
- Use local/no-op analytics only.
```

## Prompt 6 - Active Skill Runtime

```text
Implement active skill runtime foundation.

Read 04_COMBAT_AND_ABILITIES.md, 08_UI_UX_CONTROLS.md, 24_DATA_CONFIG_SCHEMA.md, and 30_ABILITY_AND_ENEMY_DESIGN_CATALOG.md.

Scope:
- Active skill slots.
- Cooldowns.
- Duration.
- Hotkey input.
- Runtime ability state.
- Hooks for VFX/audio without final assets.
```

## Prompt 7 - Enemy Archetypes for Survivor MVP

```text
Implement the first survivor enemy archetypes.

Read 03_TECH_ARCHITECTURE_UNITY.md, 05_LEVEL_ROOM_WAVE.md, 21_VERTICAL_SLICE_SPEC.md, 24_DATA_CONFIG_SCHEMA.md, 25_PREFAB_AND_SCENE_CONTRACTS.md, and 30_ABILITY_AND_ENEMY_DESIGN_CATALOG.md.

Scope:
- Basic melee chaser.
- Swarm enemy.
- Ranged shooter.
- Charger.
- Tank.
- Shared health/damage flow.
- Pool-friendly lifecycle.
```

## Prompt 8 - Elite and Boss Milestone

```text
Implement elite and boss milestone foundation.

Read 05_LEVEL_ROOM_WAVE.md, 08_UI_UX_CONTROLS.md, 21_VERTICAL_SLICE_SPEC.md, 24_DATA_CONFIG_SCHEMA.md, 25_PREFAB_AND_SCENE_CONTRACTS.md, and 30_ABILITY_AND_ENEMY_DESIGN_CATALOG.md.

Scope:
- Elite spawn warning.
- Boss warning.
- Boss health bar binding.
- Boss defeat event.
- Run completion hook.
```

## Prompt 9 - Desktop HUD and Level-Up UI

```text
Implement the MVP desktop gameplay UI.

Read 08_UI_UX_CONTROLS.md, 21_VERTICAL_SLICE_SPEC.md, and 25_PREFAB_AND_SCENE_CONTRACTS.md.

Scope:
- Health bar.
- XP bar.
- Run timer.
- Active skill cooldown slots.
- Boss bar.
- Wave/boss warning.
- Level-up modal.
- Run result screen.
```

## Prompt 10 - Prototype Scene Builder

```text
Create an approved Editor tool for DesktopSurvivorPrototype scene setup.

Read 03_TECH_ARCHITECTURE_UNITY.md, 21_VERTICAL_SLICE_SPEC.md, 25_PREFAB_AND_SCENE_CONTRACTS.md, and 28_CONTENT_PIPELINE_BALANCING_EDITOR_TOOLS.md.

Scope:
- Editor menu action.
- Creates scene hierarchy only after user action.
- Does not silently overwrite scenes.
- Creates arena root, player spawn, camera rig, managers, HUD placeholder, and spawn anchors or spawn ring helper.
- No direct .unity YAML edits.
```
