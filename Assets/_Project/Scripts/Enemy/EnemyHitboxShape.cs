namespace TapKnockout.Enemy
{
    /// <summary>
    /// Hitbox shape descriptor for EnemyAttackConfig.
    /// Controls which OverlapXxx Physics call is made during the active damage window.
    /// </summary>
    public enum EnemyHitboxShape
    {
        /// <summary>Sphere overlap centred on the enemy (radial burst, slam).</summary>
        Circle = 0,

        /// <summary>Sphere overlap forward of the enemy (melee arc).</summary>
        Arc = 1,

        /// <summary>SphereCast along enemy forward (beam).</summary>
        Line = 2,

        /// <summary>No hitbox — damage applied by projectile collision.</summary>
        Projectile = 3,

        /// <summary>No hitbox — damage applied by area zone tick.</summary>
        Area = 4
    }
}
