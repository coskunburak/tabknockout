# Analytics, Remote Config, and A/B Testing

## Event Naming

Use lowercase snake_case.

## Core Events

- session_start
- ftue_start
- ftue_step_complete
- chapter_start
- room_start
- room_complete
- ability_offered
- ability_selected
- player_death
- run_end
- gear_upgrade
- talent_upgrade
- rewarded_ad_offer
- rewarded_ad_complete
- iap_offer_shown
- purchase_attempt
- purchase_success

## Funnels

FTUE:

```text
ftue_start
movement_complete
first_enemy_killed
first_room_complete
first_ability_selected
first_run_end
first_upgrade_complete
```

Run:

```text
chapter_start
room_start
room_complete
ability_selected
boss_start
boss_defeated
chapter_complete
run_end
```

## Remote Config Values

- enemy_health
- enemy_damage
- room_enemy_count
- ability_weights
- reward_amounts
- dash_cooldown
- ad_placements_enabled
- starter_pack_enabled

## A/B Tests

Controls:

- Dash button vs double tap
- Stop-to-attack vs moving attack

Difficulty:

- Chapter 1 health/damage
- Boss HP
- Ability frequency

Monetization:

- Revive ad timing
- Starter pack placement
- Reward multiplier

## Architecture

Use:

```csharp
IAnalyticsService.TrackEvent(...)
IRemoteConfigService.GetValue(...)
```

No direct SDK calls from gameplay code.
