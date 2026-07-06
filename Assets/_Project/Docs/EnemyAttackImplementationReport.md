# Enemy Attack Implementation Report

Generated: 2026-07-06 19:43:52

## Player Damage Receiver

- Player prefab: `Assets/_Project/Prefabs/Player/Player.prefab`
- PlayerHealth: present
- CombatHurtbox collider: present

## Attack Config Assets

| Config | Ready | Attack Type | Damage | Projectile | Area Zone | Telegraph | Active VFX | Impact VFX |
|---|---:|---|---:|---|---|---|---|---|
| AC_Bat_FlyingDive | yes | Dive | 6/0 | null | null | PF_Telegraph_Bat_DiveLine | PF_VFX_Bat_DiveTrail | PF_VFX_Bat_DiveImpact |
| AC_Bee_StingCharge | yes | Charge | 7/0 | null | null | PF_Telegraph_Bee_ChargeLine | PF_VFX_Bee_ChargeTrail | PF_VFX_Bee_StingImpact |
| AC_GreenDemon_MeleeArc | yes | MeleeArc | 8/0 | null | null | PF_Telegraph_GreenDemon_MeleeArc | PF_VFX_GreenDemon_MeleeSlash | PF_VFX_GreenDemon_HitImpact |
| AC_YellowDragon_Fireball | yes | Projectile | 12/0 | PF_EnemyProjectile_YellowDragon_Fireball | null | PF_Telegraph_EnemyProjectile_Line | PF_VFX_YellowDragon_FireMuzzle | PF_VFX_Fireball_Impact |
| AC_Cactus_SpikeProjectile | yes | SpikeProjectile | 10/0 | PF_EnemyProjectile_Cactus_Spike | null | PF_Telegraph_EnemyProjectile_Line | PF_VFX_Cactus_SpikeCast | PF_VFX_Cactus_SpikeImpact |
| AC_Cactus_RadialSpikeBurst | yes | RadialBurst | 13/0 | null | null | PF_Telegraph_Cactus_RadialCircle | PF_VFX_Cactus_RadialSpikeBurst | PF_VFX_Cactus_SpikeImpact |
| AC_Cthulhu_SlimeProjectileSlowPool | yes | SlimeProjectileArea | 6/2 | PF_EnemyProjectile_Cthulhu_SlimeOrb | PF_EnemyArea_Cthulhu_SlowPool | PF_Telegraph_EnemyProjectile_Line | PF_VFX_Cthulhu_SlimeOrbCast | PF_VFX_Cthulhu_SlimeSplash |
| AC_Cyclops_EyeBeam | yes | Beam | 18/0 | null | null | PF_Telegraph_Cyclops_BeamLine | PF_VFX_Cyclops_EyeBeam | PF_VFX_Cyclops_BeamImpact |
| AC_Demon_LeapSlash | yes | LeapSlash | 14/0 | null | null | PF_Telegraph_Demon_LandingCircle | PF_VFX_Demon_LeapTrail | PF_VFX_Demon_LandingImpact |
| AC_Ghost_PhaseHomingCurse | yes | HomingProjectile | 9/0 | PF_EnemyProjectile_Ghost_CurseOrb | null | PF_Telegraph_EnemyProjectile_Line | PF_VFX_Ghost_PhaseAura | PF_VFX_Ghost_CurseImpact |
| AC_Mushroom_SporePoisonZone | yes | SporeZone | 1/3 | null | PF_EnemyArea_Mushroom_SporePoisonZone | PF_Telegraph_Mushroom_SporeCircle | PF_VFX_Mushroom_SporeBurst | PF_VFX_Mushroom_SporeCloud |
| AC_Yeti_FrostSlamShockwave | yes | FrostSlamShockwave | 16/0 | null | PF_EnemyArea_Yeti_FrostSlowZone | PF_Telegraph_Yeti_FrostCircle | PF_VFX_Yeti_FrostSlamImpact | PF_VFX_Yeti_FrostShockwave |

## VFX Readiness Audit

Checks each attack config for production-quality VFX references:
- Telegraph, Active, Impact, Projectile, AreaZone prefabs must be non-null where required.
- VFX prefabs must carry an EnemyAttackVFXMarker with productionReady=true, placeholder=false.
- VFX prefabs must have either EnemyAttackVFXAutoCleanup or a ParticleSystem (auto-stop) to prevent leaks.
- Projectile prefabs must have a VisualRoot child (no visible renderer on physics root).
- Area zone prefabs must have a VisualRoot child.

| Enemy | Config | Attack Type | Telegraph | Active VFX | Impact VFX | Projectile | Area Zone | VFX Ready |
|---|---|---|---|---|---|---|---|---|
| Bat | AC_Bat_FlyingDive | Dive | PF_Telegraph_Bat_DiveLine (ok) | PF_VFX_Bat_DiveTrail (ok) | PF_VFX_Bat_DiveImpact (ok) | null (n/a) | null (n/a) | PASS |
| Bee | AC_Bee_StingCharge | Charge | PF_Telegraph_Bee_ChargeLine (ok) | PF_VFX_Bee_ChargeTrail (ok) | PF_VFX_Bee_StingImpact (ok) | null (n/a) | null (n/a) | PASS |
| GreenDemon | AC_GreenDemon_MeleeArc | MeleeArc | PF_Telegraph_GreenDemon_MeleeArc (ok) | PF_VFX_GreenDemon_MeleeSlash (ok) | PF_VFX_GreenDemon_HitImpact (ok) | null (n/a) | null (n/a) | PASS |
| YellowDragon | AC_YellowDragon_Fireball | Projectile | PF_Telegraph_EnemyProjectile_Line (ok) | PF_VFX_YellowDragon_FireMuzzle (ok) | PF_VFX_Fireball_Impact (ok) | PF_EnemyProjectile_YellowDragon_Fireball (ok) | null (n/a) | PASS |
| Cactus | AC_Cactus_SpikeProjectile | SpikeProjectile | PF_Telegraph_EnemyProjectile_Line (ok) | PF_VFX_Cactus_SpikeCast (ok) | PF_VFX_Cactus_SpikeImpact (ok) | PF_EnemyProjectile_Cactus_Spike (ok) | null (n/a) | PASS |
| Cactus | AC_Cactus_RadialSpikeBurst | RadialBurst | PF_Telegraph_Cactus_RadialCircle (ok) | PF_VFX_Cactus_RadialSpikeBurst (ok) | PF_VFX_Cactus_SpikeImpact (ok) | null (n/a) | null (n/a) | PASS |
| Cthulhu | AC_Cthulhu_SlimeProjectileSlowPool | SlimeProjectileArea | PF_Telegraph_EnemyProjectile_Line (ok) | PF_VFX_Cthulhu_SlimeOrbCast (ok) | PF_VFX_Cthulhu_SlimeSplash (ok) | PF_EnemyProjectile_Cthulhu_SlimeOrb (ok) | PF_EnemyArea_Cthulhu_SlowPool (ok) | PASS |
| Cyclops | AC_Cyclops_EyeBeam | Beam | PF_Telegraph_Cyclops_BeamLine (ok) | PF_VFX_Cyclops_EyeBeam (ok) | PF_VFX_Cyclops_BeamImpact (ok) | null (n/a) | null (n/a) | PASS |
| Demon | AC_Demon_LeapSlash | LeapSlash | PF_Telegraph_Demon_LandingCircle (ok) | PF_VFX_Demon_LeapTrail (ok) | PF_VFX_Demon_LandingImpact (ok) | null (n/a) | null (n/a) | PASS |
| Ghost | AC_Ghost_PhaseHomingCurse | HomingProjectile | PF_Telegraph_EnemyProjectile_Line (ok) | PF_VFX_Ghost_PhaseAura (ok) | PF_VFX_Ghost_CurseImpact (ok) | PF_EnemyProjectile_Ghost_CurseOrb (ok) | null (n/a) | PASS |
| Mushroom | AC_Mushroom_SporePoisonZone | SporeZone | PF_Telegraph_Mushroom_SporeCircle (ok) | PF_VFX_Mushroom_SporeBurst (ok) | PF_VFX_Mushroom_SporeCloud (ok) | null (n/a) | PF_EnemyArea_Mushroom_SporePoisonZone (ok) | PASS |
| Yeti | AC_Yeti_FrostSlamShockwave | FrostSlamShockwave | PF_Telegraph_Yeti_FrostCircle (ok) | PF_VFX_Yeti_FrostSlamImpact (ok) | PF_VFX_Yeti_FrostShockwave (ok) | null (n/a) | PF_EnemyArea_Yeti_FrostSlowZone (ok) | PASS |

## Runtime Prefab Mapping

| Enemy | Runtime Prefab Path | Spawn Group Source | Distinct Controller | Configs Valid | Contact Damage | References |
|---|---|---|---:|---:|---|---|
| Bat | `Assets/_Project/Prefabs/Enemies/CuteMonsters/PF_Enemy_Bat.prefab` | `Assets/_Project/ScriptableObjects/Waves/CuteMonsters/SpawnGroup_Cute_Bat.asset` | yes | yes | disabled (distinct ready) | AC_Bat_FlyingDive: projectile=null, area=null, telegraph=PF_Telegraph_Bat_DiveLine, activeVfx=PF_VFX_Bat_DiveTrail, impactVfx=PF_VFX_Bat_DiveImpact |
| Bee | `Assets/_Project/Prefabs/Enemies/CuteMonsters/PF_Enemy_Bee.prefab` | `Assets/_Project/ScriptableObjects/Waves/CuteMonsters/SpawnGroup_Cute_Bee.asset` | yes | yes | disabled (distinct ready) | AC_Bee_StingCharge: projectile=null, area=null, telegraph=PF_Telegraph_Bee_ChargeLine, activeVfx=PF_VFX_Bee_ChargeTrail, impactVfx=PF_VFX_Bee_StingImpact |
| GreenDemon | `Assets/_Project/Prefabs/Enemies/CuteMonsters/PF_Enemy_BasicMelee_GreenDemon_Generated.prefab` | `fallback path` | yes | yes | disabled (distinct ready) | AC_GreenDemon_MeleeArc: projectile=null, area=null, telegraph=PF_Telegraph_GreenDemon_MeleeArc, activeVfx=PF_VFX_GreenDemon_MeleeSlash, impactVfx=PF_VFX_GreenDemon_HitImpact |
| YellowDragon | `Assets/_Project/Prefabs/Enemies/CuteMonsters/PF_Boss_YellowDragon.prefab` | `Assets/_Project/ScriptableObjects/Waves/CuteMonsters/SpawnGroup_Cute_YellowDragon_Boss.asset` | yes | yes | disabled (distinct ready) | AC_YellowDragon_Fireball: projectile=PF_EnemyProjectile_YellowDragon_Fireball, area=null, telegraph=PF_Telegraph_EnemyProjectile_Line, activeVfx=PF_VFX_YellowDragon_FireMuzzle, impactVfx=PF_VFX_Fireball_Impact |
| Cactus | `Assets/_Project/Prefabs/Enemies/CuteMonsters/PF_Enemy_Cactus.prefab` | `Assets/_Project/ScriptableObjects/Waves/CuteMonsters/SpawnGroup_Cute_Cactus.asset` | yes | yes | disabled (distinct ready) | AC_Cactus_SpikeProjectile: projectile=PF_EnemyProjectile_Cactus_Spike, area=null, telegraph=PF_Telegraph_EnemyProjectile_Line, activeVfx=PF_VFX_Cactus_SpikeCast, impactVfx=PF_VFX_Cactus_SpikeImpact<br>AC_Cactus_RadialSpikeBurst: projectile=null, area=null, telegraph=PF_Telegraph_Cactus_RadialCircle, activeVfx=PF_VFX_Cactus_RadialSpikeBurst, impactVfx=PF_VFX_Cactus_SpikeImpact |
| Cthulhu | `Assets/_Project/Prefabs/Enemies/CuteMonsters/PF_Enemy_Cthulhu.prefab` | `Assets/_Project/ScriptableObjects/Waves/CuteMonsters/SpawnGroup_Cute_Cthulhu.asset` | yes | yes | disabled (distinct ready) | AC_Cthulhu_SlimeProjectileSlowPool: projectile=PF_EnemyProjectile_Cthulhu_SlimeOrb, area=PF_EnemyArea_Cthulhu_SlowPool, telegraph=PF_Telegraph_EnemyProjectile_Line, activeVfx=PF_VFX_Cthulhu_SlimeOrbCast, impactVfx=PF_VFX_Cthulhu_SlimeSplash |
| Cyclops | `Assets/_Project/Prefabs/Enemies/CuteMonsters/PF_Enemy_Cyclops.prefab` | `Assets/_Project/ScriptableObjects/Waves/CuteMonsters/SpawnGroup_Cute_Cyclops.asset` | yes | yes | disabled (distinct ready) | AC_Cyclops_EyeBeam: projectile=null, area=null, telegraph=PF_Telegraph_Cyclops_BeamLine, activeVfx=PF_VFX_Cyclops_EyeBeam, impactVfx=PF_VFX_Cyclops_BeamImpact |
| Demon | `Assets/_Project/Prefabs/Enemies/CuteMonsters/PF_Enemy_Demon.prefab` | `Assets/_Project/ScriptableObjects/Waves/CuteMonsters/SpawnGroup_Cute_Demon.asset` | yes | yes | disabled (distinct ready) | AC_Demon_LeapSlash: projectile=null, area=null, telegraph=PF_Telegraph_Demon_LandingCircle, activeVfx=PF_VFX_Demon_LeapTrail, impactVfx=PF_VFX_Demon_LandingImpact |
| Ghost | `Assets/_Project/Prefabs/Enemies/CuteMonsters/PF_Enemy_Ghost.prefab` | `Assets/_Project/ScriptableObjects/Waves/CuteMonsters/SpawnGroup_Cute_Ghost.asset` | yes | yes | disabled (distinct ready) | AC_Ghost_PhaseHomingCurse: projectile=PF_EnemyProjectile_Ghost_CurseOrb, area=null, telegraph=PF_Telegraph_EnemyProjectile_Line, activeVfx=PF_VFX_Ghost_PhaseAura, impactVfx=PF_VFX_Ghost_CurseImpact |
| Mushroom | `Assets/_Project/Prefabs/Enemies/CuteMonsters/PF_Enemy_Mushroom.prefab` | `Assets/_Project/ScriptableObjects/Waves/CuteMonsters/SpawnGroup_Cute_Mushroom.asset` | yes | yes | disabled (distinct ready) | AC_Mushroom_SporePoisonZone: projectile=null, area=PF_EnemyArea_Mushroom_SporePoisonZone, telegraph=PF_Telegraph_Mushroom_SporeCircle, activeVfx=PF_VFX_Mushroom_SporeBurst, impactVfx=PF_VFX_Mushroom_SporeCloud |
| Yeti | `Assets/_Project/Prefabs/Enemies/CuteMonsters/PF_Enemy_Yeti.prefab` | `Assets/_Project/ScriptableObjects/Waves/CuteMonsters/SpawnGroup_Cute_Yeti.asset` | yes | yes | disabled (distinct ready) | AC_Yeti_FrostSlamShockwave: projectile=null, area=PF_EnemyArea_Yeti_FrostSlowZone, telegraph=PF_Telegraph_Yeti_FrostCircle, activeVfx=PF_VFX_Yeti_FrostSlamImpact, impactVfx=PF_VFX_Yeti_FrostShockwave |

## Summary

- Errors: 0
- Warnings: 0
- Status: PASS - all active cute monster attacks are gameplay-ready.

