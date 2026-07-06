# Survivor Core Loop Implementation Notes

## Status

This pass implements a minimum playable desktop survivor core-loop layer. It is still prototype-grade, but it moves the prototype beyond scaffolding by adding non-locking primary attacks, active skill casting, projectile pooling, stronger enemy pooling, and a boss milestone director.

## What Was Implemented

- Primary attack can run while the player is moving.
- Primary projectile direction can prefer mouse aim.
- Primary projectiles spawn through `ProjectilePoolService` when pooling is enabled.
- Enemy ranged projectiles also spawn through the projectile pool.
- `ActiveSkillController` supports four Q/E/R/F skill slots.
- Fallback active skills work even before authored active skill assets are assigned:
  - Q/R: forward cleave.
  - E/F: ground impact.
- Active skill slots track cooldown and cast state.
- `SurvivorHudController` can bind `ActiveSkillSlotHud` views to active skill cooldown state.
- `EnemyPoolService` and `PooledEnemy` provide prefab-keyed survivor enemy reuse.
- `ArenaBossDirector` owns the survivor boss encounter milestone and raises boss events.
- `DesktopSurvivorPrototypeBuilder` repair/create menus now wire active skills, enemy pooling, and boss director references.

## Primary Attack Changes

`PlayerAttackController` no longer requires the player to be stationary by default. The old stop-to-attack behavior is still available through `Require Stationary To Attack` for legacy/mobile-style tuning.

Projectile direction now uses mouse aim when `Prefer Mouse Aim For Projectiles` is enabled. If no target is found, `Allow Aim Fallback Without Target` can still fire a projectile in the aim/facing direction.

## Active Skill Assignment and Use

Add or repair the player with `ActiveSkillController`.

Default prototype controls:

- Q: fallback forward cleave.
- E: fallback ground impact.
- R: fallback forward cleave.
- F: fallback ground impact.
- 1/2/3/4 also map through `DesktopInputReader`.

To use authored active skills later, assign `AbilityDefinition` assets into the `ActiveSkillController` slot list. The current runtime reads damage, cooldown, duration, and secondary value from the assigned ability when present.

## Projectile Pooling

`ProjectilePoolService` pools projectile instances by prefab. It is created automatically at runtime if no service exists in the scene.

Pool-compatible projectile lifecycle:

- `ProjectileController` returns to pool on hit or lifetime end.
- `EnemyProjectileController` returns to pool on hit or lifetime end.
- Non-pooled projectiles keep their previous deactivate/destroy behavior.

## Enemy Pooling

`EnemyPoolService` and `PooledEnemy` pool survivor enemies by prefab. `SurvivorSpawnDirector` uses the service when `Use Pooling` is enabled.

On reuse, pooled enemies reset common runtime state:

- Rigidbody linear/angular velocity.
- Collider enabled state.
- `EnemyMovement` and `EnemyAttackController` enabled state.
- Health/config via existing `EnemyController.Initialize` or `EnemyHealth.Initialize`.

Legacy room/wave enemy spawning remains untouched.

## Boss Milestone

`ArenaBossDirector` starts the boss encounter when `ArenaRunDirector` reaches `RunConfig.BossSpawnTimeSeconds`.

Current behavior:

- Uses `RunConfig.BossSpawnGroup`.
- Spawns through `SurvivorSpawnDirector.SpawnBoss`.
- Can stop normal survivor spawning during the boss.
- Raises boss intro/defeat events for HUD bindings.
- Boss defeat ends the run as victory through `ArenaRunDirector`.

## Manual Unity Assignments Required

- `WeaponConfig.ProjectilePrefab` should point to a prefab with `ProjectileController`.
- `WeaponConfig.TargetLayers` should point to enemy layers for nearest-target aim. Mouse aim fallback can still fire without it.
- `ActiveSkillController.TargetLayers` should include enemy layers.
- `SurvivorHudController.ActiveSkillSlots` should be assigned if cooldown UI is desired.
- `RunConfig.BossSpawnGroup` must contain a valid boss/enemy config and boss prefab to test boss milestone.
- Boss prefab should include existing boss components if boss bar/phase behavior is expected.
- `BossHealthBarController` should be present in HUD if visible boss HP is required.

## How To Test

1. Open Unity and wait for compile.
2. Run `Tap Knockout > Survivor > Repair Current Scene EventSystem For Input System`.
3. Run `Tap Knockout > Survivor > Repair Current Scene Survivor Runtime`.
4. Save the scene.
5. Enter Play mode.
6. Verify WASD movement while primary attack continues firing.
7. Press Q and E near enemies and verify damage is applied.
8. Watch active skill cooldown fields in `ActiveSkillController`.
9. Verify projectiles reuse under `ProjectilePoolService`.
10. Kill enemies repeatedly and verify hierarchy does not grow endlessly.
11. Set boss spawn time to a short value and verify boss milestone starts.
12. Kill the boss and verify run victory/result flow.

## Known TODOs

- Active skills currently support two simple effect shapes only.
- Active skill VFX/SFX hooks are placeholders.
- Projectile pierce/ricochet/homing runtime is still partial.
- Enemy pool reset covers common components, not every bespoke enemy behavior state.
- Boss director is a milestone controller, not a full production boss encounter authoring system.
- Boss HP bar still needs scene HUD assignment.
- 100+ enemy stress should be validated in Unity Profiler before calling this production-ready.
