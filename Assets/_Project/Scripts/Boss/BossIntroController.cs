using System.Collections;
using UnityEngine;

namespace TapKnockout.Boss
{
    [DisallowMultipleComponent]
    public sealed class BossIntroController : MonoBehaviour
    {
        [SerializeField] private BossConfig config;
        [SerializeField] private BossPatternController patternController;
        [SerializeField, Min(0f)] private float introDuration = 0.75f;
        [SerializeField] private bool startPatternAfterIntro = true;

        private Coroutine introRoutine;

        public bool IsIntroRunning => introRoutine != null;

        private void Awake()
        {
            if (patternController == null)
            {
                patternController = GetComponent<BossPatternController>();
            }
        }

        public void PlayIntro()
        {
            if (introRoutine != null)
            {
                StopCoroutine(introRoutine);
            }

            introRoutine = StartCoroutine(PlayIntroRoutine());
        }

        private IEnumerator PlayIntroRoutine()
        {
            BossEvents.RaiseBossIntroStarted(new BossEventArgs(gameObject, config, BossPhaseState.Phase1, "boss_intro_started"));
            if (introDuration > 0f)
            {
                yield return new WaitForSeconds(introDuration);
            }

            BossEvents.RaiseBossIntroCompleted(new BossEventArgs(gameObject, config, BossPhaseState.Phase1, "boss_intro_completed"));
            if (startPatternAfterIntro)
            {
                patternController?.StartPattern();
            }

            introRoutine = null;
        }
    }
}
