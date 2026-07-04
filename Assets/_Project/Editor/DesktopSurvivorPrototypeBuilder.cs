#if UNITY_EDITOR
using TapKnockout.Ability;
using TapKnockout.Camera;
using TapKnockout.Characters;
using TapKnockout.Editor.Tools;
using TapKnockout.Input;
using TapKnockout.Pickups;
using TapKnockout.Player;
using TapKnockout.Projectile;
using TapKnockout.Survivor;
using TapKnockout.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace TapKnockout.Editor
{
    public static class DesktopSurvivorPrototypeBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/DesktopSurvivorPrototype.unity";
        private const string ForestScenePath = "Assets/_Project/Scenes/DesktopSurvivorPrototype_ForestArena.unity";
        private const string RunConfigPath = "Assets/_Project/ScriptableObjects/Runs/RunConfig_DesktopSurvivorPrototype.asset";
        private const string ArenaConfigPath = "Assets/_Project/ScriptableObjects/Arenas/ArenaConfig_DesktopSurvivorPrototype.asset";
        private const string WaveTimelinePath = "Assets/_Project/ScriptableObjects/Waves/WaveTimeline_DesktopSurvivorPrototype.asset";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player/Player.prefab";
        private const string ArcBlastAbilityId = "skill_arc_blast";
        private const string GroundSlamAbilityId = "skill_ground_slam";
        private static readonly Vector3 SurvivorCameraOffset = new Vector3(0f, 32f, -6f);
        private static readonly Vector3 SurvivorCameraLookAtOffset = new Vector3(0f, 0.35f, 0f);
        private const float SurvivorCameraOrthographicSize = 14f;
        private const float SurvivorCameraFieldOfView = 38f;
        private const float SurvivorCameraNearClip = 0.1f;
        private const float SurvivorCameraFarClip = 220f;
        private const float SurvivorAimMinDirectionDistance = 0.35f;
        private static readonly string[] ActiveSkillHotkeys = { "Q", "E", "R", "F" };

        [MenuItem("Tap Knockout/Survivor/Create Desktop Survivor Prototype Scene")]
        public static void CreatePrototypeScene()
        {
            EnsureAssetFolders();

            var runConfig = LoadOrCreateAsset<RunConfig>(RunConfigPath);
            var arenaConfig = LoadOrCreateAsset<ArenaConfig>(ArenaConfigPath);
            EnsureAbilityCatalogAssets();
            CuteMonsterEnemyContentBuilder.BuildCuteMonsterEnemyContent(wirePrototypeRun: true, logToConsole: false);
            var waveTimeline = ResolvePrototypeWaveTimeline();
            WireRunConfig(runConfig, arenaConfig, waveTimeline);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new GameObject("DesktopSurvivorPrototypeRoot");
            var managers = CreateChild(root.transform, "Managers");
            var arenaRoot = CreateChild(root.transform, "ArenaRoot");
            var playerSpawn = CreateChild(root.transform, "PlayerSpawn");
            var cameraRigObject = CreateChild(root.transform, "CameraRig");
            var canvasObject = CreateChild(root.transform, "GameplayCanvas");
            CreateChild(root.transform, "LightingRoot");
            var debugRoot = CreateChild(root.transform, "DebugRoot");
            var feedbackPlayer = debugRoot.AddComponent<SurvivorFeedbackPlayer>();
            var vfxPoolRoot = CreateChild(debugRoot.transform, "VFXPoolRoot");

            playerSpawn.transform.position = Vector3.zero;
            CreateArenaPlaceholder(arenaRoot.transform);

            var player = CreatePlayerInstance(playerSpawn.transform.position);
            var playerTransform = player != null ? player.transform : playerSpawn.transform;
            var playerHealth = player != null ? EnsureComponent<PlayerHealth>(player) : null;
            var playerXP = EnsureComponent<PlayerXPController>(playerTransform.gameObject);
            var pickupCollector = EnsureComponent<PickupCollector>(playerTransform.gameObject);
            var desktopInput = EnsureComponent<DesktopInputReader>(playerTransform.gameObject);
            var mouseAim = EnsureComponent<MouseAimController>(playerTransform.gameObject);
            var aimReticle = EnsureComponent<MouseAimReticleController>(playerTransform.gameObject);
            var inputBridge = EnsureComponent<DesktopSurvivorInputBridge>(playerTransform.gameObject);
            var activeSkills = EnsureComponent<ActiveSkillController>(playerTransform.gameObject);
            var playerDash = player != null ? player.GetComponent<PlayerDashController>() : null;
            var playerMovement = player != null ? player.GetComponent<PlayerMovementController>() : null;
            var playerStats = player != null ? EnsureComponent<PlayerRuntimeStats>(player) : null;
            var abilityCombatEffects = EnsureComponent<PlayerAbilityCombatEffectController>(playerTransform.gameObject);
            abilityCombatEffects.SetRuntimeStats(playerStats);
            abilityCombatEffects.SetPlayerHealth(playerHealth);

            if (playerMovement != null)
            {
                playerMovement.SetInputSource(desktopInput);
                playerMovement.SetRotateTowardMovement(false);
                SetObjectReference(playerMovement, "inputSourceBehaviour", desktopInput);
                SetBool(playerMovement, "rotateTowardMovement", false);
            }

            var camera = cameraRigObject.AddComponent<UnityEngine.Camera>();
            var audioListener = cameraRigObject.AddComponent<AudioListener>();
            _ = audioListener;
            var cameraRig = cameraRigObject.AddComponent<SurvivorCameraRig>();
            var cameraShakeReceiver = cameraRigObject.AddComponent<CameraShakeReceiver>();
            ApplyDesktopSurvivorCameraPreset(camera, cameraRig, playerTransform);
            mouseAim.SetAimCamera(camera);
            ApplyStableDesktopFacingPreset(playerTransform.gameObject, playerMovement, mouseAim, camera);
            ConfigureAimReticle(aimReticle, mouseAim, desktopInput, playerMovement != null ? playerMovement.Config : null);
            SetObjectReference(feedbackPlayer, "vfxPoolRoot", vfxPoolRoot.transform);
            SetObjectReference(feedbackPlayer, "cameraShakeReceiver", cameraShakeReceiver);

            var spawnDirectorObject = CreateChild(managers.transform, "SurvivorSpawnDirector");
            var spawnDirector = spawnDirectorObject.AddComponent<SurvivorSpawnDirector>();
            var enemyPool = spawnDirectorObject.AddComponent<EnemyPoolService>();
            var projectilePool = CreateChild(managers.transform, "ProjectilePoolService").AddComponent<ProjectilePoolService>();
            var stressTest = debugRoot.AddComponent<SurvivorStressTestController>();
            spawnDirector.Configure(arenaConfig, waveTimeline, playerTransform);
            SetObjectReference(spawnDirector, "arenaConfig", arenaConfig);
            SetObjectReference(spawnDirector, "waveTimeline", waveTimeline);
            SetObjectReference(spawnDirector, "playerTarget", playerTransform);
            SetObjectReference(spawnDirector, "enemyPoolService", enemyPool);

            var runDirectorObject = CreateChild(managers.transform, "ArenaRunDirector");
            var runDirector = runDirectorObject.AddComponent<ArenaRunDirector>();
            var bossDirector = runDirectorObject.AddComponent<ArenaBossDirector>();
            bossDirector.Configure(spawnDirector);
            SetObjectReference(bossDirector, "feedbackPlayer", feedbackPlayer);
            SetObjectReference(runDirector, "runConfig", runConfig);
            SetObjectReference(runDirector, "arenaConfigOverride", arenaConfig);
            SetObjectReference(runDirector, "waveTimelineOverride", waveTimeline);
            SetBool(runDirector, "autoStartOnStart", true);
            SetObjectReference(runDirector, "playerTransform", playerTransform);
            SetObjectReference(runDirector, "playerHealth", playerHealth);
            SetObjectReference(runDirector, "xpController", playerXP);
            SetObjectReference(runDirector, "pickupCollector", pickupCollector);
            SetObjectReference(runDirector, "spawnDirector", spawnDirector);
            SetObjectReference(runDirector, "bossDirector", bossDirector);
            SetObjectReference(inputBridge, "runDirector", runDirector);
            inputBridge.Configure(desktopInput, playerMovement, playerDash, runDirector);
            inputBridge.SetActiveSkillController(activeSkills);

            var abilitySelection = runDirectorObject.AddComponent<AbilitySelectionController>();
            SetObjectReference(runDirector, "abilitySelectionController", abilitySelection);
            var abilityApplier = player != null ? EnsureComponent<PlayerAbilityEffectApplier>(player) : null;
            if (abilityApplier != null)
            {
                abilityApplier.SetRuntimeStats(playerStats);
                abilityApplier.SetPlayerHealth(playerHealth);
                abilitySelection.SetAbilityEffectApplier(abilityApplier);
            }

            activeSkills.Configure(inputBridge, abilitySelection, playerMovement, playerStats, mouseAim, playerTransform, feedbackPlayer);
            SetObjectReference(activeSkills, "inputBridge", inputBridge);
            SetObjectReference(activeSkills, "abilitySelectionController", abilitySelection);
            SetObjectReference(activeSkills, "movementController", playerMovement);
            SetObjectReference(activeSkills, "runtimeStats", playerStats);
            SetObjectReference(activeSkills, "mouseAimController", mouseAim);
            SetObjectReference(activeSkills, "playerConfig", playerMovement != null ? playerMovement.Config : null);
            SetObjectReference(activeSkills, "playerHealth", playerHealth);
            SetObjectReference(activeSkills, "castOrigin", playerTransform);
            SetObjectReference(activeSkills, "feedbackPlayer", feedbackPlayer);
            ApplyDefaultActiveSkillSlotPolicies(activeSkills);
            SetObjectReference(inputBridge, "activeSkillController", activeSkills);

            SetObjectReference(stressTest, "spawnDirector", spawnDirector);
            SetObjectReference(stressTest, "enemyPoolService", enemyPool);
            SetObjectReference(stressTest, "projectilePoolService", projectilePool);
            SetObjectReference(stressTest, "runDirector", runDirector);
            SetObjectReference(stressTest, "bossDirector", bossDirector);

            BuildPrototypeCanvas(canvasObject, runDirector, spawnDirector, playerXP, playerHealth, playerDash, activeSkills, abilitySelection);
            VFXFeedbackSetupBuilder.CreateFeedbackSystemRoot();
            VFXFeedbackSetupBuilder.EnsureImpactFeedbackConfigProfiles();
            PrototypeVerticalSlicePrefabBuilder.EnsureAndWireSceneReferences(
                runDirector,
                spawnDirector,
                Object.FindFirstObjectByType<DamageNumberSpawner>());

            Selection.activeGameObject = root;
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Created desktop survivor prototype scene at {ScenePath}. Assign wave spawn groups, enemy prefabs, XP orb prefab, and ability pool before playtesting.", root);
        }

        [MenuItem("Tap Knockout/Survivor/Repair Current Scene EventSystem For Input System")]
        public static void RepairCurrentSceneEventSystem()
        {
            var eventSystem = EnsureCompatibleEventSystem();
            EditorSceneManager.MarkSceneDirty(eventSystem.gameObject.scene);
            Debug.Log("Repaired current scene EventSystem for the active input backend.", eventSystem);
        }

        [MenuItem("Tap Knockout/Survivor/Repair Current Scene Player Controls")]
        public static void RepairCurrentScenePlayerControls()
        {
            Time.timeScale = 1f;
            var movements = Object.FindObjectsByType<PlayerMovementController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var repairedCount = 0;
            for (var i = 0; i < movements.Length; i++)
            {
                if (movements[i] == null)
                {
                    continue;
                }

                RepairPlayerControls(movements[i].gameObject);
                repairedCount++;
            }

            if (repairedCount == 0)
            {
                Debug.LogWarning("No PlayerMovementController found in the current scene. Select the Player object or recreate the prototype scene.");
                return;
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"Repaired desktop controls for {repairedCount} player object(s).");
        }

        [MenuItem("Tap Knockout/Survivor/Repair Current Scene Survivor Runtime")]
        public static void RepairCurrentSceneSurvivorRuntime()
        {
            EnsureAssetFolders();

            var runConfig = LoadOrCreateAsset<RunConfig>(RunConfigPath);
            var arenaConfig = LoadOrCreateAsset<ArenaConfig>(ArenaConfigPath);
            EnsureAbilityCatalogAssets();
            CuteMonsterEnemyContentBuilder.BuildCuteMonsterEnemyContent(wirePrototypeRun: true, logToConsole: false);
            var waveTimeline = ResolvePrototypeWaveTimeline();
            WireRunConfig(runConfig, arenaConfig, waveTimeline);

            var playerMovement = Object.FindFirstObjectByType<PlayerMovementController>();
            var playerObject = playerMovement != null ? playerMovement.gameObject : GameObject.FindWithTag("Player");
            if (playerObject == null)
            {
                Debug.LogWarning("Could not find a Player object. Add or select the Player prefab before repairing survivor runtime.");
                return;
            }

            RepairPlayerControls(playerObject);

            var managers = ResolveOrCreateManagersRoot();
            var spawnDirector = Object.FindFirstObjectByType<SurvivorSpawnDirector>();
            if (spawnDirector == null)
            {
                spawnDirector = CreateChild(managers.transform, "SurvivorSpawnDirector").AddComponent<SurvivorSpawnDirector>();
            }

            var enemyPool = spawnDirector.GetComponent<EnemyPoolService>();
            if (enemyPool == null)
            {
                enemyPool = spawnDirector.gameObject.AddComponent<EnemyPoolService>();
            }

            var projectilePool = Object.FindFirstObjectByType<ProjectilePoolService>();
            if (projectilePool == null)
            {
                projectilePool = EnsureComponent<ProjectilePoolService>(ResolveOrCreateNamedChild(managers.transform, "ProjectilePoolService"));
            }

            var runDirector = Object.FindFirstObjectByType<ArenaRunDirector>();
            if (runDirector == null)
            {
                runDirector = CreateChild(managers.transform, "ArenaRunDirector").AddComponent<ArenaRunDirector>();
            }

            var bossDirector = runDirector.GetComponent<ArenaBossDirector>();
            if (bossDirector == null)
            {
                bossDirector = runDirector.gameObject.AddComponent<ArenaBossDirector>();
            }

            var debugRoot = GameObject.Find("DebugRoot");
            if (debugRoot == null)
            {
                var prototypeRoot = GameObject.Find("DesktopSurvivorPrototypeRoot");
                debugRoot = prototypeRoot != null ? CreateChild(prototypeRoot.transform, "DebugRoot") : new GameObject("DebugRoot");
            }

            var feedbackPlayer = Object.FindFirstObjectByType<SurvivorFeedbackPlayer>();
            if (feedbackPlayer == null)
            {
                feedbackPlayer = debugRoot.AddComponent<SurvivorFeedbackPlayer>();
            }

            var vfxPoolRoot = ResolveOrCreateNamedChild(debugRoot.transform, "VFXPoolRoot");
            var cameraShakeReceiver = ResolveOrCreateCameraShakeReceiver(debugRoot);
            SetObjectReference(feedbackPlayer, "vfxPoolRoot", vfxPoolRoot.transform);
            SetObjectReference(feedbackPlayer, "cameraShakeReceiver", cameraShakeReceiver);
            VFXFeedbackSetupBuilder.CreateFeedbackSystemRoot();
            VFXFeedbackSetupBuilder.EnsureImpactFeedbackConfigProfiles();

            var stressTest = Object.FindFirstObjectByType<SurvivorStressTestController>();
            if (stressTest == null)
            {
                stressTest = debugRoot.AddComponent<SurvivorStressTestController>();
            }

            var playerTransform = playerObject.transform;
            var playerHealth = playerObject.GetComponent<PlayerHealth>();
            var playerMovementController = playerObject.GetComponent<PlayerMovementController>();
            var playerDash = playerObject.GetComponent<PlayerDashController>();
            var playerStats = EnsureComponent<PlayerRuntimeStats>(playerObject);
            var abilityCombatEffects = EnsureComponent<PlayerAbilityCombatEffectController>(playerObject);
            abilityCombatEffects.SetRuntimeStats(playerStats);
            abilityCombatEffects.SetPlayerHealth(playerHealth);
            var desktopInput = EnsureComponent<DesktopInputReader>(playerObject);
            var mouseAim = EnsureComponent<MouseAimController>(playerObject);
            var aimReticle = EnsureComponent<MouseAimReticleController>(playerObject);
            var inputBridge = EnsureComponent<DesktopSurvivorInputBridge>(playerObject);
            var activeSkills = EnsureComponent<ActiveSkillController>(playerObject);
            var playerXP = EnsureComponent<PlayerXPController>(playerObject);
            var pickupCollector = EnsureComponent<PickupCollector>(playerObject);
            var cameraRig = ResolveOrCreateSurvivorCameraRig(playerTransform);
            var gameplayCamera = EnsureComponent<UnityEngine.Camera>(cameraRig.gameObject);
            ApplyDesktopSurvivorCameraPreset(gameplayCamera, cameraRig, playerTransform);
            mouseAim.SetAimCamera(gameplayCamera);
            ApplyStableDesktopFacingPreset(playerObject, playerMovementController, mouseAim, gameplayCamera);
            ConfigureAimReticle(aimReticle, mouseAim, desktopInput, playerMovementController != null ? playerMovementController.Config : null);
            var abilitySelection = runDirector.GetComponent<AbilitySelectionController>();
            if (abilitySelection == null)
            {
                abilitySelection = runDirector.gameObject.AddComponent<AbilitySelectionController>();
            }
            var abilityApplier = EnsureComponent<PlayerAbilityEffectApplier>(playerObject);
            abilityApplier.SetRuntimeStats(playerStats);
            abilityApplier.SetPlayerHealth(playerHealth);

            SetObjectReference(runDirector, "runConfig", runConfig);
            SetObjectReference(runDirector, "arenaConfigOverride", arenaConfig);
            SetObjectReference(runDirector, "waveTimelineOverride", waveTimeline);
            SetBool(runDirector, "autoStartOnStart", true);
            SetObjectReference(runDirector, "playerTransform", playerTransform);
            SetObjectReference(runDirector, "playerHealth", playerHealth);
            SetObjectReference(runDirector, "xpController", playerXP);
            SetObjectReference(runDirector, "pickupCollector", pickupCollector);
            SetObjectReference(runDirector, "abilitySelectionController", abilitySelection);
            abilitySelection.SetAbilityEffectApplier(abilityApplier);
            SetObjectReference(abilitySelection, "abilityEffectApplier", abilityApplier);
            SetObjectReference(runDirector, "spawnDirector", spawnDirector);
            SetObjectReference(runDirector, "bossDirector", bossDirector);

            bossDirector.Configure(spawnDirector);
            SetObjectReference(bossDirector, "feedbackPlayer", feedbackPlayer);
            spawnDirector.Configure(arenaConfig, waveTimeline, playerTransform);
            SetObjectReference(spawnDirector, "arenaConfig", arenaConfig);
            SetObjectReference(spawnDirector, "waveTimeline", waveTimeline);
            SetObjectReference(spawnDirector, "playerTarget", playerTransform);
            SetObjectReference(spawnDirector, "enemyPoolService", enemyPool);

            inputBridge.Configure(desktopInput, playerMovementController, playerDash, runDirector);
            inputBridge.SetActiveSkillController(activeSkills);
            activeSkills.Configure(inputBridge, abilitySelection, playerMovementController, playerStats, mouseAim, playerTransform, feedbackPlayer);
            SetObjectReference(inputBridge, "activeSkillController", activeSkills);
            SetObjectReference(activeSkills, "inputBridge", inputBridge);
            SetObjectReference(activeSkills, "abilitySelectionController", abilitySelection);
            SetObjectReference(activeSkills, "movementController", playerMovementController);
            SetObjectReference(activeSkills, "runtimeStats", playerStats);
            SetObjectReference(activeSkills, "mouseAimController", mouseAim);
            SetObjectReference(activeSkills, "playerConfig", playerMovementController != null ? playerMovementController.Config : null);
            SetObjectReference(activeSkills, "playerHealth", playerHealth);
            SetObjectReference(activeSkills, "castOrigin", playerTransform);
            SetObjectReference(activeSkills, "feedbackPlayer", feedbackPlayer);
            ApplyDefaultActiveSkillSlotPolicies(activeSkills);

            var gameplayCanvasObject = ResolveOrCreateGameplayCanvas();
            var hud = EnsureComponent<SurvivorHudController>(gameplayCanvasObject);
            RepairSurvivorHudUi(gameplayCanvasObject, hud, runDirector, spawnDirector, playerXP, playerHealth, playerDash, activeSkills);
            EnsureAbilitySelectionPanel(gameplayCanvasObject.transform, abilitySelection);

            SetObjectReference(stressTest, "spawnDirector", spawnDirector);
            SetObjectReference(stressTest, "enemyPoolService", enemyPool);
            SetObjectReference(stressTest, "projectilePoolService", projectilePool);
            SetObjectReference(stressTest, "runDirector", runDirector);
            SetObjectReference(stressTest, "bossDirector", bossDirector);
            PrototypeVerticalSlicePrefabBuilder.EnsureAndWireSceneReferences(
                runDirector,
                spawnDirector,
                Object.FindFirstObjectByType<DamageNumberSpawner>());

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            Debug.Log("Repaired survivor runtime wiring. Ensure WaveTimeline has an active entry and SpawnGroupConfig has EnemyConfig + EnemyPrefab assigned.", runDirector);
        }

        [MenuItem("Tap Knockout/Survivor/Repair Prototype Scenes Survivor Runtime")]
        public static void RepairPrototypeScenesSurvivorRuntime()
        {
            RepairSceneSurvivorRuntime(ScenePath);
            RepairSceneSurvivorRuntime(ForestScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void RepairSceneSurvivorRuntime(string scenePath)
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (sceneAsset == null)
            {
                Debug.LogWarning($"Could not repair survivor runtime because scene was not found: {scenePath}");
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            RepairCurrentSceneSurvivorRuntime();
            EditorSceneManager.SaveScene(scene);
        }

        [MenuItem("Tap Knockout/Survivor/Repair Current Scene Ability Selection UI")]
        public static void RepairCurrentSceneAbilitySelectionUi()
        {
            if (RepairCurrentSceneAbilitySelectionUiInternal())
            {
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                AssetDatabase.SaveAssets();
            }
        }

        [MenuItem("Tap Knockout/Survivor/Repair Prototype Scenes Ability Selection UI")]
        public static void RepairPrototypeScenesAbilitySelectionUi()
        {
            RepairSceneAbilitySelectionUi(ScenePath);
            RepairSceneAbilitySelectionUi(ForestScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureAssetFolders()
        {
            EnsureFolder("Assets/_Project/ScriptableObjects", "Runs");
            EnsureFolder("Assets/_Project/ScriptableObjects", "Arenas");
            EnsureFolder("Assets/_Project/ScriptableObjects", "Waves");
            EnsureFolder("Assets/_Project", "Scenes");
        }

        private static void EnsureFolder(string parent, string child)
        {
            var path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static T LoadOrCreateAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static WaveTimelineConfig ResolvePrototypeWaveTimeline()
        {
            return AssetDatabase.LoadAssetAtPath<WaveTimelineConfig>(CuteMonsterEnemyContentBuilder.TimelinePath)
                ?? LoadOrCreateAsset<WaveTimelineConfig>(WaveTimelinePath);
        }

        private static void WireRunConfig(RunConfig runConfig, ArenaConfig arenaConfig, WaveTimelineConfig waveTimeline)
        {
            SetObjectReference(runConfig, "arenaConfig", arenaConfig);
            SetObjectReference(runConfig, "waveTimeline", waveTimeline);
            EditorUtility.SetDirty(runConfig);
        }

        private static void EnsureAbilityCatalogAssets()
        {
            AbilityCatalogBuilder.CreateOrUpdateVerticalSliceCatalog();
            AbilityCatalogBuilder.WireRunConfigsToVerticalSliceCatalog();
        }

        private static GameObject CreatePlayerInstance(Vector3 position)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab == null)
            {
                var fallback = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                fallback.name = "Player_PrototypeFallback";
                fallback.transform.position = position;
                fallback.AddComponent<Rigidbody>();
                fallback.AddComponent<DesktopInputReader>();
                fallback.AddComponent<PlayerXPController>();
                fallback.AddComponent<PickupCollector>();
                return fallback;
            }

            var player = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            player.name = prefab.name;
            player.transform.position = position;
            return player;
        }

        private static void RepairPlayerControls(GameObject playerObject)
        {
            var desktopInput = EnsureComponent<DesktopInputReader>(playerObject);
            var movement = playerObject.GetComponent<PlayerMovementController>();
            var dash = playerObject.GetComponent<PlayerDashController>();
            var bridge = EnsureComponent<DesktopSurvivorInputBridge>(playerObject);
            var activeSkills = EnsureComponent<ActiveSkillController>(playerObject);
            var mouseAim = EnsureComponent<MouseAimController>(playerObject);
            var aimReticle = EnsureComponent<MouseAimReticleController>(playerObject);
            var runtimeStats = EnsureComponent<PlayerRuntimeStats>(playerObject);
            var abilityCombatEffects = EnsureComponent<PlayerAbilityCombatEffectController>(playerObject);
            abilityCombatEffects.SetRuntimeStats(runtimeStats);
            abilityCombatEffects.SetPlayerHealth(playerObject.GetComponent<PlayerHealth>());
            var feedbackPlayer = Object.FindFirstObjectByType<SurvivorFeedbackPlayer>();
            var cameraRig = Object.FindFirstObjectByType<SurvivorCameraRig>();
            var gameplayCamera = cameraRig != null
                ? cameraRig.GetComponent<UnityEngine.Camera>()
                : Object.FindFirstObjectByType<UnityEngine.Camera>();
            if (cameraRig != null && gameplayCamera != null)
            {
                ApplyDesktopSurvivorCameraPreset(gameplayCamera, cameraRig, playerObject.transform);
            }

            if (gameplayCamera != null)
            {
                mouseAim.SetAimCamera(gameplayCamera);
            }

            movement?.SetInputSource(desktopInput);
            if (movement != null)
            {
                movement.SetRotateTowardMovement(false);
                SetObjectReference(movement, "inputSourceBehaviour", desktopInput);
                SetBool(movement, "rotateTowardMovement", false);
            }

            ApplyStableDesktopFacingPreset(playerObject, movement, mouseAim, gameplayCamera);
            ConfigureAimReticle(aimReticle, mouseAim, desktopInput, movement != null ? movement.Config : null);

            var runDirector = Object.FindFirstObjectByType<ArenaRunDirector>();
            bridge.Configure(desktopInput, movement, dash, runDirector);
            bridge.SetActiveSkillController(activeSkills);
            SetObjectReference(bridge, "inputReader", desktopInput);
            SetObjectReference(bridge, "movementController", movement);
            SetObjectReference(bridge, "dashController", dash);
            SetObjectReference(bridge, "runDirector", runDirector);
            SetObjectReference(bridge, "activeSkillController", activeSkills);

            var abilitySelection = runDirector != null ? runDirector.GetComponent<AbilitySelectionController>() : null;
            var abilityApplier = EnsureComponent<PlayerAbilityEffectApplier>(playerObject);
            abilityApplier.SetRuntimeStats(runtimeStats);
            abilityApplier.SetPlayerHealth(playerObject.GetComponent<PlayerHealth>());
            if (abilitySelection != null)
            {
                abilitySelection.SetAbilityEffectApplier(abilityApplier);
                SetObjectReference(abilitySelection, "abilityEffectApplier", abilityApplier);
            }

            activeSkills.Configure(bridge, abilitySelection, movement, runtimeStats, mouseAim, playerObject.transform, feedbackPlayer);
            SetObjectReference(activeSkills, "inputBridge", bridge);
            SetObjectReference(activeSkills, "abilitySelectionController", abilitySelection);
            SetObjectReference(activeSkills, "movementController", movement);
            SetObjectReference(activeSkills, "runtimeStats", runtimeStats);
            SetObjectReference(activeSkills, "mouseAimController", mouseAim);
            SetObjectReference(activeSkills, "playerConfig", movement != null ? movement.Config : null);
            SetObjectReference(activeSkills, "playerHealth", playerObject.GetComponent<PlayerHealth>());
            SetObjectReference(activeSkills, "castOrigin", playerObject.transform);
            SetObjectReference(activeSkills, "feedbackPlayer", feedbackPlayer);
            ApplyDefaultActiveSkillSlotPolicies(activeSkills);

            if (playerObject.TryGetComponent<Rigidbody>(out var rigidbody))
            {
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
                rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                rigidbody.constraints &= ~RigidbodyConstraints.FreezePositionX;
                rigidbody.constraints &= ~RigidbodyConstraints.FreezePositionY;
                rigidbody.constraints &= ~RigidbodyConstraints.FreezePositionZ;
                rigidbody.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                EditorUtility.SetDirty(rigidbody);
            }

            EditorUtility.SetDirty(playerObject);
        }

        private static GameObject ResolveOrCreateManagersRoot()
        {
            var existing = GameObject.Find("Managers");
            if (existing != null)
            {
                return existing;
            }

            var prototypeRoot = GameObject.Find("DesktopSurvivorPrototypeRoot");
            if (prototypeRoot != null)
            {
                return CreateChild(prototypeRoot.transform, "Managers");
            }

            return new GameObject("Managers");
        }

        private static SurvivorCameraRig ResolveOrCreateSurvivorCameraRig(Transform playerTransform)
        {
            var cameraRig = Object.FindFirstObjectByType<SurvivorCameraRig>();
            if (cameraRig != null)
            {
                return cameraRig;
            }

            var camera = Object.FindFirstObjectByType<UnityEngine.Camera>();
            if (camera != null)
            {
                return EnsureComponent<SurvivorCameraRig>(camera.gameObject);
            }

            var prototypeRoot = GameObject.Find("DesktopSurvivorPrototypeRoot");
            var cameraRigObject = prototypeRoot != null
                ? ResolveOrCreateNamedChild(prototypeRoot.transform, "CameraRig")
                : new GameObject("CameraRig");
            cameraRigObject.transform.position = playerTransform != null
                ? playerTransform.position + SurvivorCameraOffset
                : SurvivorCameraOffset;
            return EnsureComponent<SurvivorCameraRig>(cameraRigObject);
        }

        private static void ApplyDesktopSurvivorCameraPreset(
            UnityEngine.Camera gameplayCamera,
            SurvivorCameraRig cameraRig,
            Transform playerTransform)
        {
            if (gameplayCamera != null)
            {
                gameplayCamera.tag = "MainCamera";
                gameplayCamera.usePhysicalProperties = false;
                gameplayCamera.orthographic = true;
                gameplayCamera.orthographicSize = SurvivorCameraOrthographicSize;
                gameplayCamera.fieldOfView = SurvivorCameraFieldOfView;
                gameplayCamera.nearClipPlane = SurvivorCameraNearClip;
                gameplayCamera.farClipPlane = SurvivorCameraFarClip;
                EditorUtility.SetDirty(gameplayCamera);
            }

            if (cameraRig == null)
            {
                return;
            }

            cameraRig.ApplySurvivor2_5DPreset(playerTransform, true);
            EditorUtility.SetDirty(cameraRig);
        }

        private static void ApplyStableDesktopFacingPreset(
            GameObject playerObject,
            PlayerMovementController movement,
            MouseAimController mouseAim,
            UnityEngine.Camera gameplayCamera)
        {
            if (playerObject == null)
            {
                return;
            }

            var desktopInput = playerObject.GetComponent<DesktopInputReader>();
            var attack = playerObject.GetComponent<PlayerAttackController>();
            var targetProvider = playerObject.GetComponent<PlayerTargetProvider>();
            var runtimeStats = playerObject.GetComponent<PlayerRuntimeStats>();
            var animationDriver = playerObject.GetComponent<CharacterAnimationDriver>();
            var playerHealth = playerObject.GetComponent<PlayerHealth>();
            var aimReticle = playerObject.GetComponent<MouseAimReticleController>();
            var cameraShakeReceiver = Object.FindFirstObjectByType<CameraShakeReceiver>();

            if (movement != null)
            {
                movement.SetRotateTowardMovement(false);
                SetBool(movement, "rotateTowardMovement", false);
            }

            if (attack != null)
            {
                SetObjectReference(attack, "playerConfig", movement != null ? movement.Config : null);
                SetObjectReference(attack, "desktopInputReader", desktopInput);
                SetObjectReference(attack, "movementController", movement);
                SetObjectReference(attack, "playerHealth", playerHealth);
                SetObjectReference(attack, "targetProvider", targetProvider);
                SetObjectReference(attack, "runtimeStats", runtimeStats);
                SetObjectReference(attack, "mouseAimController", mouseAim);
                SetObjectReference(attack, "aimReticle", aimReticle);
                SetObjectReference(attack, "shotCameraShakeReceiver", cameraShakeReceiver);
                var projectileSpawnPoint = FindChildRecursive(playerObject.transform, "ProjectileSpawnPoint");
                if (projectileSpawnPoint != null)
                {
                    SetObjectReference(attack, "projectileSpawnPoint", projectileSpawnPoint);
                }

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

            if (animationDriver != null)
            {
                SetBool(animationDriver, "isPlayer", true);
                SetBool(animationDriver, "playerAttackLocksDirectState", false);
                SetObjectReference(animationDriver, "playerMovement", movement);
                SetObjectReference(animationDriver, "playerAttack", attack);
                SetObjectReference(animationDriver, "playerDash", playerObject.GetComponent<PlayerDashController>());
                SetObjectReference(animationDriver, "playerHealth", playerObject.GetComponent<PlayerHealth>());
            }

            if (mouseAim != null && gameplayCamera != null)
            {
                mouseAim.SetAimCamera(gameplayCamera);
                SetObjectReference(mouseAim, "aimCamera", gameplayCamera);
            }

            if (mouseAim != null)
            {
                mouseAim.SetFacingTarget(playerObject.transform);
                SetObjectReference(mouseAim, "facingTarget", playerObject.transform);
                SetBool(mouseAim, "preferStableGroundPlane", true);
                SetBool(mouseAim, "usePhysicsRaycast", false);
                SetBool(mouseAim, "rotateFacingTarget", true);
                SetBool(mouseAim, "rotateRigidbodyInFixedUpdate", true);
                SetFloat(mouseAim, "fallbackGroundPlaneY", 0f);
                SetFloat(mouseAim, "minAimDirectionDistance", SurvivorAimMinDirectionDistance);
                SetLayerMask(mouseAim, "groundLayers", ResolveGroundMaskExcludingReticle(aimReticle));
            }

            ApplyManualFirePlayerConfigPreset(movement != null ? movement.Config : null);
        }

        private static void CreateArenaPlaceholder(Transform parent)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "PrototypeGround";
            ground.transform.SetParent(parent, false);
            ground.transform.localScale = new Vector3(5f, 1f, 5f);
        }

        private static void BuildPrototypeCanvas(
            GameObject canvasObject,
            ArenaRunDirector runDirector,
            SurvivorSpawnDirector spawnDirector,
            PlayerXPController xpController,
            PlayerHealth playerHealth,
            PlayerDashController dashController,
            ActiveSkillController activeSkillController,
            AbilitySelectionController abilitySelectionController)
        {
            var hud = EnsureComponent<SurvivorHudController>(canvasObject);
            RepairSurvivorHudUi(canvasObject, hud, runDirector, spawnDirector, xpController, playerHealth, dashController, activeSkillController);
            EnsureAbilitySelectionPanel(canvasObject != null ? canvasObject.transform : null, abilitySelectionController);
            EnsureCompatibleEventSystem();
        }

        private static void RepairSurvivorHudUi(
            GameObject canvasObject,
            SurvivorHudController hud,
            ArenaRunDirector runDirector,
            SurvivorSpawnDirector spawnDirector,
            PlayerXPController xpController,
            PlayerHealth playerHealth,
            PlayerDashController dashController,
            ActiveSkillController activeSkillController)
        {
            if (canvasObject == null)
            {
                return;
            }

            EnsureGameplayCanvasComponents(canvasObject);
            if (hud == null)
            {
                hud = EnsureComponent<SurvivorHudController>(canvasObject);
            }

            hud.Bind(runDirector, spawnDirector, xpController, playerHealth, dashController, activeSkillController);
            SetObjectReference(hud, "runDirector", runDirector);
            SetObjectReference(hud, "spawnDirector", spawnDirector);
            SetObjectReference(hud, "xpController", xpController);
            SetObjectReference(hud, "playerHealth", playerHealth);
            SetObjectReference(hud, "dashController", dashController);
            SetObjectReference(hud, "activeSkillController", activeSkillController);

            var slots = EnsureActiveSkillSlots(canvasObject.transform);
            SetObjectArrayReference(hud, "activeSkillSlots", slots);
            EnsureBossHealthBarUnderCanvas(canvasObject.transform);
            EditorUtility.SetDirty(hud);
        }

        private static bool RepairCurrentSceneAbilitySelectionUiInternal()
        {
            var canvasObject = ResolveOrCreateGameplayCanvas();
            var abilitySelection = Object.FindFirstObjectByType<AbilitySelectionController>(FindObjectsInactive.Include);
            if (abilitySelection == null)
            {
                Debug.LogWarning("No AbilitySelectionController found while repairing ability selection UI.");
                return false;
            }

            EnsureAbilitySelectionPanel(canvasObject.transform, abilitySelection);
            EnsureCompatibleEventSystem();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            return true;
        }

        private static void RepairSceneAbilitySelectionUi(string scenePath)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                Debug.LogWarning($"Skipping ability selection UI repair. Scene not found: {scenePath}");
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (RepairCurrentSceneAbilitySelectionUiInternal())
            {
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"Repaired ability selection UI in {scenePath}.");
            }
        }

        private static AbilitySelectionPanelController EnsureAbilitySelectionPanel(
            Transform canvasTransform,
            AbilitySelectionController selectionController)
        {
            if (canvasTransform == null || selectionController == null)
            {
                return null;
            }

            var panelTransform = EnsureUiChild(canvasTransform, "AbilitySelectionPanel");
            panelTransform.SetAsLastSibling();

            var panelRect = panelTransform.GetComponent<RectTransform>();
            Stretch(panelRect);

            var overlay = EnsureComponent<Image>(panelTransform.gameObject);
            overlay.color = new Color(0f, 0f, 0f, 0.68f);

            var canvasGroup = EnsureComponent<CanvasGroup>(panelTransform.gameObject);
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            var title = EnsureText(panelTransform, "Title", "Choose an Ability", 44, TextAnchor.MiddleCenter);
            title.rectTransform.anchorMin = new Vector2(0.5f, 0.88f);
            title.rectTransform.anchorMax = new Vector2(0.5f, 0.88f);
            title.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            title.rectTransform.anchoredPosition = Vector2.zero;
            title.rectTransform.sizeDelta = new Vector2(760f, 72f);

            var row = EnsureUiChild(panelTransform, "CardRow");
            var rowRect = row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.5f, 0.48f);
            rowRect.anchorMax = new Vector2(0.5f, 0.48f);
            rowRect.pivot = new Vector2(0.5f, 0.5f);
            rowRect.anchoredPosition = Vector2.zero;
            rowRect.sizeDelta = new Vector2(1010f, 560f);

            var layout = EnsureComponent<HorizontalLayoutGroup>(row.gameObject);
            layout.spacing = 26f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var cardViews = new AbilityChoiceCardView[3];
            for (var i = 0; i < cardViews.Length; i++)
            {
                cardViews[i] = EnsureAbilityChoiceCard(row, i);
            }

            var oldHint = panelTransform.Find("HotkeyHint");
            if (oldHint != null)
            {
                Object.DestroyImmediate(oldHint.gameObject);
            }

            var panel = EnsureComponent<AbilitySelectionPanelController>(panelTransform.gameObject);
            SetObjectReference(panel, "abilitySelectionController", selectionController);
            SetObjectReference(panel, "canvasGroup", canvasGroup);
            SetObjectArrayReference(panel, "cardViews", cardViews);
            SetBool(panel, "hideOnAwake", true);
            SetBool(panel, "pauseGameWhileOpen", true);
            EditorUtility.SetDirty(panel);
            EditorUtility.SetDirty(panelTransform.gameObject);
            return panel;
        }

        private static AbilityChoiceCardView EnsureAbilityChoiceCard(Transform parent, int index)
        {
            var card = EnsureUiChild(parent, $"AbilityCard_{index + 1}");
            card.gameObject.SetActive(true);

            var rect = card.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300f, 520f);

            var layoutElement = EnsureComponent<LayoutElement>(card.gameObject);
            layoutElement.preferredWidth = 300f;
            layoutElement.preferredHeight = 520f;
            layoutElement.flexibleWidth = 0f;
            layoutElement.flexibleHeight = 0f;

            var background = EnsureComponent<Image>(card.gameObject);
            background.color = new Color(0.84f, 0.88f, 0.92f, 1f);

            var button = EnsureComponent<Button>(card.gameObject);
            button.targetGraphic = background;

            var cardLayout = EnsureComponent<VerticalLayoutGroup>(card.gameObject);
            cardLayout.padding = new RectOffset(22, 22, 22, 22);
            cardLayout.spacing = 14f;
            cardLayout.childAlignment = TextAnchor.UpperCenter;
            cardLayout.childControlWidth = true;
            cardLayout.childControlHeight = false;
            cardLayout.childForceExpandWidth = true;
            cardLayout.childForceExpandHeight = false;

            var icon = EnsureImage(card, "Icon", new Color(1f, 1f, 1f, 0.18f));
            var iconLayout = EnsureComponent<LayoutElement>(icon.gameObject);
            iconLayout.preferredWidth = 108f;
            iconLayout.preferredHeight = 108f;
            icon.preserveAspect = true;
            icon.enabled = false;

            var title = EnsureText(card, "Title", string.Empty, 28, TextAnchor.MiddleCenter);
            SetLayout(title.gameObject, 250f, 72f);
            title.color = new Color(0.08f, 0.09f, 0.11f, 1f);

            var rarity = EnsureText(card, "Rarity", string.Empty, 18, TextAnchor.MiddleCenter);
            SetLayout(rarity.gameObject, 250f, 32f);
            rarity.color = title.color;

            var description = EnsureText(card, "Description", string.Empty, 20, TextAnchor.UpperCenter);
            SetLayout(description.gameObject, 250f, 166f);
            description.color = title.color;

            var stack = EnsureText(card, "Stack", string.Empty, 18, TextAnchor.MiddleCenter);
            SetLayout(stack.gameObject, 250f, 36f);
            stack.color = title.color;

            var view = EnsureComponent<AbilityChoiceCardView>(card.gameObject);
            var serializedObject = new SerializedObject(view);
            SetSerializedObjectReference(serializedObject, "selectButton", button);
            SetSerializedObjectReference(serializedObject, "background", background);
            SetSerializedObjectReference(serializedObject, "icon", icon);
            SetSerializedObjectReference(serializedObject, "titleText", title);
            SetSerializedObjectReference(serializedObject, "descriptionText", description);
            SetSerializedObjectReference(serializedObject, "rarityText", rarity);
            SetSerializedObjectReference(serializedObject, "stackText", stack);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(view);
            return view;
        }

        private static void SetLayout(GameObject target, float preferredWidth, float preferredHeight)
        {
            var layout = EnsureComponent<LayoutElement>(target);
            layout.preferredWidth = preferredWidth;
            layout.preferredHeight = preferredHeight;
        }

        private static GameObject ResolveOrCreateGameplayCanvas()
        {
            var namedCanvas = GameObject.Find("GameplayCanvas");
            if (namedCanvas != null)
            {
                EnsureGameplayCanvasComponents(namedCanvas);
                return namedCanvas;
            }

            var existingCanvas = Object.FindFirstObjectByType<Canvas>();
            if (existingCanvas != null)
            {
                EnsureGameplayCanvasComponents(existingCanvas.gameObject);
                return existingCanvas.gameObject;
            }

            var canvasObject = new GameObject("GameplayCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            EnsureGameplayCanvasComponents(canvasObject);
            return canvasObject;
        }

        private static void EnsureGameplayCanvasComponents(GameObject canvasObject)
        {
            if (canvasObject == null)
            {
                return;
            }

            var canvas = EnsureComponent<Canvas>(canvasObject);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = EnsureComponent<CanvasScaler>(canvasObject);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            EnsureComponent<GraphicRaycaster>(canvasObject);
        }

        private static ActiveSkillSlotHud[] EnsureActiveSkillSlots(Transform canvasTransform)
        {
            var root = EnsureUiChild(canvasTransform, "ActiveSkillHud");
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0f);
            rootRect.anchorMax = new Vector2(0.5f, 0f);
            rootRect.pivot = new Vector2(0.5f, 0f);
            rootRect.anchoredPosition = new Vector2(0f, 34f);
            rootRect.sizeDelta = new Vector2(360f, 78f);

            var layout = root.GetComponent<HorizontalLayoutGroup>() ?? root.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(0, 0, 0, 0);

            var slots = new ActiveSkillSlotHud[ActiveSkillHotkeys.Length];
            for (var i = 0; i < ActiveSkillHotkeys.Length; i++)
            {
                slots[i] = EnsureActiveSkillSlot(root, ActiveSkillHotkeys[i]);
            }

            return slots;
        }

        private static ActiveSkillSlotHud EnsureActiveSkillSlot(Transform parent, string hotkey)
        {
            var slot = EnsureUiChild(parent, $"ActiveSkillSlot_{hotkey}");
            var slotRect = slot.GetComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(78f, 70f);

            var layoutElement = slot.GetComponent<LayoutElement>() ?? slot.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 78f;
            layoutElement.preferredHeight = 70f;
            layoutElement.minWidth = 70f;
            layoutElement.minHeight = 66f;
            layoutElement.flexibleWidth = 0f;
            layoutElement.flexibleHeight = 0f;

            var background = slot.GetComponent<Image>() ?? slot.gameObject.AddComponent<Image>();
            background.color = new Color(0.08f, 0.1f, 0.13f, 0.84f);
            var canvasGroup = slot.GetComponent<CanvasGroup>() ?? slot.gameObject.AddComponent<CanvasGroup>();
            var view = slot.GetComponent<ActiveSkillSlotHud>() ?? slot.gameObject.AddComponent<ActiveSkillSlotHud>();

            var readyFrame = EnsureImage(slot, "ReadyFrame", new Color(0.35f, 0.95f, 0.65f, 0.25f));
            Stretch(readyFrame.rectTransform);

            var icon = EnsureImage(slot, "Icon", Color.white);
            icon.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            icon.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            icon.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            icon.rectTransform.anchoredPosition = new Vector2(0f, 6f);
            icon.rectTransform.sizeDelta = new Vector2(42f, 42f);
            icon.preserveAspect = true;

            var cooldown = EnsureImage(slot, "CooldownFill", new Color(0f, 0f, 0f, 0.5f));
            Stretch(cooldown.rectTransform);
            cooldown.type = Image.Type.Filled;
            cooldown.fillMethod = Image.FillMethod.Radial360;
            cooldown.fillOrigin = (int)Image.Origin360.Top;
            cooldown.fillClockwise = false;
            cooldown.fillAmount = 0f;

            var disabled = EnsureImage(slot, "DisabledOverlay", new Color(0f, 0f, 0f, 0.48f));
            Stretch(disabled.rectTransform);
            disabled.enabled = false;

            var hotkeyLabel = EnsureText(slot, "Hotkey", hotkey, 18, TextAnchor.UpperLeft);
            hotkeyLabel.rectTransform.anchorMin = new Vector2(0f, 1f);
            hotkeyLabel.rectTransform.anchorMax = new Vector2(0f, 1f);
            hotkeyLabel.rectTransform.pivot = new Vector2(0f, 1f);
            hotkeyLabel.rectTransform.anchoredPosition = new Vector2(6f, -4f);
            hotkeyLabel.rectTransform.sizeDelta = new Vector2(28f, 22f);

            var state = EnsureText(slot, "State", "READY", 11, TextAnchor.LowerCenter);
            state.rectTransform.anchorMin = new Vector2(0f, 0f);
            state.rectTransform.anchorMax = new Vector2(1f, 0f);
            state.rectTransform.pivot = new Vector2(0.5f, 0f);
            state.rectTransform.anchoredPosition = new Vector2(0f, 4f);
            state.rectTransform.sizeDelta = new Vector2(0f, 16f);

            var charges = EnsureText(slot, "Charges", string.Empty, 12, TextAnchor.UpperRight);
            charges.rectTransform.anchorMin = new Vector2(1f, 1f);
            charges.rectTransform.anchorMax = new Vector2(1f, 1f);
            charges.rectTransform.pivot = new Vector2(1f, 1f);
            charges.rectTransform.anchoredPosition = new Vector2(-6f, -5f);
            charges.rectTransform.sizeDelta = new Vector2(28f, 18f);

            var serializedObject = new SerializedObject(view);
            SetSerializedObjectReference(serializedObject, "iconImage", icon);
            SetSerializedObjectReference(serializedObject, "cooldownFillImage", cooldown);
            SetSerializedObjectReference(serializedObject, "hotkeyLabel", hotkeyLabel);
            SetSerializedObjectReference(serializedObject, "chargesLabel", charges);
            SetSerializedObjectReference(serializedObject, "stateLabel", state);
            SetSerializedObjectReference(serializedObject, "readyFrameImage", readyFrame);
            SetSerializedObjectReference(serializedObject, "disabledOverlayImage", disabled);
            SetSerializedObjectReference(serializedObject, "canvasGroup", canvasGroup);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            return view;
        }

        private static BossHealthBarController EnsureBossHealthBarUnderCanvas(Transform canvasTransform)
        {
            var existingBars = Object.FindObjectsByType<BossHealthBarController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (existingBars.Length > 0)
            {
                var existing = existingBars[0];
                if (existing.GetComponentInParent<Canvas>(true) == null && canvasTransform != null)
                {
                    existing.transform.SetParent(canvasTransform, false);
                }

                existing.gameObject.name = "BossHealthBar_Playtest";
                return existing;
            }

            var prefab = BossHealthBarSetupBuilder.CreateOrUpdateBossHealthBarPlaceholder();
            GameObject instance = null;
            if (prefab != null)
            {
                instance = PrefabUtility.InstantiatePrefab(prefab, canvasTransform) as GameObject;
                if (instance == null)
                {
                    instance = Object.Instantiate(prefab, canvasTransform);
                }
            }

            if (instance == null)
            {
                instance = new GameObject("BossHealthBar_Playtest", typeof(RectTransform), typeof(CanvasGroup), typeof(BossHealthBarController));
                instance.transform.SetParent(canvasTransform, false);
            }

            instance.name = "BossHealthBar_Playtest";
            var rect = instance.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.24f, 0.9f);
                rect.anchorMax = new Vector2(0.76f, 0.97f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            return instance.GetComponentInChildren<BossHealthBarController>(true)
                ?? instance.AddComponent<BossHealthBarController>();
        }

        private static Transform EnsureUiChild(Transform parent, string name)
        {
            var existing = parent != null ? parent.Find(name) : null;
            if (existing != null)
            {
                return existing;
            }

            var child = new GameObject(name, typeof(RectTransform));
            if (parent != null)
            {
                child.transform.SetParent(parent, false);
            }

            return child.transform;
        }

        private static Image EnsureImage(Transform parent, string name, Color color)
        {
            var child = EnsureUiChild(parent, name);
            var image = child.GetComponent<Image>() ?? child.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text EnsureText(Transform parent, string name, string text, int fontSize, TextAnchor alignment)
        {
            var child = EnsureUiChild(parent, name);
            var label = child.GetComponent<Text>() ?? child.gameObject.AddComponent<Text>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = Color.white;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return label;
        }

        private static void Stretch(RectTransform rect)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static EventSystem EnsureCompatibleEventSystem()
        {
            var eventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                var eventSystemObject = new GameObject("EventSystem");
                eventSystem = eventSystemObject.AddComponent<EventSystem>();
            }

#if ENABLE_INPUT_SYSTEM
            var legacyModules = eventSystem.GetComponents<StandaloneInputModule>();
            for (var i = 0; i < legacyModules.Length; i++)
            {
                Object.DestroyImmediate(legacyModules[i]);
            }

            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
#else
            if (eventSystem.GetComponent<StandaloneInputModule>() == null)
            {
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }
#endif

            return eventSystem;
        }

        private static GameObject CreateChild(Transform parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static GameObject ResolveOrCreateNamedChild(Transform parent, string name)
        {
            if (parent == null)
            {
                return new GameObject(name);
            }

            var existing = parent.Find(name);
            return existing != null ? existing.gameObject : CreateChild(parent, name);
        }

        private static CameraShakeReceiver ResolveOrCreateCameraShakeReceiver(GameObject fallbackRoot)
        {
            var existing = Object.FindFirstObjectByType<CameraShakeReceiver>();
            if (existing != null)
            {
                return existing;
            }

            var cameraRig = Object.FindFirstObjectByType<SurvivorCameraRig>();
            if (cameraRig != null)
            {
                return EnsureComponent<CameraShakeReceiver>(cameraRig.gameObject);
            }

            var sceneCamera = Object.FindFirstObjectByType<UnityEngine.Camera>();
            if (sceneCamera != null)
            {
                return EnsureComponent<CameraShakeReceiver>(sceneCamera.gameObject);
            }

            return fallbackRoot != null ? EnsureComponent<CameraShakeReceiver>(fallbackRoot) : null;
        }

        private static void ConfigureAimReticle(
            MouseAimReticleController reticle,
            MouseAimController mouseAim,
            DesktopInputReader desktopInput,
            PlayerConfig playerConfig)
        {
            if (reticle == null)
            {
                return;
            }

            reticle.SetAimController(mouseAim);
            reticle.SetInputReader(desktopInput);
            SetObjectReference(reticle, "aimController", mouseAim);
            SetObjectReference(reticle, "inputReader", desktopInput);
            SetBool(reticle, "reticleEnabled", playerConfig == null || playerConfig.AimReticleEnabled);
            SetBool(reticle, "allowRuntimeFallback", true);
            SetBool(reticle, "showReticleOnlyDuringGameplay", playerConfig == null || playerConfig.ShowReticleOnlyDuringGameplay);
            SetBool(reticle, "showReticleOnlyWhileAimingOrFiring", playerConfig != null && playerConfig.ShowReticleOnlyWhileAimingOrFiring);
            SetBool(reticle, "hideSystemCursorDuringGameplay", playerConfig == null || playerConfig.HideSystemCursorDuringGameplay);
            SetFloat(reticle, "reticleScale", playerConfig != null ? playerConfig.AimReticleScale : 1f);
            SetFloat(reticle, "yOffset", playerConfig != null ? playerConfig.AimReticleYOffset : 0.18f);
            SetFloat(reticle, "smoothTime", playerConfig != null ? playerConfig.AimReticleSmoothTime : 0f);
            SetEnum(
                reticle,
                "invalidAimBehavior",
                (int)(playerConfig != null
                    ? playerConfig.ReticleInvalidAimBehavior
                    : ReticleInvalidAimBehavior.ShowAtFallbackPoint));
        }

        private static void ApplyManualFirePlayerConfigPreset(PlayerConfig config)
        {
            if (config == null)
            {
                return;
            }

            SetEnum(config, "primaryAttackFirePolicy", (int)PrimaryAttackFirePolicy.HoldMouseAim);
            SetBool(config, "attackWhileMoving", true);
            SetBool(config, "manualFireRequiresInput", true);
            SetBool(config, "aimReticleEnabled", true);
            SetFloat(config, "aimReticleScale", 1f);
            SetFloat(config, "aimReticleYOffset", 0.18f);
            SetFloat(config, "aimReticleSmoothTime", 0f);
            SetBool(config, "hideSystemCursorDuringGameplay", true);
            SetBool(config, "showReticleOnlyDuringGameplay", true);
            SetBool(config, "showReticleOnlyWhileAimingOrFiring", false);
            SetEnum(config, "reticleInvalidAimBehavior", (int)ReticleInvalidAimBehavior.ShowAtFallbackPoint);
        }

        private static int ResolveGroundMaskExcludingReticle(MouseAimReticleController reticle)
        {
            var reticleLayer = reticle != null ? reticle.ReticleLayer : 2;
            return ~(1 << Mathf.Clamp(reticleLayer, 0, 31));
        }

        private static T EnsureComponent<T>(GameObject gameObject) where T : Component
        {
            if (gameObject.TryGetComponent<T>(out var existing))
            {
                return existing;
            }

            return gameObject.AddComponent<T>();
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

        private static void SetObjectArrayReference(Object target, string propertyName, Object[] values)
        {
            if (target == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            if (property == null || !property.isArray)
            {
                return;
            }

            property.arraySize = values != null ? values.Length : 0;
            for (var i = 0; values != null && i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetSerializedObjectReference(SerializedObject serializedObject, string propertyName, Object value)
        {
            var property = serializedObject?.FindProperty(propertyName);
            if (property == null || property.propertyType != SerializedPropertyType.ObjectReference)
            {
                return;
            }

            property.objectReferenceValue = value;
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

        private static void SetLayerMask(Object target, string propertyName, int value)
        {
            if (target == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            if (property == null || property.propertyType != SerializedPropertyType.LayerMask)
            {
                return;
            }

            property.intValue = value;
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

        private static void ApplyDefaultActiveSkillSlotPolicies(ActiveSkillController activeSkills)
        {
            if (activeSkills == null)
            {
                return;
            }

            AbilityCatalogBuilder.CreateOrUpdateVerticalSliceCatalog();
            var arcBlast = AbilityCatalogBuilder.LoadVerticalSliceAbility(ArcBlastAbilityId);
            var groundSlam = AbilityCatalogBuilder.LoadVerticalSliceAbility(GroundSlamAbilityId);

            var serializedObject = new SerializedObject(activeSkills);
            var slotsProperty = serializedObject.FindProperty("slots");
            if (slotsProperty == null || !slotsProperty.isArray)
            {
                return;
            }

            SetActiveSkillSlotPolicy(
                slotsProperty,
                0,
                ActiveSkillAimMode.MouseAim,
                ActiveSkillTargetMode.DirectionalArea,
                ActiveSkillOriginMode.Player);
            SetActiveSkillSlotAbility(slotsProperty, 0, arcBlast);
            SetActiveSkillSlotPolicy(
                slotsProperty,
                1,
                ActiveSkillAimMode.MouseAim,
                ActiveSkillTargetMode.SelfArea,
                ActiveSkillOriginMode.Player);
            SetActiveSkillSlotAbility(slotsProperty, 1, groundSlam);
            SetActiveSkillSlotPolicy(
                slotsProperty,
                2,
                ActiveSkillAimMode.MouseAim,
                ActiveSkillTargetMode.DirectionalArea,
                ActiveSkillOriginMode.Player);
            SetActiveSkillSlotAbility(slotsProperty, 2, arcBlast);
            SetActiveSkillSlotPolicy(
                slotsProperty,
                3,
                ActiveSkillAimMode.MouseAim,
                ActiveSkillTargetMode.SelfArea,
                ActiveSkillOriginMode.Player);
            SetActiveSkillSlotAbility(slotsProperty, 3, groundSlam);

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(activeSkills);
        }

        private static void SetActiveSkillSlotPolicy(
            SerializedProperty slotsProperty,
            int slotIndex,
            ActiveSkillAimMode aimMode,
            ActiveSkillTargetMode targetMode,
            ActiveSkillOriginMode originMode)
        {
            if (slotIndex < 0 || slotIndex >= slotsProperty.arraySize)
            {
                return;
            }

            var slot = slotsProperty.GetArrayElementAtIndex(slotIndex);
            SetEnumProperty(slot.FindPropertyRelative("aimMode"), (int)aimMode);
            SetEnumProperty(slot.FindPropertyRelative("targetMode"), (int)targetMode);
            SetEnumProperty(slot.FindPropertyRelative("originMode"), (int)originMode);

            var lockMovement = slot.FindPropertyRelative("lockMovementDuringCast");
            if (lockMovement != null && lockMovement.propertyType == SerializedPropertyType.Boolean)
            {
                lockMovement.boolValue = false;
            }
        }

        private static void SetActiveSkillSlotAbility(
            SerializedProperty slotsProperty,
            int slotIndex,
            AbilityDefinition ability)
        {
            if (slotIndex < 0 || slotIndex >= slotsProperty.arraySize)
            {
                return;
            }

            var slot = slotsProperty.GetArrayElementAtIndex(slotIndex);
            var abilityProperty = slot.FindPropertyRelative("ability");
            if (abilityProperty == null || abilityProperty.propertyType != SerializedPropertyType.ObjectReference)
            {
                return;
            }

            abilityProperty.objectReferenceValue = ability;
        }

        private static void SetEnumProperty(SerializedProperty property, int value)
        {
            if (property == null || property.propertyType != SerializedPropertyType.Enum)
            {
                return;
            }

            property.enumValueIndex = Mathf.Clamp(value, 0, property.enumDisplayNames.Length - 1);
        }

        private static Transform FindChildRecursive(Transform root, string childName)
        {
            if (root == null || string.IsNullOrWhiteSpace(childName))
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var match = FindChildRecursive(root.GetChild(i), childName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }
    }
}
#endif
