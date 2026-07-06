# Analytics, Remote Config, and A/B Testing

## Analytics Position

Analytics are planned through interfaces first. No real SDK is approved by default. Console/local analytics are enough until the prototype proves the loop.

Event names use lowercase snake_case and must not include personal data.

## Core Run Events

Required run-based events:

- `session_start`
- `run_start`
- `run_end`
- `death_time`
- `wave_reached`
- `boss_spawned`
- `boss_defeated`
- `level_up`
- `ability_offered`
- `ability_selected`
- `active_skill_used`
- `dash_used`
- `dash_hit`
- `damage_taken`
- `enemy_killed`
- `elite_spawned`
- `elite_killed`
- `pickup_collected`
- `xp_collected`
- `player_death`

## Common Parameters

- `session_id`
- `run_id`
- `arena_id`
- `run_time_seconds`
- `player_level`
- `ability_ids`
- `enemy_id`
- `boss_id`
- `wave_id`
- `difficulty_scalar`
- `source`
- `result`

## Funnels

First run funnel:

```text
session_start
run_start
first_enemy_killed
first_pickup_collected
first_level_up
first_ability_selected
first_active_skill_used
first_elite_spawned
first_boss_spawned
run_end
```

Run performance funnel:

```text
run_start
wave_reached
level_up
elite_killed
boss_spawned
boss_defeated
run_end
```

## Remote Config

Remote/local config should support:

- Enemy spawn rates.
- Wave timing.
- XP curve.
- Ability weights.
- Boss timing.
- Difficulty multipliers.
- Enemy health and damage multipliers.
- Pickup magnet base values.
- Active skill cooldown multipliers.
- Elite spawn timing.

Local defaults must always exist so the game works offline.

## A/B Tests

Candidate tests after core feel is stable:

- Auto-fire vs hold-to-fire.
- Dash on Space vs Shift.
- Boss spawn at 8, 9, or 10 minutes.
- XP curve speed.
- Ability offer rarity mix.
- Enemy density vs enemy toughness.

No monetization A/B tests are planned for MVP.

## Architecture

Gameplay code should use:

```csharp
IAnalyticsService.TrackEvent(...)
IRemoteConfigService.GetValue(...)
```

No direct vendor SDK calls from gameplay code.
