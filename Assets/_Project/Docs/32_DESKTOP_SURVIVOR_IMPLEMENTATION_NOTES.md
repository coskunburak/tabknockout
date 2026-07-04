# Desktop Survivor Implementation Notes

## Status

This is the first prototype foundation pass for the desktop survivor pivot. It adds an isolated survivor runtime path without deleting or replacing the legacy room/chapter flow.

## Implemented

- Desktop input reader for WASD, dash, active skill hotkeys, pause, and primary fire state.
- Mouse aim projection from screen cursor to ground plane or physics ground hit.
- Isometric/top-down survivor camera follow rig.
- Survivor config ScriptableObjects:
  - `RunConfig`
  - `ArenaConfig`
  - `WaveTimelineConfig`
  - `SpawnGroupConfig`
- `ArenaRunDirector` with run state, timer, level-up pause hooks, boss milestone placeholder, result summary, and player death handling.
- `SurvivorSpawnDirector` with player-centered spawn ring, safety radius, live cap, weighted spawn groups, and lightweight enemy pooling.
- XP prototype loop:
  - enemy death -> XP reward
  - optional XP orb spawn
  - `PickupCollector`
  - `PlayerXPController`
  - level-up event -> existing `AbilitySelectionController`
- Prototype HUD scripts for timer, XP, level, HP, dash cooldown, live enemies, boss warning, active skill slots, and result screen.
- Editor builder menu:
  - `Tap Knockout/Survivor/Create Desktop Survivor Prototype Scene`

## Reused Systems

- `AbilityDefinition`
- `AbilityChoiceProvider`
- `AbilitySelectionController`
- `PlayerAbilityEffectApplier`
- `PlayerMovementController`
- `PlayerDashController`
- `PlayerHealth`
- `EnemyConfig`
- `EnemyController`
- `EnemyHealth`
- shared combat events and hit data

## Legacy Systems Left Untouched

- `ChapterRunner`
- `RoomManager`
- legacy `WaveManager`
- legacy fixed `EnemySpawner`
- room templates and chapter configs
- `SampleScene` room/chapter wiring

These remain available for legacy tests or future challenge-mode migration, but they are not the canonical desktop survivor run path.

## How To Create The Prototype Scene

1. Open the project in Unity.
2. Run `Tap Knockout/Survivor/Create Desktop Survivor Prototype Scene`.
3. Open `Assets/_Project/Scenes/DesktopSurvivorPrototype.unity`.
4. Inspect `ArenaRunDirector`, `SurvivorSpawnDirector`, and generated configs.
5. Assign spawn groups with explicit `EnemyConfig` and enemy prefab references.
6. Assign an `XPOrb` prefab if orb collection should be tested. If no orb prefab is assigned, XP is granted directly as a prototype fallback.
7. Assign an ability pool on `RunConfig` or `AbilitySelectionController`.

## Required Manual Assignments

- `WaveTimelineConfig` entries and spawn groups.
- `SpawnGroupConfig` assets for the specific enemy prefabs to test.
- Boss spawn group if boss milestone testing is desired.
- XP orb prefab if physical pickup behavior is desired.
- HUD text/slider/image references if the generated placeholder canvas is expanded.
- Ability card UI references if using the existing level-up modal.

## Known TODOs

- Primary attack is still the existing stop-to-attack implementation unless separately migrated.
- Active skill hotkeys are captured, but active skill runtime casting is still a TODO.
- Boss milestone can request a boss spawn group, but full boss encounter direction remains future work.
- Enemy pooling is intentionally lightweight and should be hardened before 100+ enemy stress tests.
- Projectile pooling is not migrated in this pass.
- Generated scene is a prototype scaffold, not a production arena.

## Validation Checklist

- Unity compile has no errors.
- `DesktopSurvivorPrototype` scene can be created from the menu.
- Player prefab receives `DesktopInputReader`, `MouseAimController`, `PlayerXPController`, and `PickupCollector`.
- `SurvivorSpawnDirector` spawns only configured survivor spawn groups, not room spawn points.
- Killing an enemy grants XP or spawns an XP orb.
- Level-up calls the existing ability offer flow.
- Player death ends the run as defeat.
- Run duration completion ends as victory.
