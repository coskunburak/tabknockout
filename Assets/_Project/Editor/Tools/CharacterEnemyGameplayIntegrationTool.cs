using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TapKnockout.Ability;
using TapKnockout.Camera;
using TapKnockout.Characters;
using TapKnockout.Enemy;
using TapKnockout.Feedback;
using TapKnockout.Level;
using TapKnockout.Player;
using TapKnockout.UI;
using TapKnockout.Wave;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TapKnockout.Editor.Tools
{
    public static class CharacterEnemyGameplayIntegrationTool
    {
        private const string ReportPath = "Assets/_Project/Docs/CharacterEnemyGameplayIntegrationReport.md";
        private const string PlayerAnimationRepairReportPath = "Assets/_Project/Docs/PlayerAnimationWiringRepairReport.md";
        private const string EnemyAnimationRepairReportPath = "Assets/_Project/Docs/EnemyAnimationWiringRepairReport.md";
        private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
        private const string GeneratedEnemyConfigRoot = "Assets/_Project/ScriptableObjects/Enemies/Generated";

        [MenuItem("Tools/Tap Knockout/Characters/Repair Player Animation Wiring")]
        public static void RepairPlayerAnimationWiring()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    "Player Animation Wiring",
                    "Exit Play Mode before repairing player animation wiring.",
                    "OK");
                return;
            }

            var report = new StringBuilder();
            report.AppendLine("# Player Animation Wiring Repair Report");
            report.AppendLine();

            RepairPlayerPrefabAnimation(report);
            RepairOpenScenePlayerAnimation(report);

            WriteTextAsset(PlayerAnimationRepairReportPath, report.ToString());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(report.ToString());
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(
                    "Player Animation Wiring",
                    "Player prefab and open-scene Player animation wiring were repaired. Review the report in Assets/_Project/Docs.",
                    "OK");
            }
        }

        [MenuItem("Tools/Tap Knockout/Characters/Repair Enemy Animation Wiring")]
        public static void RepairEnemyAnimationWiring()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    "Enemy Animation Wiring",
                    "Exit Play Mode before repairing enemy animation wiring.",
                    "OK");
                return;
            }

            var report = new StringBuilder();
            report.AppendLine("# Enemy Animation Wiring Repair Report");
            report.AppendLine();
            report.AppendLine("This pass rebuilds project-owned Animator Controllers, repairs generated enemy prefabs, and repairs enemy instances in the open scene.");
            report.AppendLine();

            var animationReport = CharacterEnemyAnimationControllerBuilder.BuildAndApplyAnimationControllersInternal();
            report.AppendLine("## Animation Builder Summary");
            AppendIndented(report, ExtractSummary(animationReport));

            RepairEnemyPrefabAnimations(report);
            RepairOpenSceneEnemyAnimations(report);

            WriteTextAsset(EnemyAnimationRepairReportPath, report.ToString());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(report.ToString());
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(
                    "Enemy Animation Wiring",
                    "Generated enemy prefabs and open-scene enemy animation wiring were repaired. Review the report in Assets/_Project/Docs.",
                    "OK");
            }
        }

        [MenuItem("Tools/Tap Knockout/Characters/Apply Generated Characters To Gameplay")]
        public static void ApplyGeneratedCharactersToGameplay()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    "Character Gameplay Integration",
                    "Exit Play Mode before applying generated character/enemy gameplay integration.",
                    "OK");
                return;
            }

            var report = new StringBuilder();
            report.AppendLine("# Character Enemy Gameplay Integration Report");
            report.AppendLine();
            report.AppendLine("This pass wires project-owned generated character/enemy prefabs into gameplay assets and the open scene.");
            report.AppendLine("Source asset packs and ThirdParty folders are not modified.");
            report.AppendLine();

            var roleConfigs = CreateOrUpdateRoleConfigs(report);

            CharacterEnemyPrefabBuilder.BuildSelectedCharacterEnemyPrefabs();
            report.AppendLine("- Generated character/enemy prefabs rebuilt.");

            var animationReport = CharacterEnemyAnimationControllerBuilder.BuildAndApplyAnimationControllersInternal();
            report.AppendLine("- Animator Controllers rebuilt and applied to generated prefabs.");
            report.AppendLine();
            report.AppendLine("## Animation Builder Summary");
            AppendIndented(report, ExtractSummary(animationReport));

            RewriteVerticalSliceWaves(roleConfigs, report);
            ApplyGeneratedPlayerToOpenScene(report);
            EditorSceneManager.SaveOpenScenes();
            report.AppendLine("- Open scenes saved.");

            WriteTextAsset(ReportPath, report.ToString());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(report.ToString());
            EditorUtility.DisplayDialog(
                "Character Gameplay Integration",
                "Generated characters, animation controllers, enemy configs, waves, and open-scene player visuals were updated. Review the report in Assets/_Project/Docs.",
                "OK");
        }

        private static void RepairPlayerPrefabAnimation(StringBuilder report)
        {
            var prefabPath = CharacterEnemyAssetSelection.Player.GeneratedPrefabPath;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                report.AppendLine($"- Player prefab missing: `{prefabPath}`");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                ConfigureScenePlayerAnimation(root);
                var saveSuccess = false;
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out saveSuccess);
                report.AppendLine(saveSuccess
                    ? $"- Player prefab repaired: `{prefabPath}`"
                    : $"- Player prefab save failed: `{prefabPath}`");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void RepairOpenScenePlayerAnimation(StringBuilder report)
        {
            var player = ResolveScenePlayer();
            if (player == null && File.Exists(SampleScenePath))
            {
                EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
                player = ResolveScenePlayer();
            }

            if (player == null)
            {
                report.AppendLine("- Open scene Player not found.");
                return;
            }

            var revertedAnimatorOverrides = RevertAddedAnimatorOverrides(player);
            ConfigureScenePlayerAnimation(player);
            PrefabUtility.RecordPrefabInstancePropertyModifications(player);
            EditorSceneManager.MarkSceneDirty(player.scene);
            EditorSceneManager.SaveOpenScenes();
            report.AppendLine($"- Open scene Player repaired: `{player.name}`");
            report.AppendLine($"- Reverted scene-only Animator overrides: {revertedAnimatorOverrides}");
        }

        private static void RepairEnemyPrefabAnimations(StringBuilder report)
        {
            report.AppendLine("## Enemy Prefabs");
            var specs = CharacterEnemyAssetSelection.Enemies;
            if (specs.Length <= 0)
            {
                report.AppendLine("- No enemy asset specs configured.");
                report.AppendLine();
                return;
            }

            for (var i = 0; i < specs.Length; i++)
            {
                var spec = specs[i];
                var prefabPath = spec.GeneratedPrefabPath;
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                {
                    report.AppendLine($"- Enemy prefab missing: `{prefabPath}`");
                    continue;
                }

                var root = PrefabUtility.LoadPrefabContents(prefabPath);
                try
                {
                    ConfigureSceneEnemyAnimation(root, spec);
                    var saveSuccess = false;
                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out saveSuccess);
                    report.AppendLine(saveSuccess
                        ? $"- `{spec.DisplayName}` prefab repaired: `{prefabPath}`"
                        : $"- `{spec.DisplayName}` prefab save failed: `{prefabPath}`");
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            report.AppendLine();
        }

        private static void RepairOpenSceneEnemyAnimations(StringBuilder report)
        {
            report.AppendLine("## Open Scene Enemies");
            if (CharacterEnemyAssetSelection.Enemies.Length <= 0)
            {
                report.AppendLine("- No enemy asset specs configured.");
                report.AppendLine();
                return;
            }

            var enemies = UnityEngine.Object.FindObjectsByType<EnemyController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (enemies.Length <= 0 && File.Exists(SampleScenePath))
            {
                EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
                enemies = UnityEngine.Object.FindObjectsByType<EnemyController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            }

            if (enemies.Length <= 0)
            {
                report.AppendLine("- Open scene enemies not found.");
                report.AppendLine();
                return;
            }

            for (var i = 0; i < enemies.Length; i++)
            {
                var enemy = enemies[i];
                if (enemy == null)
                {
                    continue;
                }

                var spec = ResolveEnemySpecForObject(enemy.gameObject);
                var animator = ConfigureSceneEnemyAnimation(enemy.gameObject, spec);
                PrefabUtility.RecordPrefabInstancePropertyModifications(enemy.gameObject);
                if (animator != null)
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(animator);
                }

                var driver = enemy.GetComponent<CharacterAnimationDriver>();
                if (driver != null)
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(driver);
                }

                EditorSceneManager.MarkSceneDirty(enemy.gameObject.scene);
                report.AppendLine($"- Open scene enemy repaired: `{enemy.name}` as `{spec.RoleId}`");
            }

            EditorSceneManager.SaveOpenScenes();
            report.AppendLine($"- Open scene enemies repaired: {enemies.Length}");
            report.AppendLine();
        }

        private static int RevertAddedAnimatorOverrides(GameObject root)
        {
            if (root == null)
            {
                return 0;
            }

            var revertedCount = 0;
            var animators = root.GetComponentsInChildren<Animator>(true);
            for (var i = 0; i < animators.Length; i++)
            {
                var animator = animators[i];
                if (animator == null || !PrefabUtility.IsAddedComponentOverride(animator))
                {
                    continue;
                }

                PrefabUtility.RevertAddedComponent(animator, InteractionMode.AutomatedAction);
                revertedCount++;
            }

            return revertedCount;
        }

        private static Dictionary<CharacterEnemyRoleId, EnemyConfig> CreateOrUpdateRoleConfigs(StringBuilder report)
        {
            EnsureFolder(GeneratedEnemyConfigRoot);
            var configs = new Dictionary<CharacterEnemyRoleId, EnemyConfig>();
            var specs = EnemyConfigSpecs();

            report.AppendLine("## Enemy Configs");
            for (var i = 0; i < specs.Length; i++)
            {
                var spec = specs[i];
                var config = AssetDatabase.LoadAssetAtPath<EnemyConfig>(spec.Path);
                if (config == null)
                {
                    config = ScriptableObject.CreateInstance<EnemyConfig>();
                    AssetDatabase.CreateAsset(config, spec.Path);
                }

                ApplyEnemyConfigSpec(config, spec);
                configs[spec.RoleId] = config;
                report.AppendLine($"- `{spec.RoleId}` -> `{spec.Path}`");
            }

            report.AppendLine();
            return configs;
        }

        private static void ApplyEnemyConfigSpec(EnemyConfig config, EnemyConfigSpec spec)
        {
            var serializedObject = new SerializedObject(config);
            serializedObject.FindProperty("enemyId").stringValue = spec.EnemyId;
            serializedObject.FindProperty("maxHealth").floatValue = spec.MaxHealth;
            serializedObject.FindProperty("deathDelay").floatValue = spec.DeathDelay;
            serializedObject.FindProperty("moveSpeed").floatValue = spec.MoveSpeed;
            serializedObject.FindProperty("acceleration").floatValue = spec.Acceleration;
            serializedObject.FindProperty("rotationSpeed").floatValue = spec.RotationSpeed;
            serializedObject.FindProperty("stoppingDistance").floatValue = spec.StoppingDistance;
            serializedObject.FindProperty("contactDamage").floatValue = spec.ContactDamage;
            serializedObject.FindProperty("attackRange").floatValue = spec.AttackRange;
            serializedObject.FindProperty("attackCooldown").floatValue = spec.AttackCooldown;
            serializedObject.FindProperty("knockbackResistance").floatValue = spec.KnockbackResistance;
            serializedObject.FindProperty("canBeKnockedBack").boolValue = spec.CanBeKnockedBack;
            serializedObject.FindProperty("canBeInterrupted").boolValue = spec.CanBeInterrupted;
            serializedObject.FindProperty("coinReward").intValue = spec.CoinReward;
            serializedObject.FindProperty("xpReward").intValue = spec.XpReward;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }

        private static void RewriteVerticalSliceWaves(
            IReadOnlyDictionary<CharacterEnemyRoleId, EnemyConfig> roleConfigs,
            StringBuilder report)
        {
            var waveSpecs = WaveRewriteSpecs();
            report.AppendLine("## Vertical Slice Waves");

            for (var i = 0; i < waveSpecs.Length; i++)
            {
                var waveSpec = waveSpecs[i];
                var wave = AssetDatabase.LoadAssetAtPath<WaveConfig>(waveSpec.Path);
                if (wave == null)
                {
                    report.AppendLine($"- Missing wave: `{waveSpec.Path}`");
                    continue;
                }

                var serializedObject = new SerializedObject(wave);
                var enemiesProperty = serializedObject.FindProperty("enemies");
                enemiesProperty.arraySize = waveSpec.Entries.Length;

                for (var entryIndex = 0; entryIndex < waveSpec.Entries.Length; entryIndex++)
                {
                    var entrySpec = waveSpec.Entries[entryIndex];
                    var entryProperty = enemiesProperty.GetArrayElementAtIndex(entryIndex);
                    entryProperty.FindPropertyRelative("enemyConfig").objectReferenceValue = roleConfigs.TryGetValue(entrySpec.RoleId, out var config)
                        ? config
                        : null;
                    entryProperty.FindPropertyRelative("enemyPrefab").objectReferenceValue = LoadEnemyPrefab(entrySpec.RoleId);
                    entryProperty.FindPropertyRelative("count").intValue = Mathf.Max(0, entrySpec.Count);
                    entryProperty.FindPropertyRelative("spawnDelay").floatValue = Mathf.Max(0f, entrySpec.SpawnDelay);
                    entryProperty.FindPropertyRelative("spawnPointIndex").intValue = Mathf.Max(-1, entrySpec.SpawnPointIndex);
                }

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(wave);
                report.AppendLine($"- `{wave.name}` rewritten with {waveSpec.Entries.Length} role entries.");
            }

            report.AppendLine();
        }

        private static void ApplyGeneratedPlayerToOpenScene(StringBuilder report)
        {
            report.AppendLine("## Open Scene Player");

            var player = ResolveScenePlayer();
            var generatedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterEnemyAssetSelection.Player.GeneratedPrefabPath);
            if (generatedPrefab == null)
            {
                report.AppendLine($"- Skipped: missing generated player prefab `{CharacterEnemyAssetSelection.Player.GeneratedPrefabPath}`.");
                report.AppendLine();
                return;
            }

            if (player == null)
            {
                var created = PrefabUtility.InstantiatePrefab(generatedPrefab) as GameObject;
                if (created == null)
                {
                    report.AppendLine("- Skipped: could not instantiate generated player prefab.");
                    report.AppendLine();
                    return;
                }

                Undo.RegisterCreatedObjectUndo(created, "Create Generated Player");
                created.name = "Player";
                player = created;
                report.AppendLine("- Created generated Player prefab instance in the open scene.");
            }
            else if (IsGeneratedPrefabInstance(player, generatedPrefab))
            {
                player = ReplaceScenePlayerWithGeneratedPrefab(
                    player,
                    generatedPrefab,
                    "Refreshed generated scene player prefab instance to clear stale visual and Animator overrides.",
                    report);
            }
            else
            {
                player = ReplaceScenePlayerWithGeneratedPrefab(
                    player,
                    generatedPrefab,
                    "Replaced legacy scene player with generated player prefab instance.",
                    report);
            }

            ConfigureScenePlayerAnimation(player);
            WirePlayerSocketReferences(player);
            RemoveRootPlaceholderGeometry(player);
            WireOpenSceneReferences(player.transform, report);
            EditorSceneManager.MarkSceneDirty(player.scene);
            report.AppendLine();
        }

        private static GameObject ReplaceScenePlayerWithGeneratedPrefab(
            GameObject oldPlayer,
            GameObject generatedPrefab,
            string successMessage,
            StringBuilder report)
        {
            var oldTransform = oldPlayer.transform;
            var oldParent = oldTransform.parent;
            var oldPosition = oldTransform.position;
            var oldRotation = oldTransform.rotation;
            var oldScale = oldTransform.localScale;
            var oldName = oldPlayer.name;
            var oldScene = oldPlayer.scene;

            var created = PrefabUtility.InstantiatePrefab(generatedPrefab, oldParent) as GameObject;
            if (created == null)
            {
                report.AppendLine($"- Failed to replace `{oldPlayer.name}` with generated player prefab; keeping existing object.");
                return oldPlayer;
            }

            Undo.RegisterCreatedObjectUndo(created, "Create Generated Player");
            created.name = oldName;
            created.transform.SetPositionAndRotation(oldPosition, oldRotation);
            created.transform.localScale = oldScale;
            created.tag = oldPlayer.tag;
            created.layer = oldPlayer.layer;

            Undo.DestroyObjectImmediate(oldPlayer);
            EditorSceneManager.MarkSceneDirty(oldScene);
            report.AppendLine($"- {successMessage} `{oldName}`.");
            return created;
        }

        private static bool IsGeneratedPrefabInstance(GameObject player, GameObject generatedPrefab)
        {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(player);
            return source == generatedPrefab;
        }

        private static Avatar ResolveAvatar(string visualAssetPath)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(visualAssetPath);
            for (var i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Avatar avatar)
                {
                    return avatar;
                }
            }

            return null;
        }

        private static void ConfigureScenePlayerAnimation(GameObject player)
        {
            var visualRoot = FindChild(player.transform, "VisualRoot");
            var animator = visualRoot != null ? ResolveVisualAnimator(visualRoot) : player.GetComponentInChildren<Animator>(true);
            if (animator == null)
            {
                var target = visualRoot != null && visualRoot.childCount > 0 ? visualRoot.GetChild(0) : player.transform;
                animator = target.gameObject.AddComponent<Animator>();
            }

            var controllerPath = CharacterEnemyAnimationControllerBuilder.GetControllerPath(CharacterEnemyRoleId.MainPlayer);
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            var resolvedAvatar = ResolveAvatar(CharacterEnemyAssetSelection.Player.VisualAssetPath);
            if (resolvedAvatar != null)
            {
                animator.avatar = resolvedAvatar;
            }

            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
            }

            animator.applyRootMotion = false;
            animator.updateMode = AnimatorUpdateMode.Normal;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            var driver = player.GetComponent<CharacterAnimationDriver>() ?? player.AddComponent<CharacterAnimationDriver>();
            var serializedDriver = new SerializedObject(driver);
            serializedDriver.FindProperty("animator").objectReferenceValue = animator;
            serializedDriver.FindProperty("isPlayer").boolValue = true;
            serializedDriver.FindProperty("playerMovement").objectReferenceValue = player.GetComponent<PlayerMovementController>();
            serializedDriver.FindProperty("playerAttack").objectReferenceValue = player.GetComponent<PlayerAttackController>();
            serializedDriver.FindProperty("playerDash").objectReferenceValue = player.GetComponent<PlayerDashController>();
            serializedDriver.FindProperty("playerHealth").objectReferenceValue = player.GetComponent<PlayerHealth>();
            SetObjectReference(serializedDriver, "enemyMovement", null);
            SetObjectReference(serializedDriver, "enemyAttack", null);
            SetObjectReference(serializedDriver, "enemyHealth", null);
            SetObjectReference(serializedDriver, "enemyKnockbackReceiver", null);
            serializedDriver.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(driver);
        }

        private static Animator ConfigureSceneEnemyAnimation(GameObject enemy, CharacterEnemyAssetSpec spec)
        {
            var visualRoot = FindChild(enemy.transform, "VisualRoot");
            var animator = visualRoot != null ? ResolveVisualAnimator(visualRoot) : enemy.GetComponentInChildren<Animator>(true);
            if (animator == null)
            {
                var target = visualRoot != null && visualRoot.childCount > 0 ? visualRoot.GetChild(0) : enemy.transform;
                animator = target.gameObject.AddComponent<Animator>();
            }

            var controllerPath = CharacterEnemyAnimationControllerBuilder.GetControllerPath(spec.RoleId);
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            var resolvedAvatar = ResolveAvatar(spec.VisualAssetPath);
            if (resolvedAvatar != null)
            {
                animator.avatar = resolvedAvatar;
            }

            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
            }

            animator.applyRootMotion = false;
            animator.updateMode = AnimatorUpdateMode.Normal;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            EditorUtility.SetDirty(animator);

            RemoveChildAnimationDrivers(enemy);
            var driver = enemy.GetComponent<CharacterAnimationDriver>() ?? enemy.AddComponent<CharacterAnimationDriver>();
            var serializedDriver = new SerializedObject(driver);
            SetObjectReference(serializedDriver, "animator", animator);
            serializedDriver.FindProperty("isPlayer").boolValue = false;
            SetObjectReference(serializedDriver, "playerMovement", null);
            SetObjectReference(serializedDriver, "playerAttack", null);
            SetObjectReference(serializedDriver, "playerDash", null);
            SetObjectReference(serializedDriver, "playerHealth", null);
            SetObjectReference(serializedDriver, "enemyMovement", enemy.GetComponent<EnemyMovement>());
            SetObjectReference(serializedDriver, "enemyAttack", enemy.GetComponent<EnemyAttackController>());
            SetObjectReference(serializedDriver, "enemyHealth", enemy.GetComponent<EnemyHealth>());
            SetObjectReference(serializedDriver, "enemyKnockbackReceiver", enemy.GetComponent<KnockbackReceiver>());
            serializedDriver.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(driver);

            return animator;
        }

        private static void RemoveChildAnimationDrivers(GameObject root)
        {
            var drivers = root.GetComponentsInChildren<CharacterAnimationDriver>(true);
            for (var i = 0; i < drivers.Length; i++)
            {
                var driver = drivers[i];
                if (driver == null || driver.gameObject == root)
                {
                    continue;
                }

                UnityEngine.Object.DestroyImmediate(driver);
            }
        }

        private static void SetObjectReference(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        private static Animator ResolveVisualAnimator(Transform visualRoot)
        {
            var animators = visualRoot.GetComponentsInChildren<Animator>(true);
            var preferred = SelectPreferredVisualAnimator(visualRoot, animators, requireController: true, preferNonRoot: true)
                ?? SelectPreferredVisualAnimator(visualRoot, animators, requireController: true, preferNonRoot: false)
                ?? SelectPreferredVisualAnimator(visualRoot, animators, requireController: false, preferNonRoot: true)
                ?? SelectPreferredVisualAnimator(visualRoot, animators, requireController: false, preferNonRoot: false);

            for (var i = 0; i < animators.Length; i++)
            {
                if (animators[i] != null && animators[i] != preferred)
                {
                    UnityEngine.Object.DestroyImmediate(animators[i]);
                }
            }

            if (preferred != null)
            {
                return preferred;
            }

            var target = visualRoot.childCount > 0 ? visualRoot.GetChild(0) : visualRoot;
            return target.gameObject.AddComponent<Animator>();
        }

        private static Animator SelectPreferredVisualAnimator(
            Transform visualRoot,
            Animator[] animators,
            bool requireController,
            bool preferNonRoot)
        {
            for (var i = 0; i < animators.Length; i++)
            {
                var candidate = animators[i];
                if (candidate == null)
                {
                    continue;
                }

                if (requireController && candidate.runtimeAnimatorController == null)
                {
                    continue;
                }

                if (preferNonRoot && candidate.transform == visualRoot)
                {
                    continue;
                }

                return candidate;
            }

            return null;
        }

        private static void WirePlayerSocketReferences(GameObject player)
        {
            var projectileSpawnPoint = FindChild(player.transform, "ProjectileSpawnPoint");
            var dashHitVolume = FindChild(player.transform, "DashHitVolume");

            var attack = player.GetComponent<PlayerAttackController>();
            if (attack != null && projectileSpawnPoint != null)
            {
                var serializedObject = new SerializedObject(attack);
                serializedObject.FindProperty("projectileSpawnPoint").objectReferenceValue = projectileSpawnPoint;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(attack);
            }

            var dash = player.GetComponent<PlayerDashController>();
            if (dash != null && dashHitVolume != null)
            {
                var serializedObject = new SerializedObject(dash);
                serializedObject.FindProperty("hitQueryOrigin").objectReferenceValue = dashHitVolume;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(dash);
            }
        }

        private static void WireOpenSceneReferences(Transform player, StringBuilder report)
        {
            var spawners = UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < spawners.Length; i++)
            {
                var serializedObject = new SerializedObject(spawners[i]);
                serializedObject.FindProperty("playerTarget").objectReferenceValue = player;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(spawners[i]);
            }

            var cameraControllers = UnityEngine.Object.FindObjectsByType<GameplayCameraController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < cameraControllers.Length; i++)
            {
                cameraControllers[i].SetFollowTarget(player, true);
                var serializedObject = new SerializedObject(cameraControllers[i]);
                serializedObject.FindProperty("followTarget").objectReferenceValue = player;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(cameraControllers[i]);
            }

            var playerHealth = player.GetComponent<PlayerHealth>();
            var playerDash = player.GetComponent<PlayerDashController>();
            var playerRuntimeStats = player.GetComponent<PlayerRuntimeStats>();
            var playerAbilityApplier = player.GetComponent<PlayerAbilityEffectApplier>();

            var abilityControllers = UnityEngine.Object.FindObjectsByType<AbilitySelectionController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < abilityControllers.Length; i++)
            {
                var serializedObject = new SerializedObject(abilityControllers[i]);
                serializedObject.FindProperty("abilityEffectApplier").objectReferenceValue = playerAbilityApplier;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                abilityControllers[i].SetAbilityEffectApplier(playerAbilityApplier);
                EditorUtility.SetDirty(abilityControllers[i]);
            }

            var rewardFlowControllers = UnityEngine.Object.FindObjectsByType<ChapterRoomRewardFlowController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < rewardFlowControllers.Length; i++)
            {
                var serializedObject = new SerializedObject(rewardFlowControllers[i]);
                serializedObject.FindProperty("playerHealth").objectReferenceValue = playerHealth;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(rewardFlowControllers[i]);
            }

            var abilityVfxControllers = UnityEngine.Object.FindObjectsByType<AbilityVFXFeedbackController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < abilityVfxControllers.Length; i++)
            {
                var serializedObject = new SerializedObject(abilityVfxControllers[i]);
                serializedObject.FindProperty("playerRuntimeStats").objectReferenceValue = playerRuntimeStats;
                serializedObject.FindProperty("playerAnchor").objectReferenceValue = player;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                abilityVfxControllers[i].SetPlayerRuntimeStats(playerRuntimeStats);
                abilityVfxControllers[i].SetPlayerAnchor(player);
                EditorUtility.SetDirty(abilityVfxControllers[i]);
            }

            var healthHuds = UnityEngine.Object.FindObjectsByType<PlayerHealthHudController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < healthHuds.Length; i++)
            {
                var serializedObject = new SerializedObject(healthHuds[i]);
                serializedObject.FindProperty("playerHealth").objectReferenceValue = playerHealth;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                healthHuds[i].SetPlayerHealth(playerHealth);
                EditorUtility.SetDirty(healthHuds[i]);
            }

            var dashHuds = UnityEngine.Object.FindObjectsByType<DashCooldownHudController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < dashHuds.Length; i++)
            {
                var serializedObject = new SerializedObject(dashHuds[i]);
                serializedObject.FindProperty("dashController").objectReferenceValue = playerDash;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                dashHuds[i].SetDashController(playerDash);
                EditorUtility.SetDirty(dashHuds[i]);
            }

            report.AppendLine($"- Rewired open-scene EnemySpawner targets: {spawners.Length}");
            report.AppendLine($"- Rewired open-scene camera follow targets: {cameraControllers.Length}");
            report.AppendLine($"- Rewired ability selection controllers: {abilityControllers.Length}");
            report.AppendLine($"- Rewired chapter reward flow controllers: {rewardFlowControllers.Length}");
            report.AppendLine($"- Rewired ability VFX controllers: {abilityVfxControllers.Length}");
            report.AppendLine($"- Rewired player HUD controllers: {healthHuds.Length + dashHuds.Length}");
        }

        private static void RemoveRootPlaceholderGeometry(GameObject root)
        {
            if (root.TryGetComponent<MeshRenderer>(out var meshRenderer))
            {
                Undo.DestroyObjectImmediate(meshRenderer);
            }

            if (root.TryGetComponent<MeshFilter>(out var meshFilter))
            {
                Undo.DestroyObjectImmediate(meshFilter);
            }
        }

        private static GameObject ResolveScenePlayer()
        {
            var playerMovement = UnityEngine.Object.FindFirstObjectByType<PlayerMovementController>(FindObjectsInactive.Include);
            if (playerMovement != null)
            {
                return playerMovement.gameObject;
            }

            var allTransforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < allTransforms.Length; i++)
            {
                if (allTransforms[i].CompareTag("Player"))
                {
                    return allTransforms[i].gameObject;
                }
            }

            return null;
        }

        private static GameObject LoadEnemyPrefab(CharacterEnemyRoleId roleId)
        {
            var spec = GetEnemySpec(roleId);
            return string.IsNullOrWhiteSpace(spec.GeneratedPrefabPath)
                ? null
                : AssetDatabase.LoadAssetAtPath<GameObject>(spec.GeneratedPrefabPath);
        }

        private static CharacterEnemyAssetSpec GetEnemySpec(CharacterEnemyRoleId roleId)
        {
            var specs = CharacterEnemyAssetSelection.Enemies;
            for (var i = 0; i < specs.Length; i++)
            {
                if (specs[i].RoleId == roleId)
                {
                    return specs[i];
                }
            }

            return default;
        }

        private static CharacterEnemyAssetSpec ResolveEnemySpecForObject(GameObject enemy)
        {
            var specs = CharacterEnemyAssetSelection.Enemies;
            if (specs.Length <= 0)
            {
                return default;
            }

            var prefabSource = enemy != null ? PrefabUtility.GetCorrespondingObjectFromSource(enemy) : null;
            var sourcePath = prefabSource != null ? AssetDatabase.GetAssetPath(prefabSource) : string.Empty;
            for (var i = 0; i < specs.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(sourcePath) &&
                    sourcePath.Equals(specs[i].GeneratedPrefabPath, StringComparison.OrdinalIgnoreCase))
                {
                    return specs[i];
                }
            }

            var enemyName = enemy != null ? enemy.name : string.Empty;
            for (var i = 0; i < specs.Length; i++)
            {
                if (NameMatchesSpec(enemyName, specs[i]))
                {
                    return specs[i];
                }
            }

            return specs[0];
        }

        private static bool NameMatchesSpec(string objectName, CharacterEnemyAssetSpec spec)
        {
            if (string.IsNullOrWhiteSpace(objectName))
            {
                return false;
            }

            var normalizedName = NormalizeSearchName(objectName);
            return normalizedName.Contains(NormalizeSearchName(spec.RoleId.ToString())) ||
                normalizedName.Contains(NormalizeSearchName(spec.DisplayName));
        }

        private static string NormalizeSearchName(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Replace(" ", string.Empty)
                    .Replace("_", string.Empty)
                    .Replace("-", string.Empty)
                    .ToLowerInvariant();
        }

        private static Transform FindChild(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            var children = root.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < children.Length; i++)
            {
                if (children[i].name == childName)
                {
                    return children[i];
                }
            }

            return null;
        }

        private static EnemyConfigSpec[] EnemyConfigSpecs()
        {
            return new[]
            {
                new EnemyConfigSpec(CharacterEnemyRoleId.BasicMelee, "enemy_basic_melee_green_demon", $"{GeneratedEnemyConfigRoot}/EnemyConfig_BasicMelee_GreenDemon.asset", 45f, 0.35f, 2.25f, 18f, 720f, 1.05f, 8f, 1.2f, 1f, 0.2f, true, true, 1, 1)
            };
        }

        private static WaveRewriteSpec[] WaveRewriteSpecs()
        {
            return new[]
            {
                new WaveRewriteSpec("Assets/_Project/ScriptableObjects/Waves/Wave_VS_01_SmallMelee.asset",
                    Entry(CharacterEnemyRoleId.BasicMelee, 3, 0.35f)),
                new WaveRewriteSpec("Assets/_Project/ScriptableObjects/Waves/Wave_VS_02_MeleeGroup.asset",
                    Entry(CharacterEnemyRoleId.BasicMelee, 5, 0.32f)),
                new WaveRewriteSpec("Assets/_Project/ScriptableObjects/Waves/Wave_VS_03_MixedPressure.asset",
                    Entry(CharacterEnemyRoleId.BasicMelee, 5, 0.28f)),
                new WaveRewriteSpec("Assets/_Project/ScriptableObjects/Waves/Wave_VS_04_ElitePlaceholder.asset",
                    Entry(CharacterEnemyRoleId.BasicMelee, 6, 0.42f)),
                new WaveRewriteSpec("Assets/_Project/ScriptableObjects/Waves/Wave_VS_05_LightRecoveryCombat.asset",
                    Entry(CharacterEnemyRoleId.BasicMelee, 3, 0.4f)),
                new WaveRewriteSpec("Assets/_Project/ScriptableObjects/Waves/Wave_VS_06_CombatPressure.asset",
                    Entry(CharacterEnemyRoleId.BasicMelee, 7, 0.24f)),
                new WaveRewriteSpec("Assets/_Project/ScriptableObjects/Waves/Wave_VS_07_RangedPressure.asset",
                    Entry(CharacterEnemyRoleId.BasicMelee, 7, 0.32f)),
                new WaveRewriteSpec("Assets/_Project/ScriptableObjects/Waves/Wave_VS_08_EliteAbility.asset",
                    Entry(CharacterEnemyRoleId.BasicMelee, 8, 0.3f)),
                new WaveRewriteSpec("Assets/_Project/ScriptableObjects/Waves/Wave_VS_09_PreBossPressure.asset",
                    Entry(CharacterEnemyRoleId.BasicMelee, 8, 0.22f)),
                new WaveRewriteSpec("Assets/_Project/ScriptableObjects/Waves/Wave_VS_10_BossPlaceholder.asset",
                    Entry(CharacterEnemyRoleId.BasicMelee, 10, 0.18f, 0))
            };
        }

        private static WaveEntryRewriteSpec Entry(CharacterEnemyRoleId roleId, int count, float spawnDelay, int spawnPointIndex = -1)
        {
            return new WaveEntryRewriteSpec(roleId, count, spawnDelay, spawnPointIndex);
        }

        private static string ExtractSummary(string report)
        {
            if (string.IsNullOrWhiteSpace(report))
            {
                return "- No animation report generated.";
            }

            var lines = report.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var summary = new StringBuilder();
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("- Candidate animation clips:", StringComparison.Ordinal)
                    || lines[i].StartsWith("## ", StringComparison.Ordinal)
                    || lines[i].StartsWith("- Animator Controller:", StringComparison.Ordinal)
                    || lines[i].StartsWith("- Prefab wiring:", StringComparison.Ordinal))
                {
                    summary.AppendLine(lines[i]);
                }
            }

            return summary.ToString();
        }

        private static void AppendIndented(StringBuilder report, string value)
        {
            var lines = value.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < lines.Length; i++)
            {
                report.AppendLine($"> {lines[i]}");
            }

            report.AppendLine();
        }

        private static void EnsureFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        private static void WriteTextAsset(string path, string contents)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, contents);
        }

        private readonly struct EnemyConfigSpec
        {
            public EnemyConfigSpec(
                CharacterEnemyRoleId roleId,
                string enemyId,
                string path,
                float maxHealth,
                float deathDelay,
                float moveSpeed,
                float acceleration,
                float rotationSpeed,
                float stoppingDistance,
                float contactDamage,
                float attackRange,
                float attackCooldown,
                float knockbackResistance,
                bool canBeKnockedBack,
                bool canBeInterrupted,
                int coinReward,
                int xpReward)
            {
                RoleId = roleId;
                EnemyId = enemyId;
                Path = path;
                MaxHealth = maxHealth;
                DeathDelay = deathDelay;
                MoveSpeed = moveSpeed;
                Acceleration = acceleration;
                RotationSpeed = rotationSpeed;
                StoppingDistance = stoppingDistance;
                ContactDamage = contactDamage;
                AttackRange = attackRange;
                AttackCooldown = attackCooldown;
                KnockbackResistance = knockbackResistance;
                CanBeKnockedBack = canBeKnockedBack;
                CanBeInterrupted = canBeInterrupted;
                CoinReward = coinReward;
                XpReward = xpReward;
            }

            public CharacterEnemyRoleId RoleId { get; }
            public string EnemyId { get; }
            public string Path { get; }
            public float MaxHealth { get; }
            public float DeathDelay { get; }
            public float MoveSpeed { get; }
            public float Acceleration { get; }
            public float RotationSpeed { get; }
            public float StoppingDistance { get; }
            public float ContactDamage { get; }
            public float AttackRange { get; }
            public float AttackCooldown { get; }
            public float KnockbackResistance { get; }
            public bool CanBeKnockedBack { get; }
            public bool CanBeInterrupted { get; }
            public int CoinReward { get; }
            public int XpReward { get; }
        }

        private readonly struct WaveRewriteSpec
        {
            public WaveRewriteSpec(string path, params WaveEntryRewriteSpec[] entries)
            {
                Path = path;
                Entries = entries;
            }

            public string Path { get; }
            public WaveEntryRewriteSpec[] Entries { get; }
        }

        private readonly struct WaveEntryRewriteSpec
        {
            public WaveEntryRewriteSpec(CharacterEnemyRoleId roleId, int count, float spawnDelay, int spawnPointIndex)
            {
                RoleId = roleId;
                Count = count;
                SpawnDelay = spawnDelay;
                SpawnPointIndex = spawnPointIndex;
            }

            public CharacterEnemyRoleId RoleId { get; }
            public int Count { get; }
            public float SpawnDelay { get; }
            public int SpawnPointIndex { get; }
        }
    }
}
