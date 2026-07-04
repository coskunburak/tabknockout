# Game Design Document

## Game Overview

Tap Knockout is a 3D desktop arena survivor roguelike. The player enters a combat arena, survives escalating wave pressure, collects XP and drops, levels up, chooses abilities, fights elites and bosses, and ends the run with win/loss rewards and progression.

## Core Run Loop

1. Select character and starting loadout.
2. Enter one arena.
3. Move with WASD and aim with mouse.
4. Kill enemies with auto-fire, aimed attacks, active skills, dash impacts, and ability effects.
5. Collect XP orbs and pickups.
6. Level up and choose one of several build options.
7. Survive timed wave pressure and elite milestones.
8. Fight boss milestone.
9. Finish run, die, or extract if a future mode adds extraction.
10. View result screen and apply eligible progression.

## Arena and Progression Structure

The main unit of play is the run, not a room. Each run uses:

- `ArenaConfig` for environment bounds, spawn rules, camera constraints, and pickups.
- `RunConfig` for duration, level curve, milestones, and result rules.
- `WaveTimelineConfig` for timed enemy pressure.
- `BossEncounterConfig` for boss spawn timing and fight behavior.

Rooms are legacy terminology. If reused later, they should mean optional biome/challenge arenas or special event stages, not the core run structure.

## Controls

MVP controls:

- WASD: movement.
- Mouse: aim direction and targeting intent.
- Left mouse: primary attack policy, either hold-to-fire or auto-fire with aim direction.
- Space or Shift: dash/evade.
- Q/E/R/F or number keys: active skills.
- Esc: pause.
- Tab: build overview.

Controller support is optional future work. Touch controls are deprecated for the desktop prototype.

## Combat Model

Combat combines:

- Auto-fire or cadence-based primary weapon behavior.
- Mouse-aimed attacks where appropriate.
- Cooldown-based active skills.
- Passive stat upgrades.
- Projectile modifiers.
- Area damage abilities.
- Dash modifiers and dash impact effects.
- Defensive/survival upgrades.

Damage, health, knockback, projectile, and status systems should remain data-driven and reusable.

## XP and Level-Up

Enemies drop XP orbs or XP-equivalent pickups. When the player reaches the next level:

- Combat pauses or slows according to the final UX decision.
- A level-up modal offers 3 choices by default.
- Choices respect rarity, weights, tags, max stacks, exclusions, and prerequisites.
- Selected abilities apply immediately and emit analytics.

The first MVP should include at least 12 ability choices across active skills, passives, projectile modifiers, dash upgrades, and survival upgrades.

## Enemies

MVP enemy groups:

- Basic melee chaser.
- Swarm/light melee.
- Ranged shooter.
- Charger.
- Tank or shield enemy.
- Elite variant.
- Boss.

Enemies should use lightweight chase/steering logic, clear silhouettes, readable attack states, and pooled spawning. Heavy per-enemy behavior should be avoided until performance is proven.

## Boss Milestones

Bosses are run milestones, not room endpoints. A boss spawn should include:

- Warning UI and audio/VFX cue.
- Boss health bar.
- Arena pressure adjustment.
- Readable telegraphs.
- Fair recovery windows.
- Result transition on defeat or player death.

## Run Result

The result screen should show:

- Win/loss.
- Run duration.
- Level reached.
- Waves or milestones reached.
- Enemies, elites, and boss kills.
- Ability build summary.
- Rewards/unlocks if implemented.
- Retry and return-to-menu actions.

## MVP Out of Scope

- Mobile touch UI.
- Ad/IAP flows.
- Full gear economy.
- Multiple biomes.
- Procedural arena generation.
- Full liveops.
- Network or multiplayer.

## Fun Test

The MVP is successful only if:

- Movement feels responsive.
- Enemy pressure escalates without becoming unreadable.
- Level-up choices visibly alter combat.
- Active skill timing matters.
- Boss attacks are readable and fair.
- The player wants to retry with a different build.
