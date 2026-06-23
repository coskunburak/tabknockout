# Meta Progression and Economy

## Currencies

### Coins

Sources:
- Room rewards
- Chapter completion
- Daily missions
- Rewarded ads
- Events

Sinks:
- Gear upgrades
- Talents
- Basic shop

### Gems

Sources:
- Achievements
- Daily milestones
- IAP
- Events

Sinks:
- Chests
- Rerolls
- Revive
- Premium shop

### Materials

Sources:
- Bosses
- Daily dungeons
- Chests
- Events

Sinks:
- Weapon/armor upgrades
- Talent nodes

## Gear

Slots:

- Weapon
- Armor
- Ring
- Amulet
- Boots
- Companion later

Rarity:

- Common
- Uncommon
- Rare
- Epic
- Legendary
- Mythic later

Vertical slice:

- Weapon + armor only
- Common/Rare/Epic only

## Talents

Permanent upgrades:

- Max HP
- Attack Damage
- Move Speed
- Dash Cooldown
- Crit Chance
- Coin Bonus
- Reward Bonus

## Reward Tables

Use data-driven configs:

- RewardTableConfig
- ChapterRewardConfig
- ChestConfig
- DailyRewardConfig
- MissionRewardConfig

## Economy Rules

- Early upgrades should be frequent.
- Do not add too many currencies early.
- Rewarded ads accelerate, not block.
- Premium currency must not be required for basic progress.

## Energy

Do not implement energy in vertical slice. Keep hooks for future if needed.
