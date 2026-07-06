using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace TapKnockout.EditorTools
{
    public class FixFbxLoops
    {
        [MenuItem("TapKnockout/Extract and Loop Enemy Animations")]
        public static void ExtractEnemyAnimations()
        {
            // Define where we want to save the extracted clips
            string targetFolder = "Assets/_Project/Animation/Clips/Enemy";
            if (!AssetDatabase.IsValidFolder(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
                AssetDatabase.Refresh();
            }

            // Find all FBX files in the enemy assets folder
            string[] searchFolders = new string[] { "Assets/Assets/game asset packs/Cute Animated Monsters - Aug 2020/FBX" };
            string[] guids = AssetDatabase.FindAssets("t:Model", searchFolders);

            int extractedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.ToLower().EndsWith(".fbx")) continue;

                string fbxName = Path.GetFileNameWithoutExtension(path);
                
                // Create a subfolder for each enemy
                string enemyFolder = $"{targetFolder}/{fbxName}";
                if (!AssetDatabase.IsValidFolder(enemyFolder))
                {
                    Directory.CreateDirectory(enemyFolder);
                    AssetDatabase.Refresh();
                }

                // Load all assets inside the FBX
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

                foreach (Object asset in assets)
                {
                    if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                    {
                        // Duplicate the clip
                        AnimationClip newClip = new AnimationClip();
                        EditorUtility.CopySerialized(clip, newClip);

                        string clipName = clip.name.ToLower();
                        
                        // Set loop time for locomotion/idle
                        if (clipName.Contains("idle") || clipName.Contains("walk") || clipName.Contains("run") || 
                            clipName.Contains("move") || clipName.Contains("dash") || clipName.Contains("take 001"))
                        {
                            if (!clipName.Contains("attack") && !clipName.Contains("hit") && !clipName.Contains("death") && !clipName.Contains("die"))
                            {
                                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(newClip);
                                settings.loopTime = true;
                                AnimationUtility.SetAnimationClipSettings(newClip, settings);
                            }
                        }

                        // Save the new clip
                        string clipPath = $"{enemyFolder}/{fbxName}_{clip.name}.anim";
                        AssetDatabase.CreateAsset(newClip, clipPath);
                        extractedCount++;
                        Debug.Log($"[TapKnockout] Extracted: {clipPath}");
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Animations Extracted", $"Successfully extracted {extractedCount} .anim clips into {targetFolder}.\n\nYou can now assign these directly to your Enemy Animator Controllers!", "OK");
        }
    }
}
