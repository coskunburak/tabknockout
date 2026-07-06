using System.Collections;
using TapKnockout.Enemy;
using UnityEngine;

namespace TapKnockout.Boss
{
    [DisallowMultipleComponent]
    public sealed class BossOutroController : MonoBehaviour
    {
        [SerializeField] private BossConfig config;
        [SerializeField] private EnemyHealth health;
        [SerializeField, Min(0f)] private float outroDuration = 0.75f;
        [SerializeField] private bool playOnBossDeath = true;

        private Coroutine outroRoutine;

        public bool IsOutroRunning => outroRoutine != null;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<EnemyHealth>();
            }
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.OnDied -= HandleDied;
                health.OnDied += HandleDied;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnDied -= HandleDied;
            }
        }

        public void PlayOutro()
        {
            if (outroRoutine != null)
            {
                StopCoroutine(outroRoutine);
            }

            outroRoutine = StartCoroutine(PlayOutroRoutine());
        }

        private void HandleDied(TapKnockout.Combat.HitContext hitContext)
        {
            if (playOnBossDeath)
            {
                PlayOutro();
            }
        }

        private IEnumerator PlayOutroRoutine()
        {
            BossEvents.RaiseBossOutroStarted(new BossEventArgs(gameObject, config, BossPhaseState.Defeated, "boss_outro_started"));
            if (outroDuration > 0f)
            {
                yield return new WaitForSeconds(outroDuration);
            }

            BossEvents.RaiseBossOutroCompleted(new BossEventArgs(gameObject, config, BossPhaseState.Defeated, "boss_outro_completed"));
            outroRoutine = null;
        }
    }
}
