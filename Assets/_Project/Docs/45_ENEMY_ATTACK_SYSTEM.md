# Enemy Attack System — Technical Documentation

**Version:** 1.0  
**Assembly:** `TapKnockout.Enemy` + `TapKnockout.Combat`

---

## Overview

The Enemy Attack System provides distinct, data-driven, telegraphed attack mechanics for all 11 CuteMonster enemy types. It operates alongside the existing `EnemyAttackController` (which handles contact damage) without removing or breaking any prior system.

Each attack follows the **Telegraph → Commit → Damage → Recovery** loop enforced by the state machine in `EnemyDistinctAttackController`.

---

## Architecture

```
EnemyAttackConfig (ScriptableObject)
    ↓ assigned to
EnemyDistinctAttackController (MonoBehaviour)
    ↓ state machine
    Idle → Windup (telegraph) → Active (damage) → Recovery → Cooldown
    ↓ dispatches to
ExecuteMeleeArc / BeginDiveCharge / FireProjectile / ExecuteBeam / ...
    ↓ uses
ProjectilePoolService     — pooled projectile spawning
EnemyAreaZone             — ground zones (slow pool, spore, frost)
EnemyTelegraphController  — existing telegraph visuals
StatusEffectController    — slow, poison, freeze, stun
KnockbackData             — knockback via HitContext
```

---

## New Files

### Runtime

| File | Assembly | Purpose |
|------|----------|---------|
| `EnemyAttackConfig.cs` | Enemy | ScriptableObject — all attack parameters |
| `EnemyDistinctAttackType.cs` | Enemy | Extended enum for 12 attack type variants |
| `EnemyHitboxShape.cs` | Enemy | Circle / Arc / Line / Projectile / Area |
| `EnemyDistinctAttackController.cs` | Enemy | Main state machine + executor |
| `EnemyHomingProjectile.cs` | Enemy | Homing behaviour for Ghost curse orb |
| `ProjectileAreaZoneSpawner.cs` | Enemy | Spawns area zone when projectile is disabled |
| `EnemyAreaZone.cs` | Combat | Poolable ground zone — tick damage + status |

### Editor

| File | Purpose |
|------|---------|
| `EnemyAttackMechanicsBuilder.cs` | Creates configs + wires all 11 prefabs |
| `EnemyAttackMechanicsValidator.cs` | Validates system + generates report |

### Tests

| File | Purpose |
|------|---------|
| `EnemyAttackConfigTests.cs` | EditMode tests for all configs + components |

---

## EnemyAttackConfig Fields

### Timing
| Field | Meaning |
|-------|---------|
| `windupTime` | Telegraph duration — must be readable (≥ 0.25s) |
| `activeTime` | Damage window / projectile fire moment |
| `recoveryTime` | Post-attack vulnerability window |
| `cooldown` | Time before enemy can repeat this attack |

### Hitbox
| Field | Meaning |
|-------|---------|
| `hitboxShape` | Circle (radial), Arc (frontal), Line (beam), Projectile, Area |
| `hitboxRadius` | Radius for Circle/Arc shapes |
| `hitboxArcHalfAngle` | Degrees each side of forward (Arc shape) |
| `beamLength/Width` | Cyclops beam dimensions |

### Projectile
| Field | Meaning |
|-------|---------|
| `projectilePrefab` | Pooled prefab (must have `EnemyProjectileController`) |
| `projectileSpeed` | Units/second |
| `projectileLifetime` | Max travel time before despawn |
| `homingStrength` | 0–1 (0 = straight, keep < 0.8 for dodgeability) |

### Area Zone
| Field | Meaning |
|-------|---------|
| `areaZonePrefab` | Ground zone prefab (must have `EnemyAreaZone`) |
| `areaZoneRadius` | Zone radius |
| `areaZoneDuration` | Lifetime in seconds |
| `areaZoneTickInterval` | How often it ticks damage/status |
| `areaZoneTickDamage` | Damage per tick (0 = status-only) |
| `maxActiveZones` | Per-enemy cap |

---

## Enemy Mechanics Map

| Enemy | Attack Type | Range | Windup | Special |
|-------|-------------|-------|--------|---------|
| GreenDemon | MeleeArc | 1.5u | 0.3s | Arc half-angle 65°, locks movement |
| Bat | Dive | 7u | 0.6s | Directional dive, overshoot |
| Bee | Charge | 6u | 0.45s | Fast charge, locks direction |
| YellowDragon | Projectile | 7u | 0.55s | Fireball, preferred stand-off range |
| Cactus (far) | SpikeProjectile | 7u | 0.45s | Straight spike |
| Cactus (near) | RadialBurst | 2.2u | 0.65s | Full circle burst, high KB |
| Cthulhu | SlimeProjectileArea | 7u | 0.6s | Projectile → slow pool zone |
| Cyclops | Beam | 8u | 1.0s | Wide active window, long recovery |
| Demon | LeapSlash | 5u | 0.65s | Snaps target pos, leaps, radial hit |
| Ghost | HomingProjectile | 7u | 0.7s | Phase fade windup, homing orb |
| Mushroom | SporeZone | 7u | 0.75s | Poison zone at player's feet |
| Yeti | FrostSlamShockwave | 2.8u | 1.0s | Slam + radial + frost zone |

---

## Setup: Running the Builder

1. Open Unity Editor
2. Menu: **Tap Knockout → Combat → Build Enemy Attack Mechanics**
3. Assign projectile/area-zone prefabs to the generated configs manually  
   (or wire them through future automation)
4. Menu: **Tap Knockout → Combat → Validate Enemy Attack Mechanics**
5. Review report at `Assets/_Project/Docs/EnemyAttackImplementationReport.md`

---

## Design Constraints

- `windupTime` ≥ 0.25s — designer rule, enforced by tests
- `homingStrength` < 0.8 — Ghost orb must be dodgeable
- Status effect `slowMultiplier` ∈ [0, 1]
- Area zone `maxActiveZones` per enemy instance prevents spam
- `EnemyAttackController.autoDealContactDamage` is set to `false` for enemies  
  using `EnemyDistinctAttackController` — builder does this automatically

---

## Extending: Adding a New Enemy

1. Create a new `EnemyAttackConfig` via **Tap Knockout → Enemies → Enemy Attack Config**
2. Set attack type, timing, hitbox, and prefabs
3. Add `EnemyDistinctAttackController` to the enemy prefab
4. Assign the config in `attackConfigs` array
5. Add child transforms: `AttackOrigin`, `ProjectileSpawnPoint`, `GroundOrigin`
6. Run validator to confirm

---

## Pool Safety

`EnemyDistinctAttackController` implements `IPoolLifecycle`. On despawn:
- All state is reset (timer, direction, active config, hit set)
- Movement is restored
- Telegraph is cleared
- Zone references are cleaned

`EnemyAreaZone` implements `IPoolLifecycle` and self-deactivates on expire.

---

*Generated by Antigravity — Tap Knockout Enemy Attack System v1.0*
