using System;

namespace TapKnockout.Projectile
{
    [Flags]
    public enum ProjectileBehaviorFlags
    {
        None = 0,
        Pierce = 1 << 0,
        Ricochet = 1 << 1,
        WallBounce = 1 << 2,
        Homing = 1 << 3,
        LongRangeScaling = 1 << 4
    }
}
