# Prefab and Scene Contracts

## Scene Policy

- Do not directly edit `.unity` YAML.
- Use Unity manual setup or approved Editor scripts.
- Keep scene responsibilities narrow.
- Runtime systems should recover from prefabs/configs rather than hidden scene-only state.

## Target Scene: DesktopSurvivorPrototype

Responsibilities:

- Load one arena.
- Spawn player.
- Run survivor timer.
- Spawn waves and milestones.
- Handle XP, level-up, boss, result.
- Provide desktop HUD.

Required root objects:

- `DesktopSurvivorPrototypeRoot`
- `Managers`
- `ArenaRoot`
- `PlayerSpawn`
- `CameraRig`
- `GameplayCanvas`
- `LightingRoot`
- `DebugRoot` optional

Required manager objects or components:

- `ArenaRunDirector`
- `SpawnDirector`
- `WaveDirector`
- `LevelUpSelectionController`
- `AbilityRuntimeController`
- `BossEncounterDirector`
- `PoolRoot`

Manual Unity setup:

1. Create `Assets/_Project/Scenes/DesktopSurvivorPrototype.unity`.
2. Add the root hierarchy above.
3. Assign `ArenaConfig`, `RunConfig`, wave timeline, player prefab, enemy prefabs, boss prefab, and UI prefabs through Inspector.
4. Do not hand-edit YAML.

## Player Prefab Contract

Required components:

- Desktop movement controller.
- Mouse aim controller or aim receiver.
- Player health.
- Player stats.
- Primary attack controller.
- Dash/evade controller.
- Ability runtime receiver.
- Collider.
- Rigidbody or CharacterController, depending on movement implementation.
- Visual root.

Required child objects:

- `VisualRoot`
- `AimOrigin`
- `ProjectileSpawnPoint`
- `DashHitOrigin` if using overlap query
- `PickupMagnetOrigin`

Required references:

- Player config.
- Starting weapon/skill config.
- HUD binding hooks.

## Camera Rig Contract

Required behavior:

- Follow player.
- Use isometric/top-down angle.
- Clamp to arena bounds.
- Support zoom tuning.
- Expose screen shake hook.
- Preserve mouse aim world projection.

## Arena Root Contract

Required children or data:

- Playable bounds.
- Camera bounds.
- Visual environment root.
- Collision root.
- Pickup bounds.
- Boss spawn anchor optional.
- Spawn ring helper optional.

## Spawn Logic Contract

The canonical spawn model is data-driven spawn rings and budgets. Hand-placed spawn points are optional helper anchors, not required for every enemy.

Required rules:

- Do not spawn inside player safety radius.
- Respect arena bounds.
- Respect live enemy budget.
- Support elite and boss overrides.

## Enemy Prefab Contract

Required components:

- Enemy controller.
- Enemy health.
- Enemy movement/behavior component.
- Collider.
- Pool member.
- Visual root.

Required child objects:

- `VisualRoot`
- `AttackOrigin`
- `HitReactionRoot`
- Optional `ProjectileSpawnPoint`
- Optional `TelegraphRoot`

Acceptance:

- Can be spawned/despawned by pool.
- Death drops XP or reward event.
- Uses shared damage flow.
- Does not require room-specific object names.

## Boss Prefab Contract

Required components:

- Boss controller.
- Health using shared damage flow.
- Attack pattern controller.
- Boss event broadcaster.
- Collider.
- Pool member or explicit lifecycle policy.

Required child objects:

- `VisualRoot`
- `TelegraphRoot`
- `ProjectileSpawnPoints` optional.
- `AddSpawnAnchors` optional.

Acceptance:

- Boss HP bar can bind to health.
- Boss emits spawn, phase, and defeated events.
- Boss attacks are readable from gameplay camera.

## Projectile Prefab Contract

Required components:

- Projectile controller.
- Collider configured for trigger/collision policy.
- Pool member.

Required fields:

- Speed.
- Lifetime.
- Damage source.
- Hit layer mask.
- Pierce count.
- Impact hook.

## Pickup and XP Orb Contract

Required components:

- Pickup or XP orb controller.
- Collider/trigger.
- Pool member.
- Magnet/attraction behavior if enabled.

Required fields:

- Value.
- Lifetime.
- Collection radius.
- Magnet radius.
- Visual feedback.

## UI HUD Contract

Required elements:

- Health bar.
- XP bar.
- Level text.
- Run timer.
- Dash cooldown.
- Active skill slots.
- Boss health bar.
- Wave/elite/boss warning.
- Pause/menu input.

## Level-Up Modal Contract

Required elements:

- Three choice slots by default.
- Icon.
- Ability name.
- Category/rarity.
- Short description.
- Stack/current level.
- Keyboard and mouse selection.

Acceptance:

- Gameplay pause/resume is safe.
- Text fits common desktop resolutions.
- Selection applies before combat resumes.

## Legacy Room Contract

Old room prefab contracts are legacy/future optional. They may be reused for challenge arenas only after they are adapted to the arena survivor run model.
