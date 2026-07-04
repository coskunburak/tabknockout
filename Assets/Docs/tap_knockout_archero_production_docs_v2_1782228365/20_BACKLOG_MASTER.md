# Backlog Master

## Input and Camera

- Implement `DesktopInputController`.
- Implement `MouseAimController`.
- Implement dash/evade input binding.
- Implement `SurvivorCameraRig`.
- Add camera bounds and zoom tuning.
- Add settings for sensitivity and screenshake if needed.

## Arena and Run Director

- Define `ArenaConfig`.
- Define `RunConfig`.
- Implement `ArenaRunDirector`.
- Track run timer and state.
- Handle pause/resume around level-up.
- Trigger win/loss result.
- Expose run summary.

## Enemy Spawning

- Define `WaveTimelineConfig`.
- Define `SpawnGroupConfig`.
- Implement `SpawnDirector`.
- Implement enemy live budget.
- Add spawn ring and safety radius.
- Add elite and boss milestone hooks.

## Combat and Abilities

- Preserve shared damage/health contracts.
- Implement primary attack policy.
- Implement active skill slots.
- Implement cooldown and duration runtime.
- Implement passive upgrades.
- Implement projectile modifiers.
- Implement dash modifiers.
- Add ability tags, rarity, weights, stacks, exclusions.

## Pickups and Progression

- Implement pooled `XPOrb`.
- Implement pickup magnet behavior.
- Define XP curve.
- Trigger level-up.
- Implement weighted 3-choice offers.
- Apply ability selections.

## UI

- Gameplay HUD.
- HP bar.
- XP bar.
- Run timer.
- Active skill cooldown slots.
- Dash cooldown indicator.
- Boss health bar.
- Wave/elite/boss warnings.
- Level-up modal.
- Result screen.
- Pause/settings.

## Boss Encounters

- Define `BossEncounterConfig`.
- Implement boss milestone warning.
- Spawn boss.
- Bind boss health bar.
- Add first boss attacks.
- End run on boss defeat for MVP.

## Performance and Pooling

- Pool enemies.
- Pool projectiles.
- Pool XP orbs and pickups.
- Pool VFX bursts.
- Add stress test for 100+ enemies.
- Track GC allocations during combat.
- Cap enemy budget by performance/readability.

## Content Pipeline

- Verify staged asset licenses.
- Migrate approved assets to `Assets/ThirdParty`.
- Create production prefab variants under `Assets/_Project`.
- Create arena kit pass.
- Create VFX readability pass.
- Create ability icon placeholders.

## QA

- Desktop control smoke tests.
- 10-minute run completion test.
- Boss readability test.
- Level-up interruption test.
- Pickup pooling test.
- Projectile stress test.
- Run result test.
- Steam demo build checklist.

## Legacy Cleanup

- Search for touch/mobile/room/ad-first assumptions.
- Mark or isolate legacy room systems.
- Update prompts before using old code paths.
- Keep useful configs and combat systems where they fit the survivor loop.
