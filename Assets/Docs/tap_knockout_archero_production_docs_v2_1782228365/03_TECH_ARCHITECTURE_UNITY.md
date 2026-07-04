# Unity Technical Architecture

## Architecture Target

The target architecture is a desktop-first Unity 6 3D arena survivor prototype. Systems should be data-driven, pooled, testable where practical, and independent from live Unity scene object names.

## Folder Policy

Production work should live under:

```text
Assets/_Project/
```

Recommended structure:

```text
Assets/_Project/
  Art/
  Audio/
  Editor/Tools/
  Materials/
  Prefabs/
  Scenes/
  ScriptableObjects/
    Abilities/
    Arenas/
    Bosses/
    Enemies/
    Runs/
    Waves/
  Scripts/
    Ability/
    Analytics/
    Audio/
    Boss/
    Camera/
    Combat/
    Config/
    Enemy/
    Input/
    Pickup/
    Player/
    Pooling/
    Projectile/
    Run/
    Save/
    UI/
    VFX/
    Wave/
  Tests/
  UI/
  VFX/
```

Approved third-party assets should move only after approval to:

```text
Assets/ThirdParty/
```

## Core Runtime Systems

### Input

- `DesktopInputController`: reads keyboard/mouse input.
- `MouseAimController`: converts mouse screen position into world aim direction.
- Optional abstraction layer for future controller support.
- Mobile touch/joystick input is deprecated for the prototype.

### Camera

- `SurvivorCameraRig`: isometric/top-down follow camera.
- Supports arena bounds, target offset, zoom tuning, and screen shake hooks.
- Must preserve combat readability under enemy density.

### Run and Wave

- `ArenaRunDirector`: owns run state, timer, milestones, level-up pause/resume, result flow.
- `SpawnDirector`: selects spawn groups by timeline, player position, budgets, and spawn safety rules.
- `WaveDirector`: evaluates timed waves, intensity curves, elite windows, and boss milestones.
- Legacy room/chapter managers should be migrated or left isolated.

### Combat

- Shared damage and health contracts.
- `AbilityRuntimeController` for active skills, passives, modifiers, cooldowns, stacks, and tags.
- Projectile, area damage, knockback, status, and dash impact use shared hit data.
- Data should come from ScriptableObject configs, not hardcoded scene state.

### Enemy Crowd

- Lightweight chaser, swarm, ranged, charger, tank, elite, and boss behaviors.
- Avoid expensive per-enemy pathfinding for large groups unless profiling proves it safe.
- Prefer simple steering, separation, and pooled controllers for MVP.

### Pickups and XP

- `XPOrb` and pickup components should be pooled.
- Pickup attraction/magnet behavior should be configurable.
- XP collection feeds run level progression and level-up selection.

### UI

- HUD for HP, XP, timer, active skill cooldowns, boss HP, warnings, and pause.
- `LevelUpSelectionController` for weighted choices and application.
- Result screen receives run summary from `ArenaRunDirector`.

### Boss

- `BossEncounterDirector` handles warnings, spawn, health bar binding, phase events, death, and run completion.
- Boss attacks should use reusable telegraph and damage systems.

## Object Pooling Requirements

Pooling is required before survivor-scale combat is considered valid:

- Enemies.
- Projectiles.
- XP orbs.
- Pickups.
- VFX bursts.
- Damage number UI if used.
- Telegraph decals if spawned frequently.

Runtime systems should not instantiate/destroy repeatedly during combat waves except for debug-only cases.

## Services

Use interfaces:

- `IAnalyticsService`
- `IRemoteConfigService`
- `ISaveService`
- `IAudioService`
- Optional future `IAdService`
- Optional future `IIapService`

Initial implementations should be local/no-op/console. No real SDKs are approved by default.

## Scene Policy

- Do not directly edit `.unity` YAML.
- Use manual Unity setup or approved Editor tools.
- The first target scene is `DesktopSurvivorPrototype`.
- Existing sample scenes are not canonical gameplay scenes.

## Preserved Systems

The following concepts remain useful from earlier planning:

- ScriptableObject config IDs.
- Damage/health/projectile contracts.
- Ability definitions and modifiers.
- Enemy and boss configs.
- Wave/spawn logic.
- Pooling.
- QA/performance discipline.
- Content and license pipeline.

## Deprecated Systems

- Touch-first player input.
- Portrait safe-area-first UI.
- Room-first clear conditions as the main loop.
- Android-first release path.
- Ad/IAP-first economy architecture.
