# Production Game Design Document

## Genre

Portrait mobile action roguelite with RPG progression.

## Core Run Loop

```text
Select chapter
Enter room
Fight enemies
Clear room
Choose ability or receive reward
Enter next room
Fight boss
Win or die
Return home
Upgrade gear/talents
Start another run
```

## Moment-to-Moment Loop

```text
Move
Dodge
Stop/auto-attack
Dash
Collect rewards
Choose ability
Repeat
```

## Controls

Recommended first production scheme:

- Drag anywhere or virtual joystick to move.
- Auto-attack nearest enemy when stationary.
- Dash button bottom-right.
- Dash direction uses current move direction; if stationary, last facing direction or nearest target.

Alternatives to A/B test later:

- Double tap dash
- Swipe dash
- Attack while moving
- Manual aim release

## Combat

### Auto-Attack

Initial combat model:

```text
Stop-to-attack
```

When the player is not moving and attack cooldown is ready, the player attacks nearest valid enemy.

### Dash

Dash is signature identity.

Dash can:

- Move player quickly
- Avoid damage briefly
- Deal impact damage
- Knock enemies back
- Trigger abilities

Initial values:

```text
DashCooldown = 4.0
DashDuration = 0.18
DashDistance = 3.5
DashIFrameWindow = 0.12
DashKnockbackForce = 8
```

## Weapons

Initial weapon archetypes:

| Weapon | Style | Purpose |
|---|---|---|
| Blade | Short-range slash/projectile arc | Dash synergy |
| Bow | Straight projectile | Familiar safe option |
| Hammer | Heavy impact | Knockback identity |
| Orb | Magic projectile | Ability synergy |
| Boomerang | Returning projectile | Positioning |

Vertical slice: start with one weapon.

## Abilities

Run abilities are temporary. Examples:

- Attack Up
- Attack Speed Up
- Crit Chance Up
- Max HP Up
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

Rules:

- Show 3 ability cards.
- Player chooses 1.
- Avoid duplicates.
- Respect max stacks.
- Use rarity weights.

## Rooms

Room types:

- Combat
- Elite
- Reward
- Heal
- Shop
- Boss

Vertical slice:

- 12–15 rooms
- 3 enemy types
- 1 boss
- 1 reward/heal room
- 1 elite room

## Enemies

Initial enemy families:

1. Melee Chaser
2. Ranged Shooter
3. Charger

Boss placeholder:

```text
Stone Brute
```

Boss attacks:

- Ground slam
- Charge
- Add summon
- Circular danger zone

## FTUE

Tutorial order:

1. Move
2. Stop to attack
3. Dash
4. Clear first room
5. Pick ability
6. Upgrade after run

## Vertical Slice Acceptance

- One chapter playable
- Player movement works
- Auto-attack works
- Dash works
- Room loop works
- Ability selection works
- Basic boss works
- Android build works
