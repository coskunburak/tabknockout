# Visual Quality Foundation Notes

Status: implemented foundation. Scene changes are applied through Editor tooling, not direct `.unity` YAML edits.

## Scope

This pass adds a production-oriented visual foundation for the desktop survivor prototype:

- Desktop 2.5D orthographic gameplay camera defaults.
- Smooth follow with subtle movement look-ahead and target override support.
- Camera shake receiver reuse for combat and boss feedback.
- ScriptableObject visual quality presets for `PrototypeLow`, `PrototypeMedium`, and `PrototypeHigh`.
- ScriptableObject lighting config for one main shadow-casting light plus capped non-shadow accent lights.
- Project-owned Global Volume profile creation through the visual repair tool.
- Generated placeholder VFX catalog for dash, projectile, XP, enemy hit, and boss readability.
- XP orb glow/pulse component using `MaterialPropertyBlock`.

## Editor Tool

Run:

```text
Tools/Tap Knockout/Visuals/Apply Production Visual Foundation
```

The tool creates or updates project-owned assets under:

```text
Assets/_Project/ScriptableObjects/Visuals/
Assets/_Project/ScriptableObjects/VFX/
Assets/_Project/Prefabs/VFX/Generated/
Assets/_Project/Art/Materials/Generated/
```

It also configures the active scene camera, Global Volume, lighting root, VFX feedback root, and scene XP orb visual hooks through Unity Editor APIs.

## Performance Choices

- `PrototypeMedium` is the default target.
- Main light shadows are enabled with a 2048 shadow map.
- Additional light shadows are disabled by default.
- Accent lights are capped and non-shadow by default.
- Bloom, color adjustments, vignette, and SSAO are conservative.
- Motion blur and depth of field are disabled for gameplay.
- Generated combat VFX are pooled through the existing `VFXService`.

## Follow-Up

- Run the visual foundation tool in the target prototype scene.
- Review the generated placeholder VFX from gameplay camera distance and replace with approved production assets later.
- Validate UI readability after post-processing is enabled.
- Profile 100+ enemy stress after generated VFX and accent lighting are active.
