using TapKnockout.UI.HUD;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Editor.Tools
{
    /// <summary>
    /// Validates the Player Health HUD setup without modifying anything.
    /// Menu: Tools/Tap Knockout/UI/Validate Player Health HUD
    /// </summary>
    public static class PlayerHealthHudValidator
    {
        private const string ConfigPath = "Assets/_Project/Generated/UI/HUD/PlayerHealthHudConfig.asset";
        private const string PrefabPath = "Assets/_Project/Prefabs/UI/PF_PlayerHealthHUD_TopLeft.prefab";

        [MenuItem("Tools/Tap Knockout/UI/Validate Player Health HUD")]
        public static void ValidatePlayerHealthHud()
        {
            var results = new System.Text.StringBuilder();
            var allPassed = true;

            // ── Config ──────────────────────────────────────────────
            results.AppendLine("=== Player Health HUD Validation ===\n");
            results.AppendLine("[Config]");

            var config = AssetDatabase.LoadAssetAtPath<PlayerHealthHudConfig>(ConfigPath);
            if (config == null)
            {
                results.AppendLine($"  ✗ Config asset not found at: {ConfigPath}");
                results.AppendLine("    → Run Tools/Tap Knockout/UI/Build Player Health HUD first.");
                allPassed = false;
            }
            else
            {
                results.AppendLine($"  ✓ Config found: {ConfigPath}");

                if (config.FrameSprite != null)
                    results.AppendLine($"  ✓ FrameSprite: {config.FrameSprite.name}");
                else
                {
                    results.AppendLine("  ⚠ FrameSprite is null — builder may not have found health meter sprites.");
                    allPassed = false;
                }

                if (config.IsValid(out var configError))
                    results.AppendLine("  ✓ Config values valid.");
                else
                {
                    results.AppendLine($"  ✗ Config invalid: {configError}");
                    allPassed = false;
                }
            }

            // ── Prefab ──────────────────────────────────────────────
            results.AppendLine("\n[Prefab]");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                results.AppendLine($"  ✗ Prefab not found at: {PrefabPath}");
                allPassed = false;
            }
            else
            {
                results.AppendLine($"  ✓ Prefab found: {PrefabPath}");

                var view = prefab.GetComponentInChildren<PlayerHealthHudView>(true);
                if (view != null)
                    results.AppendLine("  ✓ PlayerHealthHudView present");
                else
                {
                    results.AppendLine("  ✗ PlayerHealthHudView missing from prefab.");
                    allPassed = false;
                }

                var presenter = prefab.GetComponentInChildren<PlayerHealthHudPresenter>(true);
                if (presenter != null)
                    results.AppendLine("  ✓ PlayerHealthHudPresenter present");
                else
                {
                    results.AppendLine("  ✗ PlayerHealthHudPresenter missing from prefab.");
                    allPassed = false;
                }

                var buffSlots = prefab.GetComponentsInChildren<PlayerHealthHudBuffSlot>(true);
                if (buffSlots != null && buffSlots.Length >= 4)
                    results.AppendLine($"  ✓ Buff slots: {buffSlots.Length} found");
                else
                {
                    results.AppendLine($"  ✗ Expected >= 4 buff slots, found: {buffSlots?.Length ?? 0}");
                    allPassed = false;
                }

                // Check for required child hierarchy
                CheckChild(prefab.transform, "SafeAreaRoot", ref results, ref allPassed);
            }

            // ── Scene ──────────────────────────────────────────────
            results.AppendLine("\n[Scene]");
            var sceneInstance = Object.FindAnyObjectByType<PlayerHealthHudView>();
            if (sceneInstance != null)
                results.AppendLine($"  ✓ HUD instance found in scene: '{sceneInstance.gameObject.name}'");
            else
                results.AppendLine("  ⚠ No PlayerHealthHudView found in active scene (place prefab to add one).");

            // ── Summary ─────────────────────────────────────────────
            results.AppendLine($"\n{'─', -48}");
            results.AppendLine(allPassed ? "✓ ALL CHECKS PASSED" : "✗ SOME CHECKS FAILED — see above.");

            var report = results.ToString();
            Debug.Log($"[PlayerHealthHudValidator]\n{report}");

            EditorUtility.DisplayDialog(
                allPassed ? "HUD Validation Passed ✓" : "HUD Validation Issues ✗",
                report,
                "OK");
        }

        private static void CheckChild(Transform parent, string childName,
            ref System.Text.StringBuilder sb, ref bool allPassed)
        {
            var found = FindDeep(parent, childName);
            if (found != null)
                sb.AppendLine($"  ✓ '{childName}' child present");
            else
            {
                sb.AppendLine($"  ✗ '{childName}' child missing from prefab.");
                allPassed = false;
            }
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                var found = FindDeep(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
