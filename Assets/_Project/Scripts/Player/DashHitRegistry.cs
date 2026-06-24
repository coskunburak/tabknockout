using System.Collections.Generic;
using UnityEngine;

namespace TapKnockout.Player
{
    public sealed class DashHitRegistry
    {
        private readonly HashSet<GameObject> hitTargets = new HashSet<GameObject>();

        public int Count => hitTargets.Count;

        public void Clear()
        {
            hitTargets.Clear();
        }

        public bool TryRegister(GameObject target)
        {
            return target != null && hitTargets.Add(target);
        }
    }
}
