# Repository Discovery and Documentation Audit

Audit date: 2026-06-28

## Project Summary After Pivot

Tap Knockout is now documented as a desktop-first Unity 3D arena survivor roguelike. The older mobile portrait room/chapter direction is legacy. Future implementation should target PC/Steam, WASD movement, mouse aim, active skills, dash/evade, XP pickups, level-up ability choices, wave/timer progression, elites, and boss milestones.

## Repository Facts

| Check | Result |
|---|---|
| Unity project root | `/Users/burakcoskun/TapKnockout` contains `Assets/`, `Packages/`, and `ProjectSettings/`. |
| Unity version | `ProjectSettings/ProjectVersion.txt` should be treated as the source of truth. Previously observed editor version was Unity 6. |
| Git | The current workspace is inside a Git work tree. |
| Production docs | `Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365/`. Folder name is historical. |
| Production code area | `Assets/_Project/` is the target for production implementation. Existing contents require task-specific inspection before editing. |
| Third-party target | `Assets/ThirdParty/` remains the target for approved asset migration. |
| Staged assets | Existing packs remain staged under `Assets/Assets/game asset packs/`. |
| Scene policy | Do not hand-edit `.unity` YAML. Use manual Unity setup or approved Editor tools. |

## Documentation Coverage

| Area | Status After Pivot |
|---|---|
| Product vision | Updated for desktop survivor MVP. |
| GDD | Updated for arena/run/wave loop. |
| Architecture | Updated for desktop input, camera, run/spawn directors, pickups, abilities, boss, pooling. |
| Combat | Updated for active skills, passives, modifiers, dash upgrades, area abilities. |
| Level/wave | Updated for run timer, spawn budget, elites, boss milestones. |
| UI/UX | Updated for WASD/mouse/hotkeys and desktop HUD. |
| Economy | Downgraded to demo-friendly progression and future meta. |
| Monetization | Repositioned as future optional after Steam demo validation. |
| Analytics | Updated to run-based telemetry and survivor tuning keys. |
| QA/performance | Updated for 100+ enemy stress, pooling, frame time, readability. |
| Backlog/sprint | Updated for the pivot implementation path. |
| Config/prefab contracts | Updated for desktop survivor systems. |

## Immediate Implementation Risks

1. Existing code/config/assets may still contain mobile or room-first assumptions.
2. Existing room systems may conflict with the continuous arena loop.
3. Enemy density will require pooling and lightweight AI.
4. Camera and VFX readability must be proven early.
5. Active skill scope can grow too quickly without config validation and balance reports.
6. Asset migration must remain license-safe.

## Recommended Next Step

Run the first implementation sprint from `22_PRODUCTION_SPRINT_PLAN.md`: `Desktop Survivor Pivot — Prototype Foundation`.
