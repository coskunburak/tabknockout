# Ability / Power-Up Implementation Backlog

This backlog expands Tap Knockout's run ability ecosystem around the original dash-impact identity. It is intentionally implementation-oriented and does not copy protected ability names, icons, UI layouts, formulas, or exact balance from other games.

## Task Group A - Core Stats

Design goal: keep common picks simple, readable, and useful without overpowering dash identity.
Required systems: `AbilityDefinition`, `PlayerRuntimeStats`, stat UI/debug readout, balance validation.
Ability examples: Iron Core, Power Channel, Battle Rhythm, Swift Footwork, Quick Release, Guarded Stance, Sharp Instinct, Heavy Crit.
Implementation priority: P0 for health, attack, attack speed, move speed, projectile speed; P1 for crit and damage reduction.
Acceptance criteria: each stat modifier stacks safely, clamps correctly, and is reflected by the relevant controller.
Risks: passive damage picks can become dominant if dash picks are weak.
Deferred polish: final icons, tuned rarity weights, localized copy.

## Task Group B - Dash Impact

Design goal: make dash timing, impact, displacement, and short control windows the main combat hook.
Required systems: dash hit events, shockwave query, stun/interrupt hooks, shield-after-hit hook.
Ability examples: Breaker Dash, Shock Dash, Aftershock, Momentum Core, Bulldozer, Ricochet Dash, Guardian Dash, Chain Impact.
Implementation priority: P0 for dash damage/cooldown/knockback/i-frame; P1 for stun, shockwave, shield; P2 for chain impact and distance scaling.
Acceptance criteria: dash abilities visibly change combat outcomes without making dash spam optimal.
Risks: too much stun or cooldown reduction can trivialize rooms.
Deferred polish: final shockwave VFX, camera impulse tuning, boss-specific dash rules.

## Task Group C - Projectile Count / Direction

Design goal: provide familiar projectile build variety while keeping portrait readability.
Required systems: `ProjectileModifierState`, `ProjectilePatternBuilder`, projectile pool expansion, damage scaling rules.
Ability examples: Twin Shot, Focused Pair, Triple Fan, Side Burst, Back Spark, Split Spark, Heavy Volley.
Implementation priority: P0 for extra/front projectiles; P1 for diagonal patterns; P2 for side/rear/split.
Acceptance criteria: projectile count respects mobile caps and does not spawn duplicate hits on the same target unless intended.
Risks: projectile clutter and unpooled instantiation cost.
Deferred polish: per-projectile damage modifiers, muzzle offsets, aim assist tuning.

## Task Group D - Projectile Behavior

Design goal: create build-defining projectile behavior without rewriting core combat each time.
Required systems: projectile hit memory, target search, ricochet resolver, pierce counters, wall collision policy.
Ability examples: Seeking Spark, Chain Bounce, Piercing Bolt, Wall Skip, Phase Bolt, Longshot Focus, Wide Charge.
Implementation priority: P1 for pierce/ricochet; P2 for homing, wall bounce, long-range scaling.
Acceptance criteria: each behavior has deterministic caps and works with projectile pooling.
Risks: recursive ricochet or homing can create target ambiguity and allocations.
Deferred polish: unique VFX trails and hit sounds per behavior.

## Task Group E - Elemental Status

Design goal: make elemental builds readable and stackable without hidden math.
Required systems: `StatusEffectController`, status immunity/resistance policy, DoT tick events, slow/freeze hooks.
Ability examples: Ember Mark, Toxic Edge, Storm Link, Frost Grip, Ember Burst, Venom Pool, Static Jump, Frost Shatter.
Implementation priority: P1 for burn, poison, slow; P2 for lightning chain, freeze, super upgrades.
Acceptance criteria: status durations tick down safely and do not crash if a target lacks a receiver.
Risks: DoT spam can obscure hit feedback and create GC pressure.
Deferred polish: status icons, enemy material tinting, VFX intensity rules.

## Task Group F - Orbitals

Design goal: support close-range positioning builds that reward weaving around enemies.
Required systems: orbital owner component, pooled orbital hit volumes, tick cooldown per target.
Ability examples: Orbit Blade, Frost Orbit, Storm Orbit, Toxic Orbit, Ember Orbit, Heavy Orbit, Orbit Web.
Implementation priority: P2 for neutral orbital; P3 for elemental variants and web synergy.
Acceptance criteria: orbitals damage at controlled intervals and respect active VFX/projectile budgets.
Risks: overlapping orbitals can become unreadable around the player.
Deferred polish: orbital meshes, trail tuning, per-element impact VFX.

## Task Group G - Drones / Sprites

Design goal: add companion builds without requiring full pet AI in the first pass.
Required systems: companion spawn anchors, simple targeting, projectile reuse, lifetime/room reset rules.
Ability examples: Spark Drone, Bomb Drone, Beam Drone, Venom Drone, Frost Drone, Drone Commander, Drone Swarm.
Implementation priority: P2 for one basic shooter drone; P3 for elemental and boost variants.
Acceptance criteria: drones never block movement, target deterministically, and reset between runs.
Risks: drone projectiles can push projectile count above budget.
Deferred polish: drone models, follow smoothing, companion UI indicators.

## Task Group H - Blade / Strike

Design goal: add summoned attacks that punctuate kills, attacks, and wave starts.
Required systems: strike scheduler, target selector, pooled strike hitbox/VFX, event hooks for kill/attack/wave.
Ability examples: Blade Pulse, Pursuit Blade, Assault Blade, Blade Storm, Double Blades, Blade Mastery, Impact Blade.
Implementation priority: P2 for periodic strike; P3 for event-triggered variants.
Acceptance criteria: strikes have cooldowns, target valid enemies only, and do not block room clear.
Risks: invisible off-screen strikes can feel unfair or confusing.
Deferred polish: telegraph timing, blade art, hit pause tuning.

## Task Group I - Meteor / Area Procs

Design goal: support area-proc builds for dense rooms without full-screen clutter.
Required systems: delayed area target marker, pooled AoE damage zone, proc chance resolver.
Ability examples: Falling Star, Execution Meteor, Storm Meteor, Frost Meteor, Ember Meteor, Meteor Rain, Crater Field.
Implementation priority: P2 for simple meteor proc; P3 for elemental and ground zone variants.
Acceptance criteria: AoE markers are readable before damage and capped per room.
Risks: random AoE can remove player agency if overused.
Deferred polish: warning decal, impact VFX, screen shake budget.

## Task Group J - Beam / Charged

Design goal: reward positioning and stationary windows with precise, high-readability attacks.
Required systems: charge state, beam hit query, line telegraph, cooldown gating.
Ability examples: Focus Beam, Beam Orbit, Charged Shot, Energy Ring, Dash Beam, Focus Lance, Overcharge Core.
Implementation priority: P2 for charged shot; P3 for beams/rings.
Acceptance criteria: charge can be interrupted by movement and beam hits are deterministic.
Risks: beams can dominate narrow rooms and bypass enemy movement.
Deferred polish: beam shader/VFX, UI charge indicator, audio build-up.

## Task Group K - Defensive / Revive

Design goal: add survival choices that preserve risk/reward instead of removing danger.
Required systems: shield charges, revive token, invulnerability-after-hit, room-start hooks.
Ability examples: Second Wind, Impact Guard, Panic Shield, Light Step, Steel Skin, Room Ward, Dash Guard.
Implementation priority: P1 for shield and damage reduction; P2 for revive.
Acceptance criteria: defensive effects expose clear state and cannot stack into permanent invulnerability.
Risks: revive and shields can hide difficulty tuning issues.
Deferred polish: shield VFX, revive UI, hit immunity feedback.

## Task Group L - Potions / Pickups

Design goal: support pickup-triggered moments without building final economy early.
Required systems: pickup spawn table, pickup trigger events, temporary stat buffs.
Ability examples: Frenzy Flask, Guard Flask, Star Flask, Lucky Heart, Warrior Heart, Abundant Drops, Magnet Pulse.
Implementation priority: P2 for basic pickup effects; P3 for potion variants.
Acceptance criteria: pickup effects are optional, timed, and never required for room completion.
Risks: pickup RNG can make runs swingy.
Deferred polish: pickup models, magnet VFX, reward UI.

## Task Group M - Kill Triggers

Design goal: make aggressive play and chain-clearing feel rewarding.
Required systems: enemy death events, temporary buff stack timer, heal cap, dash cooldown refund.
Ability examples: Momentum Kill, Soul Recovery, Execution Spark, Pursuit Blade, Overrun, Victory Tempo, Knockout Pulse.
Implementation priority: P1 for heal/cooldown refund; P2 for kill-trigger damage procs.
Acceptance criteria: kill triggers do not fire twice for pooled enemies or duplicate death events.
Risks: dense rooms can snowball too hard.
Deferred polish: buff icons, floating feedback, final proc VFX.

## Task Group N - Low-Health Risk Reward

Design goal: create clutch builds that feel tense and readable.
Required systems: health threshold evaluator, conditional stat modifiers, UI warning state.
Ability examples: Last Stand, Desperate Tempo, Blood Rush, Risk Core, Final Spark, Cornered Guard, Critical Pulse.
Implementation priority: P1 for conditional damage; P2 for dash-specific low-health bonuses.
Acceptance criteria: modifiers turn on/off reliably as health crosses threshold.
Risks: low-health builds can encourage passive stalling.
Deferred polish: HP bar styling, heartbeat audio, screen edge effect.

## Task Group O - Boss-Specific

Design goal: give players prep choices for boss rooms without invalidating boss mechanics.
Required systems: boss target tagging, room type hooks, pre-boss event, shield/heal hooks.
Ability examples: Boss Breaker, Boss Ward, Pre-Boss Recovery, Giant Slayer Dash, Brute Read, Slam Guard, Last Room Focus.
Implementation priority: P1 for boss damage storage; P2 for boss-room heal/shield.
Acceptance criteria: boss effects activate only against boss-tagged targets or boss rooms.
Risks: boss-only abilities can feel bad in normal rooms if offered too early.
Deferred polish: boss-room offer weighting and warning copy.

## Task Group P - Economy / Reward

Design goal: represent reward modifiers without committing to final economy or monetization.
Required systems: reward grant pipeline, local economy config, fake analytics event.
Ability examples: Lucky Core, Coin Spark, Reward Pulse, Potion Luck, Cache Finder, Gem Glimmer, Chest Echo.
Implementation priority: P3 after reward pipeline exists.
Acceptance criteria: economy abilities are excluded from combat-only debug pools unless reward hooks exist.
Risks: reward modifiers can distort test balance and monetization assumptions.
Deferred polish: reward UI, currency VFX, remote config tuning.

## Task Group Q - Super / Synergy Upgrades

Design goal: unlock exciting build-defining upgrades from prior picks using tags and prerequisites.
Required systems: required tags, prerequisite ability IDs, upgrade groups, exclusion groups, offer weighting.
Ability examples: Super Ember, Super Toxin, Super Storm, Super Frost, Impact Overdrive, Drone Commander, Orbit Web, Meteor Rain.
Implementation priority: P2 for offer eligibility; P3 for unique behavior.
Acceptance criteria: synergy abilities appear only when requirements are satisfied and never conflict with blocked groups.
Risks: hidden prerequisites can make offers feel random.
Deferred polish: synergy reveal UI, build summary, tutorial hints.

## Task Group R - Tap Knockout Dash-Exclusive Abilities

Design goal: keep the game identity distinct from generic projectile roguelites.
Required systems: dash hit count, dash distance, enemy displacement, dash recovery, impact feedback events.
Ability examples: Momentum Core, Chain Impact, Impact Heal, Breakpoint Rush, Rebound Step, Impact Magnet, Knockout Wake, Closeout Dash.
Implementation priority: P1 for data hooks; P2/P3 for advanced impact behaviors.
Acceptance criteria: each dash-exclusive ability changes dash decision-making, not just damage numbers.
Risks: too many dash triggers can make combat hard to parse.
Deferred polish: bespoke dash VFX, combo callouts, haptic/audio timing.
