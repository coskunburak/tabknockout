# Vertical Slice HUD And Enemy Lifecycle Notes

## Scope

This pass covers Prompt 2 vertical-slice wiring only:

- Active skill HUD slots for Q, E, R, and F.
- Boss health HUD binding and phase display assumptions.
- Boss phase controller spawn/init safety.
- Enemy prefab pooling and lifecycle reset contracts.
- Hit flash and knockback reset expectations.
- Validator and repair behavior for these systems.

XP orb prefab, spawn telegraph prefab, damage number prefab, and ImpactFeedbackConfig primary prefab wiring remain Prompt 1 scope.

## Active Skill HUD Wiring

`SurvivorHudController` binds to `ActiveSkillController.OnSlotStateChanged` and serializes four `ActiveSkillSlotHud` entries for Q, E, R, and F.

Each slot is expected to show:

- hotkey label,
- icon when an active ability provides one,
- cooldown fill using normalized cooldown remaining,
- ready text,
- casting text,
- safe icon-missing state.

`Tap Knockout > Survivor > Repair Prototype Scene` creates or repairs the `GameplayCanvas/ActiveSkillHud` hierarchy and assigns the slot views to `SurvivorHudController.activeSkillSlots`.

## Boss Health HUD Wiring

`BossHealthBarController` listens to `BossEvents` and can bind from the active boss GameObject. It resolves:

- `EnemyHealth` for current and max health,
- `BossPhaseController` for phase text,
- `BossConfig` for display name when available.

The repair path reuses `BossHealthBarSetupBuilder` and places `BossHealthBar_Playtest` under the gameplay canvas when missing.

## Boss Phase Controller Assumptions

Prototype boss prefabs that are used as boss spawn groups should include:

- `BossPhaseController`,
- `BossPatternController`,
- configured `BossConfig`,
- `EnemyHealth`,
- optional intro/outro and add-spawn helpers.

`BossPhaseController.Initialize` resets runtime enrage state and starts phase evaluation from `None`, so pooled or repaired bosses do not keep stale phase flags. Same-phase events are idempotent after the initial phase entry.

## Enemy Pooling And Lifecycle Contract

Generated/prototype enemy prefabs should include:

- `EnemyController`,
- `EnemyHealth`,
- `EnemyMovement`,
- `EnemyAttackController`,
- `KnockbackReceiver`,
- `HitFlashController`,
- `PooledEnemy`,
- at least one enabled collider,
- Enemy layer on the root and collider objects,
- explicit `EnemyHealth.targetTransform` where possible.

`PooledEnemy` now restores renderers, enables colliders and common enemy behaviours on spawn, and parks pooled enemies with colliders and common enemy behaviours disabled before deactivation.

`EnemyHealth` implements `IPoolLifecycle`, marks pooled enemies as not alive and not targetable, resets health on spawn, and stops pending death coroutines before pooling.

`ArenaRunDirector` clears its XP reward guard when `SurvivorSpawnDirector` announces a new enemy spawn. This preserves one reward per enemy life while allowing reused pooled GameObjects to grant XP again after respawn.

## HitFlash And Knockback Reset Behavior

`HitFlashController` and `KnockbackReceiver` already implement `IPoolLifecycle`.

The pooling path invokes lifecycle hooks for all child `MonoBehaviour` instances, so:

- hit flash material property blocks are restored,
- knockback velocity and collision state are cleared,
- movement target and velocity are cleared,
- attack cooldown/windup state is cleared,
- telegraphs, particles, trails, audio, and animators are reset.

`CharacterAnimationDriver` now implements `IPoolLifecycle` so death/hit/attack triggers and direct animation state are reset before reuse.

## Validator And Repair Behavior

`SurvivorReferenceValidator` checks:

- `SurvivorHudController.activeSkillController`,
- four assigned active skill HUD slots,
- boss health bar presence, canvas parent, and slider reference,
- enemy prefab health, movement, attack, hit flash, knockback, pooled enemy, lifecycle hooks, collider state, layer state, and targetable setup,
- boss prefab phase controller presence for boss spawn groups.

`Repair Prototype Scene` now also runs the enemy/boss prefab reference repair tool so project-owned enemy and boss prefabs receive safe lifecycle components and self-references.

## Manual Unity Assignments Still Required

Final art and tuning still need manual review:

- final active skill icons and slot styling,
- final boss health bar styling,
- final boss phase names or phase-specific UI styling,
- final enemy visuals and animator controller polish,
- any intentionally hand-authored enemy prefab exceptions,
- final wave/run asset choices for the vertical-slice scene.

## Test Checklist

1. Open `Assets/_Project/Scenes/DesktopSurvivorPrototype.unity`.
2. Run `Tap Knockout > Survivor > Repair Prototype Scene`.
3. Run `Tap Knockout > Survivor > Validate Prototype Scene`.
4. Confirm active skill HUD, boss health HUD, boss phase, and pooled enemy warnings are gone or reduced to real content TODOs.
5. Enter Play Mode.
6. Confirm Q/E/R/F slots are visible.
7. Cast Q/E/R/F and confirm cooldown fill, casting state, and ready state update.
8. Kill and respawn enemies through the pool.
9. Confirm respawned enemies are not stuck flashing, knocked back, dead, untargetable, or collider-disabled.
10. Trigger the boss wave.
11. Confirm boss health appears, updates on damage, shows phase text when available, and hides after death/outro.
12. Check the Console for errors and unexpected warnings.

## Known TODOs

- Replace placeholder active skill visuals with production icons.
- Decide whether empty fallback skill slots should show ability names or stay as hotkey-only prototype slots.
- Add phase-specific boss HUD art once phase content is final.
- Add PlayMode coverage for full pool despawn/respawn loops once Unity test execution is stable in CI or local batch mode.
