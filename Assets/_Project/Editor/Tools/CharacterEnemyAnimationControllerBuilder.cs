using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TapKnockout.Characters;
using TapKnockout.Enemy;
using TapKnockout.Player;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace TapKnockout.Editor.Tools
{
    public static class CharacterEnemyAnimationControllerBuilder
    {
        private const string ControllerRootPath = "Assets/_Project/Animation/Controllers";
        private const string ReportPath = "Assets/_Project/Docs/CharacterEnemyAnimationControllerBuilderReport.md";

        private static readonly string[] ClipScanRoots =
        {
            "Assets/_Project",
            "Assets/Assets/game asset packs",
            "Assets/ThirdParty"
        };

        [MenuItem("Tools/Tap Knockout/Characters/Build Animation Controllers For Generated Prefabs")]
        public static void BuildAndApplyAnimationControllers()
        {
            var report = BuildAndApplyAnimationControllersInternal();
            Debug.Log(report);
        }

        public static string BuildAndApplyAnimationControllersInternal()
        {
            EnsureFolder(ControllerRootPath);

            var report = new StringBuilder();
            report.AppendLine("# Character Enemy Animation Controller Builder Report");
            report.AppendLine();
            report.AppendLine("Project-owned Animator Controllers are generated under `Assets/_Project/Animation/Controllers`.");
            report.AppendLine("Source model contents and ThirdParty assets are not modified; selected model import settings may be prepared for Generic same-asset animation clips.");
            report.AppendLine();

            var importSettingsReport = new StringBuilder();
            PrepareAnimationImportSettings(importSettingsReport);
            if (importSettingsReport.Length > 0)
            {
                report.AppendLine("## Import Settings Preparation");
                report.Append(importSettingsReport);
                report.AppendLine();
            }

            var clipLibrary = AnimationClipLibrary.Build(ClipScanRoots);
            report.AppendLine("## Clip Scan");
            report.AppendLine($"- Candidate animation clips: {clipLibrary.Count}");
            report.AppendLine();

            BuildAndApplyForSpec(CharacterEnemyAssetSelection.Player, isPlayer: true, clipLibrary, report);
            for (var i = 0; i < CharacterEnemyAssetSelection.Enemies.Length; i++)
            {
                BuildAndApplyForSpec(CharacterEnemyAssetSelection.Enemies[i], isPlayer: false, clipLibrary, report);
            }

            WriteTextAsset(ReportPath, report.ToString());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return report.ToString();
        }

        public static string GetControllerPath(CharacterEnemyRoleId roleId)
        {
            return roleId == CharacterEnemyRoleId.MainPlayer
                ? $"{ControllerRootPath}/AC_Player_Rogue.controller"
                : $"{ControllerRootPath}/AC_Enemy_{roleId}.controller";
        }

        private static void BuildAndApplyForSpec(
            CharacterEnemyAssetSpec spec,
            bool isPlayer,
            AnimationClipLibrary clipLibrary,
            StringBuilder report)
        {
            var controllerPath = GetControllerPath(spec.RoleId);
            var selection = SelectClips(spec, clipLibrary);
            var controller = CreateController(controllerPath, selection);
            var applied = ApplyControllerToGeneratedPrefab(spec, controller, isPlayer);

            report.AppendLine($"## {spec.DisplayName}");
            report.AppendLine($"- Role: `{spec.RoleId}`");
            report.AppendLine($"- Generated prefab: `{spec.GeneratedPrefabPath}`");
            report.AppendLine($"- Animator Controller: `{controllerPath}`");
            report.AppendLine($"- Prefab wiring: {(applied ? "applied" : "skipped")}");
            AppendClip(report, "Idle", selection.Idle);
            AppendClip(report, "Move", selection.Move);
            AppendClip(report, "Attack", selection.Attack);
            AppendClip(report, "Dash", selection.Dash);
            AppendClip(report, "Hit", selection.Hit);
            AppendClip(report, "Death", selection.Death);
            report.AppendLine();
        }

        private static ClipSelection SelectClips(CharacterEnemyAssetSpec spec, AnimationClipLibrary clipLibrary)
        {
            var selection = new ClipSelection
            {
                Idle = clipLibrary.SelectBest(spec, ClipRole.Idle),
                Move = clipLibrary.SelectBest(spec, ClipRole.Move),
                Attack = clipLibrary.SelectBest(spec, ClipRole.Attack),
                Dash = clipLibrary.SelectBest(spec, ClipRole.Dash),
                Hit = clipLibrary.SelectBest(spec, ClipRole.Hit),
                Death = clipLibrary.SelectBest(spec, ClipRole.Death)
            };

            selection.Move ??= selection.Idle;
            selection.Attack ??= selection.Move;
            selection.Dash ??= selection.Move;
            selection.Hit ??= selection.Attack;
            ApplyPlayerRangerClipOverrides(spec, selection);
            return selection;
        }

        private static void ApplyPlayerRangerClipOverrides(CharacterEnemyAssetSpec spec, ClipSelection selection)
        {
            if (spec.RoleId != CharacterEnemyRoleId.MainPlayer ||
                !IsPlayerRangerVisual(spec.VisualAssetPath))
            {
                return;
            }

            selection.Idle = ResolvePlayerRangerPackClip(
                spec.VisualAssetPath,
                "CharacterArmature|ACIdle_Weapon",
                "ACIdle_Weapon",
                "CharacterArmature|Idle_Weapon",
                "Idle_Weapon",
                "CharacterArmature|ACIdle_Attacking",
                "ACIdle_Attacking",
                "CharacterArmature|Idle_Attacking",
                "Idle_Attacking",
                "CharacterArmature|ACIdle",
                "ACIdle",
                "CharacterArmature|Idle",
                "Idle") ?? selection.Idle;
            selection.Move = ResolvePlayerRangerPackClip(
                spec.VisualAssetPath,
                "CharacterArmature|ACRun_Holding",
                "ACRun_Holding",
                "CharacterArmature|Run_Holding",
                "Run_Holding",
                "CharacterArmature|ACRun",
                "ACRun",
                "CharacterArmature|Run",
                "Run",
                "CharacterArmature|Walk",
                "Walk") ?? selection.Move;
            selection.Attack = ResolvePlayerRangerPackClip(
                spec.VisualAssetPath,
                "CharacterArmature|ACBow_Attack_Shoot",
                "ACBow_Attack_Shoot",
                "CharacterArmature|Bow_Attack_Shoot",
                "Bow_Attack_Shoot",
                "CharacterArmature|ACBow_Attack_Draw",
                "ACBow_Attack_Draw",
                "CharacterArmature|Bow_Attack_Draw",
                "Bow_Attack_Draw") ?? selection.Attack;
            selection.Dash = ResolvePlayerRangerPackClip(
                spec.VisualAssetPath,
                "CharacterArmature|ACRoll",
                "ACRoll",
                "CharacterArmature|Roll",
                "Roll") ?? selection.Dash;
            selection.Hit = ResolvePlayerRangerPackClip(
                spec.VisualAssetPath,
                "CharacterArmature|ACRecieveHit_2",
                "ACRecieveHit_2",
                "CharacterArmature|ACRecieveHit",
                "ACRecieveHit",
                "CharacterArmature|RecieveHit_Attacking",
                "RecieveHit_Attacking",
                "CharacterArmature|RecieveHit",
                "RecieveHit") ?? selection.Hit;
            selection.Death = ResolvePlayerRangerPackClip(
                spec.VisualAssetPath,
                "CharacterArmature|ACDeath",
                "ACDeath",
                "CharacterArmature|Death",
                "Death") ?? selection.Death;
        }

        private static bool IsPlayerRangerVisual(string visualAssetPath)
        {
            return Path.GetFileNameWithoutExtension(visualAssetPath)
                .Equals("Ranger", StringComparison.OrdinalIgnoreCase);
        }

        private static AnimationClip ResolvePlayerRangerPackClip(
            string sourceAssetPath,
            params string[] sourceClipNames)
        {
            return FindClipAtPath(sourceAssetPath, sourceClipNames)
                ?? FindPreviewClipAtPath(sourceAssetPath, sourceClipNames);
        }

        private static AnimatorController CreateController(string controllerPath, ClipSelection selection)
        {
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
            {
                AssetDatabase.DeleteAsset(controllerPath);
            }

            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter(CharacterAnimationDriver.MoveSpeedParameter, AnimatorControllerParameterType.Float);
            controller.AddParameter(CharacterAnimationDriver.IsMovingParameter, AnimatorControllerParameterType.Bool);
            controller.AddParameter(CharacterAnimationDriver.IsDashingParameter, AnimatorControllerParameterType.Bool);
            controller.AddParameter(CharacterAnimationDriver.AttackTrigger, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(CharacterAnimationDriver.DashTrigger, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(CharacterAnimationDriver.HitTrigger, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(CharacterAnimationDriver.DeathTrigger, AnimatorControllerParameterType.Trigger);

            var stateMachine = controller.layers[0].stateMachine;
            var idle = AddState(stateMachine, "Idle", selection.Idle, new Vector3(260f, 70f, 0f));
            var move = AddState(stateMachine, "Move", selection.Move, new Vector3(520f, 70f, 0f));
            var attack = AddState(stateMachine, "Attack", selection.Attack, new Vector3(520f, 210f, 0f));
            var dash = AddState(stateMachine, "Dash", selection.Dash, new Vector3(260f, 210f, 0f));
            var hit = AddState(stateMachine, "Hit", selection.Hit, new Vector3(0f, 210f, 0f));
            var death = AddState(stateMachine, "Death", selection.Death, new Vector3(0f, 350f, 0f));

            stateMachine.defaultState = idle;
            AddBoolTransition(idle, move, CharacterAnimationDriver.IsMovingParameter, true, 0.08f);
            AddBoolTransition(move, idle, CharacterAnimationDriver.IsMovingParameter, false, 0.1f);
            AddAnyStateTriggerTransition(stateMachine, attack, CharacterAnimationDriver.AttackTrigger, 0.05f);
            AddTimedTransition(attack, idle, 0.75f, 0.05f);
            AddAnyStateTriggerTransition(stateMachine, dash, CharacterAnimationDriver.DashTrigger, 0.03f);
            AddTimedTransition(dash, idle, 0.65f, 0.05f);
            AddAnyStateBoolTransition(stateMachine, dash, CharacterAnimationDriver.IsDashingParameter, true, 0.03f);
            AddBoolTransition(dash, idle, CharacterAnimationDriver.IsDashingParameter, false, 0.08f);
            AddAnyStateTriggerTransition(stateMachine, hit, CharacterAnimationDriver.HitTrigger, 0.04f);
            AddTimedTransition(hit, idle, 0.35f, 0.05f);
            AddAnyStateTriggerTransition(stateMachine, death, CharacterAnimationDriver.DeathTrigger, 0.04f);

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static AnimatorState AddState(AnimatorStateMachine stateMachine, string name, AnimationClip clip, Vector3 position)
        {
            var state = stateMachine.AddState(name, position);
            state.motion = clip;
            state.speed = 1f;
            return state;
        }

        private static void AddBoolTransition(
            AnimatorState from,
            AnimatorState to,
            string parameterName,
            bool expectedValue,
            float duration)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = duration;
            transition.AddCondition(expectedValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, parameterName);
        }

        private static void AddAnyStateBoolTransition(
            AnimatorStateMachine stateMachine,
            AnimatorState to,
            string parameterName,
            bool expectedValue,
            float duration)
        {
            var transition = stateMachine.AddAnyStateTransition(to);
            transition.canTransitionToSelf = false;
            transition.hasExitTime = false;
            transition.duration = duration;
            transition.AddCondition(expectedValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, parameterName);
        }

        private static void AddAnyStateTriggerTransition(
            AnimatorStateMachine stateMachine,
            AnimatorState to,
            string triggerName,
            float duration)
        {
            var transition = stateMachine.AddAnyStateTransition(to);
            transition.canTransitionToSelf = false;
            transition.hasExitTime = false;
            transition.duration = duration;
            transition.AddCondition(AnimatorConditionMode.If, 0f, triggerName);
        }

        private static void AddTimedTransition(AnimatorState from, AnimatorState to, float exitTime, float duration)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = true;
            transition.exitTime = exitTime;
            transition.duration = duration;
        }

        private static bool ApplyControllerToGeneratedPrefab(CharacterEnemyAssetSpec spec, RuntimeAnimatorController controller, bool isPlayer)
        {
            var prefabPath = spec.GeneratedPrefabPath;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null || controller == null)
            {
                return false;
            }

            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var animator = ResolveAnimator(root);
                var resolvedAvatar = ResolveAvatar(spec.VisualAssetPath);
                if (resolvedAvatar != null)
                {
                    animator.avatar = resolvedAvatar;
                }

                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.updateMode = AnimatorUpdateMode.Normal;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                EditorUtility.SetDirty(animator);

                ConfigureAnimationDriver(root, animator, isPlayer);

                var saveSuccess = false;
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out saveSuccess);
                return saveSuccess;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ConfigureAnimationDriver(GameObject root, Animator animator, bool isPlayer)
        {
            RemoveChildAnimationDrivers(root);

            var driver = root.GetComponent<CharacterAnimationDriver>() ?? root.AddComponent<CharacterAnimationDriver>();
            var serializedDriver = new SerializedObject(driver);
            SetObjectReference(serializedDriver, "animator", animator);
            serializedDriver.FindProperty("isPlayer").boolValue = isPlayer;

            if (isPlayer)
            {
                SetObjectReference(serializedDriver, "playerMovement", root.GetComponent<PlayerMovementController>());
                SetObjectReference(serializedDriver, "playerAttack", root.GetComponent<PlayerAttackController>());
                SetObjectReference(serializedDriver, "playerDash", root.GetComponent<PlayerDashController>());
                SetObjectReference(serializedDriver, "playerHealth", root.GetComponent<PlayerHealth>());
                SetObjectReference(serializedDriver, "enemyMovement", null);
                SetObjectReference(serializedDriver, "enemyAttack", null);
                SetObjectReference(serializedDriver, "enemyHealth", null);
                SetObjectReference(serializedDriver, "enemyKnockbackReceiver", null);
            }
            else
            {
                SetObjectReference(serializedDriver, "playerMovement", null);
                SetObjectReference(serializedDriver, "playerAttack", null);
                SetObjectReference(serializedDriver, "playerDash", null);
                SetObjectReference(serializedDriver, "playerHealth", null);
                SetObjectReference(serializedDriver, "enemyMovement", root.GetComponent<EnemyMovement>());
                SetObjectReference(serializedDriver, "enemyAttack", root.GetComponent<EnemyAttackController>());
                SetObjectReference(serializedDriver, "enemyHealth", root.GetComponent<EnemyHealth>());
                SetObjectReference(serializedDriver, "enemyKnockbackReceiver", root.GetComponent<KnockbackReceiver>());
            }

            serializedDriver.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(driver);
        }

        private static void RemoveChildAnimationDrivers(GameObject root)
        {
            var drivers = root.GetComponentsInChildren<CharacterAnimationDriver>(true);
            for (var i = 0; i < drivers.Length; i++)
            {
                var driver = drivers[i];
                if (driver == null || driver.gameObject == root)
                {
                    continue;
                }

                UnityEngine.Object.DestroyImmediate(driver);
            }
        }

        private static void SetObjectReference(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        private static Animator ResolveAnimator(GameObject root)
        {
            var visualRoot = FindChild(root.transform, "VisualRoot");
            if (visualRoot != null)
            {
                var visualRootAnimator = visualRoot.GetComponent<Animator>();
                var animators = visualRoot.GetComponentsInChildren<Animator>(true);
                for (var i = 0; i < animators.Length; i++)
                {
                    if (animators[i] != null && animators[i].transform != visualRoot)
                    {
                        if (visualRootAnimator != null)
                        {
                            UnityEngine.Object.DestroyImmediate(visualRootAnimator);
                        }

                        return animators[i];
                    }
                }

                if (visualRootAnimator != null)
                {
                    return visualRootAnimator;
                }

                var target = visualRoot.childCount > 0 ? visualRoot.GetChild(0) : visualRoot;
                return target.gameObject.AddComponent<Animator>();
            }

            var animator = root.GetComponentInChildren<Animator>(true);
            return animator != null ? animator : root.AddComponent<Animator>();
        }

        private static Avatar ResolveAvatar(string visualAssetPath)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(visualAssetPath);
            for (var i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Avatar avatar)
                {
                    return avatar;
                }
            }

            return null;
        }

        private static AnimationClip FindClipAtPath(string assetPath, params string[] names)
        {
            var representedAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
            var representedClip = FindClipInAssets(representedAssets, names, includePreviewClips: false);
            if (representedClip != null)
            {
                return representedClip;
            }

            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            return FindClipInAssets(assets, names, includePreviewClips: false);
        }

        private static AnimationClip FindPreviewClipAtPath(string assetPath, params string[] names)
        {
            var representedAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
            var representedClip = FindClipInAssets(representedAssets, names, includePreviewClips: true);
            if (representedClip != null)
            {
                return representedClip;
            }

            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            return FindClipInAssets(assets, names, includePreviewClips: true);
        }

        private static AnimationClip FindClipInAssets(UnityEngine.Object[] assets, string[] names, bool includePreviewClips)
        {
            for (var nameIndex = 0; nameIndex < names.Length; nameIndex++)
            {
                for (var assetIndex = 0; assetIndex < assets.Length; assetIndex++)
                {
                    if (assets[assetIndex] is AnimationClip clip &&
                        (includePreviewClips || !IsPreviewClip(clip)) &&
                        ClipNameMatches(clip.name, names[nameIndex]))
                    {
                        return clip;
                    }
                }
            }

            return null;
        }

        private static bool IsPreviewClip(AnimationClip clip)
        {
            return clip == null ||
                clip.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ClipNameMatches(string clipName, string expectedName)
        {
            if (clipName.Equals(expectedName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var clipLeafName = GetClipLeafName(clipName);
            var expectedLeafName = GetClipLeafName(expectedName);
            if (clipLeafName.Equals(expectedLeafName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return expectedLeafName.StartsWith("AC", StringComparison.OrdinalIgnoreCase)
                ? clipLeafName.Equals(expectedLeafName.Substring(2), StringComparison.OrdinalIgnoreCase)
                : clipLeafName.Equals($"AC{expectedLeafName}", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetClipLeafName(string clipName)
        {
            var separatorIndex = clipName.LastIndexOf('|');
            return separatorIndex >= 0 ? clipName.Substring(separatorIndex + 1) : clipName;
        }

        private static void PrepareAnimationImportSettings(StringBuilder report)
        {
            var changed = EnsureGenericModelImporter(CharacterEnemyAssetSelection.Player.VisualAssetPath, report);
            for (var i = 0; i < CharacterEnemyAssetSelection.Enemies.Length; i++)
            {
                changed |= EnsureGenericModelImporter(CharacterEnemyAssetSelection.Enemies[i].VisualAssetPath, report);
            }

            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private static bool EnsureGenericModelImporter(string assetPath, StringBuilder report)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer == null)
            {
                report.AppendLine($"- Skipped importer preparation: `{assetPath}` is not a model importer asset.");
                return false;
            }

            var changed = false;
            if (importer.animationType != ModelImporterAnimationType.Generic)
            {
                importer.animationType = ModelImporterAnimationType.Generic;
                changed = true;
            }

            if (importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
            {
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                changed = true;
            }

            if (!importer.importAnimation)
            {
                importer.importAnimation = true;
                changed = true;
            }

            if (importer.optimizeGameObjects)
            {
                importer.optimizeGameObjects = false;
                changed = true;
            }

            if (!changed)
            {
                report.AppendLine($"- Importer already ready for Generic Avatar clips: `{assetPath}`");
                return false;
            }

            importer.SaveAndReimport();
            report.AppendLine($"- Set Generic Avatar animation import settings: `{assetPath}`");
            return true;
        }

        private static Transform FindChild(Transform root, string childName)
        {
            var children = root.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < children.Length; i++)
            {
                if (children[i].name == childName)
                {
                    return children[i];
                }
            }

            return null;
        }

        private static void AppendClip(StringBuilder report, string label, AnimationClip clip)
        {
            if (clip == null)
            {
                report.AppendLine($"- {label}: missing");
                return;
            }

            report.AppendLine($"- {label}: `{clip.name}` from `{AssetDatabase.GetAssetPath(clip)}`");
        }

        private static void EnsureFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
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

        private enum ClipRole
        {
            Idle,
            Move,
            Attack,
            Dash,
            Hit,
            Death
        }

        private sealed class ClipSelection
        {
            public AnimationClip Idle;
            public AnimationClip Move;
            public AnimationClip Attack;
            public AnimationClip Dash;
            public AnimationClip Hit;
            public AnimationClip Death;
        }

        private sealed class AnimationClipLibrary
        {
            private readonly List<ClipCandidate> candidates = new List<ClipCandidate>();

            public int Count => candidates.Count;

            public static AnimationClipLibrary Build(string[] scanRoots)
            {
                var library = new AnimationClipLibrary();
                var seen = new HashSet<string>(StringComparer.Ordinal);
                var guids = AssetDatabase.FindAssets("t:AnimationClip", scanRoots);
                for (var i = 0; i < guids.Length; i++)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    library.AddClipsAtPath(path, seen);
                }

                return library;
            }

            public AnimationClip SelectBest(CharacterEnemyAssetSpec spec, ClipRole role)
            {
                ClipCandidate best = null;
                var bestScore = 0;
                for (var i = 0; i < candidates.Count; i++)
                {
                    var candidate = candidates[i];
                    var score = Score(candidate, spec, role);
                    if (score <= bestScore)
                    {
                        continue;
                    }

                    best = candidate;
                    bestScore = score;
                }

                return best != null ? best.Clip : null;
            }

            private void AddClipsAtPath(string path, HashSet<string> seen)
            {
                AddClip(AssetDatabase.LoadAssetAtPath<AnimationClip>(path), path, seen);

                var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
                for (var i = 0; i < assets.Length; i++)
                {
                    AddClip(assets[i] as AnimationClip, path, seen);
                }
            }

            private void AddClip(AnimationClip clip, string path, HashSet<string> seen)
            {
                if (clip == null || string.IsNullOrWhiteSpace(clip.name))
                {
                    return;
                }

                if (clip.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                var key = $"{path}::{clip.name}";
                if (!seen.Add(key))
                {
                    return;
                }

                candidates.Add(new ClipCandidate(clip, path));
            }

            private static int Score(ClipCandidate candidate, CharacterEnemyAssetSpec spec, ClipRole role)
            {
                var score = ScoreRoleKeywords(candidate.SearchText, role, spec.RoleId);
                var isSelectedVisualSource = candidate.Path.Equals(spec.VisualAssetPath, StringComparison.OrdinalIgnoreCase);
                if (score <= 0 && isSelectedVisualSource)
                {
                    score = 8;
                }

                if (score <= 0)
                {
                    return 0;
                }

                if (isSelectedVisualSource)
                {
                    score += 70;
                }

                if (spec.RoleId == CharacterEnemyRoleId.MainPlayer)
                {
                    score += ScorePlayerRangerPreference(candidate.SearchText, role);
                }
                else if (spec.RoleId == CharacterEnemyRoleId.BasicMelee)
                {
                    score += ScoreBasicMeleeGreenDemonPreference(candidate.SearchText, role);
                }

                if (candidate.SearchText.Contains("kaykit_character_animations"))
                {
                    score += IsHumanoidRole(spec.RoleId) ? 24 : 6;
                }

                if (candidate.SearchText.Contains("rig_medium") && spec.RoleId != CharacterEnemyRoleId.Tank && spec.RoleId != CharacterEnemyRoleId.BossCandidate)
                {
                    score += 12;
                }

                if (candidate.SearchText.Contains("rig_large") && (spec.RoleId == CharacterEnemyRoleId.Tank || spec.RoleId == CharacterEnemyRoleId.BossCandidate))
                {
                    score += 12;
                }

                if (role == ClipRole.Attack && candidate.SearchText.Contains("combatranged") && IsRangedRole(spec.RoleId))
                {
                    score += 22;
                }

                if (role == ClipRole.Attack && candidate.SearchText.Contains("combatmelee") && !IsRangedRole(spec.RoleId))
                {
                    score += 18;
                }

                if ((role == ClipRole.Idle || role == ClipRole.Death) && candidate.SearchText.Contains("general"))
                {
                    score += 8;
                }

                if (role == ClipRole.Move && candidate.SearchText.Contains("movement"))
                {
                    score += 12;
                }

                return score;
            }

            private static int ScorePlayerRangerPreference(string text, ClipRole role)
            {
                switch (role)
                {
                    case ClipRole.Idle:
                        return text.Contains("idle_weapon") ? 28 : text.Contains("idle_attacking") ? 16 : 0;
                    case ClipRole.Move:
                        return text.Contains("run_holding") ? 26 : text.Contains("run") ? 10 : 0;
                    case ClipRole.Attack:
                        if (text.Contains("bow_attack_shoot"))
                        {
                            return 52;
                        }

                        if (text.Contains("bow_attack_draw"))
                        {
                            return 34;
                        }

                        return text.Contains("bow") || text.Contains("shoot") ? 18 : 0;
                    case ClipRole.Dash:
                        return text.Contains("roll") ? 24 : 0;
                    case ClipRole.Hit:
                        return text.Contains("recievehit_attacking") ? 18 : text.Contains("recievehit") ? 12 : 0;
                    default:
                        return 0;
                }
            }

            private static int ScoreBasicMeleeGreenDemonPreference(string text, ClipRole role)
            {
                if (!text.Contains("greendemon"))
                {
                    return 0;
                }

                switch (role)
                {
                    case ClipRole.Idle:
                        return text.Contains("idle") ? 36 : 0;
                    case ClipRole.Move:
                        return text.Contains("walk") ? 36 : 0;
                    case ClipRole.Attack:
                        return text.Contains("bite_front") ? 54 : text.Contains("bite_inplace") ? 46 : 0;
                    case ClipRole.Dash:
                        return text.Contains("jump") ? 28 : 0;
                    case ClipRole.Hit:
                        return text.Contains("hitrecieve") ? 40 : 0;
                    case ClipRole.Death:
                        return text.Contains("death") ? 42 : 0;
                    default:
                        return 0;
                }
            }

            private static int ScoreRoleKeywords(string text, ClipRole role, CharacterEnemyRoleId roleId)
            {
                switch (role)
                {
                    case ClipRole.Idle:
                        return Any(text, "idle", "stand", "breath", "rest") ? 40 : 0;
                    case ClipRole.Move:
                        var moveScore = Any(text, "run", "walk", "move", "locomotion", "jog") ? 36 : 0;
                        if (moveScore > 0 && (roleId == CharacterEnemyRoleId.FastMelee || roleId == CharacterEnemyRoleId.Charger) && text.Contains("run"))
                        {
                            moveScore += 16;
                        }

                        if (moveScore > 0 && (roleId == CharacterEnemyRoleId.Tank || roleId == CharacterEnemyRoleId.BossCandidate) && text.Contains("walk"))
                        {
                            moveScore += 14;
                        }

                        return moveScore;
                    case ClipRole.Attack:
                        var attackScore = Any(text, "attack", "slash", "strike", "punch", "hit", "cast", "spell", "shoot", "bow", "ranged") ? 38 : 0;
                        if (attackScore > 0 && IsRangedRole(roleId) && Any(text, "shoot", "bow", "ranged", "cast", "spell"))
                        {
                            attackScore += 20;
                        }

                        return attackScore;
                    case ClipRole.Dash:
                        return Any(text, "dash", "roll", "dodge", "sprint", "jump") ? 34 : 0;
                    case ClipRole.Hit:
                        return Any(text, "hurt", "damage", "hit", "impact", "stagger") ? 32 : 0;
                    case ClipRole.Death:
                        return Any(text, "death", "die", "dead", "defeat", "knockdown") ? 42 : 0;
                    default:
                        return 0;
                }
            }

            private static bool Any(string text, params string[] needles)
            {
                for (var i = 0; i < needles.Length; i++)
                {
                    if (text.Contains(needles[i]))
                    {
                        return true;
                    }
                }

                return false;
            }

            private static bool IsHumanoidRole(CharacterEnemyRoleId roleId)
            {
                return roleId == CharacterEnemyRoleId.MainPlayer
                    || roleId == CharacterEnemyRoleId.Ranged
                    || roleId == CharacterEnemyRoleId.Caster;
            }

            private static bool IsRangedRole(CharacterEnemyRoleId roleId)
            {
                return roleId == CharacterEnemyRoleId.Ranged
                    || roleId == CharacterEnemyRoleId.Caster;
            }
        }

        private sealed class ClipCandidate
        {
            public ClipCandidate(AnimationClip clip, string path)
            {
                Clip = clip;
                Path = path;
                SearchText = $"{path}/{clip.name}".ToLowerInvariant();
            }

            public AnimationClip Clip { get; }
            public string Path { get; }
            public string SearchText { get; }
        }
    }
}
