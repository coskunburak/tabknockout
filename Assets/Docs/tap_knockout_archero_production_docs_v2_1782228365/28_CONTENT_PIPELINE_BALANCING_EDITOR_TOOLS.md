# Content Pipeline, Balancing, and Editor Tools

## Asset Intake Pipeline

1. Identify asset need from vertical slice or sprint plan.
2. Confirm source URL, author, license, and commercial-use rights.
3. Add entry to `17_CREDITS_TEMPLATE.md`.
4. Import or migrate into `Assets/ThirdParty/<Source>/<PackName>` only after approval.
5. Create production prefabs/material variants under `Assets/_Project`.
6. Do not modify third-party source assets unless unavoidable; prefer variants.
7. Validate scale, orientation, material count, texture size, and mobile readability.

## Current Staged Asset Notes

Known license files found:

- KayKit Character Animations: CC0.
- KayKit Dungeon Remastered: CC0.
- Quaternius Ultimate Animated Character Pack: CC0.
- Quaternius Medieval Weapons: CC0.
- Quaternius RPG Characters: CC0.
- Kenney Mini Dungeon: CC0.
- Kenney UI Pack: CC0.

Risk:

- `Cute Animated Monsters - Aug 2020` has no license file found in the shallow audit. Do not use it until source and license are proven.

## Balancing Spreadsheet Spec

Recommended workbook tabs:

| Tab | Purpose |
|---|---|
| `player_base` | HP, move speed, attack, dash defaults. |
| `weapons` | Damage, cooldown, range, projectile speed, special rules. |
| `enemies` | HP, damage, speed, attack cooldown, role, reward. |
| `bosses` | Boss HP, phase thresholds, attack cooldowns, damage. |
| `abilities` | Rarity, tags, weights, max stacks, effect values. |
| `chapter_1_rooms` | Room sequence, room type, waves, difficulty rating. |
| `waves` | Enemy groups, counts, spawn patterns, max alive. |
| `rewards` | Coins, materials, gems, chest rates, room rewards. |
| `gear_costs` | Upgrade level, coin/material costs, stat growth. |
| `talent_costs` | Node, level, cost, stat gain. |
| `remote_config` | Key, default, min, max, owner, experiment. |
| `analytics_events` | Event, trigger, parameters, owner, QA status. |

Rules:

- Spreadsheet IDs must match config IDs.
- Use explicit units, for example seconds, meters, percent.
- Any value controlled by remote config should have safe min/max.
- Do not tune monetization before retention and core fun are measured.

## Editor Tools Plan

Allowed future Editor tools:

| Tool | Purpose | Priority |
|---|---|---|
| Vertical Slice Scene Builder | Create placeholder Gameplay scene hierarchy without manual YAML edits. | P0 |
| Config Validator | Validate ScriptableObject ids, missing references, duplicate ids. | P0 |
| Ability Catalog Generator | Generate report of abilities, tags, weights, max stacks. | P1 |
| Room Preview Builder | Instantiate room template with spawn points for designer review. | P1 |
| Balance Importer | Import CSV/spreadsheet exports into config assets after schema stabilizes. | P2 |
| Credits Report | Generate third-party asset/license report. | P1 |
| Build Preflight | Check project settings, scenes, packages, SDK flags before Android build. | P1 |

Editor tool rules:

- Tools must live under `Assets/_Project/Editor/Tools`.
- Tools must not silently overwrite production scenes.
- Tools must log created/modified assets.
- Destructive actions require confirmation.

## Content Review Checklist

- Is the asset original or properly licensed?
- Is it visually distinct from Archero and other protected games?
- Does it read clearly in portrait?
- Does it support dash-impact combat readability?
- Does it fit mobile performance constraints?
- Is it credited?
- Is it placed in the correct folder?

