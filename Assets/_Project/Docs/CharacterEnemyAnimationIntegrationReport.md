# Character Enemy Animation Audit

## Scan Roots
- `Assets/_Project`
- `Assets/Assets/game asset packs`
- `Assets/ThirdParty`

## Asset Counts
- Model assets: 1060
- Project/gameplay prefabs: 11
- Animator Controllers: 9
- Animation Clips: 184
- Textures: 1390

## Selected Player Candidate
### Player Rogue
- Role: `MainPlayer`
- Score: A
- Visual asset: `Assets/Assets/game asset packs/RPG Characters - Nov 2020/Humanoid Rig Versions/FBX/Rogue.fbx`
- Generated prefab: `Assets/_Project/Prefabs/Player/PF_Player_Rogue_Generated.prefab`
- Requires ProjectileSpawnPoint: False
- Rationale: Agile humanoid silhouette, good dash-impact identity, visually distinct from monster enemies.
- Visual asset exists: True
- Base prefab exists: True

## Selected Enemy Candidates
### Basic Melee Green Demon
- Role: `BasicMelee`
- Score: A
- Visual asset: `Assets/Assets/game asset packs/Cute Animated Monsters - Aug 2020/FBX/GreenDemon.fbx`
- Generated prefab: `Assets/_Project/Prefabs/Enemies/Generated/PF_Enemy_BasicMelee_GreenDemon_Generated.prefab`
- Requires ProjectileSpawnPoint: False
- Rationale: Medium readable silhouette for frequent melee chaser waves.
- Visual asset exists: True
- Base prefab exists: True
### Fast Melee Alien
- Role: `FastMelee`
- Score: B
- Visual asset: `Assets/Assets/game asset packs/Cute Animated Monsters - Aug 2020/FBX/Alien.fbx`
- Generated prefab: `Assets/_Project/Prefabs/Enemies/Generated/PF_Enemy_FastMelee_Alien_Generated.prefab`
- Requires ProjectileSpawnPoint: False
- Rationale: Small, simple shape suitable for fast pressure enemy.
- Visual asset exists: True
- Base prefab exists: True
### Tank Cyclops
- Role: `Tank`
- Score: A
- Visual asset: `Assets/Assets/game asset packs/Cute Animated Monsters - Aug 2020/FBX/Cyclops.fbx`
- Generated prefab: `Assets/_Project/Prefabs/Enemies/Generated/PF_Enemy_Tank_Cyclops_Generated.prefab`
- Requires ProjectileSpawnPoint: False
- Rationale: Large body mass, clear tank role, good dash knockback readability.
- Visual asset exists: True
- Base prefab exists: True
### Ranged Ranger
- Role: `Ranged`
- Score: B
- Visual asset: `Assets/Assets/game asset packs/RPG Characters - Nov 2020/Humanoid Rig Versions/FBX/Ranger.fbx`
- Generated prefab: `Assets/_Project/Prefabs/Enemies/Generated/PF_Enemy_Ranged_Ranger_Generated.prefab`
- Requires ProjectileSpawnPoint: True
- Rationale: Bow-bearing humanoid communicates projectile role; behavior remains existing-safe until ranged AI pass.
- Visual asset exists: True
- Base prefab exists: True
### Charger Demon
- Role: `Charger`
- Score: B
- Visual asset: `Assets/Assets/game asset packs/Cute Animated Monsters - Aug 2020/FBX/Demon.fbx`
- Generated prefab: `Assets/_Project/Prefabs/Enemies/Generated/PF_Enemy_Charger_Demon_Generated.prefab`
- Requires ProjectileSpawnPoint: False
- Rationale: Aggressive forward-facing silhouette; charge behavior is deferred to existing/future AI.
- Visual asset exists: True
- Base prefab exists: True
### Caster Wizard
- Role: `Caster`
- Score: B
- Visual asset: `Assets/Assets/game asset packs/RPG Characters - Nov 2020/Humanoid Rig Versions/FBX/Wizard.fbx`
- Generated prefab: `Assets/_Project/Prefabs/Enemies/Generated/PF_Enemy_Caster_Wizard_Generated.prefab`
- Requires ProjectileSpawnPoint: True
- Rationale: Staff silhouette reads as caster; uses safe ranged/contact fallback until caster AI exists.
- Visual asset exists: True
- Base prefab exists: True
### Boss Candidate Yeti
- Role: `BossCandidate`
- Score: A
- Visual asset: `Assets/Assets/game asset packs/Cute Animated Monsters - Aug 2020/FBX/Yeti.fbx`
- Generated prefab: `Assets/_Project/Prefabs/Enemies/Generated/PF_Enemy_BossCandidate_Yeti_Generated.prefab`
- Requires ProjectileSpawnPoint: False
- Rationale: Large readable boss-candidate shape, strong mass for dash-impact showcase.
- Visual asset exists: True
- Base prefab exists: True

## Existing Gameplay Prefab Validation
### Player Generated
- Path: `Assets/_Project/Prefabs/Player/PF_Player_Rogue_Generated.prefab`
- Valid: True
- Issues: none
### Enemy Melee Chaser
- Path: `Assets/_Project/Prefabs/Enemies/PF_Enemy_MeleeChaser_Test.prefab`
- Valid: False
- missing_animator: Prefab needs an Animator on the root or visual child.
- missing_visual_root: Prefab should contain a VisualRoot child for model swapping.

## Animation Source Notes
- Character packs contain FBX model assets with `importAnimation` enabled, but no project-owned Animator Controllers were found for player/enemy gameplay yet.
- KayKit animation FBXs are available as animation sources, but clip compatibility must be validated in Unity before retargeting.
- This tool does not retarget or mutate source clips.

## Mobile Suitability Notes
- Selected assets are low-poly/stylized and use simple texture atlases, suitable for portrait mobile readability.
- Do not keep demo lights, cameras, particle children, or unnecessary helper objects inside generated gameplay prefabs.
- Validate material count and texture import sizes in Unity Inspector before production build.
