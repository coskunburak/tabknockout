using NUnit.Framework;
using TapKnockout.Combat;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Combat.Tests
{
    public sealed class ImpactFeedbackControllerTests
    {
        [Test]
        public void TryTriggerFeedback_WithDashHit_TriggersWithoutSceneReferences()
        {
            var controllerObject = new GameObject("ImpactFeedback");
            var source = new GameObject("Player");
            var target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var config = CreateQuietConfig();

            try
            {
                var controller = controllerObject.AddComponent<ImpactFeedbackController>();
                SetConfig(controller, config);
                var triggered = false;
                controller.OnImpactFeedbackTriggered += _ => triggered = true;

                var hitContext = new HitContext(source, target, 12f, DamageType.Impact)
                {
                    IsDashHit = true,
                    HitPoint = target.transform.position,
                    HitDirection = Vector3.forward
                };

                Assert.That(controller.TryTriggerFeedback(hitContext), Is.True);
                Assert.That(triggered, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(config);
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(source);
                Object.DestroyImmediate(controllerObject);
            }
        }

        [Test]
        public void TryTriggerFeedback_WithNonDashHit_DoesNotTrigger()
        {
            var controllerObject = new GameObject("ImpactFeedback");

            try
            {
                var controller = controllerObject.AddComponent<ImpactFeedbackController>();
                var triggered = false;
                controller.OnImpactFeedbackTriggered += _ => triggered = true;

                var hitContext = new HitContext(null, null, 1f)
                {
                    IsDashHit = false
                };

                Assert.That(controller.TryTriggerFeedback(hitContext), Is.False);
                Assert.That(triggered, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(controllerObject);
            }
        }

        private static ImpactFeedbackConfig CreateQuietConfig()
        {
            var config = ScriptableObject.CreateInstance<ImpactFeedbackConfig>();
            var serializedObject = new SerializedObject(config);
            serializedObject.FindProperty("dashHitPauseDuration").floatValue = 0f;
            serializedObject.FindProperty("hitFlashDuration").floatValue = 0f;
            serializedObject.FindProperty("dashCameraShakeDuration").floatValue = 0f;
            serializedObject.FindProperty("dashCameraShakeMagnitude").floatValue = 0f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }

        private static void SetConfig(ImpactFeedbackController controller, ImpactFeedbackConfig config)
        {
            var serializedObject = new SerializedObject(controller);
            serializedObject.FindProperty("config").objectReferenceValue = config;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
