# Vertical Slice Spec

## Target

Build one internal playable desktop survivor slice:

- One 3D arena.
- One player character.
- WASD movement.
- Mouse aim.
- Dash/evade key.
- Primary attack policy.
- Active skill hotkeys.
- XP pickups.
- Level-up ability choices.
- Wave/timer progression.
- At least 5 enemy archetypes.
- At least 1 elite.
- At least 1 boss.
- At least 12 ability choices.
- 10-minute run target.
- Boss warning and boss health bar.
- Run win/loss result screen.

## Arena

MVP arena requirements:

- Clear movement space.
- Distinct spawn zones or spawn ring logic.
- Camera bounds.
- Ground readability for telegraphs.
- No unavoidable geometry traps.
- Supports 100+ simple enemies in stress test.

## Player

Player requirements:

- Responsive movement.
- Mouse-facing or mouse-aimed attack direction.
- Dash/evade with cooldown.
- Health and death.
- Receives active/passive ability effects.

## Enemies

Minimum enemy archetypes:

- Basic melee chaser.
- Swarm/light enemy.
- Ranged shooter.
- Charger.
- Tank/shield enemy.

Elite:

- At least one enhanced variant with warning and reward.

Boss:

- At least one boss with health bar, warning, telegraphs, and defeat event.

## Abilities

Minimum 12 ability choices across:

- Active skills.
- Passive upgrades.
- Projectile modifiers.
- Dash upgrades.
- Area damage abilities.
- Defensive/survival upgrades.

Each ability must have an ID, category, rarity, tags, stack rule, and short player-facing description.

## Run Flow

1. Start run.
2. Basic enemies spawn.
3. Player collects XP.
4. Level-up modal appears.
5. Enemy pressure escalates.
6. Elite milestone appears.
7. Boss warning appears.
8. Boss spawns.
9. Boss defeated or player dies.
10. Result screen appears.

## Fun Test Acceptance

The vertical slice passes the fun test only if:

- Player understands major threats.
- Movement feels responsive.
- Aiming and skill use feel intentional.
- Level-up choices meaningfully alter combat.
- Enemy pressure escalates across the run.
- XP pickups feel rewarding without hiding danger.
- Boss is readable and fair.
- The player can describe a build direction after the run.
- The player wants to retry.

## Technical Acceptance

- 60 FPS target on mid-range PC.
- 100+ enemy stress test is recorded.
- Projectiles and pickups are pooled.
- No major recurring GC spikes during combat waves.
- Level-up pause/resume cannot break run state.
- Result screen handles both win and loss.
- No real monetization SDKs.
- No direct `.unity` YAML hand edits.
