using TapKnockout.Enemy;
using TapKnockout.Player;
using UnityEngine;

namespace TapKnockout.Characters
{
    public static class CharacterEnemyPrefabValidation
    {
        public const string MissingPrefab = "missing_prefab";
        public const string MissingAnimator = "missing_animator";
        public const string MissingAnimatorController = "missing_animator_controller";
        public const string MissingCollider = "missing_collider";
        public const string MissingRigidbody = "missing_rigidbody";
        public const string MissingVisualRoot = "missing_visual_root";
        public const string MissingProjectileSpawnPoint = "missing_projectile_spawn_point";
        public const string MissingPlayerMovement = "missing_player_movement";
        public const string MissingPlayerAttack = "missing_player_attack";
        public const string MissingPlayerDash = "missing_player_dash";
        public const string MissingPlayerHealth = "missing_player_health";
        public const string MissingPlayerRuntimeStats = "missing_player_runtime_stats";
        public const string MissingEnemyController = "missing_enemy_controller";
        public const string MissingEnemyHealth = "missing_enemy_health";
        public const string MissingEnemyMovement = "missing_enemy_movement";
        public const string MissingEnemyAttack = "missing_enemy_attack";
        public const string UnsafeGeneratedPath = "unsafe_generated_path";

        public static CharacterEnemyPrefabValidationResult ValidatePlayer(GameObject prefabRoot)
        {
            var result = ValidateCommon(prefabRoot, requireAnimatorController: true);
            if (prefabRoot == null)
            {
                return result;
            }

            RequireComponent<PlayerMovementController>(prefabRoot, result, MissingPlayerMovement, "Player prefab must keep PlayerMovementController.");
            RequireComponent<PlayerAttackController>(prefabRoot, result, MissingPlayerAttack, "Player prefab must keep PlayerAttackController.");
            RequireComponent<PlayerDashController>(prefabRoot, result, MissingPlayerDash, "Player prefab must keep PlayerDashController.");
            RequireComponent<PlayerHealth>(prefabRoot, result, MissingPlayerHealth, "Player prefab must keep PlayerHealth.");
            RequireComponent<PlayerRuntimeStats>(prefabRoot, result, MissingPlayerRuntimeStats, "Player prefab must keep PlayerRuntimeStats.");
            return result;
        }

        public static CharacterEnemyPrefabValidationResult ValidateEnemy(
            GameObject prefabRoot,
            bool requiresProjectileSpawnPoint = false,
            bool requireAnimatorController = false)
        {
            var result = ValidateCommon(prefabRoot, requireAnimatorController);
            if (prefabRoot == null)
            {
                return result;
            }

            RequireComponent<EnemyController>(prefabRoot, result, MissingEnemyController, "Enemy prefab must include EnemyController.");
            RequireComponent<EnemyHealth>(prefabRoot, result, MissingEnemyHealth, "Enemy prefab must include EnemyHealth.");
            RequireComponent<EnemyMovement>(prefabRoot, result, MissingEnemyMovement, "Enemy prefab must include EnemyMovement.");
            RequireComponent<EnemyAttackController>(prefabRoot, result, MissingEnemyAttack, "Enemy prefab must include EnemyAttackController.");

            if (requiresProjectileSpawnPoint && FindChild(prefabRoot.transform, "ProjectileSpawnPoint") == null)
            {
                result.Add(MissingProjectileSpawnPoint, "Ranged and caster prefabs need a ProjectileSpawnPoint socket.");
            }

            return result;
        }

        public static bool IsSafeGeneratedPrefabPath(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return false;
            }

            return assetPath.StartsWith("Assets/_Project/Prefabs/Player/", System.StringComparison.Ordinal)
                || assetPath.StartsWith("Assets/_Project/Prefabs/Enemies/", System.StringComparison.Ordinal);
        }

        public static bool TryValidateGeneratedPrefabPath(string assetPath, out CharacterEnemyPrefabValidationIssue issue)
        {
            if (IsSafeGeneratedPrefabPath(assetPath))
            {
                issue = default;
                return true;
            }

            issue = new CharacterEnemyPrefabValidationIssue(
                UnsafeGeneratedPath,
                "Generated character/enemy prefabs must be written under Assets/_Project/Prefabs/Player or Assets/_Project/Prefabs/Enemies.");
            return false;
        }

        private static CharacterEnemyPrefabValidationResult ValidateCommon(GameObject prefabRoot, bool requireAnimatorController)
        {
            var result = new CharacterEnemyPrefabValidationResult();
            if (prefabRoot == null)
            {
                result.Add(MissingPrefab, "Prefab root is missing.");
                return result;
            }

            if (prefabRoot.GetComponentInChildren<Animator>(true) == null)
            {
                result.Add(MissingAnimator, "Prefab needs an Animator on the root or visual child.");
            }
            else if (requireAnimatorController)
            {
                var animator = prefabRoot.GetComponentInChildren<Animator>(true);
                if (animator.runtimeAnimatorController == null)
                {
                    result.Add(MissingAnimatorController, "Animator Controller is not assigned.");
                }
            }

            if (prefabRoot.GetComponentInChildren<Collider>(true) == null)
            {
                result.Add(MissingCollider, "Prefab needs at least one Collider.");
            }

            if (prefabRoot.GetComponentInChildren<Rigidbody>(true) == null)
            {
                result.Add(MissingRigidbody, "Prefab needs a Rigidbody for the current 3D gameplay controllers.");
            }

            if (FindChild(prefabRoot.transform, "VisualRoot") == null)
            {
                result.Add(MissingVisualRoot, "Prefab should contain a VisualRoot child for model swapping.");
            }

            return result;
        }

        private static void RequireComponent<T>(GameObject root, CharacterEnemyPrefabValidationResult result, string code, string message)
            where T : Component
        {
            if (root.GetComponentInChildren<T>(true) == null)
            {
                result.Add(code, message);
            }
        }

        private static Transform FindChild(Transform root, string childName)
        {
            if (root == null || string.IsNullOrWhiteSpace(childName))
            {
                return null;
            }

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
    }
}
