using UnityEngine;

namespace TapKnockout.Enemy
{
    public sealed class SplitSpawnRuntimeMarker : MonoBehaviour
    {
        public int Depth { get; private set; }

        public void SetDepth(int depth)
        {
            Depth = Mathf.Max(0, depth);
        }
    }
}
