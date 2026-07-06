# Desktop Survivor Pivot Plan

## Why the Pivot Happened

The project direction changed from a mobile room-based action roguelite to a desktop-first 3D arena survivor roguelike. The new target better matches the desired platform shift to PC/Steam, active skill usage, wave/timer pressure, build crafting, and large-scale 3D arena combat.

## Old Direction vs New Direction

| Area | Old Direction | New Direction |
|---|---|---|
| Platform | Mobile/Android first | Desktop PC/Steam first |
| Controls | Touch/virtual joystick | WASD, mouse aim, hotkeys |
| Core unit | Rooms/chapters | Arena runs and wave timeline |
| Progression | Room rewards and chapter completion | XP pickups, level-ups, elites, boss milestones |
| Combat | Stop-to-attack and dash impact | Auto/aimed attacks, active skills, dash, AoE, build crafting |
| Monetization | Rewarded ads/IAP readiness | Demo/premium-friendly future model |
| QA | Mobile device and safe area | Desktop 60 FPS, 100+ enemy stress, readability |

## Systems to Preserve

- Data-driven ScriptableObject configs.
- Ability definitions, tags, rarity, stack rules.
- Enemy configs and boss configs.
- Damage, health, projectile, knockback concepts.
- Wave/spawn logic concepts.
- Object pooling requirement.
- QA/performance planning.
- Content pipeline and license tracking.
- Service abstraction policy.

## Systems to Replace or Deprecate

Replace:

- Chapter/room runner with `ArenaRunDirector`.
- Room spawn points as primary model with spawn rings and budgets.
- Mobile HUD with desktop HUD.
- Room rewards with XP/drop/run result flow.

Deprecate for MVP:

- Touch joystick.
- Portrait orientation.
- Android-first release gates.
- Rewarded-ad-first progression.
- Daily mobile economy.
- Room clear as the main gameplay loop.

## Documentation Files Updated

Docs `00` through `30` have been migrated to the desktop survivor direction. Root `AGENTS.md` and package `AGENTS.md` have also been updated so future Codex work starts from the new canonical target.

## Code Migration Phases

### Phase 1 - Audit Existing Code and Data

Search for mobile, touch, room, Android, ad, and daily economy assumptions. Classify files as preserve, migrate, replace, or delete-later.

### Phase 2 - Prototype Foundation

Create or implement the foundation for desktop input, mouse aim, survivor camera, arena run director, spawn/wave director, XP pickups, level-up selection, and pooling requirements.

### Phase 3 - Playable Arena Loop

Make a single arena playable with movement, primary attack, dash, enemies, XP, level-up choices, and result screen.

### Phase 4 - Active Skills and Build Crafting

Add active skill slots, cooldowns, passives, projectile modifiers, dash modifiers, and area skills.

### Phase 5 - Enemy, Elite, and Boss Integration

Add at least 5 enemy archetypes, 1 elite, and 1 boss with warning, HP bar, telegraphs, and result flow.

### Phase 6 - Performance and Tooling

Add stress tests, pooling validation, wave timeline editor/report, ability weight report, XP curve tooling, and debug overlay.

### Phase 7 - Vertical Slice and Steam Demo Prep

Polish the 10-minute run, settings, UI, readability, build quality, known issues, and Steam-facing assets.

## First Implementation Sprint

Use `22_PRODUCTION_SPRINT_PLAN.md`: `Desktop Survivor Pivot — Prototype Foundation`.

The first sprint should not claim full gameplay completion. It should establish architecture and safe implementation direction.

## Risks

- Old mobile assumptions may remain hidden in docs/code.
- Room-based architecture may conflict with the arena loop.
- Enemy density can create performance risk.
- Active skill complexity can grow too quickly.
- Camera readability may fail under 3D crowd pressure.
- Ability balance scope may expand beyond the MVP.
- Asset licensing must be verified before production use.

## Acceptance Criteria

The pivot documentation is accepted when:

- The docs clearly define Tap Knockout as a desktop-first 3D arena survivor roguelike.
- Mobile/room/ad-first concepts are marked legacy or future optional.
- Required survivor systems are named and scoped.
- The vertical slice target is clear.
- The first sprint has a safe implementation path.
- No gameplay code is implemented by the documentation task.
- No `.meta` files are changed unless Unity asset creation requires it.

## Open TODOs

- Decide final primary attack policy: full auto-fire, hold-to-fire, or hybrid.
- Decide whether right mouse or Space/Shift is the default dash binding.
- Decide whether the first demo uses keyboard-only skills or optional controller support.
- Decide exact art style and which staged assets are approved for migration.
- Decide commercial model after the vertical slice: premium, demo-to-full, DLC, or another model.
