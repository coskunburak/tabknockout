namespace TapKnockout.Combat
{
    public interface IPoolLifecycle
    {
        void OnBeforeSpawnFromPool();
        void OnSpawnedFromPool();
        void OnBeforeDespawnToPool();
        void ResetForPool();
    }
}
