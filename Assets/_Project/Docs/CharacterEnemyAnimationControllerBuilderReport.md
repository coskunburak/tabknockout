# Character Enemy Animation Controller Builder Report

Project-owned Animator Controllers are generated under `Assets/_Project/Animation/Controllers`.
Source model contents and ThirdParty assets are not modified; selected model import settings may be prepared for Generic same-asset animation clips.

## Import Settings Preparation
- Importer already ready for Generic Avatar clips: `Assets/Assets/game asset packs/RPG Characters - Nov 2020/FBX/Ranger.fbx`
- Importer already ready for Generic Avatar clips: `Assets/Assets/game asset packs/Cute Animated Monsters - Aug 2020/FBX/GreenDemon.fbx`

## Clip Scan
- Candidate animation clips: 1648

## Player Ranger
- Role: `MainPlayer`
- Generated prefab: `Assets/_Project/Prefabs/Player/PF_Player_Rogue_Generated.prefab`
- Animator Controller: `Assets/_Project/Animation/Controllers/AC_Player_Rogue.controller`
- Prefab wiring: applied
- Idle: `CharacterArmature|Idle_Weapon` from `Assets/Assets/game asset packs/RPG Characters - Nov 2020/FBX/Ranger.fbx`
- Move: `CharacterArmature|Run_Holding` from `Assets/Assets/game asset packs/RPG Characters - Nov 2020/FBX/Ranger.fbx`
- Attack: `CharacterArmature|Bow_Attack_Shoot` from `Assets/Assets/game asset packs/RPG Characters - Nov 2020/FBX/Ranger.fbx`
- Dash: `CharacterArmature|Roll` from `Assets/Assets/game asset packs/RPG Characters - Nov 2020/FBX/Ranger.fbx`
- Hit: `CharacterArmature|RecieveHit` from `Assets/Assets/game asset packs/RPG Characters - Nov 2020/FBX/Ranger.fbx`
- Death: `CharacterArmature|Death` from `Assets/Assets/game asset packs/RPG Characters - Nov 2020/FBX/Ranger.fbx`

## Basic Melee Green Demon
- Role: `BasicMelee`
- Generated prefab: `Assets/_Project/Prefabs/Enemies/Generated/PF_Enemy_BasicMelee_GreenDemon_Generated.prefab`
- Animator Controller: `Assets/_Project/Animation/Controllers/AC_Enemy_BasicMelee.controller`
- Prefab wiring: applied
- Idle: `Idle` from `Assets/Assets/game asset packs/Cute Animated Monsters - Aug 2020/FBX/GreenDemon.fbx`
- Move: `MonsterArmature|Walk` from `Assets/Assets/game asset packs/Cute Animated Monsters - Aug 2020/FBX/GreenDemon.fbx`
- Attack: `Bite_Front` from `Assets/Assets/game asset packs/Cute Animated Monsters - Aug 2020/FBX/GreenDemon.fbx`
- Dash: `Jump` from `Assets/Assets/game asset packs/Cute Animated Monsters - Aug 2020/FBX/GreenDemon.fbx`
- Hit: `HitRecieve` from `Assets/Assets/game asset packs/Cute Animated Monsters - Aug 2020/FBX/GreenDemon.fbx`
- Death: `Death` from `Assets/Assets/game asset packs/Cute Animated Monsters - Aug 2020/FBX/GreenDemon.fbx`

