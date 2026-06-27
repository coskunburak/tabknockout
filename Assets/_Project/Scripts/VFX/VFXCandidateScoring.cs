using System;

namespace TapKnockout.VFX
{
    public static class VFXCandidateScoring
    {
        public static int ScoreCandidate(VFXEventType eventType, string assetPathOrName)
        {
            if (string.IsNullOrWhiteSpace(assetPathOrName))
            {
                return 0;
            }

            var text = assetPathOrName.ToLowerInvariant();
            var score = 0;

            score += ScoreCommonSignals(text);
            score += eventType switch
            {
                VFXEventType.DashImpact => ScoreDashImpact(text),
                VFXEventType.EnemyHit => ScoreEnemyHit(text),
                VFXEventType.ProjectileHit => ScoreProjectileHit(text),
                VFXEventType.EnemyDeath => ScoreEnemyDeath(text),
                VFXEventType.EnemyKnockbackDust => ScoreKnockbackDust(text),
                VFXEventType.RoomClear => ScoreRoomClear(text),
                VFXEventType.AbilityOffered => ScoreAbility(text),
                VFXEventType.AbilitySelected => ScoreAbility(text),
                VFXEventType.BossWarning => ScoreBossWarning(text),
                VFXEventType.BossHit => ScoreBossHit(text),
                VFXEventType.BossDeath => ScoreBossDeath(text),
                VFXEventType.Pickup => ScorePickup(text),
                VFXEventType.Heal => ScoreHeal(text),
                VFXEventType.GenericBurst => ScoreGenericBurst(text),
                VFXEventType.AbilityAttackBuff => ScoreAbilityAttack(text),
                VFXEventType.AbilityAttackSpeedBuff => ScoreAbilitySpeed(text),
                VFXEventType.AbilityDefenseBuff => ScoreAbilityShield(text),
                VFXEventType.AbilityMoveSpeedBuff => ScoreAbilitySpeed(text),
                VFXEventType.AbilityHealthBuff => ScoreHeal(text),
                VFXEventType.AbilityDashBuff => ScoreDashImpact(text),
                VFXEventType.AbilityDashShockwave => ScoreDashImpact(text) + ScoreKeyword(text, "ground", "shockwave"),
                VFXEventType.AbilityDashPhase => ScoreKeyword(text, "flash", "poof", "phase", "light"),
                VFXEventType.AbilityDashStagger => ScoreKeyword(text, "yellow", "stun", "hit", "impact"),
                VFXEventType.AbilityProjectileBuff => ScoreProjectileHit(text),
                VFXEventType.AbilityProjectileSplit => ScoreKeyword(text, "bounce", "bubble", "glow", "magic"),
                VFXEventType.AbilityProjectilePierce => ScoreProjectileHit(text) + ScoreKeyword(text, "blue", "glowing"),
                VFXEventType.AbilityProjectileRicochet => ScoreKeyword(text, "bounce", "bubble", "glow", "magic"),
                VFXEventType.AbilityProjectileHoming => ScoreKeyword(text, "glow", "bubble", "magic", "light"),
                VFXEventType.AbilityProjectileSize => ScoreGenericBurst(text),
                VFXEventType.AbilityFireProc => ScoreKeyword(text, "fire", "flame", "burn"),
                VFXEventType.AbilityPoisonProc => ScoreKeyword(text, "poison", "cloud", "green"),
                VFXEventType.AbilityIceProc => ScoreKeyword(text, "ice", "frost", "blue"),
                VFXEventType.AbilityLightningProc => ScoreKeyword(text, "lightning", "electric", "spark"),
                VFXEventType.AbilityShield => ScoreAbilityShield(text),
                VFXEventType.AbilitySoulHeal => ScoreHeal(text) + ScoreKeyword(text, "soul"),
                VFXEventType.AbilityBossBreaker => ScoreBossHit(text),
                VFXEventType.AbilityLowHealthSurge => ScoreKeyword(text, "purple", "red", "hit", "magic"),
                VFXEventType.AbilityRewardLuck => ScoreRoomClear(text),
                VFXEventType.AbilityPickupFrenzy => ScorePickup(text),
                VFXEventType.AbilityOrbital => ScoreKeyword(text, "sword", "trail", "spiral", "360"),
                VFXEventType.AbilityDrone => ScoreKeyword(text, "bubble", "glow", "magic"),
                VFXEventType.AbilityBladeStrike => ScoreKeyword(text, "sword", "hit", "slash", "cross"),
                VFXEventType.AbilityMeteor => ScoreKeyword(text, "meteor", "falling", "star", "explosion"),
                VFXEventType.AbilityEnergyBeam => ScoreKeyword(text, "pillar", "beam", "light"),
                VFXEventType.AbilityEnergyRing => ScoreBossWarning(text) + ScoreKeyword(text, "aura", "runic"),
                VFXEventType.AbilityRevive => ScoreKeyword(text, "pillar", "light", "green"),
                VFXEventType.AbilityInvulnerability => ScoreKeyword(text, "shield", "glow", "light"),
                VFXEventType.AbilityGenericUpgrade => ScoreAbility(text),
                _ => 0
            };

            return Math.Max(0, score);
        }

        private static int ScoreCommonSignals(string text)
        {
            var score = 0;

            if (ContainsAny(text, "prefab", "vfx", "fx_", "cfxr"))
            {
                score += 2;
            }

            if (ContainsAny(text, "demo", "scene", "text", "_boom_", "_pow_", "_wham_", "blood"))
            {
                score -= 12;
            }

            if (ContainsAny(text, "loop", "rain", "firewall", "breath"))
            {
                score -= 5;
            }

            return score;
        }

        private static int ScoreDashImpact(string text)
        {
            var score = 0;
            if (ContainsAny(text, "impact", "shockwave", "lightning", "electric", "blue", "glowing"))
            {
                score += 10;
            }

            if (ContainsAny(text, "hit"))
            {
                score += 6;
            }

            return score;
        }

        private static int ScoreEnemyHit(string text)
        {
            var score = 0;
            if (ContainsAny(text, "basic hit", "hit", "flash", "impact"))
            {
                score += 10;
            }

            if (ContainsAny(text, "small", "misc"))
            {
                score += 3;
            }

            return score;
        }

        private static int ScoreProjectileHit(string text)
        {
            var score = 0;
            if (ContainsAny(text, "hit", "spark", "impact", "basic"))
            {
                score += 9;
            }

            if (ContainsAny(text, "projectile", "fireball"))
            {
                score += 5;
            }

            return score;
        }

        private static int ScoreEnemyDeath(string text)
        {
            var score = 0;
            if (ContainsAny(text, "poof", "smoke", "shadow", "explosion", "burst"))
            {
                score += 10;
            }

            if (ContainsAny(text, "enemy"))
            {
                score += 3;
            }

            return score;
        }

        private static int ScoreKnockbackDust(string text)
        {
            var score = 0;
            if (ContainsAny(text, "dust", "smoke", "ground", "shadow"))
            {
                score += 10;
            }

            return score;
        }

        private static int ScoreRoomClear(string text)
        {
            var score = 0;
            if (ContainsAny(text, "gold", "magic", "door", "loot", "star", "firework"))
            {
                score += 10;
            }

            return score;
        }

        private static int ScoreAbility(string text)
        {
            var score = 0;
            if (ContainsAny(text, "magic", "glow", "star", "loot", "greenlight", "shrink"))
            {
                score += 9;
            }

            return score;
        }

        private static int ScoreBossWarning(string text)
        {
            var score = 0;
            if (ContainsAny(text, "magiccircle", "magic circle", "circle", "rune", "telegraph", "warning"))
            {
                score += 12;
            }

            return score;
        }

        private static int ScoreBossHit(string text)
        {
            var score = ScoreDashImpact(text);
            if (ContainsAny(text, "big", "new", "explosion"))
            {
                score += 5;
            }

            return score;
        }

        private static int ScoreBossDeath(string text)
        {
            var score = ScoreEnemyDeath(text);
            if (ContainsAny(text, "big", "new", "explosion"))
            {
                score += 6;
            }

            return score;
        }

        private static int ScorePickup(string text)
        {
            var score = 0;
            if (ContainsAny(text, "loot", "drop", "shiny", "gold", "sparkle"))
            {
                score += 10;
            }

            return score;
        }

        private static int ScoreHeal(string text)
        {
            var score = 0;
            if (ContainsAny(text, "green", "heal", "light", "shrink"))
            {
                score += 10;
            }

            return score;
        }

        private static int ScoreGenericBurst(string text)
        {
            var score = 0;
            if (ContainsAny(text, "hit", "burst", "magic", "impact"))
            {
                score += 8;
            }

            return score;
        }

        private static int ScoreAbilityAttack(string text)
        {
            var score = ScoreAbility(text);
            if (ContainsAny(text, "weapon", "slash", "sword", "hit"))
            {
                score += 8;
            }

            return score;
        }

        private static int ScoreAbilitySpeed(string text)
        {
            var score = 0;
            if (ContainsAny(text, "wind", "trail", "speed", "trace"))
            {
                score += 12;
            }

            if (ContainsAny(text, "loop"))
            {
                score -= 2;
            }

            return score;
        }

        private static int ScoreAbilityShield(string text)
        {
            var score = 0;
            if (ContainsAny(text, "shield", "pillar", "light", "aura", "glow"))
            {
                score += 11;
            }

            return score;
        }

        private static int ScoreKeyword(string text, params string[] keywords)
        {
            return ContainsAny(text, keywords) ? 12 : 0;
        }

        private static bool ContainsAny(string text, params string[] keywords)
        {
            for (var i = 0; i < keywords.Length; i++)
            {
                if (text.IndexOf(keywords[i], StringComparison.Ordinal) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
