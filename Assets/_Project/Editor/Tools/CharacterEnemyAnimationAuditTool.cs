using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TapKnockout.Characters;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Editor.Tools
{
    public static class CharacterEnemyAnimationAuditTool
    {
        private static readonly string[] ScanRoots =
        {
            "Assets/_Project",
            "Assets/Assets/game asset packs",
            "Assets/ThirdParty"
        };

        [MenuItem("Tools/Tap Knockout/Characters/Audit Character Enemy Animations")]
        public static void AuditCharacterEnemyAnimations()
        {
            var report = BuildAuditReport();
            WriteTextAsset(CharacterEnemyAssetSelection.ReportPath, report);
            Debug.Log(report);
            AssetDatabase.Refresh();
        }

        public static string BuildAuditReportForTests()
        {
            return BuildAuditReport();
        }

        private static string BuildAuditReport()
        {
            var modelPaths = FindAssetPaths("t:GameObject", IsModelPath);
            var prefabPaths = FindAssetPaths("t:Prefab", IsPrefabPath);
            var animatorControllerPaths = FindAssetPaths("t:AnimatorController", _ => true);
            var animationClipPaths = FindAssetPaths("t:AnimationClip", _ => true);
            var texturePaths = FindAssetPaths("t:Texture2D", path => path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".tga", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".psd", StringComparison.OrdinalIgnoreCase));

            var report = new StringBuilder();
            report.AppendLine("# Character Enemy Animation Audit");
            report.AppendLine();
            report.AppendLine("## Scan Roots");
            for (var i = 0; i < ScanRoots.Length; i++)
            {
                report.AppendLine($"- `{ScanRoots[i]}`");
            }

            report.AppendLine();
            report.AppendLine("## Asset Counts");
            report.AppendLine($"- Model assets: {modelPaths.Count}");
            report.AppendLine($"- Project/gameplay prefabs: {prefabPaths.Count}");
            report.AppendLine($"- Animator Controllers: {animatorControllerPaths.Count}");
            report.AppendLine($"- Animation Clips: {animationClipPaths.Count}");
            report.AppendLine($"- Textures: {texturePaths.Count}");

            report.AppendLine();
            report.AppendLine("## Selected Player Candidate");
            AppendSpec(report, CharacterEnemyAssetSelection.Player);

            report.AppendLine();
            report.AppendLine("## Selected Enemy Candidates");
            for (var i = 0; i < CharacterEnemyAssetSelection.Enemies.Length; i++)
            {
                AppendSpec(report, CharacterEnemyAssetSelection.Enemies[i]);
            }

            report.AppendLine();
            report.AppendLine("## Existing Gameplay Prefab Validation");
            AppendPrefabValidation(report, "Player Generated", CharacterEnemyAssetSelection.Player.GeneratedPrefabPath, false, true);
            if (CharacterEnemyAssetSelection.Enemies.Length > 0)
            {
                AppendPrefabValidation(report, "Enemy BasicMelee Generated", CharacterEnemyAssetSelection.Enemies[0].GeneratedPrefabPath, false, false);
            }

            report.AppendLine();
            report.AppendLine("## Animation Source Notes");
            report.AppendLine("- Character packs contain FBX model assets with `importAnimation` enabled, but no project-owned Animator Controllers were found for player/enemy gameplay yet.");
            report.AppendLine("- KayKit animation FBXs are available as animation sources, but clip compatibility must be validated in Unity before retargeting.");
            report.AppendLine("- This tool does not retarget or mutate source clips.");

            report.AppendLine();
            report.AppendLine("## Mobile Suitability Notes");
            report.AppendLine("- Selected assets are low-poly/stylized and use simple texture atlases, suitable for portrait mobile readability.");
            report.AppendLine("- Do not keep demo lights, cameras, particle children, or unnecessary helper objects inside generated gameplay prefabs.");
            report.AppendLine("- Validate material count and texture import sizes in Unity Inspector before production build.");

            return report.ToString();
        }

        private static void AppendSpec(StringBuilder report, CharacterEnemyAssetSpec spec)
        {
            report.AppendLine($"### {spec.DisplayName}");
            report.AppendLine($"- Role: `{spec.RoleId}`");
            report.AppendLine($"- Score: {spec.Score}");
            report.AppendLine($"- Visual asset: `{spec.VisualAssetPath}`");
            report.AppendLine($"- Generated prefab: `{spec.GeneratedPrefabPath}`");
            if (spec.HeldWeapon.HasAsset)
            {
                report.AppendLine($"- Held weapon: `{spec.HeldWeapon.AssetPath}` on `{spec.HeldWeapon.SocketName}`");
            }

            report.AppendLine($"- Requires ProjectileSpawnPoint: {spec.RequiresProjectileSpawnPoint}");
            report.AppendLine($"- Rationale: {spec.Rationale}");
            report.AppendLine($"- Visual asset exists: {AssetDatabase.LoadAssetAtPath<GameObject>(spec.VisualAssetPath) != null}");
            if (string.IsNullOrWhiteSpace(spec.BasePrefabPath))
            {
                report.AppendLine("- Base prefab: none; prefab builder creates a project-owned runtime skeleton.");
            }
            else
            {
                report.AppendLine($"- Base prefab exists: {AssetDatabase.LoadAssetAtPath<GameObject>(spec.BasePrefabPath) != null}");
            }
        }

        private static void AppendPrefabValidation(StringBuilder report, string label, string prefabPath, bool requiresProjectileSocket, bool isPlayer)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var result = isPlayer
                ? CharacterEnemyPrefabValidation.ValidatePlayer(prefab)
                : CharacterEnemyPrefabValidation.ValidateEnemy(prefab, requiresProjectileSocket);

            report.AppendLine($"### {label}");
            report.AppendLine($"- Path: `{prefabPath}`");
            report.AppendLine($"- Valid: {result.IsValid}");

            if (result.IsValid)
            {
                report.AppendLine("- Issues: none");
                return;
            }

            for (var i = 0; i < result.Issues.Count; i++)
            {
                report.AppendLine($"- {result.Issues[i].Code}: {result.Issues[i].Message}");
            }
        }

        private static List<string> FindAssetPaths(string filter, Func<string, bool> predicate)
        {
            var paths = new List<string>();
            var guids = AssetDatabase.FindAssets(filter, ScanRoots);
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (predicate(path))
                {
                    paths.Add(path);
                }
            }

            paths.Sort(StringComparer.Ordinal);
            return paths;
        }

        private static bool IsModelPath(string path)
        {
            return path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".blend", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".obj", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsPrefabPath(string path)
        {
            return path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)
                && path.IndexOf("/VFX/", StringComparison.OrdinalIgnoreCase) < 0;
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
    }
}
