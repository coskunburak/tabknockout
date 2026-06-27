using System.Collections.Generic;
using UnityEngine;

namespace TapKnockout.Boss
{
    [CreateAssetMenu(fileName = "BossPatternConfig", menuName = "Tap Knockout/Boss/Boss Pattern Config")]
    public sealed class BossPatternConfig : ScriptableObject
    {
        [SerializeField, Min(0f)] private float initialDelay;
        [SerializeField] private bool loop = true;
        [SerializeField] private List<BossAttackStep> steps = new List<BossAttackStep>();

        public float InitialDelay => initialDelay;
        public bool Loop => loop;
        public IReadOnlyList<BossAttackStep> Steps => steps;

        private void OnValidate()
        {
            initialDelay = Mathf.Max(0f, initialDelay);
            for (var i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                step.ClampValues();
                steps[i] = step;
            }
        }

        public void SetSteps(IEnumerable<BossAttackStep> newSteps)
        {
            steps.Clear();

            if (newSteps == null)
            {
                return;
            }

            foreach (var stepValue in newSteps)
            {
                var step = stepValue;
                step.ClampValues();
                steps.Add(step);
            }
        }

        public void SetLoop(bool shouldLoop)
        {
            loop = shouldLoop;
        }
    }
}
