# Monetization, Analytics, and Remote Config Spec

## Rules

- No real SDKs before explicit approval.
- Gameplay code must call interfaces only.
- Events must not include personally identifiable information.
- Remote config must have local defaults so the game works offline.
- Monetization accelerates or adds optional rewards; it must not block basic progress in the vertical slice.

## Service Interfaces

Required abstractions:

- `IAnalyticsService`
- `IRemoteConfigService`
- `IAdService`
- `IIapService`

Initial implementations:

- `ConsoleAnalyticsService`
- `LocalRemoteConfigService`
- `FakeAdService`
- `FakeIapService`

## Analytics Event Schema

Common parameters:

| Parameter | Type | Required | Notes |
|---|---|---|---|
| `session_id` | string | Yes | Local/generated session id. |
| `run_id` | string | Run events | New id per chapter attempt. |
| `chapter_id` | string | Chapter events | Example `chapter_001`. |
| `room_id` | string | Room events | Stable room id or generated sequence id. |
| `room_index` | int | Room events | 1-based. |
| `player_level` | int | Relevant events | Runtime level in run. |
| `power_score` | int | Relevant events | Later calculated from gear/talents. |
| `source` | string | Optional | Reward, damage, purchase, or UI source. |

Core events:

| Event | Trigger | Required Parameters |
|---|---|---|
| `session_start` | App/session start | `session_id`, app version. |
| `ftue_start` | First tutorial begins | `session_id`. |
| `ftue_step_complete` | Tutorial step done | `step_id`, `step_index`. |
| `chapter_start` | Run starts | `run_id`, `chapter_id`. |
| `room_start` | Room begins | `run_id`, `chapter_id`, `room_id`, `room_index`, `room_type`. |
| `room_complete` | Room cleared | room params, `duration_seconds`, `damage_taken`, `enemies_killed`. |
| `ability_offered` | Ability choices shown | `run_id`, `room_index`, offered ability ids. |
| `ability_selected` | Player selects ability | `ability_id`, `rarity`, `stack_count`. |
| `dash_used` | Dash starts | `run_id`, `room_index`, `cooldown_remaining_before`. |
| `dash_hit` | Dash hits target | `enemy_id`, `damage`, `knockback_applied`, `ability_source`. |
| `player_death` | Player dies | `run_id`, `chapter_id`, `room_index`, `killer_id`. |
| `boss_start` | Boss room starts | `boss_id`, `chapter_id`. |
| `boss_defeated` | Boss defeated | `boss_id`, `duration_seconds`. |
| `run_end` | Run ends | `result`, `rooms_cleared`, `duration_seconds`, rewards. |
| `reward_granted` | Currency/item granted | `reward_type`, `reward_id`, `amount`, `source`. |
| `gear_upgrade` | Gear upgraded | `item_id`, `slot`, `old_level`, `new_level`, `cost`. |
| `talent_upgrade` | Talent upgraded | `talent_id`, `old_level`, `new_level`, `cost`. |
| `rewarded_ad_offer` | Rewarded placement shown | `placement_id`, `reward_preview`. |
| `rewarded_ad_complete` | Rewarded ad result | `placement_id`, `result`, `reward_granted`. |
| `iap_offer_shown` | IAP offer displayed | `product_id`, `placement_id`. |
| `purchase_attempt` | Purchase started | `product_id`. |
| `purchase_success` | Purchase completed | `product_id`, `price_local`, `currency_code`. |
| `remote_config_loaded` | Config provider loaded | `source`, `config_version`, `variant_ids`. |

## Remote Config Keys

| Key | Type | Default | Owner | Notes |
|---|---|---|---|---|
| `dash_cooldown` | float | `4.0` | Combat | Must not break dash feel. |
| `dash_distance` | float | `3.5` | Combat | Validate against room sizes. |
| `dash_impact_damage_multiplier` | float | `1.0` | Combat | Affects dash identity. |
| `enemy_health_multiplier` | float | `1.0` | Balance | Chapter-wide override. |
| `enemy_damage_multiplier` | float | `1.0` | Balance | Chapter-wide override. |
| `room_enemy_count_multiplier` | float | `1.0` | Balance | Must respect max alive budget. |
| `ability_weight_overrides` | map | empty | Design | Overrides ability offer weights. |
| `reward_coin_multiplier` | float | `1.0` | Economy | Tune early progression. |
| `rewarded_revive_enabled` | bool | `false` | Monetization | Fake service first. |
| `double_reward_ad_enabled` | bool | `false` | Monetization | Fake service first. |
| `free_chest_cooldown_seconds` | int | `86400` | Monetization | Daily retention. |
| `ability_reroll_ad_enabled` | bool | `false` | Monetization | Off until ability UI stable. |
| `starter_pack_enabled` | bool | `false` | Monetization | Requires IAP readiness. |
| `interstitial_enabled` | bool | `false` | Monetization | Off for FTUE and vertical slice. |
| `interstitial_frequency_runs` | int | `0` | Monetization | Ignored while disabled. |
| `ab_dash_control_variant` | string | `dash_button` | Product | Later A/B test. |
| `ab_attack_variant` | string | `stop_to_attack` | Product | Later A/B test. |

## Monetization Placements

| Placement | Vertical Slice | Soft Launch | Notes |
|---|---|---|---|
| Rewarded revive | Fake only | Candidate | Once per run, never automatic. |
| Double run rewards | Fake only | Candidate | After run result. |
| Free chest | Fake only | Candidate | Home screen. |
| Ability reroll | Deferred | Test cautiously | Can hurt balance readability. |
| Starter pack | Config only | Candidate | Requires IAP sandbox and compliance. |
| No Ads | Config only | Later | Requires real ads product logic. |
| Interstitial | Off | Later | Never during combat; not before retention validation. |

## SDK Readiness Gate

Real SDK integration requires:

- Privacy policy URL.
- Store data collection disclosure.
- Consent plan where legally required.
- Test mode setup.
- Age rating implications reviewed.
- Purchase restore plan for iOS later.
- Analytics event QA checklist.
- Rollback or feature-disable path through config.
- User approval for package installation.

