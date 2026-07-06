# Production Lighting Visibility Pass

## Goal

The gameplay scene should read as a dark forest arena without making combat unreadable. The player visibility source is a soft, direction-independent local readability boost centered on the player. It should not read as a neon green circle, debug radius, poison pool, or flashlight. A cone/spot light is allowed only as an optional, low-intensity aim accent and is disabled by default.

The first production lighting pass fixed the old reversed cone-light problem, but it still leaned too hard on a saturated green ground glow. This pass reduces the visible ground VFX, desaturates the player light, softens falloff, and gives the environment more moonlit ambient readability.

## Runtime Layers

1. Environment moonlight
   - Source: `EnvironmentLightingProfile`
   - Cool directional light, soft shadows, non-black ambient, lightweight fog.
   - This keeps props, enemies, grass, and silhouettes readable outside the player aura.

2. Player visibility aura
   - Source: `PlayerVisibilityLightingController` + `PlayerVisibilityLightingProfile`
   - Main light type: Point.
   - Follows player position only.
   - Does not inherit player rotation, movement direction, or aim direction.
   - Tuned by aura color, intensity, range, height offset, follow sharpness, and optional shadow fields.
   - Default color is pale mint/cyan-white rather than saturated green.
   - Default intensity and range are deliberately modest to avoid a bright circular pool.

3. Outer ambient fill
   - Source: `PlayerVisibilityLightingController` + `PlayerVisibilityLightingProfile`
   - Main light type: Point.
   - Larger radius than the inner aura.
   - Very low intensity.
   - No shadows by default.
   - The goal is soft nearby readability, not a visible light boundary.

4. Optional aim accent
   - Source: `PlayerVisibilityLightingController`.
   - Disabled by default in `PlayerVisibilityLightingProfile`.
   - If enabled, aim comes from `MouseAimController.TryGetAimDirection`.
   - It never uses raw movement direction, so backing away from enemies does not invert the main visibility light.
   - Intensity is capped as a fraction of aura intensity.

5. Legacy glow support
   - `TapKnockoutPlayerLightRigConfig` now keeps the previous movement cone and forward lantern disabled by default.
   - Existing glow/dash support can remain, but it is no longer the primary visibility system.
   - Ground glow colors are low-alpha pale cyan-white.
   - The generated ground disc mesh uses a subtle irregular edge and lower vertex alpha so it does not read as a perfect AoE ring.

6. Post and radial darkness
   - `TapKnockoutRenderProfile` uses conservative bloom, exposure, contrast, vignette, and a wider player-centered radial darkness falloff.
   - The intent is dark edges with a smooth readable area around the player, not a hard black mask.

## Editor Tools

Use:

```text
Tools > Tap Knockout > Visuals > Apply Production Lighting Pass
```

This creates or updates:

```text
Assets/_Project/ScriptableObjects/Visuals/EnvironmentLighting_ForestArena_Default.asset
Assets/_Project/ScriptableObjects/Visuals/PlayerVisibilityLighting_Default.asset
Assets/_Project/ScriptableObjects/Visuals/TapKnockoutLightingConfig.asset
Assets/_Project/ScriptableObjects/Visuals/TapKnockoutPlayerLightRigConfig.asset
Assets/_Project/ScriptableObjects/Visuals/VolumeProfile_TapKnockoutGameplay.asset
```

Then run:

```text
Tools > Tap Knockout > Visuals > Validate Production Lighting
```

The validator checks that the player aura exists as an enabled point light, the outer fill exists, ambient lighting is not near black, moonlight is assigned, the global volume is present, bloom is not excessive, and the player glow config is not likely to create an obvious saturated ground circle.

## Tuning

Recommended first-pass tuning order:

1. Set environment darkness with `EnvironmentLightingProfile.AmbientIntensity`, `MoonlightIntensity`, `MoonlightShadowStrength`, and `FogDensity`.
2. Set local readability with `PlayerVisibilityLightingProfile.AuraIntensity` and `AuraRange`.
3. Use `OuterFillIntensity` and `OuterFillRange` only for barely noticeable nearby readability.
4. If a green circle appears, reduce `TapKnockoutPlayerLightRigConfig.GroundGlowColor.a`, `GroundGlowRadius`, `LanternFieldColor.a`, and `LanternFieldRadius` before increasing scene brightness.
5. Only after the aura feels right, optionally enable `EnableAimAccent` and keep its intensity subtle.
6. Adjust radial darkness using `RadialDarknessClearRadius`, `RadialDarknessFullRadius`, and `RadialDarknessEdgeOpacity`.
7. Keep bloom subtle. If the player looks blurry or overexposed, reduce bloom before reducing aura intensity.

## Safety

- No scene YAML should be edited manually.
- No third-party assets are modified.
- The default setup uses one main shadow-casting moonlight, one player point aura, one low-intensity non-shadow outer fill, non-shadow aim accent lights, and disabled additional light shadows.
- Motion blur and depth of field remain disabled for gameplay readability.
