# Vertical Slice Spec

## Purpose

The vertical slice proves that Tap Knockout can support a commercial Archero-style mobile action roguelite loop while establishing an original dash-impact combat identity. It is not a throwaway prototype.

## Target Platform

- Unity 6000.5.0f1 project
- URP mobile renderer path
- Android first
- Portrait orientation
- One-finger movement plus dash button
- No real ads/IAP/analytics SDKs in the vertical slice

## Player Promise

In a 5-8 minute test run, a new player should understand:

1. Drag to move.
2. Stop or position to attack.
3. Dash to avoid danger.
4. Dash into enemies to create impact, knockback, and ability-triggered moments.
5. Clear rooms.
6. Pick run abilities.
7. Beat a boss or die.
8. Return home with rewards and visible upgrade paths.

## Slice Scope

| System | Required Scope |
|---|---|
| Movement | Drag-anywhere or virtual joystick movement, portrait-safe, mobile-friendly. |
| Auto-attack | Stop-to-attack nearest valid enemy with cooldown and weapon config. |
| Dash | Cooldown, direction, short invulnerability hook, impact hitbox, knockback, event hooks. |
| Damage | Shared hit context, physical and impact damage, health, death events. |
| Projectile | One initial weapon and projectile pool or clearly bounded placeholder. |
| Enemies | Melee chaser, ranged shooter, charger. |
| Boss | Stone Brute placeholder with slam, charge, and add summon patterns. |
| Rooms | 12-15 room Chapter 1 sequence with combat, elite, reward/heal, mini-boss, boss. |
| Abilities | At least 10 implemented, 25 cataloged; 3-card selection after defined rooms. |
| Meta | Coins, gems, materials, weapon/armor placeholders, talent placeholders, local save. |
| UI | Gameplay HUD, ability cards, pause, run result, basic home shell. |
| Analytics | Console analytics events only; no external SDK. |
| Remote config | Local config provider only; no external service. |
| Monetization | Fake rewarded ad and IAP interfaces only, if needed for UI paths. |
| QA | Manual Android smoke test and data validation tests. |

## Out of Scope

- Real Ads, IAP, Analytics, crash reporting, consent, or remote config SDKs.
- Energy system.
- Battle pass.
- Procedural generation.
- Final store art.
- Direct `.unity` YAML edits.
- Archero asset, icon, enemy, map, economy, or store copying.

## Chapter 1 Draft

| Room | Type | Goal | Content |
|---|---|---|---|
| 1 | Tutorial Combat | Teach movement and stop-to-attack | 3 melee chasers, low damage. |
| 2 | Combat | Reinforce targeting | 5 melee chasers, staggered spawn. |
| 3 | Combat | Introduce ranged threat | 2 melee, 2 ranged. |
| 4 | Ability Reward | First choice | 3-card ability offer. |
| 5 | Combat | Teach dash defense | Ranged crossfire with dash prompt. |
| 6 | Combat | Introduce charger | 2 chargers, clear telegraph. |
| 7 | Combat | Mixed pressure | Melee + ranged + charger. |
| 8 | Elite | Test dash impact | Tankier elite chaser vulnerable to knockback. |
| 9 | Heal/Reward | Recovery and pacing | Heal pickup or small currency reward. |
| 10 | Combat | Ability synergy check | Mixed wave with enough density for dash shockwave. |
| 11 | Mini-boss | Pattern preview | Small brute with slam and charge. |
| 12 | Combat | Higher pressure | Larger mixed wave. |
| 13 | Combat | Ranged/charger combo | Requires dash timing. |
| 14 | Reward/Ability | Final prep | Ability offer before boss. |
| 15 | Boss | Slice climax | Stone Brute boss. |

## Ability Minimums

Implemented by slice:

- Attack Up
- Attack Speed Up
- Max HP Up
- Move Speed Up
- Double Shot or Pierce
- Dash Shockwave
- Dash Cooldown Down
- Dash Damage Up
- Dash Knockback Up
- Heal After Room

Cataloged but optional by slice:

- Crit Chance Up
- Ricochet
- Side Shot
- Burning Hits
- Chain Lightning
- Orbiting Blade
- Dash Leaves Fire
- Dash Chain Lightning
- Dash Heal On Hit
- Coin Bonus
- Shield On Room Start
- Projectile Size Up
- Boss Damage Up
- Revive Token
- Pickup Magnet

## Acceptance Criteria

- Android debug build launches into a controllable portrait gameplay flow.
- The player can complete or fail a 12-15 room chapter.
- Dash can damage and knock back enemies.
- At least one dash-specific ability changes combat behavior.
- Ability selection pauses gameplay and resumes correctly.
- Boss room has at least three readable attack behaviors or placeholders.
- Rewards are granted at run end and persist locally.
- No external SDKs are required.
- No direct scene YAML edits were made.
- Manual Unity setup is documented for any scene/prefab work.

## Definition of Done

- All P0 slice tasks in `20_BACKLOG_MASTER.md` are complete or explicitly deferred with owner and reason.
- Test plan in `27_QA_PERFORMANCE_SOFT_LAUNCH_PLAN.md` passes for Editor and one Android device.
- All configs used by the slice are listed in `24_DATA_CONFIG_SCHEMA.md`.
- All prefabs/scenes used by the slice match `25_PREFAB_AND_SCENE_CONTRACTS.md`.
- Any third-party asset used in the slice is listed in `17_CREDITS_TEMPLATE.md`.
- Known issues are documented with severity and reproduction steps.

