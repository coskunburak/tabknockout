# Codex Task Prompts

These prompts are designed for future implementation phases. They assume Unity MCP is unavailable unless the user explicitly says otherwise.

## Prompt 1 - Inspect Current Project and Docs

```text
You are working on Tap Knockout, a production Unity 3D portrait mobile action roguelite with an original dash-impact combat identity.

Do not use Unity MCP. Work only through repository/filesystem inspection.

Read:
- AGENTS.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/00_README_INDEX.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/12_CODEX_AGENT_GUIDE.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/18_REPOSITORY_DISCOVERY_AND_DOCUMENTATION_AUDIT.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/22_PRODUCTION_SPRINT_PLAN.md

Inspect:
- Root folder
- Assets, but do not scan Library/Temp/Logs/build outputs
- Packages/manifest.json
- ProjectSettings/ProjectVersion.txt
- ProjectSettings/EditorBuildSettings.asset
- Existing docs

Scope:
- Discovery only.
- Do not modify files.

Out of scope:
- Gameplay code
- Scene edits
- Package installs
- Asset imports
- Real SDK work

Return:
1. Current project structure
2. Existing scenes/scripts/packages
3. Missing production folders
4. Current documentation status
5. Immediate implementation risks
6. Proposed first safe branch/task
7. Files you would create/change next
8. Manual Unity checks required
```

## Prompt 2 - Create Production Folder Structure

```text
You are preparing Tap Knockout for production implementation.

Do not use Unity MCP. Do not modify scenes. Do not implement gameplay code.

Read:
- AGENTS.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/03_TECH_ARCHITECTURE_UNITY.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/12_CODEX_AGENT_GUIDE.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/22_PRODUCTION_SPRINT_PLAN.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/25_PREFAB_AND_SCENE_CONTRACTS.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/28_CONTENT_PIPELINE_BALANCING_EDITOR_TOOLS.md

Inspect:
- Assets/
- Existing Assets/Docs/
- Existing staged asset pack folders only at shallow depth

Scope:
- Create Assets/_Project production folders.
- Create Assets/ThirdParty folder structure.
- Add short README.md files in key folders if useful.
- Do not move existing asset packs unless explicitly approved.

Out of scope:
- Gameplay scripts
- Scene YAML edits
- Asset import/migration
- Package changes
- SDK work

Return:
- Changed files/folders
- Manual Unity project setting checklist for portrait Android
- Validation steps
```

## Prompt 3 - Create Core Combat Interfaces and Data Models

```text
Implement core combat contracts for Tap Knockout.

Do not use Unity MCP. Do not modify scenes.

Read:
- AGENTS.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/03_TECH_ARCHITECTURE_UNITY.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/04_COMBAT_AND_ABILITIES.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/12_CODEX_AGENT_GUIDE.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/23_TECHNICAL_DECISIONS_ADR.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/24_DATA_CONFIG_SCHEMA.md

Inspect:
- Assets/_Project/Scripts if it exists
- Any existing combat/core scripts

Scope:
- Create IDamageable.
- Create HitContext.
- Create DamageType.
- Create KnockbackData or equivalent.
- Create lightweight combat event contracts.
- Use namespace TapKnockout.*.
- Add focused EditMode tests if test structure exists or can be created safely.

Out of scope:
- Player movement
- Player attack behavior
- Enemy behavior
- Dash implementation
- Scene/prefab edits

Return:
- Changed files
- Test results or reason tests could not run
- Manual Unity validation steps
```

## Prompt 4 - Create Player Movement Foundation

```text
Implement the player movement foundation for Tap Knockout.

Do not use Unity MCP. Do not directly edit .unity scene YAML.

Read:
- AGENTS.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/02_GDD_PRODUCTION.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/03_TECH_ARCHITECTURE_UNITY.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/08_UI_UX_CONTROLS.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/24_DATA_CONFIG_SCHEMA.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/25_PREFAB_AND_SCENE_CONTRACTS.md

Inspect:
- Assets/_Project/Scripts/Input
- Assets/_Project/Scripts/Player
- Packages/manifest.json for input package

Scope:
- Implement drag/virtual-joystick-ready movement input abstraction.
- Implement PlayerMovementController.
- Track last movement/facing direction.
- Expose movement threshold needed by stop-to-attack.
- Create PlayerConfig fields required for movement if not already present.

Out of scope:
- Attacking
- Dash
- Enemies
- Ability system
- Scene edits

Return:
- Changed files
- Manual Unity setup for a player prefab
- Editor/mobile validation steps
```

## Prompt 5 - Create Auto-Attack and Targeting Foundation

```text
Implement stop-to-attack and targeting foundation for Tap Knockout.

Do not use Unity MCP. Do not directly edit .unity scene YAML.

Read:
- AGENTS.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/02_GDD_PRODUCTION.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/04_COMBAT_AND_ABILITIES.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/24_DATA_CONFIG_SCHEMA.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/25_PREFAB_AND_SCENE_CONTRACTS.md

Inspect:
- Assets/_Project/Scripts/Player
- Assets/_Project/Scripts/Combat
- Assets/_Project/Scripts/Projectile
- Assets/_Project/ScriptableObjects/Weapons

Scope:
- Implement target provider for nearest valid enemy/damageable.
- Implement PlayerAttackController using stop-to-attack threshold.
- Create WeaponConfig for one initial weapon.
- Create projectile/melee hit foundation if core contracts exist.
- Document projectile prefab contract/manual setup.

Out of scope:
- Dash
- Ability modifiers
- Full enemy AI
- Final VFX/audio
- Scene edits

Return:
- Changed files
- Tests/validation
- Manual Unity setup for weapon config, projectile prefab, and dummy target
```

## Prompt 6 - Create Dash-Impact System

```text
Implement the dash-impact foundation for Tap Knockout.

Do not use Unity MCP. Do not directly edit .unity scene YAML.

Read:
- AGENTS.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/02_GDD_PRODUCTION.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/04_COMBAT_AND_ABILITIES.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/21_VERTICAL_SLICE_SPEC.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/24_DATA_CONFIG_SCHEMA.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/30_ABILITY_AND_ENEMY_DESIGN_CATALOG.md

Inspect:
- Assets/_Project/Scripts/Player
- Assets/_Project/Scripts/Combat
- Assets/_Project/Scripts/Ability

Scope:
- Implement PlayerDashController.
- Add cooldown, duration, distance, direction resolver.
- Add i-frame hook without building final damage immunity system if not ready.
- Add dash impact hit detection.
- Add knockback and duplicate-hit prevention per dash.
- Emit dash start, dash hit, dash end events for future VFX/SFX/analytics/abilities.
- Keep values config-driven.

Out of scope:
- Full ability selection
- Final VFX/SFX
- Scene edits
- Real analytics SDK

Return:
- Changed files
- Tests/validation
- Manual Unity setup for dash hit volume or overlap query
- Known tuning risks
```

## Prompt 7 - Create Enemy Base System

```text
Implement the enemy base system for Tap Knockout.

Do not use Unity MCP. Do not directly edit .unity scene YAML.

Read:
- AGENTS.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/03_TECH_ARCHITECTURE_UNITY.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/04_COMBAT_AND_ABILITIES.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/05_LEVEL_ROOM_WAVE.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/24_DATA_CONFIG_SCHEMA.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/25_PREFAB_AND_SCENE_CONTRACTS.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/30_ABILITY_AND_ENEMY_DESIGN_CATALOG.md

Inspect:
- Assets/_Project/Scripts/Enemy
- Assets/_Project/Scripts/Combat
- Assets/_Project/ScriptableObjects/Enemies

Scope:
- Implement EnemyConfig.
- Implement EnemyController.
- Implement EnemyHealth using shared damage flow.
- Implement movement/attack hooks.
- Implement first melee chaser foundation.
- Document enemy prefab manual setup.

Out of scope:
- Full wave/room manager
- Boss
- Final animations/art
- Scene edits

Return:
- Changed files
- Tests/validation
- Manual Unity setup for enemy prefab and dummy combat test
```

## Prompt 8 - Create Wave and Room System

```text
Implement wave and room foundation for Tap Knockout.

Do not use Unity MCP. Do not directly edit .unity scene YAML.

Read:
- AGENTS.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/05_LEVEL_ROOM_WAVE.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/21_VERTICAL_SLICE_SPEC.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/24_DATA_CONFIG_SCHEMA.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/25_PREFAB_AND_SCENE_CONTRACTS.md

Inspect:
- Assets/_Project/Scripts/Level
- Assets/_Project/Scripts/Room
- Assets/_Project/Scripts/Wave
- Assets/_Project/Scripts/Enemy
- Assets/_Project/ScriptableObjects/Chapters
- Assets/_Project/ScriptableObjects/Rooms

Scope:
- Implement RoomTemplateConfig and WaveConfig if not present.
- Implement EnemySpawner.
- Implement WaveManager.
- Implement RoomManager clear condition for all enemies defeated.
- Add ChapterRunner skeleton for configured room sequence.
- Emit room start and room complete events.

Out of scope:
- Procedural generation
- Final scene art
- Boss implementation unless needed as a placeholder interface
- Scene YAML edits

Return:
- Changed files
- Tests/validation
- Manual Unity setup for room root and spawn points
```

## Prompt 9 - Create Ability Definition and Selection System

```text
Implement the roguelite ability definition and selection foundation for Tap Knockout.

Do not use Unity MCP. Do not directly edit .unity scene YAML.

Read:
- AGENTS.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/04_COMBAT_AND_ABILITIES.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/08_UI_UX_CONTROLS.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/21_VERTICAL_SLICE_SPEC.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/24_DATA_CONFIG_SCHEMA.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/30_ABILITY_AND_ENEMY_DESIGN_CATALOG.md

Inspect:
- Assets/_Project/Scripts/Ability
- Assets/_Project/Scripts/UI
- Assets/_Project/ScriptableObjects/Abilities

Scope:
- Implement AbilityDefinition.
- Implement run ability state.
- Implement weighted 3-choice provider.
- Avoid duplicates and respect max stacks.
- Add hooks for ability selected event.
- Create sample ability definitions or instructions for creating them manually.

Out of scope:
- Full UI art
- Monetized reroll
- Full set of ability effects
- Scene edits

Return:
- Changed files
- Tests/validation
- Manual Unity setup for ability assets and ability selection panel
```

## Prompt 10 - Create Editor Scene Builder for Placeholder Vertical Slice

```text
Create an Editor tool to build a placeholder vertical slice scene for Tap Knockout.

Do not use Unity MCP. Do not hand-edit .unity YAML. The Editor tool should be the safe path for generating a placeholder scene.

Read:
- AGENTS.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/03_TECH_ARCHITECTURE_UNITY.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/21_VERTICAL_SLICE_SPEC.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/25_PREFAB_AND_SCENE_CONTRACTS.md
- Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/28_CONTENT_PIPELINE_BALANCING_EDITOR_TOOLS.md

Inspect:
- Assets/_Project/Editor/Tools
- Assets/_Project/Scenes
- Assets/_Project/Prefabs
- Existing SampleScene only for awareness, not modification

Scope:
- Create an Editor script under Assets/_Project/Editor/Tools.
- Add menu item Tools/Tap Knockout/Create Vertical Slice Placeholder Scene.
- The tool should create or save a new placeholder scene only after clear user action in Unity.
- Include GameplayRoot, Managers, RoomRoot, simple arena, PlayerSpawn, EnemySpawnPoints, camera, light, and HUD placeholders.
- The tool should not overwrite existing production scenes silently.

Out of scope:
- Final art
- Final UI
- Gameplay balancing
- Real SDKs
- Direct scene YAML editing

Return:
- Changed files
- Manual Unity steps to run the menu item
- Validation steps for generated hierarchy
- Known limitations
```
