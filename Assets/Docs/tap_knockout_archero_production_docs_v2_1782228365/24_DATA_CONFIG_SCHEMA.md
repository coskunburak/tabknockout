# Data and Config Schema

## Global Config Rules

- Every gameplay/config asset has a stable `id`.
- IDs use lowercase snake_case, for example `dash_shockwave`, `enemy_melee_chaser`, `chapter_001`.
- Display names are not IDs and may be localized later.
- Configs should have validation paths through tests, editor validation, or manual checklist.
- Runtime code should not depend on scene object names for gameplay identity.

## Common Fields

| Field | Type | Required | Notes |
|---|---|---|---|
| `id` | string | Yes | Stable analytics/save/config key. |
| `displayName` | string | Yes for player-facing content | Can be replaced by localization key later. |
| `description` | string | Optional | Player-facing short text or designer note. |
| `tags` | string/list enum | Optional | Use for ability filters, enemy roles, reward tables. |
| `version` | int | Optional | Required for save, economy, and remote config payloads. |

## PlayerConfig

Required fields:

- `maxHp`
- `moveSpeed`
- `rotationSpeed`
- `targetingRadius`
- `stopToAttackMoveThreshold`
- `baseAttackDamage`
- `baseAttackCooldown`
- `dashCooldown`
- `dashDuration`
- `dashDistance`
- `dashIFrameSeconds`
- `dashImpactDamage`
- `dashKnockbackForce`

Validation:

- Numeric values must be positive unless documented.
- Dash duration must be shorter than dash cooldown.
- Targeting radius must be large enough for room combat.

## WeaponConfig

Required fields:

- `id`
- `displayName`
- `weaponType`
- `damageMultiplier`
- `attackCooldown`
- `range`
- `projectileSpeed`
- `projectilePrefab`
- `maxProjectiles`
- `canPierce`
- `basePierceCount`

Validation:

- Projectile prefab must match projectile prefab contract.
- Attack cooldown must be nonzero.
- Vertical slice starts with one weapon.

## EnemyConfig

Required fields:

- `id`
- `displayName`
- `role`
- `maxHp`
- `contactDamage`
- `moveSpeed`
- `attackRange`
- `attackCooldown`
- `projectileConfig` for ranged enemies
- `chargeConfig` for charger enemies
- `xpReward`
- `coinReward`

Validation:

- Role must be one of `melee`, `ranged`, `charger`, `elite`, `boss`, or future documented role.
- Boss configs must provide boss health bar and attack pattern references.

## AbilityConfig

Required fields:

- `id`
- `displayName`
- `description`
- `rarity`
- `tags`
- `maxStacks`
- `weight`
- `effectType`
- `effectValues`
- `exclusionGroup`
- `icon`

Validation:

- `maxStacks` must be at least 1.
- `weight` must be nonnegative.
- Ability tags should include at least one of `attack`, `projectile`, `dash`, `defense`, `utility`, `status`, `economy`, `summon`.
- Dash abilities must be testable through dash events.

## ChapterConfig

Required fields:

- `id`
- `displayName`
- `chapterIndex`
- `roomSequence`
- `recommendedPower`
- `entryCost` optional and not used in vertical slice
- `completionRewards`

Validation:

- Vertical slice Chapter 1 should contain 12-15 rooms.
- Last room should be boss room.

## RoomTemplateConfig

Required fields:

- `id`
- `roomType`
- `arenaPrefab`
- `spawnPoints`
- `waves`
- `clearCondition`
- `rewardDefinition`
- `difficultyRating`
- `cameraSettings`

Validation:

- Combat rooms require at least one wave.
- Boss rooms require exactly one boss definition or boss wave.
- Reward/heal rooms do not require enemy waves.

## WaveConfig

Required fields:

- `id`
- `enemyGroups`
- `spawnDelay`
- `spawnInterval`
- `maxAlive`
- `clearCondition`

Enemy group fields:

- `enemyConfig`
- `count`
- `spawnPattern`
- `delay`

Validation:

- `maxAlive` cannot exceed performance budget without approval.
- All enemy references must be valid.

## RewardTableConfig

Required fields:

- `id`
- `rewardEntries`
- `rollCount`
- `guaranteedRewards`
- `scalingRules`

Reward entry fields:

- `rewardType`
- `itemOrCurrencyId`
- `minAmount`
- `maxAmount`
- `weight`

Validation:

- No premium currency hard gate for basic vertical slice progress.
- Reward ranges must be compatible with economy spreadsheet.

## MonetizationConfig

Required fields:

- `rewardedReviveEnabled`
- `doubleRewardAdEnabled`
- `freeChestCooldownSeconds`
- `abilityRerollAdEnabled`
- `starterPackEnabled`
- `interstitialEnabled`
- `interstitialFrequency`

Validation:

- Interstitials default off for vertical slice.
- Fake ad service only until SDK approval.

## Save Data

Required fields:

- `schemaVersion`
- `playerIdLocal`
- `currencies`
- `gearInventory`
- `equippedGear`
- `talents`
- `chapterProgress`
- `settings`
- `lastDailyRewardClaim`

Validation:

- Save version must support future migration.
- Save/load failures must not erase data without explicit recovery path.

## Analytics Event Payload

Required fields:

- `eventName`
- `timestamp`
- `sessionId`
- `runId` where relevant
- `parameters`

Validation:

- Event names use lowercase snake_case.
- Do not send PII.
- Gameplay code uses service interface only.

