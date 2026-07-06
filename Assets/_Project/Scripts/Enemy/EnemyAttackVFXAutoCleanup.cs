using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Enemy
{
    [DisallowMultipleComponent]
    public sealed class EnemyAttackVFXAutoCleanup : MonoBehaviour, IPoolLifecycle
    {
        [SerializeField, Min(0.05f)] private float lifetimeSeconds = 1.25f;
        [SerializeField] private bool deactivateInsteadOfDestroy;

        private float remaining;

        public float LifetimeSeconds => lifetimeSeconds;
        public bool DeactivateInsteadOfDestroy => deactivateInsteadOfDestroy;

        private void OnEnable()
        {
            remaining = Mathf.Max(0.05f, lifetimeSeconds);
            RestartVisuals();
        }

        private void Update()
        {
            remaining -= Time.deltaTime;
            if (remaining > 0f)
            {
                return;
            }

            if (deactivateInsteadOfDestroy)
            {
                gameObject.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Configure(float lifetime, bool deactivateOnExpire = false)
        {
            lifetimeSeconds = Mathf.Max(0.05f, lifetime);
            deactivateInsteadOfDestroy = deactivateOnExpire;
            remaining = lifetimeSeconds;
        }

        public void OnBeforeSpawnFromPool()
        {
            ResetForPool();
        }

        public void OnSpawnedFromPool()
        {
            remaining = Mathf.Max(0.05f, lifetimeSeconds);
            RestartVisuals();
        }

        public void OnBeforeDespawnToPool()
        {
            StopVisuals();
        }

        public void ResetForPool()
        {
            remaining = Mathf.Max(0.05f, lifetimeSeconds);
            StopVisuals();
        }

        private void RestartVisuals()
        {
            var particles = GetComponentsInChildren<ParticleSystem>(true);
            for (var i = 0; i < particles.Length; i++)
            {
                particles[i].Clear(true);
                particles[i].Play(true);
            }

            var trails = GetComponentsInChildren<TrailRenderer>(true);
            for (var i = 0; i < trails.Length; i++)
            {
                trails[i].Clear();
                trails[i].emitting = true;
            }
        }

        private void StopVisuals()
        {
            var particles = GetComponentsInChildren<ParticleSystem>(true);
            for (var i = 0; i < particles.Length; i++)
            {
                particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particles[i].Clear(true);
            }

            var trails = GetComponentsInChildren<TrailRenderer>(true);
            for (var i = 0; i < trails.Length; i++)
            {
                trails[i].emitting = false;
                trails[i].Clear();
            }
        }
    }
}
