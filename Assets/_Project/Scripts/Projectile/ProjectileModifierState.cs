using System;
using UnityEngine;

namespace TapKnockout.Projectile
{
    [Serializable]
    public readonly struct ProjectileModifierState
    {
        private const int MaxPatternCount = 6;
        private const int MaxPierceCount = 5;
        private const int MaxRicochetCount = 5;
        private const int MaxWallBounceCount = 3;

        public ProjectileModifierState(
            int extraProjectileCount,
            int frontProjectileCount,
            int diagonalProjectileCount,
            int sideProjectileCount,
            int rearProjectileCount,
            int pierceCount,
            int ricochetCount,
            int wallBounceCount,
            float homingStrength,
            float projectileSizeMultiplier,
            float projectileSpeedMultiplier)
        {
            ExtraProjectileCount = Mathf.Clamp(extraProjectileCount, 0, MaxPatternCount);
            FrontProjectileCount = Mathf.Clamp(frontProjectileCount, 0, MaxPatternCount);
            DiagonalProjectileCount = Mathf.Clamp(diagonalProjectileCount, 0, MaxPatternCount);
            SideProjectileCount = Mathf.Clamp(sideProjectileCount, 0, MaxPatternCount);
            RearProjectileCount = Mathf.Clamp(rearProjectileCount, 0, MaxPatternCount);
            PierceCount = Mathf.Clamp(pierceCount, 0, MaxPierceCount);
            RicochetCount = Mathf.Clamp(ricochetCount, 0, MaxRicochetCount);
            WallBounceCount = Mathf.Clamp(wallBounceCount, 0, MaxWallBounceCount);
            HomingStrength = Mathf.Max(0f, homingStrength);
            ProjectileSizeMultiplier = Mathf.Max(0.1f, projectileSizeMultiplier);
            ProjectileSpeedMultiplier = Mathf.Max(0.1f, projectileSpeedMultiplier);
        }

        public static ProjectileModifierState Neutral => new ProjectileModifierState(0, 0, 0, 0, 0, 0, 0, 0, 0f, 1f, 1f);

        public int ExtraProjectileCount { get; }
        public int FrontProjectileCount { get; }
        public int DiagonalProjectileCount { get; }
        public int SideProjectileCount { get; }
        public int RearProjectileCount { get; }
        public int PierceCount { get; }
        public int RicochetCount { get; }
        public int WallBounceCount { get; }
        public float HomingStrength { get; }
        public float ProjectileSizeMultiplier { get; }
        public float ProjectileSpeedMultiplier { get; }

        public ProjectileBehaviorFlags BehaviorFlags
        {
            get
            {
                var flags = ProjectileBehaviorFlags.None;
                if (PierceCount > 0)
                {
                    flags |= ProjectileBehaviorFlags.Pierce;
                }

                if (RicochetCount > 0)
                {
                    flags |= ProjectileBehaviorFlags.Ricochet;
                }

                if (WallBounceCount > 0)
                {
                    flags |= ProjectileBehaviorFlags.WallBounce;
                }

                if (HomingStrength > 0f)
                {
                    flags |= ProjectileBehaviorFlags.Homing;
                }

                return flags;
            }
        }
    }
}
