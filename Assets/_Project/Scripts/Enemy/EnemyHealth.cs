using System;
using System.Collections;
using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Enemy
{
    [DisallowMultipleComponent]
    public sealed class EnemyHealth : MonoBehaviour, IDamageable, ITargetable, ICombatTargetTraits, IPoolLifecycle
    {
        [Header("Config")]
        [SerializeField] private EnemyConfig config;

        [Header("Targeting")]
        [SerializeField] private Transform targetTransform;
        [SerializeField] private bool targetableWhenAlive = true;

        [Header("Death")]
        [SerializeField] private bool deactivateOnDeath;
        [SerializeField] private bool disableCollidersOnDeath = true;

        [Header("Combat Hurtbox")]
        [SerializeField] private bool autoConfigureCombatHurtbox = true;
        [SerializeField, Min(0f)] private float combatHurtboxVerticalPadding = 0.24f;
        [SerializeField, Min(0f)] private float combatHurtboxHorizontalPadding = 0.12f;
        [SerializeField, Min(0f)] private float combatHurtboxBodyTopPadding = 0.6f;
        [SerializeField, Min(0.1f)] private float minimumCombatHurtboxRadius = 0.42f;

        [Header("Debug")]
        [SerializeField] private bool logHits;
        [SerializeField] private bool logDeath = true;

        private const string CombatHurtboxName = "CombatHurtbox";
        private KnockbackReceiver knockbackReceiver;
        private ShieldDamageFilter shieldDamageFilter;
        private Coroutine deathCoroutine;
        private bool hasDied;
        private bool isPoolInactive;

        public event Action<HitContext> OnDamaged;
        public event Action<HitContext> OnDied;

        public bool IsAlive => !isPoolInactive && !hasDied && CurrentHealth > 0f;
        public bool IsTargetable => !isPoolInactive && targetableWhenAlive && IsAlive;
        public GameObject GameObject => gameObject;
        public Transform TargetTransform => targetTransform != null ? targetTransform : transform;
        public float CurrentHealth { get; private set; }
        public float MaxHealth => config != null ? config.MaxHealth : 40f;
        public EnemyConfig Config => config;
        public bool IsBossTarget => config != null && (config.Rank == EnemyRank.Boss || config.Rank == EnemyRank.MiniBoss);
        public bool IsEliteTarget => config != null && (config.Rank == EnemyRank.Elite || config.Rank == EnemyRank.MiniBoss || config.Rank == EnemyRank.Boss);

        private void Awake()
        {
            knockbackReceiver = GetComponent<KnockbackReceiver>();
            shieldDamageFilter = GetComponent<ShieldDamageFilter>();
            EnsureCombatHurtbox();
            ResetHealth();
        }

        private void OnValidate()
        {
            if (targetTransform == null)
            {
                targetTransform = transform;
            }
        }

        public void Initialize(EnemyConfig enemyConfig, bool resetHealth = true)
        {
            config = enemyConfig;

            if (resetHealth)
            {
                ResetHealth();
            }
        }

        public void ResetHealth()
        {
            isPoolInactive = false;
            hasDied = false;
            CurrentHealth = MaxHealth;
            EnsureCombatHurtbox();
            SetCollidersEnabled(true);

            if (deathCoroutine != null)
            {
                StopCoroutine(deathCoroutine);
                deathCoroutine = null;
            }
        }

        public void OnBeforeSpawnFromPool()
        {
            ResetHealth();
        }

        public void OnSpawnedFromPool()
        {
            ResetHealth();
        }

        public void OnBeforeDespawnToPool()
        {
            MarkInactiveForPool();
        }

        public void ResetForPool()
        {
            MarkInactiveForPool();
        }

        public void ReceiveHit(HitContext hitContext)
        {
            if (hitContext == null || !IsAlive)
            {
                return;
            }

            if (shieldDamageFilter == null)
            {
                TryGetComponent(out shieldDamageFilter);
            }

            shieldDamageFilter?.ApplyToHit(hitContext);

            var damageAmount = Mathf.Max(0f, hitContext.DamageAmount);
            CurrentHealth = Mathf.Max(0f, CurrentHealth - damageAmount);

            if (logHits)
            {
                Debug.Log(
                    $"{nameof(EnemyHealth)} on {name} received {damageAmount:0.##} {hitContext.DamageType} damage. DashHit: {hitContext.IsDashHit}. HP: {CurrentHealth:0.##}/{MaxHealth:0.##}",
                    this);
            }

            OnDamaged?.Invoke(hitContext);

            if (IsAlive)
            {
                ApplyKnockbackIfAllowed(hitContext);
                return;
            }

            Die(hitContext);
        }

        private void ApplyKnockbackIfAllowed(HitContext hitContext)
        {
            if (!hitContext.Knockback.HasKnockback || config != null && !config.CanBeKnockedBack)
            {
                return;
            }

            if (knockbackReceiver == null)
            {
                TryGetComponent(out knockbackReceiver);
            }

            knockbackReceiver?.ApplyKnockback(hitContext);
        }

        private void Die(HitContext killingHit)
        {
            if (hasDied)
            {
                return;
            }

            hasDied = true;
            CurrentHealth = 0f;

            if (logDeath)
            {
                Debug.Log($"{nameof(EnemyHealth)} on {name} died.", this);
            }

            OnDied?.Invoke(killingHit);
            CombatEvents.RaiseEntityKilled(new EntityKilledEvent(gameObject, killingHit.Source, killingHit));
            DisableRuntimeBlockingIfNeeded();

            if (deactivateOnDeath)
            {
                deathCoroutine = StartCoroutine(DeactivateAfterDelay());
            }
        }

        private void DisableRuntimeBlockingIfNeeded()
        {
            if (disableCollidersOnDeath)
            {
                SetCollidersEnabled(false);
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

        private void EnsureCombatHurtbox()
        {
            if (!autoConfigureCombatHurtbox)
            {
                return;
            }

            var hurtbox = transform.Find(CombatHurtboxName);
            if (hurtbox == null)
            {
                var hurtboxObject = new GameObject(CombatHurtboxName);
                hurtbox = hurtboxObject.transform;
                hurtbox.SetParent(transform, false);
            }

            hurtbox.localPosition = Vector3.zero;
            hurtbox.localRotation = Quaternion.identity;
            hurtbox.localScale = Vector3.one;
            hurtbox.gameObject.layer = gameObject.layer;

            var collider = hurtbox.GetComponent<CapsuleCollider>();
            if (collider == null)
            {
                collider = hurtbox.gameObject.AddComponent<CapsuleCollider>();
            }

            var bodyBounds = ResolveFallbackBodyBounds();
            var localBounds = bodyBounds;
            if (TryResolveVisualBounds(out var visualBounds))
            {
                localBounds.Encapsulate(visualBounds);
            }

            var horizontalRadius = Mathf.Max(
                minimumCombatHurtboxRadius,
                Mathf.Max(localBounds.extents.x, localBounds.extents.z, bodyBounds.extents.x, bodyBounds.extents.z) + combatHurtboxHorizontalPadding);

            var bottom = Mathf.Min(0f, bodyBounds.min.y, localBounds.min.y - combatHurtboxVerticalPadding * 0.5f);
            var top = Mathf.Max(
                bodyBounds.max.y + combatHurtboxBodyTopPadding,
                localBounds.max.y + combatHurtboxVerticalPadding * 0.5f);
            var height = Mathf.Max(
                horizontalRadius * 2f,
                bodyBounds.size.y + combatHurtboxVerticalPadding,
                localBounds.size.y + combatHurtboxVerticalPadding,
                top - bottom);

            top = Mathf.Max(top, bottom + height);

            collider.isTrigger = true;
            collider.direction = 1;
            collider.radius = horizontalRadius;
            collider.height = height;
            collider.center = new Vector3(0f, (bottom + top) * 0.5f, 0f);
            collider.enabled = true;
        }

        private bool TryResolveVisualBounds(out Bounds localBounds)
        {
            localBounds = default;
            var visualRoot = transform.Find("VisualRoot");
            var renderers = visualRoot != null
                ? visualRoot.GetComponentsInChildren<Renderer>(true)
                : GetComponentsInChildren<Renderer>(true);

            var hasBounds = false;
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                EncapsulateWorldBounds(renderer.bounds, ref localBounds, ref hasBounds);
            }

            return hasBounds;
        }

        private Bounds ResolveFallbackBodyBounds()
        {
            var hasBounds = false;
            var localBounds = new Bounds(new Vector3(0f, 0.75f, 0f), new Vector3(0.84f, 1.5f, 0.84f));
            var hurtbox = transform.Find(CombatHurtboxName);
            var colliders = GetComponentsInChildren<Collider>(true);
            for (var i = 0; i < colliders.Length; i++)
            {
                var collider = colliders[i];
                if (collider == null || collider.isTrigger || collider.transform == hurtbox)
                {
                    continue;
                }

                if (!EncapsulateColliderLocalBounds(collider, ref localBounds, ref hasBounds))
                {
                    EncapsulateWorldBounds(collider.bounds, ref localBounds, ref hasBounds);
                }
            }

            return localBounds;
        }

        private bool EncapsulateColliderLocalBounds(Collider collider, ref Bounds localBounds, ref bool hasBounds)
        {
            if (collider is CapsuleCollider capsuleCollider)
            {
                var radius = Mathf.Max(0f, capsuleCollider.radius);
                var height = Mathf.Max(radius * 2f, capsuleCollider.height);
                var size = capsuleCollider.direction switch
                {
                    0 => new Vector3(height, radius * 2f, radius * 2f),
                    2 => new Vector3(radius * 2f, radius * 2f, height),
                    _ => new Vector3(radius * 2f, height, radius * 2f)
                };
                EncapsulateShapeLocalBounds(collider.transform, new Bounds(capsuleCollider.center, size), ref localBounds, ref hasBounds);
                return true;
            }

            if (collider is BoxCollider boxCollider)
            {
                EncapsulateShapeLocalBounds(collider.transform, new Bounds(boxCollider.center, boxCollider.size), ref localBounds, ref hasBounds);
                return true;
            }

            if (collider is SphereCollider sphereCollider)
            {
                var diameter = Mathf.Max(0f, sphereCollider.radius) * 2f;
                EncapsulateShapeLocalBounds(collider.transform, new Bounds(sphereCollider.center, Vector3.one * diameter), ref localBounds, ref hasBounds);
                return true;
            }

            if (collider is MeshCollider meshCollider && meshCollider.sharedMesh != null)
            {
                EncapsulateShapeLocalBounds(collider.transform, meshCollider.sharedMesh.bounds, ref localBounds, ref hasBounds);
                return true;
            }

            return false;
        }

        private void EncapsulateShapeLocalBounds(Transform shapeTransform, Bounds shapeLocalBounds, ref Bounds localBounds, ref bool hasBounds)
        {
            var min = shapeLocalBounds.min;
            var max = shapeLocalBounds.max;
            EncapsulateLocalPoint(shapeTransform.TransformPoint(new Vector3(min.x, min.y, min.z)), ref localBounds, ref hasBounds);
            EncapsulateLocalPoint(shapeTransform.TransformPoint(new Vector3(min.x, min.y, max.z)), ref localBounds, ref hasBounds);
            EncapsulateLocalPoint(shapeTransform.TransformPoint(new Vector3(min.x, max.y, min.z)), ref localBounds, ref hasBounds);
            EncapsulateLocalPoint(shapeTransform.TransformPoint(new Vector3(min.x, max.y, max.z)), ref localBounds, ref hasBounds);
            EncapsulateLocalPoint(shapeTransform.TransformPoint(new Vector3(max.x, min.y, min.z)), ref localBounds, ref hasBounds);
            EncapsulateLocalPoint(shapeTransform.TransformPoint(new Vector3(max.x, min.y, max.z)), ref localBounds, ref hasBounds);
            EncapsulateLocalPoint(shapeTransform.TransformPoint(new Vector3(max.x, max.y, min.z)), ref localBounds, ref hasBounds);
            EncapsulateLocalPoint(shapeTransform.TransformPoint(new Vector3(max.x, max.y, max.z)), ref localBounds, ref hasBounds);
        }

        private void EncapsulateWorldBounds(Bounds worldBounds, ref Bounds localBounds, ref bool hasBounds)
        {
            var min = worldBounds.min;
            var max = worldBounds.max;
            EncapsulateLocalPoint(new Vector3(min.x, min.y, min.z), ref localBounds, ref hasBounds);
            EncapsulateLocalPoint(new Vector3(min.x, min.y, max.z), ref localBounds, ref hasBounds);
            EncapsulateLocalPoint(new Vector3(min.x, max.y, min.z), ref localBounds, ref hasBounds);
            EncapsulateLocalPoint(new Vector3(min.x, max.y, max.z), ref localBounds, ref hasBounds);
            EncapsulateLocalPoint(new Vector3(max.x, min.y, min.z), ref localBounds, ref hasBounds);
            EncapsulateLocalPoint(new Vector3(max.x, min.y, max.z), ref localBounds, ref hasBounds);
            EncapsulateLocalPoint(new Vector3(max.x, max.y, min.z), ref localBounds, ref hasBounds);
            EncapsulateLocalPoint(new Vector3(max.x, max.y, max.z), ref localBounds, ref hasBounds);
        }

        private void EncapsulateLocalPoint(Vector3 worldPoint, ref Bounds localBounds, ref bool hasBounds)
        {
            var localPoint = transform.InverseTransformPoint(worldPoint);
            if (hasBounds)
            {
                localBounds.Encapsulate(localPoint);
                return;
            }

            localBounds = new Bounds(localPoint, Vector3.zero);
            hasBounds = true;
        }

        private IEnumerator DeactivateAfterDelay()
        {
            var delay = config != null ? config.DeathDelay : 0f;
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            gameObject.SetActive(false);
            deathCoroutine = null;
        }

        private void MarkInactiveForPool()
        {
            isPoolInactive = true;
            hasDied = true;
            CurrentHealth = 0f;
            SetCollidersEnabled(false);

            if (deathCoroutine != null)
            {
                StopCoroutine(deathCoroutine);
                deathCoroutine = null;
            }
        }
    }
}
