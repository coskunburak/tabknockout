# Production Sprint Plan

Recommended sprint duration: 1 week for foundation/combat sprints, 2 weeks for content, economy, monetization, QA, and soft-launch sprints. Keep branches small enough that each sprint can be reviewed and reverted independently.

Global rules for every sprint:

- Read `AGENTS.md`, `00_README_INDEX.md`, `12_CODEX_AGENT_GUIDE.md`, and sprint-specific docs before editing.
- Do not use Unity MCP unless the user explicitly confirms it works.
- Do not directly edit `.unity` YAML.
- Do not add real Ads, IAP, Analytics, crash, or remote config SDKs until the SDK readiness sprint.
- List changed files and validation steps at the end of each sprint.

## Milestone 0 - Production Foundation

### Sprint ID: M0-S1

Sprint Name: Repository Baseline and Production Rules

Estimated Duration: 1 week

Sprint Objective: Establish rollback, documentation, rules, and project setting visibility before implementation.

Business/Product Value: Prevents uncontrolled template churn and makes future Codex-driven work auditable.

Prerequisites: Current repository and docs audit.

Relevant Docs: `18_REPOSITORY_DISCOVERY_AND_DOCUMENTATION_AUDIT.md`, `19_MISSING_DOCUMENTATION_PLAN.md`, `29_RELEASE_BRANCHING_AND_GIT_WORKFLOW.md`.

Relevant Folders: project root, `Assets/Docs`, `ProjectSettings`, `Packages`.

Systems Affected: documentation, Git, project governance.

Out of Scope: gameplay code, scene edits, package additions, asset imports.

Detailed Tasks:

- Design: confirm production target and no-copy constraints.
- Engineering: initialize Git if user approves; add Unity `.gitignore`; verify package manifest.
- Unity Scene/Prefab: inspect only; document manual settings changes.
- UI/UX: none.
- Art/Audio/VFX: inventory staged assets only.
- Economy/Balance: none.
- Analytics: none.
- QA: confirm generated folders are ignored; record current risks.

Deliverables: Git baseline recommendation, root `AGENTS.md`, updated docs index, current project settings risk list.

Acceptance Criteria: `git status` works after Git init if approved; root rules exist; future tasks know docs location; no gameplay files changed.

Definition of Done: repository can be safely branched; current risks are documented; no generated files committed.

Manual Test Plan: open Unity manually and verify project loads without compile errors.

Performance Checks: none beyond Unity project load.

Risks: initializing Git incorrectly; accidentally committing `Library/`; leaving package id default.

Rollback Plan: revert only docs/Git setup changes; do not alter Unity assets.

Suggested Branch: `chore/production-foundation`

Suggested Commits: `docs: add production rules`; `chore: initialize unity git baseline`; `docs: record discovery audit`.

Codex Prompt for This Sprint:

```text
Read AGENTS.md, 00_README_INDEX.md, 12_CODEX_AGENT_GUIDE.md, 18_REPOSITORY_DISCOVERY_AND_DOCUMENTATION_AUDIT.md, and 29_RELEASE_BRANCHING_AND_GIT_WORKFLOW.md. Inspect the root, Assets, Packages, and ProjectSettings only. Do not use Unity MCP. Do not modify scenes or gameplay code. Prepare the Git baseline and documentation updates only. List changed files, manual Unity checks, and validation steps.
```

### Sprint ID: M0-S2

Sprint Name: Production Folder Structure and Project Setting Checklist

Estimated Duration: 1 week

Sprint Objective: Create the intended production structure without adding behavior.

Business/Product Value: Gives all future systems stable paths and reduces file sprawl.

Prerequisites: M0-S1.

Relevant Docs: `03_TECH_ARCHITECTURE_UNITY.md`, `25_PREFAB_AND_SCENE_CONTRACTS.md`, `28_CONTENT_PIPELINE_BALANCING_EDITOR_TOOLS.md`.

Relevant Folders: `Assets/_Project`, `Assets/ThirdParty`, `Assets/Docs`.

Systems Affected: folder structure, docs, future asset organization.

Out of Scope: moving imported assets without license review, gameplay code, scene YAML.

Detailed Tasks:

- Design: confirm production taxonomy and naming.
- Engineering: create empty folders and README files; optionally add asmdef plan but not required yet.
- Unity Scene/Prefab: document manual creation of Boot, Home, Gameplay scenes for later.
- UI/UX: reserve folders for UI prefabs and art.
- Art/Audio/VFX: create third-party intake folders but do not import or migrate assets.
- Economy/Balance: create config folders only.
- Analytics: create service/config folder placeholders only.
- QA: verify no generated folders changed unexpectedly.

Deliverables: `Assets/_Project` and `Assets/ThirdParty` structure, folder readmes, project settings checklist.

Acceptance Criteria: structure matches architecture doc; imported assets remain untouched unless approved; Unity can reimport cleanly.

Definition of Done: folder structure exists; no scene or gameplay behavior added; manual Unity settings list is ready.

Manual Test Plan: open Unity, let folders import, confirm no compile errors.

Performance Checks: none.

Risks: accidental asset migration; `.meta` churn; hidden package changes.

Rollback Plan: remove only empty folders/readmes if needed.

Suggested Branch: `chore/production-folder-structure`

Suggested Commits: `chore: create production unity folders`; `docs: add project settings checklist`.

Codex Prompt for This Sprint:

```text
Read AGENTS.md, 03_TECH_ARCHITECTURE_UNITY.md, 12_CODEX_AGENT_GUIDE.md, 25_PREFAB_AND_SCENE_CONTRACTS.md, and 28_CONTENT_PIPELINE_BALANCING_EDITOR_TOOLS.md. Create only production folders and README files under Assets/_Project and Assets/ThirdParty. Do not move assets, import packages, implement gameplay, or modify scenes. Explain manual Unity project setting changes for portrait Android. List changed files and validation steps.
```

## Milestone 1 - Core Combat Foundation

### Sprint ID: M1-S1

Sprint Name: Core Runtime Contracts

Estimated Duration: 1 week

Sprint Objective: Add combat, config, event, and service interfaces before gameplay controllers.

Business/Product Value: Reduces rewrite risk and enables data-driven combat, analytics, and monetization stubs.

Prerequisites: M0-S2.

Relevant Docs: `03_TECH_ARCHITECTURE_UNITY.md`, `04_COMBAT_AND_ABILITIES.md`, `24_DATA_CONFIG_SCHEMA.md`, `26_MONETIZATION_ANALYTICS_REMOTE_CONFIG_SPEC.md`.

Relevant Folders: `Assets/_Project/Scripts/Core`, `Combat`, `Config`, `Analytics`, `Ads`, `IAP`, `Save`.

Systems Affected: combat data models, service contracts, configuration.

Out of Scope: player movement, enemies, UI, scene setup.

Detailed Tasks:

- Design: confirm damage types, hit context, config id rules.
- Engineering: create `IDamageable`, `HitContext`, `DamageType`, knockback data, event channels, service interfaces.
- Unity Scene/Prefab: none.
- UI/UX: none.
- Art/Audio/VFX: define event hooks only.
- Economy/Balance: define config id and validation conventions.
- Analytics: define `IAnalyticsService` only.
- QA: add EditMode tests for pure data validation where feasible.

Deliverables: compile-safe contracts and minimal tests.

Acceptance Criteria: project compiles; no gameplay scene dependency; contracts match docs.

Definition of Done: unit tests pass or are documented as not runnable; no singleton service locator hardcoding.

Manual Test Plan: open Unity and confirm no compile errors.

Performance Checks: none.

Risks: over-abstracting too early; creating giant managers.

Rollback Plan: revert contracts and tests as one commit.

Suggested Branch: `feat/core-runtime-contracts`

Suggested Commits: `feat: add combat contracts`; `feat: add service interfaces`; `test: add config validation tests`.

Codex Prompt for This Sprint:

```text
Read AGENTS.md, 03_TECH_ARCHITECTURE_UNITY.md, 04_COMBAT_AND_ABILITIES.md, 24_DATA_CONFIG_SCHEMA.md, and 26_MONETIZATION_ANALYTICS_REMOTE_CONFIG_SPEC.md. Inspect Assets/_Project/Scripts if it exists. Implement only core interfaces/data models for combat, configs, events, analytics, ads, IAP, save, audio, and remote config. Do not create player/enemy behavior and do not modify scenes. Provide changed files, tests, and manual Unity validation.
```

### Sprint ID: M1-S2

Sprint Name: Player Movement and Targeting Foundation

Estimated Duration: 1 week

Sprint Objective: Create responsive portrait movement and deterministic target selection.

Business/Product Value: Establishes the core feel that every combat system depends on.

Prerequisites: M1-S1.

Relevant Docs: `02_GDD_PRODUCTION.md`, `03_TECH_ARCHITECTURE_UNITY.md`, `08_UI_UX_CONTROLS.md`, `25_PREFAB_AND_SCENE_CONTRACTS.md`.

Relevant Folders: `Assets/_Project/Scripts/Input`, `Player`, `Combat`, `ScriptableObjects/Player`, `Prefabs/Player`.

Systems Affected: input abstraction, player movement, target scanning.

Out of Scope: attacking, dash, enemies beyond target interface/test doubles, scene YAML.

Detailed Tasks:

- Design: define movement threshold for stop-to-attack and last-facing direction.
- Engineering: implement movement input reader, movement controller, target provider, player config.
- Unity Scene/Prefab: document player prefab components and manual setup.
- UI/UX: verify control placement assumptions for portrait.
- Art/Audio/VFX: placeholder capsule/model only if manually assigned later.
- Economy/Balance: movement config values only.
- Analytics: no events yet.
- QA: test movement math and target selection with mock targets.

Deliverables: movement scripts, config, prefab contract, validation tests.

Acceptance Criteria: movement compiles; targeting is deterministic; no scene modified by code.

Definition of Done: manual setup instructions can create a moving player in a test scene.

Manual Test Plan: in Unity, create a temporary scene or use future builder to attach controller and verify drag movement.

Performance Checks: target scan has configurable radius/frequency and does not allocate each frame where avoidable.

Risks: input backend mismatch; mobile feel not testable in Editor.

Rollback Plan: revert movement/targeting files and docs update.

Suggested Branch: `feat/player-movement-targeting`

Suggested Commits: `feat: add player movement foundation`; `feat: add target selection foundation`; `docs: document player prefab contract`.

Codex Prompt for This Sprint:

```text
Read AGENTS.md, 02_GDD_PRODUCTION.md, 08_UI_UX_CONTROLS.md, 24_DATA_CONFIG_SCHEMA.md, and 25_PREFAB_AND_SCENE_CONTRACTS.md. Inspect Assets/_Project/Scripts/Input, Player, and Combat. Implement player movement and target selection foundation only. Do not implement attacks, dash, enemy AI, or scene YAML changes. Explain manual Unity setup for a player prefab and input testing. List changed files and validation steps.
```

### Sprint ID: M1-S3

Sprint Name: Stop-to-Attack and Projectile Foundation

Estimated Duration: 1 week

Sprint Objective: Add first weapon attack flow using shared damage contracts.

Business/Product Value: Creates the familiar Archero-style readability while staying original in content.

Prerequisites: M1-S1, M1-S2.

Relevant Docs: `02_GDD_PRODUCTION.md`, `04_COMBAT_AND_ABILITIES.md`, `24_DATA_CONFIG_SCHEMA.md`, `25_PREFAB_AND_SCENE_CONTRACTS.md`.

Relevant Folders: `Assets/_Project/Scripts/Player`, `Combat`, `Projectile`, `ScriptableObjects/Weapons`.

Systems Affected: player attack, weapon config, projectile pooling.

Out of Scope: dash, ability modifiers, final VFX/audio.

Detailed Tasks:

- Design: define stationary threshold, attack cadence, range, target priority.
- Engineering: create weapon config, attack controller, projectile controller, hit dispatch, pool interface.
- Unity Scene/Prefab: document projectile prefab contract.
- UI/UX: expose cooldown/state only through debug hooks for now.
- Art/Audio/VFX: event hooks for fire/hit.
- Economy/Balance: initial weapon values in config.
- Analytics: no runtime analytics yet.
- QA: test cooldown, target validity, hit context creation.

Deliverables: one config-driven weapon path and projectile contract.

Acceptance Criteria: a player can fire at a valid target in manual setup; damage uses `HitContext`.

Definition of Done: no runtime `Instantiate/Destroy` loop in combat path unless explicitly temporary and documented.

Manual Test Plan: attach to test prefabs in Unity and verify projectile hits a dummy damageable.

Performance Checks: projectile pool avoids per-shot allocations where feasible.

Risks: premature complex weapon system; pooling complexity before need.

Rollback Plan: revert attack/projectile files.

Suggested Branch: `feat/stop-to-attack`

Suggested Commits: `feat: add weapon config`; `feat: add stop-to-attack controller`; `feat: add projectile foundation`.

Codex Prompt for This Sprint:

```text
Read AGENTS.md, 02_GDD_PRODUCTION.md, 04_COMBAT_AND_ABILITIES.md, 24_DATA_CONFIG_SCHEMA.md, and 25_PREFAB_AND_SCENE_CONTRACTS.md. Implement stop-to-attack and one projectile/weapon foundation. Do not implement dash, abilities, enemy AI, or scene edits. Include manual Unity setup for weapon config, projectile prefab, and dummy target validation. List changed files and tests.
```

### Sprint ID: M1-S4

Sprint Name: Dash-Impact Foundation

Estimated Duration: 1 week

Sprint Objective: Implement dash as movement plus combat impact.

Business/Product Value: Proves the product's differentiating mechanic early.

Prerequisites: M1-S2, M1-S3.

Relevant Docs: `02_GDD_PRODUCTION.md`, `04_COMBAT_AND_ABILITIES.md`, `21_VERTICAL_SLICE_SPEC.md`, `30_ABILITY_AND_ENEMY_DESIGN_CATALOG.md`.

Relevant Folders: `Assets/_Project/Scripts/Player`, `Combat`, `Ability`, `ScriptableObjects/Player`.

Systems Affected: player dash, impact damage, knockback, invulnerability hook, events.

Out of Scope: full ability system, final VFX, scene YAML.

Detailed Tasks:

- Design: finalize cooldown, duration, distance, i-frame hook, knockback force.
- Engineering: implement dash controller, cooldown, direction resolver, impact hit collector, duplicate-hit prevention.
- Unity Scene/Prefab: document dash hit volume setup.
- UI/UX: expose cooldown state for future HUD button.
- Art/Audio/VFX: fire dash start, hit, end events.
- Economy/Balance: config-driven values only.
- Analytics: prepare `dash_used` and `dash_hit` event hooks but console service can be later.
- QA: test dash through targets, cooldown, hit once per enemy per dash, collision edge cases.

Deliverables: dash movement and dash-impact combat path.

Acceptance Criteria: dash can damage and knock back enemies or dummy targets; dash cannot be spammed; events fire.

Definition of Done: dash values live in config, not scattered constants.

Manual Test Plan: use Unity test scene/manual builder to validate dash direction, cooldown, hit, knockback, and player control recovery.

Performance Checks: dash overlap checks are bounded and non-allocating where feasible.

Risks: dash feels too floaty; invulnerability creates balance issues; collision tunneling.

Rollback Plan: disable dash component and revert sprint commits if core movement regresses.

Suggested Branch: `feat/dash-impact-foundation`

Suggested Commits: `feat: add dash config`; `feat: implement dash movement`; `feat: add dash impact hit flow`.

Codex Prompt for This Sprint:

```text
Read AGENTS.md, 02_GDD_PRODUCTION.md, 04_COMBAT_AND_ABILITIES.md, 21_VERTICAL_SLICE_SPEC.md, and 30_ABILITY_AND_ENEMY_DESIGN_CATALOG.md. Implement dash-impact foundation: cooldown, direction, i-frame hook, impact hit detection, knockback, and events. Do not implement full ability selection, final VFX, or scene YAML edits. Explain manual Unity setup for dash hit volume and validation. List changed files, tests, and risks.
```

## Milestone 2 - Room/Wave/Chapter Loop

### Sprint ID: M2-S1

Sprint Name: Enemy Base and First Behaviors

Estimated Duration: 1 week

Sprint Objective: Add reusable enemy foundation and three vertical slice archetypes.

Business/Product Value: Provides the pressure needed to evaluate movement, attack, and dash fun.

Prerequisites: M1-S1 through M1-S4.

Relevant Docs: `05_LEVEL_ROOM_WAVE.md`, `24_DATA_CONFIG_SCHEMA.md`, `25_PREFAB_AND_SCENE_CONTRACTS.md`, `30_ABILITY_AND_ENEMY_DESIGN_CATALOG.md`.

Relevant Folders: `Assets/_Project/Scripts/Enemy`, `Combat`, `Projectile`, `ScriptableObjects/Enemies`.

Systems Affected: enemies, health, AI hooks, enemy configs.

Out of Scope: room sequencing, boss, final animations.

Detailed Tasks:

- Design: define melee, ranged, charger roles and telegraphs.
- Engineering: enemy controller, health, movement, attack hooks, melee/ranged/charge behaviors.
- Unity Scene/Prefab: document enemy prefab contracts.
- UI/UX: none.
- Art/Audio/VFX: animation event hooks only.
- Economy/Balance: enemy HP/damage/speed configs.
- Analytics: enemy kill hook for future.
- QA: test damage, death, knockback, charge interrupt.

Deliverables: three enemy archetype foundations and configs.

Acceptance Criteria: each enemy can be spawned manually and can damage or threaten player according to role.

Definition of Done: no enemy-specific hardcoded assets; configs drive tuning.

Manual Test Plan: manually place enemy prefabs/dummies in test scene and verify behavior.

Performance Checks: enemy update logic supports at least 25 enemies at target frame rate in later tests.

Risks: charger readability; ranged projectile clutter.

Rollback Plan: revert enemy behavior commits independently from player combat.

Suggested Branch: `feat/enemy-base-archetypes`

Suggested Commits: `feat: add enemy base`; `feat: add melee chaser`; `feat: add ranged shooter`; `feat: add charger`.

Codex Prompt for This Sprint:

```text
Read AGENTS.md, 05_LEVEL_ROOM_WAVE.md, 24_DATA_CONFIG_SCHEMA.md, 25_PREFAB_AND_SCENE_CONTRACTS.md, and 30_ABILITY_AND_ENEMY_DESIGN_CATALOG.md. Implement enemy base plus melee chaser, ranged shooter, and charger foundations. Do not implement room sequencing, boss, or scene YAML edits. Include manual prefab setup and validation steps.
```

### Sprint ID: M2-S2

Sprint Name: Room, Wave, and Chapter Runner

Estimated Duration: 1-2 weeks

Sprint Objective: Sequence enemies into rooms and rooms into a chapter.

Business/Product Value: Turns combat pieces into the core run loop.

Prerequisites: M2-S1.

Relevant Docs: `05_LEVEL_ROOM_WAVE.md`, `21_VERTICAL_SLICE_SPEC.md`, `24_DATA_CONFIG_SCHEMA.md`, `25_PREFAB_AND_SCENE_CONTRACTS.md`.

Relevant Folders: `Assets/_Project/Scripts/Level`, `Room`, `Wave`, `Enemy`, `ScriptableObjects/Chapters`, `Rooms`.

Systems Affected: spawner, wave manager, room manager, chapter runner.

Out of Scope: procedural generation, final room art, monetization.

Detailed Tasks:

- Design: lock Chapter 1 room sequence and clear conditions.
- Engineering: spawn patterns, wave sequencing, room state machine, chapter runner, reward/ability transition hooks.
- Unity Scene/Prefab: document room root, spawn points, exits, camera bounds.
- UI/UX: debug room state display optional.
- Art/Audio/VFX: room start/clear event hooks.
- Economy/Balance: reward hook only.
- Analytics: room start/complete hook names.
- QA: test all-enemies-defeated, survive-duration placeholder, boss-room hook.

Deliverables: config-driven 12-15 room chapter runner.

Acceptance Criteria: a manual scene can run multiple rooms and report chapter completion/failure.

Definition of Done: room data is not embedded in scene objects only; configs can define sequence.

Manual Test Plan: run through a short 3-room test and a full 15-room config in Unity.

Performance Checks: spawning does not allocate heavily during waves; max alive enforced.

Risks: state machine bugs; dangling enemies blocking room clear.

Rollback Plan: keep chapter runner separate from combat controllers for targeted revert.

Suggested Branch: `feat/room-wave-chapter-loop`

Suggested Commits: `feat: add wave manager`; `feat: add room manager`; `feat: add chapter runner`; `test: cover room clear logic`.

Codex Prompt for This Sprint:

```text
Read AGENTS.md, 05_LEVEL_ROOM_WAVE.md, 21_VERTICAL_SLICE_SPEC.md, 24_DATA_CONFIG_SCHEMA.md, and 25_PREFAB_AND_SCENE_CONTRACTS.md. Implement room, wave, enemy spawner, and chapter runner foundations. Do not implement procedural generation or direct scene YAML edits. Explain manual Unity setup for room roots and spawn points. List changed files, tests, and validation.
```

## Milestone 3 - Ability System Vertical Slice

### Sprint ID: M3-S1

Sprint Name: Ability Definitions and Selection Logic

Estimated Duration: 1 week

Sprint Objective: Add data-driven roguelite ability selection.

Business/Product Value: Introduces replayability and run-to-run variation.

Prerequisites: M1-S1, M2-S2.

Relevant Docs: `04_COMBAT_AND_ABILITIES.md`, `21_VERTICAL_SLICE_SPEC.md`, `24_DATA_CONFIG_SCHEMA.md`, `30_ABILITY_AND_ENEMY_DESIGN_CATALOG.md`.

Relevant Folders: `Assets/_Project/Scripts/Ability`, `ScriptableObjects/Abilities`.

Systems Affected: ability definitions, run ability state, weighted offers.

Out of Scope: final UI polish, monetized rerolls, every ability implementation.

Detailed Tasks:

- Design: define tags, rarity, max stacks, mutually exclusive rules.
- Engineering: AbilityDefinition config, ability state, weighted choice provider, run reset.
- Unity Scene/Prefab: none.
- UI/UX: provide data for 3-card panel.
- Art/Audio/VFX: icon references only.
- Economy/Balance: rarity weights and stack caps.
- Analytics: offer/selection hook names.
- QA: deterministic weighted selection tests, duplicate/max-stack tests.

Deliverables: ability config and selection logic.

Acceptance Criteria: system can offer 3 valid abilities and record selected ability stacks.

Definition of Done: invalid configs are detected by tests or validator.

Manual Test Plan: create sample ability assets and verify console/debug offer flow.

Performance Checks: selection is infrequent and alloc-safe enough for pause screen use.

Risks: ability data too generic to implement cleanly; duplicate rules unclear.

Rollback Plan: revert ability system files without affecting combat.

Suggested Branch: `feat/ability-selection-core`

Suggested Commits: `feat: add ability definitions`; `feat: add weighted choice provider`; `test: cover ability selection rules`.

Codex Prompt for This Sprint:

```text
Read AGENTS.md, 04_COMBAT_AND_ABILITIES.md, 21_VERTICAL_SLICE_SPEC.md, 24_DATA_CONFIG_SCHEMA.md, and 30_ABILITY_AND_ENEMY_DESIGN_CATALOG.md. Implement AbilityDefinition, run ability state, weighted 3-choice provider, rarity, max-stack, and duplicate rules. Do not implement full UI, monetized rerolls, or scene edits. Include sample data setup instructions and tests.
```

### Sprint ID: M3-S2

Sprint Name: Ability Effects and Choice UI Hooks

Estimated Duration: 1-2 weeks

Sprint Objective: Implement initial ability effects and connect to selection UI hooks.

Business/Product Value: Makes the run progression visible and reinforces dash-impact identity.

Prerequisites: M3-S1, M1-S4.

Relevant Docs: `04_COMBAT_AND_ABILITIES.md`, `08_UI_UX_CONTROLS.md`, `21_VERTICAL_SLICE_SPEC.md`, `30_ABILITY_AND_ENEMY_DESIGN_CATALOG.md`.

Relevant Folders: `Assets/_Project/Scripts/Ability`, `UI`, `Player`, `Combat`.

Systems Affected: stat modifiers, projectile modifiers, dash modifiers, ability panel hooks.

Out of Scope: final icons, final UI art, ad reroll.

Detailed Tasks:

- Design: choose first 10 implemented abilities.
- Engineering: implement modifier pipeline for stats, projectiles, dash impact, heal-after-room.
- Unity Scene/Prefab: document ability panel prefab contract.
- UI/UX: hook 3-card panel to provider and selected effects.
- Art/Audio/VFX: placeholder icons and selection SFX hook.
- Economy/Balance: initial ability weights and stack caps.
- Analytics: console hooks for offered/selected.
- QA: test each ability effect and stacking rule.

Deliverables: 10 implemented abilities, ability panel hooks, sample configs.

Acceptance Criteria: player can select abilities during a run and at least one dash ability changes gameplay.

Definition of Done: selection pause/resume works; selected effects persist for current run and reset afterward.

Manual Test Plan: play 5 room sequence and verify repeated ability offers/effects.

Performance Checks: modifiers do not add per-frame reflection/string lookups.

Risks: modifier architecture becomes too broad; UI pause bugs.

Rollback Plan: disable new ability configs or revert effect pipeline.

Suggested Branch: `feat/ability-effects-ui-hooks`

Suggested Commits: `feat: add ability modifier pipeline`; `feat: implement first ability effects`; `feat: hook ability choice panel`.

Codex Prompt for This Sprint:

```text
Read AGENTS.md, 04_COMBAT_AND_ABILITIES.md, 08_UI_UX_CONTROLS.md, 21_VERTICAL_SLICE_SPEC.md, and 30_ABILITY_AND_ENEMY_DESIGN_CATALOG.md. Implement the first 10 ability effects and UI hooks for a 3-card selection panel. Do not add final art, monetized rerolls, SDKs, or scene YAML edits. Explain manual prefab setup and validation.
```

## Milestone 4 - Enemy/Boss/Content Foundation

### Sprint ID: M4-S1

Sprint Name: Boss Placeholder and Content Pass

Estimated Duration: 1-2 weeks

Sprint Objective: Add Stone Brute boss and tune the first 15-room chapter.

Business/Product Value: Creates a complete run climax and content structure for retention testing.

Prerequisites: M2-S2, M3-S2.

Relevant Docs: `05_LEVEL_ROOM_WAVE.md`, `21_VERTICAL_SLICE_SPEC.md`, `27_QA_PERFORMANCE_SOFT_LAUNCH_PLAN.md`, `30_ABILITY_AND_ENEMY_DESIGN_CATALOG.md`.

Relevant Folders: `Assets/_Project/Scripts/Enemy`, `Level`, `ScriptableObjects/Enemies`, `Chapters`.

Systems Affected: boss AI, chapter content configs, tuning.

Out of Scope: final boss art, cinematic presentation, store polish.

Detailed Tasks:

- Design: define boss attack rotation, telegraphs, fail states.
- Engineering: boss controller hooks, slam, charge, add summon, boss health bar events.
- Unity Scene/Prefab: document boss prefab and arena setup.
- UI/UX: boss HP hook.
- Art/Audio/VFX: placeholder telegraph and hit events.
- Economy/Balance: HP/damage/room difficulty ramp.
- Analytics: boss start/defeat/death hooks.
- QA: run boss repeatedly with different ability builds.

Deliverables: boss placeholder and tuned Chapter 1 config.

Acceptance Criteria: boss can be defeated; attacks are readable; player death/run end works.

Definition of Done: boss uses same damage/health conventions as enemies.

Manual Test Plan: direct boss-room start and full chapter boss entry.

Performance Checks: boss adds respect max alive and pooling rules.

Risks: boss charge collision bugs; difficulty spike.

Rollback Plan: disable boss room in config or revert boss controller.

Suggested Branch: `feat/boss-content-foundation`

Suggested Commits: `feat: add boss placeholder`; `feat: tune chapter one content`; `test: add boss smoke checks`.

Codex Prompt for This Sprint:

```text
Read AGENTS.md, 05_LEVEL_ROOM_WAVE.md, 21_VERTICAL_SLICE_SPEC.md, 27_QA_PERFORMANCE_SOFT_LAUNCH_PLAN.md, and 30_ABILITY_AND_ENEMY_DESIGN_CATALOG.md. Implement Stone Brute boss placeholder, boss events, and Chapter 1 content tuning. Do not use final art or edit scene YAML directly. Document manual boss prefab/arena setup and validation.
```

## Milestone 5 - Meta Progression/Economy

### Sprint ID: M5-S1

Sprint Name: Save, Currency, Gear, and Talent Stubs

Estimated Duration: 1-2 weeks

Sprint Objective: Add the first persistent meta loop without monetization pressure.

Business/Product Value: Supports retention and gives runs a reason to repeat.

Prerequisites: M2-S2.

Relevant Docs: `06_META_ECONOMY.md`, `20_BACKLOG_MASTER.md`, `24_DATA_CONFIG_SCHEMA.md`, `28_CONTENT_PIPELINE_BALANCING_EDITOR_TOOLS.md`.

Relevant Folders: `Assets/_Project/Scripts/Save`, `Economy`, `Meta`, `ScriptableObjects/Economy`.

Systems Affected: save, currencies, rewards, gear, talents.

Out of Scope: real economy monetization, cloud save, battle pass.

Detailed Tasks:

- Design: define first upgrade costs and reward ranges.
- Engineering: local save, currency wallet, reward grant, gear/talent data stubs.
- Unity Scene/Prefab: none.
- UI/UX: placeholder screens or data hooks only.
- Art/Audio/VFX: none.
- Economy/Balance: create first economy tables and config defaults.
- Analytics: hooks for reward grant, gear upgrade, talent upgrade.
- QA: save/load, reward persistence, offline path.

Deliverables: persistent local meta stubs and reward flow.

Acceptance Criteria: run rewards persist and can be spent on placeholder upgrade paths.

Definition of Done: save schema has version and migration placeholder.

Manual Test Plan: complete run, restart app/editor, confirm reward persistence.

Performance Checks: save operations are not performed every frame.

Risks: save corruption; currency inflation; premature monetization coupling.

Rollback Plan: reset local save and revert save/economy commits.

Suggested Branch: `feat/meta-economy-stubs`

Suggested Commits: `feat: add save service`; `feat: add currency wallet`; `feat: add gear talent stubs`; `test: cover reward persistence`.

Codex Prompt for This Sprint:

```text
Read AGENTS.md, 06_META_ECONOMY.md, 24_DATA_CONFIG_SCHEMA.md, and 28_CONTENT_PIPELINE_BALANCING_EDITOR_TOOLS.md. Implement local save, currency wallet, reward grant, and placeholder gear/talent upgrade data. Do not add cloud save, real IAP, ads, or final UI. Explain manual validation and save reset steps.
```

## Milestone 6 - UI/UX Production Pass

### Sprint ID: M6-S1

Sprint Name: Gameplay HUD, Run Result, and Home Shell

Estimated Duration: 1-2 weeks

Sprint Objective: Make the run understandable and connect gameplay to meta screens.

Business/Product Value: Turns systems into a usable mobile product flow.

Prerequisites: M3-S2, M5-S1.

Relevant Docs: `08_UI_UX_CONTROLS.md`, `21_VERTICAL_SLICE_SPEC.md`, `25_PREFAB_AND_SCENE_CONTRACTS.md`, `27_QA_PERFORMANCE_SOFT_LAUNCH_PLAN.md`.

Relevant Folders: `Assets/_Project/Scripts/UI`, `Prefabs/UI`, `Art/UI`.

Systems Affected: HUD, ability UI, run result, home shell, safe area.

Out of Scope: final store/shop, real ad/IAP screens, localization.

Detailed Tasks:

- Design: define first screen flow and FTUE order.
- Engineering: UI controllers, safe-area utility, run result data binding.
- Unity Scene/Prefab: create/manual setup prefabs through Unity or Editor tool, not YAML.
- UI/UX: HP, dash cooldown, ability cards, run result, home shell.
- Art/Audio/VFX: placeholder UI assets and feedback SFX hooks.
- Economy/Balance: display currency/rewards.
- Analytics: screen/event hooks.
- QA: safe area and portrait validation.

Deliverables: playable UI flow from home to run to result.

Acceptance Criteria: tester can start a chapter, play, choose abilities, see result, return home.

Definition of Done: UI scales to 1080x1920 reference and common aspect ratios.

Manual Test Plan: Editor and Android touch test for all buttons and panels.

Performance Checks: UI does not allocate heavily during combat HUD updates.

Risks: UI prefab setup requires manual Unity attention; safe area differences.

Rollback Plan: disable UI prefabs and use debug flow if needed.

Suggested Branch: `feat/gameplay-ui-flow`

Suggested Commits: `feat: add gameplay hud`; `feat: add run result panel`; `feat: add home shell`.

Codex Prompt for This Sprint:

```text
Read AGENTS.md, 08_UI_UX_CONTROLS.md, 21_VERTICAL_SLICE_SPEC.md, 25_PREFAB_AND_SCENE_CONTRACTS.md, and 27_QA_PERFORMANCE_SOFT_LAUNCH_PLAN.md. Implement UI controllers and prefab contracts for HUD, ability panel, run result, and home shell. Do not add real shop/ad/IAP SDK flows or direct scene YAML edits. Explain manual Unity prefab setup and validation.
```

## Milestone 7 - Monetization and Analytics Stubs

### Sprint ID: M7-S1

Sprint Name: Analytics, Remote Config, Ads, and IAP Stubs

Estimated Duration: 1-2 weeks

Sprint Objective: Add safe commercial architecture without external SDKs.

Business/Product Value: Enables measurement and monetization design while avoiding compliance risk.

Prerequisites: M1-S1, M5-S1, M6-S1.

Relevant Docs: `07_MONETIZATION_LIVEOPS.md`, `10_ANALYTICS_REMOTE_CONFIG.md`, `15_STORE_COMPLIANCE.md`, `26_MONETIZATION_ANALYTICS_REMOTE_CONFIG_SPEC.md`.

Relevant Folders: `Assets/_Project/Scripts/Analytics`, `Ads`, `IAP`, `Config`, `Economy`.

Systems Affected: console analytics, local remote config, fake ads, fake IAP.

Out of Scope: real SDKs, consent management, platform purchases.

Detailed Tasks:

- Design: confirm event taxonomy and monetization placements.
- Engineering: console analytics, local remote config provider, fake rewarded ad result paths, fake IAP catalog/result paths.
- Unity Scene/Prefab: none or optional debug panel contract.
- UI/UX: placeholder revive/double reward/free chest hooks only.
- Art/Audio/VFX: none.
- Economy/Balance: reward grant integration for fake rewarded ad success.
- Analytics: events for offers, completions, purchases, remote config variants.
- QA: success/cancel/fail paths.

Deliverables: SDK-free commercial service layer.

Acceptance Criteria: gameplay/meta code talks to interfaces only; fake rewarded rewards can be granted and denied correctly.

Definition of Done: no external SDK package or network dependency added.

Manual Test Plan: trigger fake ad success, cancel, fail; trigger fake purchase success/fail; verify analytics console output.

Performance Checks: analytics logging does not spam per-frame events.

Risks: accidental monetization logic coupled to UI; privacy assumptions too early.

Rollback Plan: swap service bindings back to no-op implementations.

Suggested Branch: `feat/commercial-stubs`

Suggested Commits: `feat: add analytics service stub`; `feat: add local remote config`; `feat: add fake ads iap services`.

Codex Prompt for This Sprint:

```text
Read AGENTS.md, 07_MONETIZATION_LIVEOPS.md, 10_ANALYTICS_REMOTE_CONFIG.md, 15_STORE_COMPLIANCE.md, and 26_MONETIZATION_ANALYTICS_REMOTE_CONFIG_SPEC.md. Implement console analytics, local remote config, fake rewarded ads, and fake IAP services only. Do not add real SDKs or packages. Include success/cancel/fail validation steps and changed files.
```

## Milestone 8 - Asset Integration and Art Direction

### Sprint ID: M8-S1

Sprint Name: Licensed Placeholder Asset Integration

Estimated Duration: 1-2 weeks

Sprint Objective: Integrate approved placeholder art/audio/VFX with license discipline.

Business/Product Value: Makes the slice readable and presentable without legal risk.

Prerequisites: M0-S2, M2-S2, M6-S1.

Relevant Docs: `09_ASSET_PIPELINE.md`, `17_CREDITS_TEMPLATE.md`, `25_PREFAB_AND_SCENE_CONTRACTS.md`, `28_CONTENT_PIPELINE_BALANCING_EDITOR_TOOLS.md`.

Relevant Folders: `Assets/ThirdParty`, `Assets/_Project/Art`, `Audio`, `Prefabs`, staged asset packs.

Systems Affected: prefabs, materials, animations, VFX, SFX.

Out of Scope: unknown-license assets, final brand art, store screenshots.

Detailed Tasks:

- Design: choose coherent stylized direction and readable silhouettes.
- Engineering: no gameplay behavior unless prefab bindings require safe references.
- Unity Scene/Prefab: manually assign approved models/materials/animations to prefabs.
- UI/UX: use approved placeholder UI assets.
- Art/Audio/VFX: dash trail, impact hit, projectile, enemy death, room clear placeholders.
- Economy/Balance: none.
- Analytics: none.
- QA: license checklist and visual readability review.

Deliverables: approved art pass for player, enemies, boss, room, projectiles, dash VFX, UI placeholders.

Acceptance Criteria: every used asset has source/license entry; unknown-license pack is excluded until resolved.

Definition of Done: content is under correct folders and credited; gameplay remains functional.

Manual Test Plan: inspect each prefab in Unity; play full slice and verify visual clarity.

Performance Checks: check material count, texture size, animator overhead, VFX count.

Risks: license uncertainty; asset scale/rig mismatch; performance regressions.

Rollback Plan: revert prefab assignments and asset migration commit.

Suggested Branch: `art/licensed-placeholder-pass`

Suggested Commits: `docs: record asset licenses`; `art: migrate approved placeholders`; `art: hook placeholder vfx audio`.

Codex Prompt for This Sprint:

```text
Read AGENTS.md, 09_ASSET_PIPELINE.md, 17_CREDITS_TEMPLATE.md, 25_PREFAB_AND_SCENE_CONTRACTS.md, and 28_CONTENT_PIPELINE_BALANCING_EDITOR_TOOLS.md. Inspect staged asset licenses before use. Integrate only approved placeholder assets and update credits. Do not use unknown-license assets, import random assets, or directly edit scene YAML. Explain manual Unity prefab assignment and validation.
```

## Milestone 9 - QA, Performance, and Android Build

### Sprint ID: M9-S1

Sprint Name: Vertical Slice QA and Android Build Gate

Estimated Duration: 1-2 weeks

Sprint Objective: Validate the slice on Android and create a repeatable build gate.

Business/Product Value: Confirms production viability on the target platform.

Prerequisites: M1-M8.

Relevant Docs: `11_QA_PERFORMANCE_RELEASE.md`, `21_VERTICAL_SLICE_SPEC.md`, `27_QA_PERFORMANCE_SOFT_LAUNCH_PLAN.md`, `29_RELEASE_BRANCHING_AND_GIT_WORKFLOW.md`.

Relevant Folders: `Assets/_Project/Tests`, `ProjectSettings`, build output path.

Systems Affected: tests, build settings, performance budgets, QA logs.

Out of Scope: public release, real SDKs, store listing.

Detailed Tasks:

- Design: confirm pass/fail criteria.
- Engineering: add data validation tests and smoke tests where feasible.
- Unity Scene/Prefab: verify build scenes through Unity, not YAML.
- UI/UX: safe-area and touch-device pass.
- Art/Audio/VFX: performance/readability pass.
- Economy/Balance: reward/save test pass.
- Analytics: console event audit.
- QA: Android device build, 5-minute stability test, issue log.

Deliverables: Android debug build, QA report, performance report, release blocker list.

Acceptance Criteria: build runs on Android; 5-minute session stable; no critical errors; performance targets are measured.

Definition of Done: all P0 issues fixed or explicitly accepted; build instructions documented.

Manual Test Plan: execute checklist in `27_QA_PERFORMANCE_SOFT_LAUNCH_PLAN.md`.

Performance Checks: 60 FPS target, 30 FPS minimum, GC spikes, memory, enemies/projectiles/VFX budgets.

Risks: device-specific input issues; build setting gaps; URP mobile performance issues.

Rollback Plan: revert offending sprint branches or disable high-cost content.

Suggested Branch: `qa/android-vertical-slice-gate`

Suggested Commits: `test: add vertical slice validation`; `qa: document android build gate`; `fix: address qa blockers`.

Codex Prompt for This Sprint:

```text
Read AGENTS.md, 11_QA_PERFORMANCE_RELEASE.md, 21_VERTICAL_SLICE_SPEC.md, 27_QA_PERFORMANCE_SOFT_LAUNCH_PLAN.md, and 29_RELEASE_BRANCHING_AND_GIT_WORKFLOW.md. Run or prepare tests and Android build validation. Do not add new gameplay features. Document build steps, performance captures, issues, changed files, and rollback plan.
```

## Milestone 10 - Soft Launch Preparation

### Sprint ID: M10-S1

Sprint Name: Soft-Launch Candidate Architecture

Estimated Duration: 2 weeks

Sprint Objective: Prepare the systems and decisions needed before external testing.

Business/Product Value: Converts a vertical slice into a measurable soft-launch candidate plan.

Prerequisites: M9-S1.

Relevant Docs: `15_STORE_COMPLIANCE.md`, `26_MONETIZATION_ANALYTICS_REMOTE_CONFIG_SPEC.md`, `27_QA_PERFORMANCE_SOFT_LAUNCH_PLAN.md`, `29_RELEASE_BRANCHING_AND_GIT_WORKFLOW.md`.

Relevant Folders: docs, ProjectSettings, service layers, store assets folder when created.

Systems Affected: KPI plan, SDK readiness, compliance, remote config, store prep.

Out of Scope: global launch, battle pass, large content expansion.

Detailed Tasks:

- Design: define soft-launch content target and KPI thresholds.
- Engineering: prepare SDK integration plan but do not integrate without approval.
- Unity Scene/Prefab: verify production scenes and build settings.
- UI/UX: draft store-safe screenshots plan and FTUE improvements.
- Art/Audio/VFX: identify creative gaps for store assets.
- Economy/Balance: define first A/B tests and remote keys.
- Analytics: create event QA checklist and dashboard spec.
- QA: store compliance preflight, privacy requirements, crash plan.

Deliverables: soft-launch readiness checklist, KPI dashboard spec, SDK integration plan, store/compliance gap list.

Acceptance Criteria: team knows exactly what remains before real SDKs and store submission.

Definition of Done: no compliance blockers are hidden; SDK work is gated by explicit approval.

Manual Test Plan: dry-run soft-launch checklist.

Performance Checks: ensure performance budgets include lower-end Android devices.

Risks: SDK privacy complexity; insufficient content depth; weak D1 retention.

Rollback Plan: keep SDK plan docs separate from code until approved.

Suggested Branch: `docs/soft-launch-prep`

Suggested Commits: `docs: add soft launch readiness plan`; `docs: add sdk integration gate`; `docs: add kpi dashboard spec`.

Codex Prompt for This Sprint:

```text
Read AGENTS.md, 15_STORE_COMPLIANCE.md, 26_MONETIZATION_ANALYTICS_REMOTE_CONFIG_SPEC.md, 27_QA_PERFORMANCE_SOFT_LAUNCH_PLAN.md, and 29_RELEASE_BRANCHING_AND_GIT_WORKFLOW.md. Produce soft-launch readiness docs and SDK integration gate plan. Do not integrate real SDKs. List changed docs, unresolved compliance items, and validation steps.
```

## Milestone 11 - LiveOps Expansion

### Sprint ID: M11-S1

Sprint Name: LiveOps and Content Expansion Plan

Estimated Duration: 2 weeks

Sprint Objective: Plan post-soft-launch content cadence and operations.

Business/Product Value: Enables retention and monetization growth after core validation.

Prerequisites: M10-S1.

Relevant Docs: `07_MONETIZATION_LIVEOPS.md`, `13_ROADMAP.md`, `27_QA_PERFORMANCE_SOFT_LAUNCH_PLAN.md`, `28_CONTENT_PIPELINE_BALANCING_EDITOR_TOOLS.md`.

Relevant Folders: docs, content configs, economy configs, event configs.

Systems Affected: events, missions, battle pass later, content pipeline, analytics.

Out of Scope: implementing battle pass before KPI validation.

Detailed Tasks:

- Design: define 8-week event calendar and content themes.
- Engineering: plan event config model and remote toggles.
- Unity Scene/Prefab: identify event-specific scene/prefab needs.
- UI/UX: plan event entry points and mission surfaces.
- Art/Audio/VFX: estimate event asset load.
- Economy/Balance: model event rewards and sink pressure.
- Analytics: define event funnel and offer metrics.
- QA: regression checklist for rotating events.

Deliverables: LiveOps roadmap, event config spec, 8-week calendar, content production estimate.

Acceptance Criteria: no LiveOps feature enters implementation without content, config, analytics, and QA plan.

Definition of Done: first three events are scoped with rewards, configs, UI, analytics, and QA gates.

Manual Test Plan: future event dry-run checklist.

Performance Checks: event content respects mobile asset budgets.

Risks: content treadmill too large; economy inflation; over-monetized events.

Rollback Plan: event toggles must disable each event remotely or locally.

Suggested Branch: `docs/liveops-expansion-plan`

Suggested Commits: `docs: plan liveops calendar`; `docs: define event config model`; `docs: add event qa checklist`.

Codex Prompt for This Sprint:

```text
Read AGENTS.md, 07_MONETIZATION_LIVEOPS.md, 13_ROADMAP.md, 27_QA_PERFORMANCE_SOFT_LAUNCH_PLAN.md, and 28_CONTENT_PIPELINE_BALANCING_EDITOR_TOOLS.md. Produce a LiveOps expansion plan, event config spec, analytics requirements, and QA checklist. Do not implement battle pass or event gameplay yet. List changed files and next decisions.
```

