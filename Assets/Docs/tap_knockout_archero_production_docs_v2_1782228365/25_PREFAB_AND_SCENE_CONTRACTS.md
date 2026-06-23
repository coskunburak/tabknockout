# Prefab and Scene Contracts

## Scene Policy

- Do not directly edit `.unity` YAML.
- Use Unity manual setup or approved Editor scripts.
- Keep scene responsibilities narrow.
- Runtime systems should be recoverable from prefabs/configs rather than hidden scene-only state.

## Target Scenes

### Boot

Responsibilities:

- Initialize service bindings.
- Load local configs.
- Load save data.
- Route to Home or Gameplay.

Required root objects:

- `BootRoot`
- `ServiceInstaller`
- `ConfigLoader`
- `SaveBootstrap`
- `SceneFlowController`

Manual Unity setup:

1. Create `Assets/_Project/Scenes/Boot.unity`.
2. Add root GameObject `BootRoot`.
3. Attach bootstrap/service scripts after implementation.
4. Add to build settings before Home and Gameplay.

### Home

Responsibilities:

- Chapter select.
- Gear placeholder.
- Talents placeholder.
- Shop placeholder.
- Daily rewards and missions placeholders.
- Settings.

Required root objects:

- `HomeRoot`
- `HomeCanvas`
- `HomeController`
- `ChapterSelectPanel`
- `MetaPanelRoot`

Manual Unity setup:

1. Create `Assets/_Project/Scenes/Home.unity`.
2. Add portrait canvas using safe-area container.
3. Hook buttons to controller after UI scripts exist.

### Gameplay

Responsibilities:

- Runtime chapter.
- Player.
- Room/wave/chapter managers.
- Camera.
- HUD.
- Ability selection.
- Run result.

Required root objects:

- `GameplayRoot`
- `Managers`
- `RoomRoot`
- `PlayerSpawn`
- `EnemySpawnPoints`
- `GameplayCamera`
- `GameplayCanvas`

Manual Unity setup:

1. Create `Assets/_Project/Scenes/Gameplay.unity`.
2. Add root hierarchy.
3. Use placeholder arena and spawn points.
4. Assign configs and prefabs through Inspector.
5. Do not hand-edit YAML.

## Player Prefab Contract

Required components:

- Player movement controller
- Player attack controller
- Player dash controller
- Player health
- Player stats
- Collider
- Rigidbody or CharacterController, depending on selected movement implementation
- Visual root
- Optional animation controller

Required child objects:

- `VisualRoot`
- `DashHitVolume` or configured hit query origin
- `ProjectileSpawnPoint`
- `TargetingOrigin`

Required references:

- `PlayerConfig`
- Initial `WeaponConfig`
- HUD target hooks

Acceptance:

- Can be spawned into a gameplay scene and controlled without scene-specific code.
- Can be used with placeholder capsule before final model.

## Enemy Prefab Contract

Required components:

- Enemy controller
- Enemy health
- Enemy movement/behavior component
- Collider
- Rigidbody or NavMeshAgent, depending on movement approach
- Visual root

Required child objects:

- `VisualRoot`
- `AttackOrigin`
- `HitReactionRoot`
- Optional `ProjectileSpawnPoint`
- Optional `TelegraphRoot`

Required references:

- `EnemyConfig`
- Projectile prefab for ranged enemies
- VFX/SFX event hooks optional

Acceptance:

- Can be spawned by EnemySpawner from config.
- Death event releases room/wave progress.
- Dash impact can knock back or interrupt where allowed.

## Boss Prefab Contract

Required components:

- Boss controller
- Enemy health or boss health wrapper using shared damage flow
- Attack pattern controller
- Boss event broadcaster
- Collider

Required child objects:

- `VisualRoot`
- `SlamTelegraph`
- `ChargeTelegraph`
- `AddSpawnPoints`
- `BossHitRoot`

Acceptance:

- Boss HP bar can bind to health.
- Boss can enter, attack, die, and notify room clear.

## Projectile Prefab Contract

Required components:

- Projectile controller
- Collider configured for trigger or collision policy
- Pool member component if pooling implementation uses one

Required fields:

- Speed
- Lifetime
- Damage data source
- Hit layer mask
- Max pierce count
- Impact event hook

Acceptance:

- Projectiles return to pool on hit, lifetime end, or room cleanup.
- Projectiles do not damage the same target repeatedly unless config allows it.

## Room Prefab Contract

Required root:

- `RoomRoot`

Required children:

- `Arena`
- `PlayerSpawn`
- `EnemySpawnPoints`
- `ExitGate`
- `CameraBounds`
- `HazardRoot` optional
- `RewardSpawnRoot` optional

Acceptance:

- RoomManager can query spawn points and bounds.
- Room works with portrait camera.
- No room data is only implicit in object names.

## UI Prefab Contracts

Gameplay HUD:

- HP bar
- XP/level bar
- Dash cooldown button
- Ability icon row
- Pause button
- Boss HP bar
- Reward feedback area

Ability choice panel:

- Three card slots
- Icon, title, short description, rarity styling
- Select button or card click
- Optional reroll placeholder disabled by default

Run result:

- Win/loss title
- Rooms cleared
- Rewards
- Retry
- Home

Acceptance:

- Safe area container wraps interactive UI.
- UI supports 1080x1920 reference resolution.
- Text fits without clipping at common mobile aspect ratios.

## Editor Builder Expectations

The future placeholder scene builder should:

- Create `GameplayRoot`, `Managers`, `RoomRoot`, player spawn, spawn points, camera, light, and placeholder HUD.
- Save a new scene under `Assets/_Project/Scenes` only after user approval.
- Avoid touching production scenes unless explicitly requested.
- Be repeatable or create uniquely named test scenes.

