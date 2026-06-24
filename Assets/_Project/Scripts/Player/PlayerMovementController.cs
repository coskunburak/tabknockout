using TapKnockout.Input;
using UnityEngine;

namespace TapKnockout.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PlayerMovementController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private PlayerConfig config;

        [Header("Input")]
        [SerializeField] private MonoBehaviour inputSourceBehaviour;
        [SerializeField] private bool findInputSourceOnSameObject = true;

        [Header("Fallback Values")]
        [SerializeField, Min(0f)] private float fallbackMoveSpeed = 5f;
        [SerializeField, Min(0f)] private float fallbackAcceleration = 45f;
        [SerializeField, Min(0f)] private float fallbackRotationSpeed = 720f;
        [SerializeField, Range(0f, 0.95f)] private float fallbackMovementDeadZone = 0.12f;
        [SerializeField, Min(0f)] private float fallbackStopToAttackMovementThreshold = 0.08f;

        private Rigidbody cachedRigidbody;
        private IPlayerInputSource inputSource;
        private bool loggedMissingConfig;
        private bool loggedMissingInputSource;

        public bool IsMoving { get; private set; }
        public bool IsMovingAboveAttackThreshold { get; private set; }
        public bool IsMovementLocked { get; private set; }
        public Vector3 CurrentMoveDirection { get; private set; }
        public Vector3 LastFacingDirection { get; private set; } = Vector3.forward;

        private float MoveSpeed => config != null ? config.MoveSpeed : fallbackMoveSpeed;
        private float Acceleration => config != null ? config.Acceleration : fallbackAcceleration;
        private float RotationSpeed => config != null ? config.RotationSpeed : fallbackRotationSpeed;
        private float MovementDeadZone => config != null ? config.MovementDeadZone : fallbackMovementDeadZone;
        private float StopToAttackMovementThreshold => config != null
            ? config.StopToAttackMovementThreshold
            : fallbackStopToAttackMovementThreshold;

        private void Reset()
        {
            cachedRigidbody = GetComponent<Rigidbody>();
            if (cachedRigidbody != null)
            {
                cachedRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                cachedRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                cachedRigidbody.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
        }

        private void Awake()
        {
            cachedRigidbody = GetComponent<Rigidbody>();
            ResolveInputSource();
            InitializeFacingDirection();
        }

        private void FixedUpdate()
        {
            if (IsMovementLocked)
            {
                CurrentMoveDirection = Vector3.zero;
                IsMoving = false;
                IsMovingAboveAttackThreshold = true;
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
            var targetDirection = ToWorldMoveDirection(moveInput);
            var isInputMoving = targetDirection.sqrMagnitude > 0f;

            CurrentMoveDirection = isInputMoving ? targetDirection : Vector3.zero;
            IsMoving = isInputMoving;

            if (isInputMoving)
            {
                LastFacingDirection = targetDirection;
            }

            MoveRigidbody(targetDirection);
            RotateTowardMovement(targetDirection);
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
            IsMovingAboveAttackThreshold = true;

            if (cachedRigidbody != null)
            {
                var velocity = cachedRigidbody.linearVelocity;
                cachedRigidbody.linearVelocity = new Vector3(0f, velocity.y, 0f);
            }
        }

        private void ResolveInputSource()
        {
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
            var targetHorizontalVelocity = targetDirection * MoveSpeed;
            var currentVelocity = cachedRigidbody.linearVelocity;
            var currentHorizontalVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
            var newHorizontalVelocity = Vector3.MoveTowards(
                currentHorizontalVelocity,
                targetHorizontalVelocity,
                Acceleration * Time.fixedDeltaTime);

            cachedRigidbody.linearVelocity = new Vector3(newHorizontalVelocity.x, currentVelocity.y, newHorizontalVelocity.z);
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
            var velocity = cachedRigidbody.linearVelocity;
            var horizontalSpeed = new Vector2(velocity.x, velocity.z).magnitude;
            IsMovingAboveAttackThreshold = horizontalSpeed > StopToAttackMovementThreshold;
        }
    }
}
