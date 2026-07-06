using TapKnockout.Combat;
using TapKnockout.Enemy;
using UnityEngine;

namespace TapKnockout.Boss
{
    [DisallowMultipleComponent]
    public sealed class BossRuntimeBindingBridge : MonoBehaviour, IEnemyRuntimeConfigReceiver, IPoolLifecycle
    {
        [SerializeField] private BossConfig bossConfig;
        [SerializeField] private BossPhaseController phaseController;
        [SerializeField] private BossPatternController patternController;
        [SerializeField] private BossAddSpawnAction addSpawnAction;
        [SerializeField] private BossIntroController introController;
        [SerializeField] private bool playIntroOnInitialize = true;

        private bool hasInitialized;

        private void Reset()
        {
            ResolveReferences();
        }

        private void Awake()
        {
            ResolveReferences();
        }

        public void Initialize(EnemyConfig enemyConfig, Transform runtimeTarget)
        {
            ResolveReferences();
            patternController?.SetTarget(runtimeTarget);
            addSpawnAction?.Initialize(bossConfig);
            phaseController?.Initialize(bossConfig);

            if (playIntroOnInitialize && !hasInitialized)
            {
                introController?.PlayIntro();
            }

            hasInitialized = true;
        }

        public void OnBeforeSpawnFromPool()
        {
            hasInitialized = false;
        }

        public void OnSpawnedFromPool()
        {
        }

        public void OnBeforeDespawnToPool()
        {
            hasInitialized = false;
        }

        public void ResetForPool()
        {
            hasInitialized = false;
        }

        private void ResolveReferences()
        {
            if (phaseController == null)
            {
                phaseController = GetComponent<BossPhaseController>();
            }

            if (patternController == null)
            {
                patternController = GetComponent<BossPatternController>();
            }

            if (addSpawnAction == null)
            {
                addSpawnAction = GetComponent<BossAddSpawnAction>();
            }

            if (introController == null)
            {
                introController = GetComponent<BossIntroController>();
            }
        }
    }
}
