#if UNITY_EDITOR
using System.Collections.Generic;
using TapKnockout.Camera;
using TapKnockout.Editor.Tools;
using TapKnockout.Survivor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TapKnockout.Editor
{
    public static class ForestSurvivorArenaBuilder
    {
        private const string BuildMenuPath = "Tap Knockout/Survivor/Build Forest Survivor Arena Scene";
        private const string ValidateMenuPath = "Tap Knockout/Survivor/Validate Forest Arena Scene";
        private const string PrototypeScenePath = "Assets/_Project/Scenes/DesktopSurvivorPrototype.unity";
        private const string ForestScenePath = "Assets/_Project/Scenes/DesktopSurvivorPrototype_ForestArena.unity";
        private const string ForestArenaPrefabPath = "Assets/_Project/Prefabs/Arena/ForestSurvivorArena.prefab";
        private const string ForestArenaConfigPath = "Assets/_Project/ScriptableObjects/Arenas/ArenaConfig_ForestSurvivorArena.asset";
        private const string PrototypeRunConfigPath = "Assets/_Project/ScriptableObjects/Runs/RunConfig_DesktopSurvivorPrototype.asset";
        private const string ForestRunConfigPath = "Assets/_Project/ScriptableObjects/Runs/RunConfig_ForestSurvivorArena.asset";
        private const string ForestModelFolder = "Assets/Assets/game asset packs/KayKit_Forest_Nature_Pack_1 1.0_FREE/Assets/fbx(unity)";
        private const string DungeonModelFolder = "Assets/Assets/game asset packs/KayKit_DungeonRemastered_1.1_FREE/Assets/fbx(unity)";
        private const string MaterialFolder = "Assets/_Project/Art/Materials/Generated";
        private const string ForestRootName = "ForestSurvivorArena";

        private const float ArenaRadius = 34f;
        private const float GroundSize = 76f;
        private const float BorderRadius = 37.5f;
        private const float InnerClearRadius = 10.75f;
        private const float PrimaryLaneHalfWidth = 3.35f;
        private const int GroundLayer = 0;
        private const int BlockerLayer = 2;

        private static readonly Vector3 CameraOffset = new Vector3(0f, 36f, -8f);
        private static readonly Vector3 CameraLookAtOffset = new Vector3(0f, 0.35f, 0f);

        [MenuItem(BuildMenuPath)]
        public static void BuildForestArenaSceneMenu()
        {
            BuildForestArenaScene();
        }

        public static void BuildForestArenaSceneBatch()
        {
            BuildForestArenaScene();
        }

        [MenuItem(ValidateMenuPath)]
        public static void ValidateForestArenaSceneMenu()
        {
            var report = ValidateForestArenaScene();
            if (report.Count == 0)
            {
                Debug.Log("Forest survivor arena validation passed.");
                return;
            }

            Debug.LogWarning("Forest survivor arena validation found issues:\n" + string.Join("\n", report));
        }

        public static List<string> ValidateForestArenaScene()
        {
            var issues = new List<string>();
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != ForestScenePath)
            {
                issues.Add($"Active scene is '{scene.path}'. Expected '{ForestScenePath}'.");
            }

            var arenaRoot = GameObject.Find(ForestRootName);
            if (arenaRoot == null)
            {
                issues.Add($"{ForestRootName} is missing.");
                return issues;
            }

            RequireChild(issues, arenaRoot.transform, "ForestArena_Ground");
            RequireChild(issues, arenaRoot.transform, "ForestArena_Borders");
            RequireChild(issues, arenaRoot.transform, "ForestArena_Decor");
            RequireChild(issues, arenaRoot.transform, "ForestArena_Landmarks");
            RequireChild(issues, arenaRoot.transform, "ForestArena_SpawnZones");
            RequireChild(issues, arenaRoot.transform, "ForestArena_Blockers");
            RequireChild(issues, arenaRoot.transform, "ForestArena_Lighting");

            var ground = arenaRoot.transform.Find("ForestArena_Ground/WalkableForestGround");
            if (ground == null || ground.GetComponent<Collider>() == null)
            {
                issues.Add("WalkableForestGround with a collider is missing.");
            }

            var visualGround = arenaRoot.transform.Find("ForestArena_Ground");
            if (visualGround == null || visualGround.childCount < 24)
            {
                issues.Add("ForestArena_Ground should contain layered visual ground, trails, and canopy shadow patches.");
            }

            var borders = arenaRoot.transform.Find("ForestArena_Borders");
            if (borders == null || borders.GetComponentsInChildren<Transform>(true).Length < 180)
            {
                issues.Add("ForestArena_Borders should contain dense multi-layer forest silhouettes.");
            }

            var landmarks = arenaRoot.transform.Find("ForestArena_Landmarks");
            if (landmarks == null || landmarks.childCount < 6 || landmarks.GetComponentsInChildren<Transform>(true).Length < 70)
            {
                issues.Add("ForestArena_Landmarks should contain at least six dressed production landmarks.");
            }

            var lighting = arenaRoot.transform.Find("ForestArena_Lighting");
            if (lighting == null || lighting.GetComponentsInChildren<Light>(true).Length < 5)
            {
                issues.Add("ForestArena_Lighting should contain the key light and landmark accent lights.");
            }

            var blockers = arenaRoot.transform.Find("ForestArena_Blockers");
            if (blockers == null || blockers.GetComponentsInChildren<Collider>(true).Length < 8)
            {
                issues.Add("ForestArena_Blockers should contain at least 8 simple boundary colliders.");
            }

            var spawnZones = arenaRoot.transform.Find("ForestArena_SpawnZones");
            if (spawnZones == null || spawnZones.childCount < 8)
            {
                issues.Add("ForestArena_SpawnZones should contain at least 8 edge helper anchors.");
            }

            var config = AssetDatabase.LoadAssetAtPath<ArenaConfig>(ForestArenaConfigPath);
            if (config == null)
            {
                issues.Add($"Forest arena config is missing at {ForestArenaConfigPath}.");
            }
            else
            {
                if (config.SpawnPressureMode != SpawnPressureMode.Mixed && config.SpawnPressureMode != SpawnPressureMode.EdgePressure)
                {
                    issues.Add("Forest arena config should use Mixed or EdgePressure spawn pressure.");
                }

                if (config.PlayerAvoidSpawnRadius < 7f)
                {
                    issues.Add("Forest arena player avoid spawn radius should be at least 7.");
                }

                if (config.SpawnBlockerLayers.value == 0)
                {
                    issues.Add("Forest arena spawn blocker layers are empty.");
                }
            }

            var runDirector = Object.FindAnyObjectByType<ArenaRunDirector>();
            var spawnDirector = Object.FindAnyObjectByType<SurvivorSpawnDirector>();
            if (runDirector == null)
            {
                issues.Add("ArenaRunDirector is missing.");
            }

            if (spawnDirector == null)
            {
                issues.Add("SurvivorSpawnDirector is missing.");
            }

            return issues;
        }

        private static void BuildForestArenaScene()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("Stop Play Mode before building the forest survivor arena.");
                return;
            }

            EnsureFolder(MaterialFolder);
            EnsureFolder("Assets/_Project/Scenes");
            EnsureFolder("Assets/_Project/Prefabs/Arena");
            EnsureFolder("Assets/_Project/ScriptableObjects/Arenas");
            EnsureFolder("Assets/_Project/ScriptableObjects/Runs");

            if (!AssetDatabase.IsValidFolder(ForestModelFolder))
            {
                Debug.LogWarning($"KayKit forest model folder was not found at {ForestModelFolder}. Primitive fallbacks will be used.");
            }

            if (!AssetDatabase.IsValidFolder(DungeonModelFolder))
            {
                Debug.LogWarning($"KayKit dungeon model folder was not found at {DungeonModelFolder}. Ruin landmark fallbacks will be used.");
            }

            EnsureForestSceneAsset();
            var scene = EditorSceneManager.OpenScene(ForestScenePath, OpenSceneMode.Single);
            var arenaConfig = CreateOrUpdateArenaConfig();
            var runConfig = CreateOrUpdateRunConfig(arenaConfig);
            AbilityCatalogBuilder.CreateOrUpdateVerticalSliceCatalog();
            AbilityCatalogBuilder.WireRunConfigsToVerticalSliceCatalog();

            var prototypeRoot = GameObject.Find("DesktopSurvivorPrototypeRoot");
            var arenaRoot = ResolveArenaRoot(prototypeRoot);
            DisableCopiedPrototypeArena(arenaRoot.transform);
            var forestRoot = ReplaceForestArena(arenaRoot.transform);

            ConfigureSceneRuntime(arenaConfig, runConfig);
            ConfigureCamera();
            MarkEnvironmentStatic(forestRoot);

            PrefabUtility.SaveAsPrefabAssetAndConnect(forestRoot, ForestArenaPrefabPath, InteractionMode.AutomatedAction);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ForestScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var validationIssues = ValidateForestArenaScene();
            if (validationIssues.Count == 0)
            {
                Debug.Log($"Built forest survivor arena scene at {ForestScenePath}, prefab at {ForestArenaPrefabPath}, and config at {ForestArenaConfigPath}.", forestRoot);
            }
            else
            {
                Debug.LogWarning("Built forest survivor arena, but validation reported:\n" + string.Join("\n", validationIssues), forestRoot);
            }
        }

        private static void EnsureForestSceneAsset()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ForestScenePath) != null)
            {
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(PrototypeScenePath) == null)
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                _ = new GameObject("DesktopSurvivorPrototypeRoot");
                EditorSceneManager.SaveScene(scene, ForestScenePath);
                return;
            }

            AssetDatabase.CopyAsset(PrototypeScenePath, ForestScenePath);
            AssetDatabase.ImportAsset(ForestScenePath);
        }

        private static ArenaConfig CreateOrUpdateArenaConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<ArenaConfig>(ForestArenaConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<ArenaConfig>();
                AssetDatabase.CreateAsset(config, ForestArenaConfigPath);
            }

            var serializedObject = new SerializedObject(config);
            SetString(serializedObject, "arenaId", "forest_survivor_arena");
            SetVector3(serializedObject, "arenaCenter", Vector3.zero);
            SetFloat(serializedObject, "arenaRadius", ArenaRadius);
            SetFloat(serializedObject, "playerSafeSpawnRadius", 7f);
            SetFloat(serializedObject, "enemySpawnMinRadiusFromPlayer", 11f);
            SetFloat(serializedObject, "enemySpawnMaxRadiusFromPlayer", 31f);
            SetInt(serializedObject, "maxLiveEnemies", 110);
            SetEnum(serializedObject, "spawnPressureMode", (int)SpawnPressureMode.Mixed);
            SetFloat(serializedObject, "playerAvoidSpawnRadius", 7.5f);
            SetFloat(serializedObject, "edgeSpawnInnerRadiusFactor", 0.78f);
            SetFloat(serializedObject, "mixedEdgePressureChance", 0.68f);
            SetInt(serializedObject, "spawnPositionRetries", 28);
            SetFloat(serializedObject, "spawnClearanceRadius", 0.75f);
            SetLayerMask(serializedObject, "spawnBlockerLayers", 1 << BlockerLayer);
            SetBool(serializedObject, "fallbackToArenaEdgeWhenInvalid", true);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            return config;
        }

        private static RunConfig CreateOrUpdateRunConfig(ArenaConfig arenaConfig)
        {
            var runConfig = AssetDatabase.LoadAssetAtPath<RunConfig>(ForestRunConfigPath);
            if (runConfig == null)
            {
                var source = AssetDatabase.LoadAssetAtPath<RunConfig>(PrototypeRunConfigPath);
                runConfig = source != null ? Object.Instantiate(source) : ScriptableObject.CreateInstance<RunConfig>();
                AssetDatabase.CreateAsset(runConfig, ForestRunConfigPath);
            }

            var serializedObject = new SerializedObject(runConfig);
            SetString(serializedObject, "runId", "run_forest_survivor_arena");
            SetObject(serializedObject, "arenaConfig", arenaConfig);
            SetInt(serializedObject, "startingEnemyCap", 16);
            SetInt(serializedObject, "maxEnemyCap", 100);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(runConfig);
            return runConfig;
        }

        private static GameObject ResolveArenaRoot(GameObject prototypeRoot)
        {
            var arenaRoot = GameObject.Find("ArenaRoot");
            if (arenaRoot != null)
            {
                return arenaRoot;
            }

            arenaRoot = new GameObject("ArenaRoot");
            if (prototypeRoot != null)
            {
                arenaRoot.transform.SetParent(prototypeRoot.transform, false);
            }

            return arenaRoot;
        }

        private static void DisableCopiedPrototypeArena(Transform arenaRoot)
        {
            DisableChild(arenaRoot, "PrototypeGround");
            DisableChild(arenaRoot, "KayKitSurvivorArena_Generated");
        }

        private static void DisableChild(Transform parent, string childName)
        {
            var child = parent != null ? parent.Find(childName) : null;
            if (child == null)
            {
                return;
            }

            child.gameObject.SetActive(false);
            EditorUtility.SetDirty(child.gameObject);
        }

        private static GameObject ReplaceForestArena(Transform arenaRoot)
        {
            var existing = arenaRoot.Find(ForestRootName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            var root = CreateChild(arenaRoot, ForestRootName);
            var ground = CreateChild(root.transform, "ForestArena_Ground");
            var borders = CreateChild(root.transform, "ForestArena_Borders");
            var decor = CreateChild(root.transform, "ForestArena_Decor");
            var landmarks = CreateChild(root.transform, "ForestArena_Landmarks");
            var spawnZones = CreateChild(root.transform, "ForestArena_SpawnZones");
            var blockers = CreateChild(root.transform, "ForestArena_Blockers");
            var lighting = CreateChild(root.transform, "ForestArena_Lighting");

            BuildGround(ground.transform);
            BuildBorders(borders.transform);
            BuildDecor(decor.transform);
            BuildLandmarks(landmarks.transform);
            BuildSpawnZones(spawnZones.transform);
            BuildBlockers(blockers.transform);
            BuildLighting(lighting.transform);
            return root;
        }

        private static void BuildGround(Transform parent)
        {
            var groundMaterial = GetOrCreateMaterial("MAT_ForestArena_Ground", new Color(0.18f, 0.34f, 0.19f, 1f));
            var mossMaterial = GetOrCreateMaterial("MAT_ForestArena_MossDepth", new Color(0.12f, 0.25f, 0.16f, 1f));
            var clearingMaterial = GetOrCreateMaterial("MAT_ForestArena_Clearing", new Color(0.38f, 0.34f, 0.23f, 1f));
            var leafMaterial = GetOrCreateMaterial("MAT_ForestArena_LeafLitter", new Color(0.33f, 0.28f, 0.18f, 1f));
            var pathMaterial = GetOrCreateMaterial("MAT_ForestArena_Path", new Color(0.48f, 0.39f, 0.25f, 1f));
            var shadowMaterial = GetOrCreateMaterial("MAT_ForestArena_CanopyShadow", new Color(0.08f, 0.18f, 0.13f, 1f));

            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "WalkableForestGround";
            ground.layer = GroundLayer;
            ground.transform.SetParent(parent, false);
            ground.transform.localPosition = new Vector3(0f, -0.08f, 0f);
            ground.transform.localScale = new Vector3(GroundSize, 0.12f, GroundSize);
            if (ground.TryGetComponent<Renderer>(out var renderer))
            {
                renderer.enabled = false;
            }

            CreateGroundDisk(parent, "VisualForestFloor_DarkOuterMass", new Vector3(0f, -0.066f, 0f), new Vector3(72f, 0.035f, 68f), mossMaterial, -3f);
            CreateGroundDisk(parent, "VisualForestFloor_MainMoss", new Vector3(0f, -0.052f, 0f), new Vector3(66f, 0.03f, 61f), groundMaterial, 9f);
            CreateGroundDisk(parent, "CentralCombatClearing_Decal", new Vector3(0f, 0.006f, 0f), new Vector3(24f, 0.012f, 20f), clearingMaterial, 14f);
            CreateGroundDisk(parent, "CentralCombatClearing_WornEdge", new Vector3(0f, 0.008f, 0f), new Vector3(16f, 0.012f, 13.5f), leafMaterial, -18f);

            CreatePath(parent, "AncientTrail_WestApproach", new Vector3(-17f, 0.018f, -0.9f), new Vector3(33f, 0.03f, 4.7f), pathMaterial, 88f);
            CreatePath(parent, "AncientTrail_EastApproach", new Vector3(17f, 0.02f, 1.1f), new Vector3(33f, 0.03f, 4.4f), pathMaterial, 94f);
            CreatePath(parent, "HunterTrail_NorthApproach", new Vector3(-2.1f, 0.022f, 16.6f), new Vector3(4.3f, 0.03f, 32f), pathMaterial, -8f);
            CreatePath(parent, "HunterTrail_SouthApproach", new Vector3(2.2f, 0.024f, -16.1f), new Vector3(4.5f, 0.03f, 31f), pathMaterial, 7f);
            CreatePath(parent, "BossApproachPath_NorthEast", new Vector3(16.3f, 0.026f, 15f), new Vector3(4.15f, 0.03f, 29f), pathMaterial, -39f);
            CreatePath(parent, "BrokenCartTrail_SouthWest", new Vector3(-17.8f, 0.025f, -16.2f), new Vector3(3.7f, 0.03f, 24f), pathMaterial, -47f);

            for (var i = 0; i < 18; i++)
            {
                var angle = i * 360f / 18f + 5f;
                var radius = 28.5f + (i % 4) * 1.2f;
                var position = Polar(radius, angle);
                var scale = new Vector3(10.5f + (i % 5) * 1.6f, 0.01f, 5.5f + (i % 3) * 1.4f);
                CreateGroundDisk(parent, $"CanopyShadowPatch_{i:00}", new Vector3(position.x, 0.011f, position.z), scale, shadowMaterial, angle + 28f);
            }

            for (var i = 0; i < 14; i++)
            {
                var angle = i * 360f / 14f + 17f;
                var radius = 16.5f + (i % 5) * 2.1f;
                var position = Polar(radius, angle);
                if (IsInReadableCombatLane(position, 1.5f))
                {
                    position += Polar(4.2f, angle + 78f);
                }

                var material = (i & 1) == 0 ? leafMaterial : clearingMaterial;
                var scale = new Vector3(5.6f + (i % 4) * 0.9f, 0.01f, 2.9f + (i % 3) * 0.6f);
                CreateGroundDisk(parent, $"LeafLitterRunPatch_{i:00}", new Vector3(position.x, 0.014f, position.z), scale, material, angle * 1.7f);
            }
        }

        private static void CreateGroundDisk(Transform parent, string name, Vector3 position, Vector3 scale, Material material, float yRotation = 0f)
        {
            var disk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disk.name = name;
            disk.layer = GroundLayer;
            disk.transform.SetParent(parent, false);
            disk.transform.localPosition = position;
            disk.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
            disk.transform.localScale = scale;
            if (disk.TryGetComponent<Renderer>(out var renderer))
            {
                renderer.sharedMaterial = material;
            }

            RemoveVisualColliders(disk);
        }

        private static void CreatePath(Transform parent, string name, Vector3 position, Vector3 scale, Material material, float yRotation = 0f)
        {
            var path = GameObject.CreatePrimitive(PrimitiveType.Cube);
            path.name = name;
            path.layer = GroundLayer;
            path.transform.SetParent(parent, false);
            path.transform.localPosition = position;
            path.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
            path.transform.localScale = scale;
            if (path.TryGetComponent<Renderer>(out var renderer))
            {
                renderer.sharedMaterial = material;
            }

            if (path.TryGetComponent<Collider>(out var collider))
            {
                Object.DestroyImmediate(collider);
            }
        }

        private static void BuildBorders(Transform parent)
        {
            var treeNames = new[]
            {
                "Tree_1_A_Color1", "Tree_1_B_Color1", "Tree_1_C_Color1",
                "Tree_2_A_Color1", "Tree_2_B_Color1", "Tree_2_C_Color1", "Tree_2_D_Color1", "Tree_2_E_Color1",
                "Tree_3_A_Color1", "Tree_3_B_Color1", "Tree_3_C_Color1",
                "Tree_4_A_Color1", "Tree_4_B_Color1", "Tree_4_C_Color1",
                "Tree_Bare_1_A_Color1", "Tree_Bare_1_B_Color1", "Tree_Bare_1_C_Color1",
                "Tree_Bare_2_A_Color1", "Tree_Bare_2_B_Color1", "Tree_Bare_2_C_Color1"
            };

            var bushNames = new[]
            {
                "Bush_1_A_Color1", "Bush_1_C_Color1", "Bush_1_E_Color1",
                "Bush_2_A_Color1", "Bush_2_C_Color1", "Bush_2_E_Color1",
                "Bush_3_A_Color1", "Bush_3_C_Color1",
                "Bush_4_A_Color1", "Bush_4_C_Color1", "Bush_4_E_Color1"
            };

            for (var i = 0; i < 96; i++)
            {
                var angle = i * 360f / 96f + Mathf.Sin(i * 0.71f) * 1.8f;
                var radius = BorderRadius + OrganicRadiusOffset(i, angle, 1.65f);
                var position = Polar(radius, angle);
                var treeName = treeNames[Mathf.Abs(i * 7 + 3) % treeNames.Length];
                var tree = InstantiateForestModel(treeName, $"OuterCanopyTree_{i:00}", parent, position, angle + 180f + Mathf.Sin(i) * 11f, Vector3.one * ResolveTreeScale(i));
                RemoveVisualColliders(tree);
            }

            for (var i = 0; i < 72; i++)
            {
                var angle = i * 360f / 72f + 2.5f + Mathf.Cos(i * 0.63f) * 2.4f;
                var radius = BorderRadius - 4.6f + OrganicRadiusOffset(i + 37, angle, 1.25f);
                var position = Polar(radius, angle);
                var bush = InstantiateForestModel(bushNames[Mathf.Abs(i * 5 + 1) % bushNames.Length], $"InnerBorderBush_{i:00}", parent, position, angle, Vector3.one * ResolveBushScale(i));
                RemoveVisualColliders(bush);
            }

            for (var i = 0; i < 24; i++)
            {
                var angle = i * 360f / 24f + 7f;
                var radius = BorderRadius - 7.2f + (i % 3) * 0.8f;
                var position = Polar(radius, angle);
                var treeName = treeNames[Mathf.Abs(i * 11 + 5) % treeNames.Length];
                var tree = InstantiateForestModel(treeName, $"MidlineSilhouetteTree_{i:00}", parent, position, angle + 151f, Vector3.one * (0.78f + (i % 5) * 0.06f));
                RemoveVisualColliders(tree);
            }

            PlaceBorderCluster(parent, "NorthOldGrowthCluster", 0f, 4, treeNames, bushNames);
            PlaceBorderCluster(parent, "NorthEastOldGrowthCluster", 45f, 5, treeNames, bushNames);
            PlaceBorderCluster(parent, "SouthEastOldGrowthCluster", 128f, 7, treeNames, bushNames);
            PlaceBorderCluster(parent, "SouthOldGrowthCluster", 181f, 3, treeNames, bushNames);
            PlaceBorderCluster(parent, "SouthWestOldGrowthCluster", 226f, 9, treeNames, bushNames);
            PlaceBorderCluster(parent, "NorthWestOldGrowthCluster", 311f, 6, treeNames, bushNames);
        }

        private static float ResolveTreeScale(int index)
        {
            return 0.92f + (index % 5) * 0.075f + Mathf.Abs(Mathf.Sin(index * 1.37f)) * 0.09f;
        }

        private static float ResolveBushScale(int index)
        {
            return 0.85f + (index % 5) * 0.07f;
        }

        private static float OrganicRadiusOffset(int index, float angle, float strength)
        {
            return (Mathf.Sin(angle * 0.071f + index * 0.53f) + Mathf.Cos(angle * 0.113f + index * 0.31f)) * strength;
        }

        private static void PlaceBorderCluster(Transform parent, string name, float angle, int seed, string[] treeNames, string[] bushNames)
        {
            var cluster = CreateChild(parent, name);
            var center = Polar(BorderRadius - 3.6f + (seed % 3) * 0.55f, angle);
            var tangent = Quaternion.Euler(0f, angle + 90f, 0f);
            for (var i = 0; i < 7; i++)
            {
                var offset = tangent * new Vector3((i - 3) * 1.45f, 0f, -0.9f - (i % 3) * 1.1f);
                var treeName = treeNames[Mathf.Abs(seed * 13 + i * 5) % treeNames.Length];
                var tree = InstantiateForestModel(treeName, $"ClusterTree_{i:00}", cluster.transform, center + offset, angle + i * 31f, Vector3.one * (0.9f + (i % 4) * 0.08f));
                RemoveVisualColliders(tree);
            }

            for (var i = 0; i < 8; i++)
            {
                var offset = Quaternion.Euler(0f, angle + i * 37f, 0f) * new Vector3(1.7f + (i % 4) * 0.75f, 0f, 0.6f + (i % 2) * 0.6f);
                var bush = InstantiateForestModel(bushNames[Mathf.Abs(seed * 7 + i * 3) % bushNames.Length], $"ClusterBush_{i:00}", cluster.transform, center + offset, angle + i * 23f, Vector3.one * (0.78f + (i % 5) * 0.06f));
                RemoveVisualColliders(bush);
            }
        }

        private static void BuildDecor(Transform parent)
        {
            var grassNames = new[]
            {
                "Grass_1_A_Color1", "Grass_1_B_Color1", "Grass_1_C_Color1", "Grass_1_D_Color1",
                "Grass_2_A_Color1", "Grass_2_B_Color1", "Grass_2_C_Color1", "Grass_2_D_Color1",
                "Grass_1_A_Singlesided_Color1", "Grass_1_C_Singlesided_Color1",
                "Grass_2_A_Singlesided_Color1", "Grass_2_C_Singlesided_Color1"
            };

            for (var i = 0; i < 170; i++)
            {
                var angle = i * 137.507f + (i % 6) * 9.5f;
                var radius = 11.4f + (i % 15) * 1.35f + ((i / 15) % 5) * 0.55f;
                var position = Polar(Mathf.Min(radius, 31.5f), angle);
                if (IsInReadableCombatLane(position, 1.25f))
                {
                    continue;
                }

                var grass = InstantiateForestModel(grassNames[Mathf.Abs(i * 3 + 2) % grassNames.Length], $"ReadableGrassPatch_{i:00}", parent, position, angle + i * 13f, Vector3.one * (0.62f + (i % 5) * 0.075f));
                RemoveVisualColliders(grass);
            }

            var rockNames = new[]
            {
                "Rock_1_A_Color1", "Rock_1_F_Color1", "Rock_1_M_Color1", "Rock_2_B_Color1",
                "Rock_2_G_Color1", "Rock_2_H_Color1", "Rock_3_D_Color1", "Rock_3_K_Color1",
                "Rock_3_M_Color1", "Rock_3_Q_Color1"
            };

            for (var i = 0; i < 34; i++)
            {
                var angle = i * 360f / 34f + 9f + Mathf.Sin(i * 0.9f) * 6f;
                var position = Polar(21.5f + (i % 6) * 1.7f, angle);
                if (IsInReadableCombatLane(position, 1.7f))
                {
                    position += Polar(3f, angle + 90f);
                }

                var rock = InstantiateForestModel(rockNames[Mathf.Abs(i * 5 + 1) % rockNames.Length], $"NonBlockingEdgeRock_{i:00}", parent, position, angle * 1.7f, Vector3.one * (0.58f + (i % 4) * 0.09f));
                RemoveVisualColliders(rock);
            }

            var bushNames = new[] { "Bush_1_B_Color1", "Bush_1_D_Color1", "Bush_2_B_Color1", "Bush_2_D_Color1", "Bush_3_A_Color1", "Bush_4_D_Color1" };
            for (var i = 0; i < 20; i++)
            {
                var angle = i * 360f / 20f + 13f;
                var position = Polar(18.5f + (i % 4) * 2.15f, angle);
                if (IsInReadableCombatLane(position, 2f))
                {
                    continue;
                }

                var bush = InstantiateForestModel(bushNames[Mathf.Abs(i * 7) % bushNames.Length], $"LowBushClusterAccent_{i:00}", parent, position, angle + 31f, Vector3.one * (0.72f + (i % 4) * 0.08f));
                RemoveVisualColliders(bush);
            }
        }

        private static bool IsInReadableCombatLane(Vector3 localPosition, float clearance)
        {
            var radius = new Vector2(localPosition.x, localPosition.z).magnitude;
            if (radius < InnerClearRadius + clearance)
            {
                return true;
            }

            if (Mathf.Abs(localPosition.x) < PrimaryLaneHalfWidth + clearance && Mathf.Abs(localPosition.z) < ArenaRadius - 2f)
            {
                return true;
            }

            if (Mathf.Abs(localPosition.z) < PrimaryLaneHalfWidth + clearance && Mathf.Abs(localPosition.x) < ArenaRadius - 2f)
            {
                return true;
            }

            var northEastTrailDistance = Mathf.Abs(localPosition.x - localPosition.z);
            if (localPosition.x > 4f && localPosition.z > 4f && northEastTrailDistance < 4.4f + clearance)
            {
                return true;
            }

            var southWestTrailDistance = Mathf.Abs(localPosition.x - localPosition.z);
            if (localPosition.x < -4f && localPosition.z < -4f && southWestTrailDistance < 4.1f + clearance)
            {
                return true;
            }

            return false;
        }

        private static void BuildLandmarks(Transform parent)
        {
            var northEast = CreateChild(parent, "Landmark_RuinedBossShrine_NorthEast");
            PlaceRuinedShrine(northEast.transform, new Vector3(22f, 0f, 22f), 35f);
            PlaceTreeCluster(northEast.transform, new Vector3(27.5f, 0f, 20f), 15f, "Tree_4_A_Color1", "Tree_3_C_Color1", "Tree_Bare_2_B_Color1");

            var north = CreateChild(parent, "Landmark_BrokenNorthGate");
            PlaceBrokenStoneGate(north.transform, new Vector3(-4f, 0f, 28f), -6f);
            PlaceGrassMeadow(north.transform, new Vector3(-11f, 0f, 25f), 10f);

            var east = CreateChild(parent, "Landmark_OvergrownStoneRing_East");
            PlaceStoneRing(east.transform, new Vector3(28f, 0f, -2f), 86f);

            var southEast = CreateChild(parent, "Landmark_FallenExpeditionCamp_SouthEast");
            PlaceFallenCamp(southEast.transform, new Vector3(22f, 0f, -22f), -28f);
            PlaceBushGrove(southEast.transform, new Vector3(25f, 0f, -20f), -20f);

            var southWest = CreateChild(parent, "Landmark_RockGateRubble_SouthWest");
            PlaceOvergrownRubble(southWest.transform, new Vector3(-24f, 0f, -18f), -45f);
            PlaceRockCluster(southWest.transform, new Vector3(-18f, 0f, -25f), 20f);

            var west = CreateChild(parent, "Landmark_BareTreeCircle_West");
            PlaceTreeCluster(west.transform, new Vector3(-28f, 0f, 4f), 75f, "Tree_Bare_1_A_Color1", "Tree_Bare_1_B_Color1", "Tree_Bare_2_C_Color1");
            PlaceBushGrove(west.transform, new Vector3(-25f, 0f, 8f), 45f);
        }

        private static void PlaceRuinedShrine(Transform parent, Vector3 center, float rotation)
        {
            var amberGround = GetOrCreateMaterial("MAT_ForestArena_RuinWarmDirt", new Color(0.45f, 0.31f, 0.18f, 1f));
            CreateGroundDisk(parent, "BossShrine_WarmClearingGround", new Vector3(center.x, 0.018f, center.z), new Vector3(12f, 0.01f, 8.5f), amberGround, rotation);

            PlaceDungeonProp(parent, "floor_tile_extralarge_grates_open", "BossShrine_CrackedStoneFloor", center + new Vector3(0f, 0.02f, 0f), rotation, Vector3.one * 1.35f);
            PlaceDungeonProp(parent, "pillar_decorated", "BossShrine_Pillar_A", center + RotateOffset(rotation, -3.7f, 0f, 2.7f), rotation + 8f, Vector3.one * 1.05f);
            PlaceDungeonProp(parent, "pillar", "BossShrine_Pillar_B", center + RotateOffset(rotation, 3.5f, 0f, 2.4f), rotation - 14f, Vector3.one * 0.95f);
            PlaceDungeonProp(parent, "wall_broken", "BossShrine_BrokenWall_Back", center + RotateOffset(rotation, 0f, 0f, 4.8f), rotation, Vector3.one * 1.1f);
            PlaceDungeonProp(parent, "wall_half_endcap", "BossShrine_FallenWall_Left", center + RotateOffset(rotation, -5.1f, 0f, -0.5f), rotation + 90f, Vector3.one);
            PlaceDungeonProp(parent, "rubble_large", "BossShrine_RubblePile", center + RotateOffset(rotation, 3.7f, 0f, -2.7f), rotation + 32f, Vector3.one * 1.1f);
            PlaceDungeonProp(parent, "torch_lit", "BossShrine_Torch_A", center + RotateOffset(rotation, -4.5f, 0f, 3.4f), rotation - 20f, Vector3.one);
            PlaceDungeonProp(parent, "torch_lit", "BossShrine_Torch_B", center + RotateOffset(rotation, 4.3f, 0f, 3.1f), rotation + 18f, Vector3.one);
        }

        private static void PlaceBrokenStoneGate(Transform parent, Vector3 center, float rotation)
        {
            var stoneDust = GetOrCreateMaterial("MAT_ForestArena_RuinStoneDust", new Color(0.36f, 0.35f, 0.30f, 1f));
            CreateGroundDisk(parent, "BrokenGate_StoneDustGround", new Vector3(center.x, 0.016f, center.z), new Vector3(14f, 0.01f, 6.5f), stoneDust, rotation);

            PlaceDungeonProp(parent, "wall_arched", "BrokenGate_ArchLeft", center + RotateOffset(rotation, -3.2f, 0f, 0.1f), rotation + 4f, Vector3.one);
            PlaceDungeonProp(parent, "wall_broken", "BrokenGate_BrokenRight", center + RotateOffset(rotation, 3.1f, 0f, 0.2f), rotation - 3f, Vector3.one);
            PlaceDungeonProp(parent, "rubble_half", "BrokenGate_Rubble_A", center + RotateOffset(rotation, -1.1f, 0f, -2.5f), rotation + 35f, Vector3.one);
            PlaceDungeonProp(parent, "floor_tile_small_broken_A", "BrokenGate_FloorShard_A", center + RotateOffset(rotation, 1.5f, 0.02f, -1.9f), rotation + 13f, Vector3.one * 1.15f);
            PlaceDungeonProp(parent, "torch_lit", "BrokenGate_Torch", center + RotateOffset(rotation, -4.9f, 0f, 0.7f), rotation + 90f, Vector3.one * 0.95f);
        }

        private static void PlaceStoneRing(Transform parent, Vector3 center, float rotation)
        {
            var mossStone = GetOrCreateMaterial("MAT_ForestArena_MossyStoneGround", new Color(0.24f, 0.30f, 0.24f, 1f));
            CreateGroundDisk(parent, "StoneRing_MossGround", new Vector3(center.x, 0.015f, center.z), new Vector3(9.5f, 0.01f, 9.5f), mossStone, rotation);

            for (var i = 0; i < 7; i++)
            {
                var angle = rotation + i * 360f / 7f;
                var offset = Polar(3.7f + (i % 2) * 0.35f, angle);
                var name = i % 3 == 0 ? "column" : i % 3 == 1 ? "barrier_column" : "rubble_half";
                PlaceDungeonProp(parent, name, $"StoneRing_Ruin_{i:00}", center + offset, angle + 90f, Vector3.one * (0.78f + (i % 3) * 0.08f));
            }

            PlaceRockCluster(parent, center + RotateOffset(rotation, 3.9f, 0f, -3.7f), rotation + 18f);
            PlaceBushGrove(parent, center + RotateOffset(rotation, -3.8f, 0f, 3.5f), rotation - 25f);
        }

        private static void PlaceFallenCamp(Transform parent, Vector3 center, float rotation)
        {
            var campGround = GetOrCreateMaterial("MAT_ForestArena_CampMud", new Color(0.30f, 0.26f, 0.18f, 1f));
            CreateGroundDisk(parent, "FallenCamp_MuddyGround", new Vector3(center.x, 0.016f, center.z), new Vector3(12.5f, 0.01f, 8f), campGround, rotation);

            PlaceDungeonProp(parent, "crates_stacked", "FallenCamp_Crates", center + RotateOffset(rotation, -2.3f, 0f, 1.1f), rotation + 21f, Vector3.one * 0.9f);
            PlaceDungeonProp(parent, "barrel_small_stack", "FallenCamp_Barrels", center + RotateOffset(rotation, 2.4f, 0f, 0.9f), rotation - 18f, Vector3.one * 0.9f);
            PlaceDungeonProp(parent, "table_long_broken", "FallenCamp_BrokenTable", center + RotateOffset(rotation, 0.2f, 0f, -1.4f), rotation + 6f, Vector3.one);
            PlaceDungeonProp(parent, "sword_shield_broken", "FallenCamp_BrokenGear", center + RotateOffset(rotation, 3.2f, 0f, -2.4f), rotation + 72f, Vector3.one);
            PlaceRockCluster(parent, center + RotateOffset(rotation, -4.3f, 0f, -1.8f), rotation - 12f);
        }

        private static void PlaceOvergrownRubble(Transform parent, Vector3 center, float rotation)
        {
            PlaceRockCluster(parent, center, rotation);
            PlaceDungeonProp(parent, "rubble_large", "RockGate_RubbleLarge", center + RotateOffset(rotation, 1.7f, 0f, 1.6f), rotation + 22f, Vector3.one * 1.1f);
            PlaceDungeonProp(parent, "wall_half", "RockGate_BuriedWall", center + RotateOffset(rotation, -3.2f, 0f, -1.3f), rotation - 7f, Vector3.one);
            PlaceBushGrove(parent, center + RotateOffset(rotation, -1.2f, 0f, 3.4f), rotation + 28f);
        }

        private static void PlaceRockCluster(Transform parent, Vector3 center, float rotation)
        {
            var names = new[] { "Rock_3_A_Color1", "Rock_3_E_Color1", "Rock_2_H_Color1", "Rock_1_N_Color1", "Rock_1_P_Color1" };
            var offsets = new[]
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(2.4f, 0f, 1.2f),
                new Vector3(-2.1f, 0f, 1.5f),
                new Vector3(1.1f, 0f, -2.2f),
                new Vector3(-1.9f, 0f, -1.5f)
            };

            for (var i = 0; i < names.Length; i++)
            {
                var rock = InstantiateForestModel(names[i], $"RockCluster_{i:00}", parent, center + Quaternion.Euler(0f, rotation, 0f) * offsets[i], rotation + i * 37f, Vector3.one * (0.95f + i * 0.05f));
                RemoveVisualColliders(rock);
            }
        }

        private static void PlaceTreeCluster(Transform parent, Vector3 center, float rotation, params string[] names)
        {
            var offsets = new[]
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(3.1f, 0f, 1.7f),
                new Vector3(-2.8f, 0f, -1.4f)
            };

            for (var i = 0; i < names.Length; i++)
            {
                var tree = InstantiateForestModel(names[i], $"LandmarkTree_{i:00}", parent, center + Quaternion.Euler(0f, rotation, 0f) * offsets[i], rotation + i * 51f, Vector3.one * (1.05f + i * 0.08f));
                RemoveVisualColliders(tree);
            }
        }

        private static void PlaceBushGrove(Transform parent, Vector3 center, float rotation)
        {
            var names = new[] { "Bush_4_A_Color1", "Bush_4_D_Color1", "Bush_3_B_Color1", "Bush_2_F_Color1", "Bush_1_G_Color1", "Grass_2_D_Color1" };
            for (var i = 0; i < names.Length; i++)
            {
                var offset = Quaternion.Euler(0f, rotation + i * 61f, 0f) * new Vector3(1.2f + (i % 3) * 1.1f, 0f, 0.4f + (i % 2) * 1.1f);
                var bush = InstantiateForestModel(names[i], $"GrovePlant_{i:00}", parent, center + offset, rotation + i * 29f, Vector3.one * (0.86f + (i % 4) * 0.08f));
                RemoveVisualColliders(bush);
            }
        }

        private static void PlaceGrassMeadow(Transform parent, Vector3 center, float rotation)
        {
            var names = new[] { "Grass_1_A_Color1", "Grass_1_D_Color1", "Grass_2_A_Color1", "Grass_2_D_Color1" };
            for (var i = 0; i < 18; i++)
            {
                var offset = Quaternion.Euler(0f, rotation + i * 37f, 0f) * new Vector3(1f + (i % 6) * 0.9f, 0f, (i % 3) * 0.65f);
                var grass = InstantiateForestModel(names[i % names.Length], $"MeadowGrass_{i:00}", parent, center + offset, rotation + i * 23f, Vector3.one * (0.7f + (i % 3) * 0.09f));
                RemoveVisualColliders(grass);
            }
        }

        private static void BuildSpawnZones(Transform parent)
        {
            var names = new[] { "North", "NorthNorthEast", "NorthEast", "EastNorthEast", "East", "SouthEast", "South", "SouthSouthWest", "SouthWest", "WestSouthWest", "West", "NorthWest" };
            for (var i = 0; i < names.Length; i++)
            {
                var angle = i * 360f / names.Length;
                var zone = CreateChild(parent, $"SpawnZone_{names[i]}_EdgePressure");
                zone.transform.localPosition = Polar(ArenaRadius * 0.88f, angle);
                zone.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
            }

            var bossZone = CreateChild(parent, "SpawnZone_BossElite_NorthEastClearing");
            bossZone.transform.localPosition = new Vector3(22f, 0f, 22f);
            bossZone.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);

            var northGateZone = CreateChild(parent, "SpawnZone_Elite_BrokenNorthGate");
            northGateZone.transform.localPosition = new Vector3(-4f, 0f, 28f);
            northGateZone.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

            var campZone = CreateChild(parent, "SpawnZone_Elite_FallenCamp");
            campZone.transform.localPosition = new Vector3(22f, 0f, -22f);
            campZone.transform.localRotation = Quaternion.Euler(0f, 135f, 0f);
        }

        private static void BuildBlockers(Transform parent)
        {
            const int segmentCount = 36;
            const float width = 8.2f;
            const float depth = 2.8f;
            for (var i = 0; i < segmentCount; i++)
            {
                var angle = i * 360f / segmentCount;
                var position = Polar(ArenaRadius + 2.2f + OrganicRadiusOffset(i, angle, 0.25f), angle);
                var blocker = CreateChild(parent, $"BoundaryBlocker_{i:00}");
                blocker.layer = BlockerLayer;
                blocker.transform.localPosition = new Vector3(position.x, 1.2f, position.z);
                blocker.transform.localRotation = Quaternion.Euler(0f, -angle, 0f);
                var collider = blocker.AddComponent<BoxCollider>();
                collider.size = new Vector3(width, 2.6f, depth);
            }

            CreateLandmarkBlocker(parent, "BossShrine_BackWallBlocker", new Vector3(23f, 0.8f, 24.5f), new Vector3(9f, 1.8f, 3.2f), 35f);
            CreateLandmarkBlocker(parent, "BrokenNorthGateBlocker", new Vector3(-4f, 0.85f, 28.5f), new Vector3(11f, 1.8f, 2.8f), -6f);
            CreateLandmarkBlocker(parent, "EastStoneRingBlocker_A", new Vector3(29.5f, 0.8f, -2f), new Vector3(3f, 1.6f, 8.2f), 86f);
            CreateLandmarkBlocker(parent, "FallenCampSupplyBlocker", new Vector3(22f, 0.75f, -22f), new Vector3(7.2f, 1.5f, 4.2f), -28f);
            CreateLandmarkBlocker(parent, "SouthWestRockGateBlocker_A", new Vector3(-24f, 0.8f, -18f), new Vector3(7f, 1.6f, 3.6f), -45f);
            CreateLandmarkBlocker(parent, "SouthWestRockGateBlocker_B", new Vector3(-18f, 0.8f, -25f), new Vector3(6f, 1.6f, 3.2f), 20f);
        }

        private static void CreateLandmarkBlocker(Transform parent, string name, Vector3 position, Vector3 size, float rotation)
        {
            var blocker = CreateChild(parent, name);
            blocker.layer = BlockerLayer;
            blocker.transform.localPosition = position;
            blocker.transform.localRotation = Quaternion.Euler(0f, rotation, 0f);
            var collider = blocker.AddComponent<BoxCollider>();
            collider.size = size;
        }

        private static void BuildLighting(Transform parent)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.50f, 0.62f, 0.56f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.29f, 0.37f, 0.28f, 1f);
            RenderSettings.ambientGroundColor = new Color(0.11f, 0.10f, 0.08f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.38f, 0.49f, 0.44f, 1f);
            RenderSettings.fogDensity = 0.0072f;

            var sunObject = new GameObject("ForestArena_SunKey");
            sunObject.transform.SetParent(parent, false);
            sunObject.transform.localRotation = Quaternion.Euler(49f, -42f, 0f);
            var sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.22f;
            sun.color = new Color(1f, 0.90f, 0.72f, 1f);
            sun.shadows = LightShadows.Soft;

            CreatePointLight(parent, "ForestArena_CenterSoftFill", new Vector3(0f, 8f, 0f), 28f, 0.26f, new Color(0.62f, 0.78f, 0.66f, 1f));
            CreatePointLight(parent, "ForestArena_BossShrineTorchGlow", new Vector3(22f, 4.8f, 22f), 15f, 0.62f, new Color(1f, 0.63f, 0.34f, 1f));
            CreatePointLight(parent, "ForestArena_NorthGateTorchGlow", new Vector3(-4f, 4.5f, 28f), 12f, 0.38f, new Color(1f, 0.66f, 0.38f, 1f));
            CreatePointLight(parent, "ForestArena_EastRuinCoolFill", new Vector3(27f, 6f, -2f), 13f, 0.2f, new Color(0.55f, 0.72f, 0.82f, 1f));
            CreatePointLight(parent, "ForestArena_CampWarmPocket", new Vector3(22f, 4.2f, -22f), 12f, 0.28f, new Color(1f, 0.54f, 0.28f, 1f));
        }

        private static void ConfigureSceneRuntime(ArenaConfig arenaConfig, RunConfig runConfig)
        {
            var runDirector = Object.FindAnyObjectByType<ArenaRunDirector>();
            if (runDirector != null)
            {
                var serializedRunDirector = new SerializedObject(runDirector);
                SetObject(serializedRunDirector, "runConfig", runConfig);
                SetObject(serializedRunDirector, "arenaConfigOverride", arenaConfig);
                serializedRunDirector.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(runDirector);
            }

            var spawnDirector = Object.FindAnyObjectByType<SurvivorSpawnDirector>();
            if (spawnDirector != null)
            {
                var serializedSpawnDirector = new SerializedObject(spawnDirector);
                SetObject(serializedSpawnDirector, "arenaConfig", arenaConfig);
                SetLayerMask(serializedSpawnDirector, "spawnGroundLayers", 1 << GroundLayer);
                SetBool(serializedSpawnDirector, "enableSpawnTelegraph", true);
                SetFloat(serializedSpawnDirector, "spawnTelegraphDuration", 0.55f);
                SetFloat(serializedSpawnDirector, "spawnTelegraphRadius", 0.9f);
                SetInt(serializedSpawnDirector, "maxConcurrentSpawnTelegraphs", 16);
                SetInt(serializedSpawnDirector, "baseLiveEnemyBudget", 32);
                SetInt(serializedSpawnDirector, "maxLiveEnemyBudget", 132);
                SetFloat(serializedSpawnDirector, "liveEnemyBudgetRampPerMinute", 10f);
                serializedSpawnDirector.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(spawnDirector);
            }
        }

        private static void ConfigureCamera()
        {
            var cameraRig = Object.FindAnyObjectByType<SurvivorCameraRig>();
            var player = GameObject.FindGameObjectWithTag("Player");
            var target = player != null ? player.transform : null;
            if (cameraRig == null)
            {
                var camera = UnityEngine.Camera.main ?? Object.FindAnyObjectByType<UnityEngine.Camera>();
                if (camera != null)
                {
                    cameraRig = camera.GetComponent<SurvivorCameraRig>() ?? camera.gameObject.AddComponent<SurvivorCameraRig>();
                }
            }

            if (cameraRig == null)
            {
                return;
            }

            cameraRig.SetTarget(target, false);
            cameraRig.SetComposition(CameraOffset, CameraLookAtOffset, false);
            cameraRig.ConfigureProjection(true, 15.5f, 38f, 0.1f, 240f);
            cameraRig.SnapToTarget();
            EditorUtility.SetDirty(cameraRig);
        }

        private static void CreatePointLight(Transform parent, string name, Vector3 position, float range, float intensity, Color color)
        {
            var lightObject = new GameObject(name);
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.localPosition = position;
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = range;
            light.intensity = intensity;
            light.color = color;
            light.shadows = LightShadows.None;
        }

        private static GameObject PlaceDungeonProp(Transform parent, string assetName, string objectName, Vector3 localPosition, float yRotation, Vector3 localScale)
        {
            var prop = InstantiateDungeonModel(assetName, objectName, parent, localPosition, yRotation, localScale);
            RemoveVisualColliders(prop);
            return prop;
        }

        private static GameObject InstantiateDungeonModel(string assetName, string objectName, Transform parent, Vector3 localPosition, float yRotation, Vector3 localScale)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>($"{DungeonModelFolder}/{assetName}.fbx");
            GameObject instance;
            if (model != null)
            {
                instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
                if (instance == null)
                {
                    instance = Object.Instantiate(model);
                }
            }
            else
            {
                instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
                if (instance.TryGetComponent<Renderer>(out var fallbackRenderer))
                {
                    fallbackRenderer.sharedMaterial = GetOrCreateMaterial("MAT_ForestArena_RuinFallback", new Color(0.36f, 0.36f, 0.32f, 1f));
                }
            }

            instance.name = objectName;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
            instance.transform.localScale = localScale;
            return instance;
        }

        private static GameObject InstantiateForestModel(string assetName, string objectName, Transform parent, Vector3 localPosition, float yRotation, Vector3 localScale)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>($"{ForestModelFolder}/{assetName}.fbx");
            GameObject instance;
            if (model != null)
            {
                instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
                if (instance == null)
                {
                    instance = Object.Instantiate(model);
                }
            }
            else
            {
                instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            }

            instance.name = objectName;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
            instance.transform.localScale = localScale;
            return instance;
        }

        private static Vector3 RotateOffset(float yRotation, float x, float y, float z)
        {
            return Quaternion.Euler(0f, yRotation, 0f) * new Vector3(x, y, z);
        }

        private static void RemoveVisualColliders(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            var colliders = instance.GetComponentsInChildren<Collider>(true);
            for (var i = colliders.Length - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(colliders[i]);
            }
        }

        private static Vector3 Polar(float radius, float degrees)
        {
            var radians = degrees * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(radians) * radius, 0f, Mathf.Cos(radians) * radius);
        }

        private static GameObject CreateChild(Transform parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            return child;
        }

        private static Material GetOrCreateMaterial(string materialName, Color color)
        {
            var path = $"{MaterialFolder}/{materialName}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                ApplyMaterialColor(material, color);
                return material;
            }

            material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"))
            {
                name = materialName,
                color = color
            };
            ApplyMaterialColor(material, color);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void ApplyMaterialColor(Material material, Color color)
        {
            if (material == null)
            {
                return;
            }

            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.22f);
            }

            EditorUtility.SetDirty(material);
        }

        private static void MarkEnvironmentStatic(GameObject root)
        {
            var transforms = root.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < transforms.Length; i++)
            {
                GameObjectUtility.SetStaticEditorFlags(
                    transforms[i].gameObject,
                    StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
            }
        }

        private static void RequireChild(List<string> issues, Transform root, string name)
        {
            if (root.Find(name) == null)
            {
                issues.Add($"{name} is missing under {root.name}.");
            }
        }

        private static void SetString(SerializedObject serializedObject, string propertyName, string value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null && property.propertyType == SerializedPropertyType.String)
            {
                property.stringValue = value;
            }
        }

        private static void SetObject(SerializedObject serializedObject, string propertyName, Object value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null && property.propertyType == SerializedPropertyType.ObjectReference)
            {
                property.objectReferenceValue = value;
            }
        }

        private static void SetVector3(SerializedObject serializedObject, string propertyName, Vector3 value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null && property.propertyType == SerializedPropertyType.Vector3)
            {
                property.vector3Value = value;
            }
        }

        private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null && property.propertyType == SerializedPropertyType.Float)
            {
                property.floatValue = value;
            }
        }

        private static void SetInt(SerializedObject serializedObject, string propertyName, int value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null && property.propertyType == SerializedPropertyType.Integer)
            {
                property.intValue = value;
            }
        }

        private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null && property.propertyType == SerializedPropertyType.Boolean)
            {
                property.boolValue = value;
            }
        }

        private static void SetEnum(SerializedObject serializedObject, string propertyName, int value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null && property.propertyType == SerializedPropertyType.Enum)
            {
                property.enumValueIndex = Mathf.Clamp(value, 0, property.enumDisplayNames.Length - 1);
            }
        }

        private static void SetLayerMask(SerializedObject serializedObject, string propertyName, int value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null && property.propertyType == SerializedPropertyType.LayerMask)
            {
                property.intValue = value;
            }
        }

        private static void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parts = folderPath.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
#endif
