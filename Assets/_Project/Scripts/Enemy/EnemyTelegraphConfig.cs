using TapKnockout.VFX;
using UnityEngine;

namespace TapKnockout.Enemy
{
    [CreateAssetMenu(fileName = "EnemyTelegraphConfig", menuName = "Tap Knockout/Enemies/Telegraph Config")]
    public sealed class EnemyTelegraphConfig : ScriptableObject
    {
        [SerializeField] private string telegraphId = "enemy_telegraph_default";
        [SerializeField] private EnemyTelegraphType telegraphType = EnemyTelegraphType.Circle;
        [SerializeField] private GameObject telegraphPrefab;
        [SerializeField] private VFXEventType fallbackVfx = VFXEventType.EnemyTelegraph;
        [SerializeField] private Color color = new Color(1f, 0.35f, 0.12f, 0.75f);
        [SerializeField, Min(0f)] private float duration = 0.35f;
        [SerializeField, Min(0f)] private float radius = 1.25f;
        [SerializeField, Min(0f)] private float width = 0.8f;
        [SerializeField, Min(0f)] private float length = 4f;
        [SerializeField] private bool followTarget = true;
        [SerializeField] private bool lockAtCastStart = true;
        [SerializeField] private bool showFillProgress = true;

        public string TelegraphId => telegraphId;
        public EnemyTelegraphType TelegraphType => telegraphType;
        public GameObject TelegraphPrefab => telegraphPrefab;
        public VFXEventType FallbackVfx => fallbackVfx;
        public Color Color => color;
        public float Duration => duration;
        public float Radius => radius;
        public float Width => width;
        public float Length => length;
        public bool FollowTarget => followTarget;
        public bool LockAtCastStart => lockAtCastStart;
        public bool ShowFillProgress => showFillProgress;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(telegraphId))
            {
                telegraphId = "enemy_telegraph_default";
            }

            duration = Mathf.Max(0f, duration);
            radius = Mathf.Max(0f, radius);
            width = Mathf.Max(0f, width);
            length = Mathf.Max(0f, length);
        }
    }
}
