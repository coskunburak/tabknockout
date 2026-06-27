using NUnit.Framework;
using TapKnockout.Combat;
using TapKnockout.Enemy;
using UnityEngine;

namespace TapKnockout.Enemy.Tests
{
    public sealed class EnemyAttackTelegraphTests
    {
        [Test]
        public void TryBeginAttack_WithTelegraphConfig_StartsWindupWithoutImmediateCooldown()
        {
            var enemy = new GameObject("Enemy");
            var target = new GameObject("Target");
            var config = ScriptableObject.CreateInstance<EnemyAttackTelegraphConfig>();

            try
            {
                enemy.transform.position = Vector3.zero;
                target.transform.position = Vector3.forward;
                target.AddComponent<TestDamageable>();

                var controller = enemy.AddComponent<EnemyAttackController>();
                controller.SetTarget(target.transform);

                SetPrivateField(controller, "telegraphConfig", config);
                SetPrivateField(controller, "useTelegraphWindup", true);

                Assert.That(controller.TryBeginAttack(), Is.True);
                Assert.That(controller.IsWindingUp, Is.True);
                Assert.That(controller.CooldownRemaining, Is.EqualTo(0f).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(config);
                Object.DestroyImmediate(enemy);
                Object.DestroyImmediate(target);
            }
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            var field = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field.SetValue(instance, value);
        }

        private sealed class TestDamageable : MonoBehaviour, IDamageable
        {
            public bool IsAlive => true;
            public GameObject GameObject => gameObject;

            public void ReceiveHit(HitContext hitContext)
            {
            }
        }
    }
}
