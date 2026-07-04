using UnityEngine;
using UnityEditor;
using System.IO;

namespace TapKnockout.EditorTools
{
    public class CheckClipNames
    {
        [MenuItem("TapKnockout/Check FBX Clip Names")]
        public static void Check()
        {
            string path = "Assets/Assets/game asset packs/Cute Animated Monsters - Aug 2020/FBX/GreenDemon.fbx";
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) return;
            
            string output = "";
            foreach (var clip in importer.defaultClipAnimations)
            {
                output += clip.name + "\n";
            }
            File.WriteAllText("GreenDemonClips.txt", output);
            Debug.Log("Wrote clips to GreenDemonClips.txt");
        }
    }
}
