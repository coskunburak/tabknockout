# QA, Performance, and Release Pipeline

## QA Areas

- Core combat
- Room/wave loop
- Ability selection
- Boss fight
- Meta progression
- Save/load
- Ads/IAP stubs
- UI safe area
- Android build
- Performance

## Manual QA

Gameplay:

- Movement works
- Auto-attack targets enemies
- Dash works
- Dash cooldown visible
- Enemies damage player
- Rooms clear correctly
- Abilities apply correctly
- Boss can be defeated
- Run result works

Meta:

- Currency granted
- Gear upgrade works
- Talent upgrade works
- Save persists

## Performance Targets

```text
Target FPS: 60
Minimum: 30
Max enemies: 25
Max projectiles: 80
Max VFX: 20
No major GC spikes
```

## Build Gates

Before vertical slice build:

- No compile errors
- No critical runtime errors
- Android build succeeds
- Touch input works
- UI safe area acceptable
- 5-minute gameplay session stable
