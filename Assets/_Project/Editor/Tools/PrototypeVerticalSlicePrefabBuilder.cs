#if UNITY_EDITOR
using TapKnockout.Pickups;
using TapKnockout.Survivor;
using TapKnockout.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TapKnockout.Editor.Tools
{
    public static class PrototypeVerticalSlicePrefabBuilder
    {
        public const string XPOrbPrefabPath = "Assets/_Project/Prefabs/Pickups/PF_XPOrb_Prototype.prefab";
        public const string SpawnTelegraphPrefabPath = "Assets/_Project/Prefabs/VFX/PF_SpawnTelegraphCircle_Prototype.prefab";
        public const string DamageNumberPrefabPath = "Assets/_Project/Prefabs/UI/PF_DamageNumber_Prototype.prefab";

        private const string XPOrbMaterialPath = "Assets/_Project/Materials/M_XPOrb_Prototype.mat";
        private const string SpawnTelegraphMaterialPath = "Assets/_Project/Materials/M_SpawnTelegraph_Prototype.mat";
        private const string SelectedSpawnTelegraphVFXPath = "Assets/ThirdParty/VFX/Eric VFX Studio/Game VFX - Magic Circle(Free)/Prefabs/FX_MagicCircle_Icearrow01.prefab";
        private const string DesktopSurvivorPrototypeScenePath = "Assets/_Project/Scenes/DesktopSurvivorPrototype.unity";
        private const string ForestPrototypeScenePath = "Assets/_Project/Scenes/DesktopSurvivorPrototype_ForestArena.unity";

        [MenuItem("Tap Knockout/Survivor/Wire Prototype Feedback Prefabs In Current Scene")]
        public static void WireCurrentSceneMenu()
        {
            VFXFeedbackSetupBuilder.CreateFeedbackSystemRoot();
            EnsureAndWireCurrentScene();
            Debug.Log("Ensured and wired prototype XP orb, spawn telegraph, and damage number prefabs in the current scene.");
        }

        [MenuItem("Tap Knockout/Survivor/Wire Prototype Feedback Prefabs In Prototype Scenes")]
        public static void WirePrototypeScenesMenu()
        {
            var originalScenePath = EditorSceneManager.GetActiveScene().path;
            WireSceneIfPresent(DesktopSurvivorPrototypeScenePath);
            WireSceneIfPresent(ForestPrototypeScenePath);

            if (!string.IsNullOrWhiteSpace(originalScenePath) &&
                AssetDatabase.LoadAssetAtPath<SceneAsset>(originalScenePath) != null)
            {
                EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
            }

            Debug.Log("Ensured and wired prototype feedback prefabs in available prototype scenes.");
        }

        public static void EnsureAndWireCurrentScene()
        {
            var xpOrbPrefab = EnsureXPOrbPrefab();
            var spawnTelegraphPrefab = EnsureSpawnTelegraphPrefab();
            var damageNumberPrefab = EnsureDamageNumberPrefab();

            WireSceneReferences(
                Object.FindFirstObjectByType<ArenaRunDirector>(),
                Object.FindFirstObjectByType<SurvivorSpawnDirector>(),
                Object.FindFirstObjectByType<DamageNumberSpawner>(),
                xpOrbPrefab,
                spawnTelegraphPrefab,
                damageNumberPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
            }
        }

        public static void EnsureAndWireSceneReferences(
            ArenaRunDirector runDirector,
            SurvivorSpawnDirector spawnDirector,
            DamageNumberSpawner damageNumberSpawner)
        {
            WireSceneReferences(
                runDirector,
                spawnDirector,
                damageNumberSpawner,
                EnsureXPOrbPrefab(),
                EnsureSpawnTelegraphPrefab(),
                EnsureDamageNumberPrefab());
            AssetDatabase.SaveAssets();
        }

        public static XPOrb EnsureXPOrbPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<XPOrb>(XPOrbPrefabPath);
            if (existing != null)
            {
                return existing;
            }

            EnsureFolder("Assets/_Project/Prefabs", "Pickups");
            EnsureFolder("Assets/_Project", "Materials");

            var root = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            root.name = "PF_XPOrb_Prototype";
            root.layer = IgnoreRaycastLayer();
            root.transform.localScale = Vector3.one * 0.6f;

            var renderer = root.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = EnsureMaterial(
                    XPOrbMaterialPath,
                    new Color(0.25f, 0.9f, 1f, 1f),
                    false);
            }

            var collider = root.GetComponent<SphereCollider>();
            if (collider != null)
            {
                collider.isTrigger = true;
                collider.radius = 0.55f;
            }

            var rigidbody = root.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            var orb = root.AddComponent<XPOrb>();
            SetInt(orb, "xpAmount", 1);
            SetFloat(orb, "lifetimeSeconds", 20f);
            SetFloat(orb, "attractionSpeed", 8f);
            SetFloat(orb, "attractionAcceleration", 22f);

            PrefabUtility.SaveAsPrefabAsset(root, XPOrbPrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.ImportAsset(XPOrbPrefabPath);
            return AssetDatabase.LoadAssetAtPath<XPOrb>(XPOrbPrefabPath);
        }

        public static GameObject EnsureSpawnTelegraphPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(SpawnTelegraphPrefabPath);
            if (existing != null)
            {
                EnsureSpawnTelegraphPrefabUsesSelectedVFX();
                return AssetDatabase.LoadAssetAtPath<GameObject>(SpawnTelegraphPrefabPath);
            }

            EnsureFolder("Assets/_Project/Prefabs", "VFX");
            EnsureFolder("Assets/_Project", "Materials");

            var root = new GameObject("PF_SpawnTelegraphCircle_Prototype");
            ConfigureSpawnTelegraphRoot(root);
            AddSelectedSpawnTelegraphVFXChild(root.transform);

            PrefabUtility.SaveAsPrefabAsset(root, SpawnTelegraphPrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.ImportAsset(SpawnTelegraphPrefabPath);
            return AssetDatabase.LoadAssetAtPath<GameObject>(SpawnTelegraphPrefabPath);
        }

        private static void EnsureSpawnTelegraphPrefabUsesSelectedVFX()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(SpawnTelegraphPrefabPath) == null)
            {
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(SpawnTelegraphPrefabPath);
            try
            {
                ConfigureSpawnTelegraphRoot(root);
                AddSelectedSpawnTelegraphVFXChild(root.transform);
                PrefabUtility.SaveAsPrefabAsset(root, SpawnTelegraphPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ConfigureSpawnTelegraphRoot(GameObject root)
        {
            root.layer = IgnoreRaycastLayer();

            var lineRenderer = root.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = root.AddComponent<LineRenderer>();
            }

            lineRenderer.loop = true;
            lineRenderer.useWorldSpace = false;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.widthMultiplier = 0.045f;
            lineRenderer.positionCount = 64;
            lineRenderer.sharedMaterial = EnsureMaterial(
                SpawnTelegraphMaterialPath,
                new Color(1f, 0.42f, 0.08f, 0.85f),
                true);

            for (var i = 0; i < lineRenderer.positionCount; i++)
            {
                var angle = i / (float)lineRenderer.positionCount * Mathf.PI * 2f;
                lineRenderer.SetPosition(i, new Vector3(Mathf.Cos(angle) * 0.85f, 0f, Mathf.Sin(angle) * 0.85f));
            }

            var marker = root.GetComponent<SpawnTelegraphMarker>();
            if (marker == null)
            {
                marker = root.AddComponent<SpawnTelegraphMarker>();
            }

            SetInt(marker, "segments", 64);
            SetFloat(marker, "radius", 0.85f);
            SetFloat(marker, "lineWidth", 0.045f);
            SetFloat(marker, "heightOffset", 0.06f);
            SetColor(marker, "color", new Color(1f, 0.42f, 0.08f, 0.85f));
        }

        private static void AddSelectedSpawnTelegraphVFXChild(Transform root)
        {
            const string childName = "SelectedVFX_FX_MagicCircle_Icearrow01";
            if (root.Find(childName) != null)
            {
                return;
            }

            var selectedVfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SelectedSpawnTelegraphVFXPath);
            if (selectedVfxPrefab == null)
            {
                return;
            }

            var child = PrefabUtility.InstantiatePrefab(selectedVfxPrefab, root) as GameObject;
            if (child == null)
            {
                child = Object.Instantiate(selectedVfxPrefab);
                child.transform.SetParent(root, false);
            }

            child.name = childName;
            SetLayerRecursively(child, IgnoreRaycastLayer());
            child.transform.localPosition = Vector3.up * 0.02f;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one * 0.55f;
            RemoveColliders(child);
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            if (root == null)
            {
                return;
            }

            root.layer = layer;
            for (var i = 0; i < root.transform.childCount; i++)
            {
                SetLayerRecursively(root.transform.GetChild(i).gameObject, layer);
            }
        }

        private static void RemoveColliders(GameObject root)
        {
            var colliders = root.GetComponentsInChildren<Collider>(true);
            for (var i = colliders.Length - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(colliders[i]);
            }
        }

        public static DamageNumberView EnsureDamageNumberPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<DamageNumberView>(DamageNumberPrefabPath);
            if (existing != null)
            {
                return existing;
            }

            EnsureFolder("Assets/_Project/Prefabs", "UI");

            var root = new GameObject("PF_DamageNumber_Prototype", typeof(RectTransform));
            var rectTransform = root.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(112f, 44f);
            var canvasGroup = root.AddComponent<CanvasGroup>();

            var labelObject = new GameObject("Label", typeof(RectTransform));
            labelObject.transform.SetParent(root.transform, false);
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var label = labelObject.AddComponent<Text>();
            label.alignment = TextAnchor.MiddleCenter;
            label.fontSize = 28;
            label.fontStyle = FontStyle.Bold;
            label.raycastTarget = false;
            label.color = Color.white;
            label.font = ResolveRuntimeFont();

            var view = root.AddComponent<DamageNumberView>();
            SetObjectReference(view, "label", label);
            SetObjectReference(view, "canvasGroup", canvasGroup);
            SetFloat(view, "lifetime", 0.65f);
            SetVector2(view, "drift", new Vector2(0f, 72f));

            PrefabUtility.SaveAsPrefabAsset(root, DamageNumberPrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.ImportAsset(DamageNumberPrefabPath);
            return AssetDatabase.LoadAssetAtPath<DamageNumberView>(DamageNumberPrefabPath);
        }

        private static void WireSceneIfPresent(string scenePath)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            VFXFeedbackSetupBuilder.CreateFeedbackSystemRoot();
            EnsureAndWireCurrentScene();
            EditorSceneManager.SaveScene(scene);
        }

        private static void WireSceneReferences(
            ArenaRunDirector runDirector,
            SurvivorSpawnDirector spawnDirector,
            DamageNumberSpawner damageNumberSpawner,
            XPOrb xpOrbPrefab,
            GameObject spawnTelegraphPrefab,
            DamageNumberView damageNumberPrefab)
        {
            if (runDirector != null)
            {
                SetObjectReference(runDirector, "xpOrbPrefab", xpOrbPrefab);
            }

            if (spawnDirector != null)
            {
                SetObjectReference(spawnDirector, "spawnTelegraphPrefab", spawnTelegraphPrefab);
                SetBool(spawnDirector, "enableSpawnTelegraph", true);
                SetFloat(spawnDirector, "spawnTelegraphDuration", Mathf.Max(ReadFloat(spawnDirector, "spawnTelegraphDuration", 0.45f), 0.45f));
                SetInt(spawnDirector, "maxConcurrentSpawnTelegraphs", Mathf.Max(ReadInt(spawnDirector, "maxConcurrentSpawnTelegraphs", 12), 16));
            }

            if (damageNumberSpawner != null)
            {
                SetObjectReference(damageNumberSpawner, "numberPrefab", damageNumberPrefab);
                SetBool(damageNumberSpawner, "createRuntimeFallbackPrefab", true);

                if (ReadObject<Canvas>(damageNumberSpawner, "targetCanvas") == null)
                {
                    SetObjectReference(damageNumberSpawner, "targetCanvas", Object.FindFirstObjectByType<Canvas>());
                }
            }
        }

        private static Material EnsureMaterial(string path, Color color, bool transparent)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            var shader = Shader.Find("Universal Render Pipeline/Unlit") ??
                Shader.Find("Unlit/Color") ??
                Shader.Find("Sprites/Default") ??
                Shader.Find("Standard") ??
                Shader.Find("Hidden/InternalErrorShader");
            if (shader == null)
            {
                Debug.LogWarning($"Could not find a shader for {path}. The prefab will keep Unity's default material.");
                return null;
            }

            material = new Material(shader)
            {
                name = System.IO.Path.GetFileNameWithoutExtension(path)
            };

            ApplyMaterialColor(material, color);
            if (transparent)
            {
                ConfigureTransparentMaterial(material);
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void ApplyMaterialColor(Material material, Color color)
        {
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_EmissionColor"))
            {
                material.SetColor("_EmissionColor", color * 0.75f);
                material.EnableKeyword("_EMISSION");
            }
        }

        private static void ConfigureTransparentMaterial(Material material)
        {
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
            }

            if (material.HasProperty("_Mode"))
            {
                material.SetFloat("_Mode", 3f);
            }

            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        private static Font ResolveRuntimeFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static int IgnoreRaycastLayer()
        {
            var layer = LayerMask.NameToLayer("Ignore Raycast");
            return layer >= 0 ? layer : 2;
        }

        private static void EnsureFolder(string parent, string child)
        {
            var path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static T ReadObject<T>(Object target, string propertyName)
            where T : Object
        {
            if (target == null)
            {
                return null;
            }

            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            return property != null && property.propertyType == SerializedPropertyType.ObjectReference
                ? property.objectReferenceValue as T
                : null;
        }

        private static float ReadFloat(Object target, string propertyName, float fallback)
        {
            if (target == null)
            {
                return fallback;
            }

            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            return property != null && property.propertyType == SerializedPropertyType.Float ? property.floatValue : fallback;
        }

        private static int ReadInt(Object target, string propertyName, int fallback)
        {
            if (target == null)
            {
                return fallback;
            }

            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            return property != null && property.propertyType == SerializedPropertyType.Integer ? property.intValue : fallback;
        }

        private static void SetObjectReference(Object target, string propertyName, Object value)
        {
            SetSerializedProperty(target, propertyName, property =>
            {
                if (property.propertyType == SerializedPropertyType.ObjectReference)
                {
                    property.objectReferenceValue = value;
                }
            });
        }

        private static void SetBool(Object target, string propertyName, bool value)
        {
            SetSerializedProperty(target, propertyName, property =>
            {
                if (property.propertyType == SerializedPropertyType.Boolean)
                {
                    property.boolValue = value;
                }
            });
        }

        private static void SetFloat(Object target, string propertyName, float value)
        {
            SetSerializedProperty(target, propertyName, property =>
            {
                if (property.propertyType == SerializedPropertyType.Float)
                {
                    property.floatValue = value;
                }
            });
        }

        private static void SetInt(Object target, string propertyName, int value)
        {
            SetSerializedProperty(target, propertyName, property =>
            {
                if (property.propertyType == SerializedPropertyType.Integer)
                {
                    property.intValue = value;
                }
            });
        }

        private static void SetColor(Object target, string propertyName, Color value)
        {
            SetSerializedProperty(target, propertyName, property =>
            {
                if (property.propertyType == SerializedPropertyType.Color)
                {
                    property.colorValue = value;
                }
            });
        }

        private static void SetVector2(Object target, string propertyName, Vector2 value)
        {
            SetSerializedProperty(target, propertyName, property =>
            {
                if (property.propertyType == SerializedPropertyType.Vector2)
                {
                    property.vector2Value = value;
                }
            });
        }

        private static void SetSerializedProperty(Object target, string propertyName, System.Action<SerializedProperty> assign)
        {
            if (target == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                return;
            }

            assign(property);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
