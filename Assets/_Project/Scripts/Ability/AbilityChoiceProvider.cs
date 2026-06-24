using System;
using System.Collections.Generic;

namespace TapKnockout.Ability
{
    public sealed class AbilityChoiceProvider
    {
        private readonly Random random;

        public AbilityChoiceProvider()
            : this(new Random())
        {
        }

        public AbilityChoiceProvider(int seed)
            : this(new Random(seed))
        {
        }

        public AbilityChoiceProvider(Random random)
        {
            this.random = random ?? new Random();
        }

        public IReadOnlyList<AbilityDefinition> GenerateChoices(
            IReadOnlyList<AbilityDefinition> pool,
            RunAbilityState runState,
            int choiceCount = 3)
        {
            var requestedCount = Math.Max(0, choiceCount);
            var choices = new List<AbilityDefinition>(requestedCount);
            if (requestedCount == 0 || pool == null || pool.Count == 0)
            {
                return choices;
            }

            var candidates = BuildCandidates(pool, runState);
            while (choices.Count < requestedCount && candidates.Count > 0)
            {
                var selected = PickWeighted(candidates);
                if (selected == null)
                {
                    break;
                }

                choices.Add(selected);
                if (!selected.AllowDuplicateInOffer)
                {
                    RemoveCandidatesWithAbilityId(candidates, selected.AbilityId);
                }
            }

            return choices;
        }

        private static List<AbilityDefinition> BuildCandidates(IReadOnlyList<AbilityDefinition> pool, RunAbilityState runState)
        {
            var candidates = new List<AbilityDefinition>(pool.Count);
            for (var i = 0; i < pool.Count; i++)
            {
                var definition = pool[i];
                if (definition == null || definition.Weight <= 0f)
                {
                    continue;
                }

                if (runState != null)
                {
                    if (runState.CanBeOffered(definition))
                    {
                        candidates.Add(definition);
                    }

                    continue;
                }

                if (definition.IsEnabled && definition.HasValidId)
                {
                    candidates.Add(definition);
                }
            }

            return candidates;
        }

        private AbilityDefinition PickWeighted(IReadOnlyList<AbilityDefinition> candidates)
        {
            var totalWeight = 0.0;
            for (var i = 0; i < candidates.Count; i++)
            {
                totalWeight += Math.Max(0f, candidates[i].Weight);
            }

            if (totalWeight <= 0.0)
            {
                return null;
            }

            var roll = random.NextDouble() * totalWeight;
            var accumulated = 0.0;
            for (var i = 0; i < candidates.Count; i++)
            {
                accumulated += Math.Max(0f, candidates[i].Weight);
                if (roll <= accumulated)
                {
                    return candidates[i];
                }
            }

            return candidates[candidates.Count - 1];
        }

        private static void RemoveCandidatesWithAbilityId(List<AbilityDefinition> candidates, string abilityId)
        {
            for (var i = candidates.Count - 1; i >= 0; i--)
            {
                if (string.Equals(candidates[i].AbilityId, abilityId, StringComparison.Ordinal))
                {
                    candidates.RemoveAt(i);
                }
            }
        }
    }
}
