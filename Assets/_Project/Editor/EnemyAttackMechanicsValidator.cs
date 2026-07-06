// EnemyAttackMechanicsValidator.cs
// Editor tool: Tap Knockout > Combat > Validate Enemy Attack Mechanics

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TapKnockout.Combat;
using TapKnockout.Enemy;
using TapKnockout.Player;
using TapKnockout.Projectile;
using TapKnockout.Survivor;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Editor
{
    public static class EnemyAttackMechanicsValidator
    {
        private const string PrefabFolder = "Assets/_Project/Prefabs/Enemies/CuteMonsters";
        private const string ConfigFolder = "Assets/_Project/ScriptableObjects/Enemies/CuteMonsters/AttackConfigs";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player/Player.prefab";
        private const string ReportPath = "Assets/_Project/Docs/EnemyAttackImplementationReport.md";
        private const string VfxMappingPath = "Assets/_Project/Docs/EnemyAttackVFXMapping.md";

        // Types that require an active VFX
        private static readonly HashSet<EnemyDistinctAttackType> TypesRequiringActiveVfx = new HashSet<EnemyDistinctAttackType>
        {
            EnemyDistinctAttackType.MeleeArc,
            EnemyDistinctAttackType.Charge,
            EnemyDistinctAttackType.Dive,
            EnemyDistinctAttackType.LeapSlash,
            EnemyDistinctAttackType.Beam,
            EnemyDistinctAttackType.FrostSlamShockwave,
            EnemyDistinctAttackType.RadialBurst,
            EnemyDistinctAttackType.SporeZone
        };

        // Types that require an impact VFX
        private static readonly HashSet<EnemyDistinctAttackType> TypesRequiringImpactVfx = new HashSet<EnemyDistinctAttackType>
        {
            EnemyDistinctAttackType.MeleeArc,
            EnemyDistinctAttackType.Dive,
            EnemyDistinctAttackType.Charge,
            EnemyDistinctAttackType.LeapSlash,
            EnemyDistinctAttackType.Beam,
            EnemyDistinctAttackType.FrostSlamShockwave
        };

        // Types that require a projectile prefab
        private static readonly HashSet<EnemyDistinctAttackType> TypesRequiringProjectile = new HashSet<EnemyDistinctAttackType>
        {
            EnemyDistinctAttackType.Projectile,
            EnemyDistinctAttackType.SpikeProjectile,
            EnemyDistinctAttackType.SlimeProjectileArea,
            EnemyDistinctAttackType.HomingProjectile
        };

        // Types that require an area zone prefab
        private static readonly HashSet<EnemyDistinctAttackType> TypesRequiringAreaZone = new HashSet<EnemyDistinctAttackType>
        {
            EnemyDistinctAttackType.SporeZone,
            EnemyDistinctAttackType.SlimeProjectileArea,
            EnemyDistinctAttackType.FrostSlamShockwave
        };

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

        [MenuItem("Tap Knockout/Combat/Validate Enemy Attack Mechanics", priority = 201)]
        public static void ValidateAll()
        {
            var result = ValidateAllInternal(writeReport: true);
            Debug.Log($"[Validator] Report written to {ReportPath} - {result.Errors} errors, {result.Warnings} warnings");

            if (result.Errors > 0)
            {
                Debug.LogError($"[Validator] Enemy attack mechanics are not gameplay-ready: {result.Errors} error(s).");
            }
        }

        public static ValidationResult ValidateAllInternal(bool writeReport)
        {
            Debug.Log("=== EnemyAttackMechanicsValidator: START ===");

            var report = new StringBuilder();
            report.AppendLine("# Enemy Attack Implementation Report");
            report.AppendLine();
            report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();

            var result = new ValidationResult();
            ValidatePlayerHurtbox(report, result);
            ValidateAttackConfigs(report, result);
            ValidateVfxReadiness(report, result);
            ValidateRuntimePrefabs(report, result);

            report.AppendLine("## Summary");
            report.AppendLine();
            report.AppendLine($"- Errors: {result.Errors}");
            report.AppendLine($"- Warnings: {result.Warnings}");
            report.AppendLine(result.Errors == 0
                ? "- Status: PASS - all active cute monster attacks are gameplay-ready."
                : "- Status: FAIL - run Tap Knockout > Combat > Build Enemy Attack Mechanics and review errors.");
            report.AppendLine();

            if (writeReport)
            {
                var reportDir = Path.GetDirectoryName(ReportPath);
                if (!Directory.Exists(reportDir))
                {
                    Directory.CreateDirectory(reportDir);
                }

                File.WriteAllText(ReportPath, report.ToString());
                WriteVfxMappingDoc();
                AssetDatabase.Refresh();
            }

            Debug.Log("=== EnemyAttackMechanicsValidator: DONE ===");
            return result;
        }

        // ── VFX Readiness Section ──────────────────────────────────────────────────

        private static void ValidateVfxReadiness(StringBuilder report, ValidationResult result)
        {
            report.AppendLine("## VFX Readiness Audit");
            report.AppendLine();
            report.AppendLine("Checks each attack config for production-quality VFX references:");
            report.AppendLine("- Telegraph, Active, Impact, Projectile, AreaZone prefabs must be non-null where required.");
            report.AppendLine("- VFX prefabs must carry an EnemyAttackVFXMarker with productionReady=true, placeholder=false.");
            report.AppendLine("- VFX prefabs must have either EnemyAttackVFXAutoCleanup or a ParticleSystem (auto-stop) to prevent leaks.");
            report.AppendLine("- Projectile prefabs must have a VisualRoot child (no visible renderer on physics root).");
            report.AppendLine("- Area zone prefabs must have a VisualRoot child.");
            report.AppendLine();
            report.AppendLine("| Enemy | Config | Attack Type | Telegraph | Active VFX | Impact VFX | Projectile | Area Zone | VFX Ready |");
            report.AppendLine("|---|---|---|---|---|---|---|---|---|");

            foreach (var expected in ExpectedEnemies)
            {
                foreach (var configName in expected.ConfigNames)
                {
                    var cfg = LoadConfig(configName);
                    if (cfg == null)
                    {
                        report.AppendLine($"| {expected.EnemyName} | {configName} | - | - | - | - | - | - | **MISSING CONFIG** |");
                        Error(result, $"VFX audit: config {configName} not found.");
                        continue;
                    }

                    ValidateSingleConfigVfx(expected.EnemyName, configName, cfg, report, result);
                }
            }

            report.AppendLine();
        }

        private static void ValidateSingleConfigVfx(
            string enemyName,
            string configName,
            EnemyAttackConfig cfg,
            StringBuilder report,
            ValidationResult result)
        {
            var attackType = cfg.AttackType;

            // Telegraph
            var telegraphPath = Describe(cfg.TelegraphPrefab);
            var telegraphReady = cfg.TelegraphPrefab != null
                ? CheckVfxPrefab(cfg.TelegraphPrefab, EnemyAttackVFXKind.Telegraph, result, configName, "telegraph")
                : "null";

            // Active VFX
            var activeVfxPath = Describe(cfg.ActiveVfxPrefab);
            var activeVfxReady = "n/a";
            if (TypesRequiringActiveVfx.Contains(attackType))
            {
                if (cfg.ActiveVfxPrefab == null)
                {
                    Error(result, $"{configName} ({attackType}) requires activeVfxPrefab but it is null.");
                    activeVfxReady = "**NULL**";
                }
                else
                {
                    activeVfxReady = CheckVfxPrefab(cfg.ActiveVfxPrefab, EnemyAttackVFXKind.Active, result, configName, "activeVfx");
                }
            }
            else if (cfg.ActiveVfxPrefab != null)
            {
                activeVfxReady = CheckVfxPrefab(cfg.ActiveVfxPrefab, EnemyAttackVFXKind.Active, result, configName, "activeVfx");
            }

            // Impact VFX
            var impactVfxPath = Describe(cfg.ImpactVfxPrefab);
            var impactVfxReady = "n/a";
            if (TypesRequiringImpactVfx.Contains(attackType))
            {
                if (cfg.ImpactVfxPrefab == null)
                {
                    Error(result, $"{configName} ({attackType}) requires impactVfxPrefab but it is null.");
                    impactVfxReady = "**NULL**";
                }
                else
                {
                    impactVfxReady = CheckVfxPrefab(cfg.ImpactVfxPrefab, EnemyAttackVFXKind.Impact, result, configName, "impactVfx");
                }
            }
            else if (cfg.ImpactVfxPrefab != null)
            {
                impactVfxReady = CheckVfxPrefab(cfg.ImpactVfxPrefab, EnemyAttackVFXKind.Impact, result, configName, "impactVfx");
            }

            // Projectile
            var projectilePath = Describe(cfg.ProjectilePrefab);
            var projectileReady = "n/a";
            if (TypesRequiringProjectile.Contains(attackType))
            {
                if (cfg.ProjectilePrefab == null)
                {
                    Error(result, $"{configName} ({attackType}) requires projectilePrefab but it is null.");
                    projectileReady = "**NULL**";
                }
                else
                {
                    projectileReady = CheckProjectilePrefab(cfg.ProjectilePrefab, result, configName);
                }
            }

            // Area Zone
            var areaZonePath = Describe(cfg.AreaZonePrefab);
            var areaZoneReady = "n/a";
            if (TypesRequiringAreaZone.Contains(attackType))
            {
                if (cfg.AreaZonePrefab == null)
                {
                    Error(result, $"{configName} ({attackType}) requires areaZonePrefab but it is null.");
                    areaZoneReady = "**NULL**";
                }
                else
                {
                    areaZoneReady = CheckAreaZonePrefab(cfg.AreaZonePrefab, result, configName);
                }
            }

            var allReady = !telegraphReady.Contains("FAIL") && !activeVfxReady.Contains("FAIL") &&
                           !impactVfxReady.Contains("FAIL") && !projectileReady.Contains("FAIL") &&
                           !areaZoneReady.Contains("FAIL") && !telegraphReady.Contains("NULL") &&
                           !activeVfxReady.Contains("NULL") && !impactVfxReady.Contains("NULL") &&
                           !projectileReady.Contains("NULL") && !areaZoneReady.Contains("NULL");

            report.AppendLine(
                $"| {enemyName} | {configName} | {attackType} | " +
                $"{telegraphPath} ({telegraphReady}) | " +
                $"{activeVfxPath} ({activeVfxReady}) | " +
                $"{impactVfxPath} ({impactVfxReady}) | " +
                $"{projectilePath} ({projectileReady}) | " +
                $"{areaZonePath} ({areaZoneReady}) | " +
                $"{(allReady ? "PASS" : "**FAIL**")} |");
        }

        /// <summary>
        /// Returns "ok" if the prefab has a production-ready EnemyAttackVFXMarker and cleanup, or an error string.
        /// </summary>
        private static string CheckVfxPrefab(
            GameObject prefab,
            EnemyAttackVFXKind expectedKind,
            ValidationResult result,
            string configName,
            string fieldLabel)
        {
            if (prefab == null)
            {
                return "null";
            }

            var prefabPath = AssetDatabase.GetAssetPath(prefab);

            // Must live inside _Project
            if (!prefabPath.StartsWith("Assets/_Project", StringComparison.OrdinalIgnoreCase))
            {
                Error(result, $"{configName} {fieldLabel} prefab '{prefab.name}' is outside Assets/_Project ({prefabPath}). Create a project-owned wrapper.");
                return "FAIL:outside_project";
            }

            // Check for placeholder primitive renderers on the root
            if (HasPlaceholderRenderer(prefab))
            {
                Error(result, $"{configName} {fieldLabel} prefab '{prefab.name}' has a visible MeshRenderer/MeshFilter on root — this is a primitive placeholder. Replace with particles/trail/line.");
                return "FAIL:primitive_renderer";
            }

            // Check for EnemyAttackVFXMarker
            var marker = prefab.GetComponentInChildren<EnemyAttackVFXMarker>(true);
            if (marker == null)
            {
                // Warn — builder should have added this, but it doesn't block gameplay
                Warn(result, $"{configName} {fieldLabel} prefab '{prefab.name}' has no EnemyAttackVFXMarker. Run the builder to tag it.");
                return "WARN:no_marker";
            }

            if (!marker.IsProductionReady)
            {
                Error(result, $"{configName} {fieldLabel} prefab '{prefab.name}' marker has productionReady=false or placeholder=true.");
                return "FAIL:not_production_ready";
            }

            // Must have auto-cleanup or at least particle systems (they have their own lifetime)
            var hasCleanup = prefab.GetComponentInChildren<EnemyAttackVFXAutoCleanup>(true) != null;
            var hasParticle = prefab.GetComponentInChildren<ParticleSystem>(true) != null;
            var hasTrail = prefab.GetComponentInChildren<TrailRenderer>(true) != null;
            var hasLine = prefab.GetComponentInChildren<LineRenderer>(true) != null;

            if (!hasCleanup && !hasParticle && !hasTrail && !hasLine)
            {
                Error(result, $"{configName} {fieldLabel} prefab '{prefab.name}' has no VFX components (ParticleSystem/TrailRenderer/LineRenderer) and no EnemyAttackVFXAutoCleanup. It will never clean itself up.");
                return "FAIL:no_vfx_or_cleanup";
            }

            if (!hasCleanup && !hasParticle)
            {
                Warn(result, $"{configName} {fieldLabel} prefab '{prefab.name}' has trail/line but no ParticleSystem or EnemyAttackVFXAutoCleanup — verify it cleans up.");
                return "WARN:check_cleanup";
            }

            return "ok";
        }

        private static string CheckProjectilePrefab(GameObject prefab, ValidationResult result, string configName)
        {
            if (prefab == null)
            {
                return "null";
            }

            var prefabPath = AssetDatabase.GetAssetPath(prefab);
            if (!prefabPath.StartsWith("Assets/_Project", StringComparison.OrdinalIgnoreCase))
            {
                Error(result, $"{configName} projectile prefab '{prefab.name}' is outside Assets/_Project.");
                return "FAIL:outside_project";
            }

            // Root must not have a visible renderer (it's a physics root)
            if (HasPlaceholderRenderer(prefab))
            {
                Error(result, $"{configName} projectile prefab '{prefab.name}' has a MeshRenderer on root. Physics roots must be invisible; move visuals to VisualRoot child.");
                return "FAIL:renderer_on_physics_root";
            }

            // Must have a VisualRoot child
            var visualRoot = FindChildNamed(prefab.transform, "VisualRoot");
            if (visualRoot == null)
            {
                Error(result, $"{configName} projectile prefab '{prefab.name}' has no 'VisualRoot' child. Builder should have created one.");
                return "FAIL:no_visual_root";
            }

            // VisualRoot must have actual VFX
            var hasVfx = visualRoot.GetComponentInChildren<ParticleSystem>(true) != null
                || visualRoot.GetComponentInChildren<TrailRenderer>(true) != null
                || visualRoot.GetComponentInChildren<LineRenderer>(true) != null;
            if (!hasVfx)
            {
                Error(result, $"{configName} projectile prefab '{prefab.name}' VisualRoot has no VFX components (particle/trail/line).");
                return "FAIL:no_visual_vfx";
            }

            // Must have EnemyProjectileController
            if (prefab.GetComponent<EnemyProjectileController>() == null)
            {
                Error(result, $"{configName} projectile prefab '{prefab.name}' has no EnemyProjectileController.");
                return "FAIL:no_projectile_controller";
            }

            // VisualRoot marker
            var marker = visualRoot.GetComponentInChildren<EnemyAttackVFXMarker>(true);
            if (marker == null)
            {
                Warn(result, $"{configName} projectile '{prefab.name}' VisualRoot has no EnemyAttackVFXMarker.");
                return "WARN:no_marker";
            }

            return "ok";
        }

        private static string CheckAreaZonePrefab(GameObject prefab, ValidationResult result, string configName)
        {
            if (prefab == null)
            {
                return "null";
            }

            var prefabPath = AssetDatabase.GetAssetPath(prefab);
            if (!prefabPath.StartsWith("Assets/_Project", StringComparison.OrdinalIgnoreCase))
            {
                Error(result, $"{configName} area zone prefab '{prefab.name}' is outside Assets/_Project.");
                return "FAIL:outside_project";
            }

            // Must have EnemyAreaZone
            if (prefab.GetComponent<EnemyAreaZone>() == null)
            {
                Error(result, $"{configName} area zone prefab '{prefab.name}' has no EnemyAreaZone component.");
                return "FAIL:no_area_zone";
            }

            // Must have VisualRoot child
            var visualRoot = FindChildNamed(prefab.transform, "VisualRoot");
            if (visualRoot == null)
            {
                Error(result, $"{configName} area zone prefab '{prefab.name}' has no 'VisualRoot' child.");
                return "FAIL:no_visual_root";
            }

            // VisualRoot must have visible VFX (not a raw primitive)
            if (HasPlaceholderRenderer(visualRoot.gameObject))
            {
                Error(result, $"{configName} area zone '{prefab.name}' VisualRoot has a primitive MeshRenderer. Replace with particles/line for production quality.");
                return "FAIL:primitive_renderer";
            }

            var hasVfx = visualRoot.GetComponentInChildren<ParticleSystem>(true) != null
                || visualRoot.GetComponentInChildren<TrailRenderer>(true) != null
                || visualRoot.GetComponentInChildren<LineRenderer>(true) != null;

            if (!hasVfx)
            {
                Error(result, $"{configName} area zone '{prefab.name}' VisualRoot has no VFX components.");
                return "FAIL:no_visual_vfx";
            }

            return "ok";
        }

        /// <summary>
        /// Returns true if the root GameObject itself (not children) has a MeshRenderer/MeshFilter indicating a visible primitive.
        /// </summary>
        private static bool HasPlaceholderRenderer(GameObject go)
        {
            return go.GetComponent<MeshRenderer>() != null || go.GetComponent<SkinnedMeshRenderer>() != null;
        }

        // ── Gameplay Sections ──────────────────────────────────────────────────────

        private static void ValidatePlayerHurtbox(StringBuilder report, ValidationResult result)
        {
            report.AppendLine("## Player Damage Receiver");
            report.AppendLine();

            var player = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (player == null)
            {
                Error(result, $"Player prefab missing at {PlayerPrefabPath}");
                report.AppendLine($"- FAIL: Player prefab missing at `{PlayerPrefabPath}`.");
                report.AppendLine();
                return;
            }

            var health = player.GetComponent<PlayerHealth>();
            var hurtbox = FindChildNamed(player.transform, "CombatHurtbox");
            var hurtboxCollider = hurtbox != null ? hurtbox.GetComponent<Collider>() : null;

            if (health == null)
            {
                Error(result, "Player prefab has no PlayerHealth.");
            }

            if (hurtboxCollider == null || !hurtboxCollider.enabled)
            {
                Error(result, "Player prefab has no enabled CombatHurtbox collider for projectile and zone attacks.");
            }

            report.AppendLine($"- Player prefab: `{PlayerPrefabPath}`");
            report.AppendLine($"- PlayerHealth: {(health != null ? "present" : "missing")}");
            report.AppendLine($"- CombatHurtbox collider: {(hurtboxCollider != null && hurtboxCollider.enabled ? "present" : "missing")}");
            report.AppendLine();
        }

        private static void ValidateAttackConfigs(StringBuilder report, ValidationResult result)
        {
            report.AppendLine("## Attack Config Assets");
            report.AppendLine();
            report.AppendLine("| Config | Ready | Attack Type | Damage | Projectile | Area Zone | Telegraph | Active VFX | Impact VFX |");
            report.AppendLine("|---|---:|---|---:|---|---|---|---|---|");

            foreach (var configName in ExpectedConfigNames())
            {
                var cfg = LoadConfig(configName);
                if (cfg == null)
                {
                    Error(result, $"Config missing: {configName}");
                    report.AppendLine($"| {configName} | no | missing | - | - | - | - | - | - |");
                    continue;
                }

                if (!EnemyAttackReadinessUtility.IsConfigGameplayReady(cfg, out var reason))
                {
                    Error(result, $"{configName} is not gameplay-ready: {reason}");
                }

                ValidateHitLayerMaskCoversPlayer(cfg, configName, result);

                report.AppendLine(
                    $"| {configName} | {(EnemyAttackReadinessUtility.IsConfigGameplayReady(cfg, out _) ? "yes" : "no")} | {cfg.AttackType} | {cfg.Damage:0.#}/{cfg.AreaZoneTickDamage:0.#} | " +
                    $"{Describe(cfg.ProjectilePrefab)} | {Describe(cfg.AreaZonePrefab)} | {Describe(cfg.TelegraphPrefab)} | {Describe(cfg.ActiveVfxPrefab)} | {Describe(cfg.ImpactVfxPrefab)} |");
            }

            var cactusSpike = LoadConfig("AC_Cactus_SpikeProjectile");
            var cactusRadial = LoadConfig("AC_Cactus_RadialSpikeBurst");
            if (cactusSpike == null || cactusRadial == null)
            {
                Error(result, "Cactus is missing one or both required configs.");
            }

            report.AppendLine();
        }

        private static void ValidateRuntimePrefabs(StringBuilder report, ValidationResult result)
        {
            report.AppendLine("## Runtime Prefab Mapping");
            report.AppendLine();
            report.AppendLine("| Enemy | Runtime Prefab Path | Spawn Group Source | Distinct Controller | Configs Valid | Contact Damage | References |");
            report.AppendLine("|---|---|---|---:|---:|---|---|");

            var runtimePrefabs = ResolveRuntimePrefabMap();
            foreach (var expected in ExpectedEnemies)
            {
                if (!runtimePrefabs.TryGetValue(expected.EnemyName, out var runtimePrefab))
                {
                    runtimePrefab = new RuntimePrefabInfo(
                        expected.EnemyName,
                        $"{PrefabFolder}/{expected.FallbackPrefabName}.prefab",
                        "fallback path",
                        AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/{expected.FallbackPrefabName}.prefab"));
                }

                if (runtimePrefab.Prefab == null)
                {
                    Error(result, $"{expected.EnemyName} runtime prefab missing at {runtimePrefab.PrefabPath}");
                    report.AppendLine($"| {expected.EnemyName} | `{runtimePrefab.PrefabPath}` | {runtimePrefab.SourcePath} | no | no | missing | missing prefab |");
                    continue;
                }

                var controller = runtimePrefab.Prefab.GetComponent<EnemyDistinctAttackController>();
                var legacyAttack = runtimePrefab.Prefab.GetComponent<EnemyAttackController>();
                var assignedConfigs = ReadAssignedConfigs(controller);
                var readinessReason = "controller missing";
                var distinctReady = controller != null &&
                    controller.enabled &&
                    EnemyAttackReadinessUtility.IsDistinctAttackSystemReady(assignedConfigs, out readinessReason);

                if (controller == null)
                {
                    Error(result, $"{expected.EnemyName} runtime prefab has no EnemyDistinctAttackController.");
                }
                else if (!controller.enabled)
                {
                    Error(result, $"{expected.EnemyName} EnemyDistinctAttackController is disabled.");
                }

                // Check for TelegraphRoot — must be a LineRenderer, not a primitive
                ValidateTelegraphRoot(expected.EnemyName, runtimePrefab.Prefab, result);

                if (!distinctReady)
                {
                    Error(result, $"{expected.EnemyName} distinct attack setup invalid: {readinessReason}");
                }

                AssertExpectedConfigs(expected, assignedConfigs, result);
                ValidateRequiredChildren(expected, runtimePrefab.Prefab, result);

                var contactDamageEnabled = ReadBool(legacyAttack, "autoDealContactDamage", fallback: true);
                if (!distinctReady && !contactDamageEnabled)
                {
                    Error(result, $"{expected.EnemyName} has invalid distinct attacks but contact damage is disabled.");
                }

                var referenceSummary = BuildReferenceSummary(assignedConfigs);
                report.AppendLine(
                    $"| {expected.EnemyName} | `{runtimePrefab.PrefabPath}` | `{runtimePrefab.SourcePath}` | {(controller != null && controller.enabled ? "yes" : "no")} | {(distinctReady ? "yes" : "no")} | " +
                    $"{(contactDamageEnabled ? "enabled" : "disabled")} ({(distinctReady ? "distinct ready" : readinessReason)}) | {referenceSummary} |");
            }

            report.AppendLine();
        }

        private static void ValidateTelegraphRoot(string enemyName, GameObject prefab, ValidationResult result)
        {
            var telegraphRoot = FindChildNamed(prefab.transform, "TelegraphRoot");
            if (telegraphRoot == null)
            {
                // Already caught by ValidateRequiredChildren
                return;
            }

            // Must NOT have a MeshRenderer on the telegraph root (that would be a visible primitive)
            if (HasPlaceholderRenderer(telegraphRoot.gameObject))
            {
                Error(result, $"{enemyName} TelegraphRoot has a MeshRenderer — this is a primitive placeholder. The builder should have replaced it with a LineRenderer.");
                return;
            }

            // Should have a LineRenderer
            var line = telegraphRoot.GetComponent<LineRenderer>();
            if (line == null)
            {
                Warn(result, $"{enemyName} TelegraphRoot has no LineRenderer. Visual telegraph may not appear during windup.");
            }

            // Must have the VFX marker tagged as non-placeholder
            var marker = telegraphRoot.GetComponent<EnemyAttackVFXMarker>();
            if (marker != null && !marker.IsProductionReady)
            {
                Error(result, $"{enemyName} TelegraphRoot EnemyAttackVFXMarker is marked as placeholder or not production-ready.");
            }
        }

        // ── VFX Mapping Doc ────────────────────────────────────────────────────────

        private static void WriteVfxMappingDoc()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Enemy Attack VFX Mapping");
            sb.AppendLine();
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            sb.AppendLine("For each enemy, this document records which VFX prefab assets are assigned to each attack config field.");
            sb.AppendLine();

            foreach (var expected in ExpectedEnemies)
            {
                sb.AppendLine($"## {expected.EnemyName}");
                sb.AppendLine();

                foreach (var configName in expected.ConfigNames)
                {
                    var cfg = LoadConfig(configName);
                    if (cfg == null)
                    {
                        sb.AppendLine($"### {configName}");
                        sb.AppendLine();
                        sb.AppendLine("**Config missing — run the builder.**");
                        sb.AppendLine();
                        continue;
                    }

                    sb.AppendLine($"### {configName} ({cfg.AttackType})");
                    sb.AppendLine();
                    sb.AppendLine($"- **Telegraph:** {DescribePath(cfg.TelegraphPrefab)}");
                    sb.AppendLine($"- **Active VFX:** {DescribePath(cfg.ActiveVfxPrefab)}");
                    sb.AppendLine($"- **Impact VFX:** {DescribePath(cfg.ImpactVfxPrefab)}");
                    sb.AppendLine($"- **Projectile:** {DescribePath(cfg.ProjectilePrefab)}");
                    sb.AppendLine($"- **Area Zone:** {DescribePath(cfg.AreaZonePrefab)}");
                    sb.AppendLine();
                    sb.AppendLine("**VFX Asset Source:**");

                    AppendVfxSourceInfo(sb, "Telegraph", cfg.TelegraphPrefab);
                    AppendVfxSourceInfo(sb, "Active VFX", cfg.ActiveVfxPrefab);
                    AppendVfxSourceInfo(sb, "Impact VFX", cfg.ImpactVfxPrefab);
                    AppendProjectileSourceInfo(sb, cfg.ProjectilePrefab);
                    AppendAreaZoneSourceInfo(sb, cfg.AreaZonePrefab);

                    sb.AppendLine();
                    sb.AppendLine("**Timing:**");
                    sb.AppendLine($"- Windup: {cfg.WindupTime:0.00}s — Telegraph visible");
                    sb.AppendLine($"- Active: {cfg.ActiveTime:0.00}s — Damage window, Active VFX plays");
                    sb.AppendLine($"- Recovery: {cfg.RecoveryTime:0.00}s — VFX cleanup");
                    sb.AppendLine();
                }
            }

            var dir = Path.GetDirectoryName(VfxMappingPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir!);
            }

            File.WriteAllText(VfxMappingPath, sb.ToString());
        }

        private static void AppendVfxSourceInfo(StringBuilder sb, string label, GameObject prefab)
        {
            if (prefab == null)
            {
                sb.AppendLine($"  - {label}: null");
                return;
            }

            var marker = prefab.GetComponentInChildren<EnemyAttackVFXMarker>(true);
            var path = AssetDatabase.GetAssetPath(prefab);
            if (marker != null)
            {
                var sourceStr = string.IsNullOrEmpty(marker.SourceAssetPath) ? "procedural" : marker.SourceAssetPath;
                sb.AppendLine($"  - {label}: `{path}` | type={marker.SourceType} | ready={marker.IsProductionReady} | source={sourceStr}");
            }
            else
            {
                sb.AppendLine($"  - {label}: `{path}` | no VFX marker");
            }
        }

        private static void AppendProjectileSourceInfo(StringBuilder sb, GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            var path = AssetDatabase.GetAssetPath(prefab);
            var visualRoot = FindChildNamed(prefab.transform, "VisualRoot");
            var marker = visualRoot != null ? visualRoot.GetComponentInChildren<EnemyAttackVFXMarker>(true) : null;
            var sourceStr = marker != null && !string.IsNullOrEmpty(marker.SourceAssetPath) ? marker.SourceAssetPath : "procedural";
            sb.AppendLine($"  - Projectile: `{path}` | VisualRoot={visualRoot != null} | source={sourceStr}");
        }

        private static void AppendAreaZoneSourceInfo(StringBuilder sb, GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            var path = AssetDatabase.GetAssetPath(prefab);
            var visualRoot = FindChildNamed(prefab.transform, "VisualRoot");
            var marker = visualRoot != null ? visualRoot.GetComponentInChildren<EnemyAttackVFXMarker>(true) : null;
            var sourceStr = marker != null && !string.IsNullOrEmpty(marker.SourceAssetPath) ? marker.SourceAssetPath : "procedural";
            sb.AppendLine($"  - AreaZone: `{path}` | VisualRoot={visualRoot != null} | source={sourceStr}");
        }

        // ── Helpers ────────────────────────────────────────────────────────────────

        private static Dictionary<string, RuntimePrefabInfo> ResolveRuntimePrefabMap()
        {
            var map = new Dictionary<string, RuntimePrefabInfo>(StringComparer.OrdinalIgnoreCase);
            var groupGuids = AssetDatabase.FindAssets("t:SpawnGroupConfig", new[] { "Assets/_Project/ScriptableObjects/Waves" });
            for (var i = 0; i < groupGuids.Length; i++)
            {
                var groupPath = AssetDatabase.GUIDToAssetPath(groupGuids[i]);
                var group = AssetDatabase.LoadAssetAtPath<SpawnGroupConfig>(groupPath);
                if (group == null || group.EnemyPrefab == null)
                {
                    continue;
                }

                var enemyName = ResolveEnemyName(group);
                if (string.IsNullOrEmpty(enemyName))
                {
                    continue;
                }

                map[enemyName] = new RuntimePrefabInfo(
                    enemyName,
                    AssetDatabase.GetAssetPath(group.EnemyPrefab),
                    groupPath,
                    group.EnemyPrefab);
            }

            return map;
        }

        private static void AssertExpectedConfigs(ExpectedEnemy expected, EnemyAttackConfig[] assignedConfigs, ValidationResult result)
        {
            if (assignedConfigs == null || assignedConfigs.Length == 0)
            {
                Error(result, $"{expected.EnemyName} has no assigned attack configs.");
                return;
            }

            if (assignedConfigs.Length != expected.ConfigNames.Length)
            {
                Error(result, $"{expected.EnemyName} expected {expected.ConfigNames.Length} attack config(s), found {assignedConfigs.Length}.");
            }

            for (var i = 0; i < expected.ConfigNames.Length; i++)
            {
                var expectedConfig = LoadConfig(expected.ConfigNames[i]);
                var found = false;
                for (var j = 0; j < assignedConfigs.Length; j++)
                {
                    if (assignedConfigs[j] == expectedConfig)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Error(result, $"{expected.EnemyName} missing expected config {expected.ConfigNames[i]}.");
                }
            }
        }

        private static void ValidateRequiredChildren(ExpectedEnemy expected, GameObject prefab, ValidationResult result)
        {
            foreach (var childName in new[] { "AttackOrigin", "ProjectileSpawnPoint", "GroundOrigin", "VFXRoot", "TelegraphRoot" })
            {
                if (FindChildNamed(prefab.transform, childName) == null)
                {
                    Error(result, $"{expected.EnemyName} runtime prefab missing child transform {childName}.");
                }
            }
        }

        private static void ValidateHitLayerMaskCoversPlayer(EnemyAttackConfig cfg, string configName, ValidationResult result)
        {
            var playerLayer = ResolvePlayerHurtboxLayer();
            if (playerLayer < 0)
            {
                Error(result, $"{configName} cannot validate hit mask because player hurtbox layer could not be resolved.");
                return;
            }

            var playerLayerMask = 1 << playerLayer;
            if ((cfg.HitLayerMask.value & playerLayerMask) == 0)
            {
                Error(result, $"{configName} hit layer mask does not include player/hurtbox layer {playerLayer}.");
            }
        }

        private static int ResolvePlayerHurtboxLayer()
        {
            var player = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (player != null)
            {
                var hurtbox = FindChildNamed(player.transform, "CombatHurtbox");
                if (hurtbox != null)
                {
                    return hurtbox.gameObject.layer;
                }

                return player.layer;
            }

            var namedPlayerLayer = LayerMask.NameToLayer("Player");
            return namedPlayerLayer >= 0 ? namedPlayerLayer : 0;
        }

        private static EnemyAttackConfig[] ReadAssignedConfigs(EnemyDistinctAttackController controller)
        {
            if (controller == null)
            {
                return Array.Empty<EnemyAttackConfig>();
            }

            var so = new SerializedObject(controller);
            var prop = so.FindProperty("attackConfigs");
            if (prop == null || prop.arraySize == 0)
            {
                return Array.Empty<EnemyAttackConfig>();
            }

            var configs = new EnemyAttackConfig[prop.arraySize];
            for (var i = 0; i < prop.arraySize; i++)
            {
                configs[i] = prop.GetArrayElementAtIndex(i).objectReferenceValue as EnemyAttackConfig;
            }

            return configs;
        }

        private static string BuildReferenceSummary(EnemyAttackConfig[] configs)
        {
            if (configs == null || configs.Length == 0)
            {
                return "none";
            }

            var builder = new StringBuilder();
            for (var i = 0; i < configs.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append("<br>");
                }

                var cfg = configs[i];
                builder.Append(cfg != null ? $"{cfg.name}: {EnemyAttackReadinessUtility.DescribeReferences(cfg)}" : "null");
            }

            return builder.ToString();
        }

        private static IEnumerable<string> ExpectedConfigNames()
        {
            for (var i = 0; i < ExpectedEnemies.Length; i++)
            {
                for (var j = 0; j < ExpectedEnemies[i].ConfigNames.Length; j++)
                {
                    yield return ExpectedEnemies[i].ConfigNames[j];
                }
            }
        }

        private static EnemyAttackConfig LoadConfig(string configName)
        {
            return AssetDatabase.LoadAssetAtPath<EnemyAttackConfig>($"{ConfigFolder}/{configName}.asset");
        }

        private static string ResolveEnemyName(SpawnGroupConfig group)
        {
            var text = $"{group.name} {group.GroupId} {group.EnemyPrefab.name} {(group.EnemyConfig != null ? group.EnemyConfig.name : string.Empty)}";
            if (Contains(text, "YellowDragon")) return "YellowDragon";
            if (Contains(text, "GreenDemon")) return "GreenDemon";
            if (Contains(text, "Cthulhu")) return "Cthulhu";
            if (Contains(text, "Cyclops")) return "Cyclops";
            if (Contains(text, "Mushroom")) return "Mushroom";
            if (Contains(text, "Cactus")) return "Cactus";
            if (Contains(text, "Ghost")) return "Ghost";
            if (Contains(text, "Yeti")) return "Yeti";
            if (Contains(text, "Bee")) return "Bee";
            if (Contains(text, "Bat")) return "Bat";
            if (Contains(text, "Demon")) return "Demon";
            return string.Empty;
        }

        private static bool Contains(string source, string value)
        {
            return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static Transform FindChildNamed(Transform root, string childName)
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

                var nested = FindChildNamed(child, childName);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
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

        private static string Describe(UnityEngine.Object obj)
        {
            return obj != null ? obj.name : "null";
        }

        private static string DescribePath(UnityEngine.Object obj)
        {
            if (obj == null)
            {
                return "null";
            }

            var path = AssetDatabase.GetAssetPath(obj);
            return string.IsNullOrEmpty(path) ? obj.name : path;
        }

        private static void Error(ValidationResult result, string message)
        {
            result.Errors++;
            Debug.LogError($"[Validator] {message}");
        }

        private static void Warn(ValidationResult result, string message)
        {
            result.Warnings++;
            Debug.LogWarning($"[Validator] {message}");
        }

        // ── Data types ─────────────────────────────────────────────────────────────

        private readonly struct ExpectedEnemy
        {
            public ExpectedEnemy(string enemyName, string fallbackPrefabName, string[] configNames)
            {
                EnemyName = enemyName;
                FallbackPrefabName = fallbackPrefabName;
                ConfigNames = configNames;
            }

            public string EnemyName { get; }
            public string FallbackPrefabName { get; }
            public string[] ConfigNames { get; }
        }

        private readonly struct RuntimePrefabInfo
        {
            public RuntimePrefabInfo(string enemyName, string prefabPath, string sourcePath, GameObject prefab)
            {
                EnemyName = enemyName;
                PrefabPath = prefabPath;
                SourcePath = sourcePath;
                Prefab = prefab;
            }

            public string EnemyName { get; }
            public string PrefabPath { get; }
            public string SourcePath { get; }
            public GameObject Prefab { get; }
        }

        public sealed class ValidationResult
        {
            public int Errors;
            public int Warnings;
        }
    }
}
