using TapKnockout.Combat;
using TapKnockout.Enemy;
using UnityEngine;
using UnityEngine.AI;

namespace TapKnockout.Survivor
{
    [DisallowMultipleComponent]
    public sealed class PooledEnemy : MonoBehaviour
    {
        private EnemyPoolService owner;
        private GameObject prefabKey;
        private Rigidbody cachedRigidbody;
        private bool isInPool;

        public bool IsConfigured => owner != null && prefabKey != null;
        public GameObject PrefabKey => prefabKey;
        public bool IsInPool => isInPool;

        public void Configure(EnemyPoolService poolOwner, GameObject enemyPrefab)
        {
            owner = poolOwner;
            prefabKey = enemyPrefab;
            if (cachedRigidbody == null)
            {
                cachedRigidbody = GetComponent<Rigidbody>();
            }
        }

        public void PrepareForSpawn(Vector3 position, Quaternion rotation, Transform runtimeParent)
        {
            isInPool = false;
            InvokeLifecycleBeforeSpawn();
            if (runtimeParent != null)
            {
                transform.SetParent(runtimeParent, true);
            }

            transform.SetPositionAndRotation(position, rotation);
            ResetPhysics();
            SetRenderersEnabled(true);
            SetCollidersEnabled(true);
            SetCommonEnemyBehavioursEnabled(true);
            gameObject.SetActive(true);
        }

        public void NotifySpawned()
        {
            InvokeLifecycleSpawned();
        }

        public void ReleaseToPool()
        {
            if (owner == null)
            {
                gameObject.SetActive(false);
                return;
            }

            owner.Release(this);
        }

        internal void PrepareForPool(Transform poolRoot)
        {
            if (isInPool)
            {
                return;
            }

            isInPool = true;
            InvokeLifecycleBeforeDespawn();
            ResetPhysics();
            ResetNavAgents();
            ResetStatusEffects();
            ResetTelegraphs();
            ResetAnimators();
            ResetParticlesTrailsAndAudio();
            InvokeLifecycleResetForPool();
            SetRenderersEnabled(true);
            SetCommonEnemyBehavioursEnabled(false);
            SetCollidersEnabled(false);
            if (poolRoot != null)
            {
                transform.SetParent(poolRoot, true);
            }

            gameObject.SetActive(false);
        }

        private void ResetPhysics()
        {
            if (cachedRigidbody == null)
            {
                cachedRigidbody = GetComponent<Rigidbody>();
            }

            if (cachedRigidbody == null)
            {
                return;
            }

            cachedRigidbody.linearVelocity = Vector3.zero;
            cachedRigidbody.angularVelocity = Vector3.zero;
        }

        private void ResetNavAgents()
        {
            var agents = GetComponentsInChildren<NavMeshAgent>(true);
            for (var i = 0; i < agents.Length; i++)
            {
                var agent = agents[i];
                if (agent == null)
                {
                    continue;
                }

                agent.velocity = Vector3.zero;
                if (agent.enabled && agent.isOnNavMesh)
                {
                    agent.ResetPath();
                    agent.nextPosition = transform.position;
                }
            }
        }

        private void ResetStatusEffects()
        {
            var statusControllers = GetComponentsInChildren<StatusEffectController>(true);
            for (var i = 0; i < statusControllers.Length; i++)
            {
                statusControllers[i].ClearAll();
            }
        }

        private void ResetTelegraphs()
        {
            var telegraphs = GetComponentsInChildren<EnemyTelegraphController>(true);
            for (var i = 0; i < telegraphs.Length; i++)
            {
                telegraphs[i].ResetRuntimeState();
            }
        }

        private void ResetAnimators()
        {
            var animators = GetComponentsInChildren<Animator>(true);
            for (var i = 0; i < animators.Length; i++)
            {
                var animator = animators[i];
                if (animator == null)
                {
                    continue;
                }

                animator.Rebind();
                animator.Update(0f);
            }
        }

        private void ResetParticlesTrailsAndAudio()
        {
            var particleSystems = GetComponentsInChildren<ParticleSystem>(true);
            for (var i = 0; i < particleSystems.Length; i++)
            {
                particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            var trails = GetComponentsInChildren<TrailRenderer>(true);
            for (var i = 0; i < trails.Length; i++)
            {
                trails[i].Clear();
            }

            var audioSources = GetComponentsInChildren<AudioSource>(true);
            for (var i = 0; i < audioSources.Length; i++)
            {
                audioSources[i].Stop();
            }
        }

        private void SetRenderersEnabled(bool enabled)
        {
            var renderers = GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].enabled = enabled;
                }
            }
        }

        private void SetCollidersEnabled(bool enabled)
        {
            var colliders = GetComponentsInChildren<Collider>(true);
            for (var i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = enabled;
            }
        }

        private void SetCommonEnemyBehavioursEnabled(bool enabled)
        {
            var behaviours = GetComponentsInChildren<MonoBehaviour>(true);
            for (var i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is EnemyMovement ||
                    behaviours[i] is EnemyAttackController ||
                    behaviours[i] is EnemyDistinctAttackController)
                {
                    behaviours[i].enabled = enabled;
                }
            }
        }

        private void InvokeLifecycleBeforeSpawn()
        {
            var lifecycleComponents = GetComponentsInChildren<MonoBehaviour>(true);
            for (var i = 0; i < lifecycleComponents.Length; i++)
            {
                if (lifecycleComponents[i] is IPoolLifecycle lifecycle)
                {
                    lifecycle.OnBeforeSpawnFromPool();
                }
            }
        }

        private void InvokeLifecycleSpawned()
        {
            var lifecycleComponents = GetComponentsInChildren<MonoBehaviour>(true);
            for (var i = 0; i < lifecycleComponents.Length; i++)
            {
                if (lifecycleComponents[i] is IPoolLifecycle lifecycle)
                {
                    lifecycle.OnSpawnedFromPool();
                }
            }
        }

        private void InvokeLifecycleBeforeDespawn()
        {
            var lifecycleComponents = GetComponentsInChildren<MonoBehaviour>(true);
            for (var i = 0; i < lifecycleComponents.Length; i++)
            {
                if (lifecycleComponents[i] is IPoolLifecycle lifecycle)
                {
                    lifecycle.OnBeforeDespawnToPool();
                }
            }
        }

        private void InvokeLifecycleResetForPool()
        {
            var lifecycleComponents = GetComponentsInChildren<MonoBehaviour>(true);
            for (var i = 0; i < lifecycleComponents.Length; i++)
            {
                if (lifecycleComponents[i] is IPoolLifecycle lifecycle)
                {
                    lifecycle.ResetForPool();
                }
            }
        }
    }
}
