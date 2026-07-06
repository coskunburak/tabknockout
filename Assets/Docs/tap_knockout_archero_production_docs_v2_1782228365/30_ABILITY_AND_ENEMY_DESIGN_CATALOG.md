# Ability and Enemy Design Catalog

## Ability Tags

- `active`
- `passive`
- `weapon`
- `projectile`
- `dash`
- `area`
- `defense`
- `survival`
- `status`
- `summon`
- `pickup`
- `boss`

## Active Skills

| ID | Name | Tags | Rarity | Effect | Priority |
|---|---|---|---|---|---|
| `skill_arc_blast` | Arc Blast | active,area | common | Short cone burst in aim direction. | P0 |
| `skill_ground_slam` | Ground Slam | active,area,defense | uncommon | Radial AoE around player with knockback. | P0 |
| `skill_meteor_mark` | Meteor Mark | active,area | rare | Delayed targeted AoE at mouse point. | P1 |
| `skill_guard_pulse` | Guard Pulse | active,defense | uncommon | Temporary shield plus pulse damage. | P1 |
| `skill_orbit_blade` | Orbit Blade | active,summon | rare | Temporary orbiting damage source. | P1 |

## Passive Upgrades

| ID | Name | Tags | Max Stacks | Effect | Priority |
|---|---|---|---|---|---|
| `attack_up` | Attack Up | passive,weapon | 5 | Increase base damage. | P0 |
| `battle_rhythm` | Battle Rhythm | passive,weapon | 5 | Increase attack speed. | P0 |
| `iron_core` | Iron Core | passive,defense | 5 | Increase max HP and partial heal. | P0 |
| `swift_footwork` | Swift Footwork | passive,survival | 3 | Increase movement speed. | P0 |
| `pickup_magnet` | Pickup Magnet | passive,pickup | 3 | Increase pickup radius. | P0 |
| `boss_focus` | Boss Focus | passive,boss | 3 | Increase damage to bosses/elites. | P1 |

## Projectile Modifiers

| ID | Name | Tags | Max Stacks | Effect | Priority |
|---|---|---|---|---|---|
| `twin_shot` | Twin Shot | projectile,weapon | 1 | Adds an additional projectile. | P0 |
| `pierce` | Pierce | projectile | 3 | Projectiles pass through additional enemies. | P0 |
| `wide_angle` | Wide Angle | projectile | 2 | Adds angled side projectiles. | P1 |
| `ricochet` | Ricochet | projectile | 1 | Projectile jumps to nearby target after hit. | P1 |
| `charged_rounds` | Charged Rounds | projectile,status | 3 | Adds lightning/status hit chance. | P1 |

## Movement and Dash Upgrades

| ID | Name | Tags | Max Stacks | Effect | Priority |
|---|---|---|---|---|---|
| `phase_step` | Phase Step | dash,survival | 3 | Improves dash i-frame window. | P0 |
| `dash_cooldown_down` | Dash Cooldown Down | dash | 4 | Reduces dash cooldown. | P0 |
| `dash_shockwave` | Dash Shockwave | dash,area | 3 | Dash end emits radial damage. | P0 |
| `bulldozer` | Bulldozer | dash | 3 | Increases dash knockback. | P0 |
| `dash_fire_trail` | Dash Fire Trail | dash,status | 3 | Dash leaves damage trail. | P1 |

## Defensive and Survival Upgrades

| ID | Name | Tags | Max Stacks | Effect | Priority |
|---|---|---|---|---|---|
| `regen_spark` | Regen Spark | defense,survival | 3 | Small periodic heal. | P1 |
| `emergency_barrier` | Emergency Barrier | defense | 1 | Shield triggers at low HP. | P1 |
| `armor_plating` | Armor Plating | defense | 5 | Reduces incoming damage. | P0 |
| `second_wind` | Second Wind | survival | 1 | One non-monetized revive or near-death heal. | P2 |

## Basic Enemies

| ID | Role | Core Behavior | MVP Priority |
|---|---|---|---|
| `enemy_melee_chaser` | melee | Moves toward player and deals contact/short melee damage. | P0 |
| `enemy_swarm_runner` | swarm | Fast low-HP enemy used for density. | P0 |
| `enemy_ranged_shooter` | ranged | Keeps distance and fires readable projectiles. | P0 |
| `enemy_charger` | charger | Telegraphs line charge, rushes, then recovers. | P0 |
| `enemy_tank_guard` | tank | Slow, high HP, blocks space. | P0 |

## Elite Variants

| ID | Base | Modifier | Behavior |
|---|---|---|---|
| `elite_burning_charger` | `enemy_charger` | Fire trail | Charge leaves danger trail. |
| `elite_shield_guard` | `enemy_tank_guard` | Shielded | Starts shielded, vulnerable after attack. |
| `elite_storm_shooter` | `enemy_ranged_shooter` | Multi-shot | Fires spread projectiles with clear tell. |

## Boss Designs

### `boss_arena_brute`

Role: first MVP boss.

Attacks:

- Ground slam: radial telegraph, delayed impact.
- Line charge: long telegraph, burst movement, recovery.
- Add call: spawns limited melee adds.
- Shock ring: expanding ring later if readable.

Acceptance:

- Boss warning appears before spawn.
- Boss health bar binds correctly.
- Attacks are readable with adds alive.
- Defeat ends the MVP run.

## Enemy Design Rules

- Every enemy must be readable from the survivor camera.
- Enemy density should come from simple behaviors first.
- Ranged and charger attacks need clear telegraphs.
- Elite modifiers must be visible.
- Bosses need windup, impact, and recovery.

## Ability Design Rules

- Every ability needs a stable ID.
- Every level-up choice needs a short readable description.
- Active skills need cooldown UI.
- Stacks need clear limits.
- Synergy tags must support reporting and weighted offers.
