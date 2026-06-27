using System.Collections.Generic;
using UnityEngine;

namespace TapKnockout.Projectile
{
    public static class ProjectilePatternBuilder
    {
        private const float ForwardSpreadStepDegrees = 6f;
        private const float MaxForwardSpreadDegrees = 18f;
        private const float DiagonalAngleDegrees = 30f;
        private const float SideAngleDegrees = 90f;
        private const float RearAngleDegrees = 180f;

        public static int BuildDirections(Vector3 forward, ProjectileModifierState modifierState, IList<Vector3> output)
        {
            if (output == null)
            {
                return 0;
            }

            output.Clear();
            var normalizedForward = FlattenAndNormalize(forward);

            AddForwardVolley(normalizedForward, 1 + modifierState.ExtraProjectileCount + modifierState.FrontProjectileCount, output);
            AddMirroredPairs(normalizedForward, DiagonalAngleDegrees, modifierState.DiagonalProjectileCount, output);
            AddMirroredPairs(normalizedForward, SideAngleDegrees, modifierState.SideProjectileCount, output);

            for (var i = 0; i < modifierState.RearProjectileCount; i++)
            {
                output.Add(Rotate(normalizedForward, RearAngleDegrees));
            }

            return output.Count;
        }

        private static void AddForwardVolley(Vector3 forward, int count, IList<Vector3> output)
        {
            count = Mathf.Max(1, count);
            if (count == 1)
            {
                output.Add(forward);
                return;
            }

            var totalSpread = Mathf.Min(MaxForwardSpreadDegrees, (count - 1) * ForwardSpreadStepDegrees);
            var startAngle = -totalSpread * 0.5f;
            var step = count > 1 ? totalSpread / (count - 1) : 0f;
            for (var i = 0; i < count; i++)
            {
                output.Add(Rotate(forward, startAngle + step * i));
            }
        }

        private static void AddMirroredPairs(Vector3 forward, float angleDegrees, int pairCount, IList<Vector3> output)
        {
            pairCount = Mathf.Max(0, pairCount);
            for (var i = 0; i < pairCount; i++)
            {
                var angle = angleDegrees + i * ForwardSpreadStepDegrees;
                output.Add(Rotate(forward, -angle));
                output.Add(Rotate(forward, angle));
            }
        }

        private static Vector3 Rotate(Vector3 direction, float angleDegrees)
        {
            return (Quaternion.AngleAxis(angleDegrees, Vector3.up) * direction).normalized;
        }

        private static Vector3 FlattenAndNormalize(Vector3 direction)
        {
            direction.y = 0f;
            return direction.sqrMagnitude > 0f ? direction.normalized : Vector3.forward;
        }
    }
}
