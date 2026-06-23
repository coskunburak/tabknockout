# Backlog Master

This backlog translates the product docs into production epics. It is intentionally implementation-ready but still code-free.

## Priority Definitions

| Priority | Meaning |
|---|---|
| P0 | Required before a playable vertical slice can be trusted. |
| P1 | Required before soft-launch candidate. |
| P2 | Required before public soft launch or global launch. |
| P3 | LiveOps, polish, expansion, or post-launch growth. |

## Epic B0 - Production Foundation

Goal: turn the template Unity project into a controlled production workspace.

| ID | Task | Priority | Dependencies | Acceptance |
|---|---|---|---|---|
| B0-01 | Initialize Git and baseline ignore rules | P0 | None | `git status` works; Unity generated folders are ignored; initial docs are committed. |
| B0-02 | Create `Assets/_Project` production folder structure | P0 | B0-01 | Structure matches `03_TECH_ARCHITECTURE_UNITY.md`; no gameplay behavior added. |
| B0-03 | Create `Assets/ThirdParty` staging structure | P0 | B0-01 | Third-party folders exist; imported packs remain unmoved until license/migration task. |
| B0-04 | Add root and package docs readmes | P0 | B0-02 | Future Codex can find docs and rules from repo root. |
| B0-05 | Audit ProjectSettings manually in Unity | P0 | B0-01 | Company, product, portrait orientation, package id, Android target, and input backend are verified. |
| B0-06 | Create Editor-safe scene builder plan | P0 | B0-02 | Future scene creation uses Editor script/manual steps, not YAML edits. |

## Epic B1 - Runtime Architecture Contracts

Goal: define stable interfaces and data boundaries before gameplay behavior grows.

| ID | Task | Priority | Dependencies | Acceptance |
|---|---|---|---|---|
| B1-01 | Combat contracts | P0 | B0 | `IDamageable`, hit context, damage types, knockback data, and combat events are specified. |
| B1-02 | Config ID conventions | P0 | B0 | Every config has stable lowercase snake_case id and validation rules. |
| B1-03 | Runtime service interfaces | P0 | B0 | Analytics, remote config, ads, IAP, save, audio services are abstracted. |
| B1-04 | Pooling contracts | P0 | B1-01 | Projectiles, enemies, VFX, damage text, pickups have pool ownership rules. |
| B1-05 | Save data versioning | P1 | B1-03 | Save schema includes version and migration policy. |

## Epic B2 - Player Combat Foundation

Goal: make the player feel responsive and establish the dash-impact identity.

| ID | Task | Priority | Dependencies | Acceptance |
|---|---|---|---|---|
| B2-01 | One-finger movement foundation | P0 | B1 | Drag/virtual joystick movement works in portrait and respects input abstraction. |
| B2-02 | Targeting foundation | P0 | B2-01 | Nearest valid enemy targeting is deterministic and configurable. |
| B2-03 | Stop-to-attack controller | P0 | B2-02 | Player attacks only under agreed movement threshold and cooldown. |
| B2-04 | Projectile/melee hit foundation | P0 | B1-01 | Initial weapon can damage enemies through shared damage flow. |
| B2-05 | Dash movement | P0 | B2-01 | Dash uses config values, cooldown, facing/current movement direction, and collision policy. |
| B2-06 | Dash impact hit | P0 | B2-05 | Dash can damage and knock back enemies without duplicate hits per dash. |
| B2-07 | Dash ability hooks | P1 | B2-06 | Dash events can trigger shockwave, fire trail, lightning, or cooldown modifiers later. |

## Epic B3 - Enemy, Room, Wave, and Boss Loop

Goal: build the Archero-style room sequence without copying protected content.

| ID | Task | Priority | Dependencies | Acceptance |
|---|---|---|---|---|
| B3-01 | Enemy base controller | P0 | B1, B2 | Enemy has health, damageable interface, movement hook, attack hook, death event. |
| B3-02 | Melee chaser | P0 | B3-01 | Moves toward player and attacks in range. |
| B3-03 | Ranged shooter | P0 | B3-01 | Maintains distance and fires readable projectiles. |
| B3-04 | Charger | P0 | B3-01 | Telegraphs charge and can be interrupted/knocked by dash. |
| B3-05 | Enemy spawner | P0 | B3-01 | Spawns by config, pattern, delay, max alive. |
| B3-06 | Wave manager | P0 | B3-05 | Starts waves and reports clear. |
| B3-07 | Room manager | P0 | B3-06 | Handles room start, clear, reward/ability transition. |
| B3-08 | Chapter runner | P0 | B3-07 | Sequences 12-15 rooms and boss. |
| B3-09 | Boss placeholder | P0 | B3-01 | Stone Brute has at least slam, charge, and add summon placeholder hooks. |

## Epic B4 - Ability and Roguelite Run Progression

Goal: add temporary run upgrades that reinforce dash-impact combat.

| ID | Task | Priority | Dependencies | Acceptance |
|---|---|---|---|---|
| B4-01 | AbilityDefinition config | P0 | B1-02 | Ability fields match schema and validate id/tags/stacks. |
| B4-02 | Ability state manager | P0 | B4-01 | Tracks selected abilities and stacks for current run only. |
| B4-03 | Weighted choice provider | P0 | B4-01 | Offers 3 choices, avoids invalid duplicates, respects rarity. |
| B4-04 | Ability choice UI hooks | P0 | B4-03 | Pauses gameplay and resumes after selection. |
| B4-05 | First 10 ability implementations | P1 | B2, B4 | Includes attack, health, projectile, and dash-impact abilities. |
| B4-06 | First 25 ability content pass | P1 | B4-05 | Vertical slice pool is large enough for repeated runs. |

## Epic B5 - Meta Progression and Economy

Goal: prepare retention systems without overcomplicating the vertical slice.

| ID | Task | Priority | Dependencies | Acceptance |
|---|---|---|---|---|
| B5-01 | Currency model | P0 | B1-05 | Coins, gems, materials are represented; gems do not gate basic progress. |
| B5-02 | Reward table configs | P0 | B5-01 | Room, chapter, boss, ad, daily, and mission reward tables are data-driven. |
| B5-03 | Gear data model | P1 | B5-01 | Weapon and armor slots support rarity, level, stat modifiers. |
| B5-04 | Talent data model | P1 | B5-01 | Talent nodes support cost, unlock, stat modifier. |
| B5-05 | Save/load stubs | P0 | B1-05 | Save persists meta data in local test environment. |
| B5-06 | Economy spreadsheet | P1 | B5-02 | Sources/sinks and early upgrade curves can be tuned outside code. |

## Epic B6 - UI/UX and FTUE

Goal: make the first session understandable on portrait mobile.

| ID | Task | Priority | Dependencies | Acceptance |
|---|---|---|---|---|
| B6-01 | Gameplay HUD | P0 | B2, B3 | HP, XP/level, dash cooldown, pause, boss HP, rewards feedback. |
| B6-02 | Ability selection panel | P0 | B4 | Three cards, title, short text, rarity color, selected feedback. |
| B6-03 | Run result panel | P0 | B3, B5 | Win/loss, rewards, rooms cleared, retry/home. |
| B6-04 | Home shell | P1 | B5 | Chapter select plus placeholders for gear, talents, shop, missions, daily. |
| B6-05 | FTUE prompts | P1 | B2, B3, B4 | Move, stop-to-attack, dash, clear room, pick ability, upgrade. |
| B6-06 | Safe area QA | P0 | B6-01 | UI works on common notch/cutout devices. |

## Epic B7 - Monetization, Analytics, Remote Config

Goal: add integration-safe architecture without shipping real SDKs too early.

| ID | Task | Priority | Dependencies | Acceptance |
|---|---|---|---|---|
| B7-01 | Analytics abstraction | P0 | B1-03 | Gameplay calls interface only; console implementation logs events. |
| B7-02 | Remote config abstraction | P0 | B1-03 | Local config can override key balance values. |
| B7-03 | Rewarded ad stub | P1 | B7-01 | Fake rewarded flow supports success, cancel, fail, reward grant. |
| B7-04 | IAP stub | P1 | B7-01 | Fake product catalog and purchase result paths exist. |
| B7-05 | Monetization config | P1 | B5, B7 | Revive, reward multiplier, free chest, reroll, starter pack toggles are config-driven. |
| B7-06 | SDK readiness review | P1 | B7-03, B7-04 | No real SDK until privacy/store compliance is complete. |

## Epic B8 - Assets, Art, Audio, and VFX

Goal: make the vertical slice readable and commercially presentable without licensing risk.

| ID | Task | Priority | Dependencies | Acceptance |
|---|---|---|---|---|
| B8-01 | License inventory | P0 | B0 | Every used asset has source/license/credit status. |
| B8-02 | Third-party migration plan | P0 | B8-01 | Approved assets move to `Assets/ThirdParty` by source; no unknown-license use. |
| B8-03 | Placeholder art kit selection | P1 | B8-01 | Player, 3 enemies, boss, room kit, weapon, UI assets selected. |
| B8-04 | Dash VFX direction | P1 | B2-06 | Dash impact reads clearly in portrait view. |
| B8-05 | Combat SFX placeholders | P1 | B2, B3 | Movement, attack, dash, hit, death, room clear sounds are present. |
| B8-06 | Icon pass | P1 | B4 | Ability icons are original or properly licensed placeholders. |

## Epic B9 - QA, Performance, Android Build

Goal: ensure the slice runs reliably on mobile.

| ID | Task | Priority | Dependencies | Acceptance |
|---|---|---|---|---|
| B9-01 | EditMode tests for data | P0 | B1, B4, B5 | Config validation and deterministic selection tests pass. |
| B9-02 | PlayMode smoke tests | P1 | B2, B3 | Movement/combat/room loop smoke tests pass where feasible. |
| B9-03 | Manual test checklist | P0 | B2-B7 | QA plan can be executed by hand on Unity Editor and device. |
| B9-04 | Android debug build | P0 | B0-B8 | Builds and runs on Android device. |
| B9-05 | Performance capture | P0 | B9-04 | 5-minute gameplay session records FPS, memory, GC, object counts. |
| B9-06 | Release candidate gate | P1 | B9-05 | No critical errors; known issues logged; rollback path exists. |

## Epic B10 - Soft Launch and LiveOps

Goal: prepare production operations after the vertical slice is validated.

| ID | Task | Priority | Dependencies | Acceptance |
|---|---|---|---|---|
| B10-01 | KPI dashboard plan | P1 | B7 | FTUE, retention, run, monetization, economy, crash metrics defined. |
| B10-02 | Soft-launch country strategy | P2 | B9 | Target territory assumptions and release sequence documented. |
| B10-03 | A/B experiment plan | P2 | B7, B9 | Dash controls, early difficulty, revive ad, starter pack tests defined. |
| B10-04 | LiveOps content calendar | P3 | B8, B9 | First 8 weeks of events and content drops drafted. |
| B10-05 | Global launch hardening | P3 | B10 | Localization, store assets, support, compliance, scale plan ready. |

