using System.Collections.Generic;
using System.IO;
using TapKnockout.Boss;
using TapKnockout.Enemy;
using TapKnockout.Feedback;
using TapKnockout.Projectile;
using TapKnockout.Survivor;
using TapKnockout.Wave;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Editor.Tools
{
    public static class EnemyBossPrefabReferenceRepairTool
    {
        private const string MenuPath = "Tools/Tap Knockout/Enemies/Repair Enemy/Boss Prefab References";
        private const string EnemyConfigFolder = "Assets/_Project/ScriptableObjects/Enemies";
        private const string BossConfigFolder = "Assets/_Project/ScriptableObjects/Bosses";
        private const string WaveFolder = "Assets/_Project/ScriptableObjects/Waves";
        private const string EnemyPrefabFolder = "Assets/_Project/Prefabs/Enemies";
        private const string BossPrefabFolder = "Assets/_Project/Prefabs/Bosses";
        private const string GeneratedMeleePrefabPath = "Assets/_Project/Prefabs/Enemies/Generated/PF_Enemy_BasicMelee_GreenDemon_Generated.prefab";
        private const string ProjectilePrefabPath = "Assets/_Project/Prefabs/Projectiles/PF_EnemyProjectile_Playtest.prefab";
        private const string BossConfigPath = "Assets/_Project/ScriptableObjects/Bosses/BossConfig_DashCounterBrute.asset";
        private const string BossEnemyConfigPath = "Assets/_Project/ScriptableObjects/Bosses/EnemyConfig_Boss1_DashCounterBrute.asset";
        private const string BossPrefabPath = "Assets/_Project/Prefabs/Bosses/PF_Boss_DashCounterBrute.prefab";

        private static readonly EnemyPrefabSpec[] EnemySpecs =
        {
            new EnemyPrefabSpec("EnemyConfig_MeleeChaser", "Wave_Test_MeleeChaser", "PF_Enemy_MeleeChaser", EnemyArchetype.MeleeChaser, PrimitiveType.Capsule, new Color(0.25f, 0.78f, 0.34f, 1f)),
            new EnemyPrefabSpec("EnemyConfig_FastCharger", "Wave_Test_FastCharger", "PF_Enemy_FastCharger", EnemyArchetype.FastCharger, PrimitiveType.Capsule, new Color(0.9f, 0.32f, 0.18f, 1f)),
            new EnemyPrefabSpec("EnemyConfig_RangedShooter", "Wave_Test_RangedShooter", "PF_Enemy_RangedShooter", EnemyArchetype.RangedShooter, PrimitiveType.Capsule, new Color(0.22f, 0.5f, 0.9f, 1f)),
            new EnemyPrefabSpec("EnemyConfig_AreaBomber", "Wave_Test_AreaBomber", "PF_Enemy_AreaBomber", EnemyArchetype.AreaBomber, PrimitiveType.Sphere, new Color(0.9f, 0.65f, 0.18f, 1f)),
            new EnemyPrefabSpec("EnemyConfig_ShieldEnemy", "Wave_Test_ShieldEnemy", "PF_Enemy_ShieldEnemy", EnemyArchetype.ShieldEnemy, PrimitiveType.Cube, new Color(0.55f, 0.55f, 0.68f, 1f)),
            new EnemyPrefabSpec("EnemyConfig_SplitterEnemy", "Wave_Test_SplitterEnemy", "PF_Enemy_SplitterEnemy", EnemyArchetype.SplitterEnemy, PrimitiveType.Sphere, new Color(0.72f, 0.42f, 0.95f, 1f)),
            new EnemyPrefabSpec("EnemyConfig_EliteChaser", "Wave_Test_EliteChaser", "PF_Enemy_EliteChaser", EnemyArchetype.EliteChaser, PrimitiveType.Capsule, new Color(0.2f, 0.95f, 0.62f, 1f), 1.25f),
            new EnemyPrefabSpec("EnemyConfig_EliteRanged", "Wave_Test_EliteRanged", "PF_Enemy_EliteRanged", EnemyArchetype.EliteRanged, PrimitiveType.Capsule, new Color(0.38f, 0.8f, 1f, 1f), 1.2f)
        };

        [MenuItem(MenuPath)]
        public static void RepairEnemyBossPrefabReferences()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Enemy/Boss Prefab Reference Repair", "Exit Play Mode before repairing prefab references.", "OK");
                return;
            }

            var summary = RepairAllReferences();
            EditorUtility.DisplayDialog(
                "Enemy/Boss Prefab Reference Repair",
                $"Enemy prefabs ready: {summary.EnemyPrefabCount}\nBoss prefabs ready: {summary.BossPrefabCount}\nWave entries repaired: {summary.WaveEntryRepairCount}\nConfig references repaired: {summary.ConfigRepairCount}\nWarnings: {summary.WarningCount}\n\nCheck Window > General > Console for details.",
                "OK");
        }

        public static RepairSummary RepairAllReferences()
        {
            EnsureFolder(EnemyPrefabFolder);
            EnsureFolder(BossPrefabFolder);
            EnsureFolder("Assets/_Project/Prefabs/Projectiles");

            var summary = new RepairSummary();
            var projectilePrefab = CreateOrRepairProjectilePrefab(ProjectilePrefabPath);
            GameObject meleePrefab = null;

            for (var i = 0; i < EnemySpecs.Length; i++)
            {
                var spec = EnemySpecs[i];
                var config = AssetDatabase.LoadAssetAtPath<EnemyConfig>($"{EnemyConfigFolder}/{spec.ConfigAssetName}.asset");
                if (config == null)
                {
                    summary.WarningCount++;
                    Debug.LogWarning($"{nameof(EnemyBossPrefabReferenceRepairTool)} Warning: missing config {spec.ConfigAssetName}. Run Tools > Tap Knockout > Enemies > Create Enemy Archetype Configs first.");
                    continue;
                }

                var prefab = CreateOrRepairEnemyPrefab(spec, config);
                var runtimePrefab = spec.Archetype == EnemyArchetype.MeleeChaser
                    ? ResolveGeneratedMeleePrefab() ?? prefab
                    : prefab;
                if (prefab != null)
                {
                    summary.EnemyPrefabCount++;
                }

                if (spec.Archetype == EnemyArchetype.MeleeChaser)
                {
                    meleePrefab = runtimePrefab;
                }

                summary.ConfigRepairCount += RepairEnemyConfigReferences(config, spec, projectilePrefab, meleePrefab);
                summary.WaveEntryRepairCount += RepairWaveEntry($"{WaveFolder}/{spec.WaveAssetName}.asset", config, runtimePrefab);
            }

            var bossPrefab = CreateOrRepairBossPrefab();
            if (bossPrefab != null)
            {
                summary.BossPrefabCount++;
            }

            var bossEnemyConfig = AssetDatabase.LoadAssetAtPath<EnemyConfig>(BossEnemyConfigPath);
            summary.WaveEntryRepairCount += RepairWaveEntry($"{WaveFolder}/Wave_Test_Boss1_DashCounterBrute.asset", bossEnemyConfig, bossPrefab);
            summary.ConfigRepairCount += RepairBossConfigReferences(bossPrefab, meleePrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{nameof(EnemyBossPrefabReferenceRepairTool)} Done: enemy prefabs {summary.EnemyPrefabCount}, boss prefabs {summary.BossPrefabCount}, wave repairs {summary.WaveEntryRepairCount}, config repairs {summary.ConfigRepairCount}, warnings {summary.WarningCount}.");
            return summary;
        }

        private static GameObject ResolveGeneratedMeleePrefab()
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(GeneratedMeleePrefabPath);
        }

        private static GameObject CreateOrRepairEnemyPrefab(EnemyPrefabSpec spec, EnemyConfig config)
        {
            var path = $"{EnemyPrefabFolder}/{spec.PrefabName}.prefab";
            var loadedExistingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
            var root = loadedExistingPrefab
                ? PrefabUtility.LoadPrefabContents(path)
                : CreateBaseEnemyPrefab(spec.PrefabName, spec.VisualPrimitive, spec.VisualColor, spec.VisualScale);
            if (root == null)
            {
                return null;
            }

            try
            {
                var changed = RepairBaseEnemyComponents(root, config);
                changed |= RepairArchetypeComponents(root, spec.Archetype);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                Debug.Log($"{nameof(EnemyBossPrefabReferenceRepairTool)} Done: repaired enemy prefab {path}.", root);
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
            finally
            {
                if (loadedExistingPrefab)
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
                else
                {
                    Object.DestroyImmediate(root);
                }
            }
        }

        private static GameObject CreateBaseEnemyPrefab(string name, PrimitiveType primitiveType, Color color, float visualScale)
        {
            var root = new GameObject(name);
            root.transform.localScale = Vector3.one;

            var bodyCollider = root.AddComponent<CapsuleCollider>();
            bodyCollider.center = new Vector3(0f, 1f, 0f);
            bodyCollider.height = 2f;
            bodyCollider.radius = 0.45f;

            var rigidbody = root.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            root.AddComponent<EnemyController>();
            root.AddComponent<EnemyHealth>();
            root.AddComponent<EnemyMovement>();
            root.AddComponent<KnockbackReceiver>();
            root.AddComponent<EnemyAttackController>();
            root.AddComponent<PooledEnemy>();
            root.AddComponent<HitFlashController>();

            var visualRoot = CreateChild(root.transform, "VisualRoot");
            visualRoot.localScale = Vector3.one * Mathf.Max(0.1f, visualScale);
            var visual = GameObject.CreatePrimitive(primitiveType);
            visual.name = "PlaceholderVisual";
            visual.transform.SetParent(visualRoot, false);
            visual.transform.localPosition = new Vector3(0f, 1f, 0f);
            var visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null)
            {
                Object.DestroyImmediate(visualCollider);
            }

            var renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateMaterial($"{name}_MAT", color);
            }

            CreateChild(root.transform, "AttackOrigin").localPosition = new Vector3(0f, 1f, 0.55f);
            CreateChild(root.transform, "HitReactionRoot").localPosition = new Vector3(0f, 1f, 0f);
            CreateChild(root.transform, "ProjectileSpawnPoint").localPosition = new Vector3(0f, 1.2f, 0.6f);
            var telegraphRoot = CreateChild(root.transform, "TelegraphRoot");
            telegraphRoot.localPosition = new Vector3(0f, 0.02f, 0f);
            var telegraphVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            telegraphVisual.name = "TelegraphVisual";
            telegraphVisual.transform.SetParent(telegraphRoot, false);
            telegraphVisual.transform.localScale = new Vector3(1f, 0.01f, 1f);
            var telegraphCollider = telegraphVisual.GetComponent<Collider>();
            if (telegraphCollider != null)
            {
                Object.DestroyImmediate(telegraphCollider);
            }

            var telegraphRenderer = telegraphVisual.GetComponent<Renderer>();
            if (telegraphRenderer != null)
            {
                telegraphRenderer.sharedMaterial = CreateMaterial("MAT_Playtest_Telegraph_Enemy", new Color(1f, 0.25f, 0.1f, 0.55f));
            }

            telegraphRoot.gameObject.SetActive(false);
            var telegraphController = root.AddComponent<EnemyTelegraphController>();
            SetObject(new SerializedObject(telegraphController), "telegraphRoot", telegraphRoot);
            SetObject(new SerializedObject(telegraphController), "telegraphRenderer", telegraphRenderer);
            ApplyEnemyLayer(root);
            return root;
        }

        private static bool RepairBaseEnemyComponents(GameObject root, EnemyConfig config)
        {
            var changed = false;
            var rigidbody = EnsureComponent<Rigidbody>(root, ref changed);
            rigidbody.useGravity = false;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rigidbody.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            if (root.GetComponent<Collider>() == null)
            {
                var collider = root.AddComponent<CapsuleCollider>();
                collider.center = new Vector3(0f, 1f, 0f);
                collider.height = 2f;
                collider.radius = 0.45f;
                changed = true;
            }

            changed |= EnsureChild(root.transform, "VisualRoot") != null;
            changed |= EnsureChild(root.transform, "AttackOrigin") != null;
            changed |= EnsureChild(root.transform, "HitReactionRoot") != null;
            changed |= EnsureChild(root.transform, "ProjectileSpawnPoint") != null;
            var telegraphRoot = EnsureChild(root.transform, "TelegraphRoot");

            var controller = EnsureComponent<EnemyController>(root, ref changed);
            var health = EnsureComponent<EnemyHealth>(root, ref changed);
            var movement = EnsureComponent<EnemyMovement>(root, ref changed);
            var knockback = EnsureComponent<KnockbackReceiver>(root, ref changed);
            var attack = EnsureComponent<EnemyAttackController>(root, ref changed);
            var telegraph = EnsureComponent<EnemyTelegraphController>(root, ref changed);
            EnsureComponent<PooledEnemy>(root, ref changed);
            EnsureComponent<HitFlashController>(root, ref changed);

            var serializedController = new SerializedObject(controller);
            SetObject(serializedController, "config", config);
            SetObject(serializedController, "health", health);
            SetObject(serializedController, "movement", movement);
            SetObject(serializedController, "knockbackReceiver", knockback);
            SetObject(serializedController, "attackController", attack);

            var serializedHealth = new SerializedObject(health);
            SetObject(serializedHealth, "config", config);
            SetObject(serializedHealth, "targetTransform", root.transform);
            SetBool(serializedHealth, "targetableWhenAlive", true);
            SetBool(serializedHealth, "disableCollidersOnDeath", true);

            SetObject(new SerializedObject(movement), "config", config);
            SetObject(new SerializedObject(knockback), "config", config);
            SetObject(new SerializedObject(attack), "config", config);
            SetObject(new SerializedObject(telegraph), "telegraphRoot", telegraphRoot);
            ApplyEnemyLayer(root);
            EditorUtility.SetDirty(root);
            return changed;
        }

        private static bool RepairArchetypeComponents(GameObject root, EnemyArchetype archetype)
        {
            var changed = false;
            switch (archetype)
            {
                case EnemyArchetype.FastCharger:
                    EnsureComponent<FastChargerController>(root, ref changed);
                    break;
                case EnemyArchetype.RangedShooter:
                case EnemyArchetype.EliteRanged:
                    var shooter = EnsureComponent<RangedShooterController>(root, ref changed);
                    SetObject(new SerializedObject(shooter), "projectileSpawnPoint", FindDeepChild(root.transform, "ProjectileSpawnPoint"));
                    break;
                case EnemyArchetype.AreaBomber:
                    EnsureComponent<AreaBomberController>(root, ref changed);
                    break;
                case EnemyArchetype.ShieldEnemy:
                    var shieldFilter = EnsureComponent<ShieldDamageFilter>(root, ref changed);
                    var shieldController = EnsureComponent<ShieldEnemyController>(root, ref changed);
                    SetObject(new SerializedObject(shieldController), "shieldDamageFilter", shieldFilter);
                    break;
                case EnemyArchetype.SplitterEnemy:
                    EnsureComponent<SplitterEnemyController>(root, ref changed);
                    break;
            }

            return changed;
        }

        private static GameObject CreateOrRepairBossPrefab()
        {
            var bossConfig = AssetDatabase.LoadAssetAtPath<BossConfig>(BossConfigPath);
            var bossEnemyConfig = AssetDatabase.LoadAssetAtPath<EnemyConfig>(BossEnemyConfigPath);
            if (bossConfig == null || bossEnemyConfig == null)
            {
                Debug.LogWarning($"{nameof(EnemyBossPrefabReferenceRepairTool)} Warning: missing Boss 1 config assets. Run Tools > Tap Knockout > Bosses > Create Boss 1 Dash-Counter Brute first.");
                return null;
            }

            var loadedExistingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BossPrefabPath) != null;
            var root = loadedExistingPrefab
                ? PrefabUtility.LoadPrefabContents(BossPrefabPath)
                : CreateBaseEnemyPrefab("PF_Boss_DashCounterBrute", PrimitiveType.Cube, new Color(0.62f, 0.16f, 0.12f, 1f), 1.75f);
            if (root == null)
            {
                return null;
            }

            try
            {
                var changed = false;
                RepairBaseEnemyComponents(root, bossEnemyConfig);
                var phase = EnsureComponent<BossPhaseController>(root, ref changed);
                var pattern = EnsureComponent<BossPatternController>(root, ref changed);
                var slam = EnsureComponent<BossSlamAttack>(root, ref changed);
                var charge = EnsureComponent<BossChargeAttack>(root, ref changed);
                var adds = EnsureComponent<BossAddSpawnAction>(root, ref changed);
                var intro = EnsureComponent<BossIntroController>(root, ref changed);
                var outro = EnsureComponent<BossOutroController>(root, ref changed);
                var bridge = EnsureComponent<BossRuntimeBindingBridge>(root, ref changed);
                var addSpawnPoints = EnsureAddSpawnPoints(root.transform);
                var health = root.GetComponent<EnemyHealth>();
                var telegraph = root.GetComponent<EnemyTelegraphController>();

                SetObject(new SerializedObject(phase), "config", bossConfig);
                SetObject(new SerializedObject(phase), "health", health);
                SetObject(new SerializedObject(phase), "patternController", pattern);
                SetObject(new SerializedObject(phase), "addSpawnAction", adds);
                SetObject(new SerializedObject(phase), "chargeAttack", charge);
                SetObject(new SerializedObject(pattern), "slamAttack", slam);
                SetObject(new SerializedObject(pattern), "chargeAttack", charge);
                SetObject(new SerializedObject(pattern), "addSpawnAction", adds);
                SetBool(new SerializedObject(pattern), "playOnEnable", false);
                SetObject(new SerializedObject(slam), "telegraphController", telegraph);
                SetObject(new SerializedObject(charge), "telegraphController", telegraph);
                SetObject(new SerializedObject(adds), "config", bossConfig);
                SetObjectArray(new SerializedObject(adds), "spawnPoints", addSpawnPoints);
                SetObject(new SerializedObject(intro), "config", bossConfig);
                SetObject(new SerializedObject(intro), "patternController", pattern);
                SetObject(new SerializedObject(outro), "config", bossConfig);
                SetObject(new SerializedObject(outro), "health", health);
                SetObject(new SerializedObject(bridge), "bossConfig", bossConfig);
                SetObject(new SerializedObject(bridge), "phaseController", phase);
                SetObject(new SerializedObject(bridge), "patternController", pattern);
                SetObject(new SerializedObject(bridge), "addSpawnAction", adds);
                SetObject(new SerializedObject(bridge), "introController", intro);

                PrefabUtility.SaveAsPrefabAsset(root, BossPrefabPath);
                Debug.Log($"{nameof(EnemyBossPrefabReferenceRepairTool)} Done: repaired boss prefab {BossPrefabPath}.", root);
                return AssetDatabase.LoadAssetAtPath<GameObject>(BossPrefabPath);
            }
            finally
            {
                if (loadedExistingPrefab)
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
                else
                {
                    Object.DestroyImmediate(root);
                }
            }
        }

        private static int RepairEnemyConfigReferences(EnemyConfig config, EnemyPrefabSpec spec, GameObject projectilePrefab, GameObject splitPrefab)
        {
            var serializedObject = new SerializedObject(config);
            var changed = false;
            if ((spec.Archetype == EnemyArchetype.RangedShooter || spec.Archetype == EnemyArchetype.EliteRanged) && projectilePrefab != null)
            {
                changed |= SetObject(serializedObject, "projectilePrefab", projectilePrefab);
            }

            if (spec.Archetype == EnemyArchetype.SplitterEnemy && splitPrefab != null)
            {
                changed |= SetObject(serializedObject, "splitSpawnPrefab", splitPrefab);
                changed |= SetIntIfLessThan(serializedObject, "splitSpawnCount", 2);
            }

            if (!changed)
            {
                return 0;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            Debug.Log($"{nameof(EnemyBossPrefabReferenceRepairTool)} Done: repaired config references for {AssetDatabase.GetAssetPath(config)}.", config);
            return 1;
        }

        private static int RepairBossConfigReferences(GameObject bossPrefab, GameObject addPrefab)
        {
            var bossConfig = AssetDatabase.LoadAssetAtPath<BossConfig>(BossConfigPath);
            var addConfig = AssetDatabase.LoadAssetAtPath<EnemyConfig>($"{EnemyConfigFolder}/EnemyConfig_MeleeChaser.asset");
            if (bossConfig == null)
            {
                return 0;
            }

            var serializedObject = new SerializedObject(bossConfig);
            var changed = false;
            changed |= SetObject(serializedObject, "addEnemyConfig", addConfig);
            changed |= SetObject(serializedObject, "addEnemyPrefab", addPrefab);
            changed |= SetIntIfLessThan(serializedObject, "maxActiveAdds", 4);

            if (!changed)
            {
                return 0;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(bossConfig);
            Debug.Log($"{nameof(EnemyBossPrefabReferenceRepairTool)} Done: repaired boss add references for {BossConfigPath}.", bossConfig);
            return 1;
        }

        private static int RepairWaveEntry(string wavePath, EnemyConfig config, GameObject prefab)
        {
            var wave = AssetDatabase.LoadAssetAtPath<WaveConfig>(wavePath);
            if (wave == null || config == null || prefab == null)
            {
                return 0;
            }

            var serializedObject = new SerializedObject(wave);
            var enemies = serializedObject.FindProperty("enemies");
            if (enemies == null)
            {
                return 0;
            }

            if (enemies.arraySize == 0)
            {
                enemies.arraySize = 1;
            }

            var changed = false;
            var entry = enemies.GetArrayElementAtIndex(0);
            changed |= SetRelativeObject(entry, "enemyConfig", config);
            changed |= SetRelativeObject(entry, "enemyPrefab", prefab);
            changed |= SetRelativeIntIfLessThan(entry, "count", 1);
            changed |= SetRelativeFloatIfLessThan(entry, "spawnDelay", 0.1f);
            if (!changed)
            {
                return 0;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(wave);
            Debug.Log($"{nameof(EnemyBossPrefabReferenceRepairTool)} Done: repaired wave entry {wavePath}.", wave);
            return 1;
        }

        private static GameObject CreateOrRepairProjectilePrefab(string path)
        {
            var loadedExistingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
            var root = loadedExistingPrefab
                ? PrefabUtility.LoadPrefabContents(path)
                : GameObject.CreatePrimitive(PrimitiveType.Sphere);
            root.name = Path.GetFileNameWithoutExtension(path);
            root.transform.localScale = Vector3.one * 0.24f;

            try
            {
                var collider = root.GetComponent<Collider>();
                if (collider == null)
                {
                    collider = root.AddComponent<SphereCollider>();
                }

                collider.isTrigger = true;
                var rigidbody = root.GetComponent<Rigidbody>();
                if (rigidbody == null)
                {
                    rigidbody = root.AddComponent<Rigidbody>();
                }

                rigidbody.useGravity = false;
                rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                if (root.GetComponent<EnemyProjectileController>() == null)
                {
                    root.AddComponent<EnemyProjectileController>();
                }

                PrefabUtility.SaveAsPrefabAsset(root, path);
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
            finally
            {
                if (loadedExistingPrefab)
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
                else
                {
                    Object.DestroyImmediate(root);
                }
            }
        }

        private static Transform[] EnsureAddSpawnPoints(Transform root)
        {
            var addRoot = EnsureChild(root, "AddSpawnPoints");
            var positions = new[]
            {
                new Vector3(-2.5f, 0f, 1.75f),
                new Vector3(2.5f, 0f, 1.75f),
                new Vector3(-2.5f, 0f, -1.75f),
                new Vector3(2.5f, 0f, -1.75f)
            };
            var points = new Transform[positions.Length];
            for (var i = 0; i < positions.Length; i++)
            {
                var point = EnsureChild(addRoot, $"AddSpawn_{i + 1:00}");
                point.localPosition = positions[i];
                points[i] = point;
            }

            return points;
        }

        private static T EnsureComponent<T>(GameObject target, ref bool changed) where T : Component
        {
            var component = target.GetComponent<T>();
            if (component != null)
            {
                return component;
            }

            changed = true;
            return target.AddComponent<T>();
        }

        private static Transform EnsureChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null)
            {
                return child;
            }

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static Transform CreateChild(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static Transform FindDeepChild(Transform root, string name)
        {
            if (root == null)
            {
                return null;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child.name == name)
                {
                    return child;
                }

                var nested = FindDeepChild(child, name);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }

        private static Material CreateMaterial(string name, Color color)
        {
            EnsureFolder("Assets/_Project/Art/Materials");
            var path = $"Assets/_Project/Art/Materials/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"))
            {
                name = name,
                color = color
            };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void ApplyEnemyLayer(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            var enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer < 0)
            {
                return;
            }

            var transforms = root.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < transforms.Length; i++)
            {
                var candidate = transforms[i];
                if (candidate == null)
                {
                    continue;
                }

                if (candidate == root.transform || candidate.GetComponent<Collider>() != null)
                {
                    candidate.gameObject.layer = enemyLayer;
                }
            }
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            if (!string.IsNullOrWhiteSpace(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent ?? "Assets", Path.GetFileName(folderPath));
        }

        private static bool SetObject(SerializedObject serializedObject, string propertyName, Object value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private static bool SetBool(SerializedObject serializedObject, string propertyName, bool value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property == null || property.boolValue == value)
            {
                return false;
            }

            property.boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private static bool SetObjectArray(SerializedObject serializedObject, string propertyName, IReadOnlyList<Transform> values)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                return false;
            }

            property.arraySize = values.Count;
            for (var i = 0; i < values.Count; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private static bool SetIntIfLessThan(SerializedObject serializedObject, string propertyName, int minimum)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property == null || property.intValue >= minimum)
            {
                return false;
            }

            property.intValue = minimum;
            return true;
        }

        private static bool SetRelativeObject(SerializedProperty parent, string propertyName, Object value)
        {
            var property = parent.FindPropertyRelative(propertyName);
            if (property == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }

        private static bool SetRelativeIntIfLessThan(SerializedProperty parent, string propertyName, int minimum)
        {
            var property = parent.FindPropertyRelative(propertyName);
            if (property == null || property.intValue >= minimum)
            {
                return false;
            }

            property.intValue = minimum;
            return true;
        }

        private static bool SetRelativeFloatIfLessThan(SerializedProperty parent, string propertyName, float minimum)
        {
            var property = parent.FindPropertyRelative(propertyName);
            if (property == null || property.floatValue >= minimum)
            {
                return false;
            }

            property.floatValue = minimum;
            return true;
        }

        public sealed class RepairSummary
        {
            public int EnemyPrefabCount;
            public int BossPrefabCount;
            public int WaveEntryRepairCount;
            public int ConfigRepairCount;
            public int WarningCount;
        }

        private readonly struct EnemyPrefabSpec
        {
            public EnemyPrefabSpec(
                string configAssetName,
                string waveAssetName,
                string prefabName,
                EnemyArchetype archetype,
                PrimitiveType visualPrimitive,
                Color visualColor,
                float visualScale = 1f)
            {
                ConfigAssetName = configAssetName;
                WaveAssetName = waveAssetName;
                PrefabName = prefabName;
                Archetype = archetype;
                VisualPrimitive = visualPrimitive;
                VisualColor = visualColor;
                VisualScale = visualScale;
            }

            public string ConfigAssetName { get; }
            public string WaveAssetName { get; }
            public string PrefabName { get; }
            public EnemyArchetype Archetype { get; }
            public PrimitiveType VisualPrimitive { get; }
            public Color VisualColor { get; }
            public float VisualScale { get; }
        }
    }
}
