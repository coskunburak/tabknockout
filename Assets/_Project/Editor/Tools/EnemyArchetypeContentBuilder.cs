using System.Collections.Generic;
using System.IO;
using TapKnockout.Enemy;
using TapKnockout.VFX;
using TapKnockout.Wave;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Editor.Tools
{
    public static class EnemyArchetypeContentBuilder
    {
        private const string MenuPath = "Tools/Tap Knockout/Enemies/Create Enemy Archetype Configs";
        private const string EnemyFolder = "Assets/_Project/ScriptableObjects/Enemies";
        private const string WaveFolder = "Assets/_Project/ScriptableObjects/Waves";

        private static readonly EnemySpec[] EnemySpecs =
        {
            new EnemySpec("EnemyConfig_MeleeChaser", "enemy_melee_chaser", "Melee Chaser", EnemyArchetype.MeleeChaser, EnemyRank.Normal, 42f, 2.25f, 4f, 1.25f, 0.95f, 0.25f, 0f, 1, 1.2f, 0.2f, true, true),
            new EnemySpec("EnemyConfig_FastCharger", "enemy_fast_charger", "Fast Charger", EnemyArchetype.FastCharger, EnemyRank.Normal, 34f, 2.8f, 7f, 6f, 1.35f, 0.5f, 0f, 1, 1.1f, 0.1f, true, true),
            new EnemySpec("EnemyConfig_RangedShooter", "enemy_ranged_shooter", "Ranged Shooter", EnemyArchetype.RangedShooter, EnemyRank.Normal, 30f, 1.75f, 6f, 7f, 1.4f, 0.35f, 8.5f, 1, 1.1f, 0.05f, true, true),
            new EnemySpec("EnemyConfig_AreaBomber", "enemy_area_bomber", "Area Bomber", EnemyArchetype.AreaBomber, EnemyRank.Normal, 38f, 1.6f, 12f, 6f, 1.9f, 0.8f, 0f, 1, 1.8f, 0.15f, true, true),
            new EnemySpec("EnemyConfig_ShieldEnemy", "enemy_shield_guard", "Shield Guard", EnemyArchetype.ShieldEnemy, EnemyRank.Normal, 58f, 1.8f, 6f, 1.35f, 1.2f, 0.35f, 0f, 1, 1.1f, 0.35f, true, true),
            new EnemySpec("EnemyConfig_SplitterEnemy", "enemy_splitter", "Splitter", EnemyArchetype.SplitterEnemy, EnemyRank.Normal, 36f, 2.1f, 5f, 1.2f, 1.05f, 0.25f, 0f, 1, 1.1f, 0.1f, true, true),
            new EnemySpec("EnemyConfig_EliteChaser", "enemy_elite_chaser", "Elite Chaser", EnemyArchetype.EliteChaser, EnemyRank.Elite, 95f, 2.55f, 8f, 1.45f, 1.25f, 0.35f, 0f, 1, 1.2f, 0.45f, true, true),
            new EnemySpec("EnemyConfig_EliteRanged", "enemy_elite_ranged", "Elite Ranged", EnemyArchetype.EliteRanged, EnemyRank.Elite, 78f, 1.95f, 9f, 8f, 1.65f, 0.45f, 9.5f, 3, 1.25f, 0.25f, true, true)
        };

        [MenuItem(MenuPath)]
        public static void CreateEnemyArchetypeConfigs()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Enemy Archetype Configs", "Exit Play Mode before creating enemy configs.", "OK");
                return;
            }

            EnsureFolder(EnemyFolder);
            EnsureFolder(WaveFolder);
            var createdCount = 0;
            var waveCreatedCount = 0;
            var skipped = new List<string>();

            for (var i = 0; i < EnemySpecs.Length; i++)
            {
                if (CreateEnemyConfigIfMissing(EnemySpecs[i], skipped))
                {
                    createdCount++;
                }

                if (CreateTestWaveIfMissing(EnemySpecs[i], skipped))
                {
                    waveCreatedCount++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            for (var i = 0; i < skipped.Count; i++)
            {
                Debug.Log(skipped[i]);
            }

            Debug.Log($"{nameof(EnemyArchetypeContentBuilder)} created {createdCount} enemy config asset(s) and {waveCreatedCount} test wave asset(s). Existing assets were left unchanged.");
            EditorUtility.DisplayDialog(
                "Enemy Archetype Configs",
                $"Created {createdCount} missing enemy config asset(s) and {waveCreatedCount} missing test wave asset(s). Existing assets were not overwritten. Check Window > General > Console for details.",
                "OK");
        }

        private static bool CreateEnemyConfigIfMissing(EnemySpec spec, List<string> skipped)
        {
            var path = $"{EnemyFolder}/{spec.AssetName}.asset";
            if (AssetDatabase.LoadAssetAtPath<EnemyConfig>(path) != null)
            {
                skipped.Add($"Skipped existing enemy config without overwriting: {path}");
                return false;
            }

            var config = ScriptableObject.CreateInstance<EnemyConfig>();
            Undo.RegisterCreatedObjectUndo(config, "Create Enemy Archetype Config");
            AssetDatabase.CreateAsset(config, path);

            var serializedObject = new SerializedObject(config);
            SetString(serializedObject, "enemyId", spec.Id);
            SetString(serializedObject, "displayName", spec.DisplayName);
            SetEnum(serializedObject, "archetype", (int)spec.Archetype);
            SetEnum(serializedObject, "rank", (int)spec.Rank);
            SetFloat(serializedObject, "maxHealth", spec.MaxHealth);
            SetFloat(serializedObject, "moveSpeed", spec.MoveSpeed);
            SetFloat(serializedObject, "acceleration", 18f);
            SetFloat(serializedObject, "rotationSpeed", 720f);
            SetFloat(serializedObject, "stoppingDistance", spec.StoppingDistance);
            SetFloat(serializedObject, "contactDamage", spec.ContactDamage);
            SetFloat(serializedObject, "attackRange", spec.AttackRange);
            SetFloat(serializedObject, "attackCooldown", spec.AttackCooldown);
            SetFloat(serializedObject, "attackWindup", spec.AttackWindup);
            SetFloat(serializedObject, "projectileSpeed", spec.ProjectileSpeed);
            SetInt(serializedObject, "projectileCount", spec.ProjectileCount);
            SetFloat(serializedObject, "explosionRadius", spec.ExplosionRadius);
            SetFloat(serializedObject, "knockbackResistance", spec.KnockbackResistance);
            SetFloat(serializedObject, "stunResistance", spec.KnockbackResistance);
            SetBool(serializedObject, "canBeKnockedBack", spec.CanBeKnockedBack);
            SetBool(serializedObject, "canBeInterrupted", spec.CanBeInterrupted);
            SetFloat(serializedObject, "chargeSpeedMultiplier", spec.Archetype == EnemyArchetype.FastCharger ? 3.2f : 2.8f);
            SetFloat(serializedObject, "chargeDuration", spec.Archetype == EnemyArchetype.FastCharger ? 0.45f : 0.35f);
            SetFloat(serializedObject, "chargeRecoveryDuration", spec.Archetype == EnemyArchetype.FastCharger ? 0.55f : 0.4f);
            SetInt(serializedObject, "splitSpawnCount", spec.Archetype == EnemyArchetype.SplitterEnemy ? 2 : 0);
            SetFloat(serializedObject, "shieldBlockAngle", spec.Archetype == EnemyArchetype.ShieldEnemy ? 120f : 0f);
            SetFloat(serializedObject, "shieldDamageReduction", spec.Archetype == EnemyArchetype.ShieldEnemy ? 0.65f : 0f);
            SetEnum(serializedObject, "spawnVfx", (int)VFXEventType.EnemyTelegraph);
            SetEnum(serializedObject, "attackVfx", (int)VFXEventType.EnemyAttackRelease);
            SetEnum(serializedObject, "deathVfx", (int)VFXEventType.EnemyDeath);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            return true;
        }

        private static bool CreateTestWaveIfMissing(EnemySpec spec, List<string> skipped)
        {
            var path = $"{WaveFolder}/Wave_Test_{spec.AssetName.Replace("EnemyConfig_", string.Empty)}.asset";
            if (AssetDatabase.LoadAssetAtPath<WaveConfig>(path) != null)
            {
                skipped.Add($"Skipped existing test wave without overwriting: {path}");
                return false;
            }

            var enemyConfig = AssetDatabase.LoadAssetAtPath<EnemyConfig>($"{EnemyFolder}/{spec.AssetName}.asset");
            if (enemyConfig == null)
            {
                return false;
            }

            var wave = ScriptableObject.CreateInstance<WaveConfig>();
            Undo.RegisterCreatedObjectUndo(wave, "Create Enemy Test Wave");
            AssetDatabase.CreateAsset(wave, path);

            var serializedObject = new SerializedObject(wave);
            SetString(serializedObject, "waveId", $"wave_test_{spec.Id}");
            SetFloat(serializedObject, "startDelay", 0.35f);
            SetBool(serializedObject, "completeWhenAllSpawnedEnemiesDead", true);

            var enemies = serializedObject.FindProperty("enemies");
            if (enemies != null)
            {
                enemies.arraySize = 1;
                var entry = enemies.GetArrayElementAtIndex(0);
                SetRelativeObject(entry, "enemyConfig", enemyConfig);
                SetRelativeObject(entry, "enemyPrefab", null);
                SetRelativeInt(entry, "count", spec.Rank == EnemyRank.Elite ? 1 : 3);
                SetRelativeFloat(entry, "spawnDelay", 0.35f);
                SetRelativeInt(entry, "spawnPointIndex", -1);
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(wave);
            return true;
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

        private static void SetString(SerializedObject serializedObject, string propertyName, string value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.stringValue = value;
            }
        }

        private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.floatValue = value;
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

        private static void SetEnum(SerializedObject serializedObject, string propertyName, int enumValueIndex)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.enumValueIndex = enumValueIndex;
            }
        }

        private static void SetRelativeObject(SerializedProperty parent, string propertyName, Object value)
        {
            var property = parent.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        private static void SetRelativeInt(SerializedProperty parent, string propertyName, int value)
        {
            var property = parent.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.intValue = value;
            }
        }

        private static void SetRelativeFloat(SerializedProperty parent, string propertyName, float value)
        {
            var property = parent.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private readonly struct EnemySpec
        {
            public EnemySpec(
                string assetName,
                string id,
                string displayName,
                EnemyArchetype archetype,
                EnemyRank rank,
                float maxHealth,
                float moveSpeed,
                float contactDamage,
                float attackRange,
                float attackCooldown,
                float attackWindup,
                float projectileSpeed,
                int projectileCount,
                float explosionRadius,
                float knockbackResistance,
                bool canBeKnockedBack,
                bool canBeInterrupted)
            {
                AssetName = assetName;
                Id = id;
                DisplayName = displayName;
                Archetype = archetype;
                Rank = rank;
                MaxHealth = maxHealth;
                MoveSpeed = moveSpeed;
                ContactDamage = contactDamage;
                AttackRange = attackRange;
                AttackCooldown = attackCooldown;
                AttackWindup = attackWindup;
                ProjectileSpeed = projectileSpeed;
                ProjectileCount = projectileCount;
                ExplosionRadius = explosionRadius;
                KnockbackResistance = knockbackResistance;
                CanBeKnockedBack = canBeKnockedBack;
                CanBeInterrupted = canBeInterrupted;
                StoppingDistance = archetype == EnemyArchetype.RangedShooter || archetype == EnemyArchetype.EliteRanged ? 4.5f : 1.05f;
            }

            public string AssetName { get; }
            public string Id { get; }
            public string DisplayName { get; }
            public EnemyArchetype Archetype { get; }
            public EnemyRank Rank { get; }
            public float MaxHealth { get; }
            public float MoveSpeed { get; }
            public float ContactDamage { get; }
            public float AttackRange { get; }
            public float AttackCooldown { get; }
            public float AttackWindup { get; }
            public float ProjectileSpeed { get; }
            public int ProjectileCount { get; }
            public float ExplosionRadius { get; }
            public float KnockbackResistance { get; }
            public bool CanBeKnockedBack { get; }
            public bool CanBeInterrupted { get; }
            public float StoppingDistance { get; }
        }
    }
}
