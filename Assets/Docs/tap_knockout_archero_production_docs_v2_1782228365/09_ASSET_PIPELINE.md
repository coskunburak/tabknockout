# Asset Pipeline

## Asset Direction

The asset target is a readable 3D desktop survivor game:

- One combat arena for MVP.
- Distinct enemy silhouettes.
- Clear boss scale and attack tells.
- High-contrast pickups.
- Ground telegraphs for area attacks.
- Ability icons that can be read quickly.
- VFX that communicate danger and power without hiding threats.

## Required Asset Categories

MVP asset needs:

- 3D arena environment kit.
- Player character model and basic animations.
- Enemy models for melee, swarm, ranged, charger, tank, elite, and boss.
- Humanoid/creature animations for run, hit, attack, death.
- Projectile visuals.
- Active skill VFX.
- AoE telegraph decals/effects.
- Boss VFX and warning effects.
- XP orbs and pickups.
- UI icons for active skills, passives, modifiers, and pickups.
- SFX for skills, hits, pickup, level-up, elite, boss warning, and boss defeat.

## Desktop Camera Readability

Assets must be reviewed from the isometric/top-down gameplay camera, not only in close-up.

Rules:

- Use strong silhouettes.
- Keep ground telegraphs visible on arena materials.
- Avoid VFX opacity that hides enemy attack states.
- Keep pickup colors distinct from danger colors.
- Use boss size, outline, or VFX accents to separate it from crowds.
- Keep material count and shader complexity reasonable for 100+ enemies.

## Third-Party Asset Policy

Approved external assets should be placed under:

```text
Assets/ThirdParty/<Source>/<PackName>/
```

Production prefabs, variants, and tuned materials should live under:

```text
Assets/_Project/
```

Do not modify third-party source assets directly unless unavoidable. Prefer variants.

## Current Staged Asset Packs

The project contains staged packs under:

```text
Assets/Assets/game asset packs/
```

Known status from the shallow audit:

- KayKit Character Animations: CC0 license file found.
- KayKit Dungeon Remastered: CC0 license file found.
- Quaternius Ultimate Animated Character Pack: CC0 license file found.
- Quaternius Medieval Weapons: CC0 license file found.
- Quaternius RPG Characters: CC0 license file found.
- Kenney Mini Dungeon: CC0 license file found.
- Kenney UI Pack: CC0 license file found.
- Cute Animated Monsters: no license file found in shallow audit, do not use until proven.

## Intake Checklist

Before using an asset in production:

- Confirm source URL.
- Confirm author.
- Confirm license and commercial-use rights.
- Add credit entry to `17_CREDITS_TEMPLATE.md`.
- Check scale and orientation.
- Check material/shader compatibility.
- Check silhouette from gameplay camera.
- Check VFX readability under density.
- Place source and production variants in correct folders.

## Legacy Note

Mobile portrait readability and small-screen safe area constraints are no longer primary. They may inform future ports but should not dominate desktop MVP art decisions.
