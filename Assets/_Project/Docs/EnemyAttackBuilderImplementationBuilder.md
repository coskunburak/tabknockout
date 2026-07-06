Implementation Plan


Enemy Attack System — Implementation Plan
Background
Project: Tap Knockout — Desktop survivor roguelike. 11 active cute-monster enemies need distinct, telegraphed attack mechanics layered on top of the existing contact-damage infrastructure. All enemy prefabs live at: Assets/_Project/Prefabs/Enemies/CuteMonsters/

What Already Exists (Repository Truth)
System    Status
EnemyAttackController    ✅ Exists — contact-damage + windup state, IPoolLifecycle
EnemyTelegraphController    ✅ Exists — Circle, Line, ChargePath, Cone, Area, BossSlamArea
EnemyTelegraphConfig (SO)    ✅ Exists — radius, width, length, color, duration, follow/lock
EnemyAttackTelegraphConfig (SO)    ✅ Exists — windup duration, retry delay
EnemyAttackPatternConfig + EnemyAttackPatternController    ✅ Exists — windup→active→cooldown loop
EnemyAttackStep struct    ✅ Exists — type, windup, active, cooldown, damage, range, telegraph
FastChargerController    ✅ Exists — telegraph→lock direction→charge→recovery state machine
RangedShooterController    ✅ Exists — telegraph line→fire projectile
EnemyProjectileController    ✅ Exists — pooled, sweep+overlap detection, IDamageable hit
ProjectilePoolService    ✅ Exists — shared singleton, prefab-keyed stack pools
StatusEffectController + StatusEffectRequest    ✅ Exists — Slow, Freeze, Poison, Burn, Stun
IDamageable, HitContext, KnockbackData    ✅ Exists
IPoolLifecycle    ✅ Exists
Telegraph prefabs    ✅ PF_Telegraph_Circle, PF_Telegraph_Line, PF_Telegraph_ChargePath
Enemy projectile prefab    ✅ PF_EnemyProjectile_CuteMonster
VFX system    ✅ VFXEventType enum + PooledVFXSpawner
Key Architecture Decision: The existing EnemyAttackController is a generic contact-damage manager. The EnemyAttackPatternController is a pattern-based windup→active→cooldown sequencer. The RangedShooterController and FastChargerController are specialized behaviour controllers.

Strategy: Build a new EnemyDistinctAttackController that runs alongside the existing components (not replacing them) to implement the 11 distinct mechanics. The existing EnemyAttackController.autoDealContactDamage will be disabled on enemies that get distinct mechanics. The new system is self-contained, data-driven via a new EnemyAttackConfig ScriptableObject, and pools everything correctly.

Proposed Changes
1. Combat — Area Zone System
[NEW] EnemyAreaZone.cs — Assets/_Project/Scripts/Combat/
Poolable MonoBehaviour for ground zones (Cthulhu slow pool, Mushroom spore zone, Yeti frost area, optional fire patch). Fields: radius, duration, tick interval, damage per tick, StatusEffectType, slow strength, VFX lifecycle, max overlap buffer, target mask, IPoolLifecycle.

2. Enemy — Core Attack Config + Controller
[NEW] EnemyAttackConfig.cs — Assets/_Project/Scripts/Enemy/
ScriptableObject with all fields specified in the request: attackId, attackType (extended enum), damage, range, preferredRange, cooldown, windupTime, activeTime, recoveryTime, movement locks, telegraph config ref, projectile prefab ref, area prefab ref, hitbox shape (Arc/Circle/Line/Projectile/Area), hitbox parameters, knockback, status effect, homing params, debugColor.

[NEW] EnemyDistinctAttackType.cs — Assets/_Project/Scripts/Enemy/
Extended enum covering all 11 attack types: MeleeArc, Charge, Dive, Projectile, RadialBurst, SlimeProjectileArea, Beam, LeapSlash, HomingProjectile, SporeZone, FrostSlamShockwave.

[NEW] EnemyDistinctAttackController.cs — Assets/_Project/Scripts/Enemy/
Main generic controller (IPoolLifecycle). Manages:

Array of EnemyAttackConfig references (multi-config support for Cactus)
Range-based config selection (chooses best valid config each attempt)
State machine: Idle → Windup → Commit → Active → Recovery → Cooldown
Movement lock integration with EnemyMovement (sets a _lockedByAttack flag)
Animator trigger dispatch (safe null-check)
VFX/telegraph hook dispatch
Delegate to attack executor methods per attack type
Debug gizmos (OnDrawGizmosSelected)
[NEW] Attack Executor Methods (internal to controller, separated by region):
ExecuteMeleeArc — OverlapSphere in frontal arc → apply damage
ExecuteChargeDive — uses existing FastChargerController OR internal locked-direction impulse
ExecuteProjectile — uses ProjectilePoolService + EnemyProjectileController
ExecuteRadialBurst — OverlapSphere around enemy → all hits → damage + knockback
ExecuteBeam — linecast/SphereCast along locked forward direction → damage active window
ExecuteLeapSlash — Rigidbody impulse toward snapshot position, landing hitbox on arrival
ExecuteHomingProjectile — homing projectile with mild turn rate (variant of Projectile path)
ExecuteSlimeProjectileArea — Projectile that spawns EnemyAreaZone on expire/hit
ExecuteSporeZone — Choose target position, show telegraph at that world point, activate zone
ExecuteFrostSlamShockwave — Radial burst + spawn EnemyAreaZone for frost slow
3. Enemy — Telegraph Extensions
No new classes needed — EnemyTelegraphController.BeginTelegraphAtPosition already supports world-point telegraphs. The controller will call the existing API. The existing PF_Telegraph_Circle and PF_Telegraph_Line will be used; colored variants will be created as lightweight prefab variants under Assets/_Project/Prefabs/Telegraphs/.

New telegraph prefabs (simple colored ground quads — project-owned):

PF_Telegraph_Circle_Purple (Ghost/Mushroom spore)
PF_Telegraph_Circle_Blue (Frost/Yeti)
PF_Telegraph_Circle_Green (Cactus radial)
PF_Telegraph_Line_Red (Cyclops beam)
PF_Telegraph_Line_Purple (Bat dive)
PF_Telegraph_Line_Yellow (Bee charge)
4. Content — ScriptableObject Attack Configs
Location: Assets/_Project/ScriptableObjects/Enemies/CuteMonsters/AttackConfigs/

12 config assets:

AC_GreenDemon_MeleeArc.asset
AC_Bat_FlyingDive.asset
AC_Bee_StingCharge.asset
AC_YellowDragon_Fireball.asset
AC_Cactus_SpikeProjectile.asset
AC_Cactus_RadialSpikeBurst.asset
AC_Cthulhu_SlimeProjectileSlowPool.asset
AC_Cyclops_EyeBeam.asset
AC_Demon_LeapSlash.asset
AC_Ghost_PhaseHomingCurse.asset
AC_Mushroom_SporePoisonZone.asset
AC_Yeti_FrostSlamShockwave.asset
These are created by the builder tool at runtime (editor-time) from code.

5. Content — Projectile Prefab Variants
All under Assets/_Project/Prefabs/Projectiles/:

PF_EnemyProjectile_Fireball.prefab — orange sphere, EnemyProjectileController
PF_EnemyProjectile_Spike.prefab — green capsule, EnemyProjectileController
PF_EnemyProjectile_SlimeOrb.prefab — green sphere with AreaZoneSpawner component
PF_EnemyProjectile_CurseOrb.prefab — purple sphere, EnemyProjectileController + homing flag
6. Content — Area Zone Prefabs
All under Assets/_Project/Prefabs/Combat/AreaZones/:

PF_AreaZone_SlimePool.prefab — green decal + EnemyAreaZone
PF_AreaZone_SporeZone.prefab — purple decal + EnemyAreaZone
PF_AreaZone_FrostSlab.prefab — blue decal + EnemyAreaZone
7. Editor — Builder Tool
[NEW] EnemyAttackMechanicsBuilder.cs — Assets/_Project/Editor/
Menu: Tap Knockout > Combat > Build Enemy Attack Mechanics

Responsibilities:

Find all 11 CuteMonster enemy prefabs
Create/update EnemyDistinctAttackController on each
Create/ensure AttackConfigs folder + all 12 SO assets (with correct field values)
Assign correct config(s) per enemy (Cactus gets 2)
Add AttackOrigin, ProjectileSpawnPoint, GroundOrigin, VFXRoot child transforms if missing
Disable EnemyAttackController.autoDealContactDamage on enemies with distinct mechanics
Preserve all existing components
Log clear per-enemy summary
[NEW] EnemyAttackMechanicsValidator.cs — Assets/_Project/Editor/
Menu: Tap Knockout > Combat > Validate Enemy Attack Mechanics

Validates all acceptance criteria and generates: Assets/_Project/Docs/EnemyAttackImplementationReport.md

8. Tests
[NEW] EnemyAttackConfigTests.cs — Assets/_Project/Tests/EditMode/Enemy/
Tests covering:

Config assets load and have valid values
Attack type is set correctly per enemy
Damage/cooldown/windup/recovery are in valid ranges
Projectile configs have speed/lifetime/damage
Cactus has both configs
Cyclops beam has telegraph + active timing
Status effect values in safe ranges
Area zone configs have valid radius/duration/tick
Assembly Impact
All new scripts in Assets/_Project/Scripts/Enemy/ → TapKnockout.Enemy assembly. EnemyAreaZone in Assets/_Project/Scripts/Combat/ → TapKnockout.Combat assembly. Editor tools in Assets/_Project/Editor/ → no asmdef needed (editor-only folder). Tests added to existing TapKnockout.Enemy.EditModeTests assembly.

Preservation Guarantees
EnemyAttackController is NOT removed or modified
Existing FastChargerController, RangedShooterController remain intact
Existing spawn/wave/pooling/health systems untouched
Source FBX/texture/material/scene files not modified
No .unity scene YAML hand-edited
Prefabs modified only through builder (PrefabUtility.SavePrefabAsset)
Verification Plan
Automated
EditMode tests: TapKnockout > Test Runner > EditMode
Builder menu: Tap Knockout > Combat > Build Enemy Attack Mechanics
Validator menu: Tap Knockout > Combat > Validate Enemy Attack Mechanics
Manual
Open DesktopSurvivorPrototype.unity
Run Tap Knockout > Survivor > Repair Prototype Scene
Run Tap Knockout > Combat > Build Enemy Attack Mechanics
Press Play — verify each enemy has distinct readable attack behavior
Check Console for NullReferenceExceptions
