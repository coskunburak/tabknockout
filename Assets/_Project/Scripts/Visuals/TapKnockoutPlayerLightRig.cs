using TapKnockout.Combat;
using TapKnockout.Player;
using UnityEngine;

namespace TapKnockout.Visuals
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(70)]
    public sealed class TapKnockoutPlayerLightRig : MonoBehaviour
    {
        private const string LocalHeroLightName = "HeroLocalLight";
        private const string ForwardLightName = "ForwardMovementLight";
        private const string DashBurstLightName = "DashBurstLight";
        private const float DirectionEpsilon = 0.0001f;

        [Header("Config")]
        [SerializeField] private TapKnockoutPlayerLightRigConfig config;

        [Header("References")]
        [SerializeField] private Transform target;
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private Light localHeroLight;
        [SerializeField] private Light forwardLight;
        [SerializeField] private Light dashBurstLight;

        [Header("Runtime")]
        [SerializeField] private bool listenToDashEvents = true;
        [SerializeField] private bool createMissingLightsOnAwake = true;
        [SerializeField] private bool snapOnEnable = true;

        private Vector3 lastTargetPosition;
        private Vector3 smoothedForwardDirection = Vector3.forward;
        private Vector3 dashDirection = Vector3.forward;
        private float dashPulseRemaining;
        private float dashPulseDuration;
        private bool hasLastTargetPosition;

        public TapKnockoutPlayerLightRigConfig Config => config;
        public Transform Target => target;
        public PlayerMovementController MovementController => movementController;
        public Light LocalHeroLight => localHeroLight;
        public Light ForwardLight => forwardLight;
        public Light DashBurstLight => dashBurstLight;

        private void Reset()
        {
            createMissingLightsOnAwake = true;
            EnsureLightReferences();
            RefreshLightSettings();
        }

        private void Awake()
        {
            if (target == null && movementController != null)
            {
                target = movementController.transform;
            }

            if (movementController == null && target != null)
            {
                movementController = target.GetComponent<PlayerMovementController>();
            }

            if (createMissingLightsOnAwake)
            {
                EnsureLightReferences();
            }

            RefreshLightSettings();
        }

        private void OnEnable()
        {
            if (target != null)
            {
                lastTargetPosition = target.position;
                hasLastTargetPosition = true;
                if (snapOnEnable)
                {
                    transform.position = target.position;
                }
            }

            if (listenToDashEvents)
            {
                DashEvents.OnDashStarted -= HandleDashStarted;
                DashEvents.OnDashStarted += HandleDashStarted;
            }
        }

        private void OnDisable()
        {
            DashEvents.OnDashStarted -= HandleDashStarted;
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                RefreshLightSettings();
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                SetEnabled(localHeroLight, false);
                SetEnabled(forwardLight, false);
                SetEnabled(dashBurstLight, false);
                return;
            }

            var deltaTime = Time.deltaTime;
            FollowTarget(deltaTime);

            var targetDelta = ResolveTargetDelta();
            var hasDirection = TryResolvePlanarDirection(targetDelta, out var desiredDirection);
            if (hasDirection)
            {
                var t = ResolveSharpnessT(ForwardDirectionSharpness, deltaTime);
                smoothedForwardDirection = Vector3.Slerp(smoothedForwardDirection, desiredDirection, t);
                if (smoothedForwardDirection.sqrMagnitude <= DirectionEpsilon)
                {
                    smoothedForwardDirection = desiredDirection;
                }
            }

            UpdateLightTransforms(smoothedForwardDirection);
            UpdateLightState(hasDirection, deltaTime);
        }

        public void SetTarget(Transform followTarget, PlayerMovementController movement, bool snap)
        {
            target = followTarget;
            movementController = movement != null ? movement : followTarget != null ? followTarget.GetComponent<PlayerMovementController>() : null;
            hasLastTargetPosition = false;
            if (target != null)
            {
                lastTargetPosition = target.position;
                hasLastTargetPosition = true;
                if (snap)
                {
                    transform.position = target.position;
                }
            }
        }

        public void SetConfig(TapKnockoutPlayerLightRigConfig value)
        {
            config = value;
            RefreshLightSettings();
        }

        public void RequestDashPulse(Vector3 direction, float duration)
        {
            if (!DashPulseEnabled)
            {
                return;
            }

            dashDirection = direction.sqrMagnitude > DirectionEpsilon ? PlanarNormalized(direction) : smoothedForwardDirection;
            dashPulseDuration = Mathf.Max(0.03f, Mathf.Min(Mathf.Max(DashPulseDuration, duration), 0.4f));
            dashPulseRemaining = dashPulseDuration;
            if (dashBurstLight != null)
            {
                dashBurstLight.enabled = true;
            }
        }

        public void RefreshLightSettings()
        {
            if (createMissingLightsOnAwake)
            {
                EnsureLightReferences();
            }

            ConfigurePointLight(localHeroLight, LocalHeroLightEnabled, LocalHeroLightColor, LocalHeroLightIntensity, LocalHeroLightRange);
            ConfigureSpotLight(forwardLight, ForwardLightEnabled, ForwardLightColor, ForwardLightIntensity, ForwardLightRange, ForwardLightSpotAngle);
            ConfigurePointLight(dashBurstLight, false, DashPulseColor, 0f, DashPulseRange);

            if (localHeroLight != null)
            {
                localHeroLight.transform.localPosition = LocalHeroLightOffset;
            }

            if (forwardLight != null)
            {
                forwardLight.transform.localPosition = ResolveDirectionalOffset(ForwardLightOffset, smoothedForwardDirection);
            }

            if (dashBurstLight != null)
            {
                dashBurstLight.transform.localPosition = ResolveDirectionalOffset(DashPulseOffset, smoothedForwardDirection);
            }
        }

        private void FollowTarget(float deltaTime)
        {
            var desiredPosition = target.position;
            var t = ResolveSharpnessT(FollowSharpness, deltaTime);
            transform.position = t >= 1f ? desiredPosition : Vector3.Lerp(transform.position, desiredPosition, t);
        }

        private Vector3 ResolveTargetDelta()
        {
            var currentPosition = target.position;
            if (!hasLastTargetPosition)
            {
                lastTargetPosition = currentPosition;
                hasLastTargetPosition = true;
                return Vector3.zero;
            }

            var delta = currentPosition - lastTargetPosition;
            lastTargetPosition = currentPosition;
            delta.y = 0f;
            return delta;
        }

        private bool TryResolvePlanarDirection(Vector3 targetDelta, out Vector3 direction)
        {
            if (dashPulseRemaining > 0f && dashDirection.sqrMagnitude > DirectionEpsilon)
            {
                direction = dashDirection;
                return true;
            }

            if (movementController != null && movementController.CurrentMoveDirection.sqrMagnitude > DirectionDeadZoneSqr)
            {
                direction = PlanarNormalized(movementController.CurrentMoveDirection);
                return true;
            }

            if (targetDelta.sqrMagnitude > DirectionDeadZoneSqr)
            {
                direction = PlanarNormalized(targetDelta);
                return true;
            }

            direction = smoothedForwardDirection.sqrMagnitude > DirectionEpsilon ? smoothedForwardDirection : Vector3.forward;
            return false;
        }

        private void UpdateLightTransforms(Vector3 planarDirection)
        {
            if (localHeroLight != null)
            {
                localHeroLight.transform.localPosition = LocalHeroLightOffset;
            }

            if (forwardLight != null)
            {
                forwardLight.transform.localPosition = ResolveDirectionalOffset(ForwardLightOffset, planarDirection);
                forwardLight.transform.rotation = ResolveForwardLightRotation(planarDirection);
            }

            if (dashBurstLight != null)
            {
                var burstDirection = dashPulseRemaining > 0f && dashDirection.sqrMagnitude > DirectionEpsilon
                    ? dashDirection
                    : planarDirection;
                dashBurstLight.transform.localPosition = ResolveDirectionalOffset(DashPulseOffset, burstDirection);
            }
        }

        private void UpdateLightState(bool hasDirection, float deltaTime)
        {
            if (localHeroLight != null)
            {
                localHeroLight.enabled = LocalHeroLightEnabled;
                localHeroLight.intensity = LocalHeroLightEnabled ? LocalHeroLightIntensity : 0f;
            }

            if (forwardLight != null)
            {
                forwardLight.enabled = ForwardLightEnabled;
                var multiplier = hasDirection ? 1f : ForwardLightIdleIntensityMultiplier;
                forwardLight.intensity = ForwardLightEnabled ? ForwardLightIntensity * multiplier : 0f;
            }

            UpdateDashBurst(deltaTime);
        }

        private void UpdateDashBurst(float deltaTime)
        {
            if (dashBurstLight == null)
            {
                return;
            }

            if (!DashPulseEnabled || dashPulseRemaining <= 0f || dashPulseDuration <= 0f)
            {
                dashPulseRemaining = 0f;
                dashBurstLight.intensity = 0f;
                dashBurstLight.enabled = false;
                return;
            }

            dashPulseRemaining = Mathf.Max(0f, dashPulseRemaining - Mathf.Max(0f, deltaTime));
            var normalized = Mathf.Clamp01(dashPulseRemaining / dashPulseDuration);
            var shaped = normalized * normalized * (3f - 2f * normalized);
            dashBurstLight.enabled = true;
            dashBurstLight.intensity = DashPulseIntensity * shaped;
        }

        private Quaternion ResolveForwardLightRotation(Vector3 planarDirection)
        {
            var direction = planarDirection.sqrMagnitude > DirectionEpsilon ? PlanarNormalized(planarDirection) : Vector3.forward;
            var downWeight = Mathf.Tan(ForwardLightDownAngle * Mathf.Deg2Rad);
            var lightDirection = direction + Vector3.down * downWeight;
            if (lightDirection.sqrMagnitude <= DirectionEpsilon)
            {
                lightDirection = Vector3.down;
            }

            return Quaternion.LookRotation(lightDirection.normalized, Vector3.up);
        }

        private void HandleDashStarted(DashStartedEventArgs eventArgs)
        {
            if (!MatchesTarget(eventArgs.Source))
            {
                return;
            }

            RequestDashPulse(eventArgs.Direction, eventArgs.Duration);
        }

        private bool MatchesTarget(GameObject source)
        {
            if (target == null || source == null)
            {
                return false;
            }

            var sourceTransform = source.transform;
            return sourceTransform == target ||
                sourceTransform.IsChildOf(target) ||
                target.IsChildOf(sourceTransform);
        }

        private void EnsureLightReferences()
        {
            if (localHeroLight == null)
            {
                localHeroLight = ResolveOrCreateLight(LocalHeroLightName, LightType.Point);
            }

            if (forwardLight == null)
            {
                forwardLight = ResolveOrCreateLight(ForwardLightName, LightType.Spot);
            }

            if (dashBurstLight == null)
            {
                dashBurstLight = ResolveOrCreateLight(DashBurstLightName, LightType.Point);
            }
        }

        private Light ResolveOrCreateLight(string childName, LightType lightType)
        {
            var child = transform.Find(childName);
            if (child == null)
            {
                var lightObject = new GameObject(childName);
                lightObject.transform.SetParent(transform, false);
                child = lightObject.transform;
            }

            if (!child.TryGetComponent<Light>(out var light))
            {
                light = child.gameObject.AddComponent<Light>();
            }

            light.type = lightType;
            return light;
        }

        private static void ConfigurePointLight(Light light, bool enabled, Color color, float intensity, float range)
        {
            if (light == null)
            {
                return;
            }

            light.type = LightType.Point;
            light.color = color;
            light.intensity = enabled ? intensity : 0f;
            light.range = range;
            light.shadows = LightShadows.None;
            light.enabled = enabled;
        }

        private static void ConfigureSpotLight(Light light, bool enabled, Color color, float intensity, float range, float spotAngle)
        {
            if (light == null)
            {
                return;
            }

            light.type = LightType.Spot;
            light.color = color;
            light.intensity = enabled ? intensity : 0f;
            light.range = range;
            light.spotAngle = spotAngle;
            light.shadows = LightShadows.None;
            light.enabled = enabled;
        }

        private static void SetEnabled(Light light, bool enabled)
        {
            if (light != null)
            {
                light.enabled = enabled;
            }
        }

        private static Vector3 PlanarNormalized(Vector3 value)
        {
            value.y = 0f;
            return value.sqrMagnitude > DirectionEpsilon ? value.normalized : Vector3.forward;
        }

        private static Vector3 ResolveDirectionalOffset(Vector3 offset, Vector3 planarDirection)
        {
            var forward = PlanarNormalized(planarDirection);
            var right = new Vector3(forward.z, 0f, -forward.x);
            return right * offset.x + Vector3.up * offset.y + forward * offset.z;
        }

        private static float ResolveSharpnessT(float sharpness, float deltaTime)
        {
            if (sharpness <= 0f)
            {
                return 1f;
            }

            return 1f - Mathf.Exp(-sharpness * Mathf.Max(0f, deltaTime));
        }

        private float DirectionDeadZoneSqr => MovementDirectionDeadZone * MovementDirectionDeadZone;
        private float FollowSharpness => config != null ? config.FollowSharpness : 28f;
        private float ForwardDirectionSharpness => config != null ? config.ForwardDirectionSharpness : 18f;
        private float MovementDirectionDeadZone => config != null ? config.MovementDirectionDeadZone : 0.05f;
        private bool LocalHeroLightEnabled => config != null && config.LocalHeroLightEnabled;
        private Color LocalHeroLightColor => config != null ? config.LocalHeroLightColor : new Color(0.72f, 1f, 0.84f, 1f);
        private float LocalHeroLightIntensity => config != null ? config.LocalHeroLightIntensity : 0.35f;
        private float LocalHeroLightRange => config != null ? config.LocalHeroLightRange : 4.5f;
        private Vector3 LocalHeroLightOffset => config != null ? config.LocalHeroLightOffset : new Vector3(0f, 2.65f, 0f);
        private bool ForwardLightEnabled => config != null && config.ForwardLightEnabled;
        private Color ForwardLightColor => config != null ? config.ForwardLightColor : new Color(0.5f, 1f, 0.76f, 1f);
        private float ForwardLightIntensity => config != null ? config.ForwardLightIntensity : 0.16f;
        private float ForwardLightIdleIntensityMultiplier => config != null ? config.ForwardLightIdleIntensityMultiplier : 0f;
        private float ForwardLightRange => config != null ? config.ForwardLightRange : 5.5f;
        private float ForwardLightSpotAngle => config != null ? config.ForwardLightSpotAngle : 80f;
        private float ForwardLightDownAngle => config != null ? config.ForwardLightDownAngle : 48f;
        private Vector3 ForwardLightOffset => config != null ? config.ForwardLightOffset : new Vector3(0f, 2.15f, 0.9f);
        private bool DashPulseEnabled => config == null || config.DashPulseEnabled;
        private Color DashPulseColor => config != null ? config.DashPulseColor : new Color(0.78f, 0.92f, 0.9f, 1f);
        private float DashPulseIntensity => config != null ? config.DashPulseIntensity : 2.4f;
        private float DashPulseRange => config != null ? config.DashPulseRange : 8.5f;
        private float DashPulseDuration => config != null ? config.DashPulseDuration : 0.18f;
        private Vector3 DashPulseOffset => config != null ? config.DashPulseOffset : new Vector3(0f, 2.35f, 1.25f);
    }
}
