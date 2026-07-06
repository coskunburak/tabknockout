using System.IO;
using System.Text;
using TapKnockout.Characters;
using TapKnockout.Enemy;
using TapKnockout.Feedback;
using TapKnockout.Player;
using TapKnockout.Survivor;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Editor.Tools
{
    public static class CharacterEnemyPrefabBuilder
    {
        private const string BasicMeleeEnemyConfigPath = "Assets/_Project/ScriptableObjects/Enemies/Generated/EnemyConfig_BasicMelee_GreenDemon.asset";
        private const string PlayerRangerBodyMaterialPath = "Assets/_Project/Art/Characters/MAT_Player_Ranger_Body.mat";
        private const string PlayerRangerBowMaterialPath = "Assets/_Project/Art/Characters/MAT_Player_Ranger_Bow.mat";
        private const string EnemyGreenDemonBodyMaterialPath = "Assets/_Project/Art/Characters/MAT_Enemy_GreenDemon_Body.mat";

        [MenuItem("Tools/Tap Knockout/Characters/Build Selected Character Enemy Prefabs")]
        public static void BuildSelectedCharacterEnemyPrefabs()
        {
            var report = new StringBuilder();
            report.AppendLine("# Character Enemy Prefab Builder Report");
            report.AppendLine();
            report.AppendLine("Generated prefabs are project-owned variants. ThirdParty source assets are not modified.");
            report.AppendLine();

            BuildSpec(CharacterEnemyAssetSelection.Player, isPlayer: true, report);
            for (var i = 0; i < CharacterEnemyAssetSelection.Enemies.Length; i++)
            {
                BuildSpec(CharacterEnemyAssetSelection.Enemies[i], isPlayer: false, report);
            }

            WriteTextAsset(CharacterEnemyAssetSelection.BuilderReportPath, report.ToString());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(report.ToString());
        }

        [MenuItem("Tools/Tap Knockout/Characters/Rebuild Character Prefabs And Animation Controllers")]
        public static void RebuildCharacterPrefabsAndAnimationControllers()
        {
            BuildSelectedCharacterEnemyPrefabs();
            Debug.Log(CharacterEnemyAnimationControllerBuilder.BuildAndApplyAnimationControllersInternal());
        }

        private static void BuildSpec(CharacterEnemyAssetSpec spec, bool isPlayer, StringBuilder report)
        {
            report.AppendLine($"## {spec.DisplayName}");

            if (!CharacterEnemyPrefabValidation.TryValidateGeneratedPrefabPath(spec.GeneratedPrefabPath, out var pathIssue))
            {
                report.AppendLine($"- Skipped: {pathIssue.Message}");
                return;
            }

            var visualAsset = AssetDatabase.LoadAssetAtPath<GameObject>(spec.VisualAssetPath);

            if (visualAsset == null)
            {
                report.AppendLine($"- Skipped: missing visual asset `{spec.VisualAssetPath}`.");
                return;
            }

            EnsureFolderForAsset(spec.GeneratedPrefabPath);
            var root = CreateRootForSpec(spec, isPlayer, report, out var unloadPrefabContents);
            if (root == null)
            {
                return;
            }

            try
            {
                root.name = Path.GetFileNameWithoutExtension(spec.GeneratedPrefabPath);
                RemoveRootPlaceholderGeometry(root);
                var visualRoot = EnsureChild(root.transform, "VisualRoot");
                ClearChildren(visualRoot);
                var visualInstance = Object.Instantiate(visualAsset, visualRoot);
                visualInstance.name = Path.GetFileNameWithoutExtension(spec.VisualAssetPath);
                visualInstance.transform.localPosition = Vector3.zero;
                visualInstance.transform.localRotation = Quaternion.identity;
                visualInstance.transform.localScale = Vector3.one * Mathf.Max(0.01f, spec.VisualScale);

                var animator = ResolveAnimator(visualRoot);
                ConfigureAnimator(animator, spec);

                EnsureChild(root.transform, "HitVFXSocket").localPosition = new Vector3(0f, 0.9f, 0f);
                EnsureChild(root.transform, "DeathVFXSocket").localPosition = new Vector3(0f, 0.35f, 0f);

                if (!isPlayer)
                {
                    EnsureChild(root.transform, "AttackOrigin").localPosition = new Vector3(0f, 0.85f, 0.55f);
                    EnsureChild(root.transform, "HitReactionRoot").localPosition = new Vector3(0f, 0.8f, 0f);

                    if (spec.RequiresProjectileSpawnPoint)
                    {
                        EnsureChild(root.transform, "ProjectileSpawnPoint").localPosition = new Vector3(0f, 0.9f, 0.65f);
                    }

                    if (spec.RoleId == CharacterEnemyRoleId.Charger || spec.RoleId == CharacterEnemyRoleId.Caster || spec.RoleId == CharacterEnemyRoleId.BossCandidate)
                    {
                        EnsureChild(root.transform, "TelegraphRoot").localPosition = new Vector3(0f, 0.05f, 0.75f);
                    }
                }
                else
                {
                    EnsureChild(root.transform, "ProjectileSpawnPoint").localPosition = new Vector3(0f, 0.95f, 0.55f);
                    EnsureChild(root.transform, "TargetingOrigin").localPosition = new Vector3(0f, 0.85f, 0f);
                    EnsureChild(root.transform, "DashHitVolume").localPosition = new Vector3(0f, 0.45f, 0f);
                    WirePlayerSocketReferences(root);
                }

                AttachHeldWeapon(root, visualInstance.transform, spec, report);
                ApplyRoleMaterials(visualInstance, spec, report);
                if (!isPlayer)
                {
                    EnsureEnemyRuntimeContract(root);
                }

                EnsureAnimationDriver(root, animator, isPlayer);

                var saveSuccess = false;
                PrefabUtility.SaveAsPrefabAsset(root, spec.GeneratedPrefabPath, out saveSuccess);
                report.AppendLine(saveSuccess
                    ? $"- Generated: `{spec.GeneratedPrefabPath}`"
                    : $"- Save failed: `{spec.GeneratedPrefabPath}`");

                var generatedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(spec.GeneratedPrefabPath);
                var validation = isPlayer
                    ? CharacterEnemyPrefabValidation.ValidatePlayer(generatedPrefab)
                    : CharacterEnemyPrefabValidation.ValidateEnemy(generatedPrefab, spec.RequiresProjectileSpawnPoint);
                AppendValidation(report, validation);
            }
            finally
            {
                if (unloadPrefabContents)
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
                else
                {
                    Object.DestroyImmediate(root);
                }
            }
        }

        private static GameObject CreateRootForSpec(
            CharacterEnemyAssetSpec spec,
            bool isPlayer,
            StringBuilder report,
            out bool unloadPrefabContents)
        {
            unloadPrefabContents = false;

            if (!string.IsNullOrWhiteSpace(spec.BasePrefabPath))
            {
                var basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(spec.BasePrefabPath);
                if (basePrefab == null)
                {
                    report.AppendLine($"- Skipped: missing base prefab `{spec.BasePrefabPath}`.");
                    return null;
                }

                unloadPrefabContents = true;
                return PrefabUtility.LoadPrefabContents(spec.BasePrefabPath);
            }

            if (isPlayer)
            {
                report.AppendLine("- Skipped: player specs require a base prefab.");
                return null;
            }

            report.AppendLine("- Base prefab: none. Created project-owned enemy runtime skeleton with the existing enemy components.");
            return CreateEnemyRuntimeRoot(spec);
        }

        private static GameObject CreateEnemyRuntimeRoot(CharacterEnemyAssetSpec spec)
        {
            var root = new GameObject(Path.GetFileNameWithoutExtension(spec.GeneratedPrefabPath));
            var enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer >= 0)
            {
                root.layer = enemyLayer;
            }

            var capsule = root.AddComponent<CapsuleCollider>();
            capsule.radius = 0.5f;
            capsule.height = 2f;
            capsule.center = new Vector3(0f, 1f, 0f);
            capsule.direction = 1;

            var rigidbody = root.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            var controller = root.AddComponent<EnemyController>();
            var health = root.AddComponent<EnemyHealth>();
            var movement = root.AddComponent<EnemyMovement>();
            var knockbackReceiver = root.AddComponent<KnockbackReceiver>();
            var attackController = root.AddComponent<EnemyAttackController>();
            root.AddComponent<PooledEnemy>();
            root.AddComponent<HitFlashController>();

            var defaultConfig = AssetDatabase.LoadAssetAtPath<EnemyConfig>(BasicMeleeEnemyConfigPath);
            WireEnemyComponentReferences(root, controller, health, movement, knockbackReceiver, attackController, defaultConfig);
            ApplyEnemyLayer(root);
            return root;
        }

        private static void EnsureEnemyRuntimeContract(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            var controller = EnsureComponent<EnemyController>(root);
            var health = EnsureComponent<EnemyHealth>(root);
            var movement = EnsureComponent<EnemyMovement>(root);
            var knockbackReceiver = EnsureComponent<KnockbackReceiver>(root);
            var attackController = EnsureComponent<EnemyAttackController>(root);
            EnsureComponent<PooledEnemy>(root);
            EnsureComponent<HitFlashController>(root);

            var defaultConfig = AssetDatabase.LoadAssetAtPath<EnemyConfig>(BasicMeleeEnemyConfigPath);
            WireEnemyComponentReferences(root, controller, health, movement, knockbackReceiver, attackController, defaultConfig);
            ApplyEnemyLayer(root);
        }

        private static void WireEnemyComponentReferences(
            GameObject root,
            EnemyController controller,
            EnemyHealth health,
            EnemyMovement movement,
            KnockbackReceiver knockbackReceiver,
            EnemyAttackController attackController,
            EnemyConfig defaultConfig)
        {
            var serializedController = new SerializedObject(controller);
            serializedController.FindProperty("config").objectReferenceValue = defaultConfig;
            serializedController.FindProperty("health").objectReferenceValue = health;
            serializedController.FindProperty("movement").objectReferenceValue = movement;
            serializedController.FindProperty("knockbackReceiver").objectReferenceValue = knockbackReceiver;
            serializedController.FindProperty("attackController").objectReferenceValue = attackController;
            SetObjectReference(serializedController, "target", null);
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            var serializedHealth = new SerializedObject(health);
            serializedHealth.FindProperty("config").objectReferenceValue = defaultConfig;
            serializedHealth.FindProperty("targetTransform").objectReferenceValue = root.transform;
            serializedHealth.FindProperty("targetableWhenAlive").boolValue = true;
            serializedHealth.FindProperty("disableCollidersOnDeath").boolValue = true;
            serializedHealth.FindProperty("logHits").boolValue = true;
            serializedHealth.FindProperty("logDeath").boolValue = true;
            serializedHealth.ApplyModifiedPropertiesWithoutUndo();

            var serializedMovement = new SerializedObject(movement);
            serializedMovement.FindProperty("config").objectReferenceValue = defaultConfig;
            SetObjectReference(serializedMovement, "target", null);
            serializedMovement.ApplyModifiedPropertiesWithoutUndo();

            var serializedKnockback = new SerializedObject(knockbackReceiver);
            serializedKnockback.FindProperty("config").objectReferenceValue = defaultConfig;
            serializedKnockback.ApplyModifiedPropertiesWithoutUndo();

            var serializedAttack = new SerializedObject(attackController);
            serializedAttack.FindProperty("config").objectReferenceValue = defaultConfig;
            SetObjectReference(serializedAttack, "target", null);
            SetBool(serializedAttack, "autoDealContactDamage", true);
            SetBool(serializedAttack, "useTelegraphWindup", false);
            SetFloat(serializedAttack, "fallbackWindupDuration", defaultConfig != null ? defaultConfig.AttackWindup : 0.12f);
            SetFloat(serializedAttack, "fallbackCancelledRetryDelay", 0.18f);
            SetFloat(serializedAttack, "fallbackAttackRange", defaultConfig != null ? defaultConfig.AttackRange : 1.2f);
            SetFloat(serializedAttack, "fallbackAttackCooldown", defaultConfig != null ? defaultConfig.AttackCooldown : 1f);
            SetFloat(serializedAttack, "fallbackContactDamage", defaultConfig != null ? defaultConfig.ContactDamage : 8f);
            serializedAttack.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Animator ResolveAnimator(Transform visualRoot)
        {
            var visualRootAnimator = visualRoot.GetComponent<Animator>();
            var animators = visualRoot.GetComponentsInChildren<Animator>(true);
            for (var i = 0; i < animators.Length; i++)
            {
                if (animators[i] != null && animators[i].transform != visualRoot)
                {
                    if (visualRootAnimator != null)
                    {
                        Object.DestroyImmediate(visualRootAnimator);
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

        private static void ConfigureAnimator(Animator animator, CharacterEnemyAssetSpec spec)
        {
            var resolvedAvatar = ResolveAvatar(spec.VisualAssetPath);
            if (resolvedAvatar != null)
            {
                animator.avatar = resolvedAvatar;
            }

            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                CharacterEnemyAnimationControllerBuilder.GetControllerPath(spec.RoleId));
            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
            }

            animator.applyRootMotion = false;
            animator.updateMode = AnimatorUpdateMode.Normal;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }

        private static void AttachHeldWeapon(
            GameObject root,
            Transform visualInstance,
            CharacterEnemyAssetSpec spec,
            StringBuilder report)
        {
            if (!spec.HeldWeapon.HasAsset)
            {
                return;
            }

            var weaponAsset = AssetDatabase.LoadAssetAtPath<GameObject>(spec.HeldWeapon.AssetPath);
            if (weaponAsset == null)
            {
                report.AppendLine($"- Held weapon skipped: missing asset `{spec.HeldWeapon.AssetPath}`.");
                return;
            }

            var socket = FindChild(visualInstance, spec.HeldWeapon.SocketName)
                ?? FindChild(visualInstance, "Weapon.R")
                ?? FindChild(visualInstance, "LowerArm.R")
                ?? visualInstance;
            var attachmentRoot = EnsureChild(socket, "HeldWeapon");
            ClearChildren(attachmentRoot);

            var weaponInstance = Object.Instantiate(weaponAsset, attachmentRoot);
            weaponInstance.name = Path.GetFileNameWithoutExtension(spec.HeldWeapon.AssetPath);
            weaponInstance.transform.localPosition = spec.HeldWeapon.LocalPosition;
            weaponInstance.transform.localRotation = Quaternion.Euler(spec.HeldWeapon.LocalEulerAngles);
            weaponInstance.transform.localScale = spec.HeldWeapon.LocalScale;

            var projectileSpawnPoint = FindChild(root.transform, "ProjectileSpawnPoint");
            if (projectileSpawnPoint != null)
            {
                projectileSpawnPoint.SetParent(socket, false);
                projectileSpawnPoint.localPosition = new Vector3(0f, 0f, 0.65f);
                projectileSpawnPoint.localRotation = Quaternion.identity;
                projectileSpawnPoint.localScale = Vector3.one;
            }

            report.AppendLine($"- Held weapon attached: `{spec.HeldWeapon.AssetPath}` -> `{socket.name}`.");
        }

        private static void ApplyRoleMaterials(GameObject visualInstance, CharacterEnemyAssetSpec spec, StringBuilder report)
        {
            if (visualInstance == null)
            {
                return;
            }

            var bodyMaterial = ResolveBodyMaterial(spec);
            var bowMaterial = spec.RoleId == CharacterEnemyRoleId.MainPlayer
                ? AssetDatabase.LoadAssetAtPath<Material>(PlayerRangerBowMaterialPath)
                : null;

            if (bodyMaterial == null)
            {
                return;
            }

            var assignedRenderers = 0;
            var renderers = visualInstance.GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                var material = bowMaterial != null && renderer.name.Contains("Bow")
                    ? bowMaterial
                    : bodyMaterial;
                var materials = renderer.sharedMaterials;
                for (var materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    materials[materialIndex] = material;
                }

                renderer.sharedMaterials = materials;
                assignedRenderers++;
            }

            report.AppendLine($"- Materials assigned: {assignedRenderers} renderer(s).");
        }

        private static Material ResolveBodyMaterial(CharacterEnemyAssetSpec spec)
        {
            if (spec.RoleId == CharacterEnemyRoleId.MainPlayer)
            {
                return AssetDatabase.LoadAssetAtPath<Material>(PlayerRangerBodyMaterialPath);
            }

            return spec.RoleId == CharacterEnemyRoleId.BasicMelee
                ? AssetDatabase.LoadAssetAtPath<Material>(EnemyGreenDemonBodyMaterialPath)
                : null;
        }

        private static void EnsureAnimationDriver(GameObject root, Animator animator, bool isPlayer)
        {
            var driver = root.GetComponent<CharacterAnimationDriver>() ?? root.AddComponent<CharacterAnimationDriver>();
            var serializedObject = new SerializedObject(driver);
            serializedObject.FindProperty("animator").objectReferenceValue = animator;
            serializedObject.FindProperty("isPlayer").boolValue = isPlayer;

            if (isPlayer)
            {
                serializedObject.FindProperty("playerMovement").objectReferenceValue = root.GetComponent<PlayerMovementController>();
                serializedObject.FindProperty("playerAttack").objectReferenceValue = root.GetComponent<PlayerAttackController>();
                serializedObject.FindProperty("playerDash").objectReferenceValue = root.GetComponent<PlayerDashController>();
                serializedObject.FindProperty("playerHealth").objectReferenceValue = root.GetComponent<PlayerHealth>();
            }
            else
            {
                serializedObject.FindProperty("enemyMovement").objectReferenceValue = root.GetComponent<EnemyMovement>();
                serializedObject.FindProperty("enemyAttack").objectReferenceValue = root.GetComponent<EnemyAttackController>();
                serializedObject.FindProperty("enemyHealth").objectReferenceValue = root.GetComponent<EnemyHealth>();
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WirePlayerSocketReferences(GameObject root)
        {
            var projectileSpawnPoint = FindChild(root.transform, "ProjectileSpawnPoint");
            var dashHitVolume = FindChild(root.transform, "DashHitVolume");

            var attackController = root.GetComponent<PlayerAttackController>();
            if (attackController != null && projectileSpawnPoint != null)
            {
                var serializedObject = new SerializedObject(attackController);
                serializedObject.FindProperty("projectileSpawnPoint").objectReferenceValue = projectileSpawnPoint;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            var dashController = root.GetComponent<PlayerDashController>();
            if (dashController != null && dashHitVolume != null)
            {
                var serializedObject = new SerializedObject(dashController);
                serializedObject.FindProperty("hitQueryOrigin").objectReferenceValue = dashHitVolume;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void AppendValidation(StringBuilder report, CharacterEnemyPrefabValidationResult validation)
        {
            if (validation.IsValid)
            {
                report.AppendLine("- Validation: pass");
                return;
            }

            report.AppendLine("- Validation issues:");
            for (var i = 0; i < validation.Issues.Count; i++)
            {
                report.AppendLine($"  - {validation.Issues[i].Code}: {validation.Issues[i].Message}");
            }
        }

        private static Transform EnsureChild(Transform parent, string childName)
        {
            var existing = parent.Find(childName);
            if (existing != null)
            {
                return existing;
            }

            var child = new GameObject(childName);
            child.transform.SetParent(parent, false);
            return child.transform;
        }

        private static T EnsureComponent<T>(GameObject root) where T : Component
        {
            var component = root.GetComponent<T>();
            return component != null ? component : root.AddComponent<T>();
        }

        private static void ApplyEnemyLayer(GameObject root)
        {
            var enemyLayer = LayerMask.NameToLayer("Enemy");
            if (root == null || enemyLayer < 0)
            {
                return;
            }

            var transforms = root.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < transforms.Length; i++)
            {
                var candidate = transforms[i];
                if (candidate == null)
                {
                    continue;
                }

                if (candidate == root.transform || candidate.GetComponent<Collider>() != null)
                {
                    candidate.gameObject.layer = enemyLayer;
                }
            }
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

        private static void ClearChildren(Transform parent)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
            }
        }

        private static void RemoveRootPlaceholderGeometry(GameObject root)
        {
            if (root.TryGetComponent<MeshRenderer>(out var renderer))
            {
                Object.DestroyImmediate(renderer);
            }

            if (root.TryGetComponent<MeshFilter>(out var meshFilter))
            {
                Object.DestroyImmediate(meshFilter);
            }
        }

        private static void EnsureFolderForAsset(string assetPath)
        {
            var folderPath = Path.GetDirectoryName(assetPath);
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                return;
            }

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

        private static void SetObjectReference(SerializedObject serializedObject, string propertyName, Object value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.boolValue = value;
            }
        }

        private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.floatValue = value;
            }
        }
    }
}
