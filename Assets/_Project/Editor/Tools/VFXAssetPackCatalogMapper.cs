using System;
using System.Collections.Generic;
using System.Text;
using TapKnockout.VFX;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Editor.Tools
{
    public static class VFXAssetPackCatalogMapper
    {
        public const string CatalogPath = "Assets/_Project/ScriptableObjects/VFX/VFXCatalog_VerticalSlice.asset";

        private static readonly string[] VfxAuditRoots =
        {
            "Assets/ThirdParty/VFX",
            "Assets/GabrielAguiarProductions/FreeQuickEffectsVol1",
            "Assets/JMO Assets/Cartoon FX (legacy)",
            "Assets/_Project/Prefabs/VFX",
            "Assets/_Project/Art/VFX",
            "Assets/_Project/VFX"
        };

        private const string CatalogFolder = "Assets/_Project/ScriptableObjects/VFX";

        [MenuItem("Tools/Tap Knockout/VFX/Audit ThirdParty VFX Packs")]
        public static void AuditThirdPartyVfxPacks()
        {
            var candidates = FindCandidates();
            var report = BuildAuditReport(candidates);
            Debug.Log(report);
        }

        [MenuItem("Tools/Tap Knockout/VFX/Create Vertical Slice VFX Catalog")]
        public static void CreateVerticalSliceVfxCatalog()
        {
            EnsureCatalogFolder();

            var catalog = AssetDatabase.LoadAssetAtPath<VFXCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<VFXCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            var definitions = new List<VFXDefinition>();
            var warnings = new StringBuilder();

            foreach (var mapping in RecommendedMappings)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(mapping.PrefabPath);
                if (prefab == null)
                {
                    warnings.AppendLine($"Missing prefab for {mapping.EventType}: {mapping.PrefabPath}");
                }

                definitions.Add(new VFXDefinition(
                    mapping.EventType,
                    prefab,
                    mapping.InitialPoolSize,
                    mapping.DefaultLifetime,
                    mapping.ParentToRequestParent,
                    mapping.UseRequestRotation,
                    mapping.UseRequestScale,
                    mapping.PositionOffset,
                    mapping.RotationOffsetEuler,
                    mapping.ScaleMultiplier,
                    mapping.AllowColorOverride));
            }

            catalog.SetDefinitions(definitions);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = catalog;

            var message = new StringBuilder();
            message.AppendLine($"Created/updated {CatalogPath} with {definitions.Count} VFX mappings.");
            if (warnings.Length > 0)
            {
                message.AppendLine("Warnings:");
                message.Append(warnings);
            }

            Debug.Log(message.ToString(), catalog);
        }

        public static IReadOnlyList<VFXPrefabAuditInfo> FindCandidatesForTests()
        {
            return FindCandidates();
        }

        private static List<VFXPrefabAuditInfo> FindCandidates()
        {
            var candidates = new List<VFXPrefabAuditInfo>();
            var roots = ResolveValidAuditRoots();
            var guids = AssetDatabase.FindAssets("t:Prefab", roots);
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                candidates.Add(AnalyzePrefab(path, prefab));
            }

            candidates.Sort((left, right) => string.Compare(left.AssetPath, right.AssetPath, StringComparison.Ordinal));
            return candidates;
        }

        private static VFXPrefabAuditInfo AnalyzePrefab(string path, GameObject prefab)
        {
            var particleSystems = prefab.GetComponentsInChildren<ParticleSystem>(true);
            var renderers = prefab.GetComponentsInChildren<Renderer>(true);
            var components = prefab.GetComponentsInChildren<Component>(true);

            var loopingCount = 0;
            var maxParticlesTotal = 0;
            var maxDuration = 0f;
            var missingScriptCount = 0;
            var nonUnityScriptCount = 0;
            var missingMaterialCount = 0;
            var hasVisualEffectLikeComponent = false;

            for (var i = 0; i < particleSystems.Length; i++)
            {
                var main = particleSystems[i].main;
                if (main.loop)
                {
                    loopingCount++;
                }

                maxParticlesTotal += main.maxParticles;
                maxDuration = Mathf.Max(maxDuration, main.duration + main.startLifetime.constantMax);
            }

            for (var i = 0; i < components.Length; i++)
            {
                var component = components[i];
                if (component == null)
                {
                    missingScriptCount++;
                    continue;
                }

                var componentType = component.GetType();
                var componentNamespace = componentType.Namespace ?? string.Empty;
                if (!componentNamespace.StartsWith("UnityEngine", StringComparison.Ordinal))
                {
                    nonUnityScriptCount++;
                }

                if (componentType.Name.IndexOf("VisualEffect", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    hasVisualEffectLikeComponent = true;
                }
            }

            for (var i = 0; i < renderers.Length; i++)
            {
                var materials = renderers[i].sharedMaterials;
                for (var materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    if (materials[materialIndex] == null || materials[materialIndex].shader == null)
                    {
                        missingMaterialCount++;
                    }
                }
            }

            return new VFXPrefabAuditInfo(
                path,
                particleSystems.Length,
                loopingCount,
                maxParticlesTotal,
                maxDuration,
                missingScriptCount,
                nonUnityScriptCount,
                missingMaterialCount,
                hasVisualEffectLikeComponent);
        }

        private static string BuildAuditReport(IReadOnlyList<VFXPrefabAuditInfo> candidates)
        {
            var report = new StringBuilder();
            report.AppendLine("Tap Knockout VFX Audit");
            report.AppendLine("Roots:");
            var roots = ResolveValidAuditRoots();
            for (var i = 0; i < roots.Length; i++)
            {
                report.AppendLine($"- {roots[i]}");
            }

            report.AppendLine($"Prefab candidates: {candidates.Count}");
            report.AppendLine();

            AppendTopCandidates(report, candidates, VFXEventType.PrimaryFireMuzzle);
            AppendTopCandidates(report, candidates, VFXEventType.PrimaryProjectileTrail);
            AppendTopCandidates(report, candidates, VFXEventType.PrimaryProjectileImpact);
            AppendTopCandidates(report, candidates, VFXEventType.DashImpact);
            AppendTopCandidates(report, candidates, VFXEventType.DashEnd);
            AppendTopCandidates(report, candidates, VFXEventType.EnemyHit);
            AppendTopCandidates(report, candidates, VFXEventType.EnemyDeath);
            AppendTopCandidates(report, candidates, VFXEventType.ProjectileHit);
            AppendTopCandidates(report, candidates, VFXEventType.SpawnTelegraph);
            AppendTopCandidates(report, candidates, VFXEventType.RoomClear);
            AppendTopCandidates(report, candidates, VFXEventType.BossWarning);
            AppendTopCandidates(report, candidates, VFXEventType.BossPhaseTransition);
            AppendTopCandidates(report, candidates, VFXEventType.AbilitySelected);
            AppendTopCandidates(report, candidates, VFXEventType.ForwardCleaveHit);
            AppendTopCandidates(report, candidates, VFXEventType.GroundImpactArea);
            AppendTopCandidates(report, candidates, VFXEventType.Heal);
            AppendTopCandidates(report, candidates, VFXEventType.XPOrbCollect);
            AppendTopCandidates(report, candidates, VFXEventType.LevelUpBurst);
            AppendTopCandidates(report, candidates, VFXEventType.AbilityDashShockwave);
            AppendTopCandidates(report, candidates, VFXEventType.AbilityProjectileSplit);
            AppendTopCandidates(report, candidates, VFXEventType.AbilityFireProc);
            AppendTopCandidates(report, candidates, VFXEventType.AbilityPoisonProc);
            AppendTopCandidates(report, candidates, VFXEventType.AbilityIceProc);
            AppendTopCandidates(report, candidates, VFXEventType.AbilityLightningProc);
            AppendTopCandidates(report, candidates, VFXEventType.AbilityShield);
            AppendTopCandidates(report, candidates, VFXEventType.AbilityMeteor);

            report.AppendLine("Recommended vertical slice mappings:");
            for (var i = 0; i < RecommendedMappings.Length; i++)
            {
                var mapping = RecommendedMappings[i];
                report.AppendLine($"- {mapping.EventType}: {mapping.PrefabPath} | pool {mapping.InitialPoolSize}, lifetime {mapping.DefaultLifetime:0.##}, scale {mapping.ScaleMultiplier:0.##}");
            }

            report.AppendLine();
            report.AppendLine("Performance warnings to review manually:");
            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (!candidate.HasRisk)
                {
                    continue;
                }

                report.AppendLine($"- {candidate.AssetPath}: {candidate.RiskSummary}");
            }

            return report.ToString();
        }

        private static string[] ResolveValidAuditRoots()
        {
            var roots = new List<string>();
            for (var i = 0; i < VfxAuditRoots.Length; i++)
            {
                if (AssetDatabase.IsValidFolder(VfxAuditRoots[i]))
                {
                    roots.Add(VfxAuditRoots[i]);
                }
            }

            return roots.ToArray();
        }

        private static void AppendTopCandidates(StringBuilder report, IReadOnlyList<VFXPrefabAuditInfo> candidates, VFXEventType eventType)
        {
            var scored = new List<ScoredCandidate>();
            for (var i = 0; i < candidates.Count; i++)
            {
                var score = VFXCandidateScoring.ScoreCandidate(eventType, candidates[i].AssetPath);
                if (score <= 0)
                {
                    continue;
                }

                scored.Add(new ScoredCandidate(candidates[i], score));
            }

            scored.Sort((left, right) => right.Score.CompareTo(left.Score));

            report.AppendLine($"Top candidates for {eventType}:");
            var count = Mathf.Min(5, scored.Count);
            if (count == 0)
            {
                report.AppendLine("- none");
            }

            for (var i = 0; i < count; i++)
            {
                var candidate = scored[i].Candidate;
                report.AppendLine($"- score {scored[i].Score}: {candidate.AssetPath} | ps {candidate.ParticleSystemCount}, loops {candidate.LoopingParticleSystemCount}, maxParticles {candidate.MaxParticlesTotal}, maxDuration {candidate.MaxDuration:0.##}");
            }

            report.AppendLine();
        }

        private static void EnsureCatalogFolder()
        {
            if (AssetDatabase.IsValidFolder(CatalogFolder))
            {
                return;
            }

            AssetDatabase.CreateFolder("Assets/_Project/ScriptableObjects", "VFX");
        }

        private static readonly VFXCatalogMapping[] RecommendedMappings =
        {
            new VFXCatalogMapping(VFXEventType.PrimaryFireMuzzle, "Assets/GabrielAguiarProductions/FreeQuickEffectsVol1/Prefabs/vfx_MuzzleFlash_01.prefab", 8, 0.45f, false, true, true, Vector3.zero, Vector3.zero, 0.42f, true),
            new VFXCatalogMapping(VFXEventType.ProjectileSpawn, "Assets/GabrielAguiarProductions/FreeQuickEffectsVol1/Prefabs/vfx_MuzzleFlash_01.prefab", 8, 0.45f, false, true, true, Vector3.zero, Vector3.zero, 0.42f, true),
            new VFXCatalogMapping(VFXEventType.PrimaryProjectileTrail, "Assets/GabrielAguiarProductions/FreeQuickEffectsVol1/Prefabs/vfx_Projectile_01.prefab", 12, 0.75f, true, true, false, Vector3.zero, Vector3.zero, 0.28f, true),
            new VFXCatalogMapping(VFXEventType.ProjectileTrail, "Assets/GabrielAguiarProductions/FreeQuickEffectsVol1/Prefabs/vfx_Projectile_01.prefab", 12, 0.75f, true, true, false, Vector3.zero, Vector3.zero, 0.28f, true),
            new VFXCatalogMapping(VFXEventType.PrimaryProjectileImpact, "Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/Basic Hit 2.prefab", 16, 0.8f, false, true, true, Vector3.zero, new Vector3(-90f, 0f, 0f), 0.65f, true),
            new VFXCatalogMapping(VFXEventType.PrimaryCriticalImpact, "Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/1.2/Basic Hit 8  (NEW).prefab", 8, 1.05f, false, true, true, Vector3.zero, new Vector3(-90f, 0f, 0f), 0.95f, true),
            new VFXCatalogMapping(VFXEventType.ForwardCleaveCast, "Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Weapon Effect.prefab", 4, 0.75f, false, true, true, Vector3.zero, Vector3.zero, 0.7f, true),
            new VFXCatalogMapping(VFXEventType.ForwardCleaveHit, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Sword Trails/Plain/CFXR4 Sword Hit PLAIN (Cross).prefab", 8, 0.9f, false, true, true, Vector3.zero, Vector3.zero, 0.68f, true),
            new VFXCatalogMapping(VFXEventType.GroundImpactCast, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Impacts/CFXR2 Ground Hit.prefab", 4, 0.85f, false, true, true, Vector3.zero, Vector3.zero, 0.8f, true),
            new VFXCatalogMapping(VFXEventType.GroundImpactArea, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Magic Misc/CFXR3 Magic Aura A (Runic).prefab", 4, 1.2f, false, false, true, Vector3.zero, Vector3.zero, 0.72f, true),
            new VFXCatalogMapping(VFXEventType.GroundImpactHit, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Impacts/CFXR2 Ground Hit.prefab", 6, 0.95f, false, true, true, Vector3.zero, Vector3.zero, 0.95f, true),
            new VFXCatalogMapping(VFXEventType.DashStart, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Misc/CFXR Flash.prefab", 8, 0.55f, false, true, true, Vector3.zero, Vector3.zero, 0.68f, true),
            new VFXCatalogMapping(VFXEventType.DashTrail, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Nature/CFXR4 Wind Trails.prefab", 8, 0.55f, true, true, false, Vector3.zero, Vector3.zero, 0.42f, true),
            new VFXCatalogMapping(VFXEventType.DashEnd, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Misc/CFXR Flash.prefab", 8, 0.55f, false, true, true, Vector3.zero, Vector3.zero, 0.58f, true),
            new VFXCatalogMapping(VFXEventType.SpawnTelegraph, "Assets/ThirdParty/VFX/Eric VFX Studio/Game VFX - Magic Circle(Free)/Prefabs/FX_MagicCircle_Icearrow01.prefab", 12, 1f, false, false, true, Vector3.zero, Vector3.zero, 0.62f, true),
            new VFXCatalogMapping(VFXEventType.EnemySpawn, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Misc/CFXR Magic Poof.prefab", 16, 0.8f, false, false, true, Vector3.zero, Vector3.zero, 0.45f, true),
            new VFXCatalogMapping(VFXEventType.EliteSpawn, "Assets/ThirdParty/VFX/Eric VFX Studio/Game VFX - Magic Circle(Free)/Prefabs/FX_MagicCircle_Icearrow01.prefab", 4, 1.2f, false, false, true, Vector3.zero, Vector3.zero, 1.05f, true),
            new VFXCatalogMapping(VFXEventType.EnemyDeathLarge, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Eerie/CFXR2 WW Enemy Explosion.prefab", 4, 1.5f, false, true, true, Vector3.zero, Vector3.zero, 0.9f, true),
            new VFXCatalogMapping(VFXEventType.EliteDeath, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Eerie/CFXR2 WW Enemy Explosion.prefab", 4, 1.5f, false, true, true, Vector3.zero, Vector3.zero, 1.05f, true),
            new VFXCatalogMapping(VFXEventType.BossSpawnWarning, "Assets/ThirdParty/VFX/Eric VFX Studio/Game VFX - Magic Circle(Free)/Prefabs/FX_MagicCircle_Icearrow01.prefab", 4, 2f, false, false, true, Vector3.zero, Vector3.zero, 1.5f, true),
            new VFXCatalogMapping(VFXEventType.BossPhaseTransition, "Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_LightPillar.prefab", 3, 1.35f, false, false, true, Vector3.zero, Vector3.zero, 1.05f, true),
            new VFXCatalogMapping(VFXEventType.BossHeavyAttackTelegraph, "Assets/ThirdParty/VFX/Eric VFX Studio/Game VFX - Magic Circle(Free)/Prefabs/FX_MagicCircle_Icearrow01.prefab", 4, 1.2f, false, false, true, Vector3.zero, Vector3.zero, 1.25f, true),
            new VFXCatalogMapping(VFXEventType.BossHeavyAttackImpact, "Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/1.2/Basic Hit 8  (NEW).prefab", 4, 1.2f, false, true, true, Vector3.zero, new Vector3(-90f, 0f, 0f), 1.35f, true),
            new VFXCatalogMapping(VFXEventType.XPOrbIdle, "Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_LootDrop_Blue.prefab", 8, 1f, false, false, true, Vector3.zero, Vector3.zero, 0.32f, true),
            new VFXCatalogMapping(VFXEventType.XPOrbCollect, "Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Greenlight_shrink.prefab", 12, 0.75f, false, false, true, Vector3.zero, Vector3.zero, 0.42f, true),
            new VFXCatalogMapping(VFXEventType.LevelUpBurst, "Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_LightPillar.prefab", 3, 1.3f, false, false, true, Vector3.zero, Vector3.zero, 0.82f, true),
            new VFXCatalogMapping(VFXEventType.ReticleFirePulse, "Assets/GabrielAguiarProductions/FreeQuickEffectsVol1/Prefabs/vfx_MuzzleFlash_02.prefab", 8, 0.35f, false, false, true, Vector3.zero, Vector3.zero, 0.28f, true),
            new VFXCatalogMapping(VFXEventType.DashImpact, "Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/Lightning Hit Blue.prefab", 8, 1.1f, false, true, true, new Vector3(0f, 0.05f, 0f), new Vector3(-90f, 0f, 0f), 1.15f, true),
            new VFXCatalogMapping(VFXEventType.EnemyHit, "Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/Basic Hit 2.prefab", 16, 0.8f, false, true, true, new Vector3(0f, 0.05f, 0f), new Vector3(-90f, 0f, 0f), 0.75f, true),
            new VFXCatalogMapping(VFXEventType.ProjectileHit, "Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/Basic Hit 2.prefab", 16, 0.8f, false, true, true, Vector3.zero, new Vector3(-90f, 0f, 0f), 0.65f, true),
            new VFXCatalogMapping(VFXEventType.EnemyKnockbackDust, "Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/1.2/Shadow Hit (NEW).prefab", 10, 0.8f, false, true, true, Vector3.zero, new Vector3(-90f, 0f, 0f), 0.45f, true),
            new VFXCatalogMapping(VFXEventType.EnemyDeath, "Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/1.2/Shadow Hit (NEW).prefab", 12, 1.2f, false, true, true, Vector3.zero, new Vector3(-90f, 0f, 0f), 0.85f, true),
            new VFXCatalogMapping(VFXEventType.RoomClear, "Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Magic Door_Gold.prefab", 2, 2f, false, false, true, Vector3.zero, Vector3.zero, 0.85f, true),
            new VFXCatalogMapping(VFXEventType.AbilityOffered, "Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Greenlight_shrink.prefab", 3, 1.2f, false, false, true, Vector3.zero, Vector3.zero, 0.65f, true),
            new VFXCatalogMapping(VFXEventType.AbilitySelected, "Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Greenlight_shrink.prefab", 4, 1.2f, false, false, true, Vector3.zero, Vector3.zero, 0.85f, true),
            new VFXCatalogMapping(VFXEventType.BossWarning, "Assets/ThirdParty/VFX/Eric VFX Studio/Game VFX - Magic Circle(Free)/Prefabs/FX_MagicCircle_Icearrow01.prefab", 4, 2f, false, false, true, Vector3.zero, Vector3.zero, 2f, true),
            new VFXCatalogMapping(VFXEventType.BossHit, "Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/1.2/Basic Hit 8  (NEW).prefab", 6, 1.2f, false, true, true, Vector3.zero, new Vector3(-90f, 0f, 0f), 1.25f, true),
            new VFXCatalogMapping(VFXEventType.BossDeath, "Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/1.2/Basic Hit 8  (NEW).prefab", 2, 2.2f, false, true, true, Vector3.zero, new Vector3(-90f, 0f, 0f), 1.8f, true),
            new VFXCatalogMapping(VFXEventType.Pickup, "Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Greenlight_shrink.prefab", 8, 1f, false, false, true, Vector3.zero, Vector3.zero, 0.55f, true),
            new VFXCatalogMapping(VFXEventType.Heal, "Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Greenlight_shrink.prefab", 4, 1.2f, false, false, true, Vector3.zero, Vector3.zero, 0.8f, true),
            new VFXCatalogMapping(VFXEventType.GenericBurst, "Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/Magic Hit 2.prefab", 4, 1f, false, true, true, Vector3.zero, new Vector3(-90f, 0f, 0f), 0.9f, true),

            new VFXCatalogMapping(VFXEventType.AbilityAttackBuff, "Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Weapon Effect.prefab", 4, 1.15f, false, false, true, Vector3.zero, Vector3.zero, 0.75f, true),
            new VFXCatalogMapping(VFXEventType.AbilityAttackSpeedBuff, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Nature/CFXR4 Wind Trails.prefab", 4, 0.85f, false, true, true, Vector3.zero, Vector3.zero, 0.55f, true),
            new VFXCatalogMapping(VFXEventType.AbilityDefenseBuff, "Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_LightPillar.prefab", 3, 1.1f, false, false, true, Vector3.zero, Vector3.zero, 0.72f, true),
            new VFXCatalogMapping(VFXEventType.AbilityMoveSpeedBuff, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Nature/CFXR4 Wind Trails.prefab", 4, 0.85f, false, true, true, Vector3.zero, Vector3.zero, 0.65f, true),
            new VFXCatalogMapping(VFXEventType.AbilityHealthBuff, "Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Greenlight_shrink.prefab", 4, 1.1f, false, false, true, Vector3.zero, Vector3.zero, 0.9f, true),
            new VFXCatalogMapping(VFXEventType.AbilityDashBuff, "Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/Lightning Hit Blue.prefab", 8, 0.95f, false, true, true, Vector3.zero, new Vector3(-90f, 0f, 0f), 0.72f, true),
            new VFXCatalogMapping(VFXEventType.AbilityDashShockwave, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Impacts/CFXR2 Ground Hit.prefab", 6, 0.95f, false, true, true, Vector3.zero, Vector3.zero, 0.95f, true),
            new VFXCatalogMapping(VFXEventType.AbilityDashPhase, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Misc/CFXR Flash.prefab", 4, 0.6f, false, true, true, Vector3.zero, Vector3.zero, 0.8f, true),
            new VFXCatalogMapping(VFXEventType.AbilityDashStagger, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Impacts/CFXR Hit D 3D (Yellow).prefab", 6, 0.75f, false, true, true, Vector3.zero, Vector3.zero, 0.65f, true),
            new VFXCatalogMapping(VFXEventType.AbilityProjectileBuff, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Impacts/CFXR Impact Glowing HDR (Blue).prefab", 6, 0.8f, false, true, true, Vector3.zero, Vector3.zero, 0.55f, true),
            new VFXCatalogMapping(VFXEventType.AbilityProjectileSplit, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Magic Misc/CFXR4 Bouncing Glows Bubble (Blue Purple).prefab", 4, 0.95f, false, true, true, Vector3.zero, Vector3.zero, 0.55f, true),
            new VFXCatalogMapping(VFXEventType.AbilityProjectilePierce, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Impacts/CFXR Impact Glowing HDR (Blue).prefab", 6, 0.75f, false, true, true, Vector3.zero, Vector3.zero, 0.62f, true),
            new VFXCatalogMapping(VFXEventType.AbilityProjectileRicochet, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Magic Misc/CFXR4 Bouncing Glows Bubble (Blue Purple).prefab", 4, 0.9f, false, true, true, Vector3.zero, Vector3.zero, 0.62f, true),
            new VFXCatalogMapping(VFXEventType.AbilityProjectileHoming, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Magic Misc/CFXR4 Bouncing Glows Bubble (Blue Purple).prefab", 4, 0.9f, false, true, true, Vector3.zero, Vector3.zero, 0.58f, true),
            new VFXCatalogMapping(VFXEventType.AbilityProjectileSize, "Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/Magic Hit 2.prefab", 6, 0.85f, false, true, true, Vector3.zero, new Vector3(-90f, 0f, 0f), 0.8f, true),
            new VFXCatalogMapping(VFXEventType.AbilityFireProc, "Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/Fire Hit .prefab", 8, 0.9f, false, true, true, Vector3.zero, new Vector3(-90f, 0f, 0f), 0.8f, true),
            new VFXCatalogMapping(VFXEventType.AbilityPoisonProc, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Misc/CFXR2 Poison Cloud.prefab", 5, 1.2f, false, true, true, Vector3.zero, Vector3.zero, 0.52f, true),
            new VFXCatalogMapping(VFXEventType.AbilityIceProc, "Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/Ice Hit .prefab", 8, 0.9f, false, true, true, Vector3.zero, new Vector3(-90f, 0f, 0f), 0.8f, true),
            new VFXCatalogMapping(VFXEventType.AbilityLightningProc, "Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/Lightning Hit Blue.prefab", 8, 0.9f, false, true, true, Vector3.zero, new Vector3(-90f, 0f, 0f), 0.82f, true),
            new VFXCatalogMapping(VFXEventType.AbilityShield, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Nature/CFXR3 Shield Leaves A (Lit).prefab", 3, 1.2f, false, false, true, Vector3.zero, Vector3.zero, 0.75f, true),
            new VFXCatalogMapping(VFXEventType.AbilitySoulHeal, "Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Greenlight_shrink.prefab", 4, 1.1f, false, false, true, Vector3.zero, Vector3.zero, 0.82f, true),
            new VFXCatalogMapping(VFXEventType.AbilityBossBreaker, "Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/1.2/Basic Hit 8  (NEW).prefab", 4, 1.1f, false, true, true, Vector3.zero, new Vector3(-90f, 0f, 0f), 1.1f, true),
            new VFXCatalogMapping(VFXEventType.AbilityLowHealthSurge, "Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Purple_Hit_02.prefab", 3, 1.1f, false, false, true, Vector3.zero, Vector3.zero, 0.78f, true),
            new VFXCatalogMapping(VFXEventType.AbilityRewardLuck, "Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Magic Door_Gold.prefab", 2, 1.2f, false, false, true, Vector3.zero, Vector3.zero, 0.55f, true),
            new VFXCatalogMapping(VFXEventType.AbilityPickupFrenzy, "Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_LootDrop_Blue.prefab", 3, 1.2f, false, false, true, Vector3.zero, Vector3.zero, 0.62f, true),
            new VFXCatalogMapping(VFXEventType.AbilityOrbital, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Sword Trails/Plain/CFXR4 Sword Trail PLAIN (360 Thin Spiral).prefab", 3, 1.2f, false, false, true, Vector3.zero, Vector3.zero, 0.62f, true),
            new VFXCatalogMapping(VFXEventType.AbilityDrone, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Magic Misc/CFXR4 Bouncing Glows Bubble (Blue Purple).prefab", 3, 1.1f, false, true, true, Vector3.zero, Vector3.zero, 0.68f, true),
            new VFXCatalogMapping(VFXEventType.AbilityBladeStrike, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Sword Trails/Plain/CFXR4 Sword Hit PLAIN (Cross).prefab", 4, 0.9f, false, true, true, Vector3.zero, Vector3.zero, 0.7f, true),
            new VFXCatalogMapping(VFXEventType.AbilityMeteor, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Magic Misc/CFXR4 Falling Stars.prefab", 2, 1.35f, false, true, true, Vector3.zero, Vector3.zero, 0.7f, true),
            new VFXCatalogMapping(VFXEventType.AbilityEnergyBeam, "Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_LightPillar.prefab", 3, 1.05f, false, false, true, Vector3.zero, Vector3.zero, 0.82f, true),
            new VFXCatalogMapping(VFXEventType.AbilityEnergyRing, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Magic Misc/CFXR3 Magic Aura A (Runic).prefab", 3, 1.25f, false, false, true, Vector3.zero, Vector3.zero, 0.72f, true),
            new VFXCatalogMapping(VFXEventType.AbilityRevive, "Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_LightPillar.prefab", 2, 1.35f, false, false, true, Vector3.zero, Vector3.zero, 1f, true),
            new VFXCatalogMapping(VFXEventType.AbilityInvulnerability, "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Light/CFXR3 LightGlow A (Loop).prefab", 2, 1.25f, false, false, true, Vector3.zero, Vector3.zero, 0.7f, true),
            new VFXCatalogMapping(VFXEventType.AbilityGenericUpgrade, "Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/Magic Hit 2.prefab", 4, 0.95f, false, true, true, Vector3.zero, new Vector3(-90f, 0f, 0f), 0.82f, true)
        };

        private readonly struct VFXCatalogMapping
        {
            public VFXCatalogMapping(
                VFXEventType eventType,
                string prefabPath,
                int initialPoolSize,
                float defaultLifetime,
                bool parentToRequestParent,
                bool useRequestRotation,
                bool useRequestScale,
                Vector3 positionOffset,
                Vector3 rotationOffsetEuler,
                float scaleMultiplier,
                bool allowColorOverride)
            {
                EventType = eventType;
                PrefabPath = prefabPath;
                InitialPoolSize = initialPoolSize;
                DefaultLifetime = defaultLifetime;
                ParentToRequestParent = parentToRequestParent;
                UseRequestRotation = useRequestRotation;
                UseRequestScale = useRequestScale;
                PositionOffset = positionOffset;
                RotationOffsetEuler = rotationOffsetEuler;
                ScaleMultiplier = scaleMultiplier;
                AllowColorOverride = allowColorOverride;
            }

            public VFXEventType EventType { get; }
            public string PrefabPath { get; }
            public int InitialPoolSize { get; }
            public float DefaultLifetime { get; }
            public bool ParentToRequestParent { get; }
            public bool UseRequestRotation { get; }
            public bool UseRequestScale { get; }
            public Vector3 PositionOffset { get; }
            public Vector3 RotationOffsetEuler { get; }
            public float ScaleMultiplier { get; }
            public bool AllowColorOverride { get; }
        }

        private readonly struct ScoredCandidate
        {
            public ScoredCandidate(VFXPrefabAuditInfo candidate, int score)
            {
                Candidate = candidate;
                Score = score;
            }

            public VFXPrefabAuditInfo Candidate { get; }
            public int Score { get; }
        }
    }

    public readonly struct VFXPrefabAuditInfo
    {
        public VFXPrefabAuditInfo(
            string assetPath,
            int particleSystemCount,
            int loopingParticleSystemCount,
            int maxParticlesTotal,
            float maxDuration,
            int missingScriptCount,
            int nonUnityScriptCount,
            int missingMaterialCount,
            bool hasVisualEffectLikeComponent)
        {
            AssetPath = assetPath;
            ParticleSystemCount = particleSystemCount;
            LoopingParticleSystemCount = loopingParticleSystemCount;
            MaxParticlesTotal = maxParticlesTotal;
            MaxDuration = maxDuration;
            MissingScriptCount = missingScriptCount;
            NonUnityScriptCount = nonUnityScriptCount;
            MissingMaterialCount = missingMaterialCount;
            HasVisualEffectLikeComponent = hasVisualEffectLikeComponent;
        }

        public string AssetPath { get; }
        public int ParticleSystemCount { get; }
        public int LoopingParticleSystemCount { get; }
        public int MaxParticlesTotal { get; }
        public float MaxDuration { get; }
        public int MissingScriptCount { get; }
        public int NonUnityScriptCount { get; }
        public int MissingMaterialCount { get; }
        public bool HasVisualEffectLikeComponent { get; }

        public bool HasRisk => LoopingParticleSystemCount > 0
            || MaxParticlesTotal > 2500
            || MaxDuration > 2.5f
            || MissingScriptCount > 0
            || NonUnityScriptCount > 0
            || MissingMaterialCount > 0
            || HasVisualEffectLikeComponent;

        public string RiskSummary
        {
            get
            {
                var builder = new StringBuilder();
                AppendRisk(builder, LoopingParticleSystemCount > 0, $"looping PS {LoopingParticleSystemCount}");
                AppendRisk(builder, MaxParticlesTotal > 2500, $"high maxParticles {MaxParticlesTotal}");
                AppendRisk(builder, MaxDuration > 2.5f, $"long duration {MaxDuration:0.##}");
                AppendRisk(builder, MissingScriptCount > 0, $"missing scripts {MissingScriptCount}");
                AppendRisk(builder, NonUnityScriptCount > 0, $"third-party scripts {NonUnityScriptCount}");
                AppendRisk(builder, MissingMaterialCount > 0, $"missing materials {MissingMaterialCount}");
                AppendRisk(builder, HasVisualEffectLikeComponent, "VisualEffect-like component");
                return builder.Length > 0 ? builder.ToString() : "none";
            }
        }

        private static void AppendRisk(StringBuilder builder, bool condition, string text)
        {
            if (!condition)
            {
                return;
            }

            if (builder.Length > 0)
            {
                builder.Append(", ");
            }

            builder.Append(text);
        }
    }
}
