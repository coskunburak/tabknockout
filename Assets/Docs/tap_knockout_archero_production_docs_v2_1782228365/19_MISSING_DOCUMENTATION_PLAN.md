# Missing or Weak Documentation Plan

## Purpose

This document tracks documentation coverage after the desktop survivor pivot. The goal is to keep future implementation work from falling back to old mobile/room assumptions.

## Required Now

| Document | Purpose | Status |
|---|---|---|
| `00_README_INDEX.md` | Canonical entry point and migration note. | Updated. |
| `01_PRODUCT_VISION.md` | Desktop survivor vision and MVP goal. | Updated. |
| `02_GDD_PRODUCTION.md` | Arena run design. | Updated. |
| `03_TECH_ARCHITECTURE_UNITY.md` | Desktop survivor runtime architecture. | Updated. |
| `04_COMBAT_AND_ABILITIES.md` | Active skills and survivor build rules. | Updated. |
| `05_LEVEL_ROOM_WAVE.md` | Arena/run/wave timeline and spawn pressure. | Updated. |
| `06_META_ECONOMY.md` | Demo-friendly progression. | Updated. |
| `07_MONETIZATION_LIVEOPS.md` | Future optional monetization/liveops. | Updated. |
| `08_UI_UX_CONTROLS.md` | Desktop controls and HUD. | Updated. |
| `09_ASSET_PIPELINE.md` | 3D survivor asset needs. | Updated. |
| `10_ANALYTICS_REMOTE_CONFIG.md` | Run-based telemetry and tuning keys. | Updated. |
| `11_QA_PERFORMANCE_RELEASE.md` | Desktop survivor QA/performance. | Updated. |
| `20_BACKLOG_MASTER.md` | Survivor backlog groups. | Updated. |
| `21_VERTICAL_SLICE_SPEC.md` | 10-minute arena MVP. | Updated. |
| `22_PRODUCTION_SPRINT_PLAN.md` | First pivot sprint. | Updated. |
| `23_TECHNICAL_DECISIONS_ADR.md` | Pivot architecture decisions. | Updated. |
| `24_DATA_CONFIG_SCHEMA.md` | Arena survivor config schema. | Updated. |
| `25_PREFAB_AND_SCENE_CONTRACTS.md` | Prototype scene and prefab contracts. | Updated. |
| `26_MONETIZATION_ANALYTICS_REMOTE_CONFIG_SPEC.md` | Detailed analytics/config/future monetization. | Updated. |
| `27_QA_PERFORMANCE_SOFT_LAUNCH_PLAN.md` | Desktop demo QA plan. | Updated. |
| `28_CONTENT_PIPELINE_BALANCING_EDITOR_TOOLS.md` | Survivor balancing tools. | Updated. |
| `30_ABILITY_AND_ENEMY_DESIGN_CATALOG.md` | Survivor ability/enemy catalog. | Updated. |
| `31_DESKTOP_SURVIVOR_PIVOT_PLAN.md` | Migration summary and phased plan. | Added. |

## Required Later

| Future Doc | Trigger |
|---|---|
| Steam page creative brief | After vertical slice has representative visuals. |
| Keybinding/accessibility spec | Before external playtest. |
| Full balance spreadsheet | After first playable run. |
| Save data migration plan | Before persistent meta progression expands. |
| Localization plan | Before public release. |
| Privacy policy matrix | Before real analytics/crash/SDK integration. |

## Consolidation Decision

The documentation keeps one source of truth per system area. Avoid adding new docs when an existing doc can be updated clearly.
