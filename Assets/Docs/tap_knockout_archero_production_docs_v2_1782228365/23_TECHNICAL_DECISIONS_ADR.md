# Technical Decisions ADR

Use this file for durable architecture decisions. New decisions should be appended or updated deliberately.

## ADR-0001 - No Unity MCP By Default

Status: Accepted

Decision: Work through repository and filesystem inspection unless the user explicitly confirms Unity MCP is available.

Rationale: Repository-safe workflows are predictable and auditable.

Consequences:

- Scene and prefab work must be done through manual Unity steps or approved Editor scripts.
- Codex must not inspect or modify the live Unity Editor unless the user changes this rule.

## ADR-0002 - Do Not Directly Edit `.unity` Scene YAML

Status: Accepted

Decision: Future scene setup should use Unity Editor manual steps or Editor tooling, not raw YAML scene edits.

Rationale: Raw Unity scene YAML is fragile and easy to corrupt without Editor validation.

## ADR-0003 - Production Code Under `Assets/_Project`

Status: Accepted

Decision: New production scripts, prefabs, ScriptableObjects, art, UI, tests, and tools should live under `Assets/_Project`.

Rationale: A clean production root prevents sprawl.

## ADR-0004 - Third-Party Assets Under `Assets/ThirdParty`

Status: Accepted

Decision: Approved external assets should live under `Assets/ThirdParty/<Source>/<PackName>`.

Rationale: Licensing, updates, and credits are easier to audit when source assets are separated from game-authored assets.

## ADR-0005 - Data-Driven Configs With Stable IDs

Status: Accepted

Decision: Gameplay, enemy, arena, run, wave, ability, boss, progression, analytics, and optional future monetization data should be driven by ScriptableObject configs with stable IDs.

Rationale: Survivor balance requires frequent iteration and safe reporting.

## ADR-0006 - Service Abstractions Before SDKs

Status: Accepted

Decision: Analytics, remote config, save, audio, and any future ads/IAP must be behind interfaces before real SDK integration.

Rationale: Gameplay code should not depend on vendor SDKs.

## ADR-0007 - Dash-Impact Remains a First-Class Combat Source

Status: Accepted

Decision: Dash hits are represented in shared combat data with impact damage, knockback, duplicate-hit prevention, and event hooks.

Rationale: Dash-impact remains an identity element after the desktop survivor pivot.

## ADR-0008 - Pivot to Desktop 3D Survivor Roguelike

Status: Accepted

Decision: The product direction pivots from mobile room-based action roguelite to desktop-first 3D arena survivor roguelike.

Rationale: The desired game is now closer to a PC/Steam arena survivor with active skills, wave pressure, and build crafting.

Consequences:

- PC/Steam is the default platform.
- WASD/mouse is the default control model.
- Mobile monetization is not an MVP driver.
- Existing useful systems should be migrated, not blindly deleted.

## ADR-0009 - Arena Run Structure Replaces Room-First Structure

Status: Accepted

Decision: The canonical gameplay structure is an arena run with a wave timeline, spawn director, XP pickups, level-up choices, elites, and boss milestones.

Rationale: Survivor gameplay needs continuous pressure rather than discrete room clearing.

Consequences:

- Room systems become legacy or future challenge-mode infrastructure.
- New work should target `ArenaRunDirector`, `SpawnDirector`, and `WaveDirector`.

## ADR-0010 - WASD and Mouse Replace Touch-First Controls

Status: Accepted

Decision: MVP controls are WASD movement, mouse aim, keyboard active skills, and dash/evade key.

Rationale: Desktop survivor gameplay requires direct keyboard/mouse control.

Consequences:

- Touch joystick is deprecated for the prototype.
- UI must show skill hotkeys and desktop input states.

## ADR-0011 - Isometric/Top-Down Follow Camera

Status: Accepted

Decision: Use an isometric/top-down 3D follow camera for the survivor prototype.

Rationale: This supports battlefield readability, enemy density, and mouse aim.

Consequences:

- Art, VFX, telegraphs, and UI must be reviewed from gameplay camera distance.
- Camera readability is a core acceptance gate.

## ADR-0012 - Object Pooling Required for Survivor Density

Status: Accepted

Decision: Enemies, projectiles, pickups, XP orbs, VFX, telegraphs, and damage numbers should use pooling before survivor-scale stress tests are considered valid.

Rationale: Runtime instantiate/destroy patterns will not hold under 100+ enemy density.

## ADR-0013 - Docs-First Migration Before Code Migration

Status: Accepted

Decision: The documentation migration must happen before broad gameplay code migration.

Rationale: The previous docs contained incompatible platform and loop assumptions. A coherent target reduces rework and accidental implementation of legacy systems.
