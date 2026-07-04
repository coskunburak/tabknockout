# Combat and Abilities

## Combat Intent

Combat should feel like active 3D arena survival. The player is pressured by enemy density, but has enough agency through movement, aim, dash, cooldown skills, and build choices to recover or create openings.

## Combat Sources

- Primary weapon: auto-fire, hold-to-fire, or cadence-based attack using mouse aim.
- Active skills: manually triggered cooldown abilities.
- Passive upgrades: always-on stat or rule changes.
- Projectile modifiers: spread, pierce, ricochet, size, count, speed, status.
- Dash modifiers: i-frames, impact, shockwave, cooldown, trail, knockback.
- Area damage: nova, ground zone, orbit, beam, trap, meteor, pulse.
- Defensive/survival: max HP, armor, shield, regen, invulnerability window, pickup magnet.

## Ability Categories

### Active Skills

Triggered by hotkeys. Required fields:

- Cooldown.
- Cast time or instant flag.
- Duration.
- Range or radius.
- Targeting mode.
- Damage/scaling values.
- VFX/telegraph hooks.
- Charges if applicable.

### Passive Upgrades

Always-on stat or rule changes. Examples:

- Attack damage.
- Attack speed.
- Movement speed.
- Max HP.
- Armor/damage reduction.
- Pickup magnet.
- Crit chance.

### Weapon and Projectile Modifiers

Modify primary attack behavior:

- Twin shot.
- Additional projectile.
- Pierce.
- Split.
- Ricochet.
- Chain lightning.
- Projectile size.
- Projectile speed.
- Status application.

### Dash Modifiers

Preserve Tap Knockout's dash identity in the survivor pivot:

- Dash i-frame upgrade.
- Dash cooldown reduction.
- Dash impact damage.
- Dash knockback.
- Dash shockwave.
- Dash trail.
- Dash-triggered lightning or explosion.

### Area and Summon Abilities

Optional but genre-aligned:

- Orbiting blade.
- Ground trap.
- Periodic nova.
- Meteor strike.
- Temporary turret/summon.
- Area slow or burn field.

## Level-Up Selection

When XP reaches the next level:

1. `ArenaRunDirector` pauses or slows combat.
2. `LevelUpSelectionController` requests a weighted offer.
3. Three choices are shown by default.
4. Choices respect rarity, weight, max stacks, exclusion groups, prerequisites, and current build.
5. Selection applies to `AbilityRuntimeController`.
6. Analytics emits `ability_offered` and `ability_selected`.
7. Combat resumes safely.

## Ability Data

`AbilityConfig` should include:

- `id`
- `displayName`
- `description`
- `category`
- `rarity`
- `tags`
- `maxStacks`
- `weight`
- `exclusionGroup`
- `prerequisiteIds`
- `effectType`
- `effectValues`
- `cooldown`
- `duration`
- `scaling`
- `icon`
- `visualHooks`

`AbilityUpgradeConfig` should define stack-by-stack changes where an ability scales over levels.

## Rarity and Scaling

Recommended rarity roles:

- Common: reliable stats and simple upgrades.
- Uncommon: meaningful utility or moderate behavior changes.
- Rare: build-defining modifiers.
- Epic: high-impact active skills or synergy enablers.

Scaling should avoid runaway multiplicative stacks in MVP. Prefer additive values or capped multipliers until balance tools exist.

## Synergy Tags

Suggested tags:

- `active`
- `passive`
- `weapon`
- `projectile`
- `dash`
- `area`
- `defense`
- `survival`
- `status`
- `summon`
- `pickup`
- `boss`

Tags support weighted offers, synergy detection, balance reports, and analytics.

## Legacy Ability Migration

| Legacy Idea | Survivor Interpretation |
|---|---|
| Twin Shot | Projectile modifier that adds an additional shot. |
| Battle Rhythm | Passive attack speed upgrade. |
| Swift Footwork | Passive movement speed upgrade. |
| Phase Step | Dash i-frame or dash cooldown upgrade. |
| Bulldozer | Dash or weapon knockback upgrade. |
| Iron Core | Max HP or damage reduction passive. |
| Heal After Room | Heal after milestone, elite kill, or boss phase. |
| Shield On Room Start | Shield on level-up, boss spawn, or timed interval. |

## Balance Rules

- Every active skill must have visible cooldown feedback.
- Every level-up choice should alter either power, survivability, control, or build direction.
- Common upgrades must stay useful but not bury build-defining choices.
- Dash upgrades must remain visible and intentional.
- Boss fights must not be trivialized by single synergies without explicit design approval.
