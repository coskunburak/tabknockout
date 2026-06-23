# Ability and Enemy Design Catalog

## Ability Tags

- `attack`
- `projectile`
- `dash`
- `defense`
- `utility`
- `status`
- `economy`
- `summon`

## Initial Ability Catalog

| ID | Name | Tags | Rarity | Max Stacks | Effect | Slice Priority |
|---|---|---|---|---|---|---|
| `attack_up` | Attack Up | attack | common | 5 | Increase base damage. | P0 |
| `attack_speed_up` | Attack Speed Up | attack | common | 5 | Reduce attack cooldown. | P0 |
| `max_hp_up` | Max HP Up | defense | common | 5 | Increase max HP and heal partial amount. | P0 |
| `move_speed_up` | Move Speed Up | utility | common | 3 | Increase movement speed. | P0 |
| `crit_chance_up` | Crit Chance Up | attack | uncommon | 5 | Increase critical chance. | P1 |
| `double_shot` | Double Shot | projectile | rare | 1 | Fire an additional forward projectile with damage modifier. | P0 |
| `pierce` | Pierce | projectile | uncommon | 3 | Projectiles pass through additional enemies. | P0 |
| `ricochet` | Ricochet | projectile | rare | 1 | Projectile bounces to nearby target after hit. | P1 |
| `side_shot` | Side Shot | projectile | rare | 2 | Add angled side projectiles. | P1 |
| `dash_shockwave` | Dash Shockwave | dash | uncommon | 3 | Dash end emits radial impact damage. | P0 |
| `dash_cooldown_down` | Dash Cooldown Down | dash | common | 4 | Reduce dash cooldown. | P0 |
| `dash_damage_up` | Dash Damage Up | dash | common | 5 | Increase dash impact damage. | P0 |
| `dash_knockback_up` | Dash Knockback Up | dash | common | 3 | Increase dash knockback force. | P0 |
| `dash_fire_trail` | Dash Fire Trail | dash,status | rare | 3 | Dash leaves damage-over-time trail. | P1 |
| `dash_chain_lightning` | Dash Chain Lightning | dash,status | rare | 3 | Dash hit chains lightning to nearby enemies. | P1 |
| `burning_hits` | Burning Hits | status,attack | uncommon | 3 | Attacks apply burn. | P1 |
| `chain_lightning` | Chain Lightning | status,projectile | rare | 3 | Attacks chain lightning. | P1 |
| `orbiting_blade` | Orbiting Blade | summon,attack | rare | 3 | Add orbiting damage source. | P1 |
| `heal_after_room` | Heal After Room | defense | common | 3 | Heal when room clears. | P0 |
| `shield_on_room_start` | Shield On Room Start | defense | uncommon | 3 | Temporary shield at room start. | P1 |
| `coin_bonus` | Coin Bonus | economy | common | 5 | Increase coin rewards. | P2 |
| `boss_damage_up` | Boss Damage Up | attack | uncommon | 3 | Increase damage to bosses. | P1 |
| `projectile_size_up` | Projectile Size Up | projectile | uncommon | 3 | Increase projectile hit size. | P2 |
| `pickup_magnet` | Pickup Magnet | utility | common | 3 | Increase pickup radius. | P2 |
| `revive_token` | Revive Token | defense | rare | 1 | One non-ad revive in run. | P2 |

## Ability Design Rules

- Dash abilities must create visible combat moments.
- Damage-only abilities should not crowd out dash identity.
- Rare abilities may change behavior; common abilities should tune stats.
- Avoid exact names/icons/effects that copy protected games.
- Every ability needs analytics-safe `ability_id`.

## Enemy Taxonomy

| ID | Name | Role | Core Behavior | Dash Interaction | Slice Priority |
|---|---|---|---|---|---|
| `enemy_melee_chaser` | Melee Chaser | melee | Moves directly toward player and attacks in range. | Knockback interrupts movement briefly. | P0 |
| `enemy_ranged_shooter` | Ranged Shooter | ranged | Keeps distance and fires readable projectile. | Dash can close gap and interrupt windup later. | P0 |
| `enemy_charger` | Charger | charger | Telegraphs line charge, rushes forward, recovers. | Dash impact can interrupt or redirect based on config. | P0 |
| `enemy_elite_guard` | Elite Guard | elite | Tankier melee with wider attack. | Strong knockback feedback. | P1 |
| `enemy_bomber` | Area Denial | ranged | Throws delayed AoE marker. | Dash helps escape danger zone. | P2 |
| `enemy_summoner` | Summoner | support | Spawns weak adds. | Dash can punish stationary casting. | P2 |

## Boss Catalog

### `boss_stone_brute`

Role: first vertical slice boss.

Attacks:

- Ground slam: radial telegraph, delayed impact, high readability.
- Charge: line telegraph, movement burst, wall/recovery window.
- Add summon: spawns small melee enemies at defined health thresholds or cooldowns.
- Circular danger zone later: optional soft-launch expansion.

Dash interactions:

- Dash can avoid slam.
- Dash can cross charge path if timed.
- Dash impact can damage boss but should not trivialize boss interrupts unless config enables it.

Acceptance:

- Boss has clear windup/recovery.
- Boss HP bar updates.
- Boss defeat ends room/chapter.
- Boss events emit analytics hooks.

