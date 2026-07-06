using System.Collections.Generic;
using UnityEngine;

namespace TapKnockout.Combat
{
    public static class CombatHitModifierUtility
    {
        private static readonly List<IHitModifierProvider> Providers = new List<IHitModifierProvider>(8);

        public static void ApplySourceModifiers(HitContext hitContext)
        {
            if (hitContext == null || hitContext.Source == null)
            {
                return;
            }

            Providers.Clear();
            hitContext.Source.GetComponents(Providers);
            for (var i = 0; i < Providers.Count; i++)
            {
                Providers[i]?.ModifyHit(hitContext);
            }

            Providers.Clear();
        }
    }
}
