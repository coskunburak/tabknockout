# Cute Monster Enemy Content Builder Report

Generated assets are project-owned content under `Assets/_Project`. Source FBX, texture, OBJ, and `.meta` assets are not edited by this builder.

- Enemy projectile prefab: `Assets/_Project/Prefabs/Projectiles/PF_EnemyProjectile_CuteMonster.prefab`
## Green Demon
- Config: `Assets/_Project/ScriptableObjects/Enemies/CuteMonsters/EnemyConfig_GreenDemon.asset`
- Animator Controller: `Assets/_Project/Animation/Controllers/CuteMonsters/AC_PF_Enemy_GreenDemon.controller`
- Prefab: `Assets/_Project/Prefabs/Enemies/CuteMonsters/PF_Enemy_GreenDemon.prefab`
- Attack archetype: Basic contact chaser: readable melee/contact pressure
- VFX/feedback: normal enemy hit/death feedback
- Wave role: Early/mid baseline pressure
- Animation mapping: source clips 32; idle `Idle`, move `MonsterArmature|Walk`, attack `Bite_Front`, hit `HitRecieve`, death `Death`

## Demon
- Config: `Assets/_Project/ScriptableObjects/Enemies/CuteMonsters/EnemyConfig_Demon.asset`
- Animator Controller: `Assets/_Project/Animation/Controllers/CuteMonsters/AC_PF_Enemy_Demon.controller`
- Prefab: `Assets/_Project/Prefabs/Enemies/CuteMonsters/PF_Enemy_Demon.prefab`
- Attack archetype: Basic contact chaser: readable melee/contact pressure
- VFX/feedback: normal enemy hit/death feedback
- Wave role: Early/mid baseline pressure
- Animation mapping: source clips 32; idle `Idle`, move `MonsterArmature|Walk`, attack `Bite_Front`, hit `HitRecieve`, death `Death`

## Bat
- Config: `Assets/_Project/ScriptableObjects/Enemies/CuteMonsters/EnemyConfig_Bat.asset`
- Animator Controller: `Assets/_Project/Animation/Controllers/CuteMonsters/AC_PF_Enemy_Bat.controller`
- Prefab: `Assets/_Project/Prefabs/Enemies/CuteMonsters/PF_Enemy_Bat.prefab`
- Attack archetype: Fast swarm contact: quick low-damage pressure
- VFX/feedback: small frequent hit/death feedback
- Wave role: Early small doses, more frequent later
- Animation mapping: source clips 7; idle `Flying`, move `Flying`, attack `Bite_Front`, hit `HitRecieve`, death `Death`

## Bee
- Config: `Assets/_Project/ScriptableObjects/Enemies/CuteMonsters/EnemyConfig_Bee.asset`
- Animator Controller: `Assets/_Project/Animation/Controllers/CuteMonsters/AC_PF_Enemy_Bee.controller`
- Prefab: `Assets/_Project/Prefabs/Enemies/CuteMonsters/PF_Enemy_Bee.prefab`
- Attack archetype: Fast swarm contact: quick low-damage pressure
- VFX/feedback: small frequent hit/death feedback
- Wave role: Early small doses, more frequent later
- Animation mapping: source clips 7; idle `Flying`, move `Flying`, attack `Bite_Front`, hit `HitRecieve`, death `Death`

## Mushroom
- Config: `Assets/_Project/ScriptableObjects/Enemies/CuteMonsters/EnemyConfig_Mushroom.asset`
- Animator Controller: `Assets/_Project/Animation/Controllers/CuteMonsters/AC_PF_Enemy_Mushroom.controller`
- Prefab: `Assets/_Project/Prefabs/Enemies/CuteMonsters/PF_Enemy_Mushroom.prefab`
- Attack archetype: Basic contact chaser: readable melee/contact pressure
- VFX/feedback: normal enemy hit/death feedback
- Wave role: Early/mid baseline pressure
- Animation mapping: source clips 20; idle `Idle`, move `MonsterArmature|Walk`, attack `Bite_Front`, hit `HitRecieve`, death `Death`

## Cyclops
- Config: `Assets/_Project/ScriptableObjects/Enemies/CuteMonsters/EnemyConfig_Cyclops.asset`
- Animator Controller: `Assets/_Project/Animation/Controllers/CuteMonsters/AC_PF_Enemy_Cyclops.controller`
- Prefab: `Assets/_Project/Prefabs/Enemies/CuteMonsters/PF_Enemy_Cyclops.prefab`
- Attack archetype: Bruiser/tank: slower contact attack with telegraphed windup
- VFX/feedback: elite spawn/death semantic VFX with normal hit flash
- Wave role: Mid/late budget-heavy blocker
- Animation mapping: source clips 20; idle `Idle`, move `MonsterArmature|Walk`, attack `Bite_Front`, hit `HitRecieve`, death `Death`

## Yeti
- Config: `Assets/_Project/ScriptableObjects/Enemies/CuteMonsters/EnemyConfig_Yeti.asset`
- Animator Controller: `Assets/_Project/Animation/Controllers/CuteMonsters/AC_PF_Enemy_Yeti.controller`
- Prefab: `Assets/_Project/Prefabs/Enemies/CuteMonsters/PF_Enemy_Yeti.prefab`
- Attack archetype: Bruiser/tank: slower contact attack with telegraphed windup
- VFX/feedback: elite spawn/death semantic VFX with normal hit flash
- Wave role: Mid/late budget-heavy blocker
- Animation mapping: source clips 20; idle `Idle`, move `MonsterArmature|Walk`, attack `Bite_Front`, hit `HitRecieve`, death `Death`

## Cactus
- Config: `Assets/_Project/ScriptableObjects/Enemies/CuteMonsters/EnemyConfig_Cactus.asset`
- Animator Controller: `Assets/_Project/Animation/Controllers/CuteMonsters/AC_PF_Enemy_Cactus.controller`
- Prefab: `Assets/_Project/Prefabs/Enemies/CuteMonsters/PF_Enemy_Cactus.prefab`
- Attack archetype: Bruiser/tank: slower contact attack with telegraphed windup
- VFX/feedback: heavier elite/large death feedback when ranked elite
- Wave role: Mid/late budget-heavy blocker
- Animation mapping: source clips 20; idle `Idle`, move `MonsterArmature|Walk`, attack `Bite_Front`, hit `HitRecieve`, death `Death`

## Ghost
- Config: `Assets/_Project/ScriptableObjects/Enemies/CuteMonsters/EnemyConfig_Ghost.asset`
- Animator Controller: `Assets/_Project/Animation/Controllers/CuteMonsters/AC_PF_Enemy_Ghost.controller`
- Prefab: `Assets/_Project/Prefabs/Enemies/CuteMonsters/PF_Enemy_Ghost.prefab`
- Attack archetype: Fast swarm contact: quick low-damage pressure
- VFX/feedback: small frequent hit/death feedback
- Wave role: Early small doses, more frequent later
- Animation mapping: source clips 20; idle `Idle`, move `MonsterArmature|Walk`, attack `Bite_Front`, hit `HitRecieve`, death `Death`

## Cthulhu
- Config: `Assets/_Project/ScriptableObjects/Enemies/CuteMonsters/EnemyConfig_Cthulhu.asset`
- Animator Controller: `Assets/_Project/Animation/Controllers/CuteMonsters/AC_PF_Enemy_Cthulhu.controller`
- Prefab: `Assets/_Project/Prefabs/Enemies/CuteMonsters/PF_Enemy_Cthulhu.prefab`
- Attack archetype: Ranged special: projectile windup, line telegraph, pooled projectile
- VFX/feedback: elite spawn/death semantic VFX with normal hit flash
- Wave role: Mid/late ranged pressure, low weight, single-count
- Animation mapping: source clips 8; idle `Flying`, move `Flying`, attack `Bite_Front`, hit `HitRecieve`, death `Death`

## Yellow Dragon
- Config: `Assets/_Project/ScriptableObjects/Enemies/CuteMonsters/EnemyConfig_YellowDragon_Boss.asset`
- Animator Controller: `Assets/_Project/Animation/Controllers/CuteMonsters/AC_PF_Boss_YellowDragon.controller`
- Prefab: `Assets/_Project/Prefabs/Enemies/CuteMonsters/PF_Boss_YellowDragon.prefab`
- Attack archetype: Boss pattern: slam, charge, add-spawn, plus close-range contact fallback
- VFX/feedback: boss spawn/phase/heavy/death semantic VFX
- Wave role: Boss milestone via RunConfig bossSpawnGroup
- Animation mapping: source clips 8; idle `Flying`, move `Flying`, attack `Bite_Front`, hit `HitRecieve`, death `Death`

## Boss Pattern
- Pattern: `Assets/_Project/ScriptableObjects/Bosses/CuteMonsters/BossPattern_YellowDragon.asset`
- Boss config: `Assets/_Project/ScriptableObjects/Bosses/CuteMonsters/BossConfig_YellowDragon.asset`
- Boss prefab structure wired: `Assets/_Project/Prefabs/Enemies/CuteMonsters/PF_Boss_YellowDragon.prefab`
## Spawn Groups
- `Green Demon` -> `Assets/_Project/ScriptableObjects/Waves/CuteMonsters/SpawnGroup_Cute_GreenDemon.asset`
- `Demon` -> `Assets/_Project/ScriptableObjects/Waves/CuteMonsters/SpawnGroup_Cute_Demon.asset`
- `Bat` -> `Assets/_Project/ScriptableObjects/Waves/CuteMonsters/SpawnGroup_Cute_Bat.asset`
- `Bee` -> `Assets/_Project/ScriptableObjects/Waves/CuteMonsters/SpawnGroup_Cute_Bee.asset`
- `Mushroom` -> `Assets/_Project/ScriptableObjects/Waves/CuteMonsters/SpawnGroup_Cute_Mushroom.asset`
- `Cyclops` -> `Assets/_Project/ScriptableObjects/Waves/CuteMonsters/SpawnGroup_Cute_Cyclops.asset`
- `Yeti` -> `Assets/_Project/ScriptableObjects/Waves/CuteMonsters/SpawnGroup_Cute_Yeti.asset`
- `Cactus` -> `Assets/_Project/ScriptableObjects/Waves/CuteMonsters/SpawnGroup_Cute_Cactus.asset`
- `Ghost` -> `Assets/_Project/ScriptableObjects/Waves/CuteMonsters/SpawnGroup_Cute_Ghost.asset`
- `Cthulhu` -> `Assets/_Project/ScriptableObjects/Waves/CuteMonsters/SpawnGroup_Cute_Cthulhu.asset`
- `Yellow Dragon` -> `Assets/_Project/ScriptableObjects/Waves/CuteMonsters/SpawnGroup_Cute_YellowDragon_Boss.asset`

## Wave Timeline
- Timeline: `Assets/_Project/ScriptableObjects/Waves/CuteMonsters/WaveTimeline_CuteMonsters_Test.asset`
## Wave Timeline
- Timeline: `Assets/_Project/ScriptableObjects/Waves/WaveTimeline_DesktopSurvivorPrototype.asset`
- Wired `Assets/_Project/ScriptableObjects/Runs/RunConfig_DesktopSurvivorPrototype.asset` to cute monster timeline and YellowDragon boss group.
- Wired `Assets/_Project/ScriptableObjects/Runs/RunConfig_ForestSurvivorArena.asset` to cute monster timeline and YellowDragon boss group.
