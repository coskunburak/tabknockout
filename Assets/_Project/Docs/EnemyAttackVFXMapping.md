# Enemy Attack VFX Mapping

Generated: 2026-07-06 19:43:52

For each enemy, this document records which VFX prefab assets are assigned to each attack config field.

## Bat

### AC_Bat_FlyingDive (Dive)

- **Telegraph:** Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/PF_Telegraph_Bat_DiveLine.prefab
- **Active VFX:** Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Bat_DiveTrail.prefab
- **Impact VFX:** Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Bat_DiveImpact.prefab
- **Projectile:** null
- **Area Zone:** null

**VFX Asset Source:**
  - Telegraph: `Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/PF_Telegraph_Bat_DiveLine.prefab` | type=ProjectOwnedProcedural | ready=True | source=procedural
  - Active VFX: `Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Bat_DiveTrail.prefab` | type=ProjectOwnedWrapper | ready=True | source=Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Nature/CFXR4 Wind Trails.prefab
  - Impact VFX: `Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Bat_DiveImpact.prefab` | type=ProjectOwnedWrapper | ready=True | source=Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Purple_Hit_02.prefab

**Timing:**
- Windup: 0.60s — Telegraph visible
- Active: 0.50s — Damage window, Active VFX plays
- Recovery: 0.70s — VFX cleanup

## Bee

### AC_Bee_StingCharge (Charge)

- **Telegraph:** Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/PF_Telegraph_Bee_ChargeLine.prefab
- **Active VFX:** Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Bee_ChargeTrail.prefab
- **Impact VFX:** Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Bee_StingImpact.prefab
- **Projectile:** null
- **Area Zone:** null

**VFX Asset Source:**
  - Telegraph: `Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/PF_Telegraph_Bee_ChargeLine.prefab` | type=ProjectOwnedProcedural | ready=True | source=procedural
  - Active VFX: `Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Bee_ChargeTrail.prefab` | type=ProjectOwnedWrapper | ready=True | source=Assets/GabrielAguiarProductions/FreeQuickEffectsVol1/Prefabs/vfx_Hyperdrive_01.prefab
  - Impact VFX: `Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Bee_StingImpact.prefab` | type=ProjectOwnedWrapper | ready=True | source=Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Impacts/CFXR Hit D 3D (Yellow).prefab

**Timing:**
- Windup: 0.45s — Telegraph visible
- Active: 0.40s — Damage window, Active VFX plays
- Recovery: 0.60s — VFX cleanup

## GreenDemon

### AC_GreenDemon_MeleeArc (MeleeArc)

- **Telegraph:** Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/PF_Telegraph_GreenDemon_MeleeArc.prefab
- **Active VFX:** Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_GreenDemon_MeleeSlash.prefab
- **Impact VFX:** Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_GreenDemon_HitImpact.prefab
- **Projectile:** null
- **Area Zone:** null

**VFX Asset Source:**
  - Telegraph: `Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/PF_Telegraph_GreenDemon_MeleeArc.prefab` | type=ProjectOwnedProcedural | ready=True | source=procedural
  - Active VFX: `Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_GreenDemon_MeleeSlash.prefab` | type=ProjectOwnedWrapper | ready=True | source=Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Orange_Slash_1.prefab
  - Impact VFX: `Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_GreenDemon_HitImpact.prefab` | type=ProjectOwnedWrapper | ready=True | source=Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Green_Hit.prefab

**Timing:**
- Windup: 0.30s — Telegraph visible
- Active: 0.15s — Damage window, Active VFX plays
- Recovery: 0.55s — VFX cleanup

## YellowDragon

### AC_YellowDragon_Fireball (Projectile)

- **Telegraph:** Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/PF_Telegraph_EnemyProjectile_Line.prefab
- **Active VFX:** Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_YellowDragon_FireMuzzle.prefab
- **Impact VFX:** Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Fireball_Impact.prefab
- **Projectile:** Assets/_Project/Content/Combat/Projectiles/EnemyAttacks/PF_EnemyProjectile_YellowDragon_Fireball.prefab
- **Area Zone:** null

**VFX Asset Source:**
  - Telegraph: `Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/PF_Telegraph_EnemyProjectile_Line.prefab` | type=ProjectOwnedProcedural | ready=True | source=procedural
  - Active VFX: `Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_YellowDragon_FireMuzzle.prefab` | type=ProjectOwnedWrapper | ready=True | source=Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Fireball.prefab
  - Impact VFX: `Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Fireball_Impact.prefab` | type=ProjectOwnedWrapper | ready=True | source=Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/Fire Hit .prefab
  - Projectile: `Assets/_Project/Content/Combat/Projectiles/EnemyAttacks/PF_EnemyProjectile_YellowDragon_Fireball.prefab` | VisualRoot=True | source=Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Fireball.prefab

**Timing:**
- Windup: 0.55s — Telegraph visible
- Active: 0.10s — Damage window, Active VFX plays
- Recovery: 0.65s — VFX cleanup

## Cactus

### AC_Cactus_SpikeProjectile (SpikeProjectile)

- **Telegraph:** Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/PF_Telegraph_EnemyProjectile_Line.prefab
- **Active VFX:** Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Cactus_SpikeCast.prefab
- **Impact VFX:** Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Cactus_SpikeImpact.prefab
- **Projectile:** Assets/_Project/Content/Combat/Projectiles/EnemyAttacks/PF_EnemyProjectile_Cactus_Spike.prefab
- **Area Zone:** null

**VFX Asset Source:**
  - Telegraph: `Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/PF_Telegraph_EnemyProjectile_Line.prefab` | type=ProjectOwnedProcedural | ready=True | source=procedural
  - Active VFX: `Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Cactus_SpikeCast.prefab` | type=ProjectOwnedWrapper | ready=True | source=Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Green_Hit.prefab
  - Impact VFX: `Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Cactus_SpikeImpact.prefab` | type=ProjectOwnedWrapper | ready=True | source=Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Green_Hit.prefab
  - Projectile: `Assets/_Project/Content/Combat/Projectiles/EnemyAttacks/PF_EnemyProjectile_Cactus_Spike.prefab` | VisualRoot=True | source=procedural

**Timing:**
- Windup: 0.45s — Telegraph visible
- Active: 0.10s — Damage window, Active VFX plays
- Recovery: 0.55s — VFX cleanup

### AC_Cactus_RadialSpikeBurst (RadialBurst)

- **Telegraph:** Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/PF_Telegraph_Cactus_RadialCircle.prefab
- **Active VFX:** Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Cactus_RadialSpikeBurst.prefab
- **Impact VFX:** Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Cactus_SpikeImpact.prefab
- **Projectile:** null
- **Area Zone:** null

**VFX Asset Source:**
  - Telegraph: `Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/PF_Telegraph_Cactus_RadialCircle.prefab` | type=ProjectOwnedProcedural | ready=True | source=procedural
  - Active VFX: `Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Cactus_RadialSpikeBurst.prefab` | type=ProjectOwnedWrapper | ready=True | source=Assets/GabrielAguiarProductions/FreeQuickEffectsVol1/Prefabs/vfx_Sparks_01.prefab
  - Impact VFX: `Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Cactus_SpikeImpact.prefab` | type=ProjectOwnedWrapper | ready=True | source=Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Green_Hit.prefab

**Timing:**
- Windup: 0.65s — Telegraph visible
- Active: 0.15s — Damage window, Active VFX plays
- Recovery: 0.70s — VFX cleanup

## Cthulhu

### AC_Cthulhu_SlimeProjectileSlowPool (SlimeProjectileArea)

- **Telegraph:** Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/PF_Telegraph_EnemyProjectile_Line.prefab
- **Active VFX:** Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Cthulhu_SlimeOrbCast.prefab
- **Impact VFX:** Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Cthulhu_SlimeSplash.prefab
- **Projectile:** Assets/_Project/Content/Combat/Projectiles/EnemyAttacks/PF_EnemyProjectile_Cthulhu_SlimeOrb.prefab
- **Area Zone:** Assets/_Project/Content/Combat/AreaZones/EnemyAttacks/PF_EnemyArea_Cthulhu_SlowPool.prefab

**VFX Asset Source:**
  - Telegraph: `Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/PF_Telegraph_EnemyProjectile_Line.prefab` | type=ProjectOwnedProcedural | ready=True | source=procedural
  - Active VFX: `Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Cthulhu_SlimeOrbCast.prefab` | type=ProjectOwnedWrapper | ready=True | source=Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Green_Hit.prefab
  - Impact VFX: `Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Cthulhu_SlimeSplash.prefab` | type=ProjectOwnedWrapper | ready=True | source=Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Liquids/CFXR Water Splash (Smaller).prefab
  - Projectile: `Assets/_Project/Content/Combat/Projectiles/EnemyAttacks/PF_EnemyProjectile_Cthulhu_SlimeOrb.prefab` | VisualRoot=True | source=procedural
  - AreaZone: `Assets/_Project/Content/Combat/AreaZones/EnemyAttacks/PF_EnemyArea_Cthulhu_SlowPool.prefab` | VisualRoot=True | source=procedural

**Timing:**
- Windup: 0.60s — Telegraph visible
- Active: 0.10s — Damage window, Active VFX plays
- Recovery: 0.75s — VFX cleanup

## Cyclops

### AC_Cyclops_EyeBeam (Beam)

- **Telegraph:** Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/PF_Telegraph_Cyclops_BeamLine.prefab
- **Active VFX:** Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Cyclops_EyeBeam.prefab
- **Impact VFX:** Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Cyclops_BeamImpact.prefab
- **Projectile:** null
- **Area Zone:** null

**VFX Asset Source:**
  - Telegraph: `Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/PF_Telegraph_Cyclops_BeamLine.prefab` | type=ProjectOwnedProcedural | ready=True | source=procedural
  - Active VFX: `Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Cyclops_EyeBeam.prefab` | type=ProjectOwnedWrapper | ready=True | source=Assets/GabrielAguiarProductions/FreeQuickEffectsVol1/Prefabs/vfx_Sparks_01.prefab
  - Impact VFX: `Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Cyclops_BeamImpact.prefab` | type=ProjectOwnedWrapper | ready=True | source=Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/Fire Hit .prefab

**Timing:**
- Windup: 1.00s — Telegraph visible
- Active: 0.40s — Damage window, Active VFX plays
- Recovery: 1.20s — VFX cleanup

## Demon

### AC_Demon_LeapSlash (LeapSlash)

- **Telegraph:** Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/PF_Telegraph_Demon_LandingCircle.prefab
- **Active VFX:** Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Demon_LeapTrail.prefab
- **Impact VFX:** Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Demon_LandingImpact.prefab
- **Projectile:** null
- **Area Zone:** null

**VFX Asset Source:**
  - Telegraph: `Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/PF_Telegraph_Demon_LandingCircle.prefab` | type=ProjectOwnedProcedural | ready=True | source=procedural
  - Active VFX: `Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Demon_LeapTrail.prefab` | type=ProjectOwnedWrapper | ready=True | source=Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Nature/CFXR4 Wind Trails.prefab
  - Impact VFX: `Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Demon_LandingImpact.prefab` | type=ProjectOwnedWrapper | ready=True | source=Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Impacts/CFXR2 Ground Hit.prefab

**Timing:**
- Windup: 0.65s — Telegraph visible
- Active: 0.50s — Damage window, Active VFX plays
- Recovery: 1.00s — VFX cleanup

## Ghost

### AC_Ghost_PhaseHomingCurse (HomingProjectile)

- **Telegraph:** Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/PF_Telegraph_EnemyProjectile_Line.prefab
- **Active VFX:** Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Ghost_PhaseAura.prefab
- **Impact VFX:** Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Ghost_CurseImpact.prefab
- **Projectile:** Assets/_Project/Content/Combat/Projectiles/EnemyAttacks/PF_EnemyProjectile_Ghost_CurseOrb.prefab
- **Area Zone:** null

**VFX Asset Source:**
  - Telegraph: `Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/PF_Telegraph_EnemyProjectile_Line.prefab` | type=ProjectOwnedProcedural | ready=True | source=procedural
  - Active VFX: `Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Ghost_PhaseAura.prefab` | type=ProjectOwnedWrapper | ready=True | source=Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Magic Misc/CFXR3 Magic Aura A (Runic).prefab
  - Impact VFX: `Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Ghost_CurseImpact.prefab` | type=ProjectOwnedWrapper | ready=True | source=Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/Magic Hit 2.prefab
  - Projectile: `Assets/_Project/Content/Combat/Projectiles/EnemyAttacks/PF_EnemyProjectile_Ghost_CurseOrb.prefab` | VisualRoot=True | source=procedural

**Timing:**
- Windup: 0.70s — Telegraph visible
- Active: 0.10s — Damage window, Active VFX plays
- Recovery: 0.80s — VFX cleanup

## Mushroom

### AC_Mushroom_SporePoisonZone (SporeZone)

- **Telegraph:** Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/PF_Telegraph_Mushroom_SporeCircle.prefab
- **Active VFX:** Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Mushroom_SporeBurst.prefab
- **Impact VFX:** Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Mushroom_SporeCloud.prefab
- **Projectile:** null
- **Area Zone:** Assets/_Project/Content/Combat/AreaZones/EnemyAttacks/PF_EnemyArea_Mushroom_SporePoisonZone.prefab

**VFX Asset Source:**
  - Telegraph: `Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/PF_Telegraph_Mushroom_SporeCircle.prefab` | type=ProjectOwnedProcedural | ready=True | source=procedural
  - Active VFX: `Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Mushroom_SporeBurst.prefab` | type=ProjectOwnedWrapper | ready=True | source=Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Misc/CFXR2 Poison Cloud.prefab
  - Impact VFX: `Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Mushroom_SporeCloud.prefab` | type=ProjectOwnedWrapper | ready=True | source=Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Misc/CFXR2 Poison Cloud.prefab
  - AreaZone: `Assets/_Project/Content/Combat/AreaZones/EnemyAttacks/PF_EnemyArea_Mushroom_SporePoisonZone.prefab` | VisualRoot=True | source=procedural

**Timing:**
- Windup: 0.75s — Telegraph visible
- Active: 0.15s — Damage window, Active VFX plays
- Recovery: 0.65s — VFX cleanup

## Yeti

### AC_Yeti_FrostSlamShockwave (FrostSlamShockwave)

- **Telegraph:** Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/PF_Telegraph_Yeti_FrostCircle.prefab
- **Active VFX:** Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Yeti_FrostSlamImpact.prefab
- **Impact VFX:** Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Yeti_FrostShockwave.prefab
- **Projectile:** null
- **Area Zone:** Assets/_Project/Content/Combat/AreaZones/EnemyAttacks/PF_EnemyArea_Yeti_FrostSlowZone.prefab

**VFX Asset Source:**
  - Telegraph: `Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/PF_Telegraph_Yeti_FrostCircle.prefab` | type=ProjectOwnedProcedural | ready=True | source=procedural
  - Active VFX: `Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Yeti_FrostSlamImpact.prefab` | type=ProjectOwnedWrapper | ready=True | source=Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Ice/CFXR3 Hit Ice B (Air).prefab
  - Impact VFX: `Assets/_Project/Content/Enemies/AttackVFX/PF_VFX_Yeti_FrostShockwave.prefab` | type=ProjectOwnedWrapper | ready=True | source=Assets/GabrielAguiarProductions/FreeQuickEffectsVol1/Prefabs/vfx_Shockwave_01.prefab
  - AreaZone: `Assets/_Project/Content/Combat/AreaZones/EnemyAttacks/PF_EnemyArea_Yeti_FrostSlowZone.prefab` | VisualRoot=True | source=procedural

**Timing:**
- Windup: 1.00s — Telegraph visible
- Active: 0.20s — Damage window, Active VFX plays
- Recovery: 1.30s — VFX cleanup

