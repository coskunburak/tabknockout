namespace TapKnockout.Combat
{
    public interface IHitModifierProvider
    {
        void ModifyHit(HitContext hitContext);
    }
}
