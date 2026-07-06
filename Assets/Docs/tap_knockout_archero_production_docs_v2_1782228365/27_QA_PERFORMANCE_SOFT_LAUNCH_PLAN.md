# QA, Performance, and Demo Plan

## Vertical Slice QA Matrix

| Area | Test | Pass Criteria |
|---|---|---|
| Launch | Desktop build starts | No crash, main screen usable. |
| Movement | WASD movement | Player responds immediately and does not drift. |
| Aim | Mouse aim | Attacks/aim indicators face intended world point. |
| Dash | Dash/evade | Cooldown, direction, and i-frame policy behave as designed. |
| Active skills | Hotkeys | Q/E/R/F or number keys trigger correct skills. |
| XP | Collect XP orbs | XP bar updates and level-up triggers. |
| Level-up | Select choice | Combat pauses/resumes safely, effect applies. |
| Spawn | Wave timeline | Enemies spawn according to timeline/budget. |
| Elite | Elite milestone | Warning, spawn, kill, reward, analytics hook. |
| Boss | Boss milestone | Warning, HP bar, readable attacks, defeat result. |
| Death | Player dies | Run result appears and state cleans up. |
| Win | Boss defeated | Win result appears. |
| Pause | Pause/resume | Input and timers resume correctly. |
| Readability | Dense combat | Threats, pickups, and telegraphs remain understandable. |

## Performance Budget

| Metric | Target | Gate |
|---|---|---|
| FPS | 60 | Mid-range PC target. |
| Enemy stress | 100+ simple enemies | No catastrophic frame drop. |
| Projectiles | Pooled | Stable under active combat. |
| XP orbs | Pooled | No major collection spike. |
| VFX | Budgeted | Does not hide boss/elite attacks. |
| GC | Low steady state | No recurring major combat spikes. |
| Run duration | 10 minutes | Completes without state break. |

## Stress Tests

- 100 melee enemies chasing.
- 100 enemies plus ranged projectiles.
- 100 enemies plus XP drops.
- Boss with adds and telegraphs.
- Level-up during heavy spawn pressure.
- Long run with repeated pooling cycles.

## Desktop Demo Gate

Before external sharing:

- Controls are clear.
- Settings are minimally usable.
- Known issues are documented.
- No unlicensed assets are used.
- No unapproved SDKs are present.
- Performance report exists.
- Result screen handles win/loss.
- Boss is readable.
- Level-up cannot break combat state.

## Playtest KPI Plan

Initial KPIs:

- First run duration.
- Level reached.
- Boss reach rate.
- Boss defeat rate.
- Death cause.
- Ability pick distribution.
- Active skill uses per minute.
- Dash uses per minute.
- Enemy kills per minute.
- Average FPS or frame-time sample if available.

## Legacy Mobile Gates

Android device matrix, portrait safe area, rewarded ad failure path, mobile soft-launch KPI, and mobile store compliance are future port topics only.
