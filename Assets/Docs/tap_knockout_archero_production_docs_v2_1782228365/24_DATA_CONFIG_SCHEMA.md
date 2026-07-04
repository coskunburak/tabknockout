# Data and Config Schema

## Global Rules

- Every gameplay/config asset has a stable `id`.
- IDs use lowercase snake_case, for example `arena_ruins_01`, `wave_intro_01`, `skill_arc_blast`.
- Display names are not IDs.
- Runtime code should not depend on scene object names for gameplay identity.
- Configs should have validation through tests, Editor tools, or manual checklist.
- Use `planned` or `target` for fields not verified in code.

## Common Fields

| Field | Type | Required | Notes |
|---|---|---|---|
| `id` | string | Yes | Stable analytics/save/config key. |
| `displayName` | string | Player-facing content | Can be localized later. |
| `description` | string | Optional | Short player-facing text or designer note. |
| `tags` | list | Optional | Used for ability filters, enemy roles, reward tables. |
| `version` | int | Optional | Useful for saves and config migration. |

## ArenaConfig

Required fields:

- `id`
- `arenaPrefab`
- `bounds`
- `cameraBounds`
- `playerSpawn`
- `spawnMode`
- `spawnRingMinRadius`
- `spawnRingMaxRadius`
- `antiSpawnSafetyRadius`
- `pickupBounds`
- `lightingProfile`

Validation:

- Spawn ring must not place enemies inside safety radius.
- Camera bounds must contain playable bounds.
- Arena must support boss spawn and 100+ enemy stress test target.

## RunConfig

Required fields:

- `id`
- `arenaConfig`
- `durationSeconds`
- `startingPlayerLevel`
- `xpCurve`
- `waveTimeline`
- `bossEncounter`
- `resultRules`
- `startingAbilityPool`

Validation:

- MVP target duration is 600 seconds.
- Boss timing must fit inside run duration.
- Result rules must define win and loss.

## WaveTimelineConfig

Required fields:

- `id`
- `segments`
- `eliteMilestones`
- `bossWarningTime`
- `difficultyCurve`

Segment fields:

- `startTime`
- `endTime`
- `spawnGroups`
- `maxAliveBudget`
- `spawnInterval`
- `intensityMultiplier`

Validation:

- Segments must not overlap incorrectly.
- Spawn budget must respect performance targets.

## SpawnGroupConfig

Required fields:

- `id`
- `enemyArchetype`
- `count`
- `spawnPattern`
- `spawnInterval`
- `budgetCost`
- `weight`
- `minRunTime`
- `maxRunTime`

Validation:

- Enemy references must be valid.
- Count and budget cannot exceed timeline limits.

## EnemyArchetypeConfig

Required fields:

- `id`
- `role`
- `prefab`
- `maxHp`
- `contactDamage`
- `moveSpeed`
- `attackRange`
- `attackCooldown`
- `xpReward`
- `budgetCost`
- `behaviorType`

Roles:

- `swarm`
- `melee`
- `ranged`
- `charger`
- `tank`
- `elite`
- `boss_add`

## EliteConfig

Required fields:

- `id`
- `baseEnemyArchetype`
- `hpMultiplier`
- `damageMultiplier`
- `speedMultiplier`
- `modifierTags`
- `warningText`
- `xpRewardMultiplier`
- `dropTable`

Validation:

- Elite must be visually distinguishable.
- Elite spawn must emit analytics.

## BossEncounterConfig

Required fields:

- `id`
- `bossPrefab`
- `spawnTime`
- `warningLeadTime`
- `healthBarName`
- `phaseDefinitions`
- `addSpawnGroups`
- `defeatResult`

Validation:

- Boss has warning, health bar, telegraphs, defeat event.
- Boss cannot spawn before required systems are initialized.

## AbilityConfig

Required fields:

- `id`
- `displayName`
- `description`
- `category`
- `rarity`
- `tags`
- `maxStacks`
- `weight`
- `exclusionGroup`
- `effectType`
- `effectValues`
- `cooldown`
- `duration`
- `icon`

Categories:

- `active`
- `passive`
- `weapon_modifier`
- `projectile_modifier`
- `dash_modifier`
- `area_damage`
- `defense`
- `pickup`

## AbilityUpgradeConfig

Required fields:

- `id`
- `abilityId`
- `stackIndex`
- `modifiedValues`
- `descriptionOverride`

Validation:

- Stack index cannot exceed `AbilityConfig.maxStacks`.
- Upgrade values must be reportable by balance tools.

## XPDropConfig

Required fields:

- `id`
- `xpAmount`
- `orbPrefab`
- `dropChance`
- `magnetRadius`
- `lifetimeSeconds`
- `mergeRules`

Validation:

- XP orbs must be pool-compatible.
- Pickup count must not exceed performance budget.

## DifficultyCurveConfig

Required fields:

- `id`
- `timeToMultiplier`
- `enemyHealthMultiplier`
- `enemyDamageMultiplier`
- `spawnRateMultiplier`
- `eliteFrequencyMultiplier`
- `xpMultiplier`

Validation:

- Multipliers need safe min/max.
- Curves must be visible in balancing tools.

## Save Data

Planned fields:

- `schemaVersion`
- `playerIdLocal`
- `unlockedCharacters`
- `unlockedAbilities`
- `metaUpgrades`
- `settings`
- `bestRuns`

No monetization or premium currency save data is required for MVP.
