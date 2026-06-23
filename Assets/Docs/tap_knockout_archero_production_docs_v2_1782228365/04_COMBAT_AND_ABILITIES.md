# Combat and Ability System

## Damage Flow

```text
Attacker creates HitContext
Target receives hit
Damage modifiers apply
Knockback/status effects apply
Events fire
VFX/SFX/UI react
```

## HitContext

Fields:

```text
source
target
damageAmount
damageType
critChance
knockbackDirection
knockbackForce
isDashHit
isProjectileHit
abilitySource
statusEffects
```

## Damage Types

- Physical
- Impact
- Fire
- Lightning
- Poison
- Ice
- True

Vertical slice: Physical + Impact.

## Auto-Attack

- Nearest target
- Cooldown-based
- WeaponConfig-driven
- Projectile or melee slash
- Stop-to-attack first

## Dash

Dash systems:

- Cooldown
- Direction
- I-frame hook
- Impact hitbox
- Knockback
- Ability triggers
- VFX/SFX events

Dash ability examples:

- Dash Shockwave
- Dash Cooldown Down
- Dash Damage Up
- Dash Knockback Up
- Dash Leaves Fire
- Dash Chain Lightning
- Dash Heal On Hit

## AbilityDefinition

Fields:

```text
id
displayName
description
icon
rarity
tags
maxStacks
modifiers
conditions
```

Tags:

- Attack
- Projectile
- Dash
- Defense
- Utility
- Status
- Economy
- Summon

## Ability Selection

- Show 3 cards.
- Weighted random.
- Avoid duplicates.
- Respect max stacks.
- Pause gameplay while selecting.

## Initial Ability Pool

- Attack Up
- Attack Speed Up
- Crit Chance Up
- Max HP Up
- Move Speed Up
- Double Shot
- Pierce
- Ricochet
- Side Shot
- Dash Shockwave
- Dash Cooldown Down
- Dash Damage Up
- Dash Knockback Up
- Burning Hits
- Chain Lightning
- Orbiting Blade
- Heal After Room

## Events

- OnDamageDealt
- OnEnemyKilled
- OnPlayerDamaged
- OnDashStarted
- OnDashHit
- OnProjectileFired
- OnAbilitySelected
- OnRoomCleared
