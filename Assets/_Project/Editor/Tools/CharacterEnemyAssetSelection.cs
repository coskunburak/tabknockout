using TapKnockout.Characters;
using UnityEngine;

namespace TapKnockout.Editor.Tools
{
    internal static class CharacterEnemyAssetSelection
    {
        public const string ReportPath = "Assets/_Project/Docs/CharacterEnemyAnimationIntegrationReport.md";
        public const string BacklogPath = "Assets/_Project/Docs/EnemyVarietyImplementationBacklog.md";
        public const string BuilderReportPath = "Assets/_Project/Docs/CharacterEnemyPrefabBuilderReport.md";
        public const string GeneratedBasicMeleeEnemyPrefabPath = "Assets/_Project/Prefabs/Enemies/Generated/PF_Enemy_BasicMelee_GreenDemon_Generated.prefab";

        public static readonly CharacterEnemyAssetSpec Player = new CharacterEnemyAssetSpec(
            CharacterEnemyRoleId.MainPlayer,
            "Player Ranger",
            "Assets/_Project/Prefabs/Player/Player.prefab",
            "Assets/Assets/game asset packs/RPG Characters - Nov 2020/FBX/Ranger.fbx",
            "Assets/_Project/Prefabs/Player/Player.prefab",
            false,
            0.95f,
            "A",
            "Bow-bearing Ranger FBX source keeps the mesh, built-in weapon, and native Generic animation clips aligned while dash-impact remains the core identity.");

        public static readonly CharacterEnemyAssetSpec[] Enemies =
        {
            new CharacterEnemyAssetSpec(
                CharacterEnemyRoleId.BasicMelee,
                "Basic Melee Green Demon",
                string.Empty,
                "Assets/Assets/game asset packs/Cute Animated Monsters - Aug 2020/FBX/GreenDemon.fbx",
                GeneratedBasicMeleeEnemyPrefabPath,
                false,
                0.95f,
                "A",
                "Medium readable silhouette for all current melee chaser waves. Builder creates the runtime enemy skeleton directly, without the legacy capsule prefab.")
        };
    }

    internal readonly struct CharacterEnemyAssetSpec
    {
        public CharacterEnemyAssetSpec(
            CharacterEnemyRoleId roleId,
            string displayName,
            string basePrefabPath,
            string visualAssetPath,
            string generatedPrefabPath,
            bool requiresProjectileSpawnPoint,
            float visualScale,
            string score,
            string rationale)
            : this(
                roleId,
                displayName,
                basePrefabPath,
                visualAssetPath,
                generatedPrefabPath,
                requiresProjectileSpawnPoint,
                visualScale,
                score,
                rationale,
                CharacterEnemyWeaponAttachmentSpec.None)
        {
        }

        public CharacterEnemyAssetSpec(
            CharacterEnemyRoleId roleId,
            string displayName,
            string basePrefabPath,
            string visualAssetPath,
            string generatedPrefabPath,
            bool requiresProjectileSpawnPoint,
            float visualScale,
            string score,
            string rationale,
            CharacterEnemyWeaponAttachmentSpec heldWeapon)
        {
            RoleId = roleId;
            DisplayName = displayName;
            BasePrefabPath = basePrefabPath;
            VisualAssetPath = visualAssetPath;
            GeneratedPrefabPath = generatedPrefabPath;
            RequiresProjectileSpawnPoint = requiresProjectileSpawnPoint;
            VisualScale = visualScale;
            Score = score;
            Rationale = rationale;
            HeldWeapon = heldWeapon;
        }

        public CharacterEnemyRoleId RoleId { get; }
        public string DisplayName { get; }
        public string BasePrefabPath { get; }
        public string VisualAssetPath { get; }
        public string GeneratedPrefabPath { get; }
        public bool RequiresProjectileSpawnPoint { get; }
        public float VisualScale { get; }
        public string Score { get; }
        public string Rationale { get; }
        public CharacterEnemyWeaponAttachmentSpec HeldWeapon { get; }
    }

    internal readonly struct CharacterEnemyWeaponAttachmentSpec
    {
        public static readonly CharacterEnemyWeaponAttachmentSpec None = new CharacterEnemyWeaponAttachmentSpec(
            string.Empty,
            string.Empty,
            Vector3.zero,
            Vector3.zero,
            Vector3.one);

        public CharacterEnemyWeaponAttachmentSpec(
            string assetPath,
            string socketName,
            Vector3 localPosition,
            Vector3 localEulerAngles,
            Vector3 localScale)
        {
            AssetPath = assetPath;
            SocketName = socketName;
            LocalPosition = localPosition;
            LocalEulerAngles = localEulerAngles;
            LocalScale = localScale;
        }

        public string AssetPath { get; }
        public string SocketName { get; }
        public Vector3 LocalPosition { get; }
        public Vector3 LocalEulerAngles { get; }
        public Vector3 LocalScale { get; }
        public bool HasAsset => !string.IsNullOrWhiteSpace(AssetPath);
    }
}
