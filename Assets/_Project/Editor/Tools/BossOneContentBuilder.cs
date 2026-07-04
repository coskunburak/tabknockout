using System.IO;
using TapKnockout.Boss;
using TapKnockout.Enemy;
using TapKnockout.VFX;
using TapKnockout.Wave;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Editor.Tools
{
    public static class BossOneContentBuilder
    {
        private const string MenuPath = "Tools/Tap Knockout/Bosses/Create Boss 1 Dash-Counter Brute";
        private const string BossFolder = "Assets/_Project/ScriptableObjects/Bosses";
        private const string WaveFolder = "Assets/_Project/ScriptableObjects/Waves";
        private const string DefaultAddConfigPath = "Assets/_Project/ScriptableObjects/Enemies/EnemyConfig_MeleeChaser.asset";
        private const string DefaultAddPrefabPath = "Assets/_Project/Prefabs/Enemies/Generated/PF_Enemy_BasicMelee_GreenDemon_Generated.prefab";

        [MenuItem(MenuPath)]
        public static void CreateBossOneDashCounterBrute()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Boss 1", "Exit Play Mode before creating Boss 1 configs.", "OK");
                return;
            }

            EnsureFolder(BossFolder);
            EnsureFolder(WaveFolder);

            var phase1 = CreatePatternIfMissing(
                $"{BossFolder}/BossPattern_DashCounterBrute_Phase1.asset",
                new[]
                {
                    Slam(0.65f, 0.08f, 0.7f, 16f, 2f),
                    Idle(0.25f),
                    Slam(0.6f, 0.08f, 0.75f, 17f, 2.05f),
                    Idle(0.35f)
                });
            var phase2 = CreatePatternIfMissing(
                $"{BossFolder}/BossPattern_DashCounterBrute_Phase2.asset",
                new[]
                {
                    Charge(0.55f, 0.45f, 0.75f, 18f, 8.5f),
                    Slam(0.6f, 0.08f, 0.65f, 18f, 2.15f),
                    Adds(0.45f, 0.1f, 1.05f, 2),
                    Charge(0.5f, 0.45f, 0.8f, 18f, 8.75f)
                });
            var phase3 = CreatePatternIfMissing(
                $"{BossFolder}/BossPattern_DashCounterBrute_Phase3.asset",
                new[]
                {
                    Enrage(0.35f, 0.2f, 0.35f),
                    Charge(0.45f, 0.5f, 0.52f, 20f, 9.75f),
                    Slam(0.5f, 0.08f, 0.5f, 22f, 2.6f),
                    Adds(0.35f, 0.1f, 0.75f, 2),
                    Charge(0.42f, 0.5f, 0.52f, 20f, 10f),
                    Slam(0.48f, 0.08f, 0.55f, 23f, 2.7f)
                });

            var bossEnemyConfig = CreateBossEnemyConfigIfMissing($"{BossFolder}/EnemyConfig_Boss1_DashCounterBrute.asset");
            var addEnemyConfig = ResolveDefaultAddEnemyConfig();
            var addEnemyPrefab = ResolveDefaultAddEnemyPrefab();
            var bossConfig = CreateBossConfigIfMissing(
                $"{BossFolder}/BossConfig_DashCounterBrute.asset",
                bossEnemyConfig,
                phase1,
                phase2,
                phase3,
                addEnemyConfig,
                addEnemyPrefab);
            RepairBossConfigMissingReferences(bossConfig, bossEnemyConfig, phase1, phase2, phase3, addEnemyConfig, addEnemyPrefab);
            CreateBossTestWaveIfMissing($"{WaveFolder}/Wave_Test_Boss1_DashCounterBrute.asset", bossEnemyConfig);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = bossConfig;
            EditorGUIUtility.PingObject(bossConfig);
            Debug.Log($"{nameof(BossOneContentBuilder)} created Boss 1 Dash-Counter Brute config foundation in {BossFolder}. Existing assets were left unchanged.");
            EditorUtility.DisplayDialog("Boss 1", "Boss 1 config foundation is ready. Existing assets were not overwritten.", "OK");
        }

        private static BossPatternConfig CreatePatternIfMissing(string path, BossAttackStep[] steps)
        {
            var existing = AssetDatabase.LoadAssetAtPath<BossPatternConfig>(path);
            if (existing != null)
            {
                Debug.Log($"Skipped existing boss pattern without overwriting: {path}");
                return existing;
            }

            var pattern = ScriptableObject.CreateInstance<BossPatternConfig>();
            Undo.RegisterCreatedObjectUndo(pattern, "Create Boss Pattern");
            AssetDatabase.CreateAsset(pattern, path);
            pattern.SetLoop(true);
            pattern.SetSteps(steps);
            EditorUtility.SetDirty(pattern);
            return pattern;
        }

        private static EnemyConfig CreateBossEnemyConfigIfMissing(string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<EnemyConfig>(path);
            if (existing != null)
            {
                Debug.Log($"Skipped existing boss enemy config without overwriting: {path}");
                return existing;
            }

            var config = ScriptableObject.CreateInstance<EnemyConfig>();
            Undo.RegisterCreatedObjectUndo(config, "Create Boss Enemy Config");
            AssetDatabase.CreateAsset(config, path);

            var serializedObject = new SerializedObject(config);
            SetString(serializedObject, "enemyId", "boss_dash_counter_brute");
            SetString(serializedObject, "displayName", "Dash-Counter Brute");
            SetEnum(serializedObject, "archetype", (int)EnemyArchetype.Boss);
            SetEnum(serializedObject, "rank", (int)EnemyRank.Boss);
            SetFloat(serializedObject, "maxHealth", 450f);
            SetFloat(serializedObject, "moveSpeed", 1.75f);
            SetFloat(serializedObject, "contactDamage", 15f);
            SetFloat(serializedObject, "attackRange", 8f);
            SetFloat(serializedObject, "attackCooldown", 1.2f);
            SetFloat(serializedObject, "attackWindup", 0.6f);
            SetFloat(serializedObject, "knockbackResistance", 0.65f);
            SetFloat(serializedObject, "stunResistance", 0.75f);
            SetBool(serializedObject, "canBeKnockedBack", true);
            SetBool(serializedObject, "canBeInterrupted", false);
            SetEnum(serializedObject, "spawnVfx", (int)VFXEventType.BossWarning);
            SetEnum(serializedObject, "attackVfx", (int)VFXEventType.BossHit);
            SetEnum(serializedObject, "deathVfx", (int)VFXEventType.BossDeath);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            return config;
        }

        private static BossConfig CreateBossConfigIfMissing(
            string path,
            EnemyConfig enemyConfig,
            BossPatternConfig phase1,
            BossPatternConfig phase2,
            BossPatternConfig phase3,
            EnemyConfig addEnemyConfig,
            GameObject addEnemyPrefab)
        {
            var existing = AssetDatabase.LoadAssetAtPath<BossConfig>(path);
            if (existing != null)
            {
                Debug.Log($"Loaded existing boss config and will fill missing references only: {path}");
                return existing;
            }

            var config = ScriptableObject.CreateInstance<BossConfig>();
            Undo.RegisterCreatedObjectUndo(config, "Create Boss Config");
            AssetDatabase.CreateAsset(config, path);

            var serializedObject = new SerializedObject(config);
            SetString(serializedObject, "bossId", "boss_dash_counter_brute");
            SetString(serializedObject, "displayName", "Dash-Counter Brute");
            SetObject(serializedObject, "enemyConfig", enemyConfig);
            SetObject(serializedObject, "addEnemyConfig", addEnemyConfig);
            SetObject(serializedObject, "addEnemyPrefab", addEnemyPrefab);
            SetInt(serializedObject, "maxActiveAdds", 4);
            SetEnum(serializedObject, "introVfx", (int)VFXEventType.BossWarning);
            SetEnum(serializedObject, "enrageVfx", (int)VFXEventType.BossPatternTelegraph);
            SetEnum(serializedObject, "deathVfx", (int)VFXEventType.BossDeath);

            var phases = serializedObject.FindProperty("phases");
            if (phases != null)
            {
                phases.arraySize = 3;
                ApplyPhase(phases.GetArrayElementAtIndex(0), BossPhaseState.Phase1, 1f, phase1, false, 1f, 1f);
                ApplyPhase(phases.GetArrayElementAtIndex(1), BossPhaseState.Phase2, 0.66f, phase2, false, 0.9f, 1.05f);
                ApplyPhase(phases.GetArrayElementAtIndex(2), BossPhaseState.Phase3, 0.33f, phase3, true, 0.72f, 1.18f);
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            return config;
        }

        private static void RepairBossConfigMissingReferences(
            BossConfig config,
            EnemyConfig bossEnemyConfig,
            BossPatternConfig phase1,
            BossPatternConfig phase2,
            BossPatternConfig phase3,
            EnemyConfig addEnemyConfig,
            GameObject addEnemyPrefab)
        {
            if (config == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(config);
            var changed = false;

            changed |= SetObjectIfMissing(serializedObject, "enemyConfig", bossEnemyConfig);
            changed |= SetObjectIfMissing(serializedObject, "addEnemyConfig", addEnemyConfig);
            changed |= SetObjectIfMissing(serializedObject, "addEnemyPrefab", addEnemyPrefab);

            var phases = serializedObject.FindProperty("phases");
            if (phases != null)
            {
                if (phases.arraySize < 3)
                {
                    phases.arraySize = 3;
                    changed = true;
                }

                changed |= FillMissingPhase(phases.GetArrayElementAtIndex(0), BossPhaseState.Phase1, 1f, phase1, false, 1f, 1f);
                changed |= FillMissingPhase(phases.GetArrayElementAtIndex(1), BossPhaseState.Phase2, 0.66f, phase2, false, 0.9f, 1.05f);
                changed |= FillMissingPhase(phases.GetArrayElementAtIndex(2), BossPhaseState.Phase3, 0.33f, phase3, true, 0.72f, 1.18f);
            }

            if (!changed)
            {
                return;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            Debug.Log($"{nameof(BossOneContentBuilder)} filled missing BossConfig references for {AssetDatabase.GetAssetPath(config)}.", config);
        }

        private static EnemyConfig ResolveDefaultAddEnemyConfig()
        {
            return AssetDatabase.LoadAssetAtPath<EnemyConfig>(DefaultAddConfigPath)
                ?? AssetDatabase.LoadAssetAtPath<EnemyConfig>("Assets/_Project/ScriptableObjects/Enemies/Generated/EnemyConfig_BasicMelee_GreenDemon.asset");
        }

        private static GameObject ResolveDefaultAddEnemyPrefab()
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(DefaultAddPrefabPath)
                ?? AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/PF_Enemy_MeleeChaser_Test.prefab");
        }

        private static WaveConfig CreateBossTestWaveIfMissing(string path, EnemyConfig bossEnemyConfig)
        {
            var existing = AssetDatabase.LoadAssetAtPath<WaveConfig>(path);
            if (existing != null)
            {
                Debug.Log($"Skipped existing boss test wave without overwriting: {path}");
                return existing;
            }

            var wave = ScriptableObject.CreateInstance<WaveConfig>();
            Undo.RegisterCreatedObjectUndo(wave, "Create Boss Test Wave");
            AssetDatabase.CreateAsset(wave, path);

            var serializedObject = new SerializedObject(wave);
            SetString(serializedObject, "waveId", "wave_test_boss_dash_counter_brute");
            SetFloat(serializedObject, "startDelay", 0.75f);
            SetBool(serializedObject, "completeWhenAllSpawnedEnemiesDead", true);

            var enemies = serializedObject.FindProperty("enemies");
            if (enemies != null)
            {
                enemies.arraySize = 1;
                var entry = enemies.GetArrayElementAtIndex(0);
                SetRelativeObject(entry, "enemyConfig", bossEnemyConfig);
                SetRelativeObject(entry, "enemyPrefab", null);
                SetRelativeInt(entry, "count", 1);
                SetRelativeFloat(entry, "spawnDelay", 0f);
                SetRelativeInt(entry, "spawnPointIndex", 0);
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(wave);
            return wave;
        }

        private static BossAttackStep Slam(float windup, float active, float cooldown, float damage, float radius)
        {
            return new BossAttackStep(BossAttackType.BossSlam, windup, active, cooldown, damage, radius, 0f, 0, EnemyTelegraphType.BossSlamArea, VFXEventType.BossPatternTelegraph);
        }

        private static BossAttackStep Charge(float windup, float active, float cooldown, float damage, float speed)
        {
            return new BossAttackStep(BossAttackType.BossCharge, windup, active, cooldown, damage, 0f, speed, 0, EnemyTelegraphType.ChargePath, VFXEventType.BossWarning);
        }

        private static BossAttackStep Adds(float windup, float active, float cooldown, int addCount)
        {
            return new BossAttackStep(BossAttackType.SummonAdds, windup, active, cooldown, 0f, 0f, 0f, addCount, EnemyTelegraphType.Circle, VFXEventType.BossWarning);
        }

        private static BossAttackStep Enrage(float windup, float active, float cooldown)
        {
            return new BossAttackStep(BossAttackType.EnragePulse, windup, active, cooldown, 0f, 0f, 0f, 0, EnemyTelegraphType.BossSlamArea, VFXEventType.BossPatternTelegraph);
        }

        private static BossAttackStep Idle(float cooldown)
        {
            return new BossAttackStep(BossAttackType.IdleWait, 0f, 0f, cooldown, 0f, 0f, 0f, 0, EnemyTelegraphType.None, VFXEventType.GenericBurst);
        }

        private static void ApplyPhase(
            SerializedProperty phase,
            BossPhaseState phaseState,
            float threshold,
            BossPatternConfig pattern,
            bool enrage,
            float cooldownMultiplier,
            float chargeSpeedMultiplier)
        {
            SetRelativeEnum(phase, "phaseState", (int)phaseState);
            SetRelativeFloat(phase, "enterAtOrBelowHealthPercent", threshold);
            SetRelativeObject(phase, "pattern", pattern);
            SetRelativeBool(phase, "enrage", enrage);
            SetRelativeFloat(phase, "cooldownDurationMultiplier", cooldownMultiplier);
            SetRelativeFloat(phase, "chargeSpeedMultiplier", chargeSpeedMultiplier);
            SetRelativeEnum(phase, "phaseChangedVfx", (int)VFXEventType.BossPatternTelegraph);
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

        private static void SetObject(SerializedObject serializedObject, string propertyName, Object value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        private static bool SetObjectIfMissing(SerializedObject serializedObject, string propertyName, Object value)
        {
            if (value == null)
            {
                return false;
            }

            var property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue != null)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }

        private static bool FillMissingPhase(
            SerializedProperty phase,
            BossPhaseState phaseState,
            float threshold,
            BossPatternConfig pattern,
            bool enrage,
            float cooldownMultiplier,
            float chargeSpeedMultiplier)
        {
            if (phase == null)
            {
                return false;
            }

            var changed = false;
            changed |= SetRelativeEnumIfDefault(phase, "phaseState", (int)phaseState);
            changed |= SetRelativeFloatIfDefault(phase, "enterAtOrBelowHealthPercent", threshold);
            changed |= SetRelativeObjectIfMissing(phase, "pattern", pattern);
            changed |= SetRelativeBoolIfDifferent(phase, "enrage", enrage);
            changed |= SetRelativeFloatIfDefault(phase, "cooldownDurationMultiplier", cooldownMultiplier);
            changed |= SetRelativeFloatIfDefault(phase, "chargeSpeedMultiplier", chargeSpeedMultiplier);
            changed |= SetRelativeEnumIfDefault(phase, "phaseChangedVfx", (int)VFXEventType.BossPatternTelegraph);
            return changed;
        }

        private static void SetRelativeEnum(SerializedProperty parent, string propertyName, int enumValueIndex)
        {
            var property = parent.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.enumValueIndex = enumValueIndex;
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

        private static bool SetRelativeEnumIfDefault(SerializedProperty parent, string propertyName, int enumValueIndex)
        {
            var property = parent.FindPropertyRelative(propertyName);
            if (property == null || property.enumValueIndex != 0)
            {
                return false;
            }

            property.enumValueIndex = enumValueIndex;
            return true;
        }

        private static bool SetRelativeFloatIfDefault(SerializedProperty parent, string propertyName, float value)
        {
            var property = parent.FindPropertyRelative(propertyName);
            if (property == null || property.floatValue > 0f)
            {
                return false;
            }

            property.floatValue = value;
            return true;
        }

        private static bool SetRelativeBoolIfDifferent(SerializedProperty parent, string propertyName, bool value)
        {
            var property = parent.FindPropertyRelative(propertyName);
            if (property == null || property.boolValue == value)
            {
                return false;
            }

            property.boolValue = value;
            return true;
        }

        private static void SetRelativeBool(SerializedProperty parent, string propertyName, bool value)
        {
            var property = parent.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.boolValue = value;
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

        private static bool SetRelativeObjectIfMissing(SerializedProperty parent, string propertyName, Object value)
        {
            if (value == null)
            {
                return false;
            }

            var property = parent.FindPropertyRelative(propertyName);
            if (property == null || property.objectReferenceValue != null)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }

        private static void SetRelativeInt(SerializedProperty parent, string propertyName, int value)
        {
            var property = parent.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.intValue = value;
            }
        }

    }
}
