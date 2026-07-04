namespace TapKnockout.Combat
{
    public interface ICombatTargetTraits
    {
        bool IsBossTarget { get; }
        bool IsEliteTarget { get; }
    }
}
