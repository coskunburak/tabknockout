# Production VFX Integration Notes

Date: 2026-07-04

## Runtime Flow

The production pass extends the existing `VFXCatalog`, `VFXService`, `PooledVFXSpawner`, `ImpactFeedbackConfig`, and feedback controllers instead of introducing a parallel prefab-reference system. Gameplay code raises semantic events; VFX selection is centralized by `VFXEventType` and the vertical-slice catalog.

## Primary Attack

- `PlayerAttackController` still fires only on manual mouse input and cooldown success.
- Successful shots raise `CombatEvents.OnShotFired` with optional reticle position.
- `ImpactFeedbackController` uses the `ShotFired` profile for muzzle VFX, reticle pulse, shot SFX hooks, and small camera impulse.
- `CombatVFXEventController` listens for `ProjectileSpawnedEvent` and attaches `PrimaryProjectileTrail` to the spawned projectile.
- Projectile impacts remain driven by `CombatEvents.OnDamageDealt` through the existing impact feedback profile path.

## Active Skills

- `ActiveSkillController` now emits `ActiveSkillFeedbackEvents.OnFeedbackRequested` for cast, telegraph, and impact phases.
- Existing direct `ActiveSkillFeedbackConfig` prefab playback is preserved.
- `CombatVFXEventController` skips catalog skill VFX when a direct prefab is assigned, preventing duplicate skill visuals.
- Forward cleave maps to cast and hit events; ground impact maps to cast, area, and hit events.

## Dash

- Dash start/end/trail VFX are driven by `DashEvents.OnDashStarted` and `DashEvents.OnDashEnded`.
- Dash impact remains driven by the existing `ImpactFeedbackController` `DashImpact` profile.
- The trail is parented to the dashing source and has a short lifetime.

## Enemy, Elite, And Boss

- Regular hit/death VFX continue through `ImpactFeedbackController`.
- `SurvivorSpawnDirector.OnAnyEnemySpawned` lets catalog VFX play enemy and elite spawn effects.
- Elite death gets an additional larger catalog burst while generic death remains centralized in impact feedback.
- Boss warning, intro, phase, enrage, heavy attack windup/active, and defeat events map to boss VFX when those systems are present.

## XP And Rewards

- `XPOrb.OnAnyCollected` emits pickup collection feedback.
- `PlayerXPController.OnAnyLevelUp` emits a level-up burst at the player.
- `XPOrbIdle` is cataloged for future prefab-level idle glow work, but the current runtime pass only wires collection and level-up events.

## Spawn Telegraph

- `SurvivorSpawnDirector` still owns spawn timing and pending-spawn lifecycle.
- `PrototypeVerticalSlicePrefabBuilder.EnsureSpawnTelegraphPrefab()` now keeps the project-owned `SpawnTelegraphMarker` wrapper and adds the selected imported magic-circle child when available.
- The wrapper is placed on `Ignore Raycast`, strips colliders from the visual child, scales with spawn radius, and hides cleanly when the pending spawn resolves.

## Editor Setup And Repair

- `VFXFeedbackSetupBuilder.CreateFeedbackSystemRoot()` creates/updates `VFXFeedbackRoot`, `VFXService`, `ImpactFeedbackController`, `CombatVFXEventController`, `AbilityVFXFeedbackController`, `DamageNumberSpawner`, and related references.
- The builder regenerates `Assets/_Project/ScriptableObjects/VFX/VFXCatalog_VerticalSlice.asset` from selected imported assets.
- `Tap Knockout/Survivor/Repair Prototype Scene` calls the feedback builder and prototype prefab builder, so obvious VFX defaults are assigned without direct scene YAML edits.
- `SurvivorReferenceValidator` checks service/catalog/controller wiring and critical catalog definitions.

## Manual Unity Assignments Required

- Run `Tap Knockout > Survivor > Repair Prototype Scene` in each target scene to update scene references and regenerate project-owned helper prefabs.
- Confirm `VFXFeedbackRoot/VFXService.catalog` points to `Assets/_Project/ScriptableObjects/VFX/VFXCatalog_VerticalSlice.asset`.
- Confirm `SurvivorSpawnDirector.spawnTelegraphPrefab` points to `Assets/_Project/Prefabs/VFX/PF_SpawnTelegraphCircle_Prototype.prefab`.
- Review scales/lifetimes in the catalog after seeing the effects in the target arena lighting.
- Assign bespoke direct active-skill prefabs in `ActiveSkillFeedbackConfig` only when a skill needs a custom effect; otherwise the catalog fallback is active.

## Unity Test Checklist

1. Open `Assets/_Project/Scenes/DesktopSurvivorPrototype.unity`.
2. Run `Tap Knockout > Survivor > Repair Prototype Scene`.
3. Run `Tap Knockout > Survivor > Validate Prototype Scene`.
4. Press Play.
5. Move with WASD and aim with the mouse reticle.
6. Left-click primary attack and confirm muzzle VFX, projectile trail, impact VFX, and reticle pulse.
7. Use Q/E/R/F and confirm cleave/ground-impact cast, area, and hit VFX.
8. Dash and confirm start, trail, end, and dash-impact feedback.
9. Kill regular enemies and confirm hit flash, damage number, hit VFX, death VFX, and XP visibility.
10. Trigger elite/boss content if available and confirm spawn, phase, heavy attack, and death VFX are readable.
11. Collect XP and confirm pickup/level-up feedback.
12. Fight for five minutes and check that VFX do not hide the player, reticle, spawn warnings, enemies, or damage numbers.
13. Check Console for missing prefab, shader, script, or pooling warnings.

## TODOs

- Add bespoke idle glow to the XP orb prefab if design wants persistent reward shimmer.
- Add direct skill-specific VFX prefabs for future beam, projectile burst, aura, and orbital skills once those active skill implementations exist.
- Tune catalog scale/lifetime values after visual inspection in the final arena lighting.
- Add quality-level caps only if profiling shows VFX active counts are a bottleneck.
