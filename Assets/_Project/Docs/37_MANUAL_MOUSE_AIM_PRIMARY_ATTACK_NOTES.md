# Manual Mouse Aim Primary Attack Notes

## Why Default Auto Attack Was Removed

The desktop survivor prototype now prioritizes intentional mouse aim and active firing. Default auto attack made combat readable in older prototype passes, but it reduced agency and made projectile direction feel less connected to the cursor. Auto policies remain in `PrimaryAttackFirePolicy` for legacy and explicit test configs, but the default desktop survivor path is manual hold-to-fire.

## Manual Primary Fire Model

- `PlayerConfig.primaryAttackFirePolicy` defaults to `HoldMouseAim`.
- `PlayerConfig.attackWhileMoving` defaults to true.
- `PlayerConfig.manualFireRequiresInput` defaults to true.
- `WeaponConfig.attackCooldown` remains the source of primary attack cadence, damage, projectile speed, and lifetime.
- Holding left mouse repeatedly fires when cooldown is ready.
- Clicking left mouse fires one shot if cooldown is ready.
- Releasing left mouse stops new shots.

## Input Flow

`DesktopInputReader` reads WASD, dash, pause, skill hotkeys, and left mouse fire state. Primary fire exposes:

- `PrimaryFireHeld`
- `PrimaryFirePressedThisFrame`
- `PrimaryFireReleasedThisFrame`

`DesktopSurvivorInputBridge` continues to route dash, pause, and active skills. Primary fire is consumed directly by `PlayerAttackController` so movement stays independent from attack.

## Mouse Aim Flow

`MouseAimController` projects the cursor through the gameplay camera onto the stable ground plane by default. Physics raycast aim remains available for explicit future terrain cases, but the desktop prototype keeps it disabled so enemies, player colliders, props, and walls cannot pull the aim point off the floor. Aim refresh runs after the camera rig, and the reticle runs after aim, which avoids one-frame camera/reticle drift. The survivor camera snaps to the player in this prototype so camera catch-up cannot move the mouse-world projection after movement stops.

`MouseAimReticleController` follows `MouseAimController.AimPointWorld`. It can use an assigned prefab, or a runtime LineRenderer ring/crosshair fallback when no prefab exists. The reticle instance is kept as an independent world object rather than a child of the player, so player Rigidbody interpolation and facing rotation cannot drag the marker toward the player center. The fallback is lifted above the ground plane and rendered as an overlay so the blue aim marker does not sink into or fade under floor geometry. The fallback is project-owned placeholder art and should be replaced by final art later.

`PlayerAttackController` resolves projectile direction from `MouseAimController.TryGetAimDirection`. If aim projection fails, it falls back to player forward. Manual fire does not request nearest enemy targeting.

## Reticle Behavior And Prefab Assignment

Assign a final reticle prefab to `MouseAimReticleController.reticlePrefab` when art is ready. Until then, leave `allowRuntimeFallback` enabled.

Tunable reticle values live on `PlayerConfig` and are pushed into the reticle by `PlayerAttackController`:

- enabled
- scale
- y offset
- smoothing, default `0` for responsive primary mouse aim
- hide system cursor during gameplay
- show only during gameplay
- show only while aiming or firing
- invalid aim behavior

The generated fallback reticle has no colliders, uses the Ignore Raycast layer by default, and should not be part of enemy target layers.

## Cursor Behavior

The system cursor is hidden by default while gameplay is active, the player is alive, and the reticle is visible. Cursor lock is not used. Pause, level-up time scale pauses, focus loss, and component disable restore a visible unlocked cursor.

## Attack Feedback Hooks

`PlayerAttackController` supports optional shot feedback:

- reticle pulse on successful shot
- muzzle flash `ParticleSystem`
- shot `AudioSource` plus `AudioClip`
- subtle `CameraShakeReceiver` impulse

Projectile hit feedback remains centralized through `CombatEvents` and `TapKnockout.Feedback.ImpactFeedbackController`, including hit flash, damage numbers, hit pause, camera shake, hit VFX/SFX hooks, and enemy death VFX/SFX hooks.

## Inspector Assignments

After running `Tap Knockout > Survivor > Repair Prototype Scene`, verify:

- Player has `DesktopInputReader`.
- Player has `MouseAimController`.
- Player has `MouseAimReticleController`.
- `PlayerAttackController.desktopInputReader` points to the player input reader.
- `PlayerAttackController.mouseAimController` points to the player aim controller.
- `PlayerAttackController.aimReticle` points to the player reticle controller.
- `PlayerAttackController.playerHealth` points to player health.
- `PlayerAttackController.projectileSpawnPoint` points to `ProjectileSpawnPoint`.
- `PlayerConfig.primaryAttackFirePolicy` is `HoldMouseAim`.
- `PlayerConfig.attackWhileMoving` is true.
- `PlayerConfig.manualFireRequiresInput` is true.
- `MouseAimController.groundLayers` excludes the reticle layer.

## Manual Test Checklist

1. Open `Assets/_Project/Scenes/DesktopSurvivorPrototype.unity`.
2. Run `Tap Knockout > Survivor > Repair Prototype Scene`.
3. Run `Tap Knockout > Survivor > Validate Prototype Scene`.
4. Press Play.
5. Do not press left mouse and confirm no primary fire occurs.
6. Move with WASD and confirm movement does not trigger primary fire.
7. Move the mouse and confirm the world reticle follows the aim point while the system cursor stays hidden in gameplay.
8. Hold left mouse and confirm shots repeat on cooldown.
9. Release left mouse and confirm firing stops.
10. Confirm projectiles travel from `ProjectileSpawnPoint` toward the reticle.
11. Confirm movement remains responsive while firing.
12. Confirm hits still show configured flash, damage numbers, subtle hit stop, VFX/SFX, death feedback, and XP drops.
13. Pause or trigger level-up pause and confirm cursor/reticle behavior is safe.

## Known Risks And TODOs

- Runtime fallback reticle is prototype art, not final UI/VFX.
- Final reticle prefab, shot SFX, muzzle flash, and subtle camera impulse tuning still need art/audio direction.
- Manual Unity playtesting is required to validate feel, cursor behavior, and camera readability.
- Existing legacy auto-fire policies should be preserved for tests and optional modes, but not used by default desktop survivor repair.
