using NUnit.Framework;
using TapKnockout.Characters;
using TapKnockout.Enemy;
using TapKnockout.Player;
using UnityEngine;

namespace TapKnockout.Characters.Tests
{
    public sealed class CharacterEnemyPrefabValidationTests
    {
        [Test]
        public void ValidateEnemy_DetectsMissingAnimatorColliderHealthAndAi()
        {
            var prefab = new GameObject("EnemyCandidate");
            try
            {
                var result = CharacterEnemyPrefabValidation.ValidateEnemy(prefab);

                Assert.That(result.HasIssue(CharacterEnemyPrefabValidation.MissingAnimator), Is.True);
                Assert.That(result.HasIssue(CharacterEnemyPrefabValidation.MissingCollider), Is.True);
                Assert.That(result.HasIssue(CharacterEnemyPrefabValidation.MissingRigidbody), Is.True);
                Assert.That(result.HasIssue(CharacterEnemyPrefabValidation.MissingEnemyController), Is.True);
                Assert.That(result.HasIssue(CharacterEnemyPrefabValidation.MissingEnemyHealth), Is.True);
                Assert.That(result.HasIssue(CharacterEnemyPrefabValidation.MissingEnemyMovement), Is.True);
                Assert.That(result.HasIssue(CharacterEnemyPrefabValidation.MissingEnemyAttack), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void ValidateEnemy_RangedRequiresProjectileSpawnPoint()
        {
            var prefab = CreateMinimalEnemy("RangedCandidate");
            try
            {
                var result = CharacterEnemyPrefabValidation.ValidateEnemy(prefab, requiresProjectileSpawnPoint: true);
                Assert.That(result.HasIssue(CharacterEnemyPrefabValidation.MissingProjectileSpawnPoint), Is.True);

                var socket = new GameObject("ProjectileSpawnPoint");
                socket.transform.SetParent(prefab.transform, false);

                result = CharacterEnemyPrefabValidation.ValidateEnemy(prefab, requiresProjectileSpawnPoint: true);
                Assert.That(result.HasIssue(CharacterEnemyPrefabValidation.MissingProjectileSpawnPoint), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void ValidatePlayer_DetectsMissingPlayerControllers()
        {
            var prefab = new GameObject("PlayerCandidate");
            try
            {
                var result = CharacterEnemyPrefabValidation.ValidatePlayer(prefab);

                Assert.That(result.HasIssue(CharacterEnemyPrefabValidation.MissingPlayerMovement), Is.True);
                Assert.That(result.HasIssue(CharacterEnemyPrefabValidation.MissingPlayerAttack), Is.True);
                Assert.That(result.HasIssue(CharacterEnemyPrefabValidation.MissingPlayerDash), Is.True);
                Assert.That(result.HasIssue(CharacterEnemyPrefabValidation.MissingPlayerHealth), Is.True);
                Assert.That(result.HasIssue(CharacterEnemyPrefabValidation.MissingPlayerRuntimeStats), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void ValidatePlayer_DetectsAnimatorWithoutController()
        {
            var prefab = CreateMinimalPlayer("PlayerCandidate");
            try
            {
                var result = CharacterEnemyPrefabValidation.ValidatePlayer(prefab);

                Assert.That(result.HasIssue(CharacterEnemyPrefabValidation.MissingAnimator), Is.False);
                Assert.That(result.HasIssue(CharacterEnemyPrefabValidation.MissingAnimatorController), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void GeneratedPrefabPath_AllowsOnlyProjectOwnedPlayerAndEnemyFolders()
        {
            Assert.That(CharacterEnemyPrefabValidation.IsSafeGeneratedPrefabPath("Assets/_Project/Prefabs/Player/PF_Player_Test.prefab"), Is.True);
            Assert.That(CharacterEnemyPrefabValidation.IsSafeGeneratedPrefabPath("Assets/_Project/Prefabs/Enemies/Generated/PF_Enemy_Test.prefab"), Is.True);
            Assert.That(CharacterEnemyPrefabValidation.IsSafeGeneratedPrefabPath("Assets/ThirdParty/Characters/PF_Enemy_Test.prefab"), Is.False);
            Assert.That(CharacterEnemyPrefabValidation.IsSafeGeneratedPrefabPath("Assets/Assets/game asset packs/PF_Enemy_Test.prefab"), Is.False);
        }

        private static GameObject CreateMinimalEnemy(string name)
        {
            var prefab = new GameObject(name);
            prefab.AddComponent<Animator>();
            prefab.AddComponent<CapsuleCollider>();
            prefab.AddComponent<Rigidbody>();
            prefab.AddComponent<EnemyController>();
            prefab.AddComponent<EnemyHealth>();
            prefab.AddComponent<EnemyMovement>();
            prefab.AddComponent<EnemyAttackController>();

            var visualRoot = new GameObject("VisualRoot");
            visualRoot.transform.SetParent(prefab.transform, false);
            return prefab;
        }

        private static GameObject CreateMinimalPlayer(string name)
        {
            var prefab = new GameObject(name);
            prefab.AddComponent<Animator>();
            prefab.AddComponent<CapsuleCollider>();
            prefab.AddComponent<Rigidbody>();
            prefab.AddComponent<PlayerRuntimeStats>();
            prefab.AddComponent<PlayerMovementController>();
            prefab.AddComponent<PlayerAttackController>();
            prefab.AddComponent<PlayerDashController>();
            prefab.AddComponent<PlayerHealth>();

            var visualRoot = new GameObject("VisualRoot");
            visualRoot.transform.SetParent(prefab.transform, false);
            return prefab;
        }
    }
}
