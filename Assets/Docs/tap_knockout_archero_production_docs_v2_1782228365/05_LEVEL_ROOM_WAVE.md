# Level, Room, and Wave System

## Chapter Structure

A chapter is a sequence of rooms ending in a boss.

Example Chapter 1:

```text
Room 1: Tutorial melee
Room 2: Melee wave
Room 3: Ranged intro
Room 4: Mixed
Room 5: Ability reward
Room 6: Charger intro
Room 7: Mixed
Room 8: Elite
Room 9: Heal/reward
Room 10: Mixed
Room 11: Mini-boss
Room 12-14: Hard mixed
Room 15: Boss
```

## Room Types

- CombatRoom
- EliteRoom
- RewardRoom
- HealRoom
- ShopRoom
- BossRoom
- EventRoom later

## RoomTemplate

Fields:

```text
id
roomType
arenaPrefab
spawnPoints
hazards
waves
rewardDefinition
difficultyRating
cameraSettings
```

## WaveDefinition

Fields:

```text
enemyGroups
spawnDelay
spawnInterval
maxAlive
clearCondition
```

EnemyGroup:

```text
enemyConfig
count
spawnPattern
delay
```

## Spawn Patterns

- Edges
- Corners
- Circle
- RandomPoints
- Line
- BossAdds

## Clear Conditions

- AllEnemiesDefeated
- SurviveDuration
- DefeatBoss
- Objective later

## Vertical Slice Implementation

1. RoomManager
2. WaveManager
3. EnemySpawner
4. Room clear detection
5. ChapterRunner
6. Boss placeholder

Do not implement procedural generation first.
