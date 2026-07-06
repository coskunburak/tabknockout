using System.Reflection;
using NUnit.Framework;
using TapKnockout.Characters;
using TapKnockout.Enemy;
using TapKnockout.Player;
using UnityEngine;

namespace TapKnockout.Characters.Tests
{
    public sealed class CharacterAnimationDriverTests
    {
        [Test]
        public void RefreshAnimationState_WithMissingAnimator_DoesNotThrow()
        {
            var gameObject = new GameObject("Driver");
            try
            {
                var driver = gameObject.AddComponent<CharacterAnimationDriver>();
                driver.SetAnimator(null);

                Assert.DoesNotThrow(() => driver.RefreshAnimationState(0.016f));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void SetAnimator_StoresAnimatorAndRole()
        {
            var gameObject = new GameObject("Driver");
            var animatorObject = new GameObject("Animator");
            try
            {
                animatorObject.transform.SetParent(gameObject.transform, false);
                var animator = animatorObject.AddComponent<Animator>();
                var driver = gameObject.AddComponent<CharacterAnimationDriver>();

                driver.SetAnimator(animator);
                driver.SetIsPlayer(true);

                Assert.That(driver.Animator, Is.SameAs(animator));
                Assert.That(driver.IsPlayer, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void ResolveReferences_OnVisualChild_UsesParentPlayerComponents()
        {
            var playerObject = new GameObject("Player");
            var visualObject = new GameObject("Ranger");
            try
            {
                visualObject.transform.SetParent(playerObject.transform, false);
                playerObject.AddComponent<Rigidbody>();
                playerObject.AddComponent<PlayerMovementController>();

                var driver = visualObject.AddComponent<CharacterAnimationDriver>();
                driver.ResolveReferences();

                Assert.That(driver.IsPlayer, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(playerObject);
            }
        }

        [Test]
        public void ResolveReferences_OnRoot_FindsChildAnimator()
        {
            var playerObject = new GameObject("Player");
            var visualObject = new GameObject("Ranger");
            try
            {
                visualObject.transform.SetParent(playerObject.transform, false);
                var animator = visualObject.AddComponent<Animator>();

                var driver = playerObject.AddComponent<CharacterAnimationDriver>();
                driver.ResolveReferences();

                Assert.That(driver.Animator, Is.SameAs(animator));
            }
            finally
            {
                Object.DestroyImmediate(playerObject);
            }
        }

        [Test]
        public void ResolveReferences_OnEnemyRoot_FindsEnemyRuntimeReferences()
        {
            var enemyObject = new GameObject("Enemy");
            var visualObject = new GameObject("GreenDemon");
            try
            {
                visualObject.transform.SetParent(enemyObject.transform, false);
                var animator = visualObject.AddComponent<Animator>();
                enemyObject.AddComponent<Rigidbody>();
                var health = enemyObject.AddComponent<EnemyHealth>();
                var movement = enemyObject.AddComponent<EnemyMovement>();
                var attack = enemyObject.AddComponent<EnemyAttackController>();
                var knockback = enemyObject.AddComponent<KnockbackReceiver>();

                var driver = enemyObject.AddComponent<CharacterAnimationDriver>();
                driver.SetIsPlayer(false);
                driver.ResolveReferences();

                Assert.That(driver.IsPlayer, Is.False);
                Assert.That(driver.Animator, Is.SameAs(animator));
                Assert.That(GetPrivateComponent<EnemyMovement>(driver, "enemyMovement"), Is.SameAs(movement));
                Assert.That(GetPrivateComponent<EnemyAttackController>(driver, "enemyAttack"), Is.SameAs(attack));
                Assert.That(GetPrivateComponent<EnemyHealth>(driver, "enemyHealth"), Is.SameAs(health));
                Assert.That(GetPrivateComponent<KnockbackReceiver>(driver, "enemyKnockbackReceiver"), Is.SameAs(knockback));
            }
            finally
            {
                Object.DestroyImmediate(enemyObject);
            }
        }

        private static T GetPrivateComponent<T>(CharacterAnimationDriver driver, string fieldName) where T : Component
        {
            var field = typeof(CharacterAnimationDriver).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            return field != null ? field.GetValue(driver) as T : null;
        }
    }
}
