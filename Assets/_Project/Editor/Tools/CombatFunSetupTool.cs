using System.Text;
using TapKnockout.Enemy;
using TapKnockout.Feedback;
using TapKnockout.Player;
using TapKnockout.VFX;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TapKnockout.Editor.Tools
{
    public static class CombatFunSetupTool
    {
        private const string MenuPath = "Tools/Tap Knockout/Combat/Apply Phase 2 Combat Fun Setup";
        private const string ValidateMenuPath = "Tools/Tap Knockout/Combat/Validate Phase 2 Combat Fun Setup";
        private const string CombatConfigFolder = "Assets/_Project/ScriptableObjects/Combat";
        private const string PerfectDashConfigPath = CombatConfigFolder + "/PerfectDashConfig_Default.asset";
        private const string WallSlamConfigPath = CombatConfigFolder + "/WallSlamConfig_Default.asset";
        private const string ChainKnockbackConfigPath = CombatConfigFolder + "/ChainKnockbackConfig_Default.asset";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player/PF_Player_Rogue_Generated.prefab";
        private const string BasicMeleeEnemyPrefabPath = "Assets/_Project/Prefabs/Enemies/Generated/PF_Enemy_BasicMelee_GreenDemon_Generated.prefab";
        private const string FeedbackRootName = "VFXFeedbackRoot";

        [MenuItem(MenuPath)]
        public static void ApplySetup()
        {
            EnsureFolder(CombatConfigFolder);

            var perfectDashConfig = LoadOrCreateAsset<PerfectDashConfig>(PerfectDashConfigPath);
            var wallSlamConfig = LoadOrCreateAsset<WallSlamConfig>(WallSlamConfigPath);
            var chainKnockbackConfig = LoadOrCreateAsset<ChainKnockbackConfig>(ChainKnockbackConfigPath);
            var wallLayerMask = ResolveWallLayerMask();
            var enemyLayerMask = ResolveEnemyLayerMask();
            var report = new StringBuilder();

            ConfigurePerfectDashConfig(perfectDashConfig);
            ConfigureWallSlamConfig(wallSlamConfig, wallLayerMask);
            ConfigureChainKnockbackConfig(chainKnockbackConfig, enemyLayerMask);

            report.AppendLine("Phase 2 Combat Fun Setup");
            report.AppendLine($"- PerfectDashConfig: {PerfectDashConfigPath}");
            report.AppendLine($"- WallSlamConfig: {WallSlamConfigPath} mask={wallLayerMask.value}");
            report.AppendLine($"- ChainKnockbackConfig: {ChainKnockbackConfigPath} mask={enemyLayerMask.value}");

            ApplyScenePlayerSetup(perfectDashConfig, report);
            ApplySceneFeedbackSetup(report);
            ApplyPlayerPrefabSetup(perfectDashConfig, report);
            ApplyEnemyPrefabSetup(wallSlamConfig, chainKnockbackConfig, report);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid() && activeScene.isLoaded)
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
            }

            Debug.Log(report.ToString());
        }

        [MenuItem(ValidateMenuPath)]
        public static void ValidateSetup()
        {
            var report = new StringBuilder();
            report.AppendLine("Phase 2 Combat Fun Setup Validation");

            var player = ResolveScenePlayer();
            var feedbackRoot = GameObject.Find(FeedbackRootName);
            var wallSlamConfig = AssetDatabase.LoadAssetAtPath<WallSlamConfig>(WallSlamConfigPath);
            var chainKnockbackConfig = AssetDatabase.LoadAssetAtPath<ChainKnockbackConfig>(ChainKnockbackConfigPath);

            report.AppendLine(player != null && player.GetComponent<PerfectDashDetector>() != null
                ? "- Scene Player: PerfectDashDetector OK"
                : "- Scene Player: PerfectDashDetector MISSING");
            report.AppendLine(feedbackRoot != null && feedbackRoot.GetComponent<LowHealthFeedbackController>() != null
                ? "- Feedback Root: LowHealthFeedbackController OK"
                : "- Feedback Root: LowHealthFeedbackController MISSING");
            report.AppendLine(wallSlamConfig != null ? "- WallSlamConfig asset OK" : "- WallSlamConfig asset MISSING");
            report.AppendLine(chainKnockbackConfig != null ? "- ChainKnockbackConfig asset OK" : "- ChainKnockbackConfig asset MISSING");

            Debug.Log(report.ToString());
        }

        private static void ApplyScenePlayerSetup(PerfectDashConfig perfectDashConfig, StringBuilder report)
        {
            var player = ResolveScenePlayer();
            if (player == null)
            {
                report.AppendLine("- Scene Player: not found");
                return;
            }

            var detector = ResolveOrAddSceneComponent<PerfectDashDetector>(player);
            SetObjectReference(detector, "playerHealth", player.GetComponent<PlayerHealth>());
            SetObjectReference(detector, "dashController", player.GetComponent<PlayerDashController>());
            SetObjectReference(detector, "config", perfectDashConfig);
            EditorUtility.SetDirty(detector);
            PrefabUtility.RecordPrefabInstancePropertyModifications(detector);
            report.AppendLine($"- Scene Player: PerfectDashDetector wired on {player.name}");
        }

        private static void ApplySceneFeedbackSetup(StringBuilder report)
        {
            var feedbackRoot = GameObject.Find(FeedbackRootName);
            if (feedbackRoot == null)
            {
                VFXFeedbackSetupBuilder.CreateFeedbackSystemRoot();
                feedbackRoot = GameObject.Find(FeedbackRootName);
            }

            if (feedbackRoot == null)
            {
                report.AppendLine("- Feedback Root: not found or could not be created");
                return;
            }

            var lowHealthFeedback = ResolveOrAddSceneComponent<LowHealthFeedbackController>(feedbackRoot);
            var player = ResolveScenePlayer();
            SetObjectReference(lowHealthFeedback, "playerHealth", player != null ? player.GetComponent<PlayerHealth>() : null);
            SetObjectReference(lowHealthFeedback, "vfxService", feedbackRoot.GetComponent<VFXService>());
            EditorUtility.SetDirty(lowHealthFeedback);
            report.AppendLine($"- Feedback Root: LowHealthFeedbackController wired on {feedbackRoot.name}");
        }

        private static void ApplyPlayerPrefabSetup(PerfectDashConfig perfectDashConfig, StringBuilder report)
        {
            var root = LoadPrefabRoot(PlayerPrefabPath, report);
            if (root == null)
            {
                return;
            }

            try
            {
                var detector = ResolveOrAddPrefabComponent<PerfectDashDetector>(root);
                SetObjectReference(detector, "playerHealth", root.GetComponentInChildren<PlayerHealth>(true));
                SetObjectReference(detector, "dashController", root.GetComponentInChildren<PlayerDashController>(true));
                SetObjectReference(detector, "config", perfectDashConfig);
                SavePrefabRoot(root, PlayerPrefabPath, report, "Player prefab");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ApplyEnemyPrefabSetup(WallSlamConfig wallSlamConfig, ChainKnockbackConfig chainKnockbackConfig, StringBuilder report)
        {
            var root = LoadPrefabRoot(BasicMeleeEnemyPrefabPath, report);
            if (root == null)
            {
                return;
            }

            try
            {
                var knockbackReceiver = root.GetComponentInChildren<KnockbackReceiver>(true);
                if (knockbackReceiver == null)
                {
                    knockbackReceiver = ResolveOrAddPrefabComponent<KnockbackReceiver>(root);
                }

                SetObjectReference(knockbackReceiver, "wallSlamConfig", wallSlamConfig);
                SetObjectReference(knockbackReceiver, "chainKnockbackConfig", chainKnockbackConfig);
                SavePrefabRoot(root, BasicMeleeEnemyPrefabPath, report, "BasicMelee enemy prefab");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static GameObject ResolveScenePlayer()
        {
            var dashController = Object.FindFirstObjectByType<PlayerDashController>();
            if (dashController != null)
            {
                return dashController.gameObject;
            }

            var playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                return playerHealth.gameObject;
            }

            return GameObject.Find("Player");
        }

        private static T ResolveOrAddSceneComponent<T>(GameObject target)
            where T : Component
        {
            if (target.TryGetComponent<T>(out var existing))
            {
                return existing;
            }

            return Undo.AddComponent<T>(target);
        }

        private static T ResolveOrAddPrefabComponent<T>(GameObject target)
            where T : Component
        {
            if (target.TryGetComponent<T>(out var existing))
            {
                return existing;
            }

            return target.AddComponent<T>();
        }

        private static GameObject LoadPrefabRoot(string prefabPath, StringBuilder report)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                report.AppendLine($"- Prefab missing: {prefabPath}");
                return null;
            }

            return PrefabUtility.LoadPrefabContents(prefabPath);
        }

        private static void SavePrefabRoot(GameObject root, string prefabPath, StringBuilder report, string label)
        {
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out var success);
            report.AppendLine(success ? $"- {label}: saved" : $"- {label}: save failed");
        }

        private static PerfectDashConfig ConfigurePerfectDashConfig(PerfectDashConfig config)
        {
            SetFloat(config, "eventDebounceSeconds", 0.05f);
            SetFloat(config, "cooldownRefundSeconds", 0.35f);
            SetBool(config, "raiseProjectileDodgeEvents", true);
            SetBool(config, "refundDashCooldown", true);
            EditorUtility.SetDirty(config);
            return config;
        }

        private static WallSlamConfig ConfigureWallSlamConfig(WallSlamConfig config, LayerMask wallLayerMask)
        {
            SetBool(config, "enabled", true);
            SetLayerMask(config, "wallLayers", wallLayerMask);
            SetFloat(config, "minKnockbackForce", 2f);
            SetFloat(config, "baseDamage", 6f);
            SetFloat(config, "damagePerKnockbackForce", 0.5f);
            SetFloat(config, "cooldownSeconds", 0.2f);
            SetBool(config, "stopKnockbackOnSlam", true);
            EditorUtility.SetDirty(config);
            return config;
        }

        private static ChainKnockbackConfig ConfigureChainKnockbackConfig(ChainKnockbackConfig config, LayerMask enemyLayerMask)
        {
            SetBool(config, "enabled", true);
            SetLayerMask(config, "targetLayers", enemyLayerMask);
            SetFloat(config, "baseDamage", 4f);
            SetFloat(config, "damagePerKnockbackForce", 0.25f);
            SetFloat(config, "secondaryKnockbackForceMultiplier", 0.35f);
            SetFloat(config, "targetCooldownSeconds", 0.2f);
            SetInt(config, "maxHitsPerKnockback", 1);
            EditorUtility.SetDirty(config);
            return config;
        }

        private static LayerMask ResolveWallLayerMask()
        {
            var explicitWallLayer = ResolveFirstExistingLayer("Wall", "Walls", "Environment", "Arena", "Room");
            if (explicitWallLayer >= 0)
            {
                return 1 << explicitWallLayer;
            }

            var sceneMask = ResolveSceneLayerMaskByName("wall", "border", "bounds", "arena");
            return sceneMask.value != 0 ? sceneMask : 1 << 0;
        }

        private static LayerMask ResolveEnemyLayerMask()
        {
            var enemyLayer = LayerMask.NameToLayer("Enemy");
            return enemyLayer >= 0 ? 1 << enemyLayer : ~0;
        }

        private static int ResolveFirstExistingLayer(params string[] layerNames)
        {
            for (var i = 0; i < layerNames.Length; i++)
            {
                var layer = LayerMask.NameToLayer(layerNames[i]);
                if (layer >= 0)
                {
                    return layer;
                }
            }

            return -1;
        }

        private static LayerMask ResolveSceneLayerMaskByName(params string[] nameFragments)
        {
            var colliders = Object.FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var mask = 0;
            for (var i = 0; i < colliders.Length; i++)
            {
                if (ContainsAnyNameFragment(colliders[i].transform, nameFragments))
                {
                    mask |= 1 << colliders[i].gameObject.layer;
                }
            }

            return mask;
        }

        private static bool ContainsAnyNameFragment(Transform transform, string[] fragments)
        {
            while (transform != null)
            {
                var lowerName = transform.name.ToLowerInvariant();
                for (var i = 0; i < fragments.Length; i++)
                {
                    if (lowerName.Contains(fragments[i]))
                    {
                        return true;
                    }
                }

                transform = transform.parent;
            }

            return false;
        }

        private static T LoadOrCreateAsset<T>(string path)
            where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string folderPath)
        {
            var parts = folderPath.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static void SetObjectReference(Object target, string propertyName, Object value)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                return;
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetBool(Object target, string propertyName, bool value)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.boolValue = value;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetFloat(Object target, string propertyName, float value)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.floatValue = value;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetInt(Object target, string propertyName, int value)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.intValue = value;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetLayerMask(Object target, string propertyName, LayerMask value)
        {
            SetInt(target, propertyName, value.value);
        }
    }
}
