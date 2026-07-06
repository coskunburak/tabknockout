using TapKnockout.Combat;
using UnityEngine;

namespace TapKnockout.Camera
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GameplayCameraController))]
    public sealed class TapKnockoutCameraDashLookAhead : MonoBehaviour
    {
        [SerializeField] private GameplayCameraController cameraController;
        [SerializeField] private Transform target;
        [SerializeField] private bool listenToDashEvents = true;

        private void Reset()
        {
            cameraController = GetComponent<GameplayCameraController>();
            target = cameraController != null ? cameraController.FollowTarget : null;
        }

        private void Awake()
        {
            if (cameraController == null)
            {
                cameraController = GetComponent<GameplayCameraController>();
            }

            if (target == null && cameraController != null)
            {
                target = cameraController.FollowTarget;
            }
        }

        private void OnEnable()
        {
            if (!listenToDashEvents)
            {
                return;
            }

            DashEvents.OnDashStarted -= HandleDashStarted;
            DashEvents.OnDashStarted += HandleDashStarted;
        }

        private void OnDisable()
        {
            DashEvents.OnDashStarted -= HandleDashStarted;
        }

        public void Configure(GameplayCameraController controller, Transform followTarget)
        {
            cameraController = controller;
            target = followTarget;
        }

        private void HandleDashStarted(DashStartedEventArgs eventArgs)
        {
            if (cameraController == null || !MatchesTarget(eventArgs.Source))
            {
                return;
            }

            cameraController.RequestDashLookAhead(eventArgs.Direction, eventArgs.Duration);
        }

        private bool MatchesTarget(GameObject source)
        {
            var activeTarget = target != null ? target : cameraController != null ? cameraController.FollowTarget : null;
            if (activeTarget == null || source == null)
            {
                return false;
            }

            var sourceTransform = source.transform;
            return sourceTransform == activeTarget ||
                sourceTransform.IsChildOf(activeTarget) ||
                activeTarget.IsChildOf(sourceTransform);
        }
    }
}
