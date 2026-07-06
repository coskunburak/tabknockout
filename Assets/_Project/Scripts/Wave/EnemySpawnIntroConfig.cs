using UnityEngine;

namespace TapKnockout.Wave
{
    [CreateAssetMenu(fileName = "EnemySpawnIntroConfig", menuName = "Tap Knockout/Waves/Enemy Spawn Intro Config")]
    public sealed class EnemySpawnIntroConfig : ScriptableObject
    {
        [SerializeField] private string introId = "enemy_spawn_intro_default";
        [SerializeField, Min(0f)] private float introDelay = 0.2f;
        [SerializeField] private bool deactivateEnemyDuringIntro;
        [SerializeField] private GameObject introVisualPrefab;
        [SerializeField, Min(0f)] private float introVisualLifetime = 0.35f;

        public string IntroId => introId;
        public float IntroDelay => introDelay;
        public bool DeactivateEnemyDuringIntro => deactivateEnemyDuringIntro;
        public GameObject IntroVisualPrefab => introVisualPrefab;
        public float IntroVisualLifetime => introVisualLifetime;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(introId))
            {
                introId = "enemy_spawn_intro_default";
            }

            introDelay = Mathf.Max(0f, introDelay);
            introVisualLifetime = Mathf.Max(0f, introVisualLifetime);
        }
    }
}
