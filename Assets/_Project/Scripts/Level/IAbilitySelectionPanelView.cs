using TapKnockout.Ability;

namespace TapKnockout.Level
{
    public interface IAbilitySelectionPanelView
    {
        bool IsOpen { get; }
        void SetAbilitySelectionController(AbilitySelectionController controller);
        void SetPauseGameWhileOpen(bool shouldPause);
    }
}
