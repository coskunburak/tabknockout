# Risk Register

## R1 Hidden Legacy Assumptions

Risk: Old mobile, touch, room, Android, ad, or daily economy assumptions remain hidden in docs or code.

Mitigation: Search docs/code during each sprint, mark legacy paths, and update prompts before implementation.

## R2 Room Architecture Conflict

Risk: Existing or planned room managers conflict with a continuous arena run loop.

Mitigation: Treat room systems as legacy/future challenge infrastructure. Implement `ArenaRunDirector`, `SpawnDirector`, and `WaveDirector` as canonical.

## R3 Enemy Density Performance

Risk: Survivor-scale enemy counts overwhelm CPU, physics, VFX, or GC.

Mitigation: Pool enemies/projectiles/pickups/VFX, use lightweight AI, profile 100+ enemies early, and cap spawn budget.

## R4 Active Skill Complexity

Risk: Too many active skills and modifiers create untestable interactions.

Mitigation: Start with a small tagged ability pool, use max stacks/exclusions, and add balance reports.

## R5 Camera Readability

Risk: 3D camera angle hides threats or makes targeting unclear.

Mitigation: Prototype camera early, test silhouettes, telegraphs, pickups, and boss warnings under density.

## R6 Ability Balance Scope

Risk: Build crafting becomes too broad before the core loop is fun.

Mitigation: Ship MVP with 12 meaningful choices, then expand only after fun and performance gates pass.

## R7 Reference Similarity

Risk: Design becomes too close to reference games.

Mitigation: Use references only for genre grammar. Keep names, visuals, abilities, enemies, UI, and progression original.

## R8 Asset Licensing

Risk: Staged assets may have unclear license or attribution status.

Mitigation: Use `17_CREDITS_TEMPLATE.md`, migrate approved assets only, and do not use unproven packs.

## R9 SDK and Privacy Creep

Risk: Analytics/monetization SDKs are added before compliance review.

Mitigation: Use local/no-op service interfaces until explicit approval and platform policy review.

## R10 Steam Demo Quality

Risk: A demo is shared before controls, performance, or readability are stable.

Mitigation: Require vertical slice QA, performance stress results, and known issue review before external release.
