# Survivor Stability And Presentation Notes

This pass hardens the desktop survivor prototype foundation without converting the feature set into production-complete content. The goal is to make active skill feedback, enemy reuse, boss presentation, scene wiring checks, and stress testing easier to validate inside Unity.

## Scope

- Active skill cast, telegraph, and impact feedback can now be assigned from ability data or per-slot fallback config.
- Enemy pooling has an explicit lifecycle contract so reused enemies reset AI, telegraphs, status effects, animation, particles, trails, audio, physics, and NavMesh state before returning to play.
- Boss encounters expose warning, spawn, defeat, and health-threshold presentation hooks.
- Prototype scenes can be repaired and validated from editor menu tools.
- A stress test controller exposes 100-enemy spawn and pool counters through Inspector context menus.

## Active Skill Feedback

`AbilityDefinition` now owns optional feedback fields for active skills:

- Cast VFX prefab
- Telegraph VFX prefab
- Impact VFX prefab
- Cast SFX
- Loop SFX
- Impact SFX
- VFX lifetime
- SFX volume scale
- Camera shake intensity and duration

`ActiveSkillSlot` also owns an `ActiveSkillFeedbackConfig` fallback. Runtime resolution prefers the assigned `AbilityDefinition` feedback first, then the slot fallback. This allows a real ability asset to define production feedback later while keeping prototype hotkeys testable now.

Feedback is played by `SurvivorFeedbackPlayer`. It pools VFX prefab instances by prefab and plays direct `AudioClip` hooks with temporary audio sources. Camera shake is routed through `CameraShakeReceiver` when one is present.

## Enemy Pool Lifecycle

`IPoolLifecycle` is the common contract for pooled runtime cleanup:

- `OnBeforeSpawnFromPool`
- `OnSpawnedFromPool`
- `OnBeforeDespawnToPool`
- `ResetForPool`

`PooledEnemy` calls this contract around spawn and despawn, then applies defensive cleanup for:

- `EnemyMovement`
- `EnemyAttackController`
- `EnemyTelegraphController`
- `StatusEffectController`
- `KnockbackReceiver`
- `RangedShooterController`
- `FastChargerController`
- `AreaBomberController`
- `EnemyAttackPatternController`
- `SplitterEnemyController`
- `ShieldEnemyController`
- `HitFlashController`
- Rigidbody and NavMeshAgent state
- Animator, particles, trails, audio, colliders, and behaviours

`EnemyPoolService` now tracks active and inactive pooled objects. `ProjectilePoolService` exposes the same style of counters for projectiles.

## Boss Encounter Presentation

`ArenaBossDirector` now has:

- Boss warning event and feedback hooks
- Optional intro delay before spawning
- Spawn and defeat VFX/SFX/camera shake hooks
- Health-threshold events for phase pacing
- Runtime flag for whether boss defeat should trigger run victory

Global boss warning is also raised through `BossEvents.OnBossWarningStarted`, so HUD/presentation systems can subscribe without coupling to the survivor director directly.

## Scene Validation And Repair

Use:

```text
Tap Knockout > Survivor > Repair Prototype Scene
Tap Knockout > Survivor > Validate Prototype Scene
```

Repair runs the safe existing wiring passes:

- EventSystem input module repair
- Survivor runtime wiring repair
- Player controls repair

Validation reports missing or weak references for:

- `ArenaRunDirector`
- `SurvivorSpawnDirector`
- `ArenaBossDirector`
- `DesktopInputReader`
- `DesktopSurvivorInputBridge`
- `SurvivorCameraRig`
- `PlayerXPController`
- `PickupCollector`
- `SurvivorHudController`
- `EnemyPoolService`
- `ProjectilePoolService`
- `SurvivorFeedbackPlayer`
- `SurvivorStressTestController`
- Run, arena, wave, spawn group, boss group, XP orb, HUD, player, and weapon config references

## Stress Testing

`SurvivorStressTestController` is intended to live under `DebugRoot`. It exposes context menu actions:

- Spawn Stress Enemies
- Clear Live Enemies To Pool

Runtime counters include:

- Live enemies
- Active pooled enemies
- Inactive pooled enemies
- Active projectiles
- Inactive projectiles
- Run timer
- Active wave id
- Boss active state
- Last stress spawn count

There is intentionally no keyboard shortcut in this component because the project is configured for the Unity Input System package. The actions should be triggered from the Inspector context menu or later routed through proper debug UI.

## Manual Unity Assignments

After running repair, check these fields in Inspector:

- `DebugRoot` should have `SurvivorFeedbackPlayer` and `SurvivorStressTestController`.
- `ArenaBossDirector.feedbackPlayer` should point at `SurvivorFeedbackPlayer`.
- `ActiveSkillController.feedbackPlayer` should point at `SurvivorFeedbackPlayer`.
- `SurvivorStressTestController.spawnDirector` should point at `SurvivorSpawnDirector`.
- `SurvivorStressTestController.enemyPoolService` should point at the scene enemy pool.
- `SurvivorStressTestController.projectilePoolService` should point at the projectile pool, if present.
- `SurvivorStressTestController.stressSpawnGroup` should point at a valid non-boss spawn group when manually stress testing.
- Active ability assets or slots should have VFX/SFX assigned if visual feedback is expected.
- Boss spawn group prefab should have `EnemyHealth`; `BossPhaseController` is recommended for phase pacing.

## Validation Checklist

1. Open `Assets/_Project/Scenes/DesktopSurvivorPrototype.unity`.
2. Run `Tap Knockout > Survivor > Repair Prototype Scene`.
3. Run `Tap Knockout > Survivor > Validate Prototype Scene`.
4. Fix any validation errors before playtesting warnings.
5. Press Play and verify WASD movement, mouse aim, primary attack, dash, active skill hotkeys, wave spawning, XP pickup, level-up selection, boss warning, boss spawn, and boss defeat.
6. Select `DebugRoot > SurvivorStressTestController`, run `Spawn Stress Enemies`, then run `Clear Live Enemies To Pool`.
7. Confirm active/inactive pool counters change and enemies respawn without stale telegraphs, status effects, hit flash, knockback, or stuck AI state.

## Known Risks

- Feedback hooks are presentation plumbing, not final art direction.
- Enemy pooling reset is broad and defensive, but every new enemy behaviour must implement `IPoolLifecycle` if it owns custom runtime state.
- Boss encounter pacing now has warning/threshold hooks, but a full authored boss encounter director is still a later production task.
- Projectile pooling is improved with counters, but projectile prefab contracts still need production-level validation and budget testing.
- Unity scene/prefab serialization must be verified inside the Editor because this pass avoids direct `.unity` YAML edits.
