# Monetization, Analytics, and Remote Config Spec

## Rules

- No real SDKs before explicit approval.
- Gameplay code must call interfaces only.
- Events must not include personally identifiable information.
- Remote/local config must have defaults so the game works offline.
- Monetization is not an MVP driver for the desktop survivor prototype.

## Service Interfaces

Required or planned abstractions:

- `IAnalyticsService`
- `IRemoteConfigService`
- `ISaveService`
- `IAudioService`
- Optional future `IAdService`
- Optional future `IIapService`

Initial implementations:

- Console/no-op analytics.
- Local config.
- Local save.
- Audio stub if needed.

## Common Analytics Parameters

| Parameter | Type | Required | Notes |
|---|---|---|---|
| `session_id` | string | Yes | Local/generated session id. |
| `run_id` | string | Run events | New id per run. |
| `arena_id` | string | Run events | Example `arena_ruins_01`. |
| `run_time_seconds` | float | Run events | Current run time. |
| `player_level` | int | Relevant events | Runtime level. |
| `difficulty_scalar` | float | Balance events | Current difficulty value. |
| `source` | string | Optional | Damage, pickup, ability, UI, etc. |

## Core Events

| Event | Trigger | Required Parameters |
|---|---|---|
| `session_start` | App/session start | `session_id`, app version. |
| `run_start` | Arena run starts | `run_id`, `arena_id`, `run_config_id`. |
| `wave_reached` | Timeline segment begins | `run_id`, `wave_id`, `run_time_seconds`. |
| `enemy_killed` | Enemy dies | `enemy_id`, `run_time_seconds`, `source`. |
| `elite_spawned` | Elite milestone spawns | `elite_id`, `run_time_seconds`. |
| `elite_killed` | Elite dies | `elite_id`, `run_time_seconds`. |
| `pickup_collected` | Pickup collected | `pickup_id`, `pickup_type`, `amount`. |
| `xp_collected` | XP collected | `amount`, `player_level`. |
| `level_up` | Player levels up | `old_level`, `new_level`, `run_time_seconds`. |
| `ability_offered` | Choices shown | offered ability ids, rarity list. |
| `ability_selected` | Choice selected | `ability_id`, `rarity`, `stack_count`. |
| `active_skill_used` | Skill hotkey fires | `ability_id`, `cooldown_remaining_before`, `targeting_mode`. |
| `dash_used` | Dash starts | `cooldown_remaining_before`, `direction`. |
| `dash_hit` | Dash hits target | `enemy_id`, `damage`, `knockback_applied`. |
| `damage_taken` | Player takes damage | `amount`, `source`, `player_hp_after`. |
| `boss_spawned` | Boss milestone starts | `boss_id`, `run_time_seconds`. |
| `boss_defeated` | Boss defeated | `boss_id`, `duration_seconds`. |
| `player_death` | Player dies | `killer_id`, `run_time_seconds`, `player_level`. |
| `run_end` | Run ends | `result`, `duration_seconds`, `level_reached`, `boss_defeated`. |
| `remote_config_loaded` | Config provider loaded | `source`, `config_version`. |

## Remote Config Keys

| Key | Type | Default | Owner | Notes |
|---|---|---|---|---|
| `run_duration_seconds` | float | `600` | Design | MVP 10-minute run. |
| `spawn_rate_multiplier` | float | `1.0` | Balance | Must respect budget. |
| `max_alive_budget_multiplier` | float | `1.0` | Performance | Use with stress tests. |
| `enemy_health_multiplier` | float | `1.0` | Balance | Global tuning. |
| `enemy_damage_multiplier` | float | `1.0` | Balance | Global tuning. |
| `enemy_speed_multiplier` | float | `1.0` | Balance | Readability risk. |
| `xp_curve_multiplier` | float | `1.0` | Balance | Affects level cadence. |
| `ability_weight_overrides` | map | empty | Design | Offer tuning. |
| `boss_spawn_time_seconds` | float | `540` | Design | Warning should lead this. |
| `elite_spawn_times` | list | `[180,360]` | Design | MVP milestones. |
| `pickup_magnet_base_radius` | float | `2.0` | Combat | Tune feel. |
| `active_skill_cooldown_multiplier` | float | `1.0` | Combat | Global skill tuning. |
| `dash_cooldown` | float | `4.0` | Combat | Dash feel. |
| `dash_distance` | float | `3.5` | Combat | Avoid arena breaks. |
| `damage_numbers_enabled` | bool | `true` | UX | Performance/readability. |

## Future Monetization

Future PC-friendly options:

- Premium full game.
- Demo-to-full upgrade.
- Cosmetic DLC.
- Supporter pack.
- Expansion content.

Legacy mobile options such as rewarded ads, interstitials, free chests, gem packs, and daily reward monetization are not MVP requirements.

## SDK Readiness Gate

Real SDK integration requires:

- User approval.
- Privacy policy.
- Provider selection.
- Data collection disclosure.
- Test mode plan.
- Rollback path.
- Platform compliance review.
- Event QA checklist.
