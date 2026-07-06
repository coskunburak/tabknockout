using System.IO;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Editor.Tools
{
    public static class TelegraphPlaceholderBuilder
    {
        private const string MenuPath = "Tools/Tap Knockout/Enemies/Create Telegraph Placeholder Prefabs";
        private const string TelegraphFolder = "Assets/_Project/Prefabs/Telegraphs";
        private const string MaterialFolder = "Assets/_Project/Art/Materials";

        [MenuItem(MenuPath)]
        public static void CreateTelegraphPlaceholderPrefabs()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Telegraph Placeholders", "Exit Play Mode before creating telegraph prefabs.", "OK");
                return;
            }

            var created = CreateAllTelegraphPlaceholderPrefabs();
            EditorUtility.DisplayDialog("Telegraph Placeholders", $"Telegraph placeholder prefabs ready. Created/updated: {created}.", "OK");
        }

        public static int CreateAllTelegraphPlaceholderPrefabs()
        {
            EnsureFolder(TelegraphFolder);
            EnsureFolder(MaterialFolder);
            var created = 0;
            created += CreateTelegraphPrefab("PF_Telegraph_Circle", new Vector3(2f, 0.02f, 2f), PrimitiveType.Cylinder) ? 1 : 0;
            created += CreateTelegraphPrefab("PF_Telegraph_Line", new Vector3(0.75f, 0.02f, 4f), PrimitiveType.Cube) ? 1 : 0;
            created += CreateTelegraphPrefab("PF_Telegraph_ChargePath", new Vector3(1.1f, 0.02f, 6f), PrimitiveType.Cube) ? 1 : 0;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return created;
        }

        private static bool CreateTelegraphPrefab(string prefabName, Vector3 scale, PrimitiveType primitiveType)
        {
            var path = $"{TelegraphFolder}/{prefabName}.prefab";
            var existed = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
            var root = existed ? PrefabUtility.LoadPrefabContents(path) : GameObject.CreatePrimitive(primitiveType);
            root.name = prefabName;
            root.transform.localScale = scale;
            var collider = root.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            var renderer = root.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = GetOrCreateMaterial("MAT_Telegraph_Playtest", new Color(1f, 0.22f, 0.08f, 0.55f));
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
            if (existed)
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            else
            {
                Object.DestroyImmediate(root);
            }

            Debug.Log($"{nameof(TelegraphPlaceholderBuilder)} Done: {path}");
            return true;
        }

        private static Material GetOrCreateMaterial(string materialName, Color color)
        {
            var path = $"{MaterialFolder}/{materialName}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"))
            {
                name = materialName,
                color = color
            };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            if (!string.IsNullOrWhiteSpace(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent ?? "Assets", Path.GetFileName(folderPath));
        }
    }
}
