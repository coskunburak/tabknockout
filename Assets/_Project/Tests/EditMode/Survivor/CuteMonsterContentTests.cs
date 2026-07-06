using System.Reflection;
using TapKnockout.Combat;
using NUnit.Framework;
using TapKnockout.Enemy;
using TapKnockout.Projectile;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace TapKnockout.Survivor.Tests
{
    public sealed class CuteMonsterContentTests
    {
        private const string ConfigRoot = "Assets/_Project/ScriptableObjects/Enemies/CuteMonsters";
        private const string PrefabRoot = "Assets/_Project/Prefabs/Enemies/CuteMonsters";
        private const string SpawnGroupRoot = "Assets/_Project/ScriptableObjects/Waves/CuteMonsters";
        private const string ControllerRoot = "Assets/_Project/Animation/Controllers/CuteMonsters";
        private const string FbxRoot = "Assets/Assets/game asset packs/Cute Animated Monsters - Aug 2020/FBX";
        private const string ProjectilePrefabPath = "Assets/_Project/Prefabs/Projectiles/PF_EnemyProjectile_CuteMonster.prefab";
        private const string TimelinePath = SpawnGroupRoot + "/WaveTimeline_CuteMonsters_Test.asset";
        private const string DesktopTimelinePath = "Assets/_Project/ScriptableObjects/Waves/WaveTimeline_DesktopSurvivorPrototype.asset";
        private const string RunConfigPath = "Assets/_Project/ScriptableObjects/Runs/RunConfig_DesktopSurvivorPrototype.asset";
        private const string IdleState = "Idle";
        private const string MoveState = "Move";
        private const string AttackState = "Attack";
        private const string HitState = "Hit";
        private const string DeathState = "Death";

        private static readonly ExpectedEnemy[] ExpectedEnemies =
        {
            new ExpectedEnemy("GreenDemon", "EnemyConfig_GreenDemon", "PF_Enemy_BasicMelee_GreenDemon_Generated", EnemyArchetype.MeleeChaser, contactDamageEnabled: false),
            new ExpectedEnemy("Demon", "EnemyConfig_Demon", "PF_Enemy_Demon", EnemyArchetype.MeleeChaser, contactDamageEnabled: false),
            new ExpectedEnemy("Bat", "EnemyConfig_Bat", "PF_Enemy_Bat", EnemyArchetype.FastCharger, contactDamageEnabled: false, usesFlyingLocomotion: true),
            new ExpectedEnemy("Bee", "EnemyConfig_Bee", "PF_Enemy_Bee", EnemyArchetype.FastCharger, contactDamageEnabled: false, usesFlyingLocomotion: true),
            new ExpectedEnemy("Mushroom", "EnemyConfig_Mushroom", "PF_Enemy_Mushroom", EnemyArchetype.MeleeChaser, contactDamageEnabled: false),
            new ExpectedEnemy("Cyclops", "EnemyConfig_Cyclops", "PF_Enemy_Cyclops", EnemyArchetype.ShieldEnemy, contactDamageEnabled: false, usesContactWindup: true),
            new ExpectedEnemy("Yeti", "EnemyConfig_Yeti", "PF_Enemy_Yeti", EnemyArchetype.ShieldEnemy, contactDamageEnabled: false, usesContactWindup: true),
            new ExpectedEnemy("Cactus", "EnemyConfig_Cactus", "PF_Enemy_Cactus", EnemyArchetype.ShieldEnemy, contactDamageEnabled: false, usesContactWindup: true),
            new ExpectedEnemy("Ghost", "EnemyConfig_Ghost", "PF_Enemy_Ghost", EnemyArchetype.FastCharger, contactDamageEnabled: false, usesFlyingLocomotion: true),
            new ExpectedEnemy("Cthulhu", "EnemyConfig_Cthulhu", "PF_Enemy_Cthulhu", EnemyArchetype.RangedShooter, contactDamageEnabled: false, isRanged: true, usesFlyingLocomotion: true),
            new ExpectedEnemy("YellowDragon", "EnemyConfig_YellowDragon_Boss", "PF_Boss_YellowDragon", EnemyArchetype.Boss, contactDamageEnabled: false, usesContactWindup: true, isBoss: true, usesFlyingLocomotion: true)
        };

        [Test]
        public void CuteMonsterEnemyConfigs_ExistWithStableIdsAndPlayableValues()
        {
            foreach (var expected in ExpectedEnemies)
            {
                var config = LoadConfig(expected);

                Assert.That(config, Is.Not.Null, expected.ConfigName);
                Assert.That(config.EnemyId, Does.StartWith("cute_"), expected.ConfigName);
                Assert.That(config.Archetype, Is.EqualTo(expected.Archetype), expected.ConfigName);
                Assert.That(config.MaxHealth, Is.GreaterThan(0f), expected.ConfigName);
                Assert.That(config.MoveSpeed, Is.GreaterThan(0f), expected.ConfigName);
                Assert.That(config.AttackCooldown, Is.GreaterThan(0f), expected.ConfigName);
                Assert.That(config.XpReward, Is.GreaterThan(0), expected.ConfigName);

                if (expected.IsRanged)
                {
                    Assert.That(config.ProjectilePrefab, Is.EqualTo(AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePrefabPath)), expected.ConfigName);
                    Assert.That(config.ProjectileSpeed, Is.GreaterThan(0f), expected.ConfigName);
                    Assert.That(config.StoppingDistance, Is.GreaterThan(2f), expected.ConfigName);
                }
            }
        }

        [Test]
        public void CuteMonsterEnemyPrefabs_MeetRuntimeContract()
        {
            foreach (var expected in ExpectedEnemies)
            {
                var prefab = LoadPrefab(expected);
                var config = LoadConfig(expected);

                Assert.That(prefab, Is.Not.Null, expected.PrefabName);
                Assert.That(config, Is.Not.Null, expected.ConfigName);
                AssertEnemyLayer(prefab);

                var controller = prefab.GetComponent<EnemyController>();
                var health = prefab.GetComponent<EnemyHealth>();
                var movement = prefab.GetComponent<EnemyMovement>();
                var attack = prefab.GetComponent<EnemyAttackController>();
                var rigidbody = prefab.GetComponent<Rigidbody>();
                var collider = prefab.GetComponent<Collider>();

                Assert.That(controller, Is.Not.Null, expected.PrefabName);
                Assert.That(health, Is.Not.Null, expected.PrefabName);
                Assert.That(movement, Is.Not.Null, expected.PrefabName);
                Assert.That(attack, Is.Not.Null, expected.PrefabName);
                Assert.That(prefab.GetComponent<KnockbackReceiver>(), Is.Not.Null, expected.PrefabName);
                Assert.That(prefab.GetComponent<PooledEnemy>(), Is.Not.Null, expected.PrefabName);
                Assert.That(HasComponentNamed(prefab, "HitFlashController"), Is.True, expected.PrefabName);
                Assert.That(HasComponentNamed(prefab, "CharacterAnimationDriver"), Is.True, expected.PrefabName);
                Assert.That(collider, Is.Not.Null, expected.PrefabName);
                Assert.That(collider.enabled, Is.True, expected.PrefabName);
                Assert.That(rigidbody, Is.Not.Null, expected.PrefabName);
                Assert.That(rigidbody.useGravity, Is.False, expected.PrefabName);
                Assert.That((rigidbody.constraints & RigidbodyConstraints.FreezeRotationX) != 0, Is.True, expected.PrefabName);
                Assert.That((rigidbody.constraints & RigidbodyConstraints.FreezeRotationZ) != 0, Is.True, expected.PrefabName);

                Assert.That(controller.Config, Is.EqualTo(config), expected.PrefabName);
                Assert.That(controller.Target, Is.Null, expected.PrefabName);
                Assert.That(health.Config, Is.EqualTo(config), expected.PrefabName);
                Assert.That(health.TargetTransform, Is.EqualTo(prefab.transform), expected.PrefabName);
                Assert.That(movement.Target, Is.Null, expected.PrefabName);
                Assert.That(attack.Target, Is.Null, expected.PrefabName);

                Assert.That(FindChild(prefab.transform, "VisualRoot"), Is.Not.Null, expected.PrefabName);
                Assert.That(FindChild(prefab.transform, "AttackOrigin"), Is.Not.Null, expected.PrefabName);
                Assert.That(FindChild(prefab.transform, "HitReactionRoot"), Is.Not.Null, expected.PrefabName);
                Assert.That(FindChild(prefab.transform, "HitVFXSocket"), Is.Not.Null, expected.PrefabName);
                Assert.That(FindChild(prefab.transform, "DeathVFXSocket"), Is.Not.Null, expected.PrefabName);
                AssertCombatHurtbox(prefab, expected);
                Assert.That(FindChild(prefab.transform, "TelegraphRoot"), Is.Not.Null, expected.PrefabName);
                Assert.That(prefab.GetComponentInChildren<Animator>(true), Is.Not.Null, expected.PrefabName);
            }
        }

        [Test]
        public void CuteMonsterCombatHurtboxes_CatchProjectileLinesAboveSmallPhysicalCapsules()
        {
            foreach (var expected in ExpectedEnemies)
            {
                AssertProjectileHitsCombatHurtbox(expected.PrefabName);
            }
        }

        [Test]
        public void CuteMonsterAttackArchetypes_AreDistinctAndSafe()
        {
            foreach (var expected in ExpectedEnemies)
            {
                var prefab = LoadPrefab(expected);
                var attack = prefab.GetComponent<EnemyAttackController>();

                Assert.That(GetBool(attack, "autoDealContactDamage", true), Is.EqualTo(expected.ContactDamageEnabled), expected.PrefabName);
                Assert.That(GetBool(attack, "useTelegraphWindup", false), Is.EqualTo(expected.UsesContactWindup), expected.PrefabName);

                if (expected.IsRanged)
                {
                    Assert.That(prefab.GetComponent<RangedShooterController>(), Is.Not.Null, expected.PrefabName);
                    Assert.That(FindChild(prefab.transform, "ProjectileSpawnPoint"), Is.Not.Null, expected.PrefabName);
                }
                else
                {
                    Assert.That(prefab.GetComponent<RangedShooterController>(), Is.Null, expected.PrefabName);
                }

                if (expected.IsBoss)
                {
                    Assert.That(HasComponentNamed(prefab, "BossPhaseController"), Is.True, expected.PrefabName);
                    Assert.That(HasComponentNamed(prefab, "BossPatternController"), Is.True, expected.PrefabName);
                    Assert.That(HasComponentNamed(prefab, "BossSlamAttack"), Is.True, expected.PrefabName);
                    Assert.That(HasComponentNamed(prefab, "BossChargeAttack"), Is.True, expected.PrefabName);
                    Assert.That(HasComponentNamed(prefab, "BossAddSpawnAction"), Is.True, expected.PrefabName);
                }
            }
        }

        [Test]
        public void CuteMonsterAnimatorControllers_MapRequiredStatesToOwnFbxClips()
        {
            foreach (var expected in ExpectedEnemies)
            {
                var prefab = LoadPrefab(expected);
                var animator = prefab.GetComponentInChildren<Animator>(true);
                Assert.That(animator, Is.Not.Null, expected.PrefabName);
                Assert.That(animator.runtimeAnimatorController, Is.Not.Null, expected.PrefabName);
                Assert.That(AssetDatabase.GetAssetPath(animator.runtimeAnimatorController), Is.EqualTo(expected.ControllerPath), expected.PrefabName);

                var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(expected.ControllerPath);
                Assert.That(controller, Is.Not.Null, expected.ControllerPath);

                var idle = AssertStateMotion(controller, IdleState, expected);
                var move = AssertStateMotion(controller, MoveState, expected);
                AssertStateMotion(controller, AttackState, expected);
                AssertStateMotion(controller, HitState, expected);
                AssertStateMotion(controller, DeathState, expected);

                if (expected.UsesFlyingLocomotion)
                {
                    Assert.That(idle.name, Does.Contain("Flying").IgnoreCase, expected.PrefabName);
                    Assert.That(move.name, Does.Contain("Flying").IgnoreCase, expected.PrefabName);
                }
            }
        }

        [Test]
        public void CuteMonsterSpawnGroupsAndTimeline_AreUsableBySurvivorSpawnDirector()
        {
            var timeline = AssetDatabase.LoadAssetAtPath<WaveTimelineConfig>(TimelinePath);
            Assert.That(timeline, Is.Not.Null);
            Assert.That(timeline.Entries.Count, Is.GreaterThanOrEqualTo(5));
            Assert.That(ContainsGroup(timeline.Entries[0], "SpawnGroup_Cute_GreenDemon"), Is.True);
            Assert.That(ContainsGroup(timeline.Entries[0], "SpawnGroup_Cute_Mushroom"), Is.True);
            Assert.That(ContainsGroup(timeline.Entries[0], "SpawnGroup_Cute_Bee"), Is.True);
            Assert.That(ContainsGroup(timeline.Entries[0], "SpawnGroup_Cute_Ghost"), Is.True);
            Assert.That(ContainsGroup(timeline.Entries[0], "SpawnGroup_Cute_Cthulhu"), Is.False);

            var referencedGroups = 0;
            for (var entryIndex = 0; entryIndex < timeline.Entries.Count; entryIndex++)
            {
                var entry = timeline.Entries[entryIndex];
                Assert.That(entry.StartTime, Is.LessThan(entry.EndTime));
                Assert.That(entry.SpawnInterval, Is.GreaterThan(0f));
                Assert.That(entry.LiveEnemyCap, Is.GreaterThan(0));
                Assert.That(entry.SpawnGroups.Count, Is.GreaterThan(0));

                for (var groupIndex = 0; groupIndex < entry.SpawnGroups.Count; groupIndex++)
                {
                    var group = entry.SpawnGroups[groupIndex];
                    Assert.That(group, Is.Not.Null, $"entry {entryIndex} group {groupIndex}");
                    Assert.That(group.HasValidEnemy, Is.True, group.name);
                    Assert.That(group.BudgetCost, Is.GreaterThan(0), group.name);
                    referencedGroups++;
                }
            }

            Assert.That(referencedGroups, Is.GreaterThanOrEqualTo(8));

            var desktopTimeline = AssetDatabase.LoadAssetAtPath<WaveTimelineConfig>(DesktopTimelinePath);
            Assert.That(desktopTimeline, Is.Not.Null);
            Assert.That(desktopTimeline.Entries.Count, Is.EqualTo(timeline.Entries.Count));
            Assert.That(ContainsGroup(desktopTimeline.Entries[0], "SpawnGroup_Cute_GreenDemon"), Is.True);
            Assert.That(ContainsGroup(desktopTimeline.Entries[0], "SpawnGroup_Cute_Mushroom"), Is.True);
            Assert.That(ContainsGroup(desktopTimeline.Entries[0], "SpawnGroup_Cute_Bee"), Is.True);
            Assert.That(ContainsGroup(desktopTimeline.Entries[0], "SpawnGroup_Cute_Ghost"), Is.True);
            Assert.That(ContainsGroup(desktopTimeline.Entries[2], "SpawnGroup_Cute_Cthulhu"), Is.True);

            var runConfig = AssetDatabase.LoadAssetAtPath<RunConfig>(RunConfigPath);
            Assert.That(runConfig, Is.Not.Null);
            Assert.That(runConfig.WaveTimeline, Is.EqualTo(timeline));
            Assert.That(runConfig.BossSpawnGroup, Is.Not.Null);
            Assert.That(runConfig.BossSpawnGroup.HasValidEnemy, Is.True);
        }

        private static EnemyConfig LoadConfig(ExpectedEnemy expected)
        {
            return AssetDatabase.LoadAssetAtPath<EnemyConfig>($"{ConfigRoot}/{expected.ConfigName}.asset");
        }

        private static GameObject LoadPrefab(ExpectedEnemy expected)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/{expected.PrefabName}.prefab");
        }

        private static void AssertEnemyLayer(GameObject prefab)
        {
            var enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer < 0)
            {
                return;
            }

            Assert.That(prefab.layer, Is.EqualTo(enemyLayer), prefab.name);
            var colliders = prefab.GetComponentsInChildren<Collider>(true);
            for (var i = 0; i < colliders.Length; i++)
            {
                Assert.That(colliders[i].gameObject.layer, Is.EqualTo(enemyLayer), $"{prefab.name}/{colliders[i].name}");
            }
        }

        private static AnimationClip AssertStateMotion(AnimatorController controller, string stateName, ExpectedEnemy expected)
        {
            var state = FindState(controller, stateName);
            Assert.That(state, Is.Not.Null, $"{expected.PrefabName} missing state {stateName}");
            Assert.That(state.motion, Is.Not.Null, $"{expected.PrefabName} state {stateName}");
            Assert.That(AssetDatabase.GetAssetPath(state.motion), Is.EqualTo(expected.FbxPath), $"{expected.PrefabName} state {stateName}");
            return (AnimationClip)state.motion;
        }

        private static AnimatorState FindState(AnimatorController controller, string stateName)
        {
            var states = controller.layers[0].stateMachine.states;
            for (var i = 0; i < states.Length; i++)
            {
                if (states[i].state != null && states[i].state.name == stateName)
                {
                    return states[i].state;
                }
            }

            return null;
        }

        private static bool ContainsGroup(WaveTimelineConfig.WaveTimelineEntry entry, string groupName)
        {
            for (var i = 0; i < entry.SpawnGroups.Count; i++)
            {
                if (entry.SpawnGroups[i] != null && entry.SpawnGroups[i].name == groupName)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool GetBool(Component component, string propertyName, bool fallback)
        {
            var property = new SerializedObject(component).FindProperty(propertyName);
            return property != null ? property.boolValue : fallback;
        }

        private static void AssertCombatHurtbox(GameObject prefab, ExpectedEnemy expected)
        {
            var instance = Object.Instantiate(prefab);
            try
            {
                Physics.SyncTransforms();
                var bodyCollider = instance.GetComponent<Collider>();
                var hurtbox = FindChild(instance.transform, "CombatHurtbox");
                Assert.That(hurtbox, Is.Not.Null, expected.PrefabName);

                var hurtboxCollider = hurtbox.GetComponent<CapsuleCollider>();
                Assert.That(hurtboxCollider, Is.Not.Null, expected.PrefabName);
                Assert.That(hurtboxCollider.enabled, Is.True, expected.PrefabName);
                Assert.That(hurtboxCollider.isTrigger, Is.True, expected.PrefabName);
                Assert.That(hurtboxCollider.radius, Is.GreaterThan(0.1f), expected.PrefabName);
                Assert.That(hurtboxCollider.height, Is.GreaterThanOrEqualTo(hurtboxCollider.radius * 2f), expected.PrefabName);

                if (bodyCollider != null)
                {
                    Assert.That(
                        hurtboxCollider.bounds.max.y,
                        Is.GreaterThan(bodyCollider.bounds.max.y + 0.35f),
                        $"{expected.PrefabName} combat hurtbox must extend above the physical capsule so projectiles and area skills match the visible enemy.");
                }

                if (expected.UsesFlyingLocomotion && bodyCollider != null)
                {
                    Assert.That(
                        hurtboxCollider.bounds.max.y,
                        Is.GreaterThan(bodyCollider.bounds.max.y + 0.2f),
                        $"{expected.PrefabName} flying visual needs a combat hurtbox above the physical capsule.");
                }
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        private static void AssertProjectileHitsCombatHurtbox(string prefabName)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/{prefabName}.prefab");
            Assert.That(prefab, Is.Not.Null, prefabName);

            var enemy = Object.Instantiate(prefab, new Vector3(0f, 0f, 5f), Quaternion.identity);
            var projectile = new GameObject($"{prefabName}_ProjectileProbe");

            try
            {
                Physics.SyncTransforms();
                var bodyCollider = enemy.GetComponent<Collider>();
                var hurtbox = FindChild(enemy.transform, "CombatHurtbox")?.GetComponent<CapsuleCollider>();
                var health = enemy.GetComponent<EnemyHealth>();
                var hitCount = 0;

                Assert.That(bodyCollider, Is.Not.Null, prefabName);
                Assert.That(hurtbox, Is.Not.Null, prefabName);
                Assert.That(health, Is.Not.Null, prefabName);

                health.ResetHealth();
                health.OnDamaged += _ => hitCount++;

                var projectileY = Mathf.Min(hurtbox.bounds.max.y - 0.06f, bodyCollider.bounds.max.y + 0.35f);
                Assert.That(projectileY, Is.GreaterThan(bodyCollider.bounds.max.y + 0.05f), prefabName);

                projectile.transform.position = new Vector3(0f, projectileY, 0f);
                var projectileCollider = projectile.AddComponent<SphereCollider>();
                projectileCollider.radius = 0.02f;
                var controller = projectile.AddComponent<ProjectileController>();
                SetDeactivateInsteadOfDestroy(controller);

                Physics.SyncTransforms();

                controller.Initialize(new HitContext(null, null, 7f), Vector3.forward, 360f, 3f, null);
                InvokeAdvanceTransformMotion(controller, projectile.transform.position, 1f / 60f);

                Assert.That(hitCount, Is.EqualTo(1), prefabName);
                Assert.That(projectile.activeSelf, Is.False, prefabName);
            }
            finally
            {
                Object.DestroyImmediate(projectile);
                Object.DestroyImmediate(enemy);
            }
        }

        private static void SetDeactivateInsteadOfDestroy(MonoBehaviour controller)
        {
            controller.GetType()
                .GetField("deactivateInsteadOfDestroy", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(controller, true);
        }

        private static void InvokeAdvanceTransformMotion(MonoBehaviour controller, Vector3 currentPosition, float deltaTime)
        {
            controller.GetType()
                .GetMethod("TryAdvanceTransformMotion", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(controller, new object[] { currentPosition, deltaTime });
        }

        private static bool HasComponentNamed(GameObject prefab, string componentName)
        {
            var components = prefab.GetComponentsInChildren<MonoBehaviour>(true);
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] != null && components[i].GetType().Name == componentName)
                {
                    return true;
                }
            }

            return false;
        }

        private static Transform FindChild(Transform root, string name)
        {
            var children = root.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < children.Length; i++)
            {
                if (children[i].name == name)
                {
                    return children[i];
                }
            }

            return null;
        }

        private sealed class ExpectedEnemy
        {
            public ExpectedEnemy(
                string modelName,
                string configName,
                string prefabName,
                EnemyArchetype archetype,
                bool contactDamageEnabled,
                bool usesContactWindup = false,
                bool isRanged = false,
                bool isBoss = false,
                bool usesFlyingLocomotion = false)
            {
                ModelName = modelName;
                ConfigName = configName;
                PrefabName = prefabName;
                Archetype = archetype;
                ContactDamageEnabled = contactDamageEnabled;
                UsesContactWindup = usesContactWindup;
                IsRanged = isRanged;
                IsBoss = isBoss;
                UsesFlyingLocomotion = usesFlyingLocomotion;
            }

            public string ModelName { get; }
            public string ConfigName { get; }
            public string PrefabName { get; }
            public EnemyArchetype Archetype { get; }
            public bool ContactDamageEnabled { get; }
            public bool UsesContactWindup { get; }
            public bool IsRanged { get; }
            public bool IsBoss { get; }
            public bool UsesFlyingLocomotion { get; }
            public string ControllerPath => $"{ControllerRoot}/AC_{PrefabName}.controller";
            public string FbxPath => $"{FbxRoot}/{ModelName}.fbx";
        }
    }
}
