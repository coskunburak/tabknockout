# Content Pipeline, Balancing, and Editor Tools

## Asset Intake Pipeline

1. Identify asset need from vertical slice or sprint plan.
2. Confirm source URL, author, license, and commercial-use rights.
3. Add entry to `17_CREDITS_TEMPLATE.md`.
4. Import or migrate into `Assets/ThirdParty/<Source>/<PackName>` only after approval.
5. Create production prefabs/material variants under `Assets/_Project`.
6. Review from gameplay camera.
7. Validate scale, silhouette, material count, texture size, VFX readability, and performance.

## Balancing Spreadsheet Spec

Recommended workbook tabs:

| Tab | Purpose |
|---|---|
| `player_base` | HP, move speed, dash, pickup radius, starting attack. |
| `active_skills` | Cooldowns, damage, duration, radius, charges. |
| `passives` | Stat upgrades, stacks, rarity, weights. |
| `projectile_modifiers` | Pierce, split, ricochet, size, speed, count. |
| `dash_modifiers` | Cooldown, distance, i-frame, impact damage, knockback. |
| `enemies` | HP, damage, speed, role, budget cost, XP reward. |
| `elites` | Base enemy, multipliers, modifiers, rewards. |
| `bosses` | HP, phases, attacks, adds, timing. |
| `wave_timeline` | Time segments, spawn groups, budget, intensity. |
| `xp_curve` | Level thresholds and pacing. |
| `difficulty_curve` | Time-based multipliers. |
| `remote_config` | Key, default, min, max, owner. |
| `analytics_events` | Event, trigger, parameters, QA status. |

Deprecated mobile tuning tabs:

- Room reward tuning.
- Chapter room sequence.
- Rewarded ad placements.
- Daily login economy.
- Mobile IAP bundle pacing.

## Editor Tools Plan

| Tool | Purpose | Priority |
|---|---|---|
| Desktop Survivor Scene Builder | Create `DesktopSurvivorPrototype` hierarchy safely. | P0 |
| Config Validator | Validate IDs, missing references, duplicate IDs, stack limits. | P0 |
| Enemy Wave Timeline Editor | Author and preview timed spawn segments. | P0 |
| Spawn Budget Editor | Show live budget cost and max alive pressure. | P0 |
| Ability Weight Editor | Tune offer weights, rarity, tags, exclusions. | P0 |
| XP Curve Editor | Preview levels over expected kill/XP rates. | P1 |
| Difficulty Curve Editor | Preview enemy multipliers over time. | P1 |
| Boss Milestone Editor | Configure warning, spawn, phases, adds. | P1 |
| Run Simulator | Simulate timeline, spawn budget, XP, and level-up cadence without full art. | P1 |
| Debug Overlay | Show run timer, live enemies, budget, FPS, pool counts, XP, next wave. | P1 |
| Credits Report | Generate third-party asset/license report. | P1 |
| Build Preflight | Check desktop build settings, scenes, packages, SDK flags. | P1 |

## Editor Tool Rules

- Tools live under `Assets/_Project/Editor/Tools`.
- Tools must not silently overwrite production scenes.
- Tools must log created/modified assets.
- Destructive actions require confirmation.
- Tools should not import packages or SDKs without approval.

## Content Review Checklist

- Is the asset original or properly licensed?
- Does it read clearly from the survivor camera?
- Does it work under enemy density?
- Does it preserve boss/elite telegraph readability?
- Does it fit desktop performance targets?
- Is it credited?
- Is it placed in the correct folder?
