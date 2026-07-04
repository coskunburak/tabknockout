using UnityEngine;

namespace TapKnockout.Boss
{
    [CreateAssetMenu(fileName = "BossRoomIntroConfig", menuName = "Tap Knockout/Boss/Boss Room Intro Config")]
    public sealed class BossRoomIntroConfig : ScriptableObject
    {
        [SerializeField] private string introId = "boss_room_intro_default";
        [SerializeField, Min(0f)] private float introDelay = 0.75f;
        [SerializeField] private bool useBossCameraHook = true;
        [SerializeField] private GameObject warningVisualPrefab;
        [SerializeField, Min(0f)] private float warningVisualLifetime = 0.75f;

        public string IntroId => introId;
        public float IntroDelay => introDelay;
        public bool UseBossCameraHook => useBossCameraHook;
        public GameObject WarningVisualPrefab => warningVisualPrefab;
        public float WarningVisualLifetime => warningVisualLifetime;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(introId))
            {
                introId = "boss_room_intro_default";
            }

            introDelay = Mathf.Max(0f, introDelay);
            warningVisualLifetime = Mathf.Max(0f, warningVisualLifetime);
        }
    }
}
