# Combat Hit Feel Polish Notes

## Scope

This pass keeps desktop survivor controls intact: WASD movement, mouse aim, and left-click primary fire. The feedback work is centralized in `TapKnockout.Feedback.ImpactFeedbackController` and tuned through `ImpactFeedbackConfig` profiles.

## Profile Contract

`ImpactFeedbackConfig` now owns these serialized profiles:

- `NormalProjectileHit`: flash, damage number, VFX/SFX hook. No hit stop or camera shake by default.
- `HeavyProjectileHit`: larger number, flash, VFX/SFX hook, rate-limited hit stop, and rate-limited shake.
- `SkillHit`: stronger flash/number/VFX/SFX with rate-limited hit stop and shake for area hits.
- `DashImpact`: strong hit stop, shake, flash, number, VFX/SFX hook.
- `EnemyDeath`: death VFX/SFX hook and small rate-limited shake. No damage number or hit stop.
- `BossHit`: heavier hit profile for targets with boss components.
- `PlayerDamaged`: player damage number, flash if present, SFX hook, and rate-limited shake.
- `ShotFired`: reticle pulse, optional muzzle flash, shot SFX fallback, `PrimaryFireMuzzle` VFX event, optional `ReticleFirePulse` catalog VFX, and subtle rate-limited shot shake.

Run `Tools/Tap Knockout/VFX/Create Feedback System Root` or `Tap Knockout/Survivor/Repair Prototype Scene` to persist missing profiles into `Assets/_Project/VFX/ImpactFeedbackConfig.asset`.

## Event Flow

Primary fire raises `CombatEvents.RaiseShotFired` only after a shot is actually completed. If an `ImpactFeedbackController` handles the event, the `ShotFired` profile owns reticle pulse, muzzle flash, shot SFX, VFX, and camera impulse. If no handler exists, `PlayerAttackController` falls back to its previous local shot feedback fields.

Projectile, skill, dash, boss, and player-damage feedback all flow through combat damage events. Dash feedback still listens to `DashEvents.OnDashHit`, while `DamageDealt` skips dash hits to avoid duplicates.

Enemy death feedback is side-effect free. XP remains owned by `ArenaRunDirector`, which already guards rewards with `xpRewardedEnemies`, and `EnemyHealth` guards death with `hasDied`.

## Spam Control

Normal projectile hits intentionally do not request hit stop or shake. Heavy projectile, skill, dash, boss, player-damage, and enemy-death profiles have independent hit-stop and camera-shake cooldowns. `HitPauseService` also has a service-level request cooldown so overlapping area hits cannot extend freeze frames every target.

## Pooling Lifecycle

`HitFlashController` clears flash state on enable, disable, and pool lifecycle calls. `KnockbackReceiver` clears velocity, chain targets, wall-slam state, and rigidbody velocity on disable and pool transitions. `DamageNumberView` resets scale when it returns to the pool.

## Manual Assignments

The repair tool wires the feedback root, config, VFX service, hit pause service, damage number spawner, camera shake receiver, and AudioSource. Final production assets still need designer assignment:

- VFX catalog entries for primary muzzle, projectile trail/impact, dash start/trail/end/impact, enemy hit/death/spawn, active skill cast/area/hit, boss/elite, XP, and reticle pulse events.
- Optional profile AudioClips for impacts and deaths.
- Optional `PlayerAttackController.muzzleFlash` and `shotSfx`.
- A final `DamageNumberView` prefab if the runtime text fallback is not visually sufficient.
