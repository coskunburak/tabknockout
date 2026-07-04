using System.Collections.Generic;
using UnityEngine;

namespace TapKnockout.Enemy
{
    [CreateAssetMenu(fileName = "EnemyAttackPatternConfig", menuName = "Tap Knockout/Enemies/Attack Pattern Config")]
    public sealed class EnemyAttackPatternConfig : ScriptableObject
    {
        [SerializeField] private string patternId = "enemy_pattern_default";
        [SerializeField, Min(0f)] private float initialDelay;
        [SerializeField] private bool loop = true;
        [SerializeField] private List<EnemyAttackStep> steps = new List<EnemyAttackStep>();

        public string PatternId => patternId;
        public float InitialDelay => initialDelay;
        public bool Loop => loop;
        public IReadOnlyList<EnemyAttackStep> Steps => steps;
        public bool HasSteps => steps != null && steps.Count > 0;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(patternId))
            {
                patternId = "enemy_pattern_default";
            }

            initialDelay = Mathf.Max(0f, initialDelay);
            steps ??= new List<EnemyAttackStep>();

            for (var i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                step.ClampValues();
                steps[i] = step;
            }
        }

        public void SetSteps(IEnumerable<EnemyAttackStep> newSteps)
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
