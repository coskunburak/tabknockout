using UnityEngine;

namespace TapKnockout.Enemy
{
    public enum EnemyAttackVFXKind
    {
        Telegraph = 0,
        Active = 1,
        Impact = 2,
        ProjectileVisual = 3,
        AreaZoneVisual = 4
    }

    public enum EnemyAttackVFXSourceType
    {
        ProjectOwnedProcedural = 0,
        ProjectOwnedWrapper = 1,
        ExistingProjectAsset = 2
    }

    [DisallowMultipleComponent]
    public sealed class EnemyAttackVFXMarker : MonoBehaviour
    {
        [SerializeField] private EnemyAttackVFXKind kind;
        [SerializeField] private EnemyAttackVFXSourceType sourceType = EnemyAttackVFXSourceType.ProjectOwnedProcedural;
        [SerializeField] private bool productionReady = true;
        [SerializeField] private bool placeholder;
        [SerializeField, Min(0f)] private float expectedLifetimeSeconds = 1.25f;
        [SerializeField] private string sourceAssetPath;
        [SerializeField] private string notes;

        public EnemyAttackVFXKind Kind => kind;
        public EnemyAttackVFXSourceType SourceType => sourceType;
        public bool ProductionReady => productionReady;
        public bool Placeholder => placeholder;
        public float ExpectedLifetimeSeconds => expectedLifetimeSeconds;
        public string SourceAssetPath => sourceAssetPath;
        public string Notes => notes;

        public bool IsProductionReady => productionReady && !placeholder;
    }
}
