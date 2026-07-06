# Survivor Movement and Combat Feel Notes

This pass targets the desktop survivor prototype feel: continuous WASD movement, mouse-world aim, primary attack during movement, responsive dash, and active skills layered over locomotion.

## Movement Model

- `PlayerMovementController` uses Rigidbody horizontal velocity in `FixedUpdate`.
- Diagonal input is normalized before velocity is applied.
- `PlayerConfig` exposes move speed, acceleration, deceleration, rotation speed, movement deadzone, and smoothing.
- Desktop survivor repair disables `rotateTowardMovement` so movement rotation does not fight mouse aim rotation.
- Rigidbody interpolation and continuous collision are enforced for stable camera/player motion.

Recommended first tuning:

```text
Move Speed: 5-6
Acceleration: 45-60
Deceleration: 55-70
Use Movement Smoothing: true
Stop To Attack Movement Threshold: 0.08
```

## Aim and Facing Model

- `MouseAimController` projects the mouse ray onto the stable ground plane by default and rotates the Player Rigidbody in `FixedUpdate`.
- Do not use broad physics raycast masks for primary mouse aim in the prototype; enemy, player, prop, and wall colliders can pull the aim point off the floor and create jitter.
- Reticle smoothing should stay at `0` for primary mouse aim so the marker does not drift after camera/player movement settles.
- `SurvivorCameraRig.snapFollowToTarget` should stay enabled for the prototype; delayed camera catch-up changes the mouse-world projection and makes the reticle drift toward screen center after movement stops.
- `PlayerConfig.FacingPolicy` should start at `MouseAimDirection`.
- `PlayerAttackController.faceTargetOnAttack` should be off for the desktop survivor prototype.
- If the player feels like it is sliding sideways too much, keep mouse aim facing but tune animation blend and camera angle first before re-enabling movement-facing rotation.

## Primary Attack Policy

- `PlayerAttackController` supports `PrimaryAttackFirePolicy`.
- Recommended prototype policy: `HoldMouseAim`.
- Primary attack fires only while left mouse is pressed/held, then follows weapon cooldown cadence.
- Primary attack does not stop movement when `PlayerConfig.AttackWhileMoving` is true.
- Nearest-target mode uses `PlayerTargetProvider` and ignores inactive/dead/pooled targets.
- Auto-target policies remain available for legacy or explicit test configs, but they are not the default desktop survivor behavior.
- Manual mouse fire uses `MouseAimController` and fires toward the mouse-world reticle direction, falling back safely to player forward if aim projection fails.
- Successful primary attacks raise `OnPrimaryAttackFired` for animation feedback.
- `MouseAimReticleController` shows the world-space aim point and can use a prefab or runtime LineRenderer fallback.

## Active Skill Casting Policy

- `ActiveSkillController` handles Q/E/R/F through `DesktopSurvivorInputBridge`.
- Slots expose aim mode, target mode, origin mode, movement lock, cast time, effect delay, cooldown, and feedback hooks.
- Default slots should not lock movement.
- Short input buffering is driven by `PlayerConfig.SkillInputBufferSeconds`.
- Casts are blocked when the player is dead or `Time.timeScale` is paused.
- Skill cast animation is broadcast through `TriggerSkillCastAnimation` without creating a hard assembly reference from Survivor to Characters.

Recommended default slot policies:

```text
Q: ForwardCleave / MouseAim / DirectionalArea / Player
E: GroundImpact / MouseAim / SelfArea / Player
R: ForwardCleave / MouseAim / DirectionalArea / Player
F: GroundImpact / MouseAim / SelfArea / Player
Lock Movement During Cast: false
```

## Dash Behavior

- Dash input is owned by `DesktopInputReader` and `DesktopSurvivorInputBridge`.
- `PlayerDashController.enableKeyboardTestDash` should stay off to avoid duplicate input paths.
- Direction priority is movement input, mouse aim, last facing, transform forward, then world forward.
- Dash uses Rigidbody `MovePosition`, then unlocks movement when the dash state ends.
- Dash hit queries ignore inactive pooled targets.

## Animation Layering Assumptions

- `CharacterAnimationDriver` supports `MoveSpeed`, `IsMoving`, `IsDashing`, `IsAttacking`, `Attack`, `SkillCast`, `Dash`, `Hit`, and `Death`.
- Primary attack and skill cast set short visual bool/trigger signals but do not force direct attack state by default.
- `playerAttackLocksDirectState` should remain false for survivor movement.
- A future production animator should use an upper-body layer or additive action layer for attacks and casts.

## Repair and Validation

Run these menu items after code recompiles:

```text
Tap Knockout > Survivor > Repair Prototype Scene
Tap Knockout > Survivor > Validate Prototype Scene
```

Repair fills safe references for movement, input, mouse aim, attack, active skills, dash, feedback, pools, and runtime directors. Validation warns about settings that commonly cause old stop-to-attack behavior or rotation jitter.

## Manual Unity Assignments

- `PlayerConfig_Default`: verify survivor feel fields.
- `WeaponConfig`: projectile prefab, attack range, cooldown, target layers.
- `ProjectileSpawnPoint`: assign on `PlayerAttackController` if repair cannot find it.
- `ActiveSkillController.targetLayers`: Enemy.
- `PlayerDashController.fallbackDashHitLayers`: Enemy.
- Active skill VFX/SFX feedback prefabs and clips.
- Boss warning/spawn/defeat VFX/SFX if testing boss milestone.

## Test Checklist

1. Open `Assets/_Project/Scenes/DesktopSurvivorPrototype.unity`.
2. Run `Tap Knockout > Survivor > Repair Prototype Scene`.
3. Run `Tap Knockout > Survivor > Validate Prototype Scene`.
4. Press Play.
5. Test WASD start/stop and diagonal movement.
6. Move the mouse around the arena and confirm facing is stable.
7. Move while primary attack fires.
8. Fire with no enemies and confirm mouse fallback direction.
9. Spawn enemies and confirm nearest-target behavior.
10. Cast Q/E/R/F while moving.
11. Dash while moving and while standing still.
12. Check move plus attack animation does not freeze locomotion.
13. Check boss/wave flow still runs.
14. Check Console for errors or repeated warnings.

## Known TODOs

- Author a production animator controller with proper upper-body attack/cast layering.
- Tune final movement and dash values after real arena art/collision is stable.
- Add production VFX/SFX to active skill feedback configs.
- Run Unity Play Mode stress tests once licensing and local editor compile are available.
