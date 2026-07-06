# Ability VFX Production Mapping

This pass gives every vertical-slice ability a readable desktop survivor VFX identity without copying protected game content. Runtime mapping is effect-type based, so display name or balance changes do not break visual feedback.

## Runtime Layers

- Selection burst: fired when an ability is selected, anchored on the player.
- Combat accent: fired on projectile, dash, elemental, kill, or heal moments when runtime stats indicate an active ability family.
- Catalog fallback: if the production catalog has not been regenerated yet, ability events fall back to existing core VFX events so gameplay never becomes visually silent.

## Vertical Slice Ability Coverage

| Ability | Effect Type | Primary VFX Event |
|---|---|---|
| Iron Core | `MaxHealthUp` | `AbilityHealthBuff` |
| Power Channel | `AttackDamageUp` | `AbilityAttackBuff` |
| Battle Rhythm | `AttackSpeedUp` | `AbilityAttackSpeedBuff` |
| Swift Footwork | `MoveSpeedUp` | `AbilityMoveSpeedBuff` |
| Quick Release | `ProjectileSpeedUp` | `AbilityProjectileBuff` |
| Impact Training | `DashDamageUp` | `AbilityDashBuff` |
| Short Fuse Dash | `DashCooldownDown` | `AbilityDashBuff` |
| Bulldozer | `DashKnockbackUp` | `AbilityDashBuff` |
| Phase Step | `DashIFrameDurationUp` | `AbilityDashPhase` |
| Twin Shot | `ExtraProjectile` | `AbilityProjectileSplit` |
| Focused Pair | `FrontProjectile` | `AbilityProjectileSplit` |
| Piercing Bolt | `ProjectilePierce` | `AbilityProjectilePierce` |
| Chain Bounce | `ProjectileRicochet` | `AbilityProjectileRicochet` |
| Ember Mark | `BurnOnHit` | `AbilityFireProc` |
| Toxic Edge | `PoisonOnHit` | `AbilityPoisonProc` |
| Frost Grip | `FreezeOnHit` | `AbilityIceProc` |
| Storm Link | `LightningOnHit` | `AbilityLightningProc` |
| Impact Guard | `ShieldPerRoom` | `AbilityShield` |
| Soul Recovery | `HealOnKill` | `AbilitySoulHeal` |
| Boss Breaker | `BossDamageUp` | `AbilityBossBreaker` |
| Last Stand | `LowHealthDamageUp` | `AbilityLowHealthSurge` |
| Shock Dash | `DashShockwave` | `AbilityDashShockwave` |
| Stagger Master | `DashStun` | `AbilityDashStagger` |
| Overrun | `DashCooldownRefundOnKill` | `AbilityDashBuff` |
| Wide Charge | `ProjectileSizeUp` | `AbilityProjectileSize` |
| Guarded Stance | `DamageReductionUp` | `AbilityDefenseBuff` |
| Sharp Instinct | `CriticalChanceUp` | `AbilityAttackBuff` |
| Heavy Crit | `CritDamageUp` | `AbilityAttackBuff` |
| Triple Fan | `DiagonalProjectiles` | `AbilityProjectileSplit` |
| Side Burst | `SideProjectiles` | `AbilityProjectileSplit` |
| Back Spark | `RearProjectile` | `AbilityProjectileSplit` |
| Seeking Spark | `ProjectileHoming` | `AbilityProjectileBuff` |
| Wall Skip | `ProjectileWallBounce` | `AbilityProjectileRicochet` |
| Longshot Focus | `LongRangeDamageUp` | `AbilityAttackBuff` |
| Light Step | `DodgeChanceUp` | `AbilityDefenseBuff` |
| Panic Window | `InvulnerabilityAfterHit` | `AbilityInvulnerability` |
| Momentum Core | `DashDamageLowHealth` | `AbilityDashBuff` |
| Guardian Dash | `DashShieldAfterHit` | `AbilityShield` |
| Static Jump | `ChainLightning` | `AbilityLightningProc` |
| Ember Surge | `SuperBurn` | `AbilityFireProc` |

## Unity Refresh Steps

1. Run `Tools/Tap Knockout/VFX/Create Vertical Slice VFX Catalog`.
2. Run `Tools/Tap Knockout/VFX/Create Feedback System Root`.
3. Confirm `VFXFeedbackRoot` has `Ability VFX Feedback Controller` and `Combat VFX Event Controller`.
4. Confirm `VFX Service` uses `Assets/_Project/ScriptableObjects/VFX/VFXCatalog_VerticalSlice.asset`.

The current implementation avoids direct scene YAML edits and does not modify third-party prefabs.
