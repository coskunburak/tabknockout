using NUnit.Framework;
using UnityEngine;

namespace TapKnockout.Level.Tests
{
    public sealed class ChapterRunnerTimeScaleTests
    {
        [Test]
        public void StartChapter_WhenTimeScaleIsPaused_RestoresCombatTimeScale()
        {
            var previousTimeScale = Time.timeScale;
            var runnerObject = new GameObject("Runner");

            try
            {
                Time.timeScale = 0f;
                var runner = runnerObject.AddComponent<ChapterRunner>();

                runner.StartChapter();

                Assert.That(Time.timeScale, Is.EqualTo(1f));
            }
            finally
            {
                Time.timeScale = previousTimeScale;
                Object.DestroyImmediate(runnerObject);
            }
        }
    }
}
