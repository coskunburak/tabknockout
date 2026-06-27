namespace TapKnockout.Combat
{
    public interface IStatusEffectReceiver
    {
        bool TryApplyStatusEffect(StatusEffectRequest request);
    }
}
