# Forest Survivor Arena Notes

## Status

This pass adds a second desktop survivor arena path. It does not replace the existing `DesktopSurvivorPrototype` scene.

## Asset Pack Used

- Imported pack: `Assets/Assets/game asset packs/KayKit_Forest_Nature_Pack_1 1.0_FREE`
- Runtime source folder used by the builder: `Assets/Assets/game asset packs/KayKit_Forest_Nature_Pack_1 1.0_FREE/Assets/fbx(unity)`
- The pack is imported as Unity FBX model assets, not authored `.prefab` files.

## Created Locations

- Scene: `Assets/_Project/Scenes/DesktopSurvivorPrototype_ForestArena.unity`
- Arena prefab: `Assets/_Project/Prefabs/Arena/ForestSurvivorArena.prefab`
- Arena config: `Assets/_Project/ScriptableObjects/Arenas/ArenaConfig_ForestSurvivorArena.asset`
- Run config: `Assets/_Project/ScriptableObjects/Runs/RunConfig_ForestSurvivorArena.asset`
- Builder and validation menu: `Assets/_Project/Editor/ForestSurvivorArenaBuilder.cs`

## Arena Design Goals

- Large open central combat clearing for WASD movement, dash, mouse aim, and projectile combat.
- Dense visual forest ring around the outside without cluttering the core combat space.
- Edge-pressure spawn support using the existing radius-based survivor spawn system.
- Visual landmarks that help orientation from the survivor camera angle.
- Simple physics blockers instead of many detailed prop colliders.
- Ground and telegraph readability over decorative density.

## KayKit Forest Nature Inventory

The imported forest pack currently contains 105 FBX model assets:

- Bushes: 22
- Grass variants: 20
- Rocks: 43
- Trees: 20, including bare tree variants

Not present in this imported pack: logs, stumps, flowers, mushrooms, terrain tiles, water pieces, fences, bridges, or ruins. Landmarks therefore use composed rock, tree, bush, and grass clusters.

## Layout

- `ForestArena_Ground`: one large walkable ground collider, a central clearing decal, and broad dirt path decals.
- `ForestArena_Borders`: dense outer tree line plus inner bush band to frame the playable space.
- `ForestArena_Decor`: low-density grass and non-blocking edge rocks, kept out of the main aiming lanes where practical.
- `ForestArena_Landmarks`: boss clearing, rock gate, bare tree circle, bush grove, and meadow clusters.
- `ForestArena_SpawnZones`: eight named edge helper anchors plus a north-east boss/elite helper.
- `ForestArena_Blockers`: simple invisible box colliders around the perimeter and a few landmark blockers.
- `ForestArena_Lighting`: one soft directional key light, limited fill lights, and light fog.

## Spawn Zone Strategy

The runtime spawn system is config-driven, not hand-spawn-point-driven. The forest arena uses:

- `SpawnPressureMode.Mixed`
- High edge pressure chance
- Larger player avoid radius
- Larger arena radius
- More spawn retries
- Spawn blocker layers pointed at simple invisible blockers
- Ground snapping limited to the walkable ground layer

The named spawn-zone objects are layout and QA anchors for designers. They do not replace the radius-based `SurvivorSpawnDirector`.

## Collision And Layer Strategy

- Walkable ground uses the `Default` layer and a single broad BoxCollider.
- Decorative KayKit model instances have visual colliders removed by the builder.
- Boundary and landmark blockers use simple BoxColliders on layer 2, `Ignore Raycast`.
- `ArenaConfig_ForestSurvivorArena.spawnBlockerLayers` points to layer 2 so spawn validation ignores the ground but rejects blocker overlap.
- `SurvivorSpawnDirector.spawnGroundLayers` points to `Default`, so spawn telegraphs and enemy grounding resolve against the forest floor instead of blockers.

This avoids dense collision clutter while still blocking the arena edge.

## Lighting And Camera Notes

- The forest scene uses the existing `SurvivorCameraRig`.
- The builder increases orthographic size slightly for the larger arena.
- Tall trees are placed beyond the central combat radius to reduce camera occlusion.
- Fog is light and intended for background atmosphere only.
- Player, enemies, reticle, and spawn telegraphs should remain readable in the clearing.

## Validator And Repair

- `Tap Knockout > Survivor > Build Forest Survivor Arena Scene` creates or rebuilds the forest scene, prefab, arena config, and run config.
- `Tap Knockout > Survivor > Validate Forest Arena Scene` checks the forest root, required hierarchy groups, ground collider, blockers, spawn helpers, config, and core survivor directors.
- `Tap Knockout > Survivor > Validate Prototype Scene` now also runs forest-specific checks when the forest scene or `ForestSurvivorArena` root is active.
- `Tap Knockout > Survivor > Repair Prototype Scene` remains the safe runtime wiring pass for the active survivor scene.

## Manual Unity Assignments

After building the scene, inspect:

- `ArenaRunDirector.runConfig` should point to `RunConfig_ForestSurvivorArena`.
- `ArenaRunDirector.arenaConfigOverride` should point to `ArenaConfig_ForestSurvivorArena`.
- `SurvivorSpawnDirector.arenaConfig` should point to `ArenaConfig_ForestSurvivorArena`.
- `SurvivorSpawnDirector.spawnGroundLayers` should be `Default`.
- `ArenaConfig_ForestSurvivorArena.spawnBlockerLayers` should be `Ignore Raycast`.
- Assign final spawn telegraph prefab if prototype LineRenderer warning rings are not desired.
- Assign final reticle, projectile, VFX, SFX, XP orb, boss HP, and HUD art as those assets become available.

## Manual Test Checklist

1. Open `Assets/_Project/Scenes/DesktopSurvivorPrototype.unity`.
2. Confirm the original arena still exists and was not deleted.
3. Open `Assets/_Project/Scenes/DesktopSurvivorPrototype_ForestArena.unity`.
4. Run `Tap Knockout > Survivor > Repair Prototype Scene`.
5. Run `Tap Knockout > Survivor > Validate Prototype Scene`.
6. Run `Tap Knockout > Survivor > Validate Forest Arena Scene`.
7. Press Play.
8. Move with WASD through the central clearing.
9. Dash in all directions.
10. Aim with the mouse reticle across the forest floor.
11. Hold left mouse and confirm projectiles fire toward the reticle.
12. Confirm projectiles do not visually disappear into terrain unexpectedly.
13. Confirm enemies spawn with telegraph warnings.
14. Confirm enemies do not spawn inside the player, trees, rocks, or blockers.
15. Confirm edge-pressure spawns happen near arena borders.
16. Confirm enemies can move toward the player.
17. Confirm the player does not snag on decorative grass or bushes.
18. Confirm tree and rock borders block only where intended.
19. Confirm the camera is not obstructed by tall trees during normal play.
20. Confirm hit flash, damage numbers, enemy death, and XP still work.
21. Check Console for errors or repeated warnings.
22. Return to the original arena scene and confirm it still works.

## Performance Notes

- The builder uses model instances and keeps decorative colliders removed.
- Boundary physics is represented by simple box colliders.
- Only a few scene lights are added.
- The generated hierarchy is grouped by gameplay purpose for maintainability.
- Static flags are applied to the generated arena hierarchy.

## Known Risks And TODOs

- Unity play mode visual QA is still required to tune exact prop density and camera occlusion.
- The forest pack has no dedicated terrain, flower, mushroom, bridge, or ruin assets, so those concepts are represented through available nature props only.
- Layer 2 is used for blockers because the project currently has no dedicated Ground or Environment layer.
- A future project layer pass should add explicit `Ground` and `EnvironmentBlocker` layers if the team wants cleaner layer semantics.
- Final spawn telegraph, reticle, projectile, VFX, SFX, and boss presentation assets still need art direction.
