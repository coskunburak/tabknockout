using TapKnockout.Camera;
using TapKnockout.Feedback;
using TapKnockout.Player;
using TapKnockout.UI;
using TapKnockout.VFX;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TapKnockout.Editor.Tools
{
    public static class VFXFeedbackSetupBuilder
    {
        private const string RootName = "VFXFeedbackRoot";
        private const string PoolRootName = "VFXPoolRoot";
        private const string ImpactFeedbackConfigPath = "Assets/_Project/VFX/ImpactFeedbackConfig.asset";

        [MenuItem("Tools/Tap Knockout/VFX/Create Feedback System Root")]
        public static void CreateFeedbackSystemRoot()
        {
            var root = GameObject.Find(RootName);
            if (root == null)
            {
                root = new GameObject(RootName);
                Undo.RegisterCreatedObjectUndo(root, "Create VFX Feedback Root");
            }

            var poolRoot = ResolveOrCreateChild(root.transform, PoolRootName);
            var vfxService = ResolveOrAddComponent<VFXService>(root);
            var hitPauseService = ResolveOrAddComponent<HitPauseService>(root);
            var impactFeedbackController = ResolveOrAddComponent<ImpactFeedbackController>(root);
            var combatVfxEventController = ResolveOrAddComponent<CombatVFXEventController>(root);
            var abilityVfxFeedbackController = ResolveOrAddComponent<AbilityVFXFeedbackController>(root);
            var damageNumberSpawner = ResolveOrAddComponent<DamageNumberSpawner>(root);
            var audioSource = ResolveOrAddComponent<AudioSource>(root);
            var cameraShakeReceiver = ResolveOrCreateCameraShakeReceiver(root);
            var gameplayCanvas = Object.FindFirstObjectByType<Canvas>();
            var playerRuntimeStats = Object.FindFirstObjectByType<PlayerRuntimeStats>();
            VFXAssetPackCatalogMapper.CreateVerticalSliceVfxCatalog();
            var verticalSliceCatalog = AssetDatabase.LoadAssetAtPath<VFXCatalog>(VFXAssetPackCatalogMapper.CatalogPath);
            var impactFeedbackConfig = LoadOrCreateImpactFeedbackConfig();

            SetObjectReference(vfxService, "poolRoot", poolRoot);
            if (verticalSliceCatalog != null)
            {
                SetObjectReference(vfxService, "catalog", verticalSliceCatalog);
            }

            SetObjectReference(impactFeedbackController, "vfxService", vfxService);
            SetObjectReference(impactFeedbackController, "hitPauseService", hitPauseService);
            SetObjectReference(impactFeedbackController, "cameraShakeReceiver", cameraShakeReceiver);
            SetObjectReference(impactFeedbackController, "damageNumberSpawner", damageNumberSpawner);
            SetObjectReference(impactFeedbackController, "audioSource", audioSource);
            SetObjectReference(impactFeedbackController, "config", impactFeedbackConfig);
            SetObjectReference(combatVfxEventController, "vfxService", vfxService);
            SetObjectReference(abilityVfxFeedbackController, "vfxService", vfxService);

            if (playerRuntimeStats != null)
            {
                SetObjectReference(abilityVfxFeedbackController, "playerRuntimeStats", playerRuntimeStats);
                SetObjectReference(abilityVfxFeedbackController, "playerAnchor", playerRuntimeStats.transform);
            }

            if (gameplayCanvas != null)
            {
                SetObjectReference(damageNumberSpawner, "targetCanvas", gameplayCanvas);
            }

            var damageNumberPrefab = PrototypeVerticalSlicePrefabBuilder.EnsureDamageNumberPrefab();
            SetObjectReference(damageNumberSpawner, "numberPrefab", damageNumberPrefab);

            EditorUtility.SetDirty(root);
            if (cameraShakeReceiver != null)
            {
                EditorUtility.SetDirty(cameraShakeReceiver.gameObject);
            }

            if (impactFeedbackConfig != null)
            {
                EditorUtility.SetDirty(impactFeedbackConfig);
            }

            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
            }

            Selection.activeGameObject = root;
        }

        [MenuItem("Tools/Tap Knockout/VFX/Ensure Impact Feedback Config Profiles")]
        public static void EnsureImpactFeedbackConfigProfiles()
        {
            var config = LoadOrCreateImpactFeedbackConfig();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Ensured impact feedback profiles in {ImpactFeedbackConfigPath}.", config);
        }

        private static ImpactFeedbackConfig LoadOrCreateImpactFeedbackConfig()
        {
            EnsureFolder("Assets/_Project", "VFX");
            var config = AssetDatabase.LoadAssetAtPath<ImpactFeedbackConfig>(ImpactFeedbackConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<ImpactFeedbackConfig>();
                config.EnsureProfileDefaults();
                ApplyRecommendedImpactFeedbackToggles(config);
                AssetDatabase.CreateAsset(config, ImpactFeedbackConfigPath);
            }
            else
            {
                config.EnsureProfileDefaults();
                ApplyRecommendedImpactFeedbackToggles(config);
            }

            return config;
        }

        private static void ApplyRecommendedImpactFeedbackToggles(ImpactFeedbackConfig config)
        {
            if (config == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(config);
            SetBool(serializedObject, "enableHitPause", true);
            SetBool(serializedObject, "enableHitFlash", true);
            SetBool(serializedObject, "enableCameraShake", true);
            SetBool(serializedObject, "enableDamageNumbers", true);
            SetBool(serializedObject, "enableVFX", true);
            SetBool(serializedObject, "enableSFXHooks", true);
            SetEnum(serializedObject, "reticleFirePulseVFX", (int)VFXEventType.ReticleFirePulse);
            ApplyRecommendedShotFiredProfile(serializedObject);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ApplyRecommendedShotFiredProfile(SerializedObject serializedObject)
        {
            var profiles = serializedObject.FindProperty("profiles");
            if (profiles == null || !profiles.isArray)
            {
                return;
            }

            for (var i = 0; i < profiles.arraySize; i++)
            {
                var profile = profiles.GetArrayElementAtIndex(i);
                var profileId = profile.FindPropertyRelative("profileId");
                if (profileId == null || profileId.intValue != (int)ImpactFeedbackProfileId.ShotFired)
                {
                    continue;
                }

                SetRelativeBool(profile, "showDamageNumber", false);
                SetRelativeBool(profile, "applyHitStop", false);
                SetRelativeBool(profile, "spawnVFX", true);
                SetRelativeEnum(profile, "vfxEvent", (int)VFXEventType.PrimaryFireMuzzle);
                SetRelativeBool(profile, "pulseReticle", true);
                SetRelativeBool(profile, "playMuzzleFlash", true);
                return;
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

        private static void SetEnum(SerializedObject serializedObject, string propertyName, int value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.intValue = value;
            }
        }

        private static void SetRelativeBool(SerializedProperty parent, string propertyName, bool value)
        {
            var property = parent.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.boolValue = value;
            }
        }

        private static void SetRelativeEnum(SerializedProperty parent, string propertyName, int value)
        {
            var property = parent.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.intValue = value;
            }
        }

        private static void EnsureFolder(string parent, string child)
        {
            var path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static Transform ResolveOrCreateChild(Transform root, string childName)
        {
            var existing = root.Find(childName);
            if (existing != null)
            {
                return existing;
            }

            var child = new GameObject(childName);
            Undo.RegisterCreatedObjectUndo(child, $"Create {childName}");
            child.transform.SetParent(root, false);
            return child.transform;
        }

        private static T ResolveOrAddComponent<T>(GameObject target)
            where T : Component
        {
            if (target.TryGetComponent<T>(out var existing))
            {
                return existing;
            }

            return Undo.AddComponent<T>(target);
        }

        private static CameraShakeReceiver ResolveOrCreateCameraShakeReceiver(GameObject fallbackRoot)
        {
            var selectedCamera = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponent<UnityEngine.Camera>()
                : null;
            var camera = selectedCamera != null ? selectedCamera : UnityEngine.Camera.main;
            if (camera == null)
            {
                camera = Object.FindFirstObjectByType<UnityEngine.Camera>();
            }

            var targetObject = camera != null ? camera.gameObject : fallbackRoot;
            return ResolveOrAddComponent<CameraShakeReceiver>(targetObject);
        }

        private static void SetObjectReference(Object target, string propertyName, Object value)
        {
            if (target == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                return;
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
