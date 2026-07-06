namespace TapKnockout.Enemy
{
    /// <summary>
    /// Extended attack type enum for distinct per-enemy attack mechanics.
    /// Each value maps to a specific executor in EnemyDistinctAttackController.
    /// </summary>
    public enum EnemyDistinctAttackType
    {
        None = 0,

        /// <summary>GreenDemon — frontal melee arc hitbox.</summary>
        MeleeArc = 1,

        /// <summary>Bee — locks direction then charges straight.</summary>
        Charge = 2,

        /// <summary>Bat — aims a direction, telegraphs, then dives past the player.</summary>
        Dive = 3,

        /// <summary>YellowDragon — straight-line projectile (fireball).</summary>
        Projectile = 4,

        /// <summary>Cactus close-range — circular burst around caster.</summary>
        RadialBurst = 5,

        /// <summary>Cthulhu — projectile that spawns an area zone on impact/expire.</summary>
        SlimeProjectileArea = 6,

        /// <summary>Cyclops — locks direction, fires damage-active beam along that line.</summary>
        Beam = 7,

        /// <summary>Demon — snaps target position, leaps, circular landing hitbox.</summary>
        LeapSlash = 8,

        /// <summary>Ghost — fires slow homing projectile after a phase visual.</summary>
        HomingProjectile = 9,

        /// <summary>Mushroom — places a delayed area zone at a chosen position near player.</summary>
        SporeZone = 10,

        /// <summary>Yeti — heavy slam, radial hitbox, frost area zone spawned.</summary>
        FrostSlamShockwave = 11,

        /// <summary>Cactus at range — straight spike projectile.</summary>
        SpikeProjectile = 12
    }
}
