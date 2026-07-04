# QA, Performance, and Release

## Desktop Survivor QA Focus

The MVP must be tested as a desktop arena survivor, not a mobile room game.

Core QA areas:

- WASD movement responsiveness.
- Mouse aim correctness.
- Dash/evade timing and cooldown.
- Active skill hotkey reliability.
- Level-up interruption safety.
- XP pickup collection and magnet behavior.
- Wave timeline progression.
- Elite spawn and kill flow.
- Boss warning, boss health bar, boss defeat.
- Run win/loss result screen.
- Pause/resume.
- Performance under enemy density.

## Performance Targets

MVP desktop targets:

| Metric | Target |
|---|---|
| FPS | Stable 60 FPS on mid-range PC target. |
| Stress enemy count | 100+ simple enemies in controlled stress test. |
| Projectiles | Pooled and stable under combat load. |
| Pickups | Pooled XP/pickup spawning with no major spikes. |
| Combat GC | No major recurring GC spikes during waves. |
| Boss readability | Telegraphs readable with adds active. |
| Input latency | Movement and skill input feel immediate. |

Minimum acceptance can be relaxed during early prototype only if the issue is documented.

## Required Test Cases

- Start run and survive first minute.
- Collect XP and trigger level-up.
- Select each category of ability.
- Use every active skill.
- Spawn 100+ enemies in stress mode.
- Fire projectiles while 100+ enemies are active.
- Spawn many XP orbs and collect them with and without magnet.
- Trigger elite milestone.
- Trigger boss milestone.
- Defeat boss and reach result screen.
- Die and reach result screen.
- Pause during combat and after level-up.
- Verify no major console errors during a 10-minute run.

## Release Gates

Before sharing a Steam-facing demo build:

- No blocker or critical gameplay bugs.
- No direct `.unity` YAML hand edits without review.
- Build starts on target desktop platform.
- Resolution/fullscreen behavior is usable.
- Controls are documented in settings or UI.
- Performance stress test results are recorded.
- Third-party asset licenses are tracked.
- No unapproved SDKs are present.
- Known issues are documented.

## Legacy Gates

Android package id, portrait lock, safe-area notch checks, rewarded ad failure paths, and mobile store soft-launch gates are deprecated for the desktop MVP. They are future port work only.
