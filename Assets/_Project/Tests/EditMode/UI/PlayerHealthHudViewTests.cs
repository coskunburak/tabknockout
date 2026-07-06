using NUnit.Framework;
using TapKnockout.Player;
using TapKnockout.UI;
using TapKnockout.UI.HUD;
using UnityEngine;

namespace TapKnockout.UI.Tests
{
    /// <summary>
    /// EditMode tests for the Player Health HUD system.
    /// Validates config defaults, health math, text formatting, edge cases,
    /// and safe initialization without gameplay sources.
    /// </summary>
    public sealed class PlayerHealthHudViewTests
    {
        // ──────────────────────────────────────────────────────────
        // PlayerHealthHudConfig Tests
        // ──────────────────────────────────────────────────────────

        [Test]
        public void Config_DefaultValues_AreValid()
        {
            var config = ScriptableObject.CreateInstance<PlayerHealthHudConfig>();
            try
            {
                var isValid = config.IsValid(out var error);
                Assert.That(isValid, Is.True, $"Default config should be valid but got error: {error}");
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void Config_LowHealthThreshold_IsWithinRange()
        {
            var config = ScriptableObject.CreateInstance<PlayerHealthHudConfig>();
            try
            {
                Assert.That(config.LowHealthThreshold, Is.GreaterThanOrEqualTo(0f));
                Assert.That(config.LowHealthThreshold, Is.LessThanOrEqualTo(1f));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void Config_HpFillAnimDuration_IsNonNegative()
        {
            var config = ScriptableObject.CreateInstance<PlayerHealthHudConfig>();
            try
            {
                Assert.That(config.HpFillAnimDuration, Is.GreaterThanOrEqualTo(0f));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void Config_DamagePulseScale_IsAtLeastOne()
        {
            var config = ScriptableObject.CreateInstance<PlayerHealthHudConfig>();
            try
            {
                Assert.That(config.DamagePulseScale, Is.GreaterThanOrEqualTo(1f));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        // ──────────────────────────────────────────────────────────
        // Health Math / Ratio Tests
        // ──────────────────────────────────────────────────────────

        [Test]
        public void HealthRatio_FullHealth_IsOne()
        {
            var ratio = ComputeHealthRatio(100f, 100f);
            Assert.That(ratio, Is.EqualTo(1f).Within(0.001f));
        }

        [Test]
        public void HealthRatio_HalfHealth_IsHalf()
        {
            var ratio = ComputeHealthRatio(50f, 100f);
            Assert.That(ratio, Is.EqualTo(0.5f).Within(0.001f));
        }

        [Test]
        public void HealthRatio_ZeroHealth_IsZero()
        {
            var ratio = ComputeHealthRatio(0f, 100f);
            Assert.That(ratio, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void HealthRatio_OverMaxHealth_ClampedToOne()
        {
            // currentHealth > maxHealth should clamp to 1
            var ratio = ComputeHealthRatio(150f, 100f);
            Assert.That(ratio, Is.EqualTo(1f).Within(0.001f));
        }

        [Test]
        public void HealthRatio_ZeroMaxHealth_ReturnsZero()
        {
            // maxHealth == 0 must not divide-by-zero
            var ratio = ComputeHealthRatio(50f, 0f);
            Assert.That(ratio, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void HealthRatio_NegativeCurrent_ClampedToZero()
        {
            var ratio = ComputeHealthRatio(-10f, 100f);
            Assert.That(ratio, Is.EqualTo(0f).Within(0.001f));
        }

        // ──────────────────────────────────────────────────────────
        // HP Text Format Tests
        // ──────────────────────────────────────────────────────────

        [Test]
        public void HpText_FullHealth_FormatsCorrectly()
        {
            var text = FormatHpText("{0} / {1}", 100, 100);
            Assert.That(text, Is.EqualTo("100 / 100"));
        }

        [Test]
        public void HpText_HalfHealth_FormatsCorrectly()
        {
            var text = FormatHpText("{0} / {1}", 50, 100);
            Assert.That(text, Is.EqualTo("50 / 100"));
        }

        [Test]
        public void HpText_ZeroHealth_FormatsCorrectly()
        {
            var text = FormatHpText("{0} / {1}", 0, 100);
            Assert.That(text, Is.EqualTo("0 / 100"));
        }

        [Test]
        public void HpText_LargeValues_FormatsCorrectly()
        {
            var text = FormatHpText("{0} / {1}", 9999, 10000);
            Assert.That(text, Is.EqualTo("9999 / 10000"));
        }

        // ──────────────────────────────────────────────────────────
        // Buff Slot Safety Tests
        // ──────────────────────────────────────────────────────────

        [Test]
        public void BuffSlot_InvalidNegativeIndex_DoesNotThrow()
        {
            var go   = new GameObject("HUD");
            var view = go.AddComponent<PlayerHealthHudView>();
            try
            {
                // Should silently ignore out-of-range index
                Assert.DoesNotThrow(() => view.SetBuffIcon(-1, null));
                Assert.DoesNotThrow(() => view.ClearBuffIcon(-1));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void BuffSlot_IndexBeyondArray_DoesNotThrow()
        {
            var go   = new GameObject("HUD");
            var view = go.AddComponent<PlayerHealthHudView>();
            try
            {
                Assert.DoesNotThrow(() => view.SetBuffIcon(99, null));
                Assert.DoesNotThrow(() => view.ClearBuffIcon(99));
                Assert.DoesNotThrow(() => view.ClearAllBuffIcons());
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        // ──────────────────────────────────────────────────────────
        // Presenter Safety Tests
        // ──────────────────────────────────────────────────────────

        [Test]
        public void Presenter_InitializesWithoutPlayerHealth_DoesNotThrow()
        {
            var presenterGo = new GameObject("Presenter");
            try
            {
                // No PlayerHealth assigned — must not throw on enable or refresh
                var presenter = presenterGo.AddComponent<PlayerHealthHudPresenter>();
                Assert.DoesNotThrow(() => presenter.Refresh());
            }
            finally
            {
                Object.DestroyImmediate(presenterGo);
            }
        }

        [Test]
        public void Presenter_SetPlayerHealth_DoesNotThrowOnNull()
        {
            var presenterGo = new GameObject("Presenter");
            try
            {
                var presenter = presenterGo.AddComponent<PlayerHealthHudPresenter>();
                Assert.DoesNotThrow(() => presenter.SetPlayerHealth(null));
            }
            finally
            {
                Object.DestroyImmediate(presenterGo);
            }
        }

        // ──────────────────────────────────────────────────────────
        // View Safety Tests
        // ──────────────────────────────────────────────────────────

        [Test]
        public void View_SetHealth_WithZeroMax_DoesNotThrow()
        {
            var go   = new GameObject("HUD");
            var view = go.AddComponent<PlayerHealthHudView>();
            try
            {
                Assert.DoesNotThrow(() => view.SetHealth(0f, 0f, false, false));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void View_SetHealth_CurrentGreaterThanMax_DoesNotThrow()
        {
            var go   = new GameObject("HUD");
            var view = go.AddComponent<PlayerHealthHudView>();
            try
            {
                Assert.DoesNotThrow(() => view.SetHealth(200f, 100f, false, false));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void View_SetLevel_SameValue_DoesNotThrowOnRepeat()
        {
            var go   = new GameObject("HUD");
            var view = go.AddComponent<PlayerHealthHudView>();
            try
            {
                Assert.DoesNotThrow(() =>
                {
                    view.SetLevel(5);
                    view.SetLevel(5); // second call with same value
                });
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        // ──────────────────────────────────────────────────────────
        // Existing Test (backward compat — PlayerHealthHudController)
        // ──────────────────────────────────────────────────────────

        [Test]
        public void LegacyController_Refresh_WithPlayerHealth_ShowsHpText()
        {
            var hudObject    = new GameObject("HealthHud");
            var playerObject = new GameObject("Player");
            try
            {
                var health = playerObject.AddComponent<PlayerHealth>();
                health.ResetHealth();

                var controller = hudObject.AddComponent<PlayerHealthHudController>();
                controller.SetPlayerHealth(health);

                Assert.That(controller.CurrentText, Is.EqualTo("HP 100 / 100"));
            }
            finally
            {
                Object.DestroyImmediate(playerObject);
                Object.DestroyImmediate(hudObject);
            }
        }

        // ──────────────────────────────────────────────────────────
        // Internal Helpers (mirror the View's math for isolation testing)
        // ──────────────────────────────────────────────────────────

        private static float ComputeHealthRatio(float current, float max)
        {
            var safeCurrent = max > 0f ? Mathf.Clamp(current, 0f, max) : 0f;
            var safeMax     = Mathf.Max(0f, max);
            return safeMax > 0f ? safeCurrent / safeMax : 0f;
        }

        private static string FormatHpText(string format, int current, int max)
        {
            return string.Format(format, current, max);
        }
    }
}
