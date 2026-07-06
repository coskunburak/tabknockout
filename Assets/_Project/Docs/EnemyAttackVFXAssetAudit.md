# Enemy Attack VFX Asset Audit

Generated as part of the Enemy Attack VFX Polish Pass.

---

## Folders Scanned

| Folder | Found VFX Assets |
|---|---|
| `Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/` | 20+ prefabs: slashes, hits, fireballs, magic |
| `Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/` | 10+ prefabs: fire/ice/magic/basic hits |
| `Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/` | 80+ prefabs: impacts, explosions, poison, ice, wind, water, magic |
| `Assets/GabrielAguiarProductions/FreeQuickEffectsVol1/Prefabs/` | 5 prefabs: shockwave, hyperdrive, sparks, speed rings |
| `Assets/_Project/Content/Combat/VFX/` | Project-owned attack VFX (created by builder) |
| `Assets/_Project/Content/Combat/Telegraphs/` | Project-owned telegraph prefabs (created by builder) |
| `Assets/_Project/Content/Combat/Projectiles/` | Project-owned projectile prefabs (created by builder) |
| `Assets/_Project/Content/Combat/AreaZones/` | Project-owned area zone prefabs (created by builder) |

---

## Third-Party Source Assets Used (Read-Only — NOT Modified)

These assets are used as source child references inside project-owned wrapper prefabs.
The originals are **never modified**.

### Eric VFX Studio — Free Game VFX

| Asset Path | Asset Type | Used For | Enemy |
|---|---|---|---|
| `Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Orange_Slash_1.prefab` | Prefab (Particle) | Melee slash arc visual base | GreenDemon |
| `Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Green_Hit.prefab` | Prefab (Particle) | Green impact burst | GreenDemon, Cactus, Cthulhu |
| `Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Purple_Hit_02.prefab` | Prefab (Particle) | Purple impact burst | Bat |
| `Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Fireball.prefab` | Prefab (Particle) | Fireball projectile visual / muzzle | YellowDragon |

### Matthew Guz — Hits Effects FREE

| Asset Path | Asset Type | Used For | Enemy |
|---|---|---|---|
| `Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/Fire Hit .prefab` | Prefab (Particle) | Fire impact burst | YellowDragon, Cyclops |
| `Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/Magic Hit 2.prefab` | Prefab (Particle) | Magic/curse impact | Ghost |
| `Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/Ice Hit .prefab` | Prefab (Particle) | Ice/frost impact | Yeti |

### JMO Assets — Cartoon FX Remaster

| Asset Path | Asset Type | Used For | Enemy |
|---|---|---|---|
| `Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Impacts/CFXR Hit D 3D (Yellow).prefab` | Prefab (Particle) | Yellow spark impact | Bee sting |
| `Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Impacts/CFXR2 Ground Hit.prefab` | Prefab (Particle) | Ground landing impact | Demon leap landing |
| `Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Misc/CFXR2 Poison Cloud.prefab` | Prefab (Particle) | Poison/spore cloud | Mushroom |
| `Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Nature/CFXR4 Wind Trails.prefab` | Prefab (Particle) | Motion trail base | Bat dive, Demon leap |
| `Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Liquids/CFXR Water Splash (Smaller).prefab` | Prefab (Particle) | Liquid splash / slime splash | Cthulhu |
| `Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Magic Misc/CFXR3 Magic Aura A (Runic).prefab` | Prefab (Particle) | Phase aura / curse aura | Ghost |
| `Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Ice/CFXR3 Hit Ice B (Air).prefab` | Prefab (Particle) | Frost slam impact | Yeti |

### Gabriel Aguiar Productions — Free Quick Effects Vol.1

| Asset Path | Asset Type | Used For | Enemy |
|---|---|---|---|
| `Assets/GabrielAguiarProductions/FreeQuickEffectsVol1/Prefabs/vfx_Shockwave_01.prefab` | Prefab (Particle) | Shockwave ring burst | Yeti frost shockwave |
| `Assets/GabrielAguiarProductions/FreeQuickEffectsVol1/Prefabs/vfx_Hyperdrive_01.prefab` | Prefab (Particle) | Speed streak / charge trail base | Bee charge trail |
| `Assets/GabrielAguiarProductions/FreeQuickEffectsVol1/Prefabs/vfx_Sparks_01.prefab` | Prefab (Particle) | Sparks burst | Cyclops beam, Cactus radial |

---

## Project-Owned VFX Prefabs Created by Builder

All created under `Assets/_Project/Content/`. These are **not** third-party assets.

### Telegraph Prefabs

Location: `Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks/`

| Prefab Name | Shape | Color | Enemy |
|---|---|---|---|
| `PF_Telegraph_GreenDemon_MeleeArc.prefab` | Arc LineRenderer | Green #73FF2E | GreenDemon melee |
| `PF_Telegraph_Bat_DiveLine.prefab` | Line LineRenderer | Purple #B852FF | Bat dive path |
| `PF_Telegraph_Bee_ChargeLine.prefab` | Line LineRenderer | Yellow #FFE114 | Bee charge path |
| `PF_Telegraph_Cactus_RadialCircle.prefab` | Circle LineRenderer | Green #52FF2E | Cactus radial burst |
| `PF_Telegraph_Cyclops_BeamLine.prefab` | Line LineRenderer | Red #FF2E0F | Cyclops eye beam |
| `PF_Telegraph_Demon_LandingCircle.prefab` | Circle LineRenderer | Red #FF1F0D | Demon landing zone |
| `PF_Telegraph_Mushroom_SporeCircle.prefab` | Circle LineRenderer | Purple #AD28FF | Mushroom spore zone |
| `PF_Telegraph_Yeti_FrostCircle.prefab` | Circle LineRenderer | Blue #5CDBFF | Yeti frost slam zone |
| `PF_Telegraph_EnemyProjectile_Line.prefab` | Line LineRenderer | Orange #FFA81F | YellowDragon, Cactus spike, Cthulhu, Ghost |

All telegraph prefabs use LineRenderer + ParticleSystem edge sparks. No primitive meshes.

### Active / Impact VFX Prefabs

Location: `Assets/_Project/Content/Enemies/AttackVFX/`

| Prefab Name | Style | Color | Source Ref | Enemy |
|---|---|---|---|---|
| `PF_VFX_GreenDemon_MeleeSlash.prefab` | Slash arc LineRenderer + sparks | Green/yellow | FX_Orange_Slash_1 | GreenDemon active |
| `PF_VFX_GreenDemon_HitImpact.prefab` | Burst particles + ring | Green | FX_Green_Hit | GreenDemon impact |
| `PF_VFX_Bat_DiveTrail.prefab` | TrailRenderer + particles | Purple | CFXR4 Wind Trails | Bat active trail |
| `PF_VFX_Bat_DiveImpact.prefab` | Burst | Purple | FX_Purple_Hit_02 | Bat impact |
| `PF_VFX_Bee_ChargeTrail.prefab` | TrailRenderer + particles | Yellow | vfx_Hyperdrive_01 | Bee active trail |
| `PF_VFX_Bee_StingImpact.prefab` | Burst | Yellow | CFXR Hit D 3D Yellow | Bee impact |
| `PF_VFX_YellowDragon_FireMuzzle.prefab` | Muzzle burst + ring | Orange | FX_Fireball | YellowDragon active |
| `PF_VFX_Fireball_Impact.prefab` | Burst | Orange | Fire Hit | YellowDragon impact |
| `PF_VFX_Cactus_SpikeCast.prefab` | Muzzle burst + ring | Green | FX_Green_Hit | Cactus spike active |
| `PF_VFX_Cactus_SpikeImpact.prefab` | Burst | Green | FX_Green_Hit | Cactus impact |
| `PF_VFX_Cactus_RadialSpikeBurst.prefab` | Circle ring + radial particles | Yellow-green | vfx_Sparks_01 | Cactus radial active |
| `PF_VFX_Cthulhu_SlimeOrbCast.prefab` | Muzzle burst + ring | Toxic green | FX_Green_Hit | Cthulhu active |
| `PF_VFX_Cthulhu_SlimeSplash.prefab` | Splash droplets + ring | Toxic green | CFXR Water Splash | Cthulhu impact |
| `PF_VFX_Cyclops_EyeBeam.prefab` | Beam LineRenderer + trail + sparks | Red | vfx_Sparks_01 | Cyclops active |
| `PF_VFX_Cyclops_BeamImpact.prefab` | Burst | Red/orange | Fire Hit | Cyclops impact |
| `PF_VFX_Demon_LeapTrail.prefab` | TrailRenderer + particles | Red | CFXR4 Wind Trails | Demon active trail |
| `PF_VFX_Demon_LandingImpact.prefab` | Shockwave ring + burst | Red | CFXR2 Ground Hit | Demon impact |
| `PF_VFX_Ghost_PhaseAura.prefab` | Aura particles + circle ring | Purple | CFXR3 Magic Aura A | Ghost active |
| `PF_VFX_Ghost_CurseImpact.prefab` | Burst | Purple | Magic Hit 2 | Ghost impact |
| `PF_VFX_Mushroom_SporeBurst.prefab` | Cloud particles + ring | Purple | CFXR2 Poison Cloud | Mushroom active |
| `PF_VFX_Mushroom_SporeCloud.prefab` | Cloud particles + ring | Purple | CFXR2 Poison Cloud | Mushroom impact |
| `PF_VFX_Yeti_FrostSlamImpact.prefab` | Shockwave ring + burst | Blue/white | CFXR3 Hit Ice B | Yeti active |
| `PF_VFX_Yeti_FrostShockwave.prefab` | Shockwave ring + burst | Blue/white | vfx_Shockwave_01 | Yeti impact |

### Projectile Prefabs

Location: `Assets/_Project/Content/Combat/Projectiles/EnemyAttacks/`

| Prefab Name | Visual Style | Color | VisualRoot Contents | Enemy |
|---|---|---|---|---|
| `PF_EnemyProjectile_YellowDragon_Fireball.prefab` | Fireball | Orange | Particles + TrailRenderer + FX_Fireball | YellowDragon |
| `PF_EnemyProjectile_Cactus_Spike.prefab` | Spike | Green | LineRenderer dart + TrailRenderer + dust particles | Cactus |
| `PF_EnemyProjectile_Cthulhu_SlimeOrb.prefab` | Slime | Toxic green | Slime core particles + TrailRenderer | Cthulhu |
| `PF_EnemyProjectile_Ghost_CurseOrb.prefab` | Curse | Purple | Particles + TrailRenderer + halo LineRenderer | Ghost |

All projectile prefabs: physics root has no visible renderer. All visuals are on the VisualRoot child.

### Area Zone Prefabs

Location: `Assets/_Project/Content/Combat/AreaZones/EnemyAttacks/`

| Prefab Name | Visual Contents | Color | Enemy |
|---|---|---|---|
| `PF_EnemyArea_Cthulhu_SlowPool.prefab` | Circle LineRenderer + slime bubble particles | Toxic green | Cthulhu slow pool |
| `PF_EnemyArea_Mushroom_SporePoisonZone.prefab` | Circle LineRenderer + spore mote particles | Purple | Mushroom spore zone |
| `PF_EnemyArea_Yeti_FrostSlowZone.prefab` | Circle LineRenderer + frost mote particles | Blue/white | Yeti frost slow zone |

All area zone prefabs: EnemyAreaZone behavior on root; all visuals on VisualRoot child. No primitive MeshRenderer on roots.

---

## Assets Considered But Not Used

| Asset | Reason Not Used |
|---|---|
| CFXR prefabs with CFXR_DEMO_ prefix | Demo-only, single-frame non-loop |
| CFXR Fire prefabs | Scale too large for compact fireball projectile |
| Polygon/Synty assets | Not present in project |
| Eric VFX blood/sword-trail prefabs | Not visually appropriate for stylized monster attacks |

---

## Notes on Asset Usage Rules

1. No third-party source assets were modified. All originals under `Assets/ThirdParty/` and `Assets/GabrielAguiarProductions/` are read-only.
2. Source prefabs are instantiated as children of project-owned wrapper prefabs at build time. The wrapper lives under `Assets/_Project/`.
3. Colliders from source VFX prefabs are stripped at build time (StripCollidersFromHierarchy) to prevent accidental hit detection.
4. All project-owned VFX carry an EnemyAttackVFXMarker with productionReady=true and placeholder=false.
5. All timed VFX carry an EnemyAttackVFXAutoCleanup component that auto-destroys after the configured lifetime.
