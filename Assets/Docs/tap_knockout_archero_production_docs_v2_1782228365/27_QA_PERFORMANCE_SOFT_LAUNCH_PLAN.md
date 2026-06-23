# QA, Performance, and Soft-Launch Plan

## Vertical Slice QA Matrix

| Area | Test | Pass Criteria |
|---|---|---|
| Launch | App starts on Android | No crash, portrait orientation, first screen usable. |
| Movement | Drag movement | Player follows input, stops cleanly, no stuck states. |
| Stop-to-attack | Stand still near enemy | Attack fires on cooldown at valid target. |
| Targeting | Multiple enemies | Nearest/priority target is selected consistently. |
| Dash | Press dash while moving | Player dashes in intended direction, cooldown starts. |
| Dash impact | Dash through enemy | Enemy takes impact damage and knockback once per dash. |
| Enemy melee | Melee chaser attacks | Player takes damage only when in range/timing. |
| Enemy ranged | Shooter fires | Projectile is readable and can be dodged. |
| Enemy charger | Charger telegraphs | Player can dodge or interrupt/knockback where intended. |
| Room clear | Defeat all enemies | Room complete event fires and next transition appears. |
| Ability offer | Ability room/trigger | 3 valid cards appear; gameplay pauses. |
| Ability select | Choose card | Effect applies, panel closes, gameplay resumes. |
| Boss | Enter boss room | Boss attacks, HP displays, defeat ends chapter. |
| Death | Player HP reaches zero | Run end or revive flow appears correctly. |
| Rewards | Complete/fail run | Rewards match tables and persist. |
| Save/load | Restart app | Currencies/progress persist or recover safely. |
| UI safe area | Notch/cutout device | Buttons and critical HUD are not clipped. |
| Pause/resume | App background/foreground | Game does not lose control, save, or audio state. |
| Fake ads | Success/cancel/fail | Correct reward/no reward and analytics event. |
| Fake IAP | Success/fail | Correct product path and no real purchase. |

## Performance Budget

| Metric | Target | Minimum Gate |
|---|---|---|
| FPS | 60 | 30 on lower-end Android test device. |
| Max enemies alive | 25 | Must not cause severe frame drops. |
| Max projectiles alive | 80 | Pooling required before higher counts. |
| Max active VFX | 20 | Combat readability first. |
| Combat GC allocations | Near zero steady state | No major recurring spikes during combat. |
| Load to gameplay | Under 10 seconds for debug target | Track and improve. |
| App memory | Device-dependent | No runaway growth over 5-minute session. |
| Build size | Under 150 MB early target | Review after asset pass. |

## Android Build Gate

Required before a vertical slice build is accepted:

- Android package id is no longer Unity template default.
- Company/product names are reviewed.
- Portrait orientation is locked or explicitly configured.
- ARM64/IL2CPP release path is planned.
- Debug build runs on physical Android device.
- Touch input works.
- Safe area works.
- No critical console errors in 5-minute session.
- Build output is not committed to source control.

## Test Devices

Minimum device matrix:

- One mid-range Android phone.
- One lower-end Android phone or emulator profile.
- One tall/notched Android screen.
- Unity Editor play mode for rapid smoke tests.

Later:

- iPhone portrait device or simulator after iOS path begins.

## Issue Severity

| Severity | Meaning |
|---|---|
| Blocker | Prevents launch, build, core run, save, or compliance gate. |
| Critical | Major gameplay/data loss/crash issue with common path. |
| Major | Noticeable gameplay, UI, performance, or balance issue. |
| Minor | Polish issue or rare edge case. |
| Trivial | Cosmetic/documentation follow-up. |

## Soft-Launch KPI Plan

Initial KPI targets are placeholders and must be refined after playtests:

| KPI | Target Direction | Notes |
|---|---|---|
| D1 retention | Improve above baseline for genre | Requires real analytics later. |
| FTUE completion | High completion | Track each tutorial step. |
| First run completion/fail split | Balanced | Too easy or too hard both hurt learning. |
| Ability selection engagement | Near universal after offer | Low rate means UI/confusion issue. |
| Dash usage per room | Meaningful use | Confirms identity is understood. |
| Dash hit rate | Meaningful but not mandatory | Too low means mechanic is unclear/risky. |
| Chapter 1 boss reach rate | Healthy funnel | Tune room difficulty. |
| Rewarded ad opt-in | Later | Only after fake flow and compliance. |
| Crash-free sessions | Very high | Must be tracked before public launch. |
| Average FPS | Stable | Must be segmented by device tier. |

## Release Candidate Checklist

- All P0 vertical slice tests pass.
- Known issues list has no blocker/critical unresolved items.
- Asset licenses documented.
- External SDKs either absent or approved/compliant.
- Build settings reviewed.
- Version number and build number set.
- Rollback branch/tag exists after Git initialization.

