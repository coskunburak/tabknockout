namespace TapKnockout.Ability
{
    public readonly struct AbilitySelectedEventArgs
    {
        public AbilitySelectedEventArgs(
            AbilitySelectionController source,
            AbilityDefinition selectedAbility,
            int selectedIndex,
            int stackCount)
        {
            Source = source;
            SelectedAbility = selectedAbility;
            SelectedIndex = selectedIndex;
            StackCount = stackCount < 0 ? 0 : stackCount;
        }

        public AbilitySelectionController Source { get; }
        public AbilityDefinition SelectedAbility { get; }
        public int SelectedIndex { get; }
        public int StackCount { get; }
        public bool HasSelectedAbility => SelectedAbility != null;
    }
}
