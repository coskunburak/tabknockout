# Repository Discovery and Documentation Audit

Audit date: 2026-06-23

## 1. Project Understanding Summary

Tap Knockout is no longer a small arena prototype. It is planned as an Android-first, portrait-mode, commercial Unity 3D/URP action roguelite with room/chapter progression, stop-to-attack or auto-attack combat, roguelite ability choices, permanent meta progression, gear, talents, currencies, rewarded ads, IAP readiness, analytics, remote config, A/B testing readiness, soft-launch planning, and LiveOps expansion.

The differentiator is dash-impact combat. Dash is not only traversal. It is a combat verb that can damage, knock back, interrupt, trigger shockwaves, chain lightning, leave fire trails, and drive ability synergies.

## 2. Repository Discovery Summary

| Check | Result |
|---|---|
| Unity project root confirmation | `/Users/burakcoskun/TapKnockout` contains `Assets/`, `Packages/`, `ProjectSettings/`, and `UserSettings/`. This is a Unity project root. |
| Unity version | `ProjectSettings/ProjectVersion.txt` reports Unity `6000.5.0f1`. |
| Git status | The directory is not currently a Git repository. `git status` fails with `not a git repository`. |
| Existing folder structure | Top level: `Assets/`, `Library/`, `Logs/`, `Packages/`, `ProjectSettings/`, `Temp/`, `UserSettings/`. Generated folders should stay out of audits. |
| Existing Assets structure | `Assets/Settings`, `Assets/Scenes`, `Assets/Docs`, `Assets/TutorialInfo`, `Assets/Assets/game asset packs`. |
| Production folder | `Assets/_Project` does not exist yet. |
| Third-party folder | `Assets/ThirdParty` does not exist yet. Existing packs are under `Assets/Assets/game asset packs`, which should be treated as an imported asset staging area until approved migration. |
| Existing docs | Production docs exist at `Assets/Docs/tap_knockout_archero_production_docs_v2_1782228365`. |
| AGENTS.md | A docs-local `AGENTS.md` existed. A root `AGENTS.md` has been added so future Codex tasks receive rules automatically. |
| Existing scenes | `Assets/Scenes/SampleScene.unity` only. It is enabled in `ProjectSettings/EditorBuildSettings.asset`. |
| Existing scripts | Only Unity tutorial readme scripts: `Assets/TutorialInfo/Scripts/Readme.cs` and `Assets/TutorialInfo/Scripts/Editor/ReadmeEditor.cs`. No production gameplay scripts exist. |
| Packages summary | URP `17.5.0`, Input System `1.19.0`, AI Navigation `2.0.13`, Unity Test Framework `1.7.0`, uGUI `2.5.0`, Timeline `1.8.12`, Visual Scripting `1.9.11`, IDE packages, and Unity modules. |
| Render pipeline | `Assets/Settings` contains URP assets including `Mobile_RPAsset`, `Mobile_Renderer`, `PC_RPAsset`, and `PC_Renderer`. |
| Player settings risks | `companyName` is still `DefaultCompany`; Android package id is still `com.UnityTechnologies.com.unity.template.urpblank`; landscape autorotation remains enabled; Android target SDK is automatic; signing is empty. |
| Existing imported assets | KayKit, Kenney, and Quaternius packs include CC0 license files. `Cute Animated Monsters - Aug 2020` has no license file found in a shallow check and must not be used until its license is proven. |
| Production structure compliance | Not yet. The docs describe the target structure, but the repository is still a near-template Unity project with sample scene, tutorial scripts, and staged assets. |

## 3. Existing Documentation Inventory

| Document | Purpose | Status | Weaknesses | Recommended Action | Priority |
|---|---|---|---|---|---|
| `00_README_INDEX.md` | Entry point and reading order | Needs Update | Did not list newly required execution docs before this audit. Mixed language is acceptable, but future implementation prompts need a single source of truth. | Update index with new docs and current repository facts. | P0 |
| `AGENTS.md` in docs package | Codex rules | Needs Update | Useful but not visible from repo root. | Add root `AGENTS.md`; keep docs-local copy as package reference. | P0 |
| `01_PRODUCT_VISION.md` | Vision, audience, positioning | Good | Needs market/KPI follow-through in soft-launch docs. | Keep; reference from sprint plan and KPI plan. | P1 |
| `02_GDD_PRODUCTION.md` | Core game design | Good | Needs concrete vertical slice acceptance, ability/enemy catalogs, and tuning tables. | Add vertical slice and content catalog docs. | P0 |
| `03_TECH_ARCHITECTURE_UNITY.md` | Target Unity architecture | Good | Describes desired folder structure, but repo does not yet match it. Needs contracts for scenes/prefabs/configs. | Add data schema and prefab/scene contracts. | P0 |
| `04_COMBAT_AND_ABILITIES.md` | Combat and ability concepts | Good | Needs exact first-pass ability list, tags, stacking rules, and acceptance gates. | Add ability catalog and sprint tasks. | P0 |
| `05_LEVEL_ROOM_WAVE.md` | Chapter, room, wave model | Good | Needs prefab contracts, room test matrix, and first chapter spec. | Add vertical slice, prefab contracts, and QA docs. | P0 |
| `06_META_ECONOMY.md` | Currencies, gear, talents | Needs Update | Directional only; lacks economy spreadsheet schema, reward tables, and soft-launch economy gates. | Add backlog, balancing spreadsheet spec, and soft-launch KPI plan. | P1 |
| `07_MONETIZATION_LIVEOPS.md` | Ads, IAP, shop, daily systems | Needs Update | Placement list exists, but no config schema or compliance gates. | Add monetization config spec and KPI plan. | P1 |
| `08_UI_UX_CONTROLS.md` | Controls and UI screens | Needs Update | Needs screen acceptance criteria, safe-area QA, and FTUE event hooks. | Cover in sprint plan and QA plan. | P1 |
| `09_ASSET_PIPELINE.md` | Asset direction and license policy | Needs Update | Does not inventory current staged assets or define intake/migration gates. | Add content pipeline doc and update credits. | P0 |
| `10_ANALYTICS_REMOTE_CONFIG.md` | Analytics and remote config overview | Needs Update | Event list lacks parameter schema, owner, trigger, and validation rules. | Add analytics event schema and remote config key doc. | P0 |
| `11_QA_PERFORMANCE_RELEASE.md` | QA/performance/release overview | Needs Update | Good checklist, but not enough for vertical slice test execution or mobile budgets. | Add detailed QA/performance/soft-launch plan. | P0 |
| `12_CODEX_AGENT_GUIDE.md` | Codex workflow | Needs Update | Good boundaries; needs reference to root AGENTS and new sprint docs. | Update after adding new docs. | P0 |
| `13_ROADMAP.md` | High-level milestones | Needs Update | Too coarse for Codex-driven execution. | Add detailed sprint plan and backlog. | P0 |
| `14_RISK_REGISTER.md` | Key risks | Good | Needs production tracking fields later: owner, impact, likelihood, trigger, mitigation status. | Keep now; expand when implementation begins. | P2 |
| `15_STORE_COMPLIANCE.md` | Store/privacy/compliance checklist | Needs Update | Good checklist, but no SDK readiness matrix. | Use monetization/compliance spec before SDK work. | P1 |
| `16_CODEX_PROMPTS.md` | Initial Codex prompts | Needs Update | Existing prompts combine some major scopes and do not fully require all validation/out-of-scope clauses. | Replace with first 10 no-MCP implementation prompts. | P0 |
| `17_CREDITS_TEMPLATE.md` | Asset/SDK credit template | Needs Update | Says no production assets despite imported packs being present. | Add current staged asset inventory and license risk note. | P0 |

## 4. Coverage Audit

| Area | Coverage | Notes |
|---|---|---|
| Product vision | Good | Clear production direction and dash-impact identity. |
| Market positioning | Partial | Audience and commercial model exist; competitor-safe positioning and KPI plan needed. |
| Original differentiation from Archero | Good | Explicit no-copy rules and dash-impact differentiation. |
| Core game loop | Good | Run loop is clear. |
| Combat loop | Good | Stop-to-attack and dash are defined. |
| Dash-impact identity | Good | Strong concept; needs implementation acceptance criteria. |
| Auto-attack/stop-to-attack | Good | Needs targeting and movement edge-case contracts. |
| Controls | Partial | Needs test plan for touch, safe area, and A/B variants. |
| Ability system | Partial | Needs exact catalog, tags, stacking, and config schema. |
| Level/room/wave system | Partial | Needs prefab contracts and first chapter production data. |
| Boss system | Partial | Boss placeholder exists; needs attack contract and QA cases. |
| Enemy taxonomy | Partial | Needs catalog and reusable behavior contracts. |
| Meta progression | Partial | Directional; needs first economy tables. |
| Gear system | Partial | Slots/rarity exist; needs data schema and upgrade cost model. |
| Talents | Partial | Needs unlock/cost schema. |
| Currency economy | Partial | Needs sources/sinks sheet schema and economy gates. |
| Reward tables | Partial | Config names exist; actual schema needed. |
| Monetization strategy | Partial | Ethical philosophy and placements exist; config and compliance gates needed. |
| Ads/IAP/shop | Partial | Stubs only; correct for now, but missing future SDK readiness checklist. |
| Daily rewards/missions | Partial | Listed; needs schema and UI acceptance. |
| LiveOps | Partial | Event examples exist; needs soft-launch/KPI plan. |
| Analytics taxonomy | Partial | Event names exist; parameter schema needed. |
| Remote config | Partial | Key ideas exist; typed key plan needed. |
| A/B testing | Partial | Test ideas exist; experiment schema needed. |
| QA | Partial | High-level checklist; needs vertical slice test plan. |
| Performance targets | Partial | FPS/object targets exist; needs memory/build/device budgets. |
| Build/release pipeline | Partial | Store checklist exists; Git/release workflow missing. |
| Store/privacy/SDK compliance | Partial | Checklist exists; SDK gate details missing. |
| Asset/art/animation/audio/VFX pipeline | Partial | Direction exists; content intake process missing. |
| Licensing and credits | Needs Update | Imported staged assets need license inventory. |
| Codex workflow and no-MCP workflow | Good | Strong boundaries; root AGENTS now improves safety. |
| Folder structure | Good target, missing implementation | Target structure exists, repo does not match. |
| Scene architecture | Partial | Scene responsibilities exist; scene/prefab contracts missing. |
| ScriptableObject config architecture | Partial | Config names exist; schema missing. |
| Object pooling | Partial | Principle exists; budgets/contracts missing. |
| Save system | Partial | Service name exists; schema not defined. |
| Soft launch roadmap | Partial | High level only; KPI plan needed. |
| Risk register | Good | Needs operational tracking when production starts. |

## 5. Immediate Risks

1. The repository has no Git history, so production work has no rollback path until Git is initialized.
2. The Unity project still contains default template settings such as `DefaultCompany` and the default Android application identifier.
3. The desired production folder structure is not present, so implementation tasks could scatter code and assets if not governed.
4. Only `SampleScene` exists; future scene work must use approved Editor tooling or manual Unity steps, not direct YAML edits.
5. Existing assets are staged in a non-production path and one pack has no discovered license file.
6. Current docs are broad enough for direction but not detailed enough for safe task-by-task Codex implementation without the new sprint/spec docs.

