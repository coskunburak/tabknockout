#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace TapKnockout.Editor.Tools
{
    public static class TapKnockoutLightingCleanupTool
    {
        private const string ForestArenaScenePath = "Assets/_Project/Scenes/DesktopSurvivorPrototype_ForestArena.unity";

        private static readonly string[] AddedLightingObjectNames =
        {
            "TapKnockout_GlobalVolume",
            "TapKnockout_DarknessOverlayCanvas",
            "TapKnockout_PlayerVisibilityLighting",
            "TapKnockout_PlayerLightRig",
            "TapKnockout_AccentLights",
            "TapKnockout_MainDirectionalLight"
        };

        private static readonly string[] ExistingTorchLightNames =
        {
            "TorchLight_NE",
            "TorchLight_NW",
            "TorchLight_SE",
            "TorchLight_SW"
        };

        private static readonly string[] MissingScriptCleanupObjectNames =
        {
            "CameraRig"
        };

        private static readonly string[] VisualComponentTypeNames =
        {
            "TapKnockout.Visuals.TapKnockoutRadialDarknessOverlay",
            "TapKnockout.Visuals.PlayerVisibilityLightingController",
            "TapKnockout.Visuals.TapKnockoutPlayerLightRig",
            "TapKnockout.Visuals.TapKnockoutPlayerGlow",
            "TapKnockout.Visuals.TapKnockoutVisualQualityApplier"
        };

        [MenuItem("Tools/Tap Knockout/Visuals/Remove Production Lighting Pass")]
        public static void RemoveProductionLightingPass()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("Stop Play Mode before removing the production lighting pass.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ForestArenaScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                throw new InvalidOperationException($"Could not open scene at {ForestArenaScenePath}.");
            }

            var report = new List<string>();
            var removedObjectCount = 0;
            var removedComponentCount = 0;

            foreach (var objectName in AddedLightingObjectNames)
            {
                removedObjectCount += RemoveGameObjectsByName(objectName, report);
            }

            foreach (var typeName in VisualComponentTypeNames)
            {
                removedComponentCount += RemoveComponentsByFullName(typeName, report);
            }

            foreach (var torchName in ExistingTorchLightNames)
            {
                removedComponentCount += RemoveComponentsByFullName(
                    "UnityEngine.Rendering.Universal.UniversalAdditionalLightData",
                    torchName,
                    report);
            }

            removedComponentCount += RemoveMissingScriptsOnKnownObjects(report);

            RestoreOriginalRenderSettings(report);
            RestoreOriginalCameraRenderOverrides(report);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log(
                "Tap Knockout production lighting pass removed:\n- " +
                string.Join("\n- ", report) +
                $"\n- Removed objects: {removedObjectCount}" +
                $"\n- Removed components: {removedComponentCount}");
        }

        private static int RemoveGameObjectsByName(string objectName, List<string> report)
        {
            var matches = new List<GameObject>();
            foreach (var transform in UnityEngine.Object.FindObjectsByType<Transform>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (transform.name == objectName)
                {
                    matches.Add(transform.gameObject);
                }
            }

            foreach (var gameObject in matches)
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }

            if (matches.Count > 0)
            {
                report.Add($"Removed {matches.Count} scene object(s) named {objectName}.");
            }

            return matches.Count;
        }

        private static int RemoveComponentsByFullName(string typeFullName, List<string> report)
        {
            var components = new List<Component>();
            foreach (var component in UnityEngine.Object.FindObjectsByType<Component>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (component != null && component.GetType().FullName == typeFullName)
                {
                    components.Add(component);
                }
            }

            return DestroyComponents(typeFullName, components, report);
        }

        private static int RemoveComponentsByFullName(string typeFullName, string ownerName, List<string> report)
        {
            var components = new List<Component>();
            foreach (var transform in UnityEngine.Object.FindObjectsByType<Transform>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (transform.name != ownerName)
                {
                    continue;
                }

                foreach (var component in transform.GetComponents<Component>())
                {
                    if (component != null && component.GetType().FullName == typeFullName)
                    {
                        components.Add(component);
                    }
                }
            }

            return DestroyComponents($"{typeFullName} on {ownerName}", components, report);
        }

        private static int DestroyComponents(string label, IReadOnlyList<Component> components, List<string> report)
        {
            foreach (var component in components)
            {
                UnityEngine.Object.DestroyImmediate(component);
            }

            if (components.Count > 0)
            {
                report.Add($"Removed {components.Count} component(s): {label}.");
            }

            return components.Count;
        }

        private static int RemoveMissingScriptsOnKnownObjects(List<string> report)
        {
            var removed = 0;
            foreach (var objectName in MissingScriptCleanupObjectNames)
            {
                foreach (var transform in UnityEngine.Object.FindObjectsByType<Transform>(
                             FindObjectsInactive.Include,
                             FindObjectsSortMode.None))
                {
                    if (transform.name != objectName)
                    {
                        continue;
                    }

                    removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject);
                }
            }

            if (removed > 0)
            {
                report.Add($"Removed {removed} missing script component(s) left by the production lighting pass.");
            }

            return removed;
        }

        private static void RestoreOriginalRenderSettings(List<string> report)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.38f, 0.49f, 0.44f, 1f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0072f;
            RenderSettings.ambientSkyColor = new Color(0.5f, 0.62f, 0.56f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.29f, 0.37f, 0.28f, 1f);
            RenderSettings.ambientGroundColor = new Color(0.11f, 0.1f, 0.08f, 1f);
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.sun = null;

            report.Add("Restored original forest arena fog, ambient lighting, and sun reference.");
        }

        private static void RestoreOriginalCameraRenderOverrides(List<string> report)
        {
            var cameraDataType = Type.GetType(
                "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");

            if (cameraDataType == null)
            {
                report.Add("Skipped camera render override restore because UniversalAdditionalCameraData type was unavailable.");
                return;
            }

            var updated = 0;
            foreach (var component in UnityEngine.Object.FindObjectsByType<Component>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (component == null || component.GetType() != cameraDataType)
                {
                    continue;
                }

                var gameObject = component.gameObject;
                if (gameObject.name != "CameraRig")
                {
                    continue;
                }

                var serializedObject = new SerializedObject(component);
                SetInt(serializedObject, "m_RequiresDepthTextureOption", 2);
                SetInt(serializedObject, "m_RequiresOpaqueTextureOption", 2);
                SetBool(serializedObject, "m_RenderPostProcessing", false);
                SetInt(serializedObject, "m_Antialiasing", 0);
                SetInt(serializedObject, "m_AntialiasingQuality", 2);
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                updated++;
            }

            if (updated > 0)
            {
                report.Add($"Restored original camera render overrides on {updated} CameraRig component(s).");
            }
        }

        private static void SetInt(SerializedObject serializedObject, string propertyName, int value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.intValue = value;
            }
        }

        private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.boolValue = value;
            }
        }
    }
}
#endif
