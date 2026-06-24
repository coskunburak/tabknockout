namespace TapKnockout.Ability
{
    public readonly struct AbilityEffectContext
    {
        public AbilityEffectContext(
            AbilitySelectionController source,
            AbilityDefinition ability,
            RunAbilityState runState,
            int stackCount)
        {
            Source = source;
            Ability = ability;
            RunState = runState;
            StackCount = stackCount < 0 ? 0 : stackCount;
        }

        public AbilitySelectionController Source { get; }
        public AbilityDefinition Ability { get; }
        public RunAbilityState RunState { get; }
        public int StackCount { get; }
        public bool IsValid => Ability != null;
    }
}
