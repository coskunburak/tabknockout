#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TapKnockout.Boss;
using TapKnockout.Characters;
using TapKnockout.Enemy;
using TapKnockout.Feedback;
using TapKnockout.Projectile;
using TapKnockout.Survivor;
using TapKnockout.VFX;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TapKnockout.Editor.Tools
{
    public static class CuteMonsterEnemyContentBuilder
    {
        public const string IntegrationNotesPath = "Assets/_Project/Docs/44_CUTE_ANIMATED_MONSTERS_ENEMY_INTEGRATION_NOTES.md";
        public const string ReportPath = "Assets/_Project/Docs/CuteMonsterEnemyContentBuilderReport.md";
        public const string ConfigRoot = "Assets/_Project/ScriptableObjects/Enemies/CuteMonsters";
        public const string BossConfigRoot = "Assets/_Project/ScriptableObjects/Bosses/CuteMonsters";
        public const string SpawnGroupRoot = "Assets/_Project/ScriptableObjects/Waves/CuteMonsters";
        public const string PrefabRoot = "Assets/_Project/Prefabs/Enemies/CuteMonsters";
        public const string ControllerRoot = "Assets/_Project/Animation/Controllers/CuteMonsters";
        public const string ProjectilePrefabPath = "Assets/_Project/Prefabs/Projectiles/PF_EnemyProjectile_CuteMonster.prefab";
        public const string TimelinePath = SpawnGroupRoot + "/WaveTimeline_CuteMonsters_Test.asset";
        public const string DesktopPrototypeTimelinePath = "Assets/_Project/ScriptableObjects/Waves/WaveTimeline_DesktopSurvivorPrototype.asset";
        public const string BossPatternPath = BossConfigRoot + "/BossPattern_YellowDragon.asset";
        public const string BossConfigPath = BossConfigRoot + "/BossConfig_YellowDragon.asset";

        private const string FbxRoot = "Assets/Assets/game asset packs/Cute Animated Monsters - Aug 2020/FBX";
        private const string TextureRoot = "Assets/Assets/game asset packs/Cute Animated Monsters - Aug 2020/Textures";
        private const string ObjRoot = "Assets/Assets/game asset packs/Cute Animated Monsters - Aug 2020/OBJ";
        private const string RunConfigPath = "Assets/_Project/ScriptableObjects/Runs/RunConfig_DesktopSurvivorPrototype.asset";
        private const string ForestRunConfigPath = "Assets/_Project/ScriptableObjects/Runs/RunConfig_ForestSurvivorArena.asset";
        private const string ExistingGeneratedGreenDemonPrefabPath = "Assets/_Project/Prefabs/Enemies/Generated/PF_Enemy_BasicMelee_GreenDemon_Generated.prefab";
        private const string CombatHurtboxName = "CombatHurtbox";
        private const float CombatHurtboxVerticalPadding = 0.24f;
        private const float CombatHurtboxHorizontalPadding = 0.12f;

        private static readonly string[] ControllerParameters =
        {
            CharacterAnimationDriver.MoveSpeedParameter,
            CharacterAnimationDriver.IsMovingParameter,
            CharacterAnimationDriver.IsDashingParameter,
            CharacterAnimationDriver.IsAttackingParameter,
            CharacterAnimationDriver.AttackTrigger,
            CharacterAnimationDriver.SkillCastTrigger,
            CharacterAnimationDriver.DashTrigger,
            CharacterAnimationDriver.HitTrigger,
            CharacterAnimationDriver.DeathTrigger
        };

        private static readonly MonsterSpec[] MonsterSpecs =
        {
            new MonsterSpec
            {
                ModelName = "GreenDemon",
                DisplayName = "Green Demon",
                EnemyId = "cute_green_demon",
                ConfigName = "EnemyConfig_GreenDemon",
                PrefabName = "PF_Enemy_GreenDemon",
                SpawnGroupName = "SpawnGroup_Cute_GreenDemon",
                Archetype = EnemyArchetype.MeleeChaser,
                Rank = EnemyRank.Normal,
                SuggestedArchetype = "Basic melee / reference chaser",
                IntegrationStatus = "ready; reference contract preserved",
                MaxHealth = 42f,
                MoveSpeed = 2.45f,
                Acceleration = 20f,
                RotationSpeed = 720f,
                StoppingDistance = 1.05f,
                ContactDamage = 8f,
                AttackRange = 1.2f,
                AttackCooldown = 0.9f,
                AttackWindup = 0.12f,
                XpReward = 2,
                KnockbackResistance = 0.18f,
                BudgetCost = 2,
                Weight = 1.1f,
                MinCount = 1,
                MaxCount = 2,
                SpawnBurstCount = 1,
                VisualScale = 0.95f,
                ColliderRadius = 0.5f,
                ColliderHeight = 1.75f,
                ColliderCenterY = 0.88f,
                SelectedFirstPass = true
            },
            new MonsterSpec
            {
                ModelName = "Demon",
                DisplayName = "Demon",
                EnemyId = "cute_demon",
                ConfigName = "EnemyConfig_Demon",
                PrefabName = "PF_Enemy_Demon",
                SpawnGroupName = "SpawnGroup_Cute_Demon",
                Archetype = EnemyArchetype.MeleeChaser,
                Rank = EnemyRank.Normal,
                SuggestedArchetype = "Basic melee / chaser",
                IntegrationStatus = "ready",
                MaxHealth = 48f,
                MoveSpeed = 2.35f,
                Acceleration = 19f,
                RotationSpeed = 720f,
                StoppingDistance = 1.05f,
                ContactDamage = 9f,
                AttackRange = 1.2f,
                AttackCooldown = 0.95f,
                AttackWindup = 0.14f,
                XpReward = 2,
                KnockbackResistance = 0.22f,
                BudgetCost = 2,
                Weight = 1.15f,
                MinCount = 1,
                MaxCount = 2,
                SpawnBurstCount = 1,
                VisualScale = 0.95f,
                ColliderRadius = 0.52f,
                ColliderHeight = 1.8f,
                ColliderCenterY = 0.9f,
                SelectedFirstPass = true
            },
            new MonsterSpec
            {
                ModelName = "Bat",
                DisplayName = "Bat",
                EnemyId = "cute_bat",
                ConfigName = "EnemyConfig_Bat",
                PrefabName = "PF_Enemy_Bat",
                SpawnGroupName = "SpawnGroup_Cute_Bat",
                Archetype = EnemyArchetype.FastCharger,
                Rank = EnemyRank.Normal,
                SuggestedArchetype = "Fast swarm / flyer",
                IntegrationStatus = "ready; ground movement with flying visual",
                MaxHealth = 20f,
                MoveSpeed = 3.55f,
                Acceleration = 28f,
                RotationSpeed = 900f,
                StoppingDistance = 0.65f,
                ContactDamage = 6f,
                AttackRange = 0.9f,
                AttackCooldown = 0.75f,
                AttackWindup = 0.05f,
                XpReward = 1,
                KnockbackResistance = 0.05f,
                BudgetCost = 1,
                Weight = 1.25f,
                MinCount = 2,
                MaxCount = 4,
                SpawnBurstCount = 2,
                VisualScale = 0.75f,
                ColliderRadius = 0.35f,
                ColliderHeight = 0.9f,
                ColliderCenterY = 0.48f,
                CombatHurtboxBodyTopPadding = 0.75f,
                SelectedFirstPass = true
            },
            new MonsterSpec
            {
                ModelName = "Bee",
                DisplayName = "Bee",
                EnemyId = "cute_bee",
                ConfigName = "EnemyConfig_Bee",
                PrefabName = "PF_Enemy_Bee",
                SpawnGroupName = "SpawnGroup_Cute_Bee",
                Archetype = EnemyArchetype.FastCharger,
                Rank = EnemyRank.Normal,
                SuggestedArchetype = "Fast swarm / flyer",
                IntegrationStatus = "ready; ground movement with flying visual",
                MaxHealth = 22f,
                MoveSpeed = 3.35f,
                Acceleration = 26f,
                RotationSpeed = 900f,
                StoppingDistance = 0.7f,
                ContactDamage = 6f,
                AttackRange = 0.95f,
                AttackCooldown = 0.72f,
                AttackWindup = 0.05f,
                XpReward = 1,
                KnockbackResistance = 0.06f,
                BudgetCost = 1,
                Weight = 1.1f,
                MinCount = 2,
                MaxCount = 3,
                SpawnBurstCount = 2,
                VisualScale = 0.78f,
                ColliderRadius = 0.36f,
                ColliderHeight = 0.95f,
                ColliderCenterY = 0.5f,
                CombatHurtboxBodyTopPadding = 0.75f,
                SelectedFirstPass = true
            },
            new MonsterSpec
            {
                ModelName = "Mushroom",
                DisplayName = "Mushroom",
                EnemyId = "cute_mushroom",
                ConfigName = "EnemyConfig_Mushroom",
                PrefabName = "PF_Enemy_Mushroom",
                SpawnGroupName = "SpawnGroup_Cute_Mushroom",
                Archetype = EnemyArchetype.MeleeChaser,
                Rank = EnemyRank.Normal,
                SuggestedArchetype = "Basic melee / low-profile chaser",
                IntegrationStatus = "ready",
                MaxHealth = 38f,
                MoveSpeed = 2.25f,
                Acceleration = 18f,
                RotationSpeed = 700f,
                StoppingDistance = 1f,
                ContactDamage = 8f,
                AttackRange = 1.1f,
                AttackCooldown = 0.9f,
                AttackWindup = 0.12f,
                XpReward = 2,
                KnockbackResistance = 0.16f,
                BudgetCost = 2,
                Weight = 1f,
                MinCount = 1,
                MaxCount = 3,
                SpawnBurstCount = 1,
                VisualScale = 0.9f,
                ColliderRadius = 0.48f,
                ColliderHeight = 1.35f,
                ColliderCenterY = 0.68f,
                SelectedFirstPass = true
            },
            new MonsterSpec
            {
                ModelName = "Cyclops",
                DisplayName = "Cyclops",
                EnemyId = "cute_cyclops",
                ConfigName = "EnemyConfig_Cyclops",
                PrefabName = "PF_Enemy_Cyclops",
                SpawnGroupName = "SpawnGroup_Cute_Cyclops",
                Archetype = EnemyArchetype.ShieldEnemy,
                Rank = EnemyRank.Elite,
                SuggestedArchetype = "Tank / bruiser",
                IntegrationStatus = "ready; elite-capable bruiser",
                MaxHealth = 145f,
                MoveSpeed = 1.65f,
                Acceleration = 14f,
                RotationSpeed = 540f,
                StoppingDistance = 1.25f,
                ContactDamage = 15f,
                AttackRange = 1.45f,
                AttackCooldown = 1.35f,
                AttackWindup = 0.22f,
                XpReward = 7,
                KnockbackResistance = 0.58f,
                BudgetCost = 6,
                Weight = 0.42f,
                MinCount = 1,
                MaxCount = 1,
                SpawnBurstCount = 1,
                VisualScale = 1.2f,
                ColliderRadius = 0.68f,
                ColliderHeight = 2.25f,
                ColliderCenterY = 1.12f,
                SelectedFirstPass = true,
                Elite = true
            },
            new MonsterSpec
            {
                ModelName = "Yeti",
                DisplayName = "Yeti",
                EnemyId = "cute_yeti",
                ConfigName = "EnemyConfig_Yeti",
                PrefabName = "PF_Enemy_Yeti",
                SpawnGroupName = "SpawnGroup_Cute_Yeti",
                Archetype = EnemyArchetype.ShieldEnemy,
                Rank = EnemyRank.Elite,
                SuggestedArchetype = "Tank / bruiser",
                IntegrationStatus = "ready; elite-capable bruiser",
                MaxHealth = 170f,
                MoveSpeed = 1.55f,
                Acceleration = 13f,
                RotationSpeed = 520f,
                StoppingDistance = 1.3f,
                ContactDamage = 17f,
                AttackRange = 1.55f,
                AttackCooldown = 1.45f,
                AttackWindup = 0.25f,
                XpReward = 8,
                KnockbackResistance = 0.64f,
                BudgetCost = 7,
                Weight = 0.36f,
                MinCount = 1,
                MaxCount = 1,
                SpawnBurstCount = 1,
                VisualScale = 1.25f,
                ColliderRadius = 0.72f,
                ColliderHeight = 2.35f,
                ColliderCenterY = 1.18f,
                SelectedFirstPass = true,
                Elite = true
            },
            new MonsterSpec
            {
                ModelName = "Cactus",
                DisplayName = "Cactus",
                EnemyId = "cute_cactus",
                ConfigName = "EnemyConfig_Cactus",
                PrefabName = "PF_Enemy_Cactus",
                SpawnGroupName = "SpawnGroup_Cute_Cactus",
                Archetype = EnemyArchetype.ShieldEnemy,
                Rank = EnemyRank.Normal,
                SuggestedArchetype = "Tank / bruiser",
                IntegrationStatus = "ready",
                MaxHealth = 105f,
                MoveSpeed = 1.75f,
                Acceleration = 14f,
                RotationSpeed = 560f,
                StoppingDistance = 1.2f,
                ContactDamage = 13f,
                AttackRange = 1.35f,
                AttackCooldown = 1.25f,
                AttackWindup = 0.2f,
                XpReward = 5,
                KnockbackResistance = 0.5f,
                BudgetCost = 5,
                Weight = 0.55f,
                MinCount = 1,
                MaxCount = 2,
                SpawnBurstCount = 1,
                VisualScale = 1.05f,
                ColliderRadius = 0.62f,
                ColliderHeight = 2.05f,
                ColliderCenterY = 1.02f,
                SelectedFirstPass = true
            },
            new MonsterSpec
            {
                ModelName = "Ghost",
                DisplayName = "Ghost",
                EnemyId = "cute_ghost",
                ConfigName = "EnemyConfig_Ghost",
                PrefabName = "PF_Enemy_Ghost",
                SpawnGroupName = "SpawnGroup_Cute_Ghost",
                Archetype = EnemyArchetype.FastCharger,
                Rank = EnemyRank.Normal,
                SuggestedArchetype = "Fast swarm / flyer",
                IntegrationStatus = "ready; ground movement with flying visual",
                MaxHealth = 26f,
                MoveSpeed = 3.05f,
                Acceleration = 24f,
                RotationSpeed = 820f,
                StoppingDistance = 0.85f,
                ContactDamage = 7f,
                AttackRange = 1f,
                AttackCooldown = 0.8f,
                AttackWindup = 0.08f,
                XpReward = 1,
                KnockbackResistance = 0.12f,
                BudgetCost = 1,
                Weight = 0.92f,
                MinCount = 1,
                MaxCount = 3,
                SpawnBurstCount = 1,
                VisualScale = 0.9f,
                ColliderRadius = 0.42f,
                ColliderHeight = 1.45f,
                ColliderCenterY = 0.75f,
                CombatHurtboxBodyTopPadding = 0.85f,
                SelectedFirstPass = true
            },
            new MonsterSpec
            {
                ModelName = "Cthulhu",
                DisplayName = "Cthulhu",
                EnemyId = "cute_cthulhu",
                ConfigName = "EnemyConfig_Cthulhu",
                PrefabName = "PF_Enemy_Cthulhu",
                SpawnGroupName = "SpawnGroup_Cute_Cthulhu",
                Archetype = EnemyArchetype.RangedShooter,
                Rank = EnemyRank.Elite,
                SuggestedArchetype = "Ranged / special elite",
                IntegrationStatus = "ready; uses existing ranged projectile support",
                MaxHealth = 125f,
                MoveSpeed = 1.85f,
                Acceleration = 15f,
                RotationSpeed = 600f,
                StoppingDistance = 5.2f,
                ContactDamage = 12f,
                AttackRange = 7.5f,
                AttackCooldown = 1.55f,
                AttackWindup = 0.42f,
                ProjectileSpeed = 8f,
                XpReward = 8,
                KnockbackResistance = 0.45f,
                BudgetCost = 7,
                Weight = 0.28f,
                MinCount = 1,
                MaxCount = 1,
                SpawnBurstCount = 1,
                VisualScale = 1.1f,
                ColliderRadius = 0.62f,
                ColliderHeight = 2.05f,
                ColliderCenterY = 1.02f,
                CombatHurtboxBodyTopPadding = 0.75f,
                RequiresProjectileSpawnPoint = true,
                AddRangedShooterController = true,
                SelectedFirstPass = true,
                Elite = true
            },
            new MonsterSpec
            {
                ModelName = "YellowDragon",
                DisplayName = "Yellow Dragon",
                EnemyId = "cute_yellow_dragon_boss",
                ConfigName = "EnemyConfig_YellowDragon_Boss",
                PrefabName = "PF_Boss_YellowDragon",
                SpawnGroupName = "SpawnGroup_Cute_YellowDragon_Boss",
                Archetype = EnemyArchetype.Boss,
                Rank = EnemyRank.Boss,
                SuggestedArchetype = "Boss",
                IntegrationStatus = "ready; boss candidate wired to existing boss phase/pattern components",
                MaxHealth = 850f,
                MoveSpeed = 1.35f,
                Acceleration = 11f,
                RotationSpeed = 420f,
                StoppingDistance = 1.8f,
                ContactDamage = 24f,
                AttackRange = 2f,
                AttackCooldown = 1.7f,
                AttackWindup = 0.4f,
                XpReward = 35,
                KnockbackResistance = 0.82f,
                BudgetCost = 18,
                Weight = 1f,
                MinCount = 1,
                MaxCount = 1,
                SpawnBurstCount = 1,
                VisualScale = 1.85f,
                ColliderRadius = 1.05f,
                ColliderHeight = 2.75f,
                ColliderCenterY = 1.38f,
                CombatHurtboxBodyTopPadding = 0.85f,
                Boss = true,
                SelectedFirstPass = true
            },
            Deferred("Alien", "Alien", "Ranged / special", "deferred; queued for a later ranged projectile-variant pass"),
            Deferred("Alien_Tall", "Alien Tall", "Ranged / special", "deferred; queued for tall ranged/special tuning after Cthulhu validates projectile pacing"),
            Deferred("Crab", "Crab", "Ranged / special", "deferred; lower-profile special enemy needs collider/aim readability review"),
            Deferred("Chicken", "Chicken", "Basic melee", "deferred; overlaps current basic melee role and should be used after core roster balance"),
            Deferred("Deer", "Deer", "Basic melee / special", "deferred; neutral silhouette needs clearer hostile read before production waves"),
            Deferred("Panda", "Panda", "Basic melee", "deferred; overlaps basic melee roster and needs identity beyond a duplicate chaser"),
            Deferred("Penguin", "Penguin", "Basic melee", "deferred; overlaps basic melee roster and needs identity beyond a duplicate chaser"),
            Deferred("Pig", "Pig", "Basic melee", "deferred; overlaps basic melee roster and needs identity beyond a duplicate chaser"),
            Deferred("Skull", "Skull", "Ranged / special", "deferred; good projectile caster candidate after ranged projectile art/tuning"),
            Deferred("Tree", "Tree", "Tank / boss candidate", "deferred; large static silhouette needs scale/collider review before tank or boss use")
        };

        [MenuItem("Tools/Tap Knockout/Enemies/Build Cute Monster Enemy Content")]
        public static void BuildCuteMonsterEnemyContentMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Cute Monster Content", "Exit Play Mode before building cute monster enemy content.", "OK");
                return;
            }

            var summary = BuildCuteMonsterEnemyContent(wirePrototypeRun: true, logToConsole: true);
            EditorUtility.DisplayDialog(
                "Cute Monster Content",
                $"Configs: {summary.ConfigCount}\nPrefabs: {summary.PrefabCount}\nSpawn groups: {summary.SpawnGroupCount}\nWarnings: {summary.WarningCount}\n\nSee {IntegrationNotesPath}",
                "OK");
        }

        public static void BuildCuteMonsterEnemyContentBatch()
        {
            BuildCuteMonsterEnemyContent(wirePrototypeRun: true, logToConsole: true);
        }

        [MenuItem("Tools/Tap Knockout/Enemies/Repair Cute Monster Animator Clip References")]
        public static void RepairCuteMonsterAnimatorClipReferencesMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Cute Monster Content", "Exit Play Mode before repairing cute monster animator clips.", "OK");
                return;
            }

            var repairedCount = RepairCuteMonsterAnimatorClipReferences(logToConsole: true);
            EditorUtility.DisplayDialog(
                "Cute Monster Content",
                $"Animator controllers repaired: {repairedCount}",
                "OK");
        }

        public static void RepairCuteMonsterAnimatorClipReferencesBatch()
        {
            RepairCuteMonsterAnimatorClipReferences(logToConsole: true);
        }

        public static int RepairCuteMonsterAnimatorClipReferences(bool logToConsole = false)
        {
            var repairedCount = 0;
            foreach (var spec in MonsterSpecs)
            {
                if (!spec.SelectedFirstPass || string.IsNullOrEmpty(spec.ControllerPath))
                {
                    continue;
                }

                if (RepairAnimatorControllerClipReferences(spec))
                {
                    repairedCount++;
                }
            }

            if (repairedCount > 0)
            {
                AssetDatabase.SaveAssets();
            }

            if (logToConsole)
            {
                Debug.Log($"{nameof(CuteMonsterEnemyContentBuilder)} repaired {repairedCount} cute monster animator controller clip reference set(s).");
            }

            return repairedCount;
        }

        public static BuildSummary BuildCuteMonsterEnemyContent(bool wirePrototypeRun = true, bool logToConsole = false)
        {
            EnsureFolders();

            var report = new StringBuilder();
            report.AppendLine("# Cute Monster Enemy Content Builder Report");
            report.AppendLine();
            report.AppendLine("Generated assets are project-owned content under `Assets/_Project`. Source FBX, texture, OBJ, and `.meta` assets are not edited by this builder.");
            report.AppendLine();

            var summary = new BuildSummary();
            var projectilePrefab = CreateOrRepairEnemyProjectilePrefab(summary, report);
            var generated = new Dictionary<string, GeneratedMonsterContent>(StringComparer.Ordinal);

            foreach (var spec in MonsterSpecs)
            {
                if (!spec.SelectedFirstPass)
                {
                    continue;
                }

                var content = BuildMonsterContent(spec, projectilePrefab, summary, report);
                if (content != null)
                {
                    generated[spec.ModelName] = content;
                }
            }

            var bossContent = generated.TryGetValue("YellowDragon", out var yellowDragon) ? yellowDragon : null;
            var greenDemonContent = generated.TryGetValue("GreenDemon", out var greenDemon) ? greenDemon : null;
            var bossPattern = CreateOrUpdateBossPattern(summary, report);
            var bossConfig = CreateOrUpdateBossConfig(bossContent, greenDemonContent, bossPattern, summary, report);
            WireBossPrefab(bossContent, bossConfig, summary, report);
            var spawnGroups = CreateOrUpdateSpawnGroups(generated, summary, report);
            var timeline = CreateOrUpdateWaveTimeline(spawnGroups, summary, report);
            var desktopPrototypeTimeline = CreateOrUpdateWaveTimeline(
                spawnGroups,
                DesktopPrototypeTimelinePath,
                "wave_timeline_prototype_01",
                summary,
                report);

            if (wirePrototypeRun)
            {
                WirePrototypeRunConfig(timeline, spawnGroups.TryGetValue("YellowDragon", out var bossGroup) ? bossGroup : null, summary, report);
            }

            WriteIntegrationNotes(generated, spawnGroups, timeline, desktopPrototypeTimeline, bossConfig, summary);
            WriteTextAsset(ReportPath, report.ToString());

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (logToConsole)
            {
                Debug.Log(report.ToString());
                Debug.Log($"{nameof(CuteMonsterEnemyContentBuilder)} complete. Configs: {summary.ConfigCount}, prefabs: {summary.PrefabCount}, spawn groups: {summary.SpawnGroupCount}, warnings: {summary.WarningCount}.");
            }

            return summary;
        }

        public static void ValidateCuteMonsterContent(TapKnockout.Editor.SurvivorValidationReport report)
        {
            if (report == null)
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder(FbxRoot))
            {
                report.Error($"Cute monster FBX root is missing: {FbxRoot}");
                return;
            }

            foreach (var spec in MonsterSpecs)
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(spec.FbxPath);
                if (model == null)
                {
                    report.Warn($"Cute monster source model is missing: {spec.FbxPath}");
                }

                if (!spec.SelectedFirstPass)
                {
                    continue;
                }

                var config = AssetDatabase.LoadAssetAtPath<EnemyConfig>(spec.ConfigPath);
                if (config == null)
                {
                    report.Error($"Cute monster config missing: {spec.ConfigPath}. Run Tools/Tap Knockout/Enemies/Build Cute Monster Enemy Content.");
                }

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(spec.PrefabPath);
                if (prefab == null)
                {
                    report.Error($"Cute monster prefab missing: {spec.PrefabPath}. Run Tools/Tap Knockout/Enemies/Build Cute Monster Enemy Content.");
                    continue;
                }

                ValidateGeneratedPrefabContract(report, spec, prefab);

                var group = AssetDatabase.LoadAssetAtPath<SpawnGroupConfig>(spec.SpawnGroupPath);
                if (group == null)
                {
                    report.Error($"Cute monster spawn group missing: {spec.SpawnGroupPath}.");
                }
                else
                {
                    if (group.EnemyConfig == null || group.EnemyPrefab == null)
                    {
                        report.Error($"{group.name} must have EnemyConfig and EnemyPrefab assigned.");
                    }

                    if (group.EnemyConfig != config)
                    {
                        report.Warn($"{group.name}.enemyConfig does not point at {spec.ConfigName}.");
                    }

                    if (group.EnemyPrefab != prefab)
                    {
                        report.Warn($"{group.name}.enemyPrefab does not point at {spec.PrefabName}.");
                    }
                }
            }

            if (AssetDatabase.LoadAssetAtPath<WaveTimelineConfig>(TimelinePath) == null)
            {
                report.Error($"Cute monster wave timeline missing: {TimelinePath}.");
            }

            if (AssetDatabase.LoadAssetAtPath<BossConfig>(BossConfigPath) == null)
            {
                report.Warn($"Cute monster boss config missing: {BossConfigPath}.");
            }

            var runConfig = AssetDatabase.LoadAssetAtPath<RunConfig>(RunConfigPath);
            if (runConfig != null && runConfig.WaveTimeline != AssetDatabase.LoadAssetAtPath<WaveTimelineConfig>(TimelinePath))
            {
                report.Warn($"{runConfig.name}.waveTimeline is not wired to WaveTimeline_CuteMonsters_Test. Run the cute monster content builder if this roster should drive the prototype run.");
            }
        }

        private static GeneratedMonsterContent BuildMonsterContent(
            MonsterSpec spec,
            GameObject projectilePrefab,
            BuildSummary summary,
            StringBuilder report)
        {
            report.AppendLine($"## {spec.DisplayName}");

            var visualAsset = AssetDatabase.LoadAssetAtPath<GameObject>(spec.FbxPath);
            if (visualAsset == null)
            {
                report.AppendLine($"- Skipped: missing model `{spec.FbxPath}`.");
                summary.WarningCount++;
                return null;
            }

            var config = CreateOrUpdateEnemyConfig(spec, projectilePrefab, summary, report);
            var controller = CreateOrUpdateAnimatorController(spec, out var animationSummary, summary, report);
            var prefab = CreateOrUpdateEnemyPrefab(spec, visualAsset, config, controller, summary, report);
            report.AppendLine($"- Attack archetype: {spec.AttackArchetypeLabel}");
            report.AppendLine($"- VFX/feedback: {spec.VfxFeedbackLabel}");
            report.AppendLine($"- Wave role: {spec.WaveRole}");
            report.AppendLine($"- Animation mapping: {animationSummary}");
            report.AppendLine();
            return new GeneratedMonsterContent(spec, config, prefab, controller, animationSummary);
        }

        private static EnemyConfig CreateOrUpdateEnemyConfig(
            MonsterSpec spec,
            GameObject projectilePrefab,
            BuildSummary summary,
            StringBuilder report)
        {
            var config = AssetDatabase.LoadAssetAtPath<EnemyConfig>(spec.ConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<EnemyConfig>();
                AssetDatabase.CreateAsset(config, spec.ConfigPath);
                summary.ConfigCount++;
            }

            var serialized = new SerializedObject(config);
            SetString(serialized, "enemyId", spec.EnemyId);
            SetString(serialized, "displayName", spec.DisplayName);
            SetEnum(serialized, "archetype", (int)spec.Archetype);
            SetEnum(serialized, "rank", (int)spec.Rank);
            SetFloat(serialized, "maxHealth", spec.MaxHealth);
            SetFloat(serialized, "deathDelay", spec.Boss ? 0.75f : spec.Elite ? 0.45f : 0.28f);
            SetFloat(serialized, "moveSpeed", spec.MoveSpeed);
            SetFloat(serialized, "acceleration", spec.Acceleration);
            SetFloat(serialized, "rotationSpeed", spec.RotationSpeed);
            SetFloat(serialized, "stoppingDistance", spec.StoppingDistance);
            SetFloat(serialized, "contactDamage", spec.ContactDamage);
            SetFloat(serialized, "attackRange", spec.AttackRange);
            SetFloat(serialized, "attackCooldown", spec.AttackCooldown);
            SetFloat(serialized, "attackWindup", spec.AttackWindup);
            SetFloat(serialized, "projectileSpeed", spec.ProjectileSpeed);
            SetInt(serialized, "projectileCount", spec.RequiresProjectileSpawnPoint ? 1 : 0);
            SetFloat(serialized, "explosionRadius", spec.ExplosionRadius > 0f ? spec.ExplosionRadius : 1.5f);
            SetFloat(serialized, "knockbackResistance", spec.KnockbackResistance);
            SetBool(serialized, "canBeKnockedBack", spec.KnockbackResistance < 0.8f);
            SetBool(serialized, "canBeInterrupted", !spec.Boss);
            SetInt(serialized, "coinReward", spec.Boss ? 25 : spec.Elite ? 6 : 1);
            SetInt(serialized, "xpReward", spec.XpReward);
            SetEnum(serialized, "spawnVfx", spec.Elite || spec.Boss ? (int)VFXEventType.EliteSpawn : (int)VFXEventType.EnemySpawn);
            SetEnum(serialized, "attackVfx", spec.Boss ? (int)VFXEventType.BossHeavyAttackImpact : (int)VFXEventType.EnemyAttackRelease);
            SetEnum(serialized, "deathVfx", spec.Boss ? (int)VFXEventType.BossDeath : spec.Elite ? (int)VFXEventType.EliteDeath : (int)VFXEventType.EnemyDeath);

            if (spec.RequiresProjectileSpawnPoint && projectilePrefab != null)
            {
                SetObject(serialized, "projectilePrefab", projectilePrefab);
            }
            else
            {
                SetObject(serialized, "projectilePrefab", null);
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            report.AppendLine($"- Config: `{spec.ConfigPath}`");
            return config;
        }

        private static RuntimeAnimatorController CreateOrUpdateAnimatorController(
            MonsterSpec spec,
            out string animationSummary,
            BuildSummary summary,
            StringBuilder report)
        {
            EnsureFolder(ControllerRoot);
            var clips = FindAnimationClips(spec);
            var idle = SelectClip(clips, "idle", "default") ?? SelectClip(clips, "flying", "fly", "hover") ?? FirstClip(clips);
            var move = SelectClip(clips, "walk", "run", "move", "flying", "fly") ?? idle;
            var attack = SelectClip(clips, "attack", "bite", "slam") ?? move ?? idle;
            var hit = SelectClip(clips, "hitrecieve", "hitreceive", "recievehit", "receivehit", "hit", "damage") ?? attack ?? idle;
            var death = SelectClip(clips, "death", "die", "dead") ?? hit ?? idle;

            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(spec.ControllerPath) != null)
            {
                AssetDatabase.DeleteAsset(spec.ControllerPath);
            }

            var controller = AnimatorController.CreateAnimatorControllerAtPath(spec.ControllerPath);
            AddControllerParameters(controller);
            var stateMachine = controller.layers[0].stateMachine;
            var idleState = AddState(stateMachine, CharacterAnimationDriver.IdleState, idle, new Vector3(230f, 70f, 0f));
            var moveState = AddState(stateMachine, CharacterAnimationDriver.MoveState, move, new Vector3(520f, 70f, 0f));
            var attackState = AddState(stateMachine, CharacterAnimationDriver.AttackState, attack, new Vector3(520f, 215f, 0f));
            var hitState = AddState(stateMachine, CharacterAnimationDriver.HitState, hit, new Vector3(230f, 215f, 0f));
            var deathState = AddState(stateMachine, CharacterAnimationDriver.DeathState, death, new Vector3(0f, 215f, 0f));
            stateMachine.defaultState = idleState;

            AddBoolTransition(idleState, moveState, CharacterAnimationDriver.IsMovingParameter, true, 0.08f);
            AddBoolTransition(moveState, idleState, CharacterAnimationDriver.IsMovingParameter, false, 0.12f);
            AddAnyStateTriggerTransition(stateMachine, attackState, CharacterAnimationDriver.AttackTrigger, 0.05f);
            AddTimedTransition(attackState, idleState, 0.75f, 0.06f);
            AddAnyStateTriggerTransition(stateMachine, hitState, CharacterAnimationDriver.HitTrigger, 0.04f);
            AddTimedTransition(hitState, idleState, 0.45f, 0.05f);
            AddAnyStateTriggerTransition(stateMachine, deathState, CharacterAnimationDriver.DeathTrigger, 0.04f);

            EditorUtility.SetDirty(controller);
            summary.ControllerCount++;

            animationSummary = FormatClipSummary(clips.Count, idle, move, attack, hit, death);
            report.AppendLine($"- Animator Controller: `{spec.ControllerPath}`");
            return controller;
        }

        private static bool RepairAnimatorControllerClipReferences(MonsterSpec spec)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(spec.ControllerPath);
            if (controller == null)
            {
                return false;
            }

            var clips = FindAnimationClips(spec);
            if (clips.Count == 0)
            {
                return false;
            }

            var idle = SelectClip(clips, "idle", "default") ?? SelectClip(clips, "flying", "fly", "hover") ?? FirstClip(clips);
            var move = SelectClip(clips, "walk", "run", "move", "flying", "fly") ?? idle;
            var attack = SelectClip(clips, "attack", "bite", "slam") ?? move ?? idle;
            var hit = SelectClip(clips, "hitrecieve", "hitreceive", "recievehit", "receivehit", "hit", "damage") ?? attack ?? idle;
            var death = SelectClip(clips, "death", "die", "dead") ?? hit ?? idle;

            var changed = false;
            changed |= AssignStateMotion(controller, CharacterAnimationDriver.IdleState, idle);
            changed |= AssignStateMotion(controller, CharacterAnimationDriver.MoveState, move);
            changed |= AssignStateMotion(controller, CharacterAnimationDriver.AttackState, attack);
            changed |= AssignStateMotion(controller, CharacterAnimationDriver.HitState, hit);
            changed |= AssignStateMotion(controller, CharacterAnimationDriver.DeathState, death);

            if (changed)
            {
                EditorUtility.SetDirty(controller);
            }

            return changed;
        }

        private static bool AssignStateMotion(AnimatorController controller, string stateName, Motion motion)
        {
            if (motion == null)
            {
                return false;
            }

            var state = FindAnimatorState(controller, stateName);
            if (state == null || state.motion == motion)
            {
                return false;
            }

            state.motion = motion;
            return true;
        }

        private static GameObject CreateOrUpdateEnemyPrefab(
            MonsterSpec spec,
            GameObject visualAsset,
            EnemyConfig config,
            RuntimeAnimatorController controller,
            BuildSummary summary,
            StringBuilder report)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(spec.PrefabPath) != null;
            var root = existing ? PrefabUtility.LoadPrefabContents(spec.PrefabPath) : new GameObject(spec.PrefabName);
            try
            {
                root.name = spec.PrefabName;
                root.transform.localScale = Vector3.one;
                ApplyEnemyLayer(root);
                RemoveRootPlaceholderGeometry(root);

                var collider = EnsureComponent<CapsuleCollider>(root);
                collider.radius = Mathf.Max(0.05f, spec.ColliderRadius);
                collider.height = Mathf.Max(collider.radius * 2f, spec.ColliderHeight);
                collider.center = new Vector3(0f, spec.ColliderCenterY, 0f);
                collider.direction = 1;
                collider.enabled = true;

                var rigidbody = EnsureComponent<Rigidbody>(root);
                rigidbody.useGravity = false;
                rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

                var visualRoot = EnsureChild(root.transform, "VisualRoot");
                ClearChildren(visualRoot);
                var visualInstance = Object.Instantiate(visualAsset, visualRoot);
                visualInstance.name = spec.ModelName;
                visualInstance.transform.localPosition = Vector3.zero;
                visualInstance.transform.localRotation = Quaternion.identity;
                visualInstance.transform.localScale = Vector3.one * Mathf.Max(0.01f, spec.VisualScale);
                RemoveRuntimeComponentsFromVisual(visualInstance);
                var animator = EnsureAnimator(visualInstance, spec, controller);
                EnsureCombatHurtbox(root.transform, visualRoot, spec);

                EnsureSocket(root.transform, "AttackOrigin", new Vector3(0f, spec.ColliderCenterY + 0.1f, Mathf.Max(0.5f, spec.ColliderRadius + 0.25f)));
                EnsureSocket(root.transform, "HitReactionRoot", new Vector3(0f, Mathf.Max(0.65f, spec.ColliderCenterY), 0f));
                EnsureSocket(root.transform, "HitVFXSocket", new Vector3(0f, Mathf.Max(0.75f, spec.ColliderCenterY), 0f));
                EnsureSocket(root.transform, "DeathVFXSocket", new Vector3(0f, 0.25f, 0f));
                EnsureSocket(root.transform, "BossCenterTarget", new Vector3(0f, spec.ColliderCenterY, 0f));
                Transform projectileSpawn = null;
                if (spec.RequiresProjectileSpawnPoint || spec.Boss)
                {
                    projectileSpawn = EnsureSocket(root.transform, "ProjectileSpawnPoint", new Vector3(0f, spec.ColliderCenterY + 0.18f, spec.ColliderRadius + 0.45f));
                }

                var telegraphRoot = EnsureTelegraphVisual(root.transform, spec);
                var controllerComponent = EnsureComponent<EnemyController>(root);
                var health = EnsureComponent<EnemyHealth>(root);
                var movement = EnsureComponent<EnemyMovement>(root);
                var knockback = EnsureComponent<KnockbackReceiver>(root);
                var attack = EnsureComponent<EnemyAttackController>(root);
                var telegraph = EnsureComponent<EnemyTelegraphController>(root);
                EnsureComponent<PooledEnemy>(root);
                EnsureComponent<HitFlashController>(root);
                WireBaseEnemyComponents(root, controllerComponent, health, movement, knockback, attack, telegraph, telegraphRoot, config, spec);
                ApplyEnemyLayer(root);

                if (spec.AddRangedShooterController)
                {
                    var ranged = EnsureComponent<RangedShooterController>(root);
                    var rangedSerialized = new SerializedObject(ranged);
                    SetObject(rangedSerialized, "config", config);
                    SetObject(rangedSerialized, "target", null);
                    SetObject(rangedSerialized, "projectileSpawnPoint", projectileSpawn);
                    SetObject(rangedSerialized, "telegraphController", telegraph);
                    SetBool(rangedSerialized, "autoShoot", true);
                    SetFloat(rangedSerialized, "fallbackAttackRange", spec.AttackRange);
                    SetFloat(rangedSerialized, "fallbackWindupDuration", spec.AttackWindup);
                    SetFloat(rangedSerialized, "fallbackCooldown", spec.AttackCooldown);
                    SetFloat(rangedSerialized, "fallbackProjectileSpeed", spec.ProjectileSpeed);
                    SetFloat(rangedSerialized, "projectileLifetime", 4.25f);
                    rangedSerialized.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(ranged);
                }

                if (spec.Boss)
                {
                    WireBossComponents(root, config, telegraph, spec);
                }

                WireAnimationDriver(root, animator);
                ApplyEnemyLayer(root);

                var saved = false;
                PrefabUtility.SaveAsPrefabAsset(root, spec.PrefabPath, out saved);
                if (!saved)
                {
                    summary.WarningCount++;
                    report.AppendLine($"- Prefab save failed: `{spec.PrefabPath}`");
                }
                else
                {
                    summary.PrefabCount++;
                    report.AppendLine($"- Prefab: `{spec.PrefabPath}`");
                }
            }
            finally
            {
                if (existing)
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
                else
                {
                    Object.DestroyImmediate(root);
                }
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(spec.PrefabPath);
        }

        private static BossPatternConfig CreateOrUpdateBossPattern(BuildSummary summary, StringBuilder report)
        {
            var pattern = AssetDatabase.LoadAssetAtPath<BossPatternConfig>(BossPatternPath);
            if (pattern == null)
            {
                pattern = ScriptableObject.CreateInstance<BossPatternConfig>();
                AssetDatabase.CreateAsset(pattern, BossPatternPath);
                summary.BossAssetCount++;
            }

            pattern.SetLoop(true);
            pattern.SetSteps(new[]
            {
                new BossAttackStep(BossAttackType.BossSlam, 0.85f, 0.1f, 1.15f, 22f, 2.6f, 0f, 0, EnemyTelegraphType.BossSlamArea, VFXEventType.BossHeavyAttackTelegraph),
                new BossAttackStep(BossAttackType.BossCharge, 0.9f, 0.45f, 1.35f, 20f, 0f, 7.5f, 0, EnemyTelegraphType.ChargePath, VFXEventType.BossHeavyAttackTelegraph),
                new BossAttackStep(BossAttackType.SummonAdds, 0.65f, 0.05f, 1.6f, 0f, 0f, 0f, 2, EnemyTelegraphType.Circle, VFXEventType.EliteSpawn)
            });
            EditorUtility.SetDirty(pattern);
            report.AppendLine($"## Boss Pattern\n- Pattern: `{BossPatternPath}`");
            return pattern;
        }

        private static BossConfig CreateOrUpdateBossConfig(
            GeneratedMonsterContent bossContent,
            GeneratedMonsterContent addContent,
            BossPatternConfig pattern,
            BuildSummary summary,
            StringBuilder report)
        {
            var bossConfig = AssetDatabase.LoadAssetAtPath<BossConfig>(BossConfigPath);
            if (bossConfig == null)
            {
                bossConfig = ScriptableObject.CreateInstance<BossConfig>();
                AssetDatabase.CreateAsset(bossConfig, BossConfigPath);
                summary.BossAssetCount++;
            }

            var serialized = new SerializedObject(bossConfig);
            SetString(serialized, "bossId", "boss_yellow_dragon");
            SetString(serialized, "displayName", "Yellow Dragon");
            SetObject(serialized, "enemyConfig", bossContent != null ? bossContent.Config : null);
            SetObject(serialized, "addEnemyConfig", addContent != null ? addContent.Config : null);
            SetObject(serialized, "addEnemyPrefab", addContent != null ? addContent.Prefab : null);
            SetInt(serialized, "maxActiveAdds", 4);
            SetEnum(serialized, "introVfx", (int)VFXEventType.BossSpawnWarning);
            SetEnum(serialized, "enrageVfx", (int)VFXEventType.BossPhaseTransition);
            SetEnum(serialized, "deathVfx", (int)VFXEventType.BossDeath);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            bossConfig.SetPhases(new[]
            {
                new BossPhaseConfig(BossPhaseState.Phase1, 1f, pattern, false, 1f, 1f),
                new BossPhaseConfig(BossPhaseState.Phase2, 0.66f, pattern, false, 0.9f, 1.08f),
                new BossPhaseConfig(BossPhaseState.Phase3, 0.33f, pattern, true, 0.75f, 1.18f)
            });
            EditorUtility.SetDirty(bossConfig);
            report.AppendLine($"- Boss config: `{BossConfigPath}`");
            return bossConfig;
        }

        private static void WireBossPrefab(
            GeneratedMonsterContent bossContent,
            BossConfig bossConfig,
            BuildSummary summary,
            StringBuilder report)
        {
            if (bossContent == null || bossContent.Prefab == null || bossConfig == null)
            {
                summary.WarningCount++;
                report.AppendLine("- Boss prefab wiring skipped: missing YellowDragon prefab or BossConfig.");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(bossContent.Spec.PrefabPath);
            try
            {
                var phase = EnsureComponent<BossPhaseController>(root);
                var pattern = EnsureComponent<BossPatternController>(root);
                var slam = EnsureComponent<BossSlamAttack>(root);
                var charge = EnsureComponent<BossChargeAttack>(root);
                var adds = EnsureComponent<BossAddSpawnAction>(root);
                var intro = EnsureComponent<BossIntroController>(root);
                var outro = EnsureComponent<BossOutroController>(root);
                var bridge = EnsureComponent<BossRuntimeBindingBridge>(root);
                var health = root.GetComponent<EnemyHealth>();
                var telegraph = root.GetComponent<EnemyTelegraphController>();
                var addSpawnPoints = EnsureAddSpawnPoints(root.transform);

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
                SetLayerMask(new SerializedObject(slam), "damageLayers", ResolveLayerMask("Player", ~0));
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

                PrefabUtility.SaveAsPrefabAsset(root, bossContent.Spec.PrefabPath);
                report.AppendLine($"- Boss prefab structure wired: `{bossContent.Spec.PrefabPath}`");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static Dictionary<string, SpawnGroupConfig> CreateOrUpdateSpawnGroups(
            IReadOnlyDictionary<string, GeneratedMonsterContent> generated,
            BuildSummary summary,
            StringBuilder report)
        {
            var groups = new Dictionary<string, SpawnGroupConfig>(StringComparer.Ordinal);
            report.AppendLine("## Spawn Groups");
            foreach (var pair in generated)
            {
                var content = pair.Value;
                var spec = content.Spec;
                var group = AssetDatabase.LoadAssetAtPath<SpawnGroupConfig>(spec.SpawnGroupPath);
                if (group == null)
                {
                    group = ScriptableObject.CreateInstance<SpawnGroupConfig>();
                    AssetDatabase.CreateAsset(group, spec.SpawnGroupPath);
                    summary.SpawnGroupCount++;
                }

                var serialized = new SerializedObject(group);
                SetString(serialized, "groupId", spec.EnemyId + "_group");
                SetObject(serialized, "enemyConfig", content.Config);
                SetObject(serialized, "enemyPrefab", content.Prefab);
                SetBool(serialized, "elite", spec.Elite || spec.Boss);
                SetFloat(serialized, "weight", spec.Weight);
                SetInt(serialized, "minCount", spec.MinCount);
                SetInt(serialized, "maxCount", spec.MaxCount);
                SetInt(serialized, "spawnBurstCount", spec.SpawnBurstCount);
                SetInt(serialized, "budgetCost", spec.BudgetCost);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(group);
                groups[spec.ModelName] = group;
                report.AppendLine($"- `{spec.DisplayName}` -> `{spec.SpawnGroupPath}`");
            }

            report.AppendLine();
            return groups;
        }

        private static WaveTimelineConfig CreateOrUpdateWaveTimeline(
            IReadOnlyDictionary<string, SpawnGroupConfig> groups,
            BuildSummary summary,
            StringBuilder report)
        {
            return CreateOrUpdateWaveTimeline(
                groups,
                TimelinePath,
                "wave_timeline_cute_monsters_test",
                summary,
                report);
        }

        private static WaveTimelineConfig CreateOrUpdateWaveTimeline(
            IReadOnlyDictionary<string, SpawnGroupConfig> groups,
            string timelinePath,
            string timelineId,
            BuildSummary summary,
            StringBuilder report)
        {
            var timeline = AssetDatabase.LoadAssetAtPath<WaveTimelineConfig>(timelinePath);
            if (timeline == null)
            {
                timeline = ScriptableObject.CreateInstance<WaveTimelineConfig>();
                EnsureFolder(Path.GetDirectoryName(timelinePath)?.Replace('\\', '/'));
                AssetDatabase.CreateAsset(timeline, timelinePath);
                summary.WaveAssetCount++;
            }

            var specs = new[]
            {
                new TimelineEntrySpec(0f, 75f, 1.65f, 20, 1f, "GreenDemon", "Demon", "Mushroom", "Bat", "Bee", "Ghost"),
                new TimelineEntrySpec(75f, 180f, 1.45f, 30, 1.12f, "GreenDemon", "Demon", "Mushroom", "Bat", "Bee", "Ghost", "Cactus"),
                new TimelineEntrySpec(180f, 330f, 1.35f, 42, 1.25f, "Demon", "Bat", "Bee", "Cactus", "Cyclops", "Cthulhu"),
                new TimelineEntrySpec(330f, 510f, 1.15f, 58, 1.4f, "Bee", "Ghost", "Cactus", "Cyclops", "Yeti", "Cthulhu"),
                new TimelineEntrySpec(510f, 600f, 1.35f, 42, 1.25f, "Demon", "Ghost", "Yeti")
            };

            var serialized = new SerializedObject(timeline);
            SetString(serialized, "timelineId", timelineId);
            var entries = serialized.FindProperty("entries");
            entries.arraySize = specs.Length;
            for (var i = 0; i < specs.Length; i++)
            {
                var entry = entries.GetArrayElementAtIndex(i);
                SetRelativeFloat(entry, "startTime", specs[i].StartTime);
                SetRelativeFloat(entry, "endTime", specs[i].EndTime);
                SetRelativeFloat(entry, "spawnInterval", specs[i].SpawnInterval);
                SetRelativeInt(entry, "liveEnemyCap", specs[i].LiveEnemyCap);
                SetRelativeFloat(entry, "intensityMultiplier", specs[i].IntensityMultiplier);
                var spawnGroups = entry.FindPropertyRelative("spawnGroups");
                spawnGroups.arraySize = specs[i].GroupKeys.Length;
                for (var groupIndex = 0; groupIndex < specs[i].GroupKeys.Length; groupIndex++)
                {
                    groups.TryGetValue(specs[i].GroupKeys[groupIndex], out var group);
                    spawnGroups.GetArrayElementAtIndex(groupIndex).objectReferenceValue = group;
                }
            }

            SetFloat(serialized, "bossWarningTimeSeconds", 510f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(timeline);
            report.AppendLine($"## Wave Timeline\n- Timeline: `{timelinePath}`");
            return timeline;
        }

        private static void WirePrototypeRunConfig(
            WaveTimelineConfig timeline,
            SpawnGroupConfig bossGroup,
            BuildSummary summary,
            StringBuilder report)
        {
            var runConfigPaths = new[] { RunConfigPath, ForestRunConfigPath };
            foreach (var runConfigPath in runConfigPaths)
            {
                var runConfig = AssetDatabase.LoadAssetAtPath<RunConfig>(runConfigPath);
                if (runConfig == null)
                {
                    summary.WarningCount++;
                    report.AppendLine($"- Prototype run config not found, skipped run wiring: `{runConfigPath}`");
                    continue;
                }

                var serialized = new SerializedObject(runConfig);
                SetObject(serialized, "waveTimeline", timeline);
                SetObject(serialized, "bossSpawnGroup", bossGroup);
                SetFloat(serialized, "bossSpawnTimeSeconds", 540f);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(runConfig);
                report.AppendLine($"- Wired `{runConfigPath}` to cute monster timeline and YellowDragon boss group.");
            }
        }

        private static GameObject CreateOrRepairEnemyProjectilePrefab(
            BuildSummary summary,
            StringBuilder report)
        {
            EnsureFolder(Path.GetDirectoryName(ProjectilePrefabPath)?.Replace('\\', '/'));
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePrefabPath) != null;
            var root = existing ? PrefabUtility.LoadPrefabContents(ProjectilePrefabPath) : GameObject.CreatePrimitive(PrimitiveType.Sphere);
            root.name = Path.GetFileNameWithoutExtension(ProjectilePrefabPath);
            root.transform.localScale = Vector3.one * 0.22f;
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
                rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                var projectileController = EnsureComponent<EnemyProjectileController>(root);
                EnsureComponent<PooledProjectile>(root);

                var serializedProjectile = new SerializedObject(projectileController);
                SetBool(serializedProjectile, "deactivateInsteadOfDestroy", true);
                SetLayerMask(serializedProjectile, "hitLayers", ResolveLayerMask("Player", ~0));

                PrefabUtility.SaveAsPrefabAsset(root, ProjectilePrefabPath);
                report.AppendLine($"- Enemy projectile prefab: `{ProjectilePrefabPath}`");
                return AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePrefabPath);
            }
            finally
            {
                if (existing)
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
                else
                {
                    Object.DestroyImmediate(root);
                }
            }
        }

        private static void WriteIntegrationNotes(
            IReadOnlyDictionary<string, GeneratedMonsterContent> generated,
            IReadOnlyDictionary<string, SpawnGroupConfig> spawnGroups,
            WaveTimelineConfig timeline,
            WaveTimelineConfig desktopPrototypeTimeline,
            BossConfig bossConfig,
            BuildSummary summary)
        {
            var doc = new StringBuilder();
            doc.AppendLine("# Cute Animated Monsters Enemy Integration Notes");
            doc.AppendLine();
            doc.AppendLine("Generated by `CuteMonsterEnemyContentBuilder`. Local repo files remain the source of truth; NotebookLM was used only as supporting context and matched the repo-level prefab/config constraints.");
            doc.AppendLine();
            doc.AppendLine("## Source Pack");
            doc.AppendLine();
            doc.AppendLine($"- FBX root: `{FbxRoot}`");
            doc.AppendLine($"- Texture root: `{TextureRoot}`");
            doc.AppendLine($"- OBJ/material root: `{ObjRoot}`");
            doc.AppendLine("- Source FBX, texture, OBJ, blend, glTF, and `.meta` files are not modified by the builder.");
            doc.AppendLine();
            doc.AppendLine("## GreenDemon Reference Findings");
            doc.AppendLine();
            doc.AppendLine($"- Existing generated GreenDemon prefab remains available at `{ExistingGeneratedGreenDemonPrefabPath}` when present.");
            doc.AppendLine("- Runtime contract confirmed from code: `EnemyController`, `EnemyHealth`, `EnemyMovement`, `EnemyAttackController`, `KnockbackReceiver`, `HitFlashController`, `PooledEnemy`, `CharacterAnimationDriver`, collider, Rigidbody, self `targetTransform`, `VisualRoot`, `AttackOrigin`, and `HitReactionRoot`.");
            doc.AppendLine("- Prefabs intentionally do not serialize a scene Player target; `SurvivorSpawnDirector` and `EnemyController.Initialize` assign runtime target references.");
            doc.AppendLine("- XP is still owned by `ArenaRunDirector`, and pooled enemy deaths remain guarded by the current health/run reward logic.");
            doc.AppendLine();
            doc.AppendLine("## GreenDemon Contract Checklist");
            doc.AppendLine();
            doc.AppendLine("- Root is on the `Enemy` layer when that layer exists.");
            doc.AppendLine("- Root has an enabled collider and a non-gravity Rigidbody with X/Z rotation frozen.");
            doc.AppendLine("- Root has `EnemyController`, `EnemyHealth`, `EnemyMovement`, `EnemyAttackController`, `KnockbackReceiver`, `HitFlashController`, `PooledEnemy`, and `CharacterAnimationDriver`.");
            doc.AppendLine("- Required sockets exist: `VisualRoot`, `AttackOrigin`, `HitReactionRoot`, `HitVFXSocket`, and `DeathVFXSocket`.");
            doc.AppendLine("- `EnemyController`, `EnemyHealth`, `EnemyMovement`, `KnockbackReceiver`, and `EnemyAttackController` reference the enemy config.");
            doc.AppendLine("- `EnemyHealth.targetTransform` points at the prefab root so player targeting hits a stable transform.");
            doc.AppendLine("- `EnemyController.target`, `EnemyMovement.target`, and `EnemyAttackController.target` remain null in prefab assets; runtime spawn assigns the player.");
            doc.AppendLine("- Animator has `Idle`, `Move`, `Attack`, `Hit`, and `Death` states using non-null clips from the enemy model source.");
            doc.AppendLine("- Runtime behavior contract: spawn through `SurvivorSpawnDirector`, receive runtime target binding, move toward player, attack, receive damage, flash/knock back, die once, emit XP/reward flow once, return to pool, and respawn with health/colliders/animation/knockback/flash reset.");
            doc.AppendLine();
            doc.AppendLine("## Monster Asset Inventory");
            doc.AppendLine();
            doc.AppendLine("| Model | FBX path | Clips visible to builder | Rig/import note | Material/texture | Suggested archetype | Status |");
            doc.AppendLine("|---|---|---|---|---|---|---|");
            foreach (var spec in MonsterSpecs)
            {
                var clipNames = FindAnimationClips(spec);
                var clipText = clipNames.Count > 0 ? string.Join(", ", UniqueClipNames(clipNames)) : "none visible from AssetDatabase; controller states use fallback motions";
                doc.AppendLine($"| {spec.ModelName} | `{spec.FbxPath}` | {clipText} | {ResolveRigNote(spec)} | `{spec.TexturePath}`, `{spec.MaterialPath}` | {spec.SuggestedArchetype} | {spec.IntegrationStatus} |");
            }

            doc.AppendLine();
            doc.AppendLine("## Enemy Archetype Mapping");
            doc.AppendLine();
            foreach (var spec in MonsterSpecs)
            {
                doc.AppendLine($"- `{spec.ModelName}`: {spec.SuggestedArchetype}. {spec.IntegrationStatus}.");
            }

            doc.AppendLine();
            doc.AppendLine("## Configs Created/Updated");
            doc.AppendLine();
            foreach (var spec in MonsterSpecs)
            {
                if (spec.SelectedFirstPass)
                {
                    doc.AppendLine($"- `{spec.ConfigPath}`: `{spec.EnemyId}`, {spec.Archetype}, {spec.Rank}, HP {spec.MaxHealth:0.#}, speed {spec.MoveSpeed:0.##}, damage {spec.ContactDamage:0.#}, XP {spec.XpReward}.");
                }
            }

            doc.AppendLine();
            doc.AppendLine("## Prefabs Created/Updated");
            doc.AppendLine();
            foreach (var spec in MonsterSpecs)
            {
                if (spec.SelectedFirstPass)
                {
                    doc.AppendLine($"- `{spec.PrefabPath}`: model `{spec.ModelName}`, collider radius {spec.ColliderRadius:0.##}, height {spec.ColliderHeight:0.##}, budget {spec.BudgetCost}.");
                }
            }

            doc.AppendLine();
            doc.AppendLine("## Enemy Contract Validation Results");
            doc.AppendLine();
            doc.AppendLine("| Enemy | Contract result | Attack safety | Runtime target policy | Pooling/feedback |");
            doc.AppendLine("|---|---|---|---|---|");
            foreach (var spec in MonsterSpecs)
            {
                if (spec.SelectedFirstPass)
                {
                    var attackSafety = spec.AddRangedShooterController
                        ? "contact damage disabled; projectile windup owns damage"
                        : spec.UseContactTelegraphWindup
                            ? "contact damage enabled with telegraphed windup"
                            : "contact damage enabled with cooldown";
                    doc.AppendLine($"| {spec.DisplayName} | generated to GreenDemon contract | {attackSafety} | no serialized scene Player target; spawn assigns runtime target | pooled enemy, hit flash, knockback, semantic VFX |");
                }
            }

            doc.AppendLine();
            doc.AppendLine("## Animation Integration");
            doc.AppendLine();
            foreach (var pair in generated)
            {
                doc.AppendLine($"- `{pair.Value.Spec.ModelName}`: `{pair.Value.ControllerPath}`; {pair.Value.AnimationSummary}.");
            }

            doc.AppendLine();
            doc.AppendLine("## Attack Behavior Integration");
            doc.AppendLine();
            doc.AppendLine("| Enemy | Attack archetype | Config-driven values | Notes |");
            doc.AppendLine("|---|---|---|---|");
            foreach (var spec in MonsterSpecs)
            {
                if (!spec.SelectedFirstPass)
                {
                    continue;
                }

                doc.AppendLine($"| {spec.DisplayName} | {spec.AttackArchetypeLabel} | damage {spec.ContactDamage:0.#}, range {spec.AttackRange:0.##}, cooldown {spec.AttackCooldown:0.##}, windup {spec.AttackWindup:0.##} | {ResolveAttackNotes(spec)} |");
            }

            doc.AppendLine();
            doc.AppendLine("- Cthulhu keeps distance through `stoppingDistance` and uses `RangedShooterController`; its `EnemyAttackController.autoDealContactDamage` is disabled so it cannot damage the player from projectile range.");
            doc.AppendLine("- Cyclops, Yeti, Cactus, and YellowDragon use the existing telegraph windup path for heavier close-range attacks.");
            doc.AppendLine("- Basic and swarm enemies stay on short contact cooldowns for readable pressure without adding a new untested combat system.");
            doc.AppendLine();
            doc.AppendLine("## VFX/Hit Feel Integration");
            doc.AppendLine();
            doc.AppendLine("| Enemy | VFX/feedback role | Spawn event | Death event |");
            doc.AppendLine("|---|---|---|---|");
            foreach (var spec in MonsterSpecs)
            {
                if (!spec.SelectedFirstPass)
                {
                    continue;
                }

                var spawnEvent = spec.Elite || spec.Boss ? VFXEventType.EliteSpawn : VFXEventType.EnemySpawn;
                var deathEvent = spec.Boss ? VFXEventType.BossDeath : spec.Elite ? VFXEventType.EliteDeath : VFXEventType.EnemyDeath;
                doc.AppendLine($"| {spec.DisplayName} | {spec.VfxFeedbackLabel} | `{spawnEvent}` | `{deathEvent}` |");
            }

            doc.AppendLine();
            doc.AppendLine("- Every generated enemy includes `HitFlashController`, `KnockbackReceiver`, `PooledEnemy`, and semantic VFX event fields on `EnemyConfig`.");
            doc.AppendLine("- Spawn/warning and damage-number behavior remains centralized through the existing `SurvivorSpawnDirector`, `CombatVFXEventController`, and `ImpactFeedbackController` paths.");
            doc.AppendLine();
            doc.AppendLine("## Spawn/Wave Integration");
            doc.AppendLine();
            doc.AppendLine($"- Spawn groups live under `{SpawnGroupRoot}`.");
            doc.AppendLine($"- Cute monster test timeline: `{TimelinePath}`.");
            doc.AppendLine($"- Desktop prototype timeline mirror: `{DesktopPrototypeTimelinePath}`.");
            doc.AppendLine($"- Prototype run config is wired to `{TimelinePath}` and boss group `{(spawnGroups.TryGetValue("YellowDragon", out var bossGroup) && bossGroup != null ? AssetDatabase.GetAssetPath(bossGroup) : "missing")}` when the builder runs successfully.");
            doc.AppendLine($"- Scene repair tools set `ArenaRunDirector.waveTimelineOverride` and `SurvivorSpawnDirector.waveTimeline` to `{TimelinePath}`; `{(desktopPrototypeTimeline != null ? AssetDatabase.GetAssetPath(desktopPrototypeTimeline) : DesktopPrototypeTimelinePath)}` mirrors the same roster as a compatibility fallback for older scene references.");
            doc.AppendLine();
            doc.AppendLine("| Enemy | Spawn group | Wave role | Budget | Weight |");
            doc.AppendLine("|---|---|---|---:|---:|");
            foreach (var spec in MonsterSpecs)
            {
                if (spec.SelectedFirstPass)
                {
                    doc.AppendLine($"| {spec.DisplayName} | `{spec.SpawnGroupPath}` | {spec.WaveRole} | {spec.BudgetCost} | {spec.Weight:0.##} |");
                }
            }
            doc.AppendLine();
            doc.AppendLine("## Boss Structure");
            doc.AppendLine();
            doc.AppendLine($"- Boss config: `{BossConfigPath}`.");
            doc.AppendLine($"- Boss pattern: `{BossPatternPath}`.");
            doc.AppendLine($"- Boss prefab: `{MonsterSpecs[10].PrefabPath}`.");
            doc.AppendLine("- Boss health HUD compatibility uses existing `BossEvents`, `EnemyHealth`, and `BossPhaseController` binding behavior.");
            doc.AppendLine();
            doc.AppendLine("## Validator/Repair Notes");
            doc.AppendLine();
            doc.AppendLine("- `Tap Knockout > Survivor > Repair Prototype Scene` regenerates cute monster content before validating the scene.");
            doc.AppendLine("- `SurvivorReferenceValidator` now checks first-pass cute monster configs, prefabs, spawn groups, timeline, and prototype run wiring.");
            doc.AppendLine("- Safe local repairs add missing generated prefab components only through this builder; source pack assets are not changed.");
            doc.AppendLine();
            doc.AppendLine("## Manual Unity Assignments Required");
            doc.AppendLine();
            doc.AppendLine("- Run `Tap Knockout > Survivor > Repair Prototype Scene` after Unity recompiles.");
            doc.AppendLine("- Review generated prefab scales/colliders from the gameplay camera.");
            doc.AppendLine("- Assign or tune final VFX/SFX profile assets if the default semantic VFX events are not visually sufficient.");
            doc.AppendLine("- If `RunConfig_DesktopSurvivorPrototype` should not use the cute monster test timeline for a branch, reassign the prior timeline manually.");
            doc.AppendLine();
            doc.AppendLine("## Test Checklist");
            doc.AppendLine();
            doc.AppendLine("1. Open `Assets/_Project/Scenes/DesktopSurvivorPrototype.unity`.");
            doc.AppendLine("2. Run `Tap Knockout > Survivor > Repair Prototype Scene`.");
            doc.AppendLine("3. Run `Tap Knockout > Survivor > Validate Prototype Scene`.");
            doc.AppendLine("4. Press Play and verify GreenDemon, Demon, Bat/Bee/Ghost, Mushroom, Cactus/Cyclops/Yeti, Cthulhu, and YellowDragon spawn paths.");
            doc.AppendLine("5. Confirm movement, damage, hit flash, knockback, death, XP reward, pooling reset, and boss health HUD binding.");
            doc.AppendLine();
            doc.AppendLine("## Known TODOs");
            doc.AppendLine();
            doc.AppendLine("- Alien, Alien_Tall, Crab, Skull, and Tree are strong future ranged/tank/boss candidates but are intentionally deferred from the first-pass timeline.");
            doc.AppendLine("- Chicken, Panda, Penguin, and Pig need distinct mechanics before they should be added as more than duplicate chasers.");
            doc.AppendLine("- Final animation quality depends on actual imported FBX clip availability; any model with no visible clip needs Unity-side import review or authored fallback clips.");
            doc.AppendLine("- Final art/audio tuning and 100+ enemy stress testing still require Unity Play Mode validation.");
            doc.AppendLine();
            doc.AppendLine("## Builder Summary");
            doc.AppendLine();
            doc.AppendLine($"- Configs created this run: {summary.ConfigCount}");
            doc.AppendLine($"- Prefabs saved this run: {summary.PrefabCount}");
            doc.AppendLine($"- Animator controllers created this run: {summary.ControllerCount}");
            doc.AppendLine($"- Spawn groups created this run: {summary.SpawnGroupCount}");
            doc.AppendLine($"- Warnings: {summary.WarningCount}");

            WriteTextAsset(IntegrationNotesPath, doc.ToString());
        }

        private static void ValidateGeneratedPrefabContract(
            TapKnockout.Editor.SurvivorValidationReport report,
            MonsterSpec spec,
            GameObject prefab)
        {
            var config = AssetDatabase.LoadAssetAtPath<EnemyConfig>(spec.ConfigPath);
            var enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer >= 0 && prefab.layer != enemyLayer)
            {
                report.Error($"{spec.PrefabName} root must be on Enemy layer.");
            }

            var rootCollider = prefab.GetComponent<Collider>();
            if (rootCollider == null || !rootCollider.enabled)
            {
                report.Error($"{spec.PrefabName} root must have an enabled Collider.");
            }

            var rigidbody = prefab.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                report.Error($"{spec.PrefabName} root must have a Rigidbody.");
            }
            else
            {
                if (rigidbody.useGravity)
                {
                    report.Warn($"{spec.PrefabName} Rigidbody should not use gravity for survivor arena spawn placement.");
                }

                var requiredConstraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                if ((rigidbody.constraints & requiredConstraints) != requiredConstraints)
                {
                    report.Warn($"{spec.PrefabName} Rigidbody should freeze X/Z rotation.");
                }
            }

            var controller = prefab.GetComponent<EnemyController>();
            if (controller == null)
            {
                report.Error($"{spec.PrefabName} is missing EnemyController.");
            }
            else
            {
                if (controller.Config != config)
                {
                    report.Error($"{spec.PrefabName}.EnemyController must reference {spec.ConfigName}.");
                }

                if (controller.Target != null)
                {
                    report.Error($"{spec.PrefabName} must not serialize a scene/runtime target on EnemyController.");
                }
            }

            var health = prefab.GetComponent<EnemyHealth>();
            if (health == null)
            {
                report.Error($"{spec.PrefabName} is missing EnemyHealth.");
            }
            else
            {
                if (health.Config != config)
                {
                    report.Error($"{spec.PrefabName}.EnemyHealth must reference {spec.ConfigName}.");
                }

                if (health.TargetTransform != prefab.transform)
                {
                    report.Warn($"{spec.PrefabName}.EnemyHealth targetTransform should point at the prefab root for player targeting.");
                }
            }

            var movement = prefab.GetComponent<EnemyMovement>();
            if (movement == null)
            {
                report.Error($"{spec.PrefabName} is missing EnemyMovement.");
            }
            else if (movement.Target != null)
            {
                report.Error($"{spec.PrefabName} must not serialize a scene/runtime target on EnemyMovement.");
            }

            var attack = prefab.GetComponent<EnemyAttackController>();
            if (attack == null)
            {
                report.Error($"{spec.PrefabName} is missing EnemyAttackController.");
            }
            else
            {
                if (attack.Target != null)
                {
                    report.Error($"{spec.PrefabName} must not serialize a scene/runtime target on EnemyAttackController.");
                }

                var attackSerialized = new SerializedObject(attack);
                if (GetBool(attackSerialized, "autoDealContactDamage", true) != spec.ContactDamageEnabled)
                {
                    report.Error($"{spec.PrefabName}.EnemyAttackController contact damage toggle does not match {spec.AttackArchetypeLabel}.");
                }

                if (GetBool(attackSerialized, "useTelegraphWindup", false) != spec.UseContactTelegraphWindup)
                {
                    report.Warn($"{spec.PrefabName}.EnemyAttackController telegraph windup does not match archetype tuning.");
                }
            }

            if (prefab.GetComponentInChildren<KnockbackReceiver>(true) == null)
            {
                report.Warn($"{spec.PrefabName} is missing KnockbackReceiver.");
            }

            if (prefab.GetComponentInChildren<HitFlashController>(true) == null)
            {
                report.Warn($"{spec.PrefabName} is missing HitFlashController.");
            }

            if (prefab.GetComponentInChildren<PooledEnemy>(true) == null)
            {
                report.Warn($"{spec.PrefabName} is missing PooledEnemy.");
            }

            if (prefab.GetComponentInChildren<CharacterAnimationDriver>(true) == null)
            {
                report.Warn($"{spec.PrefabName} is missing CharacterAnimationDriver.");
            }

            var animator = prefab.GetComponentInChildren<Animator>(true);
            if (animator == null)
            {
                report.Warn($"{spec.PrefabName} is missing Animator.");
            }
            else
            {
                ValidateAnimatorMapping(report, spec, animator);
            }

            if (FindDeepChild(prefab.transform, "VisualRoot") == null)
            {
                report.Error($"{spec.PrefabName} is missing VisualRoot.");
            }

            if (FindDeepChild(prefab.transform, "AttackOrigin") == null)
            {
                report.Error($"{spec.PrefabName} is missing AttackOrigin.");
            }

            if (FindDeepChild(prefab.transform, "HitReactionRoot") == null)
            {
                report.Error($"{spec.PrefabName} is missing HitReactionRoot.");
            }

            if (FindDeepChild(prefab.transform, "HitVFXSocket") == null)
            {
                report.Error($"{spec.PrefabName} is missing HitVFXSocket.");
            }

            if (FindDeepChild(prefab.transform, "DeathVFXSocket") == null)
            {
                report.Error($"{spec.PrefabName} is missing DeathVFXSocket.");
            }

            if (FindDeepChild(prefab.transform, "TelegraphRoot") == null)
            {
                report.Warn($"{spec.PrefabName} is missing TelegraphRoot.");
            }

            if (spec.Boss && prefab.GetComponentInChildren<BossPhaseController>(true) == null)
            {
                report.Warn($"{spec.PrefabName} is marked boss but has no BossPhaseController.");
            }

            if (spec.RequiresProjectileSpawnPoint && FindDeepChild(prefab.transform, "ProjectileSpawnPoint") == null)
            {
                report.Warn($"{spec.PrefabName} requires ProjectileSpawnPoint but none was found.");
            }

            if (spec.AddRangedShooterController)
            {
                var ranged = prefab.GetComponent<RangedShooterController>();
                if (ranged == null)
                {
                    report.Error($"{spec.PrefabName} is configured as ranged but has no RangedShooterController.");
                }
                else if (new SerializedObject(ranged).FindProperty("target")?.objectReferenceValue != null)
                {
                    report.Error($"{spec.PrefabName} must not serialize a scene/runtime target on RangedShooterController.");
                }

                if (config == null || config.ProjectilePrefab == null)
                {
                    report.Error($"{spec.PrefabName} ranged config must reference {ProjectilePrefabPath}.");
                }
            }
        }

        private static void ValidateAnimatorMapping(TapKnockout.Editor.SurvivorValidationReport report, MonsterSpec spec, Animator animator)
        {
            if (animator.runtimeAnimatorController == null)
            {
                report.Error($"{spec.PrefabName} Animator has no controller.");
                return;
            }

            var controllerPath = AssetDatabase.GetAssetPath(animator.runtimeAnimatorController);
            if (!string.Equals(controllerPath, spec.ControllerPath, StringComparison.Ordinal))
            {
                report.Warn($"{spec.PrefabName} Animator controller should be `{spec.ControllerPath}`, found `{controllerPath}`.");
            }

            var controller = animator.runtimeAnimatorController as AnimatorController
                ?? AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null || controller.layers.Length == 0)
            {
                report.Error($"{spec.PrefabName} Animator controller is not inspectable.");
                return;
            }

            ValidateAnimatorStateMotion(report, spec, controller, CharacterAnimationDriver.IdleState);
            ValidateAnimatorStateMotion(report, spec, controller, CharacterAnimationDriver.MoveState);
            ValidateAnimatorStateMotion(report, spec, controller, CharacterAnimationDriver.AttackState);
            ValidateAnimatorStateMotion(report, spec, controller, CharacterAnimationDriver.HitState);
            ValidateAnimatorStateMotion(report, spec, controller, CharacterAnimationDriver.DeathState);
        }

        private static void ValidateAnimatorStateMotion(
            TapKnockout.Editor.SurvivorValidationReport report,
            MonsterSpec spec,
            AnimatorController controller,
            string stateName)
        {
            var state = FindAnimatorState(controller, stateName);
            if (state == null)
            {
                report.Error($"{spec.PrefabName} Animator controller is missing `{stateName}` state.");
                return;
            }

            if (state.motion == null)
            {
                report.Error($"{spec.PrefabName} Animator `{stateName}` state has no motion.");
                return;
            }

            var motionPath = AssetDatabase.GetAssetPath(state.motion);
            if (!string.Equals(motionPath, spec.FbxPath, StringComparison.Ordinal))
            {
                report.Warn($"{spec.PrefabName} Animator `{stateName}` uses `{motionPath}` instead of its own FBX `{spec.FbxPath}`.");
            }
        }

        private static AnimatorState FindAnimatorState(AnimatorController controller, string stateName)
        {
            if (controller == null || controller.layers.Length == 0)
            {
                return null;
            }

            var states = controller.layers[0].stateMachine.states;
            for (var i = 0; i < states.Length; i++)
            {
                if (states[i].state != null && string.Equals(states[i].state.name, stateName, StringComparison.Ordinal))
                {
                    return states[i].state;
                }
            }

            return null;
        }

        private static void WireBaseEnemyComponents(
            GameObject root,
            EnemyController controller,
            EnemyHealth health,
            EnemyMovement movement,
            KnockbackReceiver knockback,
            EnemyAttackController attack,
            EnemyTelegraphController telegraph,
            Transform telegraphRoot,
            EnemyConfig config,
            MonsterSpec spec)
        {
            var serializedController = new SerializedObject(controller);
            SetObject(serializedController, "config", config);
            SetObject(serializedController, "health", health);
            SetObject(serializedController, "movement", movement);
            SetObject(serializedController, "knockbackReceiver", knockback);
            SetObject(serializedController, "attackController", attack);
            SetObject(serializedController, "target", null);
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            var serializedHealth = new SerializedObject(health);
            SetObject(serializedHealth, "config", config);
            SetObject(serializedHealth, "targetTransform", root.transform);
            SetBool(serializedHealth, "targetableWhenAlive", true);
            SetBool(serializedHealth, "deactivateOnDeath", false);
            SetBool(serializedHealth, "disableCollidersOnDeath", true);
            SetBool(serializedHealth, "autoConfigureCombatHurtbox", true);
            SetFloat(serializedHealth, "combatHurtboxVerticalPadding", CombatHurtboxVerticalPadding);
            SetFloat(serializedHealth, "combatHurtboxHorizontalPadding", CombatHurtboxHorizontalPadding);
            SetFloat(serializedHealth, "combatHurtboxBodyTopPadding", spec.CombatHurtboxBodyTopPadding);
            SetFloat(serializedHealth, "minimumCombatHurtboxRadius", spec.MinimumCombatHurtboxRadius);
            SetBool(serializedHealth, "logHits", false);
            SetBool(serializedHealth, "logDeath", false);
            serializedHealth.ApplyModifiedPropertiesWithoutUndo();

            var serializedMovement = new SerializedObject(movement);
            SetObject(serializedMovement, "config", config);
            SetObject(serializedMovement, "target", null);
            serializedMovement.ApplyModifiedPropertiesWithoutUndo();

            SetObject(new SerializedObject(knockback), "config", config);

            var serializedAttack = new SerializedObject(attack);
            SetObject(serializedAttack, "config", config);
            SetObject(serializedAttack, "target", null);
            SetObject(serializedAttack, "telegraphController", telegraph);
            SetBool(serializedAttack, "autoDealContactDamage", spec.ContactDamageEnabled);
            SetBool(serializedAttack, "useTelegraphWindup", spec.UseContactTelegraphWindup);
            SetFloat(serializedAttack, "fallbackWindupDuration", spec.AttackWindup);
            SetFloat(serializedAttack, "fallbackCancelledRetryDelay", 0.18f);
            SetFloat(serializedAttack, "fallbackAttackRange", spec.AttackRange);
            SetFloat(serializedAttack, "fallbackAttackCooldown", spec.AttackCooldown);
            SetFloat(serializedAttack, "fallbackContactDamage", spec.ContactDamage);
            serializedAttack.ApplyModifiedPropertiesWithoutUndo();

            var serializedTelegraph = new SerializedObject(telegraph);
            SetObject(serializedTelegraph, "telegraphRoot", telegraphRoot);
            var renderer = telegraphRoot != null ? telegraphRoot.GetComponentInChildren<Renderer>(true) : null;
            SetObject(serializedTelegraph, "telegraphRenderer", renderer);
            serializedTelegraph.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireBossComponents(GameObject root, EnemyConfig enemyConfig, EnemyTelegraphController telegraph, MonsterSpec spec)
        {
            EnsureComponent<BossPhaseController>(root);
            EnsureComponent<BossPatternController>(root);
            var slam = EnsureComponent<BossSlamAttack>(root);
            var charge = EnsureComponent<BossChargeAttack>(root);
            EnsureComponent<BossAddSpawnAction>(root);
            EnsureComponent<BossIntroController>(root);
            EnsureComponent<BossOutroController>(root);
            EnsureComponent<BossRuntimeBindingBridge>(root);
            SetObject(new SerializedObject(slam), "telegraphController", telegraph);
            SetLayerMask(new SerializedObject(slam), "damageLayers", ResolveLayerMask("Player", ~0));
            SetObject(new SerializedObject(charge), "telegraphController", telegraph);
        }

        private static void WireAnimationDriver(GameObject root, Animator animator)
        {
            RemoveChildAnimationDrivers(root);
            var driver = EnsureComponent<CharacterAnimationDriver>(root);
            var serializedDriver = new SerializedObject(driver);
            SetObject(serializedDriver, "animator", animator);
            SetBool(serializedDriver, "isPlayer", false);
            SetObject(serializedDriver, "playerMovement", null);
            SetObject(serializedDriver, "playerAttack", null);
            SetObject(serializedDriver, "playerDash", null);
            SetObject(serializedDriver, "playerHealth", null);
            SetObject(serializedDriver, "enemyMovement", root.GetComponent<EnemyMovement>());
            SetObject(serializedDriver, "enemyAttack", root.GetComponent<EnemyAttackController>());
            SetObject(serializedDriver, "enemyHealth", root.GetComponent<EnemyHealth>());
            SetObject(serializedDriver, "enemyKnockbackReceiver", root.GetComponent<KnockbackReceiver>());
            serializedDriver.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(driver);
        }

        private static Animator EnsureAnimator(GameObject visualInstance, MonsterSpec spec, RuntimeAnimatorController controller)
        {
            var animator = visualInstance.GetComponentInChildren<Animator>(true) ?? visualInstance.AddComponent<Animator>();
            var avatar = ResolveAvatar(spec.FbxPath);
            if (avatar != null)
            {
                animator.avatar = avatar;
            }

            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.updateMode = AnimatorUpdateMode.Normal;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            EditorUtility.SetDirty(animator);
            return animator;
        }

        private static Avatar ResolveAvatar(string modelPath)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(modelPath);
            for (var i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Avatar avatar)
                {
                    return avatar;
                }
            }

            return null;
        }

        private static void RemoveRuntimeComponentsFromVisual(GameObject visualInstance)
        {
            var colliders = visualInstance.GetComponentsInChildren<Collider>(true);
            for (var i = colliders.Length - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(colliders[i]);
            }

            var rigidbodies = visualInstance.GetComponentsInChildren<Rigidbody>(true);
            for (var i = rigidbodies.Length - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(rigidbodies[i]);
            }
        }

        private static void EnsureCombatHurtbox(Transform root, Transform visualRoot, MonsterSpec spec)
        {
            var hurtbox = EnsureChild(root, CombatHurtboxName);
            hurtbox.localPosition = Vector3.zero;
            hurtbox.localRotation = Quaternion.identity;
            hurtbox.localScale = Vector3.one;

            var collider = EnsureComponent<CapsuleCollider>(hurtbox.gameObject);
            var bodyBounds = new Bounds(
                new Vector3(0f, spec.ColliderCenterY, 0f),
                new Vector3(spec.ColliderRadius * 2f, spec.ColliderHeight, spec.ColliderRadius * 2f));
            var localBounds = bodyBounds;
            if (TryCalculateLocalRendererBounds(root, visualRoot, out var rendererBounds))
            {
                localBounds.Encapsulate(rendererBounds);
            }

            var horizontalRadius = Mathf.Max(
                spec.MinimumCombatHurtboxRadius,
                Mathf.Max(spec.ColliderRadius, localBounds.extents.x, localBounds.extents.z) + CombatHurtboxHorizontalPadding);
            var bottom = Mathf.Min(0f, bodyBounds.min.y, localBounds.min.y - CombatHurtboxVerticalPadding * 0.5f);
            var top = Mathf.Max(
                bodyBounds.max.y + spec.CombatHurtboxBodyTopPadding,
                localBounds.max.y + CombatHurtboxVerticalPadding * 0.5f);
            var height = Mathf.Max(
                horizontalRadius * 2f,
                spec.ColliderHeight,
                localBounds.size.y + CombatHurtboxVerticalPadding,
                top - bottom);
            top = Mathf.Max(top, bottom + height);

            collider.isTrigger = true;
            collider.direction = 1;
            collider.radius = Mathf.Max(0.1f, horizontalRadius);
            collider.height = Mathf.Max(collider.radius * 2f, height);
            collider.center = new Vector3(0f, (bottom + top) * 0.5f, 0f);
            collider.enabled = true;
        }

        private static bool TryCalculateLocalRendererBounds(Transform root, Transform visualRoot, out Bounds bounds)
        {
            bounds = default;
            if (root == null || visualRoot == null)
            {
                return false;
            }

            var renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
            var hasBounds = false;
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                var rendererBounds = renderer.bounds;
                var min = rendererBounds.min;
                var max = rendererBounds.max;
                var corners = new[]
                {
                    new Vector3(min.x, min.y, min.z),
                    new Vector3(min.x, min.y, max.z),
                    new Vector3(min.x, max.y, min.z),
                    new Vector3(min.x, max.y, max.z),
                    new Vector3(max.x, min.y, min.z),
                    new Vector3(max.x, min.y, max.z),
                    new Vector3(max.x, max.y, min.z),
                    new Vector3(max.x, max.y, max.z)
                };

                for (var cornerIndex = 0; cornerIndex < corners.Length; cornerIndex++)
                {
                    var localPoint = root.InverseTransformPoint(corners[cornerIndex]);
                    if (hasBounds)
                    {
                        bounds.Encapsulate(localPoint);
                    }
                    else
                    {
                        bounds = new Bounds(localPoint, Vector3.zero);
                        hasBounds = true;
                    }
                }
            }

            return hasBounds;
        }

        private static void RemoveChildAnimationDrivers(GameObject root)
        {
            var drivers = root.GetComponentsInChildren<CharacterAnimationDriver>(true);
            for (var i = 0; i < drivers.Length; i++)
            {
                if (drivers[i] != null && drivers[i].gameObject != root)
                {
                    Object.DestroyImmediate(drivers[i]);
                }
            }
        }

        private static void RemoveRootPlaceholderGeometry(GameObject root)
        {
            if (root.TryGetComponent<MeshRenderer>(out var renderer))
            {
                Object.DestroyImmediate(renderer);
            }

            if (root.TryGetComponent<MeshFilter>(out var meshFilter))
            {
                Object.DestroyImmediate(meshFilter);
            }
        }

        private static Transform EnsureTelegraphVisual(Transform root, MonsterSpec spec)
        {
            var telegraphRoot = EnsureChild(root, "TelegraphRoot");
            telegraphRoot.localPosition = new Vector3(0f, 0.03f, 0f);
            telegraphRoot.localRotation = Quaternion.identity;
            telegraphRoot.localScale = Vector3.one;

            var visual = telegraphRoot.Find("TelegraphVisual");
            if (visual == null)
            {
                var primitive = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                primitive.name = "TelegraphVisual";
                primitive.transform.SetParent(telegraphRoot, false);
                visual = primitive.transform;
            }

            visual.localPosition = Vector3.zero;
            visual.localScale = new Vector3(Mathf.Max(0.75f, spec.ColliderRadius * 2f), 0.01f, Mathf.Max(0.75f, spec.ColliderRadius * 2f));
            var collider = visual.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            var renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateMaterial("MAT_CuteMonster_Telegraph", new Color(1f, 0.34f, 0.1f, 0.55f));
            }

            telegraphRoot.gameObject.SetActive(false);
            return telegraphRoot;
        }

        private static Transform[] EnsureAddSpawnPoints(Transform root)
        {
            var addRoot = EnsureChild(root, "AddSpawnPoints");
            var positions = new[]
            {
                new Vector3(-2.4f, 0f, 1.6f),
                new Vector3(2.4f, 0f, 1.6f),
                new Vector3(-2.4f, 0f, -1.6f),
                new Vector3(2.4f, 0f, -1.6f)
            };
            var points = new Transform[positions.Length];
            for (var i = 0; i < positions.Length; i++)
            {
                points[i] = EnsureSocket(addRoot, $"AddSpawn_{i + 1:00}", positions[i]);
            }

            return points;
        }

        private static List<AnimationClip> FindAnimationClips(MonsterSpec spec)
        {
            var clips = new List<AnimationClip>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            AddClipsFromObjects(AssetDatabase.LoadAllAssetRepresentationsAtPath(spec.FbxPath), clips, seen);
            AddClipsFromObjects(AssetDatabase.LoadAllAssetsAtPath(spec.FbxPath), clips, seen);

            var guids = AssetDatabase.FindAssets($"{spec.ModelName} t:AnimationClip", new[] { FbxRoot, "Assets/_Project/Animation/Clips/Enemy" });
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                AddClip(AssetDatabase.LoadAssetAtPath<AnimationClip>(path), path, clips, seen);
                AddClipsFromObjects(AssetDatabase.LoadAllAssetRepresentationsAtPath(path), clips, seen);
            }

            return clips;
        }

        private static void AddClipsFromObjects(Object[] assets, ICollection<AnimationClip> clips, HashSet<string> seen)
        {
            for (var i = 0; i < assets.Length; i++)
            {
                AddClip(assets[i] as AnimationClip, AssetDatabase.GetAssetPath(assets[i]), clips, seen);
            }
        }

        private static void AddClip(AnimationClip clip, string path, ICollection<AnimationClip> clips, HashSet<string> seen)
        {
            if (clip == null || string.IsNullOrWhiteSpace(clip.name) || clip.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (seen.Add($"{path}::{clip.name}"))
            {
                clips.Add(clip);
            }
        }

        private static AnimationClip SelectClip(IReadOnlyList<AnimationClip> clips, params string[] keywords)
        {
            for (var keywordIndex = 0; keywordIndex < keywords.Length; keywordIndex++)
            {
                for (var clipIndex = 0; clipIndex < clips.Count; clipIndex++)
                {
                    if (clips[clipIndex].name.IndexOf(keywords[keywordIndex], StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return clips[clipIndex];
                    }
                }
            }

            return null;
        }

        private static AnimationClip FirstClip(IReadOnlyList<AnimationClip> clips)
        {
            return clips.Count > 0 ? clips[0] : null;
        }

        private static string FormatClipSummary(
            int sourceClipCount,
            AnimationClip idle,
            AnimationClip move,
            AnimationClip attack,
            AnimationClip hit,
            AnimationClip death)
        {
            return $"source clips {sourceClipCount}; idle `{ClipName(idle)}`, move `{ClipName(move)}`, attack `{ClipName(attack)}`, hit `{ClipName(hit)}`, death `{ClipName(death)}`";
        }

        private static string ClipName(AnimationClip clip)
        {
            return clip != null ? clip.name : "missing";
        }

        private static IEnumerable<string> UniqueClipNames(IReadOnlyList<AnimationClip> clips)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < clips.Count; i++)
            {
                if (clips[i] != null && seen.Add(clips[i].name))
                {
                    yield return $"`{clips[i].name}`";
                }
            }
        }

        private static string ResolveRigNote(MonsterSpec spec)
        {
            var importer = AssetImporter.GetAtPath(spec.FbxPath) as ModelImporter;
            if (importer == null)
            {
                return "model importer unavailable";
            }

            return $"{importer.animationType} rig, importAnimation={importer.importAnimation}";
        }

        private static string ResolveAttackNotes(MonsterSpec spec)
        {
            if (spec.Boss)
            {
                return "Existing boss pattern stack handles slam, charge, and add-spawn; contact is only a close-range fallback.";
            }

            if (spec.AddRangedShooterController)
            {
                return $"Uses `{ProjectilePrefabPath}` and line windup telegraph; shared contact damage is disabled.";
            }

            if (spec.Archetype == EnemyArchetype.ShieldEnemy)
            {
                return "Higher HP, slower speed, higher knockback resistance, and telegraphed windup.";
            }

            if (spec.Archetype == EnemyArchetype.FastCharger)
            {
                return "Small collider, low HP, quick cooldown, and low budget cost for swarm pressure.";
            }

            return "Baseline GreenDemon-style chase, movement, contact damage, pooling, and XP contract.";
        }

        private static void AddControllerParameters(AnimatorController controller)
        {
            foreach (var parameter in ControllerParameters)
            {
                var type = parameter == CharacterAnimationDriver.MoveSpeedParameter
                    ? AnimatorControllerParameterType.Float
                    : parameter.StartsWith("Is", StringComparison.Ordinal)
                        ? AnimatorControllerParameterType.Bool
                        : AnimatorControllerParameterType.Trigger;
                controller.AddParameter(parameter, type);
            }
        }

        private static AnimatorState AddState(AnimatorStateMachine stateMachine, string name, Motion motion, Vector3 position)
        {
            var state = stateMachine.AddState(name, position);
            state.motion = motion;
            state.speed = 1f;
            return state;
        }

        private static void AddBoolTransition(AnimatorState from, AnimatorState to, string parameterName, bool expectedValue, float duration)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = duration;
            transition.AddCondition(expectedValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, parameterName);
        }

        private static void AddAnyStateTriggerTransition(AnimatorStateMachine stateMachine, AnimatorState to, string triggerName, float duration)
        {
            var transition = stateMachine.AddAnyStateTransition(to);
            transition.canTransitionToSelf = false;
            transition.hasExitTime = false;
            transition.duration = duration;
            transition.AddCondition(AnimatorConditionMode.If, 0f, triggerName);
        }

        private static void AddTimedTransition(AnimatorState from, AnimatorState to, float exitTime, float duration)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = true;
            transition.exitTime = exitTime;
            transition.duration = duration;
        }

        private static Transform EnsureSocket(Transform parent, string name, Vector3 localPosition)
        {
            var child = EnsureChild(parent, name);
            child.localPosition = localPosition;
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;
            return child;
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

        private static void ClearChildren(Transform parent)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
            }
        }

        private static T EnsureComponent<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        private static Transform FindDeepChild(Transform root, string name)
        {
            if (root == null)
            {
                return null;
            }

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

        private static void ApplyEnemyLayer(GameObject root)
        {
            var enemyLayer = LayerMask.NameToLayer("Enemy");
            if (root == null || enemyLayer < 0)
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

        private static LayerMask ResolveLayerMask(string layerName, int fallback)
        {
            var layer = LayerMask.NameToLayer(layerName);
            return layer >= 0 ? 1 << layer : fallback;
        }

        private static void EnsureFolders()
        {
            EnsureFolder(ConfigRoot);
            EnsureFolder(BossConfigRoot);
            EnsureFolder(SpawnGroupRoot);
            EnsureFolder(PrefabRoot);
            EnsureFolder(ControllerRoot);
            EnsureFolder("Assets/_Project/Docs");
            EnsureFolder("Assets/_Project/Prefabs/Projectiles");
        }

        private static void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var normalized = folderPath.Replace('\\', '/');
            var parent = Path.GetDirectoryName(normalized)?.Replace('\\', '/');
            if (!string.IsNullOrWhiteSpace(parent))
            {
                EnsureFolder(parent);
            }

            var folderName = Path.GetFileName(normalized);
            AssetDatabase.CreateFolder(string.IsNullOrWhiteSpace(parent) ? "Assets" : parent, folderName);
        }

        private static void WriteTextAsset(string path, string contents)
        {
            var directory = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (!string.IsNullOrWhiteSpace(directory))
            {
                EnsureFolder(directory);
            }

            File.WriteAllText(path, contents);
        }

        private static void SetString(SerializedObject serialized, string propertyName, string value)
        {
            var property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.stringValue = value;
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

        private static void SetInt(SerializedObject serialized, string propertyName, int value)
        {
            var property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.intValue = value;
            }
        }

        private static void SetBool(SerializedObject serialized, string propertyName, bool value)
        {
            var property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.boolValue = value;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static bool GetBool(SerializedObject serialized, string propertyName, bool fallback)
        {
            var property = serialized.FindProperty(propertyName);
            return property != null ? property.boolValue : fallback;
        }

        private static void SetEnum(SerializedObject serialized, string propertyName, int value)
        {
            var property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.intValue = value;
            }
        }

        private static void SetObject(SerializedObject serialized, string propertyName, Object value)
        {
            var property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetLayerMask(SerializedObject serialized, string propertyName, LayerMask value)
        {
            var property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.intValue = value.value;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObjectArray(SerializedObject serialized, string propertyName, IReadOnlyList<Transform> values)
        {
            var property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                return;
            }

            property.arraySize = values.Count;
            for (var i = 0; i < values.Count; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetRelativeFloat(SerializedProperty parent, string propertyName, float value)
        {
            var property = parent.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.floatValue = value;
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

        private static MonsterSpec Deferred(string modelName, string displayName, string suggestedArchetype, string status)
        {
            return new MonsterSpec
            {
                ModelName = modelName,
                DisplayName = displayName,
                SuggestedArchetype = suggestedArchetype,
                IntegrationStatus = status,
                SelectedFirstPass = false
            };
        }

        public sealed class BuildSummary
        {
            public int ConfigCount;
            public int PrefabCount;
            public int ControllerCount;
            public int SpawnGroupCount;
            public int WaveAssetCount;
            public int BossAssetCount;
            public int WarningCount;
        }

        private sealed class GeneratedMonsterContent
        {
            public GeneratedMonsterContent(
                MonsterSpec spec,
                EnemyConfig config,
                GameObject prefab,
                RuntimeAnimatorController controller,
                string animationSummary)
            {
                Spec = spec;
                Config = config;
                Prefab = prefab;
                Controller = controller;
                AnimationSummary = animationSummary;
            }

            public MonsterSpec Spec { get; }
            public EnemyConfig Config { get; }
            public GameObject Prefab { get; }
            public RuntimeAnimatorController Controller { get; }
            public string AnimationSummary { get; }
            public string ControllerPath => Spec.ControllerPath;
        }

        private sealed class TimelineEntrySpec
        {
            public TimelineEntrySpec(float startTime, float endTime, float spawnInterval, int liveEnemyCap, float intensityMultiplier, params string[] groupKeys)
            {
                StartTime = startTime;
                EndTime = endTime;
                SpawnInterval = spawnInterval;
                LiveEnemyCap = liveEnemyCap;
                IntensityMultiplier = intensityMultiplier;
                GroupKeys = groupKeys;
            }

            public float StartTime { get; }
            public float EndTime { get; }
            public float SpawnInterval { get; }
            public int LiveEnemyCap { get; }
            public float IntensityMultiplier { get; }
            public string[] GroupKeys { get; }
        }

        private sealed class MonsterSpec
        {
            public string ModelName;
            public string DisplayName;
            public string EnemyId;
            public string ConfigName;
            public string PrefabName;
            public string SpawnGroupName;
            public EnemyArchetype Archetype = EnemyArchetype.MeleeChaser;
            public EnemyRank Rank = EnemyRank.Normal;
            public string SuggestedArchetype;
            public string IntegrationStatus;
            public float MaxHealth = 40f;
            public float MoveSpeed = 2f;
            public float Acceleration = 18f;
            public float RotationSpeed = 720f;
            public float StoppingDistance = 1f;
            public float ContactDamage = 8f;
            public float AttackRange = 1.2f;
            public float AttackCooldown = 1f;
            public float AttackWindup = 0.15f;
            public float ProjectileSpeed;
            public float ExplosionRadius = 1.5f;
            public int XpReward = 1;
            public float KnockbackResistance = 0.2f;
            public int BudgetCost = 1;
            public float Weight = 1f;
            public int MinCount = 1;
            public int MaxCount = 1;
            public int SpawnBurstCount = 1;
            public float VisualScale = 1f;
            public float ColliderRadius = 0.5f;
            public float ColliderHeight = 1.8f;
            public float ColliderCenterY = 0.9f;
            public float CombatHurtboxBodyTopPadding = 0.6f;
            public float MinimumCombatHurtboxRadius = 0.42f;
            public bool RequiresProjectileSpawnPoint;
            public bool AddRangedShooterController;
            public bool Boss;
            public bool Elite;
            public bool SelectedFirstPass;
            public bool ContactDamageEnabled => !AddRangedShooterController;
            public bool UseContactTelegraphWindup => Boss || Archetype == EnemyArchetype.ShieldEnemy;
            public string AttackArchetypeLabel
            {
                get
                {
                    if (Boss)
                    {
                        return "Boss pattern: slam, charge, add-spawn, plus close-range contact fallback";
                    }

                    if (AddRangedShooterController)
                    {
                        return "Ranged special: projectile windup, line telegraph, pooled projectile";
                    }

                    return Archetype == EnemyArchetype.ShieldEnemy
                        ? "Bruiser/tank: slower contact attack with telegraphed windup"
                        : Archetype == EnemyArchetype.FastCharger
                            ? "Fast swarm contact: quick low-damage pressure"
                            : "Basic contact chaser: readable melee/contact pressure";
                }
            }
            public string VfxFeedbackLabel => Boss
                ? "boss spawn/phase/heavy/death semantic VFX"
                : Elite
                    ? "elite spawn/death semantic VFX with normal hit flash"
                    : Archetype == EnemyArchetype.FastCharger
                        ? "small frequent hit/death feedback"
                        : Archetype == EnemyArchetype.ShieldEnemy
                            ? "heavier elite/large death feedback when ranked elite"
                            : "normal enemy hit/death feedback";
            public string WaveRole => Boss
                ? "Boss milestone via RunConfig bossSpawnGroup"
                : AddRangedShooterController
                    ? "Mid/late ranged pressure, low weight, single-count"
                    : Archetype == EnemyArchetype.ShieldEnemy
                        ? "Mid/late budget-heavy blocker"
                        : Archetype == EnemyArchetype.FastCharger
                            ? "Early small doses, more frequent later"
                            : "Early/mid baseline pressure";

            public string FbxPath => $"{FbxRoot}/{ModelName}.fbx";
            public string TexturePath => $"{TextureRoot}/{ModelName}_Texture.png";
            public string MaterialPath => $"{ObjRoot}/{ModelName}.mtl";
            public string ConfigPath => SelectedFirstPass ? $"{ConfigRoot}/{ConfigName}.asset" : string.Empty;
            public string PrefabPath => SelectedFirstPass ? $"{PrefabRoot}/{PrefabName}.prefab" : string.Empty;
            public string SpawnGroupPath => SelectedFirstPass ? $"{SpawnGroupRoot}/{SpawnGroupName}.asset" : string.Empty;
            public string ControllerPath => SelectedFirstPass ? $"{ControllerRoot}/AC_{PrefabName}.controller" : string.Empty;
        }
    }
}
#endif
