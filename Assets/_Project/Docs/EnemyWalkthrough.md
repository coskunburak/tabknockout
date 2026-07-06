Enemy Attack System — Implementation Walkthrough
What Was Built
A complete production-level, data-driven enemy attack system for all 11 CuteMonster enemies. The system is fully additive — no existing code was modified or removed.

Files Created
Runtime Scripts
File    Location    Purpose
EnemyAttackConfig.cs
Enemy/    ScriptableObject — all attack parameters per attack
EnemyDistinctAttackType.cs
Enemy/    Extended enum (12 attack types)
EnemyHitboxShape.cs
Enemy/    Hitbox shape enum
EnemyDistinctAttackController.cs
Enemy/    Main state machine + all attack executors
EnemyHomingProjectile.cs
Enemy/    Ghost homing orb steering
ProjectileAreaZoneSpawner.cs
Enemy/    Cthulhu: spawns zone on projectile disable
EnemyAreaZone.cs
Combat/    Poolable ground zone — tick damage + status effect
Editor Tools
File    Menu    Purpose
EnemyAttackMechanicsBuilder.cs
Tap Knockout > Combat > Build Enemy Attack Mechanics    Creates 12 configs + wires all 11 prefabs
EnemyAttackMechanicsValidator.cs
Tap Knockout > Combat > Validate Enemy Attack Mechanics    Validates + generates Markdown report
Tests
File    Suite    Purpose
EnemyAttackConfigTests.cs
EditMode    Config existence, attack types, value ranges, lifecycle
Documentation
File    Purpose
45_ENEMY_ATTACK_SYSTEM.md
Full system reference
Architecture Summary

EnemyAttackConfig (ScriptableObject)
  ↓ assigned to
EnemyDistinctAttackController
  ↓ Telegraph → Windup → Commit → Active → Recovery → Cooldown
  ↓ dispatches
ExecuteMeleeArc / BeginDiveCharge / FireProjectile / ExecuteBeam / ExecuteLeap / ...
  ↓ uses existing
ProjectilePoolService · EnemyTelegraphController · StatusEffectController · KnockbackData
  ↓ new
EnemyAreaZone (ground zones) · EnemyHomingProjectile (steering)
The EnemyAttackController.autoDealContactDamage field is set to false by the builder on all enemies that receive the new controller — contact damage is replaced by the distinct mechanics.

Enemy Mechanics
Enemy    Attack    Key Mechanic
GreenDemon    MeleeArc    Frontal arc hitbox, 65° half-angle, movement locked during commit
Bat    Dive    Directional dive at 5× speed, contact damage on body collision
Bee    Charge    Direction-locked charge, high knockback
YellowDragon    Projectile    Fireball at 7 u/s, preferred stand-off distance
Cactus (far)    SpikeProjectile    Straight spike, fast cooldown
Cactus (near)    RadialBurst    Full sphere burst at 2.2u radius, high knockback
Cthulhu    SlimeProjectileArea    Projectile spawns slow zone (Slow × 0.55) on impact
Cyclops    Beam    1s windup, SphereCast active beam, 1.2s recovery
Demon    LeapSlash    Snaps target position at windup-end, leaps, circular landing hitbox
Ghost    HomingProjectile    Phase-fade windup, mild homing (40°/s turn rate)
Mushroom    SporeZone    Poison zone at player position, 4.5s duration, tick damage
Yeti    FrostSlamShockwave    Radial slam hitbox + frost slow zone centred on self
How to Use
First-time Setup

1. Open Unity
2. Menu: Tap Knockout → Combat → Build Enemy Attack Mechanics
3. Assign projectile/zone prefabs to config assets in:
   Assets/_Project/ScriptableObjects/Enemies/CuteMonsters/AttackConfigs/
4. Menu: Tap Knockout → Combat → Validate Enemy Attack Mechanics
5. Review: Assets/_Project/Docs/EnemyAttackImplementationReport.md
Assign Prefabs (Manual Step)
Because Unity can't reference prefabs across scripts at build time, you need to manually drag these into the config assets:

Config    projectilePrefab    areaZonePrefab
AC_YellowDragon_Fireball    PF_EnemyProjectile_CuteMonster    —
AC_Cactus_SpikeProjectile    PF_EnemyProjectile_CuteMonster    —
AC_Cthulhu_SlimeProjectileSlowPool    PF_EnemyProjectile_CuteMonster    (create zone prefab)
AC_Ghost_PhaseHomingCurse    PF_EnemyProjectile_CuteMonster    —
AC_Mushroom_SporePoisonZone    —    (create zone prefab)
AC_Yeti_FrostSlamShockwave    —    (create zone prefab)
Area Zone Prefabs
Create simple prefabs at Assets/_Project/Prefabs/Combat/AreaZones/ with:

A flat Quad child scaled to 1×1 (visual)
EnemyAreaZone component on root
Set visualRoot to the Quad transform
Design Constraints (Designer Rules)
windupTime ≥ 0.25s — enforced by EditMode test
homingStrength < 0.8 — Ghost orb must be dodgeable
statusEffectSlowMultiplier ∈ [0, 1]
maxActiveZones limits per-enemy zone spam
What Was Preserved
✅ EnemyAttackController.cs — unchanged
✅ FastChargerController.cs — unchanged
✅ RangedShooterController.cs — unchanged
✅ All spawning/wave/pooling/health systems — unchanged
✅ All enemy prefab hierarchy and third-party asset files — unchanged
✅ No .unity scene YAML hand-edited
