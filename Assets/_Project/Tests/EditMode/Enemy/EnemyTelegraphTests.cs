using NUnit.Framework;
using TapKnockout.Enemy;
using UnityEngine;

namespace TapKnockout.Enemy.Tests
{
    public sealed class EnemyTelegraphTests
    {
        [Test]
        public void TelegraphShapeValidation_AllowsNonNegativeValues()
        {
            Assert.That(EnemyTelegraphController.IsValidTelegraphShape(1f, 1f, 3f, 0.25f), Is.True);
            Assert.That(EnemyTelegraphController.IsValidTelegraphShape(-1f, 1f, 3f, 0.25f), Is.False);
            Assert.That(EnemyTelegraphController.IsValidTelegraphShape(1f, -1f, 3f, 0.25f), Is.False);
            Assert.That(EnemyTelegraphController.IsValidTelegraphShape(1f, 1f, -3f, 0.25f), Is.False);
            Assert.That(EnemyTelegraphController.IsValidTelegraphShape(1f, 1f, 3f, -0.25f), Is.False);
        }

        [Test]
        public void BeginTelegraph_StoresRuntimeTypeAndProgress()
        {
            var root = new GameObject("Telegraph");

            try
            {
                var controller = root.AddComponent<EnemyTelegraphController>();
                controller.BeginTelegraph(null, EnemyTelegraphType.ChargePath, 0.5f, root.transform, null);

                Assert.That(controller.IsTelegraphing, Is.True);
                Assert.That(controller.RuntimeTelegraphType, Is.EqualTo(EnemyTelegraphType.ChargePath));
                Assert.That(controller.NormalizedProgress, Is.EqualTo(0f).Within(0.001f));

                controller.EndTelegraph();
                Assert.That(controller.IsTelegraphing, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
