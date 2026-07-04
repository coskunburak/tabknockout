using System;
using UnityEngine;

namespace TapKnockout.Pickups
{
    [DisallowMultipleComponent]
    public sealed class XPOrb : MonoBehaviour
    {
        [Header("XP")]
        [SerializeField, Min(1)] private int xpAmount = 1;
        [SerializeField, Min(0f)] private float lifetimeSeconds = 20f;

        [Header("Magnet")]
        [SerializeField, Min(0f)] private float attractionSpeed = 7f;
        [SerializeField, Min(0f)] private float attractionAcceleration = 18f;

        [Header("Lifecycle")]
        [SerializeField] private bool deactivateOnCollect = true;
        [SerializeField] private bool deactivateOnLifetimeExpired = true;

        private PickupCollector collector;
        private float lifetimeRemaining;
        private float currentAttractionSpeed;
        private bool collected;

        public static event Action<XPOrb, PickupCollector> OnAnyCollected;
        public event Action<XPOrb, PickupCollector> OnCollected;

        public int XPAmount => xpAmount;
        public bool IsCollected => collected;

        private void OnEnable()
        {
            lifetimeRemaining = lifetimeSeconds;
            currentAttractionSpeed = attractionSpeed;
            collected = false;
        }

        private void Update()
        {
            TickLifetime(Time.deltaTime);
            TickAttraction(Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            var targetCollector = other != null ? other.GetComponentInParent<PickupCollector>() : null;
            if (targetCollector != null)
            {
                TryCollect(targetCollector);
            }
        }

        public void Initialize(int amount, PickupCollector targetCollector = null)
        {
            xpAmount = Mathf.Max(1, amount);
            collector = targetCollector;
            lifetimeRemaining = lifetimeSeconds;
            currentAttractionSpeed = attractionSpeed;
            collected = false;
        }

        public bool TryCollect(PickupCollector targetCollector)
        {
            if (collected || targetCollector == null)
            {
                return false;
            }

            collected = true;
            targetCollector.CollectXP(xpAmount);
            OnCollected?.Invoke(this, targetCollector);
            OnAnyCollected?.Invoke(this, targetCollector);

            if (deactivateOnCollect)
            {
                gameObject.SetActive(false);
            }

            return true;
        }

        private void TickLifetime(float deltaTime)
        {
            if (lifetimeSeconds <= 0f || collected)
            {
                return;
            }

            lifetimeRemaining -= deltaTime;
            if (lifetimeRemaining <= 0f && deactivateOnLifetimeExpired)
            {
                gameObject.SetActive(false);
            }
        }

        private void TickAttraction(float deltaTime)
        {
            if (collector == null || collected)
            {
                return;
            }

            var target = collector.PickupOrigin;
            if (target == null)
            {
                return;
            }

            var toCollector = target.position - transform.position;
            var distance = toCollector.magnitude;
            if (distance <= collector.CollectRadius)
            {
                TryCollect(collector);
                return;
            }

            if (distance > collector.MagnetRadius || distance <= 0.0001f)
            {
                return;
            }

            currentAttractionSpeed += attractionAcceleration * deltaTime;
            transform.position += toCollector.normalized * (currentAttractionSpeed * deltaTime);
        }
    }
}
