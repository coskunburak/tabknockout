using UnityEngine;

namespace TapKnockout.Pickups
{
    [DisallowMultipleComponent]
    public sealed class PickupCollector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerXPController xpController;
        [SerializeField] private Transform pickupOrigin;

        [Header("Pickup")]
        [SerializeField, Min(0.05f)] private float collectRadius = 0.8f;
        [SerializeField, Min(0f)] private float magnetRadius = 4f;

        public Transform PickupOrigin => pickupOrigin != null ? pickupOrigin : transform;
        public float CollectRadius => collectRadius;
        public float MagnetRadius => magnetRadius;
        public PlayerXPController XPController => xpController;

        private void Reset()
        {
            xpController = GetComponent<PlayerXPController>();
            pickupOrigin = transform;
        }

        private void Awake()
        {
            if (xpController == null)
            {
                xpController = GetComponent<PlayerXPController>();
            }

            if (pickupOrigin == null)
            {
                pickupOrigin = transform;
            }
        }

        private void OnValidate()
        {
            collectRadius = Mathf.Max(0.05f, collectRadius);
            magnetRadius = Mathf.Max(0f, magnetRadius);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other != null && other.TryGetComponent<XPOrb>(out var orb))
            {
                orb.TryCollect(this);
            }
        }

        public void CollectXP(int amount)
        {
            xpController?.AddXP(amount);
        }
    }
}
