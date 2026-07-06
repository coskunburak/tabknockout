using TapKnockout.Input;
using UnityEngine;

namespace TapKnockout.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PlayerMovementController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private PlayerConfig config;
        [SerializeField] private PlayerRuntimeStats runtimeStats;

        [Header("Input")]
        [SerializeField] private MonoBehaviour inputSourceBehaviour;
        [SerializeField] private bool findInputSourceOnSameObject = true;
        [SerializeField] private bool preferDesktopInputReader = true;

        [Header("Rigidbody")]
        [SerializeField] private bool enforceHorizontalRigidbodyMovement = true;
        [SerializeField] private bool useKinematicMotor = true;
        [SerializeField] private bool disableGravityForTopDownMotor = true;

        [Header("Facing")]
        [SerializeField] private bool rotateTowardMovement = true;

        [Header("Fallback Values")]
        [SerializeField, Min(0f)] private float fallbackMoveSpeed = 5f;
        [SerializeField, Min(0f)] private float fallbackAcceleration = 45f;
        [SerializeField, Min(0f)] private float fallbackDeceleration = 55f;
        [SerializeField, Min(0f)] private float fallbackRotationSpeed = 720f;
        [SerializeField, Range(0f, 0.95f)] private float fallbackMovementDeadZone = 0.12f;
        [SerializeField] private bool fallbackUseMovementSmoothing = true;
        [SerializeField, Min(0f)] private float fallbackStopToAttackMovementThreshold = 0.08f;

        [Header("Debug")]
        [SerializeField] private Vector2 debugMoveInput;
        [SerializeField] private Vector3 debugTargetDirection;
        [SerializeField] private Vector3 debugHorizontalVelocity;

        private Rigidbody cachedRigidbody;
        private IPlayerInputSource inputSource;
        private bool loggedMissingConfig;
        private bool loggedMissingInputSource;
        private Vector3 internalSmoothedVelocity;

        public bool IsMoving { get; private set; }
        public bool IsMovingAboveAttackThreshold { get; private set; }
        public bool IsMovementLocked { get; private set; }
        public Vector3 CurrentMoveDirection { get; private set; }
        public Vector3 LastFacingDirection { get; private set; } = Vector3.forward;

        public PlayerConfig Config => config;

        private float MoveSpeed => config != null ? config.MoveSpeed : fallbackMoveSpeed;
        private float EffectiveMoveSpeed => MoveSpeed * (runtimeStats != null ? runtimeStats.MoveSpeedMultiplier : 1f);
        private float Acceleration => config != null ? config.Acceleration : fallbackAcceleration;
        private float Deceleration => config != null ? config.Deceleration : fallbackDeceleration;
        private float RotationSpeed => config != null ? config.RotationSpeed : fallbackRotationSpeed;
        private float MovementDeadZone => config != null ? config.MovementDeadZone : fallbackMovementDeadZone;
        private bool UseMovementSmoothing => config != null ? config.UseMovementSmoothing : fallbackUseMovementSmoothing;
        private float StopToAttackMovementThreshold => config != null
            ? config.StopToAttackMovementThreshold
            : fallbackStopToAttackMovementThreshold;
        private void Reset()
        {
            cachedRigidbody = GetComponent<Rigidbody>();
            if (cachedRigidbody != null)
            {
                ConfigureRigidbodyForMovement();
            }

            runtimeStats = GetComponent<PlayerRuntimeStats>();
        }

        private void Awake()
        {
            cachedRigidbody = GetComponent<Rigidbody>();
            ConfigureRigidbodyForMovement();
            if (runtimeStats == null)
            {
                runtimeStats = GetComponent<PlayerRuntimeStats>();
            }

            ResolveInputSource();
            InitializeFacingDirection();
        }

        private void FixedUpdate()
        {
            if (IsMovementLocked)
            {
                CurrentMoveDirection = Vector3.zero;
                IsMoving = false;
                IsMovingAboveAttackThreshold = false;
                StopHorizontalMotion();
                return;
            }

            if (inputSource == null)
            {
                ResolveInputSource();
            }

            if (config == null && !loggedMissingConfig)
            {
                loggedMissingConfig = true;
                Debug.LogWarning($"{nameof(PlayerMovementController)} on {name} has no PlayerConfig assigned. Fallback movement values are being used.", this);
            }

            inputSource?.SetMovementDeadZone(MovementDeadZone);

            var moveInput = inputSource != null ? inputSource.MoveInput : Vector2.zero;
            debugMoveInput = moveInput;
            var targetDirection = ToWorldMoveDirection(moveInput);
            debugTargetDirection = targetDirection;
            var isInputMoving = targetDirection.sqrMagnitude > 0f;

            CurrentMoveDirection = isInputMoving ? targetDirection : Vector3.zero;
            IsMoving = isInputMoving;

            if (isInputMoving)
            {
                LastFacingDirection = targetDirection;
            }

            MoveRigidbody(targetDirection);
            if (rotateTowardMovement)
            {
                RotateTowardMovement(targetDirection);
            }

            UpdateAttackMovementState();
        }

        public void SetMovementLocked(bool isLocked)
        {
            IsMovementLocked = isLocked;

            if (!isLocked)
            {
                return;
            }

            CurrentMoveDirection = Vector3.zero;
            IsMoving = false;
            IsMovingAboveAttackThreshold = false;
            internalSmoothedVelocity = Vector3.zero;

            if (cachedRigidbody != null)
            {
                StopHorizontalMotion();
            }
        }

        public void SetInputSource(MonoBehaviour inputSource)
        {
            inputSourceBehaviour = inputSource;
            this.inputSource = inputSourceBehaviour as IPlayerInputSource;
            loggedMissingInputSource = false;

            if (this.inputSource == null && inputSourceBehaviour != null)
            {
                Debug.LogWarning($"{inputSourceBehaviour.name} does not implement {nameof(IPlayerInputSource)} and cannot drive {nameof(PlayerMovementController)}.", this);
            }
        }

        public void SetRotateTowardMovement(bool shouldRotateTowardMovement)
        {
            rotateTowardMovement = shouldRotateTowardMovement;
        }

        private void ResolveInputSource()
        {
            if (preferDesktopInputReader)
            {
                var desktopInputReader = GetComponent<DesktopInputReader>();
                if (desktopInputReader != null &&
                    (inputSourceBehaviour == null ||
                        inputSourceBehaviour is PlayerInputReader ||
                        inputSourceBehaviour == desktopInputReader))
                {
                    inputSource = desktopInputReader;
                    inputSourceBehaviour = desktopInputReader;
                    return;
                }
            }

            inputSource = inputSourceBehaviour as IPlayerInputSource;

            if (inputSource == null && findInputSourceOnSameObject)
            {
                inputSource = GetComponent<IPlayerInputSource>();
                inputSourceBehaviour = inputSource as MonoBehaviour;
            }

            if (inputSource == null && !loggedMissingInputSource)
            {
                loggedMissingInputSource = true;
                Debug.LogWarning($"{nameof(PlayerMovementController)} on {name} has no input source. Add PlayerInputReader or assign another IPlayerInputSource.", this);
            }
        }

        private void InitializeFacingDirection()
        {
            var forward = transform.forward;
            forward.y = 0f;
            LastFacingDirection = forward.sqrMagnitude > 0f ? forward.normalized : Vector3.forward;
        }

        private static Vector3 ToWorldMoveDirection(Vector2 moveInput)
        {
            var direction = new Vector3(moveInput.x, 0f, moveInput.y);
            return direction.sqrMagnitude > 1f ? direction.normalized : direction;
        }

        private void MoveRigidbody(Vector3 targetDirection)
        {
            var targetHorizontalVelocity = targetDirection * EffectiveMoveSpeed;
            var newHorizontalVelocity = targetHorizontalVelocity;

            if (UseMovementSmoothing)
            {
                var rate = targetDirection.sqrMagnitude > 0f ? Acceleration : Deceleration;

                // Do NOT use currentVelocity from the Rigidbody for smoothing.
                // Physics engine collision resolution (like bumping into arena walls)
                // alters currentVelocity, which causes a "pulling/dragging" feeling
                // when interpolated back towards input. We use an internal state instead.
                internalSmoothedVelocity = Vector3.MoveTowards(
                    internalSmoothedVelocity,
                    targetHorizontalVelocity,
                    rate * Time.fixedDeltaTime);

                newHorizontalVelocity = internalSmoothedVelocity;
            }
            else
            {
                internalSmoothedVelocity = targetHorizontalVelocity;
            }

            if (targetDirection.sqrMagnitude <= 0.0001f)
            {
                newHorizontalVelocity = Vector3.zero;
                internalSmoothedVelocity = Vector3.zero;
                StopHorizontalMotion();
                debugHorizontalVelocity = Vector3.zero;
                return;
            }

            ApplyHorizontalMotion(newHorizontalVelocity);
            debugHorizontalVelocity = newHorizontalVelocity;
        }

        private void StopHorizontalMotion()
        {
            if (cachedRigidbody == null)
            {
                cachedRigidbody = GetComponent<Rigidbody>();
                if (cachedRigidbody == null)
                {
                    return;
                }
            }

            var verticalVelocity = cachedRigidbody.isKinematic ? 0f : cachedRigidbody.linearVelocity.y;
            StopHorizontalMotion(verticalVelocity);
        }

        private void StopHorizontalMotion(float verticalVelocity)
        {
            internalSmoothedVelocity = Vector3.zero;
            if (!cachedRigidbody.isKinematic)
            {
                cachedRigidbody.linearVelocity = new Vector3(0f, verticalVelocity, 0f);
                cachedRigidbody.angularVelocity = Vector3.zero;
            }

            debugHorizontalVelocity = Vector3.zero;
        }

        private void ApplyHorizontalMotion(Vector3 horizontalVelocity)
        {
            if (cachedRigidbody.isKinematic)
            {
                var currentPosition = cachedRigidbody.position;
                var targetPosition = currentPosition + horizontalVelocity * Time.fixedDeltaTime;
                targetPosition.y = currentPosition.y;
                cachedRigidbody.MovePosition(targetPosition);
                return;
            }

            var currentVelocity = cachedRigidbody.linearVelocity;
            cachedRigidbody.linearVelocity = new Vector3(horizontalVelocity.x, currentVelocity.y, horizontalVelocity.z);
        }

        private void RotateTowardMovement(Vector3 targetDirection)
        {
            if (targetDirection.sqrMagnitude <= 0f || RotationSpeed <= 0f)
            {
                return;
            }

            var targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
            var newRotation = Quaternion.RotateTowards(
                cachedRigidbody.rotation,
                targetRotation,
                RotationSpeed * Time.fixedDeltaTime);

            cachedRigidbody.MoveRotation(newRotation);
        }

        private void UpdateAttackMovementState()
        {
            var horizontalSpeed = cachedRigidbody.isKinematic
                ? new Vector2(internalSmoothedVelocity.x, internalSmoothedVelocity.z).magnitude
                : new Vector2(cachedRigidbody.linearVelocity.x, cachedRigidbody.linearVelocity.z).magnitude;
            IsMovingAboveAttackThreshold = horizontalSpeed > StopToAttackMovementThreshold;
        }

        private void ConfigureRigidbodyForMovement()
        {
            if (cachedRigidbody == null || !enforceHorizontalRigidbodyMovement)
            {
                return;
            }

            cachedRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            if (useKinematicMotor)
            {
                cachedRigidbody.isKinematic = true;
                cachedRigidbody.useGravity = !disableGravityForTopDownMotor;
                cachedRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            }
            else
            {
                cachedRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }

            cachedRigidbody.constraints &= ~RigidbodyConstraints.FreezePositionX;
            cachedRigidbody.constraints &= ~RigidbodyConstraints.FreezePositionZ;
            cachedRigidbody.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            if (!useKinematicMotor && cachedRigidbody.isKinematic)
            {
                Debug.LogWarning($"{nameof(PlayerMovementController)} on {name} has a kinematic Rigidbody. WASD velocity movement requires Is Kinematic to be off.", this);
            }
        }
    }
}
