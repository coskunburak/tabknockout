#if UNITY_EDITOR
using System.Collections.Generic;
using TapKnockout.Camera;
using TapKnockout.Feedback;
using TapKnockout.Input;
using TapKnockout.Pickups;
using TapKnockout.Player;
using TapKnockout.Survivor;
using TapKnockout.VFX;
using TapKnockout.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TapKnockout.Editor.Tools
{
    public static class TapKnockoutVisualSceneRepairTool
    {
        private const string CameraConfigPath = "Assets/_Project/ScriptableObjects/Camera/GameplayCameraConfig_DesktopSurvivor.asset";
        private const string VisualConfigPath = "Assets/_Project/ScriptableObjects/Visuals/TapKnockoutVisualQualityConfig.asset";
        private const string VisualPresetFolder = "Assets/_Project/ScriptableObjects/Visuals";
        private const string LightingConfigPath = "Assets/_Project/ScriptableObjects/Visuals/TapKnockoutLightingConfig.asset";
        private const string PlayerLightRigConfigPath = "Assets/_Project/ScriptableObjects/Visuals/TapKnockoutPlayerLightRigConfig.asset";
        private const string PlayerVisibilityLightingProfilePath = "Assets/_Project/ScriptableObjects/Visuals/PlayerVisibilityLighting_Default.asset";
        private const string EnvironmentLightingProfilePath = "Assets/_Project/ScriptableObjects/Visuals/EnvironmentLighting_ForestArena_Default.asset";
        private const string VolumeProfilePath = "Assets/_Project/ScriptableObjects/Visuals/VolumeProfile_TapKnockoutGameplay.asset";
        private const string VfxCatalogPath = "Assets/_Project/ScriptableObjects/VFX/VFXCatalog_ProductionVisualFoundation.asset";
        private const string GeneratedVfxFolder = "Assets/_Project/Prefabs/VFX/Generated";
        private const string GeneratedMaterialFolder = "Assets/_Project/Art/Materials/Generated";
        private const string PcRenderPipelineAssetPath = "Assets/Settings/PC_RPAsset.asset";
        private const string PcRendererAssetPath = "Assets/Settings/PC_Renderer.asset";

        [MenuItem("Tools/Tap Knockout/Visuals/Apply Production Visual Foundation")]
        public static void ApplyProductionVisualFoundation()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Tap Knockout Visual Foundation",
                    "Stop Play Mode before applying the visual foundation.",
                    "OK");
                return;
            }

            var report = new List<string>(32);
            EnsureFolders();

            var cameraConfig = LoadOrCreateCameraConfig(report);
            var visualConfig = LoadOrCreateVisualQualityConfig(report);
            var lightingConfig = LoadOrCreateLightingConfig(report);
            var playerLightRigConfig = LoadOrCreatePlayerLightRigConfig(report);
            var environmentLightingProfile = LoadOrCreateEnvironmentLightingProfile(report);
            var playerVisibilityProfile = LoadOrCreatePlayerVisibilityLightingProfile(report);
            var volumeProfile = LoadOrCreateVolumeProfile(visualConfig.ResolveDefaultPreset(), report, environmentLightingProfile);
            var vfxCatalog = LoadOrCreateVFXCatalog(report);

            ApplyURPAssetBaseline(visualConfig.ResolveDefaultPreset(), report);
            ApplySSAOFeatureBaseline(visualConfig.ResolveDefaultPreset(), report);
            ApplyCameraFoundation(cameraConfig, visualConfig, report);
            ApplyLightingFoundation(lightingConfig, report);
            ApplyEnvironmentLightingProfile(environmentLightingProfile, report);
            ApplyPlayerLightRig(playerLightRigConfig, report);
            ApplyPlayerVisibilityLighting(playerVisibilityProfile, report);
            ApplyGlobalVolume(volumeProfile, report);
            ApplyRadialDarknessOverlay(visualConfig.ResolveDefaultPreset(), report);
            ApplyFeedbackFoundation(vfxCatalog, report);
            RepairSceneXPOrbVisuals(report);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
            }

            Debug.Log("Tap Knockout production visual foundation applied:\n- " + string.Join("\n- ", report));
        }

        [MenuItem("Tools/Tap Knockout/Visuals/Apply Production Darkness And Lantern")]
        public static void ApplyProductionDarknessAndLantern()
        {
            ApplyProductionLightingPass();
        }

        [MenuItem("Tools/Tap Knockout/Visuals/Apply Production Lighting Pass")]
        public static void ApplyProductionLightingPass()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Tap Knockout Production Lighting",
                    "Stop Play Mode before applying the production lighting pass.",
                    "OK");
                return;
            }

            var report = new List<string>(24);
            EnsureFolders();

            var visualConfig = LoadOrCreateVisualQualityConfig(report);
            var lightingConfig = LoadOrCreateLightingConfig(report);
            var playerLightRigConfig = LoadOrCreatePlayerLightRigConfig(report);
            var environmentLightingProfile = LoadOrCreateEnvironmentLightingProfile(report);
            var playerVisibilityProfile = LoadOrCreatePlayerVisibilityLightingProfile(report);
            var mediumPreset = visualConfig.ResolveDefaultPreset();
            var volumeProfile = LoadOrCreateVolumeProfile(mediumPreset, report, environmentLightingProfile);

            ApplyURPAssetBaseline(mediumPreset, report);
            ApplySSAOFeatureBaseline(mediumPreset, report);
            ApplyLightingFoundation(lightingConfig, report);
            ApplyEnvironmentLightingProfile(environmentLightingProfile, report);
            ApplyPlayerLightRig(playerLightRigConfig, report);
            ApplyPlayerVisibilityLighting(playerVisibilityProfile, report);
            ApplyGlobalVolume(volumeProfile, report);
            ApplyRadialDarknessOverlay(mediumPreset, report);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
            }

            Debug.Log("Tap Knockout production lighting pass applied:\n- " + string.Join("\n- ", report));
        }

        [MenuItem("Tools/Tap Knockout/Visuals/Validate Production Lighting")]
        public static void ValidateProductionLighting()
        {
            var report = new List<string>(32);
            var hasFailure = false;
            var hasWarning = false;

            var visibility = Object.FindAnyObjectByType<PlayerVisibilityLightingController>(FindObjectsInactive.Include);
            if (visibility == null)
            {
                hasFailure = true;
                report.Add("FAIL: Missing PlayerVisibilityLightingController.");
            }
            else
            {
                var aura = visibility.MainAuraLight;
                if (aura == null || aura.type != LightType.Point || !aura.enabled)
                {
                    hasFailure = true;
                    report.Add("FAIL: Main aura must be an enabled Point Light.");
                }
                else
                {
                    report.Add($"PASS: Main aura point light active. Intensity={aura.intensity:0.00}, Range={aura.range:0.00}.");
                }

                if (visibility.OuterFillLight == null || visibility.OuterFillLight.type != LightType.Point)
                {
                    hasWarning = true;
                    report.Add("WARN: Missing outer fill point light; nearby environment may fall off too abruptly.");
                }

                var profile = visibility.Profile;
                if (profile != null)
                {
                    var auraSaturation = ComputeColorSaturation(profile.AuraColor);
                    if (auraSaturation > 0.28f || IsGreenDominant(profile.AuraColor))
                    {
                        hasWarning = true;
                        report.Add($"WARN: Aura color may read too saturated/green. Saturation={auraSaturation:0.00}, Color={profile.AuraColor}.");
                    }
                    else
                    {
                        report.Add($"PASS: Aura color is desaturated enough for natural lighting. Saturation={auraSaturation:0.00}.");
                    }

                    if (profile.AuraIntensity > 1.05f)
                    {
                        hasWarning = true;
                        report.Add($"WARN: Aura intensity {profile.AuraIntensity:0.00} may overexpose the player center.");
                    }

                    if (profile.AuraRange > 6.2f)
                    {
                        hasWarning = true;
                        report.Add($"WARN: Aura range {profile.AuraRange:0.00} may create an obvious circular pool.");
                    }

                    if (profile.EnableAimAccent && profile.AimAccentIntensity > profile.AuraIntensity * 0.14f)
                    {
                        hasWarning = true;
                        report.Add("WARN: Aim accent is enabled with a high intensity fraction; keep it secondary only.");
                    }
                }

                var accent = visibility.AimAccentLight;
                if (accent != null && accent.enabled && visibility.Profile != null)
                {
                    var maxAccent = visibility.Profile.AuraIntensity * visibility.Profile.AimAccentMaxAuraIntensityFraction + 0.01f;
                    if (accent.intensity > maxAccent)
                    {
                        hasWarning = true;
                        report.Add($"WARN: Aim accent intensity {accent.intensity:0.00} exceeds secondary-light cap {maxAccent:0.00}.");
                    }
                }
            }

            if (RenderSettings.ambientIntensity <= 0.02f)
            {
                hasWarning = true;
                report.Add("WARN: Ambient intensity is very low; forest may crush to black outside the aura.");
            }
            else if (IsNearlyBlack(RenderSettings.ambientSkyColor) && IsNearlyBlack(RenderSettings.ambientEquatorColor))
            {
                hasWarning = true;
                report.Add("WARN: Ambient sky/equator colors are near black; forest silhouettes may crush outside player light.");
            }
            else
            {
                report.Add($"PASS: Ambient readability is non-black. Intensity={RenderSettings.ambientIntensity:0.00}.");
            }

            if (RenderSettings.sun == null || RenderSettings.sun.type != LightType.Directional)
            {
                hasWarning = true;
                report.Add("WARN: Missing directional moonlight assigned to RenderSettings.sun.");
            }
            else
            {
                report.Add($"PASS: Directional moonlight active. Intensity={RenderSettings.sun.intensity:0.00}, Shadows={RenderSettings.sun.shadows}.");
            }

            var volume = Object.FindAnyObjectByType<Volume>(FindObjectsInactive.Include);
            if (volume == null || volume.sharedProfile == null)
            {
                hasWarning = true;
                report.Add("WARN: Missing Global Volume or shared profile.");
            }
            else
            {
                report.Add("PASS: Global Volume profile assigned.");
                if (volume.sharedProfile.TryGet<Bloom>(out var bloom) && bloom.active && bloom.intensity.value > 0.45f)
                {
                    hasWarning = true;
                    report.Add($"WARN: Bloom intensity {bloom.intensity.value:0.00} may make the player aura pop too much.");
                }
            }

            var glow = Object.FindAnyObjectByType<TapKnockoutPlayerGlow>(FindObjectsInactive.Include);
            if (glow != null && glow.Config != null)
            {
                var config = glow.Config;
                if (config.GroundGlowColor.a > 0.28f || config.GroundGlowRadius > 1.8f)
                {
                    hasWarning = true;
                    report.Add($"WARN: Ground glow may read as an artificial AoE circle. Radius={config.GroundGlowRadius:0.00}, Alpha={config.GroundGlowColor.a:0.00}.");
                }

                if (config.LanternFieldColor.a > 0.14f || config.LanternFieldRadius > 5.4f)
                {
                    hasWarning = true;
                    report.Add($"WARN: Lantern field may be too visible. Radius={config.LanternFieldRadius:0.00}, Alpha={config.LanternFieldColor.a:0.00}.");
                }
            }

            var message = "Tap Knockout production lighting validation:\n- " + string.Join("\n- ", report);
            if (hasFailure)
            {
                Debug.LogError(message);
            }
            else if (hasWarning)
            {
                Debug.LogWarning(message);
            }
            else
            {
                Debug.Log(message);
            }
        }

        [MenuItem("Tools/Tap Knockout/Visuals/Refresh Production VFX Catalog")]
        public static void RefreshProductionVFXCatalogOnly()
        {
            var report = new List<string>(8);
            EnsureFolders();
            LoadOrCreateVFXCatalog(report);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Tap Knockout production VFX catalog refreshed:\n- " + string.Join("\n- ", report));
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/_Project/ScriptableObjects");
            EnsureFolder(VisualPresetFolder);
            EnsureFolder("Assets/_Project/ScriptableObjects/VFX");
            EnsureFolder(GeneratedVfxFolder);
            EnsureFolder(GeneratedMaterialFolder);
        }

        private static GameplayCameraConfig LoadOrCreateCameraConfig(List<string> report)
        {
            var config = AssetDatabase.LoadAssetAtPath<GameplayCameraConfig>(CameraConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<GameplayCameraConfig>();
                AssetDatabase.CreateAsset(config, CameraConfigPath);
                report.Add($"Created desktop gameplay camera config at {CameraConfigPath}.");
            }

            var serialized = new SerializedObject(config);
            SetFloat(serialized, "pitchDegrees", 52f);
            SetFloat(serialized, "yawDegrees", 0f);
            SetFloat(serialized, "cameraDistance", 18f);
            SetVector3(serialized, "followOffset", Vector3.zero);
            SetVector3(serialized, "lookAtOffset", new Vector3(0f, 0.65f, 0f));
            SetVector2(serialized, "playerViewportAnchor", new Vector2(0.5f, 0.49f));
            SetFloat(serialized, "forwardLookAhead", 0.65f);
            SetBool(serialized, "enableMovementLookAhead", true);
            SetFloat(serialized, "movementLookAheadStrength", 0.9f);
            SetFloat(serialized, "maxMovementLookAhead", 1.4f);
            SetBool(serialized, "enableDashLookAhead", true);
            SetFloat(serialized, "dashLookAheadMultiplier", 1.45f);
            SetFloat(serialized, "dashLookAheadDuration", 0.14f);
            SetFloat(serialized, "positionSmoothTime", 0.1f);
            SetFloat(serialized, "rotationSharpness", 18f);
            SetBool(serialized, "snapOnEnable", true);
            SetBool(serialized, "useOrthographic", true);
            SetFloat(serialized, "fieldOfView", 40f);
            SetFloat(serialized, "orthographicSize", 11.25f);
            SetFloat(serialized, "nearClipPlane", 0.1f);
            SetFloat(serialized, "farClipPlane", 220f);
            SetFloat(serialized, "minimumSupportedAspect", 1.33f);
            SetFloat(serialized, "maximumSupportedAspect", 2.4f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            report.Add("Updated desktop 2.5D gameplay camera config values.");
            return config;
        }

        private static TapKnockoutVisualQualityConfig LoadOrCreateVisualQualityConfig(List<string> report)
        {
            var low = LoadOrCreatePreset(TapKnockoutVisualQualityLevel.PrototypeLow, "TapKnockoutVisualQuality_PrototypeLow.asset", report);
            var medium = LoadOrCreatePreset(TapKnockoutVisualQualityLevel.PrototypeMedium, "TapKnockoutVisualQuality_PrototypeMedium.asset", report);
            var high = LoadOrCreatePreset(TapKnockoutVisualQualityLevel.PrototypeHigh, "TapKnockoutVisualQuality_PrototypeHigh.asset", report);

            var config = AssetDatabase.LoadAssetAtPath<TapKnockoutVisualQualityConfig>(VisualConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<TapKnockoutVisualQualityConfig>();
                AssetDatabase.CreateAsset(config, VisualConfigPath);
                report.Add($"Created visual quality config at {VisualConfigPath}.");
            }

            config.SetPresets(new[] { low, medium, high });
            EditorUtility.SetDirty(config);
            report.Add("Ensured PrototypeLow, PrototypeMedium, and PrototypeHigh visual presets.");
            return config;
        }

        private static TapKnockoutVisualQualityPreset LoadOrCreatePreset(
            TapKnockoutVisualQualityLevel level,
            string fileName,
            List<string> report)
        {
            var path = $"{VisualPresetFolder}/{fileName}";
            var preset = AssetDatabase.LoadAssetAtPath<TapKnockoutVisualQualityPreset>(path);
            if (preset == null)
            {
                preset = ScriptableObject.CreateInstance<TapKnockoutVisualQualityPreset>();
                AssetDatabase.CreateAsset(preset, path);
                report.Add($"Created {level} visual preset.");
            }

            preset.ConfigureDefaults(level);
            EditorUtility.SetDirty(preset);
            return preset;
        }

        private static TapKnockoutLightingConfig LoadOrCreateLightingConfig(List<string> report)
        {
            var config = AssetDatabase.LoadAssetAtPath<TapKnockoutLightingConfig>(LightingConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<TapKnockoutLightingConfig>();
                AssetDatabase.CreateAsset(config, LightingConfigPath);
                report.Add($"Created lighting config at {LightingConfigPath}.");
            }

            var serialized = new SerializedObject(config);
            SetColor(serialized, "mainLightColor", new Color(0.56f, 0.68f, 0.78f, 1f));
            SetFloat(serialized, "mainLightIntensity", 0.52f);
            SetVector3(serialized, "mainLightEuler", new Vector3(54f, -42f, 0f));
            SetFloat(serialized, "mainLightShadowStrength", 0.55f);
            SetBool(serialized, "mainLightSoftShadows", true);
            SetColor(serialized, "ambientSkyColor", new Color(0.052f, 0.066f, 0.072f, 1f));
            SetColor(serialized, "ambientEquatorColor", new Color(0.028f, 0.038f, 0.034f, 1f));
            SetColor(serialized, "ambientGroundColor", new Color(0.014f, 0.016f, 0.014f, 1f));
            SetFloat(serialized, "ambientIntensity", 0.22f);
            SetBool(serialized, "fogEnabled", true);
            SetColor(serialized, "fogColor", new Color(0.026f, 0.038f, 0.036f, 1f));
            SetFloat(serialized, "fogDensity", 0.011f);
            SetInt(serialized, "maxRuntimeAccentLights", 4);
            ConfigureAccentLights(serialized);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            report.Add("Updated lighting config for cool moonlight, soft shadows, non-black ambient, fog, and capped warm edge accents.");
            return config;
        }

        private static TapKnockoutPlayerLightRigConfig LoadOrCreatePlayerLightRigConfig(List<string> report)
        {
            var config = AssetDatabase.LoadAssetAtPath<TapKnockoutPlayerLightRigConfig>(PlayerLightRigConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<TapKnockoutPlayerLightRigConfig>();
                AssetDatabase.CreateAsset(config, PlayerLightRigConfigPath);
                report.Add($"Created player light rig config at {PlayerLightRigConfigPath}.");
            }

            var serialized = new SerializedObject(config);
            SetFloat(serialized, "followSharpness", 28f);
            SetFloat(serialized, "forwardDirectionSharpness", 18f);
            SetFloat(serialized, "movementDirectionDeadZone", 0.05f);
            SetBool(serialized, "localHeroLightEnabled", false);
            SetColor(serialized, "localHeroLightColor", new Color(0.72f, 1f, 0.84f, 1f));
            SetFloat(serialized, "localHeroLightIntensity", 0.35f);
            SetFloat(serialized, "localHeroLightRange", 4.5f);
            SetVector3(serialized, "localHeroLightOffset", new Vector3(0f, 2.65f, 0f));
            SetBool(serialized, "forwardLightEnabled", false);
            SetColor(serialized, "forwardLightColor", new Color(0.5f, 1f, 0.76f, 1f));
            SetFloat(serialized, "forwardLightIntensity", 0.16f);
            SetFloat(serialized, "forwardLightIdleIntensityMultiplier", 0f);
            SetFloat(serialized, "forwardLightRange", 5.5f);
            SetFloat(serialized, "forwardLightSpotAngle", 80f);
            SetFloat(serialized, "forwardLightDownAngle", 48f);
            SetVector3(serialized, "forwardLightOffset", new Vector3(0f, 2.15f, 0.9f));
            SetBool(serialized, "dashPulseEnabled", true);
            SetColor(serialized, "dashPulseColor", new Color(0.78f, 0.92f, 0.9f, 1f));
            SetFloat(serialized, "dashPulseIntensity", 2.4f);
            SetFloat(serialized, "dashPulseRange", 8.5f);
            SetFloat(serialized, "dashPulseDuration", 0.18f);
            SetVector3(serialized, "dashPulseOffset", new Vector3(0f, 2.35f, 1.25f));
            SetBool(serialized, "visibleGlowEnabled", true);
            SetColor(serialized, "groundGlowColor", new Color(0.74f, 0.92f, 0.9f, 0.18f));
            SetFloat(serialized, "groundGlowRadius", 1.45f);
            SetColor(serialized, "coreGlowColor", new Color(0.86f, 0.96f, 0.92f, 0.48f));
            SetVector2(serialized, "coreGlowSize", new Vector2(0.92f, 1.45f));
            SetVector3(serialized, "coreGlowOffset", new Vector3(0f, 0.92f, 0f));
            SetFloat(serialized, "glowPulseAmplitude", 0.045f);
            SetFloat(serialized, "glowPulseFrequency", 0.85f);
            SetBool(serialized, "lanternFieldEnabled", true);
            SetColor(serialized, "lanternFieldColor", new Color(0.66f, 0.82f, 0.84f, 0.08f));
            SetFloat(serialized, "lanternFieldRadius", 4.6f);
            SetVector3(serialized, "lanternFieldOffset", new Vector3(0f, 0.045f, 0f));
            SetBool(serialized, "forwardLanternEnabled", false);
            SetColor(serialized, "forwardLanternColor", new Color(0.78f, 1f, 0.66f, 0.12f));
            SetFloat(serialized, "forwardLanternRange", 5.2f);
            SetFloat(serialized, "forwardLanternWidth", 3.4f);
            SetVector3(serialized, "forwardLanternOffset", new Vector3(0f, 0.055f, 0.25f));
            SetFloat(serialized, "forwardLanternIdleAlphaMultiplier", 0.35f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            report.Add("Updated legacy player light rig config so movement cone/fan is disabled and only subtle glow/dash support remains.");
            return config;
        }

        private static EnvironmentLightingProfile LoadOrCreateEnvironmentLightingProfile(List<string> report)
        {
            var profile = AssetDatabase.LoadAssetAtPath<EnvironmentLightingProfile>(EnvironmentLightingProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<EnvironmentLightingProfile>();
                AssetDatabase.CreateAsset(profile, EnvironmentLightingProfilePath);
                report.Add($"Created environment lighting profile at {EnvironmentLightingProfilePath}.");
            }

            var serialized = new SerializedObject(profile);
            SetString(serialized, "profileId", "forest_arena_environment_default");
            SetEnum(serialized, "qualityTier", (int)LightingQualityTier.Default);
            SetColor(serialized, "moonlightColor", new Color(0.56f, 0.68f, 0.78f, 1f));
            SetFloat(serialized, "moonlightIntensity", 0.52f);
            SetVector3(serialized, "moonlightEuler", new Vector3(54f, -42f, 0f));
            SetEnum(serialized, "moonlightShadows", (int)LightShadows.Soft);
            SetFloat(serialized, "moonlightShadowStrength", 0.55f);
            SetColor(serialized, "ambientSkyColor", new Color(0.052f, 0.066f, 0.072f, 1f));
            SetColor(serialized, "ambientEquatorColor", new Color(0.028f, 0.038f, 0.034f, 1f));
            SetColor(serialized, "ambientGroundColor", new Color(0.014f, 0.016f, 0.014f, 1f));
            SetFloat(serialized, "ambientIntensity", 0.22f);
            SetBool(serialized, "fogEnabled", true);
            SetColor(serialized, "fogColor", new Color(0.026f, 0.038f, 0.036f, 1f));
            SetFloat(serialized, "fogDensity", 0.011f);
            SetFloat(serialized, "postExposure", -0.32f);
            SetFloat(serialized, "contrast", 18f);
            SetFloat(serialized, "saturation", 0f);
            SetFloat(serialized, "bloomIntensity", 0.32f);
            SetFloat(serialized, "bloomThreshold", 1.35f);
            SetFloat(serialized, "vignetteIntensity", 0.22f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            report.Add("Updated forest arena environment lighting profile for dark-but-readable moonlight, ambient, fog, and post values.");
            return profile;
        }

        private static PlayerVisibilityLightingProfile LoadOrCreatePlayerVisibilityLightingProfile(List<string> report)
        {
            var profile = AssetDatabase.LoadAssetAtPath<PlayerVisibilityLightingProfile>(PlayerVisibilityLightingProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<PlayerVisibilityLightingProfile>();
                AssetDatabase.CreateAsset(profile, PlayerVisibilityLightingProfilePath);
                report.Add($"Created player visibility lighting profile at {PlayerVisibilityLightingProfilePath}.");
            }

            var serialized = new SerializedObject(profile);
            SetString(serialized, "profileId", "player_visibility_default");
            SetEnum(serialized, "qualityTier", (int)LightingQualityTier.Default);
            SetBool(serialized, "enableMainAura", true);
            SetColor(serialized, "auraColor", new Color(0.78f, 0.92f, 0.9f, 1f));
            SetFloat(serialized, "auraIntensity", 0.72f);
            SetFloat(serialized, "auraRange", 4.8f);
            SetFloat(serialized, "auraHeightOffset", 2.25f);
            SetFloat(serialized, "auraFollowSharpness", 24f);
            SetEnum(serialized, "auraShadowMode", (int)LightShadows.None);
            SetFloat(serialized, "auraShadowStrength", 0.22f);
            SetFloat(serialized, "auraShadowBias", 0.08f);
            SetFloat(serialized, "auraShadowNormalBias", 0.35f);
            SetBool(serialized, "enableOuterFill", true);
            SetColor(serialized, "outerFillColor", new Color(0.48f, 0.62f, 0.68f, 1f));
            SetFloat(serialized, "outerFillIntensity", 0.16f);
            SetFloat(serialized, "outerFillRange", 10.5f);
            SetFloat(serialized, "outerFillHeightOffset", 2.9f);
            SetEnum(serialized, "outerFillShadowMode", (int)LightShadows.None);
            SetBool(serialized, "enableAimAccent", false);
            SetColor(serialized, "aimAccentColor", new Color(0.74f, 0.9f, 0.88f, 1f));
            SetFloat(serialized, "aimAccentIntensity", 0.06f);
            SetFloat(serialized, "aimAccentRange", 4.5f);
            SetFloat(serialized, "aimAccentSpotAngle", 82f);
            SetFloat(serialized, "aimAccentHeightOffset", 2f);
            SetFloat(serialized, "aimAccentForwardOffset", 1.25f);
            SetFloat(serialized, "aimAccentRotationSharpness", 12f);
            SetEnum(serialized, "aimAccentShadowMode", (int)LightShadows.None);
            SetFloat(serialized, "aimAccentMaxAuraIntensityFraction", 0.12f);
            SetBool(serialized, "enableSubtlePulse", true);
            SetFloat(serialized, "pulseAmplitude", 0.018f);
            SetFloat(serialized, "pulseSpeed", 0.9f);
            SetFloat(serialized, "combatIntensityBoost", 0.04f);
            SetFloat(serialized, "lowHealthPulseBoost", 0.04f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            report.Add("Updated player visibility profile with direction-independent soft aura and disabled-by-default aim accent.");
            return profile;
        }

        private static VolumeProfile LoadOrCreateVolumeProfile(
            TapKnockoutVisualQualityPreset preset,
            List<string> report,
            EnvironmentLightingProfile environmentProfile = null)
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, VolumeProfilePath);
                report.Add($"Created gameplay volume profile at {VolumeProfilePath}.");
            }

            var renderProfile = preset != null ? preset.RenderProfile : null;
            ConfigureVolumeProfile(profile, renderProfile, environmentProfile);
            EditorUtility.SetDirty(profile);
            report.Add("Updated gameplay Global Volume profile with conservative bloom, color, tonemapping, vignette, and disabled blur/DOF.");
            return profile;
        }

        private static VFXCatalog LoadOrCreateVFXCatalog(List<string> report)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<VFXCatalog>(VfxCatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<VFXCatalog>();
                AssetDatabase.CreateAsset(catalog, VfxCatalogPath);
                report.Add($"Created production visual foundation VFX catalog at {VfxCatalogPath}.");
            }

            var definitions = new List<VFXDefinition>
            {
                new VFXDefinition(VFXEventType.DashStart, EnsureParticlePrefab("VFX_DashStart_GreenWhite", new Color(0.42f, 1f, 0.72f, 1f), 18, 0.45f, true), 4, 0.45f),
                new VFXDefinition(VFXEventType.DashTrail, EnsureTrailPrefab("VFX_DashTrail_GreenWhite", new Color(0.35f, 1f, 0.72f, 0.72f), 0.22f, 0.36f), 4, 0.32f, true, true, true, Vector3.up * 0.45f, Vector3.zero, 1f, true),
                new VFXDefinition(VFXEventType.DashEnd, EnsureParticlePrefab("VFX_DashEnd_Ring", new Color(0.85f, 1f, 0.78f, 1f), 20, 0.45f, true), 4, 0.45f),
                new VFXDefinition(VFXEventType.DashImpact, EnsureParticlePrefab("VFX_DashImpact_Gold", new Color(1f, 0.72f, 0.25f, 1f), 18, 0.55f, false), 8, 0.55f),
                new VFXDefinition(VFXEventType.PrimaryFireMuzzle, EnsureParticlePrefab("VFX_PrimaryFireMuzzle", new Color(0.7f, 1f, 0.88f, 1f), 10, 0.28f, false), 8, 0.28f),
                new VFXDefinition(VFXEventType.PrimaryProjectileTrail, EnsureTrailPrefab("VFX_PrimaryProjectileTrail", new Color(0.55f, 0.95f, 1f, 0.68f), 0.16f, 0.28f), 24, 0.5f, true, true, true, Vector3.zero, Vector3.zero, 1f, true),
                new VFXDefinition(VFXEventType.ProjectileHit, EnsureParticlePrefab("VFX_ProjectileHit_Cyan", new Color(0.5f, 0.9f, 1f, 1f), 12, 0.42f, false), 16, 0.42f),
                new VFXDefinition(VFXEventType.EnemyHit, EnsureParticlePrefab("VFX_EnemyHit_ReadabilityFlash", new Color(1f, 0.92f, 0.72f, 1f), 10, 0.32f, false), 16, 0.32f),
                new VFXDefinition(VFXEventType.XPOrbCollect, EnsureParticlePrefab("VFX_XPOrbCollect_Glow", new Color(0.3f, 0.95f, 1f, 1f), 12, 0.45f, false), 16, 0.45f),
                new VFXDefinition(VFXEventType.LevelUpBurst, EnsureParticlePrefab("VFX_LevelUpBurst_GoldGreen", new Color(0.95f, 0.86f, 0.34f, 1f), 26, 0.8f, true), 4, 0.8f),
                new VFXDefinition(VFXEventType.EnemySpawn, EnsureParticlePrefab("VFX_EnemySpawn_DarkRed", new Color(0.95f, 0.28f, 0.18f, 1f), 8, 0.55f, true), 16, 0.55f),
                new VFXDefinition(VFXEventType.BossSpawnWarning, EnsureParticlePrefab("VFX_BossWarning_RedOrange", new Color(1f, 0.22f, 0.08f, 1f), 24, 1.2f, true), 2, 1.2f),
                new VFXDefinition(VFXEventType.BossHit, EnsureParticlePrefab("VFX_BossHit_Heavy", new Color(1f, 0.55f, 0.16f, 1f), 18, 0.55f, false), 8, 0.55f),
                new VFXDefinition(VFXEventType.BossDeath, EnsureParticlePrefab("VFX_BossDeath_Burst", new Color(1f, 0.36f, 0.08f, 1f), 36, 1.5f, true), 2, 1.5f)
            };

            AppendMissingVerticalSliceDefinitions(definitions, report);
            catalog.SetDefinitions(definitions);
            EditorUtility.SetDirty(catalog);
            report.Add("Updated production visual foundation VFX catalog with generated readable combat placeholders and legacy ability VFX coverage.");
            return catalog;
        }

        private static void AppendMissingVerticalSliceDefinitions(List<VFXDefinition> definitions, List<string> report)
        {
            VFXAssetPackCatalogMapper.CreateVerticalSliceVfxCatalog();
            var verticalSliceCatalog = AssetDatabase.LoadAssetAtPath<VFXCatalog>(VFXAssetPackCatalogMapper.CatalogPath);
            if (verticalSliceCatalog == null)
            {
                report.Add("Skipped legacy ability VFX merge because the vertical slice catalog was unavailable.");
                return;
            }

            var seen = new HashSet<VFXEventType>();
            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                if (definition != null)
                {
                    seen.Add(definition.EventType);
                }
            }

            var appended = 0;
            var verticalDefinitions = verticalSliceCatalog.Definitions;
            for (var i = 0; i < verticalDefinitions.Count; i++)
            {
                var definition = verticalDefinitions[i];
                if (definition == null || !seen.Add(definition.EventType))
                {
                    continue;
                }

                definitions.Add(definition);
                appended++;
            }

            report.Add(appended > 0
                ? $"Merged {appended} legacy ability/active-skill VFX event definition(s) into the production catalog."
                : "Production VFX catalog already covered all legacy ability/active-skill VFX events.");
        }

        private static void ApplyURPAssetBaseline(TapKnockoutVisualQualityPreset preset, List<string> report)
        {
            var renderProfile = preset != null ? preset.RenderProfile : null;
            var pipelineAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(PcRenderPipelineAssetPath);
            if (pipelineAsset == null || renderProfile == null)
            {
                report.Add("Skipped URP asset baseline because PC_RPAsset or render profile was unavailable.");
                return;
            }

            var serialized = new SerializedObject(pipelineAsset);
            SetBool(serialized, "m_SupportsHDR", renderProfile.HdrEnabled);
            SetInt(serialized, "m_MSAA", renderProfile.MsaaSampleCount);
            SetFloat(serialized, "m_RenderScale", renderProfile.RenderScale);
            SetBool(serialized, "m_RequireDepthTexture", renderProfile.DepthTextureEnabled);
            SetBool(serialized, "m_RequireOpaqueTexture", renderProfile.OpaqueTextureEnabled);
            SetBool(serialized, "m_MainLightShadowsSupported", renderProfile.MainLightShadowsEnabled);
            SetInt(serialized, "m_MainLightShadowmapResolution", renderProfile.MainLightShadowResolution);
            SetBool(serialized, "m_AdditionalLightShadowsSupported", renderProfile.AdditionalLightShadowsEnabled);
            SetInt(serialized, "m_AdditionalLightsPerObjectLimit", renderProfile.AdditionalLightsPerObjectLimit);
            SetBool(serialized, "m_SoftShadowsSupported", renderProfile.SoftShadowsEnabled);
            SetFloat(serialized, "m_ShadowDistance", renderProfile.ShadowDistance);
            SetInt(serialized, "m_ShadowCascadeCount", renderProfile.ShadowCascadeCount);
            SetBool(serialized, "m_UseSRPBatcher", renderProfile.SrpBatcherEnabled);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pipelineAsset);
            report.Add("Applied performance-conscious URP PC render asset baseline.");
        }

        private static void ApplySSAOFeatureBaseline(TapKnockoutVisualQualityPreset preset, List<string> report)
        {
            var renderProfile = preset != null ? preset.RenderProfile : null;
            var assets = AssetDatabase.LoadAllAssetsAtPath(PcRendererAssetPath);
            if (assets == null || renderProfile == null)
            {
                report.Add("Skipped SSAO renderer feature baseline because PC_Renderer or render profile was unavailable.");
                return;
            }

            for (var i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                if (asset == null || asset.GetType().Name.IndexOf("ScreenSpaceAmbientOcclusion", System.StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                var serialized = new SerializedObject(asset);
                SetBool(serialized, "m_Active", renderProfile.AmbientOcclusionIntensity > 0f);
                SetFloat(serialized, "m_Settings.Intensity", renderProfile.AmbientOcclusionIntensity);
                SetFloat(serialized, "m_Settings.Radius", renderProfile.AmbientOcclusionRadius);
                SetInt(serialized, "m_Settings.Samples", 1);
                SetInt(serialized, "m_Settings.BlurQuality", 0);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(asset);
                report.Add("Applied lightweight SSAO renderer feature baseline on PC_Renderer.");
                return;
            }

            report.Add("PC_Renderer has no SSAO feature to update; existing renderer setup was left untouched.");
        }

        private static void ApplyCameraFoundation(
            GameplayCameraConfig cameraConfig,
            TapKnockoutVisualQualityConfig visualConfig,
            List<string> report)
        {
            var camera = ResolveOrCreateGameplayCamera(report);
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = cameraConfig != null ? cameraConfig.OrthographicSize : 11.25f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 220f;
            camera.allowHDR = true;
            camera.allowMSAA = true;

            var target = ResolvePlayerTarget();
            var controller = ResolveOrAddComponent<GameplayCameraController>(camera.gameObject);
            SetObjectReference(controller, "config", cameraConfig);
            SetObjectReference(controller, "followTarget", target);
            controller.ApplyProjectionSettings();
            if (target != null)
            {
                controller.SetFollowTarget(target, true);
            }

            var dashLookAhead = ResolveOrAddComponent<TapKnockoutCameraDashLookAhead>(camera.gameObject);
            dashLookAhead.Configure(controller, target);
            SetObjectReference(dashLookAhead, "cameraController", controller);
            SetObjectReference(dashLookAhead, "target", target);

            var rig = ResolveOrAddComponent<SurvivorCameraRig>(camera.gameObject);
            rig.ApplySurvivor2_5DPreset(target, target != null);

            var shake = ResolveOrAddComponent<CameraShakeReceiver>(camera.gameObject);
            var qualityApplier = ResolveOrAddComponent<TapKnockoutVisualQualityApplier>(camera.gameObject);
            SetObjectReference(qualityApplier, "config", visualConfig);
            SetObjectReference(qualityApplier, "targetCamera", camera);

            var cameraData = camera.GetUniversalAdditionalCameraData();
            cameraData.renderPostProcessing = true;
            cameraData.requiresDepthOption = CameraOverrideOption.On;
            cameraData.requiresColorOption = CameraOverrideOption.Off;
            cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            cameraData.antialiasingQuality = AntialiasingQuality.Medium;

            EditorUtility.SetDirty(camera);
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(dashLookAhead);
            EditorUtility.SetDirty(rig);
            EditorUtility.SetDirty(shake);
            EditorUtility.SetDirty(qualityApplier);
            EditorUtility.SetDirty(cameraData);
            report.Add("Configured gameplay camera, smooth follow, movement/dash look-ahead, target assignment, camera shake, visual quality applier, and URP post-processing flags.");
        }

        private static void ApplyLightingFoundation(TapKnockoutLightingConfig config, List<string> report)
        {
            if (config == null)
            {
                report.Add("Skipped lighting foundation because lighting config was unavailable.");
                return;
            }

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = config.AmbientSkyColor;
            RenderSettings.ambientEquatorColor = config.AmbientEquatorColor;
            RenderSettings.ambientGroundColor = config.AmbientGroundColor;
            RenderSettings.ambientIntensity = config.AmbientIntensity;
            RenderSettings.fog = config.FogEnabled;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = config.FogColor;
            RenderSettings.fogDensity = config.FogDensity;

            var lightingRoot = ResolveOrCreateRoot("LightingRoot");
            var mainLight = ResolveOrCreateLight(lightingRoot.transform, "TapKnockout_MainDirectionalLight", LightType.Directional);
            mainLight.transform.localRotation = Quaternion.Euler(config.MainLightEuler);
            mainLight.color = config.MainLightColor;
            mainLight.intensity = config.MainLightIntensity;
            mainLight.shadows = config.MainLightSoftShadows ? LightShadows.Soft : LightShadows.Hard;
            mainLight.shadowStrength = config.MainLightShadowStrength;
            mainLight.shadowBias = 0.08f;
            mainLight.shadowNormalBias = 0.45f;
            RenderSettings.sun = mainLight;
            EditorUtility.SetDirty(mainLight);

            var accentsRoot = ResolveOrCreateChild(lightingRoot.transform, "TapKnockout_AccentLights");
            var count = Mathf.Min(config.MaxRuntimeAccentLights, config.AccentLights.Count);
            for (var i = 0; i < count; i++)
            {
                var accent = config.AccentLights[i];
                if (accent == null)
                {
                    continue;
                }

                var light = ResolveOrCreateLight(accentsRoot, $"TapKnockout_{accent.Id}", LightType.Point);
                light.transform.localPosition = accent.LocalPosition;
                light.color = accent.Color;
                light.intensity = accent.Intensity;
                light.range = accent.Range;
                light.shadows = accent.CastsShadows ? LightShadows.Soft : LightShadows.None;
                EditorUtility.SetDirty(light);
            }

            report.Add($"Applied stylized fantasy lighting with one main shadow light and {count} capped accent lights.");
        }

        private static void ApplyEnvironmentLightingProfile(EnvironmentLightingProfile profile, List<string> report)
        {
            if (profile == null)
            {
                report.Add("Skipped environment lighting profile because profile was unavailable.");
                return;
            }

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = profile.AmbientSkyColor;
            RenderSettings.ambientEquatorColor = profile.AmbientEquatorColor;
            RenderSettings.ambientGroundColor = profile.AmbientGroundColor;
            RenderSettings.ambientIntensity = profile.AmbientIntensity;
            RenderSettings.fog = profile.FogEnabled;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = profile.FogColor;
            RenderSettings.fogDensity = profile.FogDensity;

            var lightingRoot = ResolveOrCreateRoot("LightingRoot");
            var moonlight = ResolveOrCreateLight(lightingRoot.transform, "TapKnockout_MainDirectionalLight", LightType.Directional);
            moonlight.transform.localRotation = Quaternion.Euler(profile.MoonlightEuler);
            moonlight.color = profile.MoonlightColor;
            moonlight.intensity = profile.MoonlightIntensity;
            moonlight.shadows = profile.MoonlightShadows;
            moonlight.shadowStrength = profile.MoonlightShadowStrength;
            moonlight.shadowBias = 0.08f;
            moonlight.shadowNormalBias = 0.45f;
            RenderSettings.sun = moonlight;
            EditorUtility.SetDirty(moonlight);

            report.Add("Applied forest environment profile with cool moonlight, soft shadows, controlled ambient, and lightweight fog.");
        }

        private static void ApplyPlayerLightRig(TapKnockoutPlayerLightRigConfig config, List<string> report)
        {
            if (config == null)
            {
                report.Add("Skipped player light rig because player light rig config was unavailable.");
                return;
            }

            var target = ResolvePlayerTarget();
            var movement = target != null
                ? target.GetComponent<PlayerMovementController>()
                : Object.FindAnyObjectByType<PlayerMovementController>();

            var lightingRoot = ResolveOrCreateRoot("LightingRoot");
            var rigRoot = ResolveOrCreateChild(lightingRoot.transform, "TapKnockout_PlayerLightRig");
            var rig = ResolveOrAddComponent<TapKnockoutPlayerLightRig>(rigRoot.gameObject);

            var localHeroLight = ResolveOrCreateLight(rigRoot, "HeroLocalLight", LightType.Point);
            localHeroLight.transform.localPosition = config.LocalHeroLightOffset;
            localHeroLight.color = config.LocalHeroLightColor;
            localHeroLight.intensity = config.LocalHeroLightEnabled ? config.LocalHeroLightIntensity : 0f;
            localHeroLight.range = config.LocalHeroLightRange;
            localHeroLight.shadows = LightShadows.None;
            localHeroLight.enabled = config.LocalHeroLightEnabled;

            var forwardLight = ResolveOrCreateLight(rigRoot, "ForwardMovementLight", LightType.Spot);
            forwardLight.transform.localPosition = config.ForwardLightOffset;
            forwardLight.color = config.ForwardLightColor;
            forwardLight.intensity = config.ForwardLightEnabled ? config.ForwardLightIntensity : 0f;
            forwardLight.range = config.ForwardLightRange;
            forwardLight.spotAngle = config.ForwardLightSpotAngle;
            forwardLight.shadows = LightShadows.None;
            forwardLight.enabled = config.ForwardLightEnabled;

            var dashBurstLight = ResolveOrCreateLight(rigRoot, "DashBurstLight", LightType.Point);
            dashBurstLight.transform.localPosition = config.DashPulseOffset;
            dashBurstLight.color = config.DashPulseColor;
            dashBurstLight.intensity = 0f;
            dashBurstLight.range = config.DashPulseRange;
            dashBurstLight.shadows = LightShadows.None;
            dashBurstLight.enabled = false;

            SetObjectReference(rig, "config", config);
            SetObjectReference(rig, "target", target);
            SetObjectReference(rig, "movementController", movement);
            SetObjectReference(rig, "localHeroLight", localHeroLight);
            SetObjectReference(rig, "forwardLight", forwardLight);
            SetObjectReference(rig, "dashBurstLight", dashBurstLight);
            rig.SetTarget(target, movement, target != null);
            rig.SetConfig(config);
            rig.RefreshLightSettings();

            var glow = ResolveOrAddComponent<TapKnockoutPlayerGlow>(rigRoot.gameObject);
            var gameplayCamera = UnityEngine.Camera.main ?? Object.FindAnyObjectByType<UnityEngine.Camera>();
            glow.Configure(config, target, gameplayCamera, movement);
            SetObjectReference(glow, "config", config);
            SetObjectReference(glow, "target", target);
            SetObjectReference(glow, "movementController", movement);
            SetObjectReference(glow, "billboardCamera", gameplayCamera);

            EditorUtility.SetDirty(rigRoot.gameObject);
            EditorUtility.SetDirty(rig);
            EditorUtility.SetDirty(glow);
            EditorUtility.SetDirty(localHeroLight);
            EditorUtility.SetDirty(forwardLight);
            EditorUtility.SetDirty(dashBurstLight);
            report.Add(target != null
                ? "Configured legacy player glow/dash support with hero and forward primary lights disabled on the current player target."
                : "Configured legacy player glow/dash support; target will need assignment when a player exists in the scene.");
        }

        private static void ApplyPlayerVisibilityLighting(PlayerVisibilityLightingProfile profile, List<string> report)
        {
            if (profile == null)
            {
                report.Add("Skipped player visibility lighting because profile was unavailable.");
                return;
            }

            var target = ResolvePlayerTarget();
            var aimController = target != null
                ? target.GetComponent<MouseAimController>()
                : Object.FindAnyObjectByType<MouseAimController>();

            var lightingRoot = ResolveOrCreateRoot("LightingRoot");
            var rigRoot = ResolveOrCreateChild(lightingRoot.transform, "TapKnockout_PlayerVisibilityLighting");
            if (target != null)
            {
                rigRoot.position = target.position;
            }

            var controller = ResolveOrAddComponent<PlayerVisibilityLightingController>(rigRoot.gameObject);
            var mainAuraLight = ResolveOrCreateLight(rigRoot, "MainAura_PointLight", LightType.Point);
            var outerFillLight = ResolveOrCreateLight(rigRoot, "OuterFill_PointLight", LightType.Point);
            var aimAccentPivot = ResolveOrCreateChild(rigRoot, "AimAccentPivot");
            var aimAccentLight = ResolveOrCreateLight(aimAccentPivot, "OptionalAimAccent_SpotLight", LightType.Spot);

            mainAuraLight.transform.localPosition = Vector3.up * profile.AuraHeightOffset;
            mainAuraLight.transform.localRotation = Quaternion.identity;
            mainAuraLight.color = profile.AuraColor;
            mainAuraLight.intensity = profile.EnableMainAura ? profile.AuraIntensity : 0f;
            mainAuraLight.range = profile.AuraRange;
            mainAuraLight.shadows = profile.AuraShadowMode;
            mainAuraLight.shadowStrength = profile.AuraShadowStrength;
            mainAuraLight.shadowBias = profile.AuraShadowBias;
            mainAuraLight.shadowNormalBias = profile.AuraShadowNormalBias;
            mainAuraLight.enabled = profile.EnableMainAura;

            outerFillLight.transform.localPosition = Vector3.up * profile.OuterFillHeightOffset;
            outerFillLight.transform.localRotation = Quaternion.identity;
            outerFillLight.color = profile.OuterFillColor;
            outerFillLight.intensity = profile.EnableOuterFill ? profile.OuterFillIntensity : 0f;
            outerFillLight.range = profile.OuterFillRange;
            outerFillLight.shadows = profile.OuterFillShadowMode;
            outerFillLight.enabled = profile.EnableOuterFill && outerFillLight.intensity > 0.005f;

            aimAccentPivot.localPosition = Vector3.up * profile.AimAccentHeightOffset;
            aimAccentLight.color = profile.AimAccentColor;
            aimAccentLight.intensity = profile.EnableAimAccent
                ? Mathf.Min(profile.AimAccentIntensity, profile.AuraIntensity * profile.AimAccentMaxAuraIntensityFraction)
                : 0f;
            aimAccentLight.range = profile.AimAccentRange;
            aimAccentLight.spotAngle = profile.AimAccentSpotAngle;
            aimAccentLight.shadows = profile.AimAccentShadowMode;
            aimAccentLight.enabled = profile.EnableAimAccent && aimAccentLight.intensity > 0.005f;

            SetObjectReference(controller, "profile", profile);
            SetObjectReference(controller, "followTarget", target);
            SetObjectReference(controller, "aimController", aimController);
            SetObjectReference(controller, "mainAuraLight", mainAuraLight);
            SetObjectReference(controller, "outerFillLight", outerFillLight);
            SetObjectReference(controller, "aimAccentLight", aimAccentLight);
            SetObjectReference(controller, "aimAccentPivot", aimAccentPivot);
            controller.SetTarget(target);
            controller.SetAimController(aimController);
            controller.ApplyProfile(profile);

            DisableLegacyPrimaryPlayerLights(report);

            EditorUtility.SetDirty(rigRoot.gameObject);
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(mainAuraLight);
            EditorUtility.SetDirty(outerFillLight);
            EditorUtility.SetDirty(aimAccentPivot);
            EditorUtility.SetDirty(aimAccentLight);
            report.Add(target != null
                ? "Configured direction-independent player aura; optional aim accent is bound to MouseAimController and remains secondary."
                : "Configured player visibility lighting rig; follow target will need assignment when a player exists in the scene.");
        }

        private static void DisableLegacyPrimaryPlayerLights(List<string> report)
        {
            var legacyRigs = Object.FindObjectsByType<TapKnockoutPlayerLightRig>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var disabledLights = 0;
            for (var i = 0; i < legacyRigs.Length; i++)
            {
                var rig = legacyRigs[i];
                if (rig == null)
                {
                    continue;
                }

                if (rig.LocalHeroLight != null)
                {
                    rig.LocalHeroLight.enabled = false;
                    rig.LocalHeroLight.intensity = 0f;
                    EditorUtility.SetDirty(rig.LocalHeroLight);
                    disabledLights++;
                }

                if (rig.ForwardLight != null)
                {
                    rig.ForwardLight.enabled = false;
                    rig.ForwardLight.intensity = 0f;
                    EditorUtility.SetDirty(rig.ForwardLight);
                    disabledLights++;
                }

                EditorUtility.SetDirty(rig);
            }

            report.Add(disabledLights > 0
                ? $"Disabled {disabledLights} legacy hero/forward player light(s); new PlayerVisibilityLightingController now owns primary visibility."
                : "No legacy hero/forward player lights needed disabling.");
        }

        private static void ApplyGlobalVolume(VolumeProfile profile, List<string> report)
        {
            var volumeObject = GameObject.Find("TapKnockout_GlobalVolume");
            if (volumeObject == null)
            {
                volumeObject = new GameObject("TapKnockout_GlobalVolume");
                Undo.RegisterCreatedObjectUndo(volumeObject, "Create Tap Knockout Global Volume");
            }

            var volume = ResolveOrAddComponent<Volume>(volumeObject);
            volume.isGlobal = true;
            volume.priority = 0f;
            volume.sharedProfile = profile;
            EditorUtility.SetDirty(volume);
            report.Add("Configured project-owned Global Volume object and profile reference.");
        }

        private static void ApplyRadialDarknessOverlay(TapKnockoutVisualQualityPreset preset, List<string> report)
        {
            var renderProfile = preset != null ? preset.RenderProfile : null;
            var camera = UnityEngine.Camera.main ?? Object.FindAnyObjectByType<UnityEngine.Camera>();
            var target = ResolvePlayerTarget();

            var canvasObject = GameObject.Find("TapKnockout_DarknessOverlayCanvas");
            if (canvasObject == null)
            {
                canvasObject = new GameObject("TapKnockout_DarknessOverlayCanvas");
                Undo.RegisterCreatedObjectUndo(canvasObject, "Create Tap Knockout Darkness Overlay Canvas");
            }

            var canvas = ResolveOrAddComponent<Canvas>(canvasObject);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = -500;
            canvas.pixelPerfect = false;

            var scaler = ResolveOrAddComponent<CanvasScaler>(canvasObject);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var rectTransform = ResolveOrCreateRectChild(canvasObject.transform, "RadialDarknessOverlay");
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;

            var overlay = ResolveOrAddComponent<TapKnockoutRadialDarknessOverlay>(rectTransform.gameObject);
            overlay.Configure(renderProfile, camera, target);
            SetObjectReference(overlay, "worldCamera", camera);
            SetObjectReference(overlay, "target", target);

            EditorUtility.SetDirty(canvasObject);
            EditorUtility.SetDirty(canvas);
            EditorUtility.SetDirty(scaler);
            EditorUtility.SetDirty(rectTransform);
            EditorUtility.SetDirty(overlay);
            report.Add("Configured player-following radial darkness overlay behind gameplay HUD for darker arena edges.");
        }

        private static void ApplyFeedbackFoundation(VFXCatalog catalog, List<string> report)
        {
            var root = GameObject.Find("VFXFeedbackRoot");
            if (root == null)
            {
                root = new GameObject("VFXFeedbackRoot");
                Undo.RegisterCreatedObjectUndo(root, "Create VFX Feedback Root");
            }

            var poolRoot = ResolveOrCreateChild(root.transform, "VFXPoolRoot");
            var vfxService = ResolveOrAddComponent<VFXService>(root);
            var hitPause = ResolveOrAddComponent<HitPauseService>(root);
            var impactFeedback = ResolveOrAddComponent<ImpactFeedbackController>(root);
            var combatVfx = ResolveOrAddComponent<CombatVFXEventController>(root);
            var survivorFeedback = ResolveOrAddComponent<SurvivorFeedbackPlayer>(root);
            var audioSource = ResolveOrAddComponent<AudioSource>(root);
            var cameraShake = Object.FindAnyObjectByType<CameraShakeReceiver>();

            SetObjectReference(vfxService, "poolRoot", poolRoot);
            SetObjectReference(vfxService, "catalog", catalog);
            SetObjectReference(impactFeedback, "vfxService", vfxService);
            SetObjectReference(impactFeedback, "hitPauseService", hitPause);
            SetObjectReference(impactFeedback, "cameraShakeReceiver", cameraShake);
            SetObjectReference(impactFeedback, "audioSource", audioSource);
            SetObjectReference(combatVfx, "vfxService", vfxService);
            SetObjectReference(survivorFeedback, "vfxPoolRoot", poolRoot);
            SetObjectReference(survivorFeedback, "cameraShakeReceiver", cameraShake);

            EditorUtility.SetDirty(root);
            EditorUtility.SetDirty(vfxService);
            EditorUtility.SetDirty(impactFeedback);
            EditorUtility.SetDirty(combatVfx);
            EditorUtility.SetDirty(survivorFeedback);
            report.Add("Configured VFX feedback root with pooled VFX, combat event routing, hit pause, camera shake, and SurvivorFeedbackPlayer hooks.");
        }

        private static void RepairSceneXPOrbVisuals(List<string> report)
        {
            var orbs = Object.FindObjectsByType<XPOrb>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var added = 0;
            for (var i = 0; i < orbs.Length; i++)
            {
                var orb = orbs[i];
                if (orb == null || orb.GetComponent<XPOrbVisualFeedback>() != null)
                {
                    continue;
                }

                Undo.AddComponent<XPOrbVisualFeedback>(orb.gameObject);
                added++;
            }

            report.Add(added > 0
                ? $"Added XPOrbVisualFeedback to {added} scene XP orb object(s)."
                : "Scene XP orb visuals already had glow feedback or no XP orbs were present.");
        }

        private static void ConfigureVolumeProfile(
            VolumeProfile profile,
            TapKnockoutRenderProfile renderProfile,
            EnvironmentLightingProfile environmentProfile)
        {
            var bloom = GetOrAddVolumeComponent<Bloom>(profile);
            bloom.active = true;
            bloom.intensity.Override(environmentProfile != null ? environmentProfile.BloomIntensity : renderProfile != null ? renderProfile.BloomIntensity : 0.32f);
            bloom.threshold.Override(environmentProfile != null ? environmentProfile.BloomThreshold : renderProfile != null ? renderProfile.BloomThreshold : 1.2f);
            bloom.scatter.Override(renderProfile != null ? renderProfile.BloomScatter : 0.58f);
            bloom.highQualityFiltering.Override(false);

            var color = GetOrAddVolumeComponent<ColorAdjustments>(profile);
            color.active = true;
            color.postExposure.Override(environmentProfile != null ? environmentProfile.PostExposure : renderProfile != null ? renderProfile.PostExposure : -0.45f);
            color.contrast.Override(environmentProfile != null ? environmentProfile.Contrast : renderProfile != null ? renderProfile.Contrast : 24f);
            color.saturation.Override(environmentProfile != null ? environmentProfile.Saturation : renderProfile != null ? renderProfile.Saturation : 4f);

            var tonemapping = GetOrAddVolumeComponent<Tonemapping>(profile);
            tonemapping.active = true;
            tonemapping.mode.Override(TonemappingMode.ACES);

            var vignette = GetOrAddVolumeComponent<Vignette>(profile);
            vignette.active = true;
            vignette.intensity.Override(environmentProfile != null ? environmentProfile.VignetteIntensity : renderProfile != null ? renderProfile.VignetteIntensity : 0.28f);
            vignette.smoothness.Override(renderProfile != null ? renderProfile.VignetteSmoothness : 0.5f);

            var grain = GetOrAddVolumeComponent<FilmGrain>(profile);
            grain.active = renderProfile != null && renderProfile.FilmGrainIntensity > 0f;
            grain.intensity.Override(renderProfile != null ? renderProfile.FilmGrainIntensity : 0f);
            grain.response.Override(0.8f);

            var motionBlur = GetOrAddVolumeComponent<MotionBlur>(profile);
            motionBlur.active = renderProfile != null && renderProfile.MotionBlurEnabled;
            motionBlur.intensity.Override(0f);

            var depthOfField = GetOrAddVolumeComponent<DepthOfField>(profile);
            depthOfField.active = renderProfile != null && renderProfile.DepthOfFieldEnabled;
            depthOfField.mode.Override(DepthOfFieldMode.Off);
        }

        private static T GetOrAddVolumeComponent<T>(VolumeProfile profile)
            where T : VolumeComponent
        {
            return profile.TryGet<T>(out var component) ? component : profile.Add<T>(true);
        }

        private static UnityEngine.Camera ResolveOrCreateGameplayCamera(List<string> report)
        {
            var camera = UnityEngine.Camera.main ?? Object.FindAnyObjectByType<UnityEngine.Camera>();
            if (camera != null)
            {
                return camera;
            }

            var cameraRoot = GameObject.Find("CameraRig") ?? ResolveOrCreateRoot("CameraRig");
            var cameraObject = new GameObject("GameplayCamera");
            Undo.RegisterCreatedObjectUndo(cameraObject, "Create Gameplay Camera");
            cameraObject.transform.SetParent(cameraRoot.transform, false);
            camera = cameraObject.AddComponent<UnityEngine.Camera>();
            report.Add("Created GameplayCamera because no camera existed in the active scene.");
            return camera;
        }

        private static Transform ResolvePlayerTarget()
        {
            var playerTagged = GameObject.FindGameObjectWithTag("Player");
            if (playerTagged != null)
            {
                return playerTagged.transform;
            }

            var cameraTarget = Object.FindAnyObjectByType<TapKnockoutCameraTarget>();
            if (cameraTarget != null && cameraTarget.IsPrimaryGameplayTarget)
            {
                return cameraTarget.transform;
            }

            var movement = Object.FindAnyObjectByType<PlayerMovementController>();
            return movement != null ? movement.transform : null;
        }

        private static GameObject ResolveOrCreateRoot(string name)
        {
            var existing = GameObject.Find(name);
            if (existing != null)
            {
                return existing;
            }

            var root = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(root, $"Create {name}");

            var prototypeRoot = GameObject.Find("DesktopSurvivorPrototypeRoot");
            if (prototypeRoot != null && name != prototypeRoot.name)
            {
                root.transform.SetParent(prototypeRoot.transform, false);
            }

            return root;
        }

        private static Transform ResolveOrCreateChild(Transform root, string name)
        {
            var child = root.Find(name);
            if (child != null)
            {
                return child;
            }

            var childObject = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(childObject, $"Create {name}");
            childObject.transform.SetParent(root, false);
            return childObject.transform;
        }

        private static RectTransform ResolveOrCreateRectChild(Transform root, string name)
        {
            var child = root.Find(name);
            if (child != null && child is RectTransform existingRect)
            {
                return existingRect;
            }

            if (child != null)
            {
                var rect = child.GetComponent<RectTransform>();
                if (rect != null)
                {
                    return rect;
                }
            }

            var childObject = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(childObject, $"Create {name}");
            var rectTransform = childObject.GetComponent<RectTransform>();
            rectTransform.SetParent(root, false);
            return rectTransform;
        }

        private static Light ResolveOrCreateLight(Transform parent, string name, LightType lightType)
        {
            var child = parent.Find(name);
            GameObject lightObject;
            if (child == null)
            {
                lightObject = new GameObject(name);
                Undo.RegisterCreatedObjectUndo(lightObject, $"Create {name}");
                lightObject.transform.SetParent(parent, false);
            }
            else
            {
                lightObject = child.gameObject;
            }

            var light = lightObject.GetComponent<Light>();
            if (light == null)
            {
                light = Undo.AddComponent<Light>(lightObject);
            }

            light.type = lightType;
            return light;
        }

        private static GameObject EnsureParticlePrefab(
            string name,
            Color color,
            int burstCount,
            float lifetime,
            bool ringShape)
        {
            var path = $"{GeneratedVfxFolder}/{name}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                return prefab;
            }

            var root = new GameObject(name);
            var particleSystem = root.AddComponent<ParticleSystem>();
            ConfigureParticleSystem(particleSystem, color, burstCount, lifetime, ringShape);
            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = EnsureVFXMaterial($"{name}_MAT", color);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static GameObject EnsureTrailPrefab(string name, Color color, float width, float lifetime)
        {
            var path = $"{GeneratedVfxFolder}/{name}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                return prefab;
            }

            var root = new GameObject(name);
            var trail = root.AddComponent<TrailRenderer>();
            trail.time = Mathf.Max(0.05f, lifetime);
            trail.startWidth = Mathf.Max(0.01f, width);
            trail.endWidth = 0f;
            trail.minVertexDistance = 0.08f;
            trail.numCornerVertices = 2;
            trail.numCapVertices = 2;
            trail.emitting = true;
            trail.autodestruct = false;
            trail.sharedMaterial = EnsureVFXMaterial($"{name}_MAT", color);
            trail.startColor = color;
            trail.endColor = new Color(color.r, color.g, color.b, 0f);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static void ConfigureParticleSystem(
            ParticleSystem particleSystem,
            Color color,
            int burstCount,
            float lifetime,
            bool ringShape)
        {
            var main = particleSystem.main;
            main.loop = false;
            main.duration = Mathf.Max(0.05f, lifetime * 0.65f);
            main.startLifetime = Mathf.Max(0.05f, lifetime);
            main.startSpeed = ringShape ? 1.6f : 1.05f;
            main.startSize = ringShape ? 0.22f : 0.16f;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            var emission = particleSystem.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)Mathf.Max(1, burstCount)) });

            var shape = particleSystem.shape;
            shape.enabled = true;
            shape.shapeType = ringShape ? ParticleSystemShapeType.Circle : ParticleSystemShapeType.Cone;
            shape.radius = ringShape ? 0.75f : 0.18f;
            shape.angle = ringShape ? 0f : 18f;

            var colorOverLifetime = particleSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
                new[] { new GradientAlphaKey(color.a, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = gradient;
        }

        private static Material EnsureVFXMaterial(string name, Color color)
        {
            var path = $"{GeneratedMaterialFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                ?? Shader.Find("Particles/Standard Unlit")
                ?? Shader.Find("Sprites/Default");
            material = new Material(shader)
            {
                name = name,
                color = color
            };

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_EmissionColor"))
            {
                material.SetColor("_EmissionColor", color * 1.4f);
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void ConfigureAccentLights(SerializedObject serialized)
        {
            var accents = serialized.FindProperty("accentLights");
            if (accents == null || !accents.isArray)
            {
                return;
            }

            accents.arraySize = 4;
            ConfigureAccent(accents.GetArrayElementAtIndex(0), "north_west_torch", new Vector3(-16f, 2.2f, 16f));
            ConfigureAccent(accents.GetArrayElementAtIndex(1), "north_east_torch", new Vector3(16f, 2.2f, 16f));
            ConfigureAccent(accents.GetArrayElementAtIndex(2), "south_west_torch", new Vector3(-16f, 2.2f, -16f));
            ConfigureAccent(accents.GetArrayElementAtIndex(3), "south_east_torch", new Vector3(16f, 2.2f, -16f));
        }

        private static void ConfigureAccent(SerializedProperty accent, string id, Vector3 localPosition)
        {
            SetString(accent, "id", id);
            SetVector3(accent, "localPosition", localPosition);
            SetColor(accent, "color", new Color(1f, 0.46f, 0.2f, 1f));
            SetFloat(accent, "intensity", 1.25f);
            SetFloat(accent, "range", 4.8f);
            SetBool(accent, "castsShadows", false);
        }

        private static T ResolveOrAddComponent<T>(GameObject target)
            where T : Component
        {
            return target.TryGetComponent<T>(out var existing) ? existing : Undo.AddComponent<T>(target);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/");
            var folder = System.IO.Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(folder))
            {
                return;
            }

            EnsureFolder(parent);
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, folder);
            }
        }

        private static float ComputeColorSaturation(Color color)
        {
            var max = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
            var min = Mathf.Min(color.r, Mathf.Min(color.g, color.b));
            return Mathf.Approximately(max, 0f) ? 0f : (max - min) / max;
        }

        private static bool IsGreenDominant(Color color)
        {
            return color.g > color.r * 1.18f && color.g > color.b * 1.08f && ComputeColorSaturation(color) > 0.22f;
        }

        private static bool IsNearlyBlack(Color color)
        {
            return Mathf.Max(color.r, Mathf.Max(color.g, color.b)) < 0.012f;
        }

        private static void SetObjectReference(Object target, string propertyName, Object value)
        {
            if (target == null)
            {
                return;
            }

            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                return;
            }

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetBool(SerializedObject serialized, string propertyName, bool value)
        {
            var property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.boolValue = value;
            }
        }

        private static void SetInt(SerializedObject serialized, string propertyName, int value)
        {
            var property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.intValue = value;
            }
        }

        private static void SetString(SerializedObject serialized, string propertyName, string value)
        {
            var property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.stringValue = value;
            }
        }

        private static void SetEnum(SerializedObject serialized, string propertyName, int value)
        {
            var property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.enumValueIndex = value;
            }
        }

        private static void SetString(SerializedProperty root, string propertyName, string value)
        {
            var property = root.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.stringValue = value;
            }
        }

        private static void SetBool(SerializedProperty root, string propertyName, bool value)
        {
            var property = root.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.boolValue = value;
            }
        }

        private static void SetFloat(SerializedObject serialized, string propertyName, float value)
        {
            var property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private static void SetFloat(SerializedProperty root, string propertyName, float value)
        {
            var property = root.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private static void SetVector2(SerializedObject serialized, string propertyName, Vector2 value)
        {
            var property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.vector2Value = value;
            }
        }

        private static void SetColor(SerializedObject serialized, string propertyName, Color value)
        {
            var property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.colorValue = value;
            }
        }

        private static void SetColor(SerializedProperty root, string propertyName, Color value)
        {
            var property = root.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.colorValue = value;
            }
        }

        private static void SetVector3(SerializedObject serialized, string propertyName, Vector3 value)
        {
            var property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.vector3Value = value;
            }
        }

        private static void SetVector3(SerializedProperty root, string propertyName, Vector3 value)
        {
            var property = root.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.vector3Value = value;
            }
        }
    }
}
#endif
