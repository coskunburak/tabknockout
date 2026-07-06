// EnemyAttackMechanicsBuilder.cs
// Editor tool: Tap Knockout > Combat > Build Enemy Attack Mechanics

using System;
using System.Collections.Generic;
using System.IO;
using TapKnockout.Combat;
using TapKnockout.Enemy;
using TapKnockout.Projectile;
using TapKnockout.Survivor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TapKnockout.Editor
{
    public static class EnemyAttackMechanicsBuilder
    {
        private const string ConfigFolder = "Assets/_Project/ScriptableObjects/Enemies/CuteMonsters/AttackConfigs";
        private const string PrefabFolder = "Assets/_Project/Prefabs/Enemies/CuteMonsters";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player/Player.prefab";
        private const string MaterialFolder = "Assets/_Project/Content/Combat/Materials/EnemyAttacks";
        private const string ProjectileFolder = "Assets/_Project/Content/Combat/Projectiles/EnemyAttacks";
        private const string AreaZoneFolder = "Assets/_Project/Content/Combat/AreaZones/EnemyAttacks";
        private const string TelegraphFolder = "Assets/_Project/Content/Combat/Telegraphs/EnemyAttacks";
        private const string VfxFolder = "Assets/_Project/Content/Enemies/AttackVFX";
        private const string MenuRoot = "Tap Knockout/Combat/";

        private const string SourceSlashOrange = "Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Orange_Slash_1.prefab";
        private const string SourceGreenHit = "Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Green_Hit.prefab";
        private const string SourcePurpleHit = "Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Purple_Hit_02.prefab";
        private const string SourceFireball = "Assets/ThirdParty/VFX/Eric VFX Studio/Free Game VFX/Prefab/FX_Fireball.prefab";
        private const string SourceFireHit = "Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/Fire Hit .prefab";
        private const string SourceMagicHit = "Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/Magic Hit 2.prefab";
        private const string SourceIceHit = "Assets/ThirdParty/VFX/Matthew Guz/Hits Effects FREE/Prefab/Ice Hit .prefab";
        private const string SourceYellowHit = "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Impacts/CFXR Hit D 3D (Yellow).prefab";
        private const string SourceGroundHit = "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Impacts/CFXR2 Ground Hit.prefab";
        private const string SourcePoisonCloud = "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Misc/CFXR2 Poison Cloud.prefab";
        private const string SourceWindTrails = "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Nature/CFXR4 Wind Trails.prefab";
        private const string SourceWaterSplash = "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Liquids/CFXR Water Splash (Smaller).prefab";
        private const string SourceMagicAura = "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Magic Misc/CFXR3 Magic Aura A (Runic).prefab";
        private const string SourceIceAirHit = "Assets/ThirdParty/VFX/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Ice/CFXR3 Hit Ice B (Air).prefab";
        private const string SourceShockwave = "Assets/GabrielAguiarProductions/FreeQuickEffectsVol1/Prefabs/vfx_Shockwave_01.prefab";
        private const string SourceHyperdrive = "Assets/GabrielAguiarProductions/FreeQuickEffectsVol1/Prefabs/vfx_Hyperdrive_01.prefab";
        private const string SourceSparks = "Assets/GabrielAguiarProductions/FreeQuickEffectsVol1/Prefabs/vfx_Sparks_01.prefab";

        [MenuItem(MenuRoot + "Build Enemy Attack Mechanics", priority = 200)]
        public static void BuildAll()
        {
            Debug.Log("=== EnemyAttackMechanicsBuilder: START ===");

            EnsureFolder(ConfigFolder);
            EnsureFolder(MaterialFolder);
            EnsureFolder(ProjectileFolder);
            EnsureFolder(AreaZoneFolder);
            EnsureFolder(TelegraphFolder);
            EnsureFolder(VfxFolder);

            var fallbackAssets = CreateFallbackAssets();
            var configs = CreateAllConfigs(fallbackAssets);
            var runtimeBindings = ResolveRuntimeEnemyBindings(configs);

            WireRuntimePrefabs(runtimeBindings);
            EnsurePlayerCombatHurtbox();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EnemyAttackMechanicsValidator.ValidateAll();

            Debug.Log("=== EnemyAttackMechanicsBuilder: DONE ===");
        }

        private static BuilderConfigs CreateAllConfigs(BuilderAssets assets)
        {
            var c = new BuilderConfigs
            {
                GreenDemonMelee = CreateOrLoad<EnemyAttackConfig>(ConfigFolder, "AC_GreenDemon_MeleeArc"),
                BatDive = CreateOrLoad<EnemyAttackConfig>(ConfigFolder, "AC_Bat_FlyingDive"),
                BeeCharge = CreateOrLoad<EnemyAttackConfig>(ConfigFolder, "AC_Bee_StingCharge"),
                YellowDragonFireball = CreateOrLoad<EnemyAttackConfig>(ConfigFolder, "AC_YellowDragon_Fireball"),
                CactusSpike = CreateOrLoad<EnemyAttackConfig>(ConfigFolder, "AC_Cactus_SpikeProjectile"),
                CactusRadial = CreateOrLoad<EnemyAttackConfig>(ConfigFolder, "AC_Cactus_RadialSpikeBurst"),
                CthulhuSlime = CreateOrLoad<EnemyAttackConfig>(ConfigFolder, "AC_Cthulhu_SlimeProjectileSlowPool"),
                CyclopsBeam = CreateOrLoad<EnemyAttackConfig>(ConfigFolder, "AC_Cyclops_EyeBeam"),
                DemonLeap = CreateOrLoad<EnemyAttackConfig>(ConfigFolder, "AC_Demon_LeapSlash"),
                GhostHoming = CreateOrLoad<EnemyAttackConfig>(ConfigFolder, "AC_Ghost_PhaseHomingCurse"),
                MushroomSpore = CreateOrLoad<EnemyAttackConfig>(ConfigFolder, "AC_Mushroom_SporePoisonZone"),
                YetiFrost = CreateOrLoad<EnemyAttackConfig>(ConfigFolder, "AC_Yeti_FrostSlamShockwave")
            };

            var hitMask = ResolvePlayerHitMask();

            SetBaseFields(c.GreenDemonMelee, "green_demon_melee_arc", "GreenDemon Melee Bite",
                EnemyDistinctAttackType.MeleeArc, EnemyTelegraphType.Cone,
                triggerRange: 1.7f, cooldown: 1.2f, windup: 0.3f, active: 0.15f, recovery: 0.55f,
                damage: 8f, knockback: 3f, kbDuration: 0.12f, hitMask,
                assets.GreenDemonTelegraph, assets.MeleeSlashVfx, assets.MeleeImpactVfx);
            c.GreenDemonMelee.SetPrivate("hitboxShape", (int)EnemyHitboxShape.Arc);
            c.GreenDemonMelee.SetPrivate("hitboxRadius", 1.7f);
            c.GreenDemonMelee.SetPrivate("hitboxArcHalfAngle", 70f);
            c.GreenDemonMelee.SetPrivate("debugColor", new Color(0.2f, 0.9f, 0.2f));
            c.GreenDemonMelee.SetPrivate("commitLocksMovement", true);
            c.GreenDemonMelee.SetPrivate("canMoveDuringWindup", false);

            SetBaseFields(c.BatDive, "bat_flying_dive", "Bat Flying Dive",
                EnemyDistinctAttackType.Dive, EnemyTelegraphType.ChargePath,
                triggerRange: 7f, cooldown: 2.5f, windup: 0.6f, active: 0.5f, recovery: 0.7f,
                damage: 6f, knockback: 4f, kbDuration: 0.1f, hitMask,
                assets.BatTelegraph, assets.DiveTrailVfx, assets.DiveImpactVfx);
            c.BatDive.SetPrivate("hitboxShape", (int)EnemyHitboxShape.Projectile);
            c.BatDive.SetPrivate("hitboxRadius", 0.9f);
            c.BatDive.SetPrivate("diveSpeedMultiplier", 5f);
            c.BatDive.SetPrivate("overshootDistance", 3f);
            c.BatDive.SetPrivate("debugColor", new Color(0.7f, 0.3f, 1f));
            c.BatDive.SetPrivate("commitLocksMovement", true);

            SetBaseFields(c.BeeCharge, "bee_sting_charge", "Bee Sting Charge",
                EnemyDistinctAttackType.Charge, EnemyTelegraphType.ChargePath,
                triggerRange: 6f, cooldown: 2f, windup: 0.45f, active: 0.4f, recovery: 0.6f,
                damage: 7f, knockback: 5f, kbDuration: 0.15f, hitMask,
                assets.BeeTelegraph, assets.ChargeTrailVfx, assets.StingImpactVfx);
            c.BeeCharge.SetPrivate("hitboxShape", (int)EnemyHitboxShape.Projectile);
            c.BeeCharge.SetPrivate("hitboxRadius", 0.9f);
            c.BeeCharge.SetPrivate("projectileSpeed", 10f);
            c.BeeCharge.SetPrivate("diveSpeedMultiplier", 4.5f);
            c.BeeCharge.SetPrivate("debugColor", new Color(1f, 0.85f, 0.1f));
            c.BeeCharge.SetPrivate("commitLocksMovement", true);
            c.BeeCharge.SetPrivate("commitLocksRotation", true);

            SetBaseFields(c.YellowDragonFireball, "yellow_dragon_fireball", "YellowDragon Fireball",
                EnemyDistinctAttackType.Projectile, EnemyTelegraphType.Line,
                triggerRange: 7f, cooldown: 2.2f, windup: 0.55f, active: 0.1f, recovery: 0.65f,
                damage: 12f, knockback: 3f, kbDuration: 0.1f, hitMask,
                assets.LineTelegraph, assets.FireMuzzleVfx, assets.FireImpactVfx);
            c.YellowDragonFireball.SetPrivate("hitboxShape", (int)EnemyHitboxShape.Projectile);
            c.YellowDragonFireball.SetPrivate("projectilePrefab", assets.FireballProjectile);
            c.YellowDragonFireball.SetPrivate("projectileSpeed", 7f);
            c.YellowDragonFireball.SetPrivate("projectileLifetime", 4f);
            c.YellowDragonFireball.SetPrivate("preferredMinRange", 4f);
            c.YellowDragonFireball.SetPrivate("preferredMaxRange", 7f);
            c.YellowDragonFireball.SetPrivate("debugColor", new Color(1f, 0.5f, 0.1f));

            SetBaseFields(c.CactusSpike, "cactus_spike_projectile", "Cactus Spike Projectile",
                EnemyDistinctAttackType.SpikeProjectile, EnemyTelegraphType.Line,
                triggerRange: 7f, cooldown: 2.2f, windup: 0.45f, active: 0.1f, recovery: 0.55f,
                damage: 10f, knockback: 2f, kbDuration: 0.1f, hitMask,
                assets.LineTelegraph, assets.SpikeVfx, assets.SpikeImpactVfx);
            c.CactusSpike.SetPrivate("hitboxShape", (int)EnemyHitboxShape.Projectile);
            c.CactusSpike.SetPrivate("projectilePrefab", assets.SpikeProjectile);
            c.CactusSpike.SetPrivate("projectileSpeed", 8f);
            c.CactusSpike.SetPrivate("projectileLifetime", 3.5f);
            c.CactusSpike.SetPrivate("debugColor", new Color(0.2f, 0.8f, 0.3f));

            SetBaseFields(c.CactusRadial, "cactus_radial_spike_burst", "Cactus Radial Spike Burst",
                EnemyDistinctAttackType.RadialBurst, EnemyTelegraphType.Circle,
                triggerRange: 2.4f, cooldown: 4f, windup: 0.65f, active: 0.15f, recovery: 0.7f,
                damage: 13f, knockback: 6f, kbDuration: 0.2f, hitMask,
                assets.CactusTelegraph, assets.RadialSpikeBurstVfx, assets.SpikeImpactVfx);
            c.CactusRadial.SetPrivate("hitboxShape", (int)EnemyHitboxShape.Circle);
            c.CactusRadial.SetPrivate("hitboxRadius", 2.4f);
            c.CactusRadial.SetPrivate("initialCooldownOffset", 0.5f);
            c.CactusRadial.SetPrivate("debugColor", new Color(0.6f, 1f, 0.2f));
            c.CactusRadial.SetPrivate("commitLocksMovement", true);

            SetBaseFields(c.CthulhuSlime, "cthulhu_slime_pool", "Cthulhu Slime Projectile",
                EnemyDistinctAttackType.SlimeProjectileArea, EnemyTelegraphType.Line,
                triggerRange: 7f, cooldown: 3f, windup: 0.6f, active: 0.1f, recovery: 0.75f,
                damage: 6f, knockback: 1f, kbDuration: 0.05f, hitMask,
                assets.LineTelegraph, assets.SlimeOrbVfx, assets.SlimeSplashVfx);
            c.CthulhuSlime.SetPrivate("hitboxShape", (int)EnemyHitboxShape.Projectile);
            c.CthulhuSlime.SetPrivate("projectilePrefab", assets.SlimeProjectile);
            c.CthulhuSlime.SetPrivate("areaZonePrefab", assets.CthulhuSlowPool);
            c.CthulhuSlime.SetPrivate("projectileSpeed", 6f);
            c.CthulhuSlime.SetPrivate("projectileLifetime", 3f);
            c.CthulhuSlime.SetPrivate("areaZoneRadius", 2f);
            c.CthulhuSlime.SetPrivate("areaZoneDuration", 4f);
            c.CthulhuSlime.SetPrivate("areaZoneTickInterval", 1f);
            c.CthulhuSlime.SetPrivate("areaZoneTickDamage", 2f);
            c.CthulhuSlime.SetPrivate("statusEffectType", (int)StatusEffectType.Slow);
            c.CthulhuSlime.SetPrivate("statusEffectDuration", 3f);
            c.CthulhuSlime.SetPrivate("statusEffectSlowMultiplier", 0.55f);
            c.CthulhuSlime.SetPrivate("preferredMinRange", 4f);
            c.CthulhuSlime.SetPrivate("maxActiveZones", 2);
            c.CthulhuSlime.SetPrivate("debugColor", new Color(0.3f, 0.9f, 0.3f));

            SetBaseFields(c.CyclopsBeam, "cyclops_eye_beam", "Cyclops Eye Beam",
                EnemyDistinctAttackType.Beam, EnemyTelegraphType.ChargePath,
                triggerRange: 8f, cooldown: 5f, windup: 1f, active: 0.4f, recovery: 1.2f,
                damage: 18f, knockback: 5f, kbDuration: 0.15f, hitMask,
                assets.CyclopsTelegraph, assets.EyeBeamVfx, assets.EyeBeamImpactVfx);
            c.CyclopsBeam.SetPrivate("hitboxShape", (int)EnemyHitboxShape.Line);
            c.CyclopsBeam.SetPrivate("beamLength", 8f);
            c.CyclopsBeam.SetPrivate("beamWidth", 0.4f);
            c.CyclopsBeam.SetPrivate("commitLocksMovement", true);
            c.CyclopsBeam.SetPrivate("commitLocksRotation", true);
            c.CyclopsBeam.SetPrivate("canMoveDuringWindup", false);
            c.CyclopsBeam.SetPrivate("debugColor", new Color(1f, 0.3f, 0.1f));

            SetBaseFields(c.DemonLeap, "demon_leap_slash", "Demon Leap Slash",
                EnemyDistinctAttackType.LeapSlash, EnemyTelegraphType.Circle,
                triggerRange: 5f, cooldown: 3f, windup: 0.65f, active: 0.5f, recovery: 1f,
                damage: 14f, knockback: 7f, kbDuration: 0.2f, hitMask,
                assets.DemonTelegraph, assets.LeapTrailVfx, assets.LeapImpactVfx);
            c.DemonLeap.SetPrivate("hitboxShape", (int)EnemyHitboxShape.Circle);
            c.DemonLeap.SetPrivate("hitboxRadius", 1.8f);
            c.DemonLeap.SetPrivate("diveSpeedMultiplier", 4f);
            c.DemonLeap.SetPrivate("commitLocksMovement", true);
            c.DemonLeap.SetPrivate("debugColor", new Color(0.9f, 0.1f, 0.1f));

            SetBaseFields(c.GhostHoming, "ghost_homing_curse", "Ghost Homing Curse",
                EnemyDistinctAttackType.HomingProjectile, EnemyTelegraphType.Line,
                triggerRange: 7f, cooldown: 3f, windup: 0.7f, active: 0.1f, recovery: 0.8f,
                damage: 9f, knockback: 2f, kbDuration: 0.1f, hitMask,
                assets.LineTelegraph, assets.GhostPhaseVfx, assets.GhostCurseImpactVfx);
            c.GhostHoming.SetPrivate("hitboxShape", (int)EnemyHitboxShape.Projectile);
            c.GhostHoming.SetPrivate("projectilePrefab", assets.CurseProjectile);
            c.GhostHoming.SetPrivate("projectileSpeed", 4f);
            c.GhostHoming.SetPrivate("projectileLifetime", 5f);
            c.GhostHoming.SetPrivate("homingStrength", 0.3f);
            c.GhostHoming.SetPrivate("homingMaxTurnDegreesPerSecond", 40f);
            c.GhostHoming.SetPrivate("preferredMinRange", 4f);
            c.GhostHoming.SetPrivate("debugColor", new Color(0.6f, 0.3f, 0.9f));

            SetBaseFields(c.MushroomSpore, "mushroom_spore_zone", "Mushroom Spore Zone",
                EnemyDistinctAttackType.SporeZone, EnemyTelegraphType.Circle,
                triggerRange: 7f, cooldown: 4f, windup: 0.75f, active: 0.15f, recovery: 0.65f,
                damage: 1f, knockback: 0f, kbDuration: 0f, hitMask,
                assets.MushroomTelegraph, assets.SporeBurstVfx, assets.SporeCloudVfx);
            c.MushroomSpore.SetPrivate("hitboxShape", (int)EnemyHitboxShape.Area);
            c.MushroomSpore.SetPrivate("areaZonePrefab", assets.MushroomSporeZone);
            c.MushroomSpore.SetPrivate("areaZoneRadius", 2.2f);
            c.MushroomSpore.SetPrivate("areaZoneDuration", 4.5f);
            c.MushroomSpore.SetPrivate("areaZoneTickInterval", 1f);
            c.MushroomSpore.SetPrivate("areaZoneTickDamage", 3f);
            c.MushroomSpore.SetPrivate("statusEffectType", (int)StatusEffectType.Poison);
            c.MushroomSpore.SetPrivate("statusEffectDuration", 2f);
            c.MushroomSpore.SetPrivate("statusEffectSlowMultiplier", 1f);
            c.MushroomSpore.SetPrivate("maxActiveZones", 2);
            c.MushroomSpore.SetPrivate("debugColor", new Color(0.5f, 0.1f, 0.8f));

            SetBaseFields(c.YetiFrost, "yeti_frost_slam", "Yeti Frost Slam Shockwave",
                EnemyDistinctAttackType.FrostSlamShockwave, EnemyTelegraphType.Circle,
                triggerRange: 3.1f, cooldown: 4f, windup: 1f, active: 0.2f, recovery: 1.3f,
                damage: 16f, knockback: 8f, kbDuration: 0.25f, hitMask,
                assets.YetiTelegraph, assets.FrostSlamImpactVfx, assets.FrostShockwaveVfx);
            c.YetiFrost.SetPrivate("hitboxShape", (int)EnemyHitboxShape.Circle);
            c.YetiFrost.SetPrivate("hitboxRadius", 3.1f);
            c.YetiFrost.SetPrivate("areaZonePrefab", assets.YetiFrostZone);
            c.YetiFrost.SetPrivate("areaZoneRadius", 2.8f);
            c.YetiFrost.SetPrivate("areaZoneDuration", 2.5f);
            c.YetiFrost.SetPrivate("areaZoneTickInterval", 0.5f);
            c.YetiFrost.SetPrivate("areaZoneTickDamage", 0f);
            c.YetiFrost.SetPrivate("statusEffectType", (int)StatusEffectType.Slow);
            c.YetiFrost.SetPrivate("statusEffectDuration", 2.5f);
            c.YetiFrost.SetPrivate("statusEffectSlowMultiplier", 0.5f);
            c.YetiFrost.SetPrivate("commitLocksMovement", true);
            c.YetiFrost.SetPrivate("canMoveDuringWindup", false);
            c.YetiFrost.SetPrivate("debugColor", new Color(0.4f, 0.8f, 1f));

            foreach (var cfg in c.AllConfigs)
            {
                EditorUtility.SetDirty(cfg);
            }

            Debug.Log($"[Builder] {c.AllConfigs.Length} attack configs created/updated in {ConfigFolder}");
            return c;
        }

        private static BuilderAssets CreateFallbackAssets()
        {
            var assets = new BuilderAssets
            {
                GreenDemonTelegraph = CreateTelegraphPrefab("PF_Telegraph_GreenDemon_MeleeArc", TelegraphVisualShape.Arc, new Color(0.45f, 1f, 0.18f, 0.78f), 0.75f),
                BatTelegraph = CreateTelegraphPrefab("PF_Telegraph_Bat_DiveLine", TelegraphVisualShape.Line, new Color(0.72f, 0.32f, 1f, 0.72f), 0.9f),
                BeeTelegraph = CreateTelegraphPrefab("PF_Telegraph_Bee_ChargeLine", TelegraphVisualShape.Line, new Color(1f, 0.88f, 0.08f, 0.76f), 0.75f),
                CactusTelegraph = CreateTelegraphPrefab("PF_Telegraph_Cactus_RadialCircle", TelegraphVisualShape.Circle, new Color(0.32f, 1f, 0.18f, 0.68f), 1f),
                CyclopsTelegraph = CreateTelegraphPrefab("PF_Telegraph_Cyclops_BeamLine", TelegraphVisualShape.Line, new Color(1f, 0.18f, 0.06f, 0.75f), 1.15f),
                DemonTelegraph = CreateTelegraphPrefab("PF_Telegraph_Demon_LandingCircle", TelegraphVisualShape.Circle, new Color(1f, 0.12f, 0.05f, 0.72f), 1f),
                MushroomTelegraph = CreateTelegraphPrefab("PF_Telegraph_Mushroom_SporeCircle", TelegraphVisualShape.Circle, new Color(0.68f, 0.16f, 1f, 0.68f), 1.05f),
                YetiTelegraph = CreateTelegraphPrefab("PF_Telegraph_Yeti_FrostCircle", TelegraphVisualShape.Circle, new Color(0.36f, 0.86f, 1f, 0.68f), 1.2f),
                LineTelegraph = CreateTelegraphPrefab("PF_Telegraph_EnemyProjectile_Line", TelegraphVisualShape.Line, new Color(1f, 0.66f, 0.12f, 0.68f), 0.9f),

                MeleeSlashVfx = CreateTimedVfxPrefab("PF_VFX_GreenDemon_MeleeSlash", AttackVfxStyle.Slash, EnemyAttackVFXKind.Active, new Color(0.72f, 1f, 0.22f, 1f), 0.55f, SourceSlashOrange, 0.55f),
                MeleeImpactVfx = CreateTimedVfxPrefab("PF_VFX_GreenDemon_HitImpact", AttackVfxStyle.Burst, EnemyAttackVFXKind.Impact, new Color(0.48f, 1f, 0.24f, 1f), 0.65f, SourceGreenHit, 0.7f),
                DiveTrailVfx = CreateTimedVfxPrefab("PF_VFX_Bat_DiveTrail", AttackVfxStyle.Trail, EnemyAttackVFXKind.Active, new Color(0.58f, 0.22f, 1f, 1f), 0.75f, SourceWindTrails, 0.45f),
                DiveImpactVfx = CreateTimedVfxPrefab("PF_VFX_Bat_DiveImpact", AttackVfxStyle.Burst, EnemyAttackVFXKind.Impact, new Color(0.68f, 0.26f, 1f, 1f), 0.7f, SourcePurpleHit, 0.85f),
                ChargeTrailVfx = CreateTimedVfxPrefab("PF_VFX_Bee_ChargeTrail", AttackVfxStyle.Trail, EnemyAttackVFXKind.Active, new Color(1f, 0.86f, 0.08f, 1f), 0.65f, SourceHyperdrive, 0.35f),
                StingImpactVfx = CreateTimedVfxPrefab("PF_VFX_Bee_StingImpact", AttackVfxStyle.Burst, EnemyAttackVFXKind.Impact, new Color(1f, 0.88f, 0.1f, 1f), 0.55f, SourceYellowHit, 0.7f),
                FireMuzzleVfx = CreateTimedVfxPrefab("PF_VFX_YellowDragon_FireMuzzle", AttackVfxStyle.Muzzle, EnemyAttackVFXKind.Active, new Color(1f, 0.34f, 0.04f, 1f), 0.6f, SourceFireball, 0.35f),
                FireImpactVfx = CreateTimedVfxPrefab("PF_VFX_Fireball_Impact", AttackVfxStyle.Burst, EnemyAttackVFXKind.Impact, new Color(1f, 0.25f, 0.03f, 1f), 0.8f, SourceFireHit, 0.9f),
                SpikeVfx = CreateTimedVfxPrefab("PF_VFX_Cactus_SpikeCast", AttackVfxStyle.Muzzle, EnemyAttackVFXKind.Active, new Color(0.28f, 1f, 0.18f, 1f), 0.55f, SourceGreenHit, 0.5f),
                SpikeImpactVfx = CreateTimedVfxPrefab("PF_VFX_Cactus_SpikeImpact", AttackVfxStyle.Burst, EnemyAttackVFXKind.Impact, new Color(0.38f, 1f, 0.22f, 1f), 0.65f, SourceGreenHit, 0.65f),
                RadialSpikeBurstVfx = CreateTimedVfxPrefab("PF_VFX_Cactus_RadialSpikeBurst", AttackVfxStyle.Spikes, EnemyAttackVFXKind.Active, new Color(0.42f, 1f, 0.12f, 1f), 0.9f, SourceSparks, 0.55f),
                SlimeOrbVfx = CreateTimedVfxPrefab("PF_VFX_Cthulhu_SlimeOrbCast", AttackVfxStyle.Muzzle, EnemyAttackVFXKind.Active, new Color(0.14f, 0.95f, 0.22f, 1f), 0.65f, SourceGreenHit, 0.5f),
                SlimeSplashVfx = CreateTimedVfxPrefab("PF_VFX_Cthulhu_SlimeSplash", AttackVfxStyle.Splash, EnemyAttackVFXKind.Impact, new Color(0.18f, 1f, 0.28f, 1f), 0.85f, SourceWaterSplash, 0.7f),
                EyeBeamVfx = CreateTimedVfxPrefab("PF_VFX_Cyclops_EyeBeam", AttackVfxStyle.Beam, EnemyAttackVFXKind.Active, new Color(1f, 0.06f, 0.02f, 1f), 0.55f, SourceSparks, 0.45f),
                EyeBeamImpactVfx = CreateTimedVfxPrefab("PF_VFX_Cyclops_BeamImpact", AttackVfxStyle.Burst, EnemyAttackVFXKind.Impact, new Color(1f, 0.16f, 0.04f, 1f), 0.65f, SourceFireHit, 0.55f),
                LeapTrailVfx = CreateTimedVfxPrefab("PF_VFX_Demon_LeapTrail", AttackVfxStyle.Trail, EnemyAttackVFXKind.Active, new Color(1f, 0.1f, 0.04f, 1f), 0.8f, SourceWindTrails, 0.42f),
                LeapImpactVfx = CreateTimedVfxPrefab("PF_VFX_Demon_LandingImpact", AttackVfxStyle.Shockwave, EnemyAttackVFXKind.Impact, new Color(1f, 0.12f, 0.03f, 1f), 0.95f, SourceGroundHit, 0.8f),
                GhostPhaseVfx = CreateTimedVfxPrefab("PF_VFX_Ghost_PhaseAura", AttackVfxStyle.Aura, EnemyAttackVFXKind.Active, new Color(0.56f, 0.2f, 1f, 1f), 1f, SourceMagicAura, 0.5f),
                GhostCurseImpactVfx = CreateTimedVfxPrefab("PF_VFX_Ghost_CurseImpact", AttackVfxStyle.Burst, EnemyAttackVFXKind.Impact, new Color(0.64f, 0.2f, 1f, 1f), 0.75f, SourceMagicHit, 0.75f),
                SporeBurstVfx = CreateTimedVfxPrefab("PF_VFX_Mushroom_SporeBurst", AttackVfxStyle.Cloud, EnemyAttackVFXKind.Active, new Color(0.67f, 0.14f, 1f, 1f), 0.9f, SourcePoisonCloud, 0.45f),
                SporeCloudVfx = CreateTimedVfxPrefab("PF_VFX_Mushroom_SporeCloud", AttackVfxStyle.Cloud, EnemyAttackVFXKind.Impact, new Color(0.58f, 0.08f, 0.92f, 1f), 1f, SourcePoisonCloud, 0.55f),
                FrostSlamImpactVfx = CreateTimedVfxPrefab("PF_VFX_Yeti_FrostSlamImpact", AttackVfxStyle.Shockwave, EnemyAttackVFXKind.Active, new Color(0.44f, 0.88f, 1f, 1f), 1f, SourceIceAirHit, 0.85f),
                FrostShockwaveVfx = CreateTimedVfxPrefab("PF_VFX_Yeti_FrostShockwave", AttackVfxStyle.Shockwave, EnemyAttackVFXKind.Impact, new Color(0.5f, 0.9f, 1f, 1f), 1f, SourceShockwave, 0.65f)
            };

            assets.CthulhuSlowPool = CreateAreaZonePrefab("PF_EnemyArea_Cthulhu_SlowPool", new Color(0.1f, 0.8f, 0.25f, 0.65f), 2f, 4f, 1f, 2f, StatusEffectType.Slow, 3f, 0.55f);
            assets.MushroomSporeZone = CreateAreaZonePrefab("PF_EnemyArea_Mushroom_SporePoisonZone", new Color(0.58f, 0.08f, 0.9f, 0.65f), 2.2f, 4.5f, 1f, 3f, StatusEffectType.Poison, 2f, 1f);
            assets.YetiFrostZone = CreateAreaZonePrefab("PF_EnemyArea_Yeti_FrostSlowZone", new Color(0.35f, 0.85f, 1f, 0.6f), 2.8f, 2.5f, 0.5f, 0f, StatusEffectType.Slow, 2.5f, 0.5f);

            assets.FireballProjectile = CreateProjectilePrefab("PF_EnemyProjectile_YellowDragon_Fireball", ProjectileVisualStyle.Fireball, new Color(1f, 0.32f, 0.05f, 1f), false, false, assets.FireImpactVfx, SourceFireball);
            assets.SpikeProjectile = CreateProjectilePrefab("PF_EnemyProjectile_Cactus_Spike", ProjectileVisualStyle.Spike, new Color(0.25f, 1f, 0.2f, 1f), false, false, assets.SpikeImpactVfx, string.Empty);
            assets.SlimeProjectile = CreateProjectilePrefab("PF_EnemyProjectile_Cthulhu_SlimeOrb", ProjectileVisualStyle.Slime, new Color(0.15f, 0.9f, 0.25f, 1f), false, true, assets.SlimeSplashVfx, string.Empty);
            assets.CurseProjectile = CreateProjectilePrefab("PF_EnemyProjectile_Ghost_CurseOrb", ProjectileVisualStyle.Curse, new Color(0.55f, 0.2f, 1f, 1f), true, false, assets.GhostCurseImpactVfx, string.Empty);

            return assets;
        }

        private static List<EnemyRuntimeBinding> ResolveRuntimeEnemyBindings(BuilderConfigs configs)
        {
            var byEnemy = new Dictionary<string, EnemyRuntimeBinding>(StringComparer.OrdinalIgnoreCase);
            var configByEnemy = new Dictionary<string, EnemyAttackConfig[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["GreenDemon"] = new[] { configs.GreenDemonMelee },
                ["Bat"] = new[] { configs.BatDive },
                ["Bee"] = new[] { configs.BeeCharge },
                ["YellowDragon"] = new[] { configs.YellowDragonFireball },
                ["Cactus"] = new[] { configs.CactusSpike, configs.CactusRadial },
                ["Cthulhu"] = new[] { configs.CthulhuSlime },
                ["Cyclops"] = new[] { configs.CyclopsBeam },
                ["Demon"] = new[] { configs.DemonLeap },
                ["Ghost"] = new[] { configs.GhostHoming },
                ["Mushroom"] = new[] { configs.MushroomSpore },
                ["Yeti"] = new[] { configs.YetiFrost }
            };

            var groupGuids = AssetDatabase.FindAssets("t:SpawnGroupConfig", new[] { "Assets/_Project/ScriptableObjects/Waves" });
            for (var i = 0; i < groupGuids.Length; i++)
            {
                var groupPath = AssetDatabase.GUIDToAssetPath(groupGuids[i]);
                var group = AssetDatabase.LoadAssetAtPath<SpawnGroupConfig>(groupPath);
                if (group == null || group.EnemyPrefab == null)
                {
                    continue;
                }

                var enemyName = ResolveEnemyName(group);
                if (string.IsNullOrEmpty(enemyName) || !configByEnemy.TryGetValue(enemyName, out var attackConfigs))
                {
                    continue;
                }

                byEnemy[enemyName] = new EnemyRuntimeBinding(
                    enemyName,
                    group.EnemyPrefab,
                    AssetDatabase.GetAssetPath(group.EnemyPrefab),
                    attackConfigs,
                    groupPath);
            }

            AddFallbackBinding(byEnemy, "GreenDemon", "PF_Enemy_BasicMelee_GreenDemon_Generated", configByEnemy);
            AddFallbackBinding(byEnemy, "Bat", "PF_Enemy_Bat", configByEnemy);
            AddFallbackBinding(byEnemy, "Bee", "PF_Enemy_Bee", configByEnemy);
            AddFallbackBinding(byEnemy, "YellowDragon", "PF_Boss_YellowDragon", configByEnemy);
            AddFallbackBinding(byEnemy, "Cactus", "PF_Enemy_Cactus", configByEnemy);
            AddFallbackBinding(byEnemy, "Cthulhu", "PF_Enemy_Cthulhu", configByEnemy);
            AddFallbackBinding(byEnemy, "Cyclops", "PF_Enemy_Cyclops", configByEnemy);
            AddFallbackBinding(byEnemy, "Demon", "PF_Enemy_Demon", configByEnemy);
            AddFallbackBinding(byEnemy, "Ghost", "PF_Enemy_Ghost", configByEnemy);
            AddFallbackBinding(byEnemy, "Mushroom", "PF_Enemy_Mushroom", configByEnemy);
            AddFallbackBinding(byEnemy, "Yeti", "PF_Enemy_Yeti", configByEnemy);

            return new List<EnemyRuntimeBinding>(byEnemy.Values);
        }

        private static void WireRuntimePrefabs(List<EnemyRuntimeBinding> bindings)
        {
            var modified = 0;
            for (var i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i];
                if (binding.Prefab == null || string.IsNullOrWhiteSpace(binding.PrefabPath))
                {
                    Debug.LogError($"[Builder] Missing runtime prefab for {binding.EnemyName}.");
                    continue;
                }

                var root = PrefabUtility.LoadPrefabContents(binding.PrefabPath);
                var changed = false;

                var controller = root.GetComponent<EnemyDistinctAttackController>();
                if (controller == null)
                {
                    controller = root.AddComponent<EnemyDistinctAttackController>();
                    changed = true;
                }

                controller.enabled = true;
                var attackOrigin = EnsureChildTransform(root, "AttackOrigin", new Vector3(0f, 0.9f, 0.45f), ref changed);
                var projectileSpawnPoint = EnsureChildTransform(root, "ProjectileSpawnPoint", new Vector3(0f, 1f, 0.8f), ref changed);
                var groundOrigin = EnsureChildTransform(root, "GroundOrigin", Vector3.zero, ref changed);
                var vfxRoot = EnsureChildTransform(root, "VFXRoot", new Vector3(0f, 0.55f, 0f), ref changed);
                EnsureTelegraphController(root, binding.Configs[0], ref changed);

                var so = new SerializedObject(controller);
                SetArray(so, "attackConfigs", binding.Configs);
                SetObject(so, "attackOrigin", attackOrigin);
                SetObject(so, "projectileSpawnPoint", projectileSpawnPoint);
                SetObject(so, "groundOrigin", groundOrigin);
                SetObject(so, "vfxRoot", vfxRoot);
                SetBool(so, "recoverPlayerTargetWhenMissing", true);
                SetBool(so, "warnIfTargetMissing", true);
                SetBool(so, "debugMode", false);
                so.ApplyModifiedPropertiesWithoutUndo();
                changed = true;

                var distinctReady = EnemyAttackReadinessUtility.IsDistinctAttackSystemReady(binding.Configs, out var reason);
                var legacyAttack = root.GetComponent<EnemyAttackController>();
                if (legacyAttack != null)
                {
                    var legacySO = new SerializedObject(legacyAttack);
                    SetBool(legacySO, "autoDealContactDamage", !distinctReady);
                    legacySO.ApplyModifiedPropertiesWithoutUndo();
                    changed = true;
                    Debug.Log($"[Builder] {binding.EnemyName}: contact damage {(!distinctReady ? "enabled" : "disabled")} ({reason}).");
                }

                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, binding.PrefabPath);
                    modified++;
                }

                PrefabUtility.UnloadPrefabContents(root);
                Debug.Log($"[Builder] {binding.EnemyName} runtime prefab wired: {binding.PrefabPath}");
            }

            Debug.Log($"[Builder] {modified} runtime prefabs modified.");
        }

        private static void EnsurePlayerCombatHurtbox()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[Builder] Player prefab not found at {PlayerPrefabPath}. Projectile enemy attacks need a player collider/hurtbox.");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            var changed = false;
            var hurtbox = FindDeepChild(root.transform, "CombatHurtbox");
            if (hurtbox == null)
            {
                var hurtboxObject = new GameObject("CombatHurtbox");
                hurtboxObject.transform.SetParent(root.transform, false);
                hurtboxObject.transform.localPosition = new Vector3(0f, 0.9f, 0f);
                hurtboxObject.layer = ResolvePlayerHitLayer();
                hurtbox = hurtboxObject.transform;
                changed = true;
            }

            var capsule = hurtbox.GetComponent<CapsuleCollider>();
            if (capsule == null)
            {
                capsule = hurtbox.gameObject.AddComponent<CapsuleCollider>();
                changed = true;
            }

            capsule.isTrigger = true;
            capsule.radius = 0.55f;
            capsule.height = 1.8f;
            capsule.center = Vector3.zero;
            capsule.direction = 1;
            capsule.enabled = true;
            hurtbox.gameObject.layer = ResolvePlayerHitLayer();
            changed = true;

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
                Debug.Log($"[Builder] Player CombatHurtbox ensured at {PlayerPrefabPath}.");
            }

            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void SetBaseFields(
            EnemyAttackConfig cfg,
            string id,
            string displayName,
            EnemyDistinctAttackType attackType,
            EnemyTelegraphType telegraphType,
            float triggerRange,
            float cooldown,
            float windup,
            float active,
            float recovery,
            float damage,
            float knockback,
            float kbDuration,
            LayerMask hitMask,
            GameObject telegraphPrefab,
            GameObject activeVfxPrefab,
            GameObject impactVfxPrefab)
        {
            cfg.SetPrivate("attackId", id);
            cfg.SetPrivate("displayName", displayName);
            cfg.SetPrivate("attackType", (int)attackType);
            cfg.SetPrivate("telegraphType", (int)telegraphType);
            cfg.SetPrivate("triggerRange", triggerRange);
            cfg.SetPrivate("cooldown", cooldown);
            cfg.SetPrivate("windupTime", windup);
            cfg.SetPrivate("activeTime", active);
            cfg.SetPrivate("recoveryTime", recovery);
            cfg.SetPrivate("damage", damage);
            cfg.SetPrivate("knockbackForce", knockback);
            cfg.SetPrivate("knockbackDuration", kbDuration);
            cfg.SetPrivate("hitLayerMask", hitMask);
            cfg.SetPrivate("telegraphPrefab", telegraphPrefab);
            cfg.SetPrivate("activeVfxPrefab", activeVfxPrefab);
            cfg.SetPrivate("impactVfxPrefab", impactVfxPrefab);
            cfg.SetPrivate("vfxLifetime", 1.25f);
            cfg.SetPrivate("useAnimationTrigger", true);
            cfg.SetPrivate("animationTrigger", "Attack");
        }

        private static GameObject CreateProjectilePrefab(
            string name,
            ProjectileVisualStyle visualStyle,
            Color color,
            bool homing,
            bool spawnsAreaZone,
            GameObject impactVfx,
            string sourceAssetPath)
        {
            var path = $"{ProjectileFolder}/{name}.prefab";
            var root = LoadOrCreatePrefabRoot(path, name, out var existed);
            root.name = name;
            root.transform.localScale = Vector3.one;
            root.layer = ResolvePlayerHitLayer();
            DestroyChildren(root.transform);
            RemovePrimitiveVisualComponents(root, removeCollider: false);

            var collider = root.GetComponent<Collider>();
            if (collider == null)
            {
                collider = root.AddComponent<SphereCollider>();
            }

            collider.isTrigger = true;
            collider.enabled = true;
            if (collider is SphereCollider sphereCollider)
            {
                sphereCollider.radius = 0.28f;
                sphereCollider.center = Vector3.zero;
            }

            var rigidbody = root.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = root.AddComponent<Rigidbody>();
            }

            rigidbody.useGravity = false;
            rigidbody.isKinematic = false;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var projectile = root.GetComponent<EnemyProjectileController>();
            if (projectile == null)
            {
                projectile = root.AddComponent<EnemyProjectileController>();
            }

            var projectileSO = new SerializedObject(projectile);
            SetBool(projectileSO, "deactivateInsteadOfDestroy", true);
            SetLayerMask(projectileSO, "hitLayers", ResolvePlayerHitMask());
            SetFloat(projectileSO, "sweepRadiusOverride", 0.25f);
            SetFloat(projectileSO, "minimumSweepRadius", 0.12f);
            SetObject(projectileSO, "impactVfxPrefab", impactVfx);
            SetFloat(projectileSO, "impactVfxLifetime", 1.25f);
            projectileSO.ApplyModifiedPropertiesWithoutUndo();

            if (homing && root.GetComponent<EnemyHomingProjectile>() == null)
            {
                root.AddComponent<EnemyHomingProjectile>();
            }

            if (spawnsAreaZone && root.GetComponent<ProjectileAreaZoneSpawner>() == null)
            {
                root.AddComponent<ProjectileAreaZoneSpawner>();
            }

            var pooled = root.GetComponent<PooledProjectile>();
            if (pooled == null)
            {
                pooled = root.AddComponent<PooledProjectile>();
            }

            if (pooled == null)
            {
                Debug.LogWarning($"[Builder] Could not add PooledProjectile to {name}.");
            }

            var visual = new GameObject("VisualRoot");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.layer = root.layer;
            AddOrConfigureMarker(
                visual,
                EnemyAttackVFXKind.ProjectileVisual,
                HasSourcePrefab(sourceAssetPath) ? EnemyAttackVFXSourceType.ProjectOwnedWrapper : EnemyAttackVFXSourceType.ProjectOwnedProcedural,
                4f,
                sourceAssetPath,
                $"{name} projectile visual uses particle/trail/line renderers; root collider is invisible.");
            ConfigureProjectileVisual(visual, visualStyle, color, sourceAssetPath);

            SavePrefab(root, path, existed);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static GameObject CreateAreaZonePrefab(
            string name,
            Color color,
            float radius,
            float duration,
            float tickInterval,
            float tickDamage,
            StatusEffectType effectType,
            float effectDuration,
            float slowMultiplier)
        {
            var path = $"{AreaZoneFolder}/{name}.prefab";
            var existed = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
            var root = existed ? PrefabUtility.LoadPrefabContents(path) : new GameObject(name);
            root.name = name;
            root.layer = ResolvePlayerHitLayer();

            var visual = FindDeepChild(root.transform, "VisualRoot");
            if (visual == null)
            {
                var visualObject = new GameObject("VisualRoot");
                visualObject.name = "VisualRoot";
                visualObject.transform.SetParent(root.transform, false);
                visualObject.transform.localPosition = new Vector3(0f, 0.025f, 0f);
                visual = visualObject.transform;
            }

            DestroyChildren(visual);
            RemovePrimitiveVisualComponents(visual.gameObject, removeCollider: true);
            visual.localScale = Vector3.one;
            visual.gameObject.layer = ResolvePlayerHitLayer();
            AddOrConfigureMarker(
                visual.gameObject,
                EnemyAttackVFXKind.AreaZoneVisual,
                EnemyAttackVFXSourceType.ProjectOwnedProcedural,
                duration,
                string.Empty,
                $"{name} ground zone visual scales from EnemyAreaZone radius and expires with the zone.");
            ConfigureAreaZoneVisual(visual.gameObject, name, color, radius, duration);

            var zone = root.GetComponent<EnemyAreaZone>();
            if (zone == null)
            {
                zone = root.AddComponent<EnemyAreaZone>();
            }
            var zoneSO = new SerializedObject(zone);
            SetFloat(zoneSO, "fallbackRadius", radius);
            SetFloat(zoneSO, "fallbackDuration", duration);
            SetFloat(zoneSO, "fallbackTickInterval", tickInterval);
            SetFloat(zoneSO, "fallbackTickDamage", tickDamage);
            SetEnum(zoneSO, "fallbackStatusEffect", (int)effectType);
            SetFloat(zoneSO, "fallbackStatusDuration", effectDuration);
            SetFloat(zoneSO, "fallbackSlowMultiplier", slowMultiplier);
            SetLayerMask(zoneSO, "fallbackHitLayers", ResolvePlayerHitMask());
            SetObject(zoneSO, "visualRoot", visual);
            zoneSO.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, path, existed);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private enum TelegraphVisualShape
        {
            Line,
            Circle,
            Arc
        }

        private enum AttackVfxStyle
        {
            Slash,
            Trail,
            Burst,
            Muzzle,
            Beam,
            Aura,
            Cloud,
            Shockwave,
            Spikes,
            Splash
        }

        private enum ProjectileVisualStyle
        {
            Fireball,
            Spike,
            Slime,
            Curse
        }

        private static GameObject CreateTelegraphPrefab(string name, TelegraphVisualShape shape, Color color, float expectedLifetime)
        {
            var path = $"{TelegraphFolder}/{name}.prefab";
            var root = LoadOrCreatePrefabRoot(path, name, out var existed);
            ResetVisualPrefabRoot(root, name);
            AddOrConfigureMarker(
                root,
                EnemyAttackVFXKind.Telegraph,
                EnemyAttackVFXSourceType.ProjectOwnedProcedural,
                expectedLifetime,
                string.Empty,
                $"{name} procedural LineRenderer telegraph; no primitive mesh renderers.");

            var line = AddLineRenderer(root.transform, "TelegraphLine", $"{name}_Line_MAT", color, 0.08f);
            ConfigureTelegraphLine(line, shape);
            AddParticleSystem(root.transform, "TelegraphEdgeSparks", color, expectedLifetime, false, 0.22f, 0.25f, 0.055f, 10, 0f, ParticleSystemShapeType.Circle, 0.5f, Vector3.zero);

            SavePrefab(root, path, existed);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static GameObject CreateTimedVfxPrefab(
            string name,
            AttackVfxStyle style,
            EnemyAttackVFXKind kind,
            Color color,
            float lifetime,
            string sourceAssetPath,
            float sourceScale)
        {
            var path = $"{VfxFolder}/{name}.prefab";
            var root = LoadOrCreatePrefabRoot(path, name, out var existed);
            ResetVisualPrefabRoot(root, name);

            var hasSource = HasSourcePrefab(sourceAssetPath);
            AddOrConfigureMarker(
                root,
                kind,
                hasSource ? EnemyAttackVFXSourceType.ProjectOwnedWrapper : EnemyAttackVFXSourceType.ProjectOwnedProcedural,
                lifetime,
                hasSource ? sourceAssetPath : string.Empty,
                hasSource
                    ? $"{name} wraps an imported source effect and adds project-owned timing/readability layers."
                    : $"{name} is a project-owned procedural fallback using particles/trails/lines.");
            AddOrConfigureCleanup(root, lifetime);
            ConfigureTimedVfx(root, name, style, color, lifetime);
            TryAddSourcePrefabChild(root.transform, sourceAssetPath, sourceScale);

            SavePrefab(root, path, existed);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static void ConfigureTimedVfx(GameObject root, string name, AttackVfxStyle style, Color color, float lifetime)
        {
            switch (style)
            {
                case AttackVfxStyle.Slash:
                    ConfigureArcLine(AddLineRenderer(root.transform, "SlashArc", $"{name}_Slash_MAT", color, 0.16f), 115f, 0.85f);
                    AddParticleSystem(root.transform, "SlashSparks", color, lifetime, false, 0.28f, 1.2f, 0.08f, 20, 0f, ParticleSystemShapeType.Cone, 0.3f, Vector3.forward * 0.45f);
                    break;

                case AttackVfxStyle.Trail:
                    AddTrailRenderer(root.transform, "MotionTrail", $"{name}_Trail_MAT", color, 0.35f, 0.34f, 0.03f, Vector3.back * 0.35f);
                    AddParticleSystem(root.transform, "TrailStreaks", color, lifetime, true, 0.22f, 0.65f, 0.055f, 0, 20f, ParticleSystemShapeType.Cone, 0.18f, Vector3.back * 0.2f);
                    break;

                case AttackVfxStyle.Beam:
                    ConfigureLine(AddLineRenderer(root.transform, "BeamCore", $"{name}_Beam_MAT", color, 0.22f), Vector3.zero, Vector3.forward * 8f);
                    AddTrailRenderer(root.transform, "BeamAfterglow", $"{name}_Afterglow_MAT", color, 0.18f, 0.16f, 0.01f, Vector3.forward * 0.25f);
                    AddParticleSystem(root.transform, "BeamSparks", color, lifetime, false, 0.24f, 1.5f, 0.07f, 18, 0f, ParticleSystemShapeType.Cone, 0.2f, Vector3.forward * 2f);
                    break;

                case AttackVfxStyle.Aura:
                    AddParticleSystem(root.transform, "PhaseAura", color, lifetime, true, 0.45f, 0.18f, 0.16f, 0, 18f, ParticleSystemShapeType.Sphere, 0.65f, Vector3.zero);
                    ConfigureCircleLine(AddLineRenderer(root.transform, "AuraRing", $"{name}_Ring_MAT", color, 0.07f), 0.65f);
                    break;

                case AttackVfxStyle.Cloud:
                    AddParticleSystem(root.transform, "SporeCloud", color, lifetime, true, 0.75f, 0.22f, 0.18f, 0, 16f, ParticleSystemShapeType.Sphere, 0.75f, Vector3.up * 0.25f);
                    ConfigureCircleLine(AddLineRenderer(root.transform, "PoisonRing", $"{name}_Ring_MAT", color, 0.06f), 0.8f);
                    break;

                case AttackVfxStyle.Shockwave:
                    ConfigureCircleLine(AddLineRenderer(root.transform, "ShockwaveRing", $"{name}_Ring_MAT", color, 0.14f), 1f);
                    AddParticleSystem(root.transform, "ShockwaveBurst", color, lifetime, false, 0.38f, 2.1f, 0.09f, 34, 0f, ParticleSystemShapeType.Circle, 0.65f, Vector3.zero);
                    break;

                case AttackVfxStyle.Spikes:
                    ConfigureCircleLine(AddLineRenderer(root.transform, "SpikeWarningRing", $"{name}_Ring_MAT", color, 0.09f), 1f);
                    AddParticleSystem(root.transform, "RadialSpikeBurst", color, lifetime, false, 0.38f, 2.4f, 0.075f, 42, 0f, ParticleSystemShapeType.Circle, 0.9f, Vector3.zero);
                    break;

                case AttackVfxStyle.Splash:
                    AddParticleSystem(root.transform, "SplashDroplets", color, lifetime, false, 0.45f, 1.6f, 0.11f, 26, 0f, ParticleSystemShapeType.Cone, 0.35f, Vector3.up * 0.05f);
                    ConfigureCircleLine(AddLineRenderer(root.transform, "SplashRing", $"{name}_Ring_MAT", color, 0.08f), 0.65f);
                    break;

                case AttackVfxStyle.Burst:
                case AttackVfxStyle.Muzzle:
                default:
                    AddParticleSystem(root.transform, "CoreBurst", color, lifetime, false, 0.32f, 1.5f, 0.1f, 22, 0f, ParticleSystemShapeType.Sphere, 0.28f, Vector3.zero);
                    ConfigureCircleLine(AddLineRenderer(root.transform, "BurstRing", $"{name}_Ring_MAT", color, 0.075f), 0.45f);
                    break;
            }
        }

        private static void ConfigureProjectileVisual(GameObject visualRoot, ProjectileVisualStyle style, Color color, string sourceAssetPath)
        {
            switch (style)
            {
                case ProjectileVisualStyle.Fireball:
                    AddParticleSystem(visualRoot.transform, "FireCore", color, 1.5f, true, 0.28f, 0.2f, 0.18f, 0, 28f, ParticleSystemShapeType.Sphere, 0.18f, Vector3.zero);
                    AddTrailRenderer(visualRoot.transform, "FireTrail", $"{visualRoot.name}_FireTrail_MAT", color, 0.34f, 0.22f, 0.04f, Vector3.back * 0.25f);
                    TryAddSourcePrefabChild(visualRoot.transform, sourceAssetPath, 0.28f);
                    break;

                case ProjectileVisualStyle.Spike:
                    ConfigureLine(AddLineRenderer(visualRoot.transform, "SpikeDart", $"{visualRoot.name}_Spike_MAT", color, 0.13f), Vector3.back * 0.42f, Vector3.forward * 0.42f);
                    AddTrailRenderer(visualRoot.transform, "LeafTrail", $"{visualRoot.name}_Trail_MAT", color, 0.25f, 0.14f, 0.015f, Vector3.back * 0.25f);
                    AddParticleSystem(visualRoot.transform, "NeedleDust", color, 1.2f, true, 0.18f, 0.35f, 0.05f, 0, 10f, ParticleSystemShapeType.Cone, 0.08f, Vector3.back * 0.15f);
                    break;

                case ProjectileVisualStyle.Slime:
                    AddParticleSystem(visualRoot.transform, "SlimeCore", color, 1.4f, true, 0.35f, 0.12f, 0.18f, 0, 24f, ParticleSystemShapeType.Sphere, 0.22f, Vector3.zero);
                    AddTrailRenderer(visualRoot.transform, "GooTrail", $"{visualRoot.name}_Trail_MAT", color, 0.4f, 0.2f, 0.03f, Vector3.back * 0.2f);
                    break;

                case ProjectileVisualStyle.Curse:
                    AddParticleSystem(visualRoot.transform, "CurseOrb", color, 1.5f, true, 0.4f, 0.18f, 0.16f, 0, 18f, ParticleSystemShapeType.Sphere, 0.22f, Vector3.zero);
                    AddTrailRenderer(visualRoot.transform, "CurseTrail", $"{visualRoot.name}_Trail_MAT", color, 0.5f, 0.18f, 0.02f, Vector3.back * 0.25f);
                    ConfigureCircleLine(AddLineRenderer(visualRoot.transform, "CurseHalo", $"{visualRoot.name}_Halo_MAT", color, 0.035f), 0.24f);
                    break;
            }
        }

        private static void ConfigureAreaZoneVisual(GameObject visualRoot, string name, Color color, float radius, float duration)
        {
            ConfigureCircleLine(AddLineRenderer(visualRoot.transform, "DangerRing", $"{name}_Ring_MAT", color, 0.065f), 0.5f);

            var isFrost = Contains(name, "Frost") || Contains(name, "Yeti");
            var isPoison = Contains(name, "Mushroom") || Contains(name, "Poison");
            var particleName = isFrost ? "FrostMotes" : isPoison ? "SporeMotes" : "SlimeBubbles";
            var speed = isFrost ? 0.55f : 0.25f;
            var size = isFrost ? 0.075f : 0.12f;
            AddParticleSystem(visualRoot.transform, particleName, color, duration, true, 0.75f, speed, size, 0, 18f, ParticleSystemShapeType.Circle, 0.45f, Vector3.up * 0.04f);

            if (radius > 2.5f)
            {
                AddParticleSystem(visualRoot.transform, "OuterReadableEdge", color, duration, true, 0.55f, 0.2f, 0.06f, 0, 12f, ParticleSystemShapeType.Circle, 0.5f, Vector3.up * 0.03f);
            }
        }

        private static LineRenderer AddLineRenderer(Transform parent, string name, string materialName, Color color, float width)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.layer = parent.gameObject.layer;

            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.sharedMaterial = GetOrCreateMaterial(materialName, color);
            line.startColor = color;
            line.endColor = color;
            line.widthMultiplier = width;
            line.numCapVertices = 4;
            line.numCornerVertices = 4;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            return line;
        }

        private static void ConfigureTelegraphLine(LineRenderer line, TelegraphVisualShape shape)
        {
            switch (shape)
            {
                case TelegraphVisualShape.Line:
                    ConfigureLine(line, Vector3.zero, Vector3.forward);
                    break;
                case TelegraphVisualShape.Arc:
                    ConfigureArcLine(line, 130f, 0.5f);
                    break;
                case TelegraphVisualShape.Circle:
                default:
                    ConfigureCircleLine(line, 0.5f);
                    break;
            }
        }

        private static TelegraphVisualShape ResolveTelegraphShape(EnemyTelegraphType telegraphType)
        {
            switch (telegraphType)
            {
                case EnemyTelegraphType.Line:
                case EnemyTelegraphType.ChargePath:
                    return TelegraphVisualShape.Line;
                case EnemyTelegraphType.Cone:
                    return TelegraphVisualShape.Arc;
                default:
                    return TelegraphVisualShape.Circle;
            }
        }

        private static void ConfigureLine(LineRenderer line, Vector3 start, Vector3 end)
        {
            line.loop = false;
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
        }

        private static void ConfigureCircleLine(LineRenderer line, float radius)
        {
            const int segmentCount = 64;
            line.loop = true;
            line.positionCount = segmentCount;
            for (var i = 0; i < segmentCount; i++)
            {
                var angle = Mathf.PI * 2f * (i / (float)segmentCount);
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
            }
        }

        private static void ConfigureArcLine(LineRenderer line, float arcDegrees, float radius)
        {
            const int segmentCount = 32;
            line.loop = false;
            line.positionCount = segmentCount + 1;
            var start = -arcDegrees * 0.5f;
            for (var i = 0; i <= segmentCount; i++)
            {
                var angle = (start + arcDegrees * (i / (float)segmentCount)) * Mathf.Deg2Rad;
                line.SetPosition(i, new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius));
            }
        }

        private static TrailRenderer AddTrailRenderer(
            Transform parent,
            string name,
            string materialName,
            Color color,
            float time,
            float startWidth,
            float endWidth,
            Vector3 localPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.layer = parent.gameObject.layer;

            var trail = go.AddComponent<TrailRenderer>();
            trail.time = time;
            trail.startWidth = startWidth;
            trail.endWidth = endWidth;
            trail.sharedMaterial = GetOrCreateMaterial(materialName, color);
            trail.autodestruct = false;
            trail.emitting = true;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trail.receiveShadows = false;
            return trail;
        }

        private static ParticleSystem AddParticleSystem(
            Transform parent,
            string name,
            Color color,
            float duration,
            bool loop,
            float lifetime,
            float speed,
            float size,
            int burstCount,
            float rateOverTime,
            ParticleSystemShapeType shapeType,
            float radius,
            Vector3 localPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.layer = parent.gameObject.layer;

            var particleSystem = go.AddComponent<ParticleSystem>();
            var main = particleSystem.main;
            main.duration = Mathf.Max(0.05f, duration);
            main.loop = loop;
            main.startLifetime = Mathf.Max(0.05f, lifetime);
            main.startSpeed = speed;
            main.startSize = size;
            main.startColor = color;
            main.maxParticles = Mathf.Max(32, burstCount * 2 + Mathf.CeilToInt(rateOverTime * duration) + 8);
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            var emission = particleSystem.emission;
            emission.enabled = true;
            emission.rateOverTime = rateOverTime;
            if (burstCount > 0)
            {
                emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)Mathf.Clamp(burstCount, 1, short.MaxValue)) });
            }

            var shape = particleSystem.shape;
            shape.enabled = true;
            shape.shapeType = shapeType;
            shape.radius = Mathf.Max(0.01f, radius);
            if (shapeType == ParticleSystemShapeType.Cone)
            {
                shape.angle = 22f;
            }

            var colorOverLifetime = particleSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(color, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(color.a, 0f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = gradient;

            var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = GetOrCreateMaterial($"{name}_Particle_MAT", color);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return particleSystem;
        }

        private static void TryAddSourcePrefabChild(Transform parent, string sourceAssetPath, float sourceScale)
        {
            if (!HasSourcePrefab(sourceAssetPath))
            {
                return;
            }

            var source = AssetDatabase.LoadAssetAtPath<GameObject>(sourceAssetPath);
            var instance = PrefabUtility.InstantiatePrefab(source) as GameObject;
            if (instance == null)
            {
                return;
            }

            instance.name = $"SourceVFX_{source.name}";
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one * Mathf.Max(0.01f, sourceScale);
            SetLayerRecursively(instance, parent.gameObject.layer);
            StripCollidersFromHierarchy(instance);
        }

        private static GameObject LoadOrCreatePrefabRoot(string path, string name, out bool existed)
        {
            existed = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
            var root = existed ? PrefabUtility.LoadPrefabContents(path) : new GameObject(name);
            root.name = name;
            return root;
        }

        private static void ResetVisualPrefabRoot(GameObject root, string name)
        {
            root.name = name;
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
            root.layer = ResolvePlayerHitLayer();
            DestroyChildren(root.transform);
            RemovePrimitiveVisualComponents(root, removeCollider: true);
        }

        private static void AddOrConfigureMarker(
            GameObject target,
            EnemyAttackVFXKind kind,
            EnemyAttackVFXSourceType sourceType,
            float expectedLifetime,
            string sourceAssetPath,
            string notes)
        {
            var marker = target.GetComponent<EnemyAttackVFXMarker>();
            if (marker == null)
            {
                marker = target.AddComponent<EnemyAttackVFXMarker>();
            }

            var so = new SerializedObject(marker);
            SetEnum(so, "kind", (int)kind);
            SetEnum(so, "sourceType", (int)sourceType);
            SetBool(so, "productionReady", true);
            SetBool(so, "placeholder", false);
            SetFloat(so, "expectedLifetimeSeconds", expectedLifetime);
            SetString(so, "sourceAssetPath", sourceAssetPath);
            SetString(so, "notes", notes);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddOrConfigureCleanup(GameObject root, float lifetime)
        {
            var cleanup = root.GetComponent<EnemyAttackVFXAutoCleanup>();
            if (cleanup == null)
            {
                cleanup = root.AddComponent<EnemyAttackVFXAutoCleanup>();
            }

            var so = new SerializedObject(cleanup);
            SetFloat(so, "lifetimeSeconds", lifetime);
            SetBool(so, "deactivateInsteadOfDestroy", false);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static bool HasSourcePrefab(string sourceAssetPath)
        {
            return !string.IsNullOrWhiteSpace(sourceAssetPath) &&
                AssetDatabase.LoadAssetAtPath<GameObject>(sourceAssetPath) != null;
        }

        private static void EnsureTelegraphController(GameObject root, EnemyAttackConfig primaryConfig, ref bool changed)
        {
            var telegraphController = root.GetComponent<EnemyTelegraphController>();
            if (telegraphController == null)
            {
                telegraphController = root.AddComponent<EnemyTelegraphController>();
                changed = true;
            }

            var telegraphRoot = FindDeepChild(root.transform, "TelegraphRoot");
            if (telegraphRoot == null)
            {
                var telegraphObject = new GameObject("TelegraphRoot");
                telegraphObject.transform.SetParent(root.transform, false);
                telegraphObject.transform.localPosition = new Vector3(0f, 0.04f, 0f);
                telegraphObject.transform.localScale = Vector3.one;
                telegraphRoot = telegraphObject.transform;
                changed = true;
            }

            DestroyChildren(telegraphRoot);
            RemovePrimitiveVisualComponents(telegraphRoot.gameObject, removeCollider: true);
            telegraphRoot.gameObject.layer = root.layer;

            var lineRenderer = telegraphRoot.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = telegraphRoot.gameObject.AddComponent<LineRenderer>();
                changed = true;
            }

            var color = primaryConfig != null ? primaryConfig.DebugColor : Color.red;
            lineRenderer.useWorldSpace = false;
            lineRenderer.sharedMaterial = GetOrCreateMaterial("MAT_EnemyAttackTelegraph_Runtime", color);
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.widthMultiplier = 0.08f;
            lineRenderer.numCapVertices = 4;
            lineRenderer.numCornerVertices = 4;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            ConfigureTelegraphLine(lineRenderer, primaryConfig != null ? ResolveTelegraphShape(primaryConfig.TelegraphType) : TelegraphVisualShape.Circle);
            changed = true;

            AddOrConfigureMarker(
                telegraphRoot.gameObject,
                EnemyAttackVFXKind.Telegraph,
                EnemyAttackVFXSourceType.ProjectOwnedProcedural,
                primaryConfig != null ? primaryConfig.WindupTime : 0.75f,
                string.Empty,
                "Runtime enemy telegraph root generated by builder as LineRenderer, not a mesh primitive.");

            telegraphRoot.gameObject.SetActive(false);

            var so = new SerializedObject(telegraphController);
            SetObject(so, "telegraphRoot", telegraphRoot);
            SetObject(so, "telegraphRenderer", lineRenderer);
            SetObject(so, "telegraphLineRenderer", lineRenderer);
            SetColor(so, "windupColor", color);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Transform EnsureChildTransform(GameObject root, string childName, Vector3 localPosition, ref bool changed)
        {
            var child = FindDeepChild(root.transform, childName);
            if (child != null)
            {
                return child;
            }

            var go = new GameObject(childName);
            go.transform.SetParent(root.transform, false);
            go.transform.localPosition = localPosition;
            go.layer = root.layer;
            changed = true;
            return go.transform;
        }

        private static Transform FindDeepChild(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child.name == childName)
                {
                    return child;
                }

                var nested = FindDeepChild(child, childName);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }

        private static void AddFallbackBinding(
            Dictionary<string, EnemyRuntimeBinding> byEnemy,
            string enemyName,
            string prefabName,
            Dictionary<string, EnemyAttackConfig[]> configByEnemy)
        {
            if (byEnemy.ContainsKey(enemyName))
            {
                return;
            }

            var prefabPath = $"{PrefabFolder}/{prefabName}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null || !configByEnemy.TryGetValue(enemyName, out var configs))
            {
                return;
            }

            byEnemy[enemyName] = new EnemyRuntimeBinding(enemyName, prefab, prefabPath, configs, "fallback path");
        }

        private static string ResolveEnemyName(SpawnGroupConfig group)
        {
            var text = $"{group.name} {group.GroupId} {group.EnemyPrefab.name} {(group.EnemyConfig != null ? group.EnemyConfig.name : string.Empty)}";
            if (Contains(text, "YellowDragon")) return "YellowDragon";
            if (Contains(text, "GreenDemon")) return "GreenDemon";
            if (Contains(text, "Cthulhu")) return "Cthulhu";
            if (Contains(text, "Cyclops")) return "Cyclops";
            if (Contains(text, "Mushroom")) return "Mushroom";
            if (Contains(text, "Cactus")) return "Cactus";
            if (Contains(text, "Ghost")) return "Ghost";
            if (Contains(text, "Yeti")) return "Yeti";
            if (Contains(text, "Bee")) return "Bee";
            if (Contains(text, "Bat")) return "Bat";
            if (Contains(text, "Demon")) return "Demon";
            return string.Empty;
        }

        private static bool Contains(string source, string value)
        {
            return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static LayerMask ResolvePlayerHitMask()
        {
            var mask = 1 << ResolvePlayerHitLayer();
            var playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0)
            {
                mask |= 1 << playerLayer;
            }

            return mask;
        }

        private static int ResolvePlayerHitLayer()
        {
            var playerLayer = LayerMask.NameToLayer("Player");
            return playerLayer >= 0 ? playerLayer : 0;
        }

        private static T CreateOrLoad<T>(string folder, string assetName) where T : ScriptableObject
        {
            var path = $"{folder}/{assetName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                return existing;
            }

            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static Material GetOrCreateMaterial(string materialName, Color color)
        {
            var path = $"{MaterialFolder}/{materialName}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(
                    Shader.Find("Universal Render Pipeline/Particles/Unlit") ??
                    Shader.Find("Universal Render Pipeline/Unlit") ??
                    Shader.Find("Sprites/Default") ??
                    Shader.Find("Standard"))
                {
                    name = materialName
                };
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
            }

            if (material.HasProperty("_Blend"))
            {
                material.SetFloat("_Blend", 0f);
            }

            if (material.HasProperty("_ZWrite"))
            {
                material.SetFloat("_ZWrite", 0f);
            }

            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_ALPHABLEND_ON");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void SavePrefab(GameObject root, string path, bool existed)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            if (existed)
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            else
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void RemoveCollider(GameObject go)
        {
            var collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }
        }

        private static void RemovePrimitiveVisualComponents(GameObject go, bool removeCollider)
        {
            RemoveComponentIfPresent<MeshRenderer>(go);
            RemoveComponentIfPresent<MeshFilter>(go);
            RemoveComponentIfPresent<ParticleSystemRenderer>(go);
            RemoveComponentIfPresent<ParticleSystem>(go);
            RemoveComponentIfPresent<TrailRenderer>(go);
            RemoveComponentIfPresent<LineRenderer>(go);
            if (removeCollider)
            {
                RemoveCollider(go);
            }
        }

        private static void RemoveComponentIfPresent<T>(GameObject go) where T : Component
        {
            var component = go.GetComponent<T>();
            if (component != null)
            {
                Object.DestroyImmediate(component);
            }
        }

        private static void DestroyChildren(Transform parent)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
            }
        }

        private static void StripCollidersFromHierarchy(GameObject root)
        {
            var colliders = root.GetComponentsInChildren<Collider>(true);
            for (var i = 0; i < colliders.Length; i++)
            {
                Object.DestroyImmediate(colliders[i]);
            }
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            root.layer = layer;
            for (var i = 0; i < root.transform.childCount; i++)
            {
                SetLayerRecursively(root.transform.GetChild(i).gameObject, layer);
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parts = path.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static void SetArray(SerializedObject serializedObject, string fieldName, EnemyAttackConfig[] values)
        {
            var prop = serializedObject.FindProperty(fieldName);
            if (prop == null)
            {
                return;
            }

            prop.arraySize = values != null ? values.Length : 0;
            for (var i = 0; i < prop.arraySize; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static void SetObject(SerializedObject serializedObject, string fieldName, Object value)
        {
            var prop = serializedObject.FindProperty(fieldName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
            }
        }

        private static void SetBool(SerializedObject serializedObject, string fieldName, bool value)
        {
            var prop = serializedObject.FindProperty(fieldName);
            if (prop != null)
            {
                prop.boolValue = value;
            }
        }

        private static void SetFloat(SerializedObject serializedObject, string fieldName, float value)
        {
            var prop = serializedObject.FindProperty(fieldName);
            if (prop != null)
            {
                prop.floatValue = value;
            }
        }

        private static void SetEnum(SerializedObject serializedObject, string fieldName, int value)
        {
            var prop = serializedObject.FindProperty(fieldName);
            if (prop != null)
            {
                prop.enumValueIndex = value;
            }
        }

        private static void SetColor(SerializedObject serializedObject, string fieldName, Color value)
        {
            var prop = serializedObject.FindProperty(fieldName);
            if (prop != null)
            {
                prop.colorValue = value;
            }
        }

        private static void SetString(SerializedObject serializedObject, string fieldName, string value)
        {
            var prop = serializedObject.FindProperty(fieldName);
            if (prop != null)
            {
                prop.stringValue = value;
            }
        }

        private static void SetLayerMask(SerializedObject serializedObject, string fieldName, LayerMask value)
        {
            var prop = serializedObject.FindProperty(fieldName);
            if (prop != null)
            {
                prop.intValue = value.value;
            }
        }

        private readonly struct EnemyRuntimeBinding
        {
            public EnemyRuntimeBinding(string enemyName, GameObject prefab, string prefabPath, EnemyAttackConfig[] configs, string sourcePath)
            {
                EnemyName = enemyName;
                Prefab = prefab;
                PrefabPath = prefabPath;
                Configs = configs;
                SourcePath = sourcePath;
            }

            public string EnemyName { get; }
            public GameObject Prefab { get; }
            public string PrefabPath { get; }
            public EnemyAttackConfig[] Configs { get; }
            public string SourcePath { get; }
        }

        private sealed class BuilderConfigs
        {
            public EnemyAttackConfig GreenDemonMelee;
            public EnemyAttackConfig BatDive;
            public EnemyAttackConfig BeeCharge;
            public EnemyAttackConfig YellowDragonFireball;
            public EnemyAttackConfig CactusSpike;
            public EnemyAttackConfig CactusRadial;
            public EnemyAttackConfig CthulhuSlime;
            public EnemyAttackConfig CyclopsBeam;
            public EnemyAttackConfig DemonLeap;
            public EnemyAttackConfig GhostHoming;
            public EnemyAttackConfig MushroomSpore;
            public EnemyAttackConfig YetiFrost;

            public EnemyAttackConfig[] AllConfigs => new[]
            {
                GreenDemonMelee,
                BatDive,
                BeeCharge,
                YellowDragonFireball,
                CactusSpike,
                CactusRadial,
                CthulhuSlime,
                CyclopsBeam,
                DemonLeap,
                GhostHoming,
                MushroomSpore,
                YetiFrost
            };
        }

        private sealed class BuilderAssets
        {
            public GameObject FireballProjectile;
            public GameObject SpikeProjectile;
            public GameObject SlimeProjectile;
            public GameObject CurseProjectile;
            public GameObject CthulhuSlowPool;
            public GameObject MushroomSporeZone;
            public GameObject YetiFrostZone;
            public GameObject GreenDemonTelegraph;
            public GameObject BatTelegraph;
            public GameObject BeeTelegraph;
            public GameObject CactusTelegraph;
            public GameObject CyclopsTelegraph;
            public GameObject DemonTelegraph;
            public GameObject MushroomTelegraph;
            public GameObject YetiTelegraph;
            public GameObject LineTelegraph;
            public GameObject MeleeSlashVfx;
            public GameObject MeleeImpactVfx;
            public GameObject DiveTrailVfx;
            public GameObject DiveImpactVfx;
            public GameObject ChargeTrailVfx;
            public GameObject StingImpactVfx;
            public GameObject FireMuzzleVfx;
            public GameObject FireImpactVfx;
            public GameObject SpikeVfx;
            public GameObject SpikeImpactVfx;
            public GameObject RadialSpikeBurstVfx;
            public GameObject SlimeOrbVfx;
            public GameObject SlimeSplashVfx;
            public GameObject EyeBeamVfx;
            public GameObject EyeBeamImpactVfx;
            public GameObject LeapTrailVfx;
            public GameObject LeapImpactVfx;
            public GameObject GhostPhaseVfx;
            public GameObject GhostCurseImpactVfx;
            public GameObject SporeBurstVfx;
            public GameObject SporeCloudVfx;
            public GameObject FrostSlamImpactVfx;
            public GameObject FrostShockwaveVfx;
        }
    }

    internal static class ScriptableObjectEditorExtensions
    {
        public static void SetPrivate(this ScriptableObject so, string fieldName, object value)
        {
            var serializedObject = new SerializedObject(so);
            var prop = serializedObject.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"[Builder] Field '{fieldName}' not found on {so.GetType().Name}");
                return;
            }

            switch (prop.propertyType)
            {
                case SerializedPropertyType.String:
                    prop.stringValue = (string)value;
                    break;
                case SerializedPropertyType.Float:
                    prop.floatValue = (float)value;
                    break;
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.LayerMask:
                    prop.intValue = value is LayerMask layerMask ? layerMask.value : (int)value;
                    break;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = (bool)value;
                    break;
                case SerializedPropertyType.Enum:
                    prop.enumValueIndex = (int)value;
                    break;
                case SerializedPropertyType.Color:
                    prop.colorValue = (Color)value;
                    break;
                case SerializedPropertyType.ObjectReference:
                    prop.objectReferenceValue = (Object)value;
                    break;
                default:
                    Debug.LogWarning($"[Builder] Unsupported property type {prop.propertyType} for '{fieldName}'");
                    break;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
