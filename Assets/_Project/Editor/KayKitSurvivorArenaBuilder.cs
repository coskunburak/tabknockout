using TapKnockout.Camera;
using TapKnockout.Input;
using TapKnockout.Player;
using TapKnockout.Survivor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TapKnockout.Editor
{
    public static class KayKitSurvivorArenaBuilder
    {
        private const string MenuPath = "Tap Knockout/Survivor/Build Premium KayKit Dungeon Arena";
        private const string KayKitModelFolder = "Assets/Assets/game asset packs/KayKit_DungeonRemastered_1.1_FREE/Assets/fbx(unity)";
        private const string ArenaConfigPath = "Assets/_Project/ScriptableObjects/Arenas/ArenaConfig_DesktopSurvivorPrototype.asset";
        private const string GeneratedRootName = "KayKitSurvivorArena_Generated";
        private const string MaterialFolder = "Assets/_Project/Art/Materials";

        private const float TileSpacing = 2f;
        private const int TileHalfExtent = 13;
        private const float WallDistance = 27f;
        private const float ArenaRadius = 23f;

        [MenuItem(MenuPath)]
        public static void BuildPremiumKayKitDungeonArena()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "KayKit Survivor Arena",
                    "Stop Play Mode before rebuilding the arena.",
                    "OK");
                return;
            }

            EnsureFolder(MaterialFolder);

            if (!AssetDatabase.IsValidFolder(KayKitModelFolder))
            {
                Debug.LogWarning(
                    $"{nameof(KayKitSurvivorArenaBuilder)} could not find KayKit model folder at {KayKitModelFolder}. Fallback primitives will be used where models are missing.",
                    null);
            }

            var arenaRoot = ResolveOrCreateArenaRoot();
            ReplaceGeneratedArena(arenaRoot.transform);
            DisablePrototypeGround(arenaRoot.transform);
            UpdateArenaConfig();
            ApplyCameraPreset();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log(
                $"{nameof(KayKitSurvivorArenaBuilder)} rebuilt {GeneratedRootName}. It includes a large KayKit dungeon floor, perimeter walls, edge props, collision bounds, lighting, updated survivor arena config values, and the 2.5D orthographic camera preset.",
                arenaRoot);
        }

        private static GameObject ResolveOrCreateArenaRoot()
        {
            var arenaRoot = GameObject.Find("ArenaRoot");
            if (arenaRoot != null)
            {
                return arenaRoot;
            }

            var prototypeRoot = GameObject.Find("DesktopSurvivorPrototypeRoot");
            arenaRoot = new GameObject("ArenaRoot");
            Undo.RegisterCreatedObjectUndo(arenaRoot, "Create ArenaRoot");
            if (prototypeRoot != null)
            {
                arenaRoot.transform.SetParent(prototypeRoot.transform, false);
            }

            return arenaRoot;
        }

        private static void ReplaceGeneratedArena(Transform arenaRoot)
        {
            var existing = arenaRoot.Find(GeneratedRootName);
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing.gameObject);
            }

            var root = CreateChild(arenaRoot, GeneratedRootName);
            Undo.RegisterCreatedObjectUndo(root.gameObject, "Create KayKit Survivor Arena");

            var floorRoot = CreateChild(root, "FloorRoot");
            var wallRoot = CreateChild(root, "WallRoot");
            var propRoot = CreateChild(root, "PropRoot");
            var lightRoot = CreateChild(root, "LightRoot");
            var collisionRoot = CreateChild(root, "CollisionRoot");

            BuildFloor(floorRoot);
            BuildWalls(wallRoot);
            BuildProps(propRoot);
            BuildLighting(lightRoot);
            BuildCollision(collisionRoot);
            MarkEnvironmentStatic(root.gameObject);

            Selection.activeGameObject = root.gameObject;
            EditorGUIUtility.PingObject(root.gameObject);
        }

        private static void BuildFloor(Transform parent)
        {
            var floorMaterial = GetOrCreateMaterial("MAT_KayKitSurvivor_FallbackFloor", new Color(0.33f, 0.35f, 0.36f, 1f));

            for (var x = -TileHalfExtent; x <= TileHalfExtent; x++)
            {
                for (var z = -TileHalfExtent; z <= TileHalfExtent; z++)
                {
                    var assetName = ResolveFloorAssetName(x, z);
                    var tile = InstantiateModelOrCube(
                        LoadModel(assetName),
                        $"Floor_{x + TileHalfExtent:00}_{z + TileHalfExtent:00}",
                        parent,
                        new Vector3(x * TileSpacing, 0f, z * TileSpacing),
                        new Vector3(TileSpacing, 0.18f, TileSpacing),
                        floorMaterial);
                    tile.transform.localRotation = Quaternion.Euler(0f, ResolveFloorRotation(x, z), 0f);
                }
            }
        }

        private static string ResolveFloorAssetName(int x, int z)
        {
            if ((x == -4 || x == 4) && (z == -4 || z == 4))
            {
                return "floor_tile_big_grate";
            }

            if ((x == 0 && Mathf.Abs(z) == 7) || (z == 0 && Mathf.Abs(x) == 7))
            {
                return "floor_tile_grate";
            }

            if (Mathf.Abs(x) > 9 || Mathf.Abs(z) > 9)
            {
                return ((x + z) & 1) == 0 ? "floor_dirt_large" : "floor_dirt_large_rocky";
            }

            if ((Mathf.Abs(x * 3 + z * 5) % 17) == 0)
            {
                return "floor_tile_large_rocks";
            }

            if ((Mathf.Abs(x * 7 - z * 2) % 23) == 0)
            {
                return ((x + z) & 1) == 0 ? "floor_tile_small_broken_A" : "floor_tile_small_broken_B";
            }

            return "floor_tile_large";
        }

        private static float ResolveFloorRotation(int x, int z)
        {
            return ((Mathf.Abs(x * 31 + z * 17) % 4) * 90f);
        }

        private static void BuildWalls(Transform parent)
        {
            var wallMaterial = GetOrCreateMaterial("MAT_KayKitSurvivor_FallbackWall", new Color(0.31f, 0.35f, 0.39f, 1f));
            var corner = LoadModel("wall_corner") ?? LoadModel("wall");

            for (var i = -TileHalfExtent; i <= TileHalfExtent; i++)
            {
                var x = i * TileSpacing;
                InstantiateWallSegment(parent, ResolveNorthSouthWallAsset(i), $"NorthWall_{i + TileHalfExtent:00}", new Vector3(x, 0f, WallDistance), Quaternion.identity, wallMaterial);
                InstantiateWallSegment(parent, ResolveNorthSouthWallAsset(-i), $"SouthWall_{i + TileHalfExtent:00}", new Vector3(x, 0f, -WallDistance), Quaternion.Euler(0f, 180f, 0f), wallMaterial);
            }

            for (var i = -TileHalfExtent + 1; i <= TileHalfExtent - 1; i++)
            {
                var z = i * TileSpacing;
                InstantiateWallSegment(parent, ResolveEastWestWallAsset(i), $"EastWall_{i + TileHalfExtent:00}", new Vector3(WallDistance, 0f, z), Quaternion.Euler(0f, 90f, 0f), wallMaterial);
                InstantiateWallSegment(parent, ResolveEastWestWallAsset(-i), $"WestWall_{i + TileHalfExtent:00}", new Vector3(-WallDistance, 0f, z), Quaternion.Euler(0f, -90f, 0f), wallMaterial);
            }

            InstantiateModelOrCube(corner, "Corner_NW", parent, new Vector3(-WallDistance, 0f, WallDistance), Vector3.one, wallMaterial).transform.localRotation = Quaternion.identity;
            InstantiateModelOrCube(corner, "Corner_NE", parent, new Vector3(WallDistance, 0f, WallDistance), Vector3.one, wallMaterial).transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            InstantiateModelOrCube(corner, "Corner_SE", parent, new Vector3(WallDistance, 0f, -WallDistance), Vector3.one, wallMaterial).transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            InstantiateModelOrCube(corner, "Corner_SW", parent, new Vector3(-WallDistance, 0f, -WallDistance), Vector3.one, wallMaterial).transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
        }

        private static string ResolveNorthSouthWallAsset(int index)
        {
            if (index == 0)
            {
                return "wall_archedwindow_gated";
            }

            if (Mathf.Abs(index) == 5)
            {
                return "wall_arched";
            }

            if (index % 7 == 0)
            {
                return "wall_cracked";
            }

            if (index % 4 == 0)
            {
                return "wall_pillar";
            }

            return "wall";
        }

        private static string ResolveEastWestWallAsset(int index)
        {
            if (Mathf.Abs(index) == 6)
            {
                return "wall_window_closed";
            }

            if (index % 5 == 0)
            {
                return "wall_archedwindow_open";
            }

            if (index % 4 == 0)
            {
                return "wall_pillar";
            }

            return "wall";
        }

        private static void InstantiateWallSegment(
            Transform parent,
            string assetName,
            string objectName,
            Vector3 localPosition,
            Quaternion localRotation,
            Material fallbackMaterial)
        {
            var wall = InstantiateModelOrCube(
                LoadModel(assetName) ?? LoadModel("wall"),
                objectName,
                parent,
                localPosition,
                new Vector3(TileSpacing, 2.8f, 0.42f),
                fallbackMaterial);
            wall.transform.localRotation = localRotation;
        }

        private static void BuildProps(Transform parent)
        {
            var propMaterial = GetOrCreateMaterial("MAT_KayKitSurvivor_FallbackProp", new Color(0.48f, 0.37f, 0.28f, 1f));

            PlaceProp(parent, "pillar_decorated", "Pillar_NW", new Vector3(-14f, 0f, 14f), 0f, propMaterial);
            PlaceProp(parent, "pillar_decorated", "Pillar_NE", new Vector3(14f, 0f, 14f), 0f, propMaterial);
            PlaceProp(parent, "pillar_decorated", "Pillar_SE", new Vector3(14f, 0f, -14f), 0f, propMaterial);
            PlaceProp(parent, "pillar_decorated", "Pillar_SW", new Vector3(-14f, 0f, -14f), 0f, propMaterial);

            PlaceProp(parent, "torch_lit", "Torch_NorthWest", new Vector3(-18f, 1.15f, WallDistance - 1.25f), 180f, propMaterial);
            PlaceProp(parent, "torch_lit", "Torch_NorthEast", new Vector3(18f, 1.15f, WallDistance - 1.25f), 180f, propMaterial);
            PlaceProp(parent, "torch_lit", "Torch_SouthWest", new Vector3(-18f, 1.15f, -WallDistance + 1.25f), 0f, propMaterial);
            PlaceProp(parent, "torch_lit", "Torch_SouthEast", new Vector3(18f, 1.15f, -WallDistance + 1.25f), 0f, propMaterial);

            PlaceProp(parent, "banner_patternB_red", "Banner_NorthLeft", new Vector3(-8f, 1.2f, WallDistance - 0.75f), 180f, propMaterial);
            PlaceProp(parent, "banner_patternC_blue", "Banner_NorthRight", new Vector3(8f, 1.2f, WallDistance - 0.75f), 180f, propMaterial);
            PlaceProp(parent, "banner_triple_green", "Banner_West", new Vector3(-WallDistance + 0.75f, 1.2f, 4f), 90f, propMaterial);
            PlaceProp(parent, "banner_triple_brown", "Banner_East", new Vector3(WallDistance - 0.75f, 1.2f, -4f), -90f, propMaterial);

            PlaceProp(parent, "barrel_large_decorated", "Barrels_NW", new Vector3(-21f, 0f, 18f), 35f, propMaterial);
            PlaceProp(parent, "barrel_small_stack", "Barrels_NE", new Vector3(21f, 0f, 18f), -20f, propMaterial);
            PlaceProp(parent, "crates_stacked", "Crates_SW", new Vector3(-21f, 0f, -18f), 15f, propMaterial);
            PlaceProp(parent, "box_stacked", "Boxes_SE", new Vector3(21f, 0f, -18f), -35f, propMaterial);

            PlaceProp(parent, "chest", "Chest_NorthAlcove", new Vector3(0f, 0f, 22f), 180f, propMaterial);
            PlaceProp(parent, "sword_shield_gold", "HeroRelic_West", new Vector3(-22.5f, 0f, -8f), 90f, propMaterial);
            PlaceProp(parent, "sword_shield", "HeroRelic_East", new Vector3(22.5f, 0f, 8f), -90f, propMaterial);

            PlaceProp(parent, "rubble_half", "Rubble_NorthInner", new Vector3(-7f, 0f, 18f), 35f, propMaterial);
            PlaceProp(parent, "rubble_large", "Rubble_SouthInner", new Vector3(7f, 0f, -18f), -50f, propMaterial);
            PlaceProp(parent, "floor_tile_big_spikes", "TrapVisual_WestEdge", new Vector3(-18f, 0f, 0f), 90f, propMaterial);
            PlaceProp(parent, "floor_tile_big_spikes", "TrapVisual_EastEdge", new Vector3(18f, 0f, 0f), -90f, propMaterial);
        }

        private static void PlaceProp(
            Transform parent,
            string assetName,
            string objectName,
            Vector3 localPosition,
            float yRotation,
            Material fallbackMaterial)
        {
            var prop = InstantiateModelOrCube(
                LoadModel(assetName),
                objectName,
                parent,
                localPosition,
                Vector3.one,
                fallbackMaterial);
            prop.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
        }

        private static void BuildLighting(Transform parent)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.61f, 0.67f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.34f, 0.35f, 0.36f, 1f);
            RenderSettings.ambientGroundColor = new Color(0.15f, 0.13f, 0.11f, 1f);

            CreatePointLight(parent, "TorchLight_NW", new Vector3(-18f, 2.3f, WallDistance - 2.2f), 8f, 1.2f);
            CreatePointLight(parent, "TorchLight_NE", new Vector3(18f, 2.3f, WallDistance - 2.2f), 8f, 1.2f);
            CreatePointLight(parent, "TorchLight_SW", new Vector3(-18f, 2.3f, -WallDistance + 2.2f), 8f, 1.2f);
            CreatePointLight(parent, "TorchLight_SE", new Vector3(18f, 2.3f, -WallDistance + 2.2f), 8f, 1.2f);
            CreatePointLight(parent, "CenterSoftFill", new Vector3(0f, 7f, 0f), 28f, 0.45f, new Color(0.62f, 0.72f, 0.84f, 1f));
        }

        private static void CreatePointLight(
            Transform parent,
            string objectName,
            Vector3 localPosition,
            float range,
            float intensity,
            Color? color = null)
        {
            var lightObject = CreateChild(parent, objectName);
            lightObject.localPosition = localPosition;
            var light = lightObject.gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = range;
            light.intensity = intensity;
            light.color = color ?? new Color(1f, 0.65f, 0.34f, 1f);
            light.shadows = LightShadows.Soft;
        }

        private static void BuildCollision(Transform parent)
        {
            CreateBoxCollider(parent, "Collision_Floor", new Vector3(0f, -0.08f, 0f), new Vector3(58f, 0.16f, 58f));
            CreateBoxCollider(parent, "Collision_North", new Vector3(0f, 1.2f, WallDistance + 0.55f), new Vector3(58f, 2.4f, 1.2f));
            CreateBoxCollider(parent, "Collision_South", new Vector3(0f, 1.2f, -WallDistance - 0.55f), new Vector3(58f, 2.4f, 1.2f));
            CreateBoxCollider(parent, "Collision_East", new Vector3(WallDistance + 0.55f, 1.2f, 0f), new Vector3(1.2f, 2.4f, 58f));
            CreateBoxCollider(parent, "Collision_West", new Vector3(-WallDistance - 0.55f, 1.2f, 0f), new Vector3(1.2f, 2.4f, 58f));
        }

        private static void CreateBoxCollider(Transform parent, string objectName, Vector3 localPosition, Vector3 size)
        {
            var colliderTransform = CreateChild(parent, objectName);
            colliderTransform.localPosition = localPosition;
            var boxCollider = colliderTransform.gameObject.AddComponent<BoxCollider>();
            boxCollider.size = size;
        }

        private static void DisablePrototypeGround(Transform arenaRoot)
        {
            var prototypeGround = arenaRoot.Find("PrototypeGround");
            if (prototypeGround == null)
            {
                return;
            }

            Undo.RecordObject(prototypeGround.gameObject, "Disable PrototypeGround");
            prototypeGround.gameObject.SetActive(false);
        }

        private static void UpdateArenaConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<ArenaConfig>(ArenaConfigPath);
            if (config == null)
            {
                Debug.LogWarning($"{nameof(KayKitSurvivorArenaBuilder)} could not update arena config because {ArenaConfigPath} was not found.", null);
                return;
            }

            var serializedObject = new SerializedObject(config);
            serializedObject.FindProperty("arenaCenter").vector3Value = Vector3.zero;
            serializedObject.FindProperty("arenaRadius").floatValue = ArenaRadius;
            serializedObject.FindProperty("playerSafeSpawnRadius").floatValue = 4.5f;
            serializedObject.FindProperty("enemySpawnMinRadiusFromPlayer").floatValue = 8f;
            serializedObject.FindProperty("enemySpawnMaxRadiusFromPlayer").floatValue = 19f;
            serializedObject.FindProperty("maxLiveEnemies").intValue = 100;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }

        private static void ApplyCameraPreset()
        {
            var cameraRig = Object.FindFirstObjectByType<SurvivorCameraRig>();
            var mainCamera = UnityEngine.Camera.main ?? Object.FindFirstObjectByType<UnityEngine.Camera>();
            if (cameraRig == null && mainCamera != null)
            {
                cameraRig = mainCamera.GetComponent<SurvivorCameraRig>() ?? mainCamera.gameObject.AddComponent<SurvivorCameraRig>();
            }

            if (cameraRig == null)
            {
                Debug.LogWarning($"{nameof(KayKitSurvivorArenaBuilder)} could not apply the camera preset because no camera was found.", null);
                return;
            }

            var player = ResolvePlayerTransform();
            cameraRig.ApplySurvivor2_5DPreset(player, true);
            EditorUtility.SetDirty(cameraRig);

            var cameraComponent = cameraRig.GetComponent<UnityEngine.Camera>();
            if (cameraComponent != null)
            {
                EditorUtility.SetDirty(cameraComponent);
            }

            ApplyStableDesktopFacingPreset(player, cameraComponent);
        }

        private static Transform ResolvePlayerTransform()
        {
            var playerTagged = GameObject.FindGameObjectWithTag("Player");
            if (playerTagged != null)
            {
                return playerTagged.transform;
            }

            var playerMovement = Object.FindFirstObjectByType<PlayerMovementController>();
            return playerMovement != null ? playerMovement.transform : null;
        }

        private static void ApplyStableDesktopFacingPreset(Transform player, UnityEngine.Camera gameplayCamera)
        {
            if (player == null)
            {
                return;
            }

            var movement = player.GetComponent<PlayerMovementController>();
            if (movement != null)
            {
                movement.SetRotateTowardMovement(false);
                SetBool(movement, "rotateTowardMovement", false);
            }

            var attack = player.GetComponent<PlayerAttackController>();
            var mouseAim = player.GetComponent<MouseAimController>();
            var reticle = player.GetComponent<MouseAimReticleController>();
            if (reticle == null)
            {
                reticle = player.gameObject.AddComponent<MouseAimReticleController>();
            }

            if (attack != null)
            {
                var desktopInput = player.GetComponent<DesktopInputReader>();
                reticle.SetAimController(mouseAim);
                reticle.SetInputReader(desktopInput);
                SetObjectReference(reticle, "aimController", mouseAim);
                SetObjectReference(reticle, "inputReader", desktopInput);
                SetBool(reticle, "reticleEnabled", true);
                SetBool(reticle, "allowRuntimeFallback", true);
                SetBool(reticle, "hideSystemCursorDuringGameplay", true);
                SetFloat(reticle, "yOffset", 0.18f);
                SetObjectReference(attack, "movementController", movement);
                SetObjectReference(attack, "desktopInputReader", desktopInput);
                SetObjectReference(attack, "targetProvider", player.GetComponent<PlayerTargetProvider>());
                SetObjectReference(attack, "runtimeStats", player.GetComponent<PlayerRuntimeStats>());
                SetObjectReference(attack, "playerConfig", movement != null ? movement.Config : null);
                SetObjectReference(attack, "playerHealth", player.GetComponent<PlayerHealth>());
                SetObjectReference(attack, "mouseAimController", mouseAim);
                SetObjectReference(attack, "aimReticle", reticle);
                SetBool(attack, "faceTargetOnAttack", false);
                SetBool(attack, "requireStationaryToAttack", false);
                SetBool(attack, "preferMouseAimForProjectiles", true);
                SetBool(attack, "allowAimFallbackWithoutTarget", true);
                SetBool(attack, "useProjectilePooling", true);
                SetBool(attack, "fallbackAttackWhileMoving", true);
                SetBool(attack, "fallbackManualFireRequiresInput", true);
                SetEnum(attack, "firePolicy", (int)PrimaryAttackFirePolicy.HoldMouseAim);
                SetEnum(attack, "facingPolicy", (int)PlayerFacingPolicy.MouseAimDirection);
            }

            if (mouseAim == null)
            {
                return;
            }

            if (gameplayCamera != null)
            {
                mouseAim.SetAimCamera(gameplayCamera);
                SetObjectReference(mouseAim, "aimCamera", gameplayCamera);
            }

            mouseAim.SetFacingTarget(player);
            SetObjectReference(mouseAim, "facingTarget", player);
            SetBool(mouseAim, "preferStableGroundPlane", true);
            SetBool(mouseAim, "usePhysicsRaycast", false);
            SetBool(mouseAim, "rotateFacingTarget", true);
            SetBool(mouseAim, "rotateRigidbodyInFixedUpdate", true);
            SetFloat(mouseAim, "fallbackGroundPlaneY", 0f);
            SetFloat(mouseAim, "minAimDirectionDistance", 0.35f);

            if (attack != null)
            {
                SetObjectReference(attack, "mouseAimController", mouseAim);
            }
        }

        private static Transform CreateChild(Transform parent, string objectName)
        {
            var child = new GameObject(objectName);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            return child.transform;
        }

        private static GameObject InstantiateModelOrCube(
            GameObject model,
            string objectName,
            Transform parent,
            Vector3 localPosition,
            Vector3 fallbackScale,
            Material fallbackMaterial)
        {
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
                instance.transform.localScale = fallbackScale;
                if (fallbackMaterial != null && instance.TryGetComponent<Renderer>(out var renderer))
                {
                    renderer.sharedMaterial = fallbackMaterial;
                }
            }

            instance.name = objectName;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = localPosition;
            RemoveVisualColliders(instance);
            return instance;
        }

        private static void RemoveVisualColliders(GameObject instance)
        {
            var colliders = instance.GetComponentsInChildren<Collider>(true);
            for (var i = colliders.Length - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(colliders[i]);
            }
        }

        private static GameObject LoadModel(string assetName)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>($"{KayKitModelFolder}/{assetName}.fbx");
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

        private static void SetObjectReference(Object target, string propertyName, Object value)
        {
            if (target == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            if (property == null || property.propertyType != SerializedPropertyType.ObjectReference)
            {
                return;
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetBool(Object target, string propertyName, bool value)
        {
            if (target == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            if (property == null || property.propertyType != SerializedPropertyType.Boolean)
            {
                return;
            }

            property.boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetFloat(Object target, string propertyName, float value)
        {
            if (target == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            if (property == null || property.propertyType != SerializedPropertyType.Float)
            {
                return;
            }

            property.floatValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetEnum(Object target, string propertyName, int value)
        {
            if (target == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            if (property == null || property.propertyType != SerializedPropertyType.Enum)
            {
                return;
            }

            property.enumValueIndex = Mathf.Clamp(value, 0, property.enumDisplayNames.Length - 1);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
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
