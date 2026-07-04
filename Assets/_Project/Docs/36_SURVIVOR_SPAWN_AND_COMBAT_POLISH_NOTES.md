# Survivor Spawn And Combat Polish Notes

## Scope

This pass improves the desktop survivor prototype without changing the scene YAML by hand and without adding packages, imported assets, SDKs, or protected reference-game content.

## Spawn Telegraphs

- `SurvivorSpawnDirector` can delay regular enemy spawns behind a short warning marker.
- If `spawnTelegraphPrefab` is assigned, the prefab is pooled and reused.
- If no prefab is assigned, the runtime creates a pooled `SpawnTelegraphMarker` LineRenderer ring so prototype testing still has a visible warning.
- Boss spawns and debug spawns remain immediate so milestone/debug flows do not unexpectedly delay.

## Spawn Safety

- `ArenaConfig` now owns player-safe spawn tuning:
  - `spawnPressureMode`
  - `playerAvoidSpawnRadius`
  - `edgeSpawnInnerRadiusFactor`
  - `mixedEdgePressureChance`
  - `spawnPositionRetries`
  - `spawnClearanceRadius`
  - `spawnBlockerLayers`
  - `fallbackToArenaEdgeWhenInvalid`
- Spawn resolution retries candidates before falling back.
- Fallback selection prefers arena edge candidates and tracks the farthest candidate from the player when strict validation cannot find a perfect spot.
- Enemy grounding still happens at spawn time through `EnemySpawnPlacement`.

## Edge Pressure

- `SpawnPressureMode.Mixed` is the default direction for the prototype: it alternates between player-ring pressure and arena-edge pressure.
- `SpawnPressureMode.EdgePressure` biases candidates near the arena boundary.
- `SpawnPressureMode.RandomWithinArena` remains available for specific tests, but should be used carefully because the player-safe radius still rejects unsafe points.

## Live Cap And Budget

- Live enemy cap and live budget now include pending telegraphed spawns.
- `SurvivorSpawnDirector` has a separate live-budget cap:
  - `baseLiveEnemyBudget`
  - `maxLiveEnemyBudget`
  - `liveEnemyBudgetRampPerMinute`
- `SpawnGroupConfig.BudgetCost` now gates live spawn pressure separately from raw enemy count.
- `SpawnGroupConfig.SpawnBurstCount` contributes to requested batch size while still respecting live cap and budget.

## Dash I-Frames And Cooldown Hooks

- Dash i-frames remain driven by `PlayerDashController` and `PlayerHealth` through `IsDashInvulnerable`.
- New dash cooldown events are exposed:
  - `DashEvents.OnDashCooldownStarted`
  - `DashEvents.OnDashCooldownChanged`
  - `DashEvents.OnDashCooldownReady`
- Cooldown refunds also publish updated cooldown state.

## Combat Feedback

- `ImpactFeedbackController` already centralizes hit pause, hit flash, VFX, SFX hooks, camera shake, and damage numbers.
- Normal projectile/generic hits now request subtle camera shake only when damage passes `minimumDamageForNormalCameraShake`.
- `ImpactFeedbackConfig` enables damage numbers by default for new configs.
- `DamageNumberSpawner` now has:
  - minimum damage filtering
  - per-second rate limiting
  - pooled runtime Text fallback when no final `DamageNumberView` prefab is assigned

## Enemy Death And XP

- `ImpactFeedbackController` continues to raise enemy death VFX/SFX hooks from `CombatEvents.OnEntityKilled`.
- `ArenaRunDirector` now tracks rewarded enemies per run so repeated kill events cannot grant XP twice.
- Direct XP fallback remains available when no XP orb prefab or collector is assigned.

## Validator And Repair

- `SurvivorReferenceValidator` now checks spawn telegraph, spawn safety, live-budget, feedback root, damage numbers, hit pause, VFX service, camera shake, hit flash, and knockback receiver wiring.
- `DesktopSurvivorPrototypeBuilder` and repair flow now call `VFXFeedbackSetupBuilder.CreateFeedbackSystemRoot()` to create and wire the combat feedback root.

## Manual Unity Assignments

- Run `Tap Knockout > Survivor > Repair Prototype Scene` to assign the project-owned spawn telegraph wrapper. It keeps `SpawnTelegraphMarker` timing and adds the selected imported magic-circle VFX child when the asset is available.
- Assign/tune `ImpactFeedbackConfig` on `ImpactFeedbackController` for final hit pause, shake, VFX mapping, and damage-number thresholds.
- Assign final `DamageNumberView` prefab to `DamageNumberSpawner.numberPrefab` when UI art is ready. Runtime Text fallback is acceptable for prototype testing.
- Ensure `ArenaConfig.spawnBlockerLayers` only includes obstacle/blocker layers, not ground, player, pickups, or enemies.
- Re-save `ArenaConfig_DesktopSurvivorPrototype.asset` after inspecting the new spawn safety fields.

## Unity Test Checklist

1. Open `Assets/_Project/Scenes/DesktopSurvivorPrototype.unity`.
2. Run `Tap Knockout/Survivor/Repair Prototype Scene`.
3. Run `Tap Knockout/Survivor/Validate Prototype Scene`.
4. Enter Play Mode and stand near the arena center, then near the arena edge.
5. Confirm regular enemies show a warning ring before spawning.
6. Confirm enemies do not appear inside the player's safe radius.
7. Confirm mixed mode sometimes pushes enemies from arena edges.
8. Confirm dash i-frames still ignore player damage during the configured i-frame window.
9. Confirm dash cooldown HUD still updates, and optional listeners can use the new cooldown events.
10. Confirm hits trigger hit stop, hit flash, subtle shake, death VFX/SFX hooks, XP reward, and damage numbers when the feedback root is present.

## Known Follow-Ups

- Replace runtime spawn telegraph and runtime damage-number Text placeholders with final prefabs.
- Tune live budget per wave after several 5-10 minute playtests.
- Add scene-level visual QA once Unity batchmode licensing is available in this environment.
