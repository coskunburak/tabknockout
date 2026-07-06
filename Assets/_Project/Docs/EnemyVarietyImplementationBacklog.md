# Enemy Variety Implementation Backlog

## P0 - Safe Integration Follow-Up

- Validate generated player and enemy prefabs in Prefab Mode.
- Confirm no missing scripts or missing materials on generated prefabs.
- Confirm generated prefabs keep current gameplay components.
- Tune collider height/radius per selected role.
- Tune visual scale for mobile portrait readability.
- Assign generated basic melee, fast melee, tank, ranged, charger, caster, and boss candidate prefabs to project-owned configs/waves only after validation.

## P0 - Animation Mapping

- Inspect FBX sub-assets for Rogue, GreenDemon, Alien, Cyclops, Ranger, Demon, Wizard, and Yeti.
- Create project-owned Animator Controllers under `Assets/_Project/Animations/Player` and `Assets/_Project/Animations/Enemies`.
- Standardize parameters: `Speed`, `IsMoving`, `IsAttacking`, `IsDashing`, `IsHit`, `IsDead`, `AttackTrigger`, `HitTrigger`, `DeathTrigger`, `DashTrigger`, `CastTrigger`.
- Ensure idle/move clips loop.
- Ensure attack/hit/death/dash/cast clips do not loop.
- Confirm no missing motion references in controllers.

## P1 - Gameplay Role Expansion

- Add true ranged shooter behavior with projectile spawn timing.
- Add charger telegraph, charge movement, recovery, and interruption hooks.
- Add caster delayed cast or area warning behavior.
- Add boss-candidate attack pattern wrapper without rewriting the full boss system.
- Add role-specific hit reaction timing for dash-impact knockback.

## P1 - Socket Polish

- Position `ProjectileSpawnPoint` at bow/staff/chest/mouth as appropriate per visual.
- Add `HitVFXSocket` and `DeathVFXSocket` offsets per model height.
- Add `TelegraphRoot` for charger/caster/boss visuals.
- Add `AttackOrigin` for melee and ranged timing.

## P1 - Mobile Optimization

- Audit texture import size for all selected assets.
- Reduce material count where possible through project-owned variants.
- Confirm no real-time lights or cameras inside generated prefabs.
- Confirm skinned mesh count and bone count are acceptable on device.
- Validate 25 enemy target budget with selected visuals.

## P2 - Content Expansion

- Add alternate melee variants from remaining Cute Animated Monsters assets.
- Add elite guard visual from Warrior/Knight-style assets if style validates.
- Add area denial enemy visual once bomber/caster behavior exists.
- Add support/summoner visual after summon behavior exists.
- Add additional boss candidates after first boss flow is stable.

## Deferred

- Full retargeting pipeline.
- Full boss system rewrite.
- Final cinematic animation polish.
- Final sound design.
- New asset imports or new packages.
