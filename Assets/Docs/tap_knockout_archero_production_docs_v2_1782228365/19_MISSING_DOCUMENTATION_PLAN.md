# Missing or Weak Documentation Plan

This document records which documentation was missing or weak after repository discovery and which new documents were added to make future Codex implementation safer.

## Required Now

| Document | Why Needed | Contents | Priority |
|---|---|---|---|
| `18_REPOSITORY_DISCOVERY_AND_DOCUMENTATION_AUDIT.md` | Keeps discovery facts and documentation audit in the repo instead of only in a chat response. | Unity/root facts, package summary, docs audit table, coverage gaps, immediate risks. | P0 |
| `19_MISSING_DOCUMENTATION_PLAN.md` | Prevents random doc creation and explains why each new doc exists. | Missing docs, consolidation decisions, now/later priority. | P0 |
| `20_BACKLOG_MASTER.md` | The roadmap is too coarse for execution. | Epics, task groups, priorities, acceptance criteria, dependencies. | P0 |
| `21_VERTICAL_SLICE_SPEC.md` | The first playable target needs fixed scope and acceptance criteria. | Chapter 1 slice, player/enemy/room/ability/meta/UI/analytics/QA scope and out-of-scope. | P0 |
| `22_PRODUCTION_SPRINT_PLAN.md` | User requested a very detailed milestone/sprint plan. | Milestones, sprints, dependencies, tasks by discipline, acceptance, DoD, manual Unity steps, branches, commits, prompts. | P0 |
| `23_TECHNICAL_DECISIONS_ADR.md` | Future agents need durable decisions and rationale. | ADR log for no-MCP workflow, scene editing policy, data-driven architecture, SDK gates. | P0 |
| `24_DATA_CONFIG_SCHEMA.md` | ScriptableObject names exist but not field contracts. | IDs, config types, required fields, validation rules, save data, experiment config. | P0 |
| `25_PREFAB_AND_SCENE_CONTRACTS.md` | Scene and prefab work is risky without Unity MCP. | Boot/Home/Gameplay responsibilities, prefab required components, manual Unity setup, Editor builder expectations. | P0 |
| `26_MONETIZATION_ANALYTICS_REMOTE_CONFIG_SPEC.md` | Analytics/remote config docs are directional only. | Event schema, parameter rules, remote config keys, monetization config, SDK readiness gates. | P0 |
| `27_QA_PERFORMANCE_SOFT_LAUNCH_PLAN.md` | Existing QA doc is too short for production gates. | Vertical slice test matrix, device/performance budgets, Android build gates, soft-launch KPI gates. | P0 |
| `28_CONTENT_PIPELINE_BALANCING_EDITOR_TOOLS.md` | Assets, balance sheets, and tooling need process controls before content work. | Asset intake, license review, folder migration, balancing spreadsheet tabs, Editor tools plan. | P1 |
| `29_RELEASE_BRANCHING_AND_GIT_WORKFLOW.md` | Repo is not a Git repo and future work needs rollback and PR hygiene. | Git init recommendation, branches, commits, release channels, versioning, review checklist. | P0 |
| `30_ABILITY_AND_ENEMY_DESIGN_CATALOG.md` | Combat docs list examples but not production-ready catalogs. | First ability pool, enemy taxonomy, boss placeholder, tuning fields, acceptance criteria. | P0 |

## Required Later

| Document | Why Later | Trigger |
|---|---|---|
| Store listing copy and creative brief | Needs final art direction, screenshots, icon, and gameplay footage. | After vertical slice art pass. |
| Privacy policy implementation matrix | Depends on selected analytics, ads, IAP, crash, and consent SDKs. | Before real SDK integration. |
| Localization plan | Not needed before English/Turkish internal production text stabilizes. | Before soft launch if target territories require it. |
| LiveOps calendar | Needs production content volume and economy baselines. | After soft-launch candidate. |
| Customer support and player safety policy | Needed near launch, not for current foundation. | Before public testing. |

## Consolidation Decisions

The suggested list in the request was not copied one-file-per-item. Some areas were consolidated to reduce duplicate sources of truth:

- Data config, remote config, and analytics are split into `24_DATA_CONFIG_SCHEMA.md` and `26_MONETIZATION_ANALYTICS_REMOTE_CONFIG_SPEC.md`.
- Performance, vertical slice test plan, and soft-launch KPI planning are consolidated in `27_QA_PERFORMANCE_SOFT_LAUNCH_PLAN.md`.
- Content pipeline, balancing spreadsheet spec, and Editor tools plan are consolidated in `28_CONTENT_PIPELINE_BALANCING_EDITOR_TOOLS.md`.
- Ability and enemy catalogs are combined in `30_ABILITY_AND_ENEMY_DESIGN_CATALOG.md` because they share combat tags, roles, tuning, and vertical slice dependencies.

