# Arena, Run, and Wave Design

## Canonical Structure

The game is arena-first and wave/timer-driven. The main gameplay unit is a run inside an arena. Rooms are legacy or optional future content modules.

## Run Timer

The MVP run target is 10 minutes. A future full run may extend to 15 minutes.

The run timer drives:

- Spawn intensity.
- Enemy type unlocks.
- Elite milestones.
- Boss warning and spawn.
- Drop pacing.
- Difficulty multipliers.
- Result conditions.

## Wave Timeline

`WaveTimelineConfig` should describe timed segments:

| Segment | Example Time | Purpose |
|---|---|---|
| Intro | 0:00-1:00 | Basic enemies, low pressure, teach movement. |
| Pressure 1 | 1:00-3:00 | Add swarm and ranged enemies. |
| Elite 1 | 3:00 | Spawn first elite or elite pack. |
| Pressure 2 | 3:00-6:00 | Add charger/tank mix and higher counts. |
| Elite 2 | 6:00 | Stronger elite milestone. |
| Boss Warning | 8:30-9:00 | UI/audio/VFX warning, pressure adjustment. |
| Boss | 9:00-10:00 | Boss encounter with adds or hazards. |

The exact times are tuning targets, not verified implementation.

## Spawn Director

The spawn system should support:

- Spawn rings around the player.
- Arena edge spawn zones.
- Anti-spawn safety radius near the player.
- Line-of-sight or camera-aware constraints if needed.
- Max alive budget.
- Spawn budget by enemy cost.
- Burst spawns and trickle spawns.
- Elite and boss overrides.

## Enemy Budget

Each enemy archetype should have a budget cost:

- Swarm: low cost.
- Basic melee: low/medium cost.
- Ranged: medium cost.
- Charger: medium/high cost.
- Tank: high cost.
- Elite: milestone budget.
- Boss: encounter budget.

The director should limit total live enemy cost, not only raw count.

## Intensity Scaling

Difficulty can scale through:

- Spawn rate.
- Max alive budget.
- Enemy health multiplier.
- Enemy damage multiplier.
- Enemy speed multiplier.
- Ranged enemy ratio.
- Elite frequency.
- Boss add frequency.

Scaling must be capped by performance and readability gates.

## Rewards and Drops

Drop pacing should prioritize run feel:

- XP orbs from enemy deaths.
- Pickup magnet upgrades.
- Health pickups sparingly.
- Gold/material drops only if meta progression is enabled.
- Boss reward bundle at run completion.

XP curves should be tuned so early levels arrive quickly, mid-run levels slow slightly, and the final boss window still offers meaningful decisions.

## Elite Milestones

Elite milestones should create short spikes without derailing the run:

- Warning cue.
- Distinct silhouette or color grade.
- Higher HP and a clear modifier.
- Better XP/drop reward.
- Analytics event on spawn and kill.

## Boss Milestone

The boss is the major run climax:

- Spawn after warning.
- Bind boss health bar.
- Reduce or reshape regular spawns if needed.
- Use clear telegraphs.
- End run on boss defeat for MVP.

## Legacy Room Handling

Room templates and room managers may remain in code as legacy or future challenge-mode infrastructure. They should not drive the MVP desktop survivor loop unless explicitly migrated into arena modules.
