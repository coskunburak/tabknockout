#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using TapKnockout.Ability;
using TapKnockout.Boss;
using TapKnockout.Camera;
using TapKnockout.Characters;
using TapKnockout.Editor.Tools;
using TapKnockout.Enemy;
using TapKnockout.Feedback;
using TapKnockout.Input;
using TapKnockout.Pickups;
using TapKnockout.Player;
using TapKnockout.Projectile;
using TapKnockout.Survivor;
using TapKnockout.UI;
using TapKnockout.VFX;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace TapKnockout.Editor
{
    public static class SurvivorReferenceValidator
    {
        private const string DesktopSurvivorPrototypeScenePath = "Assets/_Project/Scenes/DesktopSurvivorPrototype.unity";

        [MenuItem("Tap Knockout/Survivor/Validate Prototype Scene")]
        public static void ValidatePrototypeSceneMenu()
        {
            var report = ValidateCurrentScene();
            report.LogToConsole();
        }

        [MenuItem("Tap Knockout/Survivor/Validate Desktop Survivor Prototype Scene")]
        public static void ValidateDesktopSurvivorPrototypeSceneMenu()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(DesktopSurvivorPrototypeScenePath) == null)
            {
                Debug.LogError($"Could not find prototype scene at {DesktopSurvivorPrototypeScenePath}.");
                return;
            }

            EditorSceneManager.OpenScene(DesktopSurvivorPrototypeScenePath, OpenSceneMode.Single);
            ValidatePrototypeSceneMenu();
        }

        [MenuItem("Tap Knockout/Survivor/Repair Prototype Scene")]
        public static void RepairPrototypeSceneMenu()
        {
            DesktopSurvivorPrototypeBuilder.RepairCurrentSceneEventSystem();
            DesktopSurvivorPrototypeBuilder.RepairCurrentSceneSurvivorRuntime();
            DesktopSurvivorPrototypeBuilder.RepairCurrentScenePlayerControls();
            EnemyBossPrefabReferenceRepairTool.RepairAllReferences();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("Repair Prototype Scene completed safe survivor wiring. Running validation report next.");
            ValidatePrototypeSceneMenu();
        }

        public static SurvivorValidationReport ValidateCurrentScene()
        {
            var report = new SurvivorValidationReport(EditorSceneManager.GetActiveScene().path);

            var runDirector = Object.FindFirstObjectByType<ArenaRunDirector>();
            var spawnDirector = Object.FindFirstObjectByType<SurvivorSpawnDirector>();
            var bossDirector = Object.FindFirstObjectByType<ArenaBossDirector>();
            var desktopInput = Object.FindFirstObjectByType<DesktopInputReader>();
            var inputBridge = Object.FindFirstObjectByType<DesktopSurvivorInputBridge>();
            var cameraRig = Object.FindFirstObjectByType<SurvivorCameraRig>();
            var xpController = Object.FindFirstObjectByType<PlayerXPController>();
            var pickupCollector = Object.FindFirstObjectByType<PickupCollector>();
            var hud = Object.FindFirstObjectByType<SurvivorHudController>();
            var enemyPool = Object.FindFirstObjectByType<EnemyPoolService>();
            var projectilePool = Object.FindFirstObjectByType<ProjectilePoolService>();
            var feedbackPlayer = Object.FindFirstObjectByType<SurvivorFeedbackPlayer>();
            var impactFeedback = Object.FindFirstObjectByType<ImpactFeedbackController>();
            var hitPauseService = Object.FindFirstObjectByType<HitPauseService>();
            var damageNumberSpawner = Object.FindFirstObjectByType<DamageNumberSpawner>();
            var cameraShakeReceiver = Object.FindFirstObjectByType<CameraShakeReceiver>();
            var vfxService = Object.FindFirstObjectByType<VFXService>();
            var combatVfx = Object.FindFirstObjectByType<CombatVFXEventController>();
            var stressTest = Object.FindFirstObjectByType<SurvivorStressTestController>();

            Require(report, runDirector, nameof(ArenaRunDirector));
            Require(report, spawnDirector, nameof(SurvivorSpawnDirector));
            Require(report, bossDirector, nameof(ArenaBossDirector));
            Require(report, desktopInput, nameof(DesktopInputReader));
            Require(report, inputBridge, nameof(DesktopSurvivorInputBridge));
            Require(report, cameraRig, nameof(SurvivorCameraRig));
            Require(report, xpController, nameof(PlayerXPController));
            Require(report, pickupCollector, nameof(PickupCollector));
            Require(report, hud, nameof(SurvivorHudController));
            Require(report, enemyPool, nameof(EnemyPoolService));

            if (projectilePool == null)
            {
                report.Warn("ProjectilePoolService is not present in the scene. Runtime can auto-create it, but explicit scene wiring is clearer for stress tests.");
            }

            if (feedbackPlayer == null)
            {
                report.Warn("SurvivorFeedbackPlayer is missing. Active skill/boss prefab VFX and direct AudioClip hooks will be skipped until it exists.");
            }

            if (impactFeedback == null)
            {
                report.Warn("ImpactFeedbackController is missing. Hit pause, hit flash, damage numbers, enemy death VFX/SFX hooks, and combat camera shake will be skipped.");
            }

            if (stressTest == null)
            {
                report.Warn("SurvivorStressTestController is missing. 100 enemy stress spawn can still be done manually, but no scene debug counters are exposed.");
            }

            ValidateCameraRig(report, cameraRig);
            ValidateRunDirector(report, runDirector);
            ValidateSpawnDirector(report, spawnDirector);
            ValidateFeedbackAndStress(
                report,
                feedbackPlayer,
                impactFeedback,
                hitPauseService,
                damageNumberSpawner,
                cameraShakeReceiver,
                vfxService,
                combatVfx,
                stressTest,
                projectilePool);
            ValidateHud(report, hud);
            ValidatePlayer(report);
            ValidateWeaponConfigs(report);
            ValidateForestArenaIfPresent(report);
            CuteMonsterEnemyContentBuilder.ValidateCuteMonsterContent(report);
            ValidateRunSpawnTimelineAlignment(report, runDirector, spawnDirector);
            return report;
        }

        private static void ValidateRunSpawnTimelineAlignment(
            SurvivorValidationReport report,
            ArenaRunDirector runDirector,
            SurvivorSpawnDirector spawnDirector)
        {
            if (runDirector == null || spawnDirector == null)
            {
                return;
            }

            var runConfig = ReadObject<RunConfig>(runDirector, "runConfig");
            var runTimeline = runConfig != null ? runConfig.WaveTimeline : null;
            var runTimelineOverride = ReadObject<WaveTimelineConfig>(runDirector, "waveTimelineOverride");
            var effectiveRunTimeline = runTimelineOverride != null ? runTimelineOverride : runTimeline;
            var spawnTimeline = ReadObject<WaveTimelineConfig>(spawnDirector, "waveTimeline");

            if (runTimeline != null && runTimelineOverride != null && runTimeline != runTimelineOverride)
            {
                report.Warn($"ArenaRunDirector.waveTimelineOverride is '{runTimelineOverride.name}' but RunConfig.waveTimeline is '{runTimeline.name}'. Clear or repair the override so Inspector and runtime spawning use the same enemy roster.");
            }

            if (effectiveRunTimeline != null && spawnTimeline != null && effectiveRunTimeline != spawnTimeline)
            {
                report.Warn($"ArenaRunDirector runtime timeline is '{effectiveRunTimeline.name}' but SurvivorSpawnDirector.waveTimeline is '{spawnTimeline.name}'. Run Repair Prototype Scene so runtime spawning uses the active run timeline.");
            }
        }

        private static void ValidateForestArenaIfPresent(SurvivorValidationReport report)
        {
            var scenePath = EditorSceneManager.GetActiveScene().path;
            var isForestScene = scenePath.EndsWith("DesktopSurvivorPrototype_ForestArena.unity");
            var hasForestRoot = GameObject.Find("ForestSurvivorArena") != null;
            if (!isForestScene && !hasForestRoot)
            {
                return;
            }

            var issues = ForestSurvivorArenaBuilder.ValidateForestArenaScene();
            for (var i = 0; i < issues.Count; i++)
            {
                var issue = issues[i];
                if (issue.Contains("missing") || issue.Contains("empty"))
                {
                    report.Error("[ForestArena] " + issue);
                }
                else
                {
                    report.Warn("[ForestArena] " + issue);
                }
            }
        }

        private static void ValidateCameraRig(SurvivorValidationReport report, SurvivorCameraRig cameraRig)
        {
            if (cameraRig == null)
            {
                return;
            }

            if (TryReadBool(cameraRig, "snapFollowToTarget", out var snapFollowToTarget) && !snapFollowToTarget)
            {
                report.Warn("SurvivorCameraRig.snapFollowToTarget is false. Enable it for the prototype so camera catch-up does not drag mouse-world aim toward the screen center.");
            }

            if (TryReadFloat(cameraRig, "followSharpness", out var followSharpness) && followSharpness > 0.001f)
            {
                report.Warn("SurvivorCameraRig.followSharpness is above 0. The prototype preset snaps follow to avoid reticle drift; run Repair if camera catch-up is visible.");
            }
        }

        private static void ValidateRunDirector(SurvivorValidationReport report, ArenaRunDirector runDirector)
        {
            if (runDirector == null)
            {
                return;
            }

            var runConfig = ReadObject<RunConfig>(runDirector, "runConfig");
            var spawnDirector = ReadObject<SurvivorSpawnDirector>(runDirector, "spawnDirector");
            var bossDirector = ReadObject<ArenaBossDirector>(runDirector, "bossDirector");
            var xpOrbPrefab = ReadObject<XPOrb>(runDirector, "xpOrbPrefab");

            RequireField(report, runConfig, "ArenaRunDirector.runConfig");
            RequireField(report, spawnDirector, "ArenaRunDirector.spawnDirector");
            RequireField(report, bossDirector, "ArenaRunDirector.bossDirector");

            if (xpOrbPrefab == null)
            {
                report.Warn("ArenaRunDirector.xpOrbPrefab is empty. XP will use direct fallback only if grantXPDirectlyWhenNoOrbPrefab is enabled.");
            }
            else
            {
                ValidateXPOrbPrefab(report, xpOrbPrefab);
            }

            if (runConfig == null)
            {
                return;
            }

            RequireField(report, runConfig.ArenaConfig, $"{runConfig.name}.arenaConfig");
            RequireField(report, runConfig.WaveTimeline, $"{runConfig.name}.waveTimeline");
            if (runConfig.BossSpawnTimeSeconds > 0f)
            {
                if (runConfig.BossSpawnGroup == null)
                {
                    report.Warn($"{runConfig.name}.bossSpawnGroup is empty. Boss milestone will warn but cannot spawn a boss.");
                }
                else
                {
                    ValidateSpawnGroup(report, runConfig.BossSpawnGroup, true);
                }
            }
        }

        private static void ValidateSpawnDirector(SurvivorValidationReport report, SurvivorSpawnDirector spawnDirector)
        {
            if (spawnDirector == null)
            {
                return;
            }

            var arenaConfig = ReadObject<ArenaConfig>(spawnDirector, "arenaConfig");
            var waveTimeline = ReadObject<WaveTimelineConfig>(spawnDirector, "waveTimeline");
            var playerTarget = ReadObject<Transform>(spawnDirector, "playerTarget");
            var enemyPool = ReadObject<EnemyPoolService>(spawnDirector, "enemyPoolService");

            RequireField(report, arenaConfig, "SurvivorSpawnDirector.arenaConfig");
            RequireField(report, waveTimeline, "SurvivorSpawnDirector.waveTimeline");
            RequireField(report, playerTarget, "SurvivorSpawnDirector.playerTarget");
            RequireField(report, enemyPool, "SurvivorSpawnDirector.enemyPoolService");

            ValidateSpawnSafetySettings(report, spawnDirector, arenaConfig);

            if (waveTimeline == null)
            {
                return;
            }

            if (waveTimeline.Entries == null || waveTimeline.Entries.Count == 0)
            {
                report.Warn($"{waveTimeline.name} has no wave entries. No regular enemies will spawn.");
                return;
            }

            var distinctSpawnGroups = new HashSet<SpawnGroupConfig>();
            for (var i = 0; i < waveTimeline.Entries.Count; i++)
            {
                var entry = waveTimeline.Entries[i];
                if (entry == null)
                {
                    report.Error($"{waveTimeline.name}.entries[{i}] is null.");
                    continue;
                }

                if (entry.SpawnGroups == null || entry.SpawnGroups.Count == 0)
                {
                    report.Warn($"{waveTimeline.name}.entries[{i}] has no spawn groups.");
                    continue;
                }

                for (var groupIndex = 0; groupIndex < entry.SpawnGroups.Count; groupIndex++)
                {
                    var group = entry.SpawnGroups[groupIndex];
                    if (group == null)
                    {
                        report.Error($"{waveTimeline.name}.entries[{i}].spawnGroups[{groupIndex}] is null.");
                        continue;
                    }

                    ValidateSpawnGroup(report, group, false);
                    distinctSpawnGroups.Add(group);
                }
            }

            if (distinctSpawnGroups.Count <= 1)
            {
                report.Warn($"{waveTimeline.name} only references {distinctSpawnGroups.Count} distinct spawn group(s). Prototype survivor spawning should reference the full cute monster roster, not only GreenDemon.");
            }
        }

        private static void ValidateSpawnSafetySettings(
            SurvivorValidationReport report,
            SurvivorSpawnDirector spawnDirector,
            ArenaConfig arenaConfig)
        {
            if (arenaConfig != null)
            {
                if (arenaConfig.PlayerAvoidSpawnRadius < arenaConfig.PlayerSafeSpawnRadius)
                {
                    report.Warn($"{arenaConfig.name}.playerAvoidSpawnRadius is lower than PlayerSafeSpawnRadius. OnValidate will repair it, but re-save the asset.");
                }

                if (arenaConfig.EnemySpawnMaxRadiusFromPlayer < arenaConfig.PlayerAvoidSpawnRadius)
                {
                    report.Warn($"{arenaConfig.name}.enemySpawnMaxRadiusFromPlayer is lower than playerAvoidSpawnRadius. Spawns may fall back to arena edge often.");
                }

                if (arenaConfig.SpawnPositionRetries < 4)
                {
                    report.Warn($"{arenaConfig.name}.spawnPositionRetries is very low. Use 12+ retries for reliable safe spawn placement.");
                }

                if (arenaConfig.SpawnPressureMode == SpawnPressureMode.EdgePressure &&
                    arenaConfig.EdgeSpawnInnerRadiusFactor < 0.65f)
                {
                    report.Warn($"{arenaConfig.name}.edgeSpawnInnerRadiusFactor is low for EdgePressure. Enemies may appear too close to the center.");
                }
            }

            if (spawnDirector == null)
            {
                return;
            }

            if (TryReadBool(spawnDirector, "enableSpawnTelegraph", out var telegraphEnabled) && telegraphEnabled)
            {
                if (TryReadFloat(spawnDirector, "spawnTelegraphDuration", out var duration) && duration <= 0f)
                {
                    report.Warn("SurvivorSpawnDirector.enableSpawnTelegraph is true, but spawnTelegraphDuration is 0. Enemies will appear immediately.");
                }

                if (TryReadInt(spawnDirector, "maxConcurrentSpawnTelegraphs", out var maxTelegraphs) && maxTelegraphs <= 0)
                {
                    report.Warn("SurvivorSpawnDirector.enableSpawnTelegraph is true, but maxConcurrentSpawnTelegraphs is 0. Telegraphs are effectively disabled.");
                }

                var spawnTelegraphPrefab = ReadObject<GameObject>(spawnDirector, "spawnTelegraphPrefab");
                if (spawnTelegraphPrefab == null)
                {
                    report.Warn("SurvivorSpawnDirector.spawnTelegraphPrefab is empty. Runtime LineRenderer marker fallback will be used until final spawn VFX is assigned.");
                }
                else
                {
                    ValidateSpawnTelegraphPrefab(report, spawnTelegraphPrefab);
                }
            }

            if (TryReadInt(spawnDirector, "baseLiveEnemyBudget", out var baseBudget) &&
                TryReadInt(spawnDirector, "maxLiveEnemyBudget", out var maxBudget) &&
                maxBudget < baseBudget)
            {
                report.Warn("SurvivorSpawnDirector.maxLiveEnemyBudget is lower than baseLiveEnemyBudget. OnValidate will clamp it; re-save the scene.");
            }
        }

        private static void ValidateXPOrbPrefab(SurvivorValidationReport report, XPOrb xpOrbPrefab)
        {
            if (xpOrbPrefab == null)
            {
                return;
            }

            var colliders = xpOrbPrefab.GetComponentsInChildren<Collider>(true);
            var hasTriggerCollider = false;
            var hasBlockingCollider = false;
            for (var i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] == null)
                {
                    continue;
                }

                hasTriggerCollider |= colliders[i].isTrigger;
                hasBlockingCollider |= !colliders[i].isTrigger;
            }

            if (!hasTriggerCollider)
            {
                report.Error($"{xpOrbPrefab.name} has no trigger Collider. XP orbs must be collectable without blocking movement.");
            }

            if (hasBlockingCollider)
            {
                report.Warn($"{xpOrbPrefab.name} has a non-trigger Collider. XP orbs should not block player or enemy movement.");
            }

            if (xpOrbPrefab.GetComponentInChildren<Renderer>(true) == null)
            {
                report.Warn($"{xpOrbPrefab.name} has no Renderer. The XP reward path works, but the pickup will be invisible.");
            }
        }

        private static void ValidateSpawnTelegraphPrefab(SurvivorValidationReport report, GameObject spawnTelegraphPrefab)
        {
            if (spawnTelegraphPrefab == null)
            {
                return;
            }

            if (spawnTelegraphPrefab.GetComponentInChildren<SpawnTelegraphMarker>(true) == null)
            {
                report.Warn($"{spawnTelegraphPrefab.name} has no SpawnTelegraphMarker. It can still be shown, but duration/pulse styling will not follow the spawn telegraph settings.");
            }

            if (spawnTelegraphPrefab.GetComponentInChildren<Renderer>(true) == null)
            {
                report.Warn($"{spawnTelegraphPrefab.name} has no Renderer. Spawn telegraphs will be pooled but invisible.");
            }

            if (spawnTelegraphPrefab.GetComponentInChildren<ParticleSystem>(true) == null)
            {
                report.Warn($"{spawnTelegraphPrefab.name} has no ParticleSystem child. The timing marker works, but run Repair Prototype Scene to add the selected imported magic-circle VFX overlay.");
            }

            var colliders = spawnTelegraphPrefab.GetComponentsInChildren<Collider>(true);
            if (colliders.Length > 0)
            {
                report.Warn($"{spawnTelegraphPrefab.name} has Collider components. Spawn warning visuals should not block raycasts, targeting, or mouse aim.");
            }

            var ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
            if (ignoreRaycastLayer >= 0 && spawnTelegraphPrefab.layer != ignoreRaycastLayer)
            {
                report.Warn($"{spawnTelegraphPrefab.name} root layer is not Ignore Raycast. Spawn warning visuals should avoid gameplay raycasts.");
            }
        }

        private static void ValidateDamageNumberPrefab(SurvivorValidationReport report, DamageNumberView numberPrefab)
        {
            if (numberPrefab == null)
            {
                return;
            }

            if (numberPrefab.GetComponent<RectTransform>() == null)
            {
                report.Error($"{numberPrefab.name} has no RectTransform. DamageNumberView prefabs must be UI objects.");
            }

            if (numberPrefab.GetComponentInChildren<Text>(true) == null)
            {
                report.Warn($"{numberPrefab.name} has no Text child. Damage numbers will spawn but no digits will be visible.");
            }

            if (numberPrefab.GetComponent<CanvasGroup>() == null)
            {
                report.Warn($"{numberPrefab.name} has no CanvasGroup. Damage numbers will work, but fade-out feedback will be skipped.");
            }
        }

        private static void ValidateSpawnGroup(SurvivorValidationReport report, SpawnGroupConfig group, bool bossGroup)
        {
            RequireField(report, group.EnemyConfig, $"{group.name}.enemyConfig");
            RequireField(report, group.EnemyPrefab, $"{group.name}.enemyPrefab");
            if (group.EnemyPrefab == null)
            {
                return;
            }

            if (group.EnemyPrefab.GetComponentInChildren<EnemyHealth>(true) == null)
            {
                report.Error($"{group.name}.enemyPrefab '{group.EnemyPrefab.name}' has no EnemyHealth component.");
            }

            if (group.EnemyPrefab.GetComponentInChildren<EnemyMovement>(true) == null)
            {
                report.Error($"{group.name}.enemyPrefab '{group.EnemyPrefab.name}' has no EnemyMovement component.");
            }

            if (group.EnemyPrefab.GetComponentInChildren<EnemyAttackController>(true) == null)
            {
                report.Error($"{group.name}.enemyPrefab '{group.EnemyPrefab.name}' has no EnemyAttackController component.");
            }

            if (group.EnemyPrefab.GetComponentInChildren<EnemyController>(true) == null)
            {
                report.Warn($"{group.name}.enemyPrefab '{group.EnemyPrefab.name}' has no EnemyController. Direct EnemyHealth initialization fallback will be used.");
            }

            if (group.EnemyPrefab.GetComponentInChildren<PooledEnemy>(true) == null)
            {
                report.Warn($"{group.name}.enemyPrefab '{group.EnemyPrefab.name}' has no PooledEnemy. Runtime pool can add it, but prefab contract expects it.");
            }

            if (group.EnemyPrefab.GetComponentInChildren<HitFlashController>(true) == null)
            {
                report.Warn($"{group.name}.enemyPrefab '{group.EnemyPrefab.name}' has no HitFlashController. ImpactFeedbackController can still run, but enemy hit flash will be skipped.");
            }

            if (group.EnemyPrefab.GetComponentInChildren<KnockbackReceiver>(true) == null)
            {
                report.Warn($"{group.name}.enemyPrefab '{group.EnemyPrefab.name}' has no KnockbackReceiver. Dash and impact knockback will be ignored unless another receiver handles it.");
            }

            ValidateEnemyPrefabLifecycleContract(report, group, group.EnemyPrefab);

            if (bossGroup && group.EnemyPrefab.GetComponentInChildren<BossPhaseController>(true) == null)
            {
                report.Warn($"{group.name}.boss prefab has no BossPhaseController. Boss health bar can still bind health, but phase pacing will be limited.");
            }
        }

        private static void ValidateEnemyPrefabLifecycleContract(SurvivorValidationReport report, SpawnGroupConfig group, GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            var health = prefab.GetComponentInChildren<EnemyHealth>(true);
            if (health != null)
            {
                if (ReadObject<Transform>(health, "targetTransform") == null)
                {
                    report.Warn($"{group.name}.enemyPrefab '{prefab.name}' EnemyHealth.targetTransform is empty. Runtime falls back to root transform, but explicit self-target keeps aim sockets predictable.");
                }

                if (TryReadBool(health, "targetableWhenAlive", out var targetableWhenAlive) && !targetableWhenAlive)
                {
                    report.Warn($"{group.name}.enemyPrefab '{prefab.name}' EnemyHealth.targetableWhenAlive is false. PlayerTargetProvider will skip this enemy while alive.");
                }
            }

            var hasPoolLifecycle = false;
            var behaviours = prefab.GetComponentsInChildren<MonoBehaviour>(true);
            for (var i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is TapKnockout.Combat.IPoolLifecycle)
                {
                    hasPoolLifecycle = true;
                    break;
                }
            }

            if (!hasPoolLifecycle)
            {
                report.Warn($"{group.name}.enemyPrefab '{prefab.name}' has no IPoolLifecycle components. Health, movement, attack, hit flash, and knockback state may persist after pooling.");
            }

            var colliders = prefab.GetComponentsInChildren<Collider>(true);
            if (colliders.Length == 0)
            {
                report.Error($"{group.name}.enemyPrefab '{prefab.name}' has no Collider. It cannot receive hits or block movement correctly.");
            }
            else
            {
                var hasEnabledCollider = false;
                for (var i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i] != null && colliders[i].enabled)
                    {
                        hasEnabledCollider = true;
                        break;
                    }
                }

                if (!hasEnabledCollider)
                {
                    report.Warn($"{group.name}.enemyPrefab '{prefab.name}' has Collider components, but all are disabled in the prefab. Pool spawn reset will enable them, but prefab defaults should be enabled.");
                }
            }

            var enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer >= 0)
            {
                if (prefab.layer != enemyLayer)
                {
                    report.Warn($"{group.name}.enemyPrefab '{prefab.name}' root layer is not Enemy. Repair generated prefabs so targeting and hit masks stay consistent.");
                }

                for (var i = 0; i < colliders.Length; i++)
                {
                    var collider = colliders[i];
                    if (collider != null && collider.gameObject.layer != enemyLayer)
                    {
                        report.Warn($"{group.name}.enemyPrefab '{prefab.name}' collider '{collider.name}' is not on Enemy layer.");
                        break;
                    }
                }
            }
        }

        private static void ValidateHud(SurvivorValidationReport report, SurvivorHudController hud)
        {
            if (hud == null)
            {
                return;
            }

            var activeSkills = Object.FindFirstObjectByType<ActiveSkillController>();
            if (activeSkills != null && ReadObject<ActiveSkillController>(hud, "activeSkillController") != activeSkills)
            {
                report.Warn("SurvivorHudController.activeSkillController should point to Player.ActiveSkillController so Q/E/R/F cooldown events reach the HUD.");
            }

            var activeSkillSlots = Object.FindObjectsByType<ActiveSkillSlotHud>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (activeSkillSlots == null || activeSkillSlots.Length == 0)
            {
                report.Warn("No ActiveSkillSlotHud entries found. Skill cooldowns will be visible only in Inspector unless HUD slots are assigned.");
            }
            else if (activeSkillSlots.Length < 4)
            {
                report.Warn($"Only {activeSkillSlots.Length} ActiveSkillSlotHud entries found. Prototype HUD should expose Q/E/R/F.");
            }

            var hudSlots = new SerializedObject(hud).FindProperty("activeSkillSlots");
            if (hudSlots == null || !hudSlots.isArray || hudSlots.arraySize < 4)
            {
                report.Warn("SurvivorHudController.activeSkillSlots should contain 4 assigned slot views for Q/E/R/F.");
            }
            else
            {
                for (var i = 0; i < 4; i++)
                {
                    if (hudSlots.GetArrayElementAtIndex(i).objectReferenceValue == null)
                    {
                        report.Warn($"SurvivorHudController.activeSkillSlots[{i}] is empty.");
                    }
                }
            }

            var bossHealthBar = Object.FindFirstObjectByType<BossHealthBarController>();
            if (bossHealthBar == null)
            {
                report.Warn("No BossHealthBarController found. Boss events will fire, but visible boss HP needs a HUD object.");
            }
            else
            {
                if (bossHealthBar.GetComponentInParent<Canvas>(true) == null)
                {
                    report.Warn("BossHealthBarController is not under a Canvas. Boss health may update but will not be visible.");
                }

                if (ReadObject<Slider>(bossHealthBar, "healthSlider") == null)
                {
                    report.Warn("BossHealthBarController.healthSlider is empty. Boss health events can bind, but no fill bar will update.");
                }
            }
        }

        private static void ValidateFeedbackAndStress(
            SurvivorValidationReport report,
            SurvivorFeedbackPlayer feedbackPlayer,
            ImpactFeedbackController impactFeedback,
            HitPauseService hitPauseService,
            DamageNumberSpawner damageNumberSpawner,
            CameraShakeReceiver cameraShakeReceiver,
            VFXService vfxService,
            CombatVFXEventController combatVfx,
            SurvivorStressTestController stressTest,
            ProjectilePoolService projectilePool)
        {
            ValidateVFXServiceAndCatalog(report, vfxService, combatVfx);

            if (feedbackPlayer != null)
            {
                var vfxPoolRoot = ReadObject<Transform>(feedbackPlayer, "vfxPoolRoot");
                var feedbackCameraShakeReceiver = ReadObject<CameraShakeReceiver>(feedbackPlayer, "cameraShakeReceiver");

                if (vfxPoolRoot == null)
                {
                    report.Warn("SurvivorFeedbackPlayer.vfxPoolRoot is empty. Runtime will use its own transform, but a DebugRoot/VFXPoolRoot child keeps pooled VFX organized.");
                }

                if (feedbackCameraShakeReceiver == null)
                {
                    report.Warn("SurvivorFeedbackPlayer.cameraShakeReceiver is empty. VFX/SFX still work, but active skill and boss camera shake will be skipped.");
                }
            }

            if (impactFeedback != null)
            {
                if (ReadObject<VFXService>(impactFeedback, "vfxService") == null && vfxService == null)
                {
                    report.Warn("ImpactFeedbackController.vfxService is empty. Combat/death VFX events will fire but no VFXService will spawn effects.");
                }

                if (ReadObject<HitPauseService>(impactFeedback, "hitPauseService") == null && hitPauseService == null)
                {
                    report.Warn("ImpactFeedbackController.hitPauseService is empty. Hit stop requests will be skipped.");
                }

                if (ReadObject<CameraShakeReceiver>(impactFeedback, "cameraShakeReceiver") == null && cameraShakeReceiver == null)
                {
                    report.Warn("ImpactFeedbackController.cameraShakeReceiver is empty. Combat camera shake will be skipped.");
                }

                if (ReadObject<DamageNumberSpawner>(impactFeedback, "damageNumberSpawner") == null && damageNumberSpawner == null)
                {
                    report.Warn("ImpactFeedbackController.damageNumberSpawner is empty. Damage numbers will be skipped even when enabled in config.");
                }

                var impactConfig = ReadObject<ImpactFeedbackConfig>(impactFeedback, "config");
                if (impactConfig == null)
                {
                    report.Warn("ImpactFeedbackController.config is empty. Runtime defaults work, but assign an ImpactFeedbackConfig to tune hit pause, camera shake, and damage-number thresholds.");
                }
                else
                {
                    ValidateImpactFeedbackProfiles(report, impactConfig);
                }
            }

            if (damageNumberSpawner != null)
            {
                if (ReadObject<Canvas>(damageNumberSpawner, "targetCanvas") == null)
                {
                    report.Warn("DamageNumberSpawner.targetCanvas is empty. It can infer a parent Canvas, but explicit assignment is more reliable.");
                }

                var numberPrefab = ReadObject<DamageNumberView>(damageNumberSpawner, "numberPrefab");
                if (numberPrefab == null)
                {
                    report.Warn("DamageNumberSpawner.numberPrefab is empty. Runtime Text fallback will be used; assign a DamageNumberView prefab for final styling.");
                }
                else
                {
                    ValidateDamageNumberPrefab(report, numberPrefab);
                }
            }

            if (stressTest == null)
            {
                return;
            }

            RequireField(report, ReadObject<SurvivorSpawnDirector>(stressTest, "spawnDirector"), "SurvivorStressTestController.spawnDirector");
            RequireField(report, ReadObject<EnemyPoolService>(stressTest, "enemyPoolService"), "SurvivorStressTestController.enemyPoolService");
            RequireField(report, ReadObject<ArenaRunDirector>(stressTest, "runDirector"), "SurvivorStressTestController.runDirector");
            RequireField(report, ReadObject<ArenaBossDirector>(stressTest, "bossDirector"), "SurvivorStressTestController.bossDirector");

            if (ReadObject<ProjectilePoolService>(stressTest, "projectilePoolService") == null && projectilePool == null)
            {
                report.Warn("SurvivorStressTestController.projectilePoolService is empty. Projectile counters will stay at zero until a ProjectilePoolService exists.");
            }

            if (ReadObject<SpawnGroupConfig>(stressTest, "stressSpawnGroup") == null)
            {
                report.Warn("SurvivorStressTestController.stressSpawnGroup is empty. Assign a non-boss SpawnGroupConfig before using the Spawn Stress Enemies context menu.");
            }
        }

        private static void ValidateVFXServiceAndCatalog(
            SurvivorValidationReport report,
            VFXService vfxService,
            CombatVFXEventController combatVfx)
        {
            if (vfxService == null)
            {
                report.Warn("VFXService is missing. Impact, projectile trail, dash, boss, XP, and catalog-driven combat VFX will not spawn.");
                return;
            }

            var catalog = ReadObject<VFXCatalog>(vfxService, "catalog");
            if (catalog == null)
            {
                report.Warn("VFXService.catalog is empty. Run Tools/Tap Knockout/VFX/Create Feedback System Root or Tap Knockout/Survivor/Repair Prototype Scene to assign the vertical-slice catalog.");
            }
            else
            {
                ValidateCatalogEvent(report, catalog, VFXEventType.PrimaryFireMuzzle, true);
                ValidateCatalogEvent(report, catalog, VFXEventType.PrimaryProjectileTrail, true);
                ValidateCatalogEvent(report, catalog, VFXEventType.PrimaryProjectileImpact, true);
                ValidateCatalogEvent(report, catalog, VFXEventType.ForwardCleaveCast, true);
                ValidateCatalogEvent(report, catalog, VFXEventType.ForwardCleaveHit, true);
                ValidateCatalogEvent(report, catalog, VFXEventType.GroundImpactArea, true);
                ValidateCatalogEvent(report, catalog, VFXEventType.DashStart, true);
                ValidateCatalogEvent(report, catalog, VFXEventType.DashTrail, true);
                ValidateCatalogEvent(report, catalog, VFXEventType.DashEnd, true);
                ValidateCatalogEvent(report, catalog, VFXEventType.EnemyHit, true);
                ValidateCatalogEvent(report, catalog, VFXEventType.EnemyDeath, true);
                ValidateCatalogEvent(report, catalog, VFXEventType.EnemyDeathLarge, true);
                ValidateCatalogEvent(report, catalog, VFXEventType.SpawnTelegraph, true);
                ValidateCatalogEvent(report, catalog, VFXEventType.EliteSpawn, true);
                ValidateCatalogEvent(report, catalog, VFXEventType.EliteDeath, true);
                ValidateCatalogEvent(report, catalog, VFXEventType.BossSpawnWarning, false);
                ValidateCatalogEvent(report, catalog, VFXEventType.BossPhaseTransition, false);
                ValidateCatalogEvent(report, catalog, VFXEventType.BossHeavyAttackTelegraph, false);
                ValidateCatalogEvent(report, catalog, VFXEventType.BossHeavyAttackImpact, false);
                ValidateCatalogEvent(report, catalog, VFXEventType.BossDeath, false);
                ValidateCatalogEvent(report, catalog, VFXEventType.XPOrbCollect, false);
                ValidateCatalogEvent(report, catalog, VFXEventType.LevelUpBurst, false);
                ValidateCatalogEvent(report, catalog, VFXEventType.ReticleFirePulse, false);
            }

            if (combatVfx == null)
            {
                report.Warn("CombatVFXEventController is missing. Projectile trails, dash start/end/trail, active skill catalog VFX, elite/boss, XP, and level-up VFX will be skipped.");
                return;
            }

            var controllerService = ReadObject<VFXService>(combatVfx, "vfxService");
            if (controllerService == null)
            {
                report.Warn("CombatVFXEventController.vfxService is empty. It can resolve a scene service at runtime, but explicit wiring avoids missed early events.");
            }
            else if (controllerService != vfxService)
            {
                report.Warn("CombatVFXEventController.vfxService points to a different VFXService than the first scene VFXService. Keep one feedback root for predictable pooling.");
            }
        }

        private static void ValidateCatalogEvent(
            SurvivorValidationReport report,
            VFXCatalog catalog,
            VFXEventType eventType,
            bool critical)
        {
            if (!catalog.TryGetDefinition(eventType, out var definition))
            {
                var severity = critical ? "critical" : "optional";
                report.Warn($"{catalog.name} has no {severity} VFX definition for {eventType}.");
                return;
            }

            if (definition.Prefab == null)
            {
                var severity = critical ? "critical" : "optional";
                report.Warn($"{catalog.name}.{eventType} has no prefab assigned. Missing VFX is safe but this {severity} event will be visually silent.");
                return;
            }

            ValidateCatalogPrefab(report, definition.Prefab, eventType);
        }

        private static void ValidateCatalogPrefab(SurvivorValidationReport report, GameObject prefab, VFXEventType eventType)
        {
            var colliders = prefab.GetComponentsInChildren<Collider>(true);
            if (colliders.Length > 0)
            {
                report.Warn($"{prefab.name} is mapped to {eventType} and has Collider components. Combat VFX prefabs should not block movement, targeting, or mouse aim.");
            }

            if (prefab.GetComponentInChildren<ParticleSystem>(true) == null &&
                prefab.GetComponentInChildren<Renderer>(true) == null)
            {
                report.Warn($"{prefab.name} is mapped to {eventType} but has no ParticleSystem or Renderer. The effect may be invisible.");
            }
        }

        private static void ValidateImpactFeedbackProfiles(SurvivorValidationReport report, ImpactFeedbackConfig config)
        {
            if (!config.EnableDamageNumbers)
            {
                report.Warn($"{config.name}.enableDamageNumbers is false. Enable it for projectile, skill, dash, boss, and player-damage profile numbers.");
            }

            if (!config.EnableHitFlash || !config.EnableVFX || !config.EnableSFXHooks)
            {
                report.Warn($"{config.name} has hit flash, VFX, or SFX hooks disabled globally. Profile settings cannot play those channels until the global toggles are enabled.");
            }

            foreach (ImpactFeedbackProfileId profileId in System.Enum.GetValues(typeof(ImpactFeedbackProfileId)))
            {
                if (!config.HasProfile(profileId))
                {
                    report.Warn($"{config.name} is missing serialized {profileId} profile. Runtime defaults will be used, but run Tools/Tap Knockout/VFX/Create Feedback System Root or Repair Prototype Scene to persist the profile.");
                }
            }

            var normalProjectile = config.GetProfile(ImpactFeedbackProfileId.NormalProjectileHit);
            if (normalProjectile.ApplyHitStop || normalProjectile.ApplyCameraShake)
            {
                report.Warn($"{config.name}.NormalProjectileHit applies hit stop or camera shake. Disable both for tiny rapid projectile hits unless this is an intentional weapon-specific override.");
            }

            var heavyProjectile = config.GetProfile(ImpactFeedbackProfileId.HeavyProjectileHit);
            if (heavyProjectile.ApplyHitStop && heavyProjectile.HitStopCooldown <= 0f)
            {
                report.Warn($"{config.name}.HeavyProjectileHit hit stop has no cooldown. Add a cooldown so heavy multishot bursts do not stack freeze frames.");
            }

            var skill = config.GetProfile(ImpactFeedbackProfileId.SkillHit);
            if (!skill.ApplyHitStop || skill.HitStopCooldown <= 0f || !skill.ApplyCameraShake || skill.CameraShakeCooldown <= 0f)
            {
                report.Warn($"{config.name}.SkillHit should use rate-limited hit stop and camera shake so area skills feel strong without spamming feedback per target.");
            }

            var dash = config.GetProfile(ImpactFeedbackProfileId.DashImpact);
            if (!dash.ApplyHitStop || !dash.ApplyCameraShake)
            {
                report.Warn($"{config.name}.DashImpact should keep both hit stop and camera shake enabled for physical dash impact feel.");
            }

            var death = config.GetProfile(ImpactFeedbackProfileId.EnemyDeath);
            if (!death.SpawnVFX || !death.PlaySFX)
            {
                report.Warn($"{config.name}.EnemyDeath should keep VFX and SFX hooks enabled; XP reward logic is separate and already guarded by ArenaRunDirector.");
            }

            var shot = config.GetProfile(ImpactFeedbackProfileId.ShotFired);
            if (!shot.PulseReticle || shot.ApplyHitStop)
            {
                report.Warn($"{config.name}.ShotFired should pulse the reticle and should not apply hit stop.");
            }

            if (!shot.SpawnVFX || shot.VFXEvent != VFXEventType.PrimaryFireMuzzle)
            {
                report.Warn($"{config.name}.ShotFired should spawn {VFXEventType.PrimaryFireMuzzle} so manual primary fire has a muzzle VFX without using legacy projectile-spawn mapping.");
            }

            if (config.ReticleFirePulseVFX != VFXEventType.ReticleFirePulse)
            {
                report.Warn($"{config.name}.reticleFirePulseVFX should use {VFXEventType.ReticleFirePulse} unless a designer intentionally remaps reticle shot feedback.");
            }
        }

        private static void ValidatePlayer(SurvivorValidationReport report)
        {
            var movement = Object.FindFirstObjectByType<PlayerMovementController>();
            if (movement == null)
            {
                report.Error("No PlayerMovementController found.");
                return;
            }

            var player = movement.gameObject;
            var desktopInput = player.GetComponent<DesktopInputReader>();
            var inputBridge = player.GetComponent<DesktopSurvivorInputBridge>();
            var attack = player.GetComponent<PlayerAttackController>();
            var dash = player.GetComponent<PlayerDashController>();
            var targetProvider = player.GetComponent<PlayerTargetProvider>();
            var mouseAim = player.GetComponent<MouseAimController>();
            var aimReticle = player.GetComponent<MouseAimReticleController>();
            var activeSkills = player.GetComponent<ActiveSkillController>();
            var playerHealth = player.GetComponent<PlayerHealth>();
            var runtimeStats = player.GetComponent<PlayerRuntimeStats>();
            var animationDriver = player.GetComponentInChildren<CharacterAnimationDriver>(true);
            var xpController = player.GetComponent<PlayerXPController>();
            var pickupCollector = player.GetComponent<PickupCollector>();

            Require(report, desktopInput, "Player.DesktopInputReader");
            Require(report, inputBridge, "Player.DesktopSurvivorInputBridge");
            Require(report, attack, "Player.PlayerAttackController");
            Require(report, dash, "Player.PlayerDashController");
            Require(report, targetProvider, "Player.PlayerTargetProvider");
            Require(report, mouseAim, "Player.MouseAimController");
            Require(report, aimReticle, "Player.MouseAimReticleController");
            Require(report, activeSkills, "Player.ActiveSkillController");
            Require(report, playerHealth, "Player.PlayerHealth");
            Require(report, runtimeStats, "Player.PlayerRuntimeStats");
            Require(report, animationDriver, "Player.CharacterAnimationDriver");
            Require(report, xpController, "Player.PlayerXPController");
            Require(report, pickupCollector, "Player.PickupCollector");

            if (desktopInput != null && ReadObject<MonoBehaviour>(movement, "inputSourceBehaviour") != desktopInput)
            {
                report.Warn("PlayerMovementController.inputSourceBehaviour should point to Player.DesktopInputReader for desktop WASD movement.");
            }

            if (TryReadBool(movement, "rotateTowardMovement", out var rotateTowardMovement) && rotateTowardMovement)
            {
                report.Warn("PlayerMovementController.rotateTowardMovement is enabled. For mouse-aim survivor control, this can fight MouseAimController rotation.");
            }

            ValidateMouseAim(report, mouseAim);
            ValidatePlayerAttack(report, attack, movement, desktopInput, targetProvider, mouseAim, aimReticle, playerHealth, runtimeStats);
            ValidateAimReticle(report, aimReticle, mouseAim, desktopInput);
            ValidatePlayerDash(report, dash, movement, mouseAim);
            ValidateActiveSkills(report, activeSkills, inputBridge, movement, runtimeStats, mouseAim, playerHealth);
            ValidatePlayerAnimation(report, animationDriver, movement, attack, dash, playerHealth);
        }

        private static void ValidateMouseAim(SurvivorValidationReport report, MouseAimController mouseAim)
        {
            if (mouseAim == null)
            {
                return;
            }

            if (TryReadBool(mouseAim, "preferStableGroundPlane", out var preferStableGroundPlane) &&
                !preferStableGroundPlane)
            {
                report.Warn("MouseAimController.preferStableGroundPlane is false. Enable it so mouse aim does not jump onto enemy, player, or prop colliders.");
            }

            if (TryReadBool(mouseAim, "usePhysicsRaycast", out var usePhysicsRaycast) && usePhysicsRaycast)
            {
                report.Warn("MouseAimController.usePhysicsRaycast is enabled. Disable it for the prototype so aim resolves against the stable ground plane instead of gameplay colliders.");
            }

            if (TryReadFloat(mouseAim, "minAimDirectionDistance", out var minAimDirectionDistance) &&
                minAimDirectionDistance < 0.2f)
            {
                report.Warn("MouseAimController.minAimDirectionDistance is below 0.2. Raise it so aim direction does not tremble when the cursor passes near the player center.");
            }

            if (TryReadBool(mouseAim, "rotateRigidbodyInFixedUpdate", out var rotateRigidbodyInFixedUpdate) &&
                !rotateRigidbodyInFixedUpdate)
            {
                report.Warn("MouseAimController.rotateRigidbodyInFixedUpdate is false. Rigidbody-facing rotation should stay in FixedUpdate to avoid transform/physics jitter.");
            }
        }

        private static void ValidatePlayerAttack(
            SurvivorValidationReport report,
            PlayerAttackController attack,
            PlayerMovementController movement,
            DesktopInputReader desktopInput,
            PlayerTargetProvider targetProvider,
            MouseAimController mouseAim,
            MouseAimReticleController aimReticle,
            PlayerHealth playerHealth,
            PlayerRuntimeStats runtimeStats)
        {
            if (attack == null)
            {
                return;
            }

            if (ReadObject<PlayerMovementController>(attack, "movementController") != movement)
            {
                report.Warn("PlayerAttackController.movementController should point to Player.PlayerMovementController.");
            }

            if (playerHealth != null && ReadObject<PlayerHealth>(attack, "playerHealth") != playerHealth)
            {
                report.Warn("PlayerAttackController.playerHealth should point to Player.PlayerHealth so dead players cannot keep firing.");
            }

            if (desktopInput != null && ReadObject<DesktopInputReader>(attack, "desktopInputReader") != desktopInput)
            {
                report.Warn("PlayerAttackController.desktopInputReader should point to Player.DesktopInputReader for hold-to-fire support.");
            }

            if (targetProvider != null && ReadObject<PlayerTargetProvider>(attack, "targetProvider") != targetProvider)
            {
                report.Warn("PlayerAttackController.targetProvider should point to Player.PlayerTargetProvider.");
            }

            if (mouseAim != null && ReadObject<MouseAimController>(attack, "mouseAimController") != mouseAim)
            {
                report.Warn("PlayerAttackController.mouseAimController should point to Player.MouseAimController.");
            }

            if (aimReticle != null && ReadObject<MouseAimReticleController>(attack, "aimReticle") != aimReticle)
            {
                report.Warn("PlayerAttackController.aimReticle should point to Player.MouseAimReticleController so successful shots can pulse the reticle.");
            }

            if (runtimeStats != null && ReadObject<PlayerRuntimeStats>(attack, "runtimeStats") != runtimeStats)
            {
                report.Warn("PlayerAttackController.runtimeStats should point to Player.PlayerRuntimeStats.");
            }

            if (TryReadBool(attack, "requireStationaryToAttack", out var requireStationary) && requireStationary)
            {
                report.Warn("PlayerAttackController.requireStationaryToAttack is enabled. Disable it for survivor move-and-shoot feel.");
            }

            if (TryReadBool(attack, "faceTargetOnAttack", out var faceTargetOnAttack) && faceTargetOnAttack)
            {
                report.Warn("PlayerAttackController.faceTargetOnAttack is enabled. MouseAimController should own desktop survivor facing to avoid rotation jitter.");
            }

            if (TryReadBool(attack, "preferMouseAimForProjectiles", out var preferMouseAim) && !preferMouseAim)
            {
                report.Warn("PlayerAttackController.preferMouseAimForProjectiles is disabled. Mouse-world aim projectiles will feel less predictable.");
            }

            if (TryReadBool(attack, "allowAimFallbackWithoutTarget", out var allowAimFallback) && !allowAimFallback)
            {
                report.Warn("PlayerAttackController.allowAimFallbackWithoutTarget is disabled. Primary attack will not fire when no enemy is targetable.");
            }

            if (TryReadBool(attack, "fallbackAttackWhileMoving", out var fallbackAttackWhileMoving) && !fallbackAttackWhileMoving)
            {
                report.Warn("PlayerAttackController.fallbackAttackWhileMoving is disabled. Assign PlayerConfig.attackWhileMoving=true or enable this fallback.");
            }

            if (TryReadBool(attack, "fallbackManualFireRequiresInput", out var fallbackManualFireRequiresInput) &&
                !fallbackManualFireRequiresInput)
            {
                report.Warn("PlayerAttackController.fallbackManualFireRequiresInput is disabled. Manual mouse fire should require left mouse input by default.");
            }

            var playerConfig = ReadObject<PlayerConfig>(attack, "playerConfig");
            if (playerConfig == null && movement != null)
            {
                playerConfig = movement.Config;
            }

            if (playerConfig != null)
            {
                ValidatePlayerConfigManualFire(report, playerConfig);
            }
            else if (TryReadEnum(attack, "firePolicy", out var firePolicy) &&
                firePolicy != (int)PrimaryAttackFirePolicy.HoldMouseAim)
            {
                report.Warn("PlayerAttackController.firePolicy should be HoldMouseAim for the desktop survivor prototype when no PlayerConfig override is assigned.");
            }

            var weapon = ReadObject<WeaponConfig>(attack, "weaponConfig");
            if (weapon != null && weapon.ProjectilePrefab != null && ReadObject<Transform>(attack, "projectileSpawnPoint") == null)
            {
                report.Warn("PlayerAttackController.projectileSpawnPoint is empty while weapon has a ProjectilePrefab. Add/assign ProjectileSpawnPoint for clean muzzle position.");
            }
        }

        private static void ValidatePlayerConfigManualFire(SurvivorValidationReport report, PlayerConfig config)
        {
            if (config == null)
            {
                return;
            }

            if (TryReadEnum(config, "primaryAttackFirePolicy", out var configFirePolicy) &&
                configFirePolicy != (int)PrimaryAttackFirePolicy.HoldMouseAim)
            {
                report.Warn($"{config.name}.primaryAttackFirePolicy should be HoldMouseAim for default desktop survivor manual fire. Legacy auto policies remain available for explicit tests.");
            }

            if (TryReadBool(config, "attackWhileMoving", out var attackWhileMoving) && !attackWhileMoving)
            {
                report.Warn($"{config.name}.attackWhileMoving is false. Set it true for survivor move-and-shoot feel.");
            }

            if (TryReadBool(config, "manualFireRequiresInput", out var manualFireRequiresInput) && !manualFireRequiresInput)
            {
                report.Warn($"{config.name}.manualFireRequiresInput is false. Default manual primary fire should require left mouse input.");
            }

            if (TryReadBool(config, "aimReticleEnabled", out var aimReticleEnabled) && !aimReticleEnabled)
            {
                report.Warn($"{config.name}.aimReticleEnabled is false. The desktop survivor prototype should show a world-space mouse reticle.");
            }

            if (TryReadBool(config, "hideSystemCursorDuringGameplay", out var hideSystemCursor) && !hideSystemCursor)
            {
                report.Warn($"{config.name}.hideSystemCursorDuringGameplay is false. Hide the system cursor so the blue world reticle is the only aim marker during gameplay.");
            }

            if (TryReadFloat(config, "aimReticleYOffset", out var reticleYOffset) && reticleYOffset < 0.1f)
            {
                report.Warn($"{config.name}.aimReticleYOffset is below 0.1. Raise it so the world reticle does not fade under the floor.");
            }

            if (TryReadFloat(config, "aimReticleSmoothTime", out var reticleSmoothTime) && reticleSmoothTime > 0.001f)
            {
                report.Warn($"{config.name}.aimReticleSmoothTime is above 0. Disable reticle smoothing for responsive mouse aim.");
            }
        }

        private static void ValidateAimReticle(
            SurvivorValidationReport report,
            MouseAimReticleController aimReticle,
            MouseAimController mouseAim,
            DesktopInputReader desktopInput)
        {
            if (aimReticle == null)
            {
                return;
            }

            if (mouseAim != null && ReadObject<MouseAimController>(aimReticle, "aimController") != mouseAim)
            {
                report.Warn("MouseAimReticleController.aimController should point to Player.MouseAimController.");
            }

            if (desktopInput != null && ReadObject<DesktopInputReader>(aimReticle, "inputReader") != desktopInput)
            {
                report.Warn("MouseAimReticleController.inputReader should point to Player.DesktopInputReader for while-firing visibility options.");
            }

            var reticlePrefab = ReadObject<GameObject>(aimReticle, "reticlePrefab");
            if (reticlePrefab == null &&
                TryReadBool(aimReticle, "allowRuntimeFallback", out var allowRuntimeFallback) &&
                !allowRuntimeFallback)
            {
                report.Error("MouseAimReticleController has no reticlePrefab and allowRuntimeFallback is disabled. Assign a reticle prefab or enable fallback.");
            }

            if (TryReadBool(aimReticle, "reticleEnabled", out var reticleEnabled) && !reticleEnabled)
            {
                report.Warn("MouseAimReticleController.reticleEnabled is false. Enable it for manual mouse aim readability.");
            }

            if (TryReadBool(aimReticle, "hideSystemCursorDuringGameplay", out var hideSystemCursor) && !hideSystemCursor)
            {
                report.Warn("MouseAimReticleController.hideSystemCursorDuringGameplay is false. Hide the system cursor during gameplay so it does not compete with the world reticle.");
            }

            if (TryReadFloat(aimReticle, "yOffset", out var yOffset) && yOffset < 0.1f)
            {
                report.Warn("MouseAimReticleController.yOffset is below 0.1. Raise it so the blue reticle stays visible above floor geometry.");
            }

            if (TryReadFloat(aimReticle, "smoothTime", out var smoothTime) && smoothTime > 0.001f)
            {
                report.Warn("MouseAimReticleController.smoothTime is above 0. Disable reticle smoothing for responsive mouse aim.");
            }

            if (mouseAim == null)
            {
                return;
            }

            if (TryReadInt(aimReticle, "reticleLayer", out var reticleLayer) &&
                TryReadLayerMask(mouseAim, "groundLayers", out var groundLayers) &&
                (groundLayers & (1 << Mathf.Clamp(reticleLayer, 0, 31))) != 0)
            {
                report.Warn("MouseAimController.groundLayers includes the reticle layer. Repair will exclude it so the reticle cannot interfere with mouse aim raycasts.");
            }
        }

        private static void ValidatePlayerDash(
            SurvivorValidationReport report,
            PlayerDashController dash,
            PlayerMovementController movement,
            MouseAimController mouseAim)
        {
            if (dash == null)
            {
                return;
            }

            if (ReadObject<PlayerMovementController>(dash, "movementController") != movement)
            {
                report.Warn("PlayerDashController.movementController should point to Player.PlayerMovementController.");
            }

            if (mouseAim != null && ReadObject<MouseAimController>(dash, "mouseAimController") != mouseAim)
            {
                report.Warn("PlayerDashController.mouseAimController should point to Player.MouseAimController for idle mouse-direction dash fallback.");
            }

            if (TryReadBool(dash, "enableKeyboardTestDash", out var keyboardTestDash) && keyboardTestDash)
            {
                report.Warn("PlayerDashController.enableKeyboardTestDash is enabled. DesktopSurvivorInputBridge already handles Space/Shift dash input.");
            }
        }

        private static void ValidateActiveSkills(
            SurvivorValidationReport report,
            ActiveSkillController activeSkills,
            DesktopSurvivorInputBridge inputBridge,
            PlayerMovementController movement,
            PlayerRuntimeStats runtimeStats,
            MouseAimController mouseAim,
            PlayerHealth playerHealth)
        {
            if (activeSkills == null)
            {
                return;
            }

            if (inputBridge != null && ReadObject<DesktopSurvivorInputBridge>(activeSkills, "inputBridge") != inputBridge)
            {
                report.Warn("ActiveSkillController.inputBridge should point to Player.DesktopSurvivorInputBridge.");
            }

            if (movement != null && ReadObject<PlayerMovementController>(activeSkills, "movementController") != movement)
            {
                report.Warn("ActiveSkillController.movementController should point to Player.PlayerMovementController.");
            }

            if (runtimeStats != null && ReadObject<PlayerRuntimeStats>(activeSkills, "runtimeStats") != runtimeStats)
            {
                report.Warn("ActiveSkillController.runtimeStats should point to Player.PlayerRuntimeStats.");
            }

            if (mouseAim != null && ReadObject<MouseAimController>(activeSkills, "mouseAimController") != mouseAim)
            {
                report.Warn("ActiveSkillController.mouseAimController should point to Player.MouseAimController.");
            }

            if (playerHealth != null && ReadObject<PlayerHealth>(activeSkills, "playerHealth") != playerHealth)
            {
                report.Warn("ActiveSkillController.playerHealth should point to Player.PlayerHealth so dead players cannot cast.");
            }

            if (ReadObject<PlayerConfig>(activeSkills, "playerConfig") == null)
            {
                report.Warn("ActiveSkillController.playerConfig is empty. It can infer through movement, but direct assignment exposes skill input buffer tuning.");
            }

            if (ReadObject<Transform>(activeSkills, "castOrigin") == null)
            {
                report.Warn("ActiveSkillController.castOrigin is empty. Assign Player or ProjectileSpawnPoint.");
            }

            if (TryReadLayerMask(activeSkills, "targetLayers", out var targetLayers) && targetLayers == 0)
            {
                report.Warn("ActiveSkillController.targetLayers is Nothing. Active skill hits will not find enemies.");
            }
        }

        private static void ValidatePlayerAnimation(
            SurvivorValidationReport report,
            CharacterAnimationDriver animationDriver,
            PlayerMovementController movement,
            PlayerAttackController attack,
            PlayerDashController dash,
            PlayerHealth playerHealth)
        {
            if (animationDriver == null)
            {
                return;
            }

            if (ReadObject<PlayerMovementController>(animationDriver, "playerMovement") != movement)
            {
                report.Warn("CharacterAnimationDriver.playerMovement should point to Player.PlayerMovementController.");
            }

            if (attack != null && ReadObject<PlayerAttackController>(animationDriver, "playerAttack") != attack)
            {
                report.Warn("CharacterAnimationDriver.playerAttack should point to Player.PlayerAttackController.");
            }

            if (dash != null && ReadObject<PlayerDashController>(animationDriver, "playerDash") != dash)
            {
                report.Warn("CharacterAnimationDriver.playerDash should point to Player.PlayerDashController.");
            }

            if (playerHealth != null && ReadObject<PlayerHealth>(animationDriver, "playerHealth") != playerHealth)
            {
                report.Warn("CharacterAnimationDriver.playerHealth should point to Player.PlayerHealth.");
            }

            if (TryReadBool(animationDriver, "playerAttackLocksDirectState", out var attackLocksDirectState) && attackLocksDirectState)
            {
                report.Warn("CharacterAnimationDriver.playerAttackLocksDirectState is enabled. Disable for survivor locomotion plus attack layering.");
            }
        }

        private static void ValidateWeaponConfigs(SurvivorValidationReport report)
        {
            var guids = AssetDatabase.FindAssets("t:WeaponConfig", new[] { "Assets/_Project" });
            if (guids == null || guids.Length == 0)
            {
                report.Warn("No WeaponConfig assets found under Assets/_Project.");
                return;
            }

            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var weapon = AssetDatabase.LoadAssetAtPath<WeaponConfig>(path);
                if (weapon == null)
                {
                    continue;
                }

                if (weapon.ProjectilePrefab == null)
                {
                    report.Warn($"{weapon.name}.projectilePrefab is empty. Primary attack will use direct-hit fallback if enabled.");
                }

                if (weapon.TargetLayers.value == 0)
                {
                    report.Warn($"{weapon.name}.targetLayers is Nothing. Mouse aim fallback may still fire, but nearest-target aim will not work.");
                }
            }
        }

        private static void Require(SurvivorValidationReport report, Object value, string label)
        {
            if (value == null)
            {
                report.Error($"{label} is missing.");
            }
        }

        private static void RequireField(SurvivorValidationReport report, Object value, string label)
        {
            if (value == null)
            {
                report.Error($"{label} is empty.");
            }
        }

        private static T ReadObject<T>(Object target, string fieldName) where T : Object
        {
            if (target == null)
            {
                return null;
            }

            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(fieldName);
            return property != null && property.propertyType == SerializedPropertyType.ObjectReference
                ? property.objectReferenceValue as T
                : null;
        }

        private static bool TryReadBool(Object target, string fieldName, out bool value)
        {
            value = false;
            if (target == null)
            {
                return false;
            }

            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(fieldName);
            if (property == null || property.propertyType != SerializedPropertyType.Boolean)
            {
                return false;
            }

            value = property.boolValue;
            return true;
        }

        private static bool TryReadEnum(Object target, string fieldName, out int value)
        {
            value = 0;
            if (target == null)
            {
                return false;
            }

            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(fieldName);
            if (property == null || property.propertyType != SerializedPropertyType.Enum)
            {
                return false;
            }

            value = property.enumValueIndex;
            return true;
        }

        private static bool TryReadFloat(Object target, string fieldName, out float value)
        {
            value = 0f;
            if (target == null)
            {
                return false;
            }

            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(fieldName);
            if (property == null || property.propertyType != SerializedPropertyType.Float)
            {
                return false;
            }

            value = property.floatValue;
            return true;
        }

        private static bool TryReadInt(Object target, string fieldName, out int value)
        {
            value = 0;
            if (target == null)
            {
                return false;
            }

            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(fieldName);
            if (property == null || property.propertyType != SerializedPropertyType.Integer)
            {
                return false;
            }

            value = property.intValue;
            return true;
        }

        private static bool TryReadLayerMask(Object target, string fieldName, out int value)
        {
            value = 0;
            if (target == null)
            {
                return false;
            }

            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(fieldName);
            if (property == null || property.propertyType != SerializedPropertyType.LayerMask)
            {
                return false;
            }

            value = property.intValue;
            return true;
        }
    }

    public sealed class SurvivorValidationReport
    {
        private readonly List<string> errors = new List<string>();
        private readonly List<string> warnings = new List<string>();

        public SurvivorValidationReport(string scenePath)
        {
            ScenePath = string.IsNullOrWhiteSpace(scenePath) ? "<unsaved scene>" : scenePath;
        }

        public string ScenePath { get; }
        public int ErrorCount => errors.Count;
        public int WarningCount => warnings.Count;

        public void Error(string message)
        {
            errors.Add(message);
        }

        public void Warn(string message)
        {
            warnings.Add(message);
        }

        public void LogToConsole()
        {
            Debug.Log($"Survivor prototype validation report: {ScenePath}. Errors: {ErrorCount}, Warnings: {WarningCount}");

            for (var i = 0; i < errors.Count; i++)
            {
                Debug.LogError($"Survivor validation error: {errors[i]}");
            }

            for (var i = 0; i < warnings.Count; i++)
            {
                Debug.LogWarning($"Survivor validation warning: {warnings[i]}");
            }

            if (errors.Count == 0 && warnings.Count == 0)
            {
                Debug.Log("No missing survivor prototype references detected.");
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Survivor prototype validation report: {ScenePath}");
            builder.AppendLine($"Errors: {ErrorCount}, Warnings: {WarningCount}");

            for (var i = 0; i < errors.Count; i++)
            {
                builder.AppendLine($"ERROR: {errors[i]}");
            }

            for (var i = 0; i < warnings.Count; i++)
            {
                builder.AppendLine($"WARN: {warnings[i]}");
            }

            if (errors.Count == 0 && warnings.Count == 0)
            {
                builder.AppendLine("No missing survivor prototype references detected.");
            }

            return builder.ToString();
        }
    }
}
#endif
