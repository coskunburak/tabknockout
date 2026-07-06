using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TapKnockout.Projectile.Tests
{
    public sealed class ProjectileModifierStateTests
    {
        [Test]
        public void ModifierState_ClampsDangerousCounts()
        {
            var state = new ProjectileModifierState(99, 99, 99, 99, 99, 99, 99, 99, 4f, 0f, 0f);

            Assert.That(state.ExtraProjectileCount, Is.EqualTo(6));
            Assert.That(state.FrontProjectileCount, Is.EqualTo(6));
            Assert.That(state.PierceCount, Is.EqualTo(5));
            Assert.That(state.RicochetCount, Is.EqualTo(5));
            Assert.That(state.WallBounceCount, Is.EqualTo(3));
            Assert.That(state.ProjectileSizeMultiplier, Is.EqualTo(0.1f));
            Assert.That(state.ProjectileSpeedMultiplier, Is.EqualTo(0.1f));
        }

        [Test]
        public void PatternBuilder_BuildsForwardAndDiagonalDirections()
        {
            var output = new List<Vector3>();
            var state = new ProjectileModifierState(1, 1, 1, 0, 0, 0, 0, 0, 0f, 1f, 1f);

            var count = ProjectilePatternBuilder.BuildDirections(Vector3.forward, state, output);

            Assert.That(count, Is.EqualTo(5));
            Assert.That(output.Count, Is.EqualTo(5));
        }
    }
}
