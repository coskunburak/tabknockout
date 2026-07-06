using NUnit.Framework;
using TapKnockout.Enemy;
using TapKnockout.Player;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Survivor.Tests
{
    public sealed class EnemyAttackMechanicsReadinessTests
    {
        private const string ConfigRoot = "Assets/_Project/ScriptableObjects/Enemies/CuteMonsters/AttackConfigs";
        private const string PrefabRoot = "Assets/_Project/Prefabs/Enemies/CuteMonsters";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player/Player.prefab";

        private static readonly ExpectedEnemy[] ExpectedEnemies =
        {
            new ExpectedEnemy("Bat", "PF_Enemy_Bat", new[] { "AC_Bat_FlyingDive" }),
            new ExpectedEnemy("Bee", "PF_Enemy_Bee", new[] { "AC_Bee_StingCharge" }),
            new ExpectedEnemy("GreenDemon", "PF_Enemy_BasicMelee_GreenDemon_Generated", new[] { "AC_GreenDemon_MeleeArc" }),
            new ExpectedEnemy("YellowDragon", "PF_Boss_YellowDragon", new[] { "AC_YellowDragon_Fireball" }),
            new ExpectedEnemy("Cactus", "PF_Enemy_Cactus", new[] { "AC_Cactus_SpikeProjectile", "AC_Cactus_RadialSpikeBurst" }),
            new ExpectedEnemy("Cthulhu", "PF_Enemy_Cthulhu", new[] { "AC_Cthulhu_SlimeProjectileSlowPool" }),
            new ExpectedEnemy("Cyclops", "PF_Enemy_Cyclops", new[] { "AC_Cyclops_EyeBeam" }),
            new ExpectedEnemy("Demon", "PF_Enemy_Demon", new[] { "AC_Demon_LeapSlash" }),
            new ExpectedEnemy("Ghost", "PF_Enemy_Ghost", new[] { "AC_Ghost_PhaseHomingCurse" }),
            new ExpectedEnemy("Mushroom", "PF_Enemy_Mushroom", new[] { "AC_Mushroom_SporePoisonZone" }),
            new ExpectedEnemy("Yeti", "PF_Enemy_Yeti", new[] { "AC_Yeti_FrostSlamShockwave" })
        };

        [Test]
        public void RuntimeEnemyPrefabs_HaveValidDistinctAttackControllers()
        {
            foreach (var expected in ExpectedEnemies)
            {
                var prefab = LoadPrefab(expected.PrefabName);
                Assert.That(prefab, Is.Not.Null, expected.PrefabName);

                var controller = prefab.GetComponent<EnemyDistinctAttackController>();
                Assert.That(controller, Is.Not.Null, expected.PrefabName);
                Assert.That(controller.enabled, Is.True, expected.PrefabName);

                var configs = ReadAssignedConfigs(controller);
                Assert.That(configs.Length, Is.EqualTo(expected.ConfigNames.Length), expected.PrefabName);
                Assert.That(EnemyAttackReadinessUtility.IsDistinctAttackSystemReady(configs, out var reason), Is.True, $"{expected.PrefabName}: {reason}");

                foreach (var configName in expected.ConfigNames)
                {
                    Assert.That(ContainsConfig(configs, LoadConfig(configName)), Is.True, $"{expected.PrefabName} missing {configName}");
                }
            }
        }

        [Test]
        public void AllAttackConfigs_AreCreatedAndGameplayReady()
        {
            foreach (var expected in ExpectedEnemies)
            {
                foreach (var configName in expected.ConfigNames)
                {
                    var config = LoadConfig(configName);
                    Assert.That(config, Is.Not.Null, configName);
                    Assert.That(EnemyAttackReadinessUtility.IsConfigGameplayReady(config, out var reason), Is.True, $"{configName}: {reason}");
                }
            }
        }

        [Test]
        public void AllAttackConfigs_HitMaskIncludesPlayerHurtboxLayer()
        {
            var playerLayer = ResolvePlayerHurtboxLayer();
            Assert.That(playerLayer, Is.GreaterThanOrEqualTo(0));

            foreach (var expected in ExpectedEnemies)
            {
                foreach (var configName in expected.ConfigNames)
                {
                    var config = LoadConfig(configName);
                    Assert.That(config, Is.Not.Null, configName);
                    Assert.That(
                        (config.HitLayerMask.value & (1 << playerLayer)) != 0,
                        Is.True,
                        $"{configName} does not include player/hurtbox layer {playerLayer}");
                }
            }
        }

        [Test]
        public void ProjectileConfigs_HaveProjectilePrefabAssigned()
        {
            foreach (var configName in new[]
                     {
                         "AC_YellowDragon_Fireball",
                         "AC_Cactus_SpikeProjectile",
                         "AC_Cthulhu_SlimeProjectileSlowPool",
                         "AC_Ghost_PhaseHomingCurse"
                     })
            {
                var config = LoadConfig(configName);
                Assert.That(config, Is.Not.Null, configName);
                Assert.That(config.NeedsProjectile, Is.True, configName);
                Assert.That(config.ProjectilePrefab, Is.Not.Null, configName);
            }
        }

        [Test]
        public void AreaConfigs_HaveAreaZonePrefabAssigned()
        {
            foreach (var configName in new[]
                     {
                         "AC_Cthulhu_SlimeProjectileSlowPool",
                         "AC_Mushroom_SporePoisonZone",
                         "AC_Yeti_FrostSlamShockwave"
                     })
            {
                var config = LoadConfig(configName);
                Assert.That(config, Is.Not.Null, configName);
                Assert.That(config.NeedsAreaZone, Is.True, configName);
                Assert.That(config.AreaZonePrefab, Is.Not.Null, configName);
            }
        }

        [Test]
        public void Cactus_HasSpikeAndRadialConfigs()
        {
            var cactus = LoadPrefab("PF_Enemy_Cactus");
            var controller = cactus != null ? cactus.GetComponent<EnemyDistinctAttackController>() : null;
            var configs = ReadAssignedConfigs(controller);

            Assert.That(ContainsConfig(configs, LoadConfig("AC_Cactus_SpikeProjectile")), Is.True);
            Assert.That(ContainsConfig(configs, LoadConfig("AC_Cactus_RadialSpikeBurst")), Is.True);
        }

        [Test]
        public void ReadinessUtility_FailsWhenRequiredProjectileReferenceIsNull()
        {
            var config = ScriptableObject.CreateInstance<EnemyAttackConfig>();
            var activeVfx = new GameObject("ActiveVfx");
            var impactVfx = new GameObject("ImpactVfx");
            var telegraph = new GameObject("Telegraph");

            try
            {
                var so = new SerializedObject(config);
                SetString(so, "attackId", "test_missing_projectile");
                SetString(so, "displayName", "Test Missing Projectile");
                SetEnum(so, "attackType", (int)EnemyDistinctAttackType.Projectile);
                SetFloat(so, "triggerRange", 5f);
                SetFloat(so, "cooldown", 1f);
                SetFloat(so, "windupTime", 0.3f);
                SetFloat(so, "activeTime", 0.1f);
                SetFloat(so, "recoveryTime", 0.2f);
                SetFloat(so, "damage", 1f);
                SetLayerMask(so, "hitLayerMask", 1);
                SetObject(so, "telegraphPrefab", telegraph);
                SetObject(so, "activeVfxPrefab", activeVfx);
                SetObject(so, "impactVfxPrefab", impactVfx);
                so.ApplyModifiedPropertiesWithoutUndo();

                Assert.That(EnemyAttackReadinessUtility.IsConfigGameplayReady(config, out var reason), Is.False);
                Assert.That(reason, Does.Contain("projectile"));
            }
            finally
            {
                Object.DestroyImmediate(config);
                Object.DestroyImmediate(activeVfx);
                Object.DestroyImmediate(impactVfx);
                Object.DestroyImmediate(telegraph);
            }
        }

        [Test]
        public void ContactDamage_IsNotDisabledUnlessDistinctAttacksAreReady()
        {
            foreach (var expected in ExpectedEnemies)
            {
                var prefab = LoadPrefab(expected.PrefabName);
                var legacyAttack = prefab != null ? prefab.GetComponent<EnemyAttackController>() : null;
                var distinct = prefab != null ? prefab.GetComponent<EnemyDistinctAttackController>() : null;
                var configs = ReadAssignedConfigs(distinct);
                var distinctReady = distinct != null &&
                    distinct.enabled &&
                    EnemyAttackReadinessUtility.IsDistinctAttackSystemReady(configs, out _);
                var contactDamageEnabled = ReadBool(legacyAttack, "autoDealContactDamage", fallback: true);

                Assert.That(contactDamageEnabled || distinctReady, Is.True, expected.PrefabName);
            }
        }

        [Test]
        public void DistinctAttackController_DisableDoesNotRestoreLockedMovement()
        {
            var enemy = new GameObject("EnemyWithDistinctAttack");
            try
            {
                var movement = enemy.AddComponent<EnemyMovement>();
                var distinct = enemy.AddComponent<EnemyDistinctAttackController>();

                movement.enabled = false;
                distinct.enabled = false;

                Assert.That(movement.enabled, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(enemy);
            }
        }

        [Test]
        public void PlayerPrefab_HasDamageReceiverAndCombatHurtbox()
        {
            var player = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            Assert.That(player, Is.Not.Null);
            Assert.That(player.GetComponent<PlayerHealth>(), Is.Not.Null);

            var hurtbox = FindChild(player.transform, "CombatHurtbox");
            Assert.That(hurtbox, Is.Not.Null);
            var collider = hurtbox.GetComponent<Collider>();
            Assert.That(collider, Is.Not.Null);
            Assert.That(collider.enabled, Is.True);
        }

        private static GameObject LoadPrefab(string prefabName)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/{prefabName}.prefab");
        }

        private static EnemyAttackConfig LoadConfig(string configName)
        {
            return AssetDatabase.LoadAssetAtPath<EnemyAttackConfig>($"{ConfigRoot}/{configName}.asset");
        }

        private static EnemyAttackConfig[] ReadAssignedConfigs(EnemyDistinctAttackController controller)
        {
            if (controller == null)
            {
                return System.Array.Empty<EnemyAttackConfig>();
            }

            var property = new SerializedObject(controller).FindProperty("attackConfigs");
            if (property == null || property.arraySize == 0)
            {
                return System.Array.Empty<EnemyAttackConfig>();
            }

            var configs = new EnemyAttackConfig[property.arraySize];
            for (var i = 0; i < property.arraySize; i++)
            {
                configs[i] = property.GetArrayElementAtIndex(i).objectReferenceValue as EnemyAttackConfig;
            }

            return configs;
        }

        private static bool ContainsConfig(EnemyAttackConfig[] configs, EnemyAttackConfig expected)
        {
            for (var i = 0; i < configs.Length; i++)
            {
                if (configs[i] == expected)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ReadBool(Component component, string propertyName, bool fallback)
        {
            if (component == null)
            {
                return fallback;
            }

            var property = new SerializedObject(component).FindProperty(propertyName);
            return property != null ? property.boolValue : fallback;
        }

        private static int ResolvePlayerHurtboxLayer()
        {
            var player = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (player == null)
            {
                return -1;
            }

            var hurtbox = FindChild(player.transform, "CombatHurtbox");
            return hurtbox != null ? hurtbox.gameObject.layer : player.layer;
        }

        private static Transform FindChild(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child.name == childName)
                {
                    return child;
                }

                var nested = FindChild(child, childName);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }

        private static void SetString(SerializedObject so, string fieldName, string value)
        {
            so.FindProperty(fieldName).stringValue = value;
        }

        private static void SetEnum(SerializedObject so, string fieldName, int value)
        {
            so.FindProperty(fieldName).enumValueIndex = value;
        }

        private static void SetFloat(SerializedObject so, string fieldName, float value)
        {
            so.FindProperty(fieldName).floatValue = value;
        }

        private static void SetLayerMask(SerializedObject so, string fieldName, int value)
        {
            so.FindProperty(fieldName).intValue = value;
        }

        private static void SetObject(SerializedObject so, string fieldName, Object value)
        {
            so.FindProperty(fieldName).objectReferenceValue = value;
        }

        private readonly struct ExpectedEnemy
        {
            public ExpectedEnemy(string enemyName, string prefabName, string[] configNames)
            {
                EnemyName = enemyName;
                PrefabName = prefabName;
                ConfigNames = configNames;
            }

            public string EnemyName { get; }
            public string PrefabName { get; }
            public string[] ConfigNames { get; }
        }
    }
}
