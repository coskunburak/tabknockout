using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TapKnockout.Input
{
    [DefaultExecutionOrder(-50)]
    [DisallowMultipleComponent]
    public sealed class DesktopInputReader : MonoBehaviour, IPlayerInputSource
    {
        [Header("Movement")]
        [SerializeField, Range(0f, 0.95f)] private float movementDeadZone = 0.12f;
        [SerializeField] private bool allowArrowKeys = true;

        [Header("Actions")]
        [SerializeField] private KeyCode legacyDashKey = KeyCode.Space;
        [SerializeField] private KeyCode legacyAlternateDashKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode legacyPauseKey = KeyCode.Escape;

        [Header("Debug")]
        [SerializeField] private Vector2 debugMoveInput;
        [SerializeField] private bool debugDashPressedThisFrame;
        [SerializeField] private bool debugPrimaryFireHeld;
        [SerializeField] private bool debugPrimaryFirePressedThisFrame;
        [SerializeField] private bool debugPrimaryFireReleasedThisFrame;

        private PlayerInputState currentInput = PlayerInputState.None();
        private Vector2 lastNonZeroMoveInput;
        private readonly bool[] activeSkillPressedThisFrame = new bool[4];

        public PlayerInputState CurrentInput => currentInput;
        public Vector2 MoveInput => currentInput.MoveInput;
        public Vector2 LastNonZeroMoveInput => currentInput.LastNonZeroMoveInput;
        public bool IsMovePressed => currentInput.IsMovePressed;
        public bool IsMovingAboveThreshold => currentInput.IsMovingAboveThreshold;
        public bool DashPressedThisFrame { get; private set; }
        public bool PausePressedThisFrame { get; private set; }
        public bool PrimaryFireHeld { get; private set; }
        public bool PrimaryFirePressedThisFrame { get; private set; }
        public bool PrimaryFireReleasedThisFrame { get; private set; }

        private void Update()
        {
            currentInput = new PlayerInputState(ReadMovement(), lastNonZeroMoveInput, movementDeadZone);
            lastNonZeroMoveInput = currentInput.LastNonZeroMoveInput;
            debugMoveInput = currentInput.MoveInput;

            DashPressedThisFrame = ReadDashPressedThisFrame();
            PausePressedThisFrame = ReadPausePressedThisFrame();
            PrimaryFireHeld = ReadPrimaryFireHeld();
            PrimaryFirePressedThisFrame = ReadPrimaryFirePressedThisFrame();
            PrimaryFireReleasedThisFrame = ReadPrimaryFireReleasedThisFrame();
            debugDashPressedThisFrame = DashPressedThisFrame;
            debugPrimaryFireHeld = PrimaryFireHeld;
            debugPrimaryFirePressedThisFrame = PrimaryFirePressedThisFrame;
            debugPrimaryFireReleasedThisFrame = PrimaryFireReleasedThisFrame;

            for (var i = 0; i < activeSkillPressedThisFrame.Length; i++)
            {
                activeSkillPressedThisFrame[i] = ReadActiveSkillPressedThisFrame(i);
            }
        }

        public void SetMovementDeadZone(float deadZone)
        {
            movementDeadZone = Mathf.Clamp(deadZone, 0f, 0.95f);
        }

        public bool WasActiveSkillPressedThisFrame(int slotIndex)
        {
            return slotIndex >= 0 &&
                slotIndex < activeSkillPressedThisFrame.Length &&
                activeSkillPressedThisFrame[slotIndex];
        }

        private Vector2 ReadMovement()
        {
            var movement = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || allowArrowKeys && keyboard.leftArrowKey.isPressed)
                {
                    movement.x -= 1f;
                }

                if (keyboard.dKey.isPressed || allowArrowKeys && keyboard.rightArrowKey.isPressed)
                {
                    movement.x += 1f;
                }

                if (keyboard.sKey.isPressed || allowArrowKeys && keyboard.downArrowKey.isPressed)
                {
                    movement.y -= 1f;
                }

                if (keyboard.wKey.isPressed || allowArrowKeys && keyboard.upArrowKey.isPressed)
                {
                    movement.y += 1f;
                }

                return Vector2.ClampMagnitude(movement, 1f);
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (UnityEngine.Input.GetKey(KeyCode.A) || allowArrowKeys && UnityEngine.Input.GetKey(KeyCode.LeftArrow))
            {
                movement.x -= 1f;
            }

            if (UnityEngine.Input.GetKey(KeyCode.D) || allowArrowKeys && UnityEngine.Input.GetKey(KeyCode.RightArrow))
            {
                movement.x += 1f;
            }

            if (UnityEngine.Input.GetKey(KeyCode.S) || allowArrowKeys && UnityEngine.Input.GetKey(KeyCode.DownArrow))
            {
                movement.y -= 1f;
            }

            if (UnityEngine.Input.GetKey(KeyCode.W) || allowArrowKeys && UnityEngine.Input.GetKey(KeyCode.UpArrow))
            {
                movement.y += 1f;
            }
#endif

            return Vector2.ClampMagnitude(movement, 1f);
        }

        private bool ReadDashPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                return keyboard.spaceKey.wasPressedThisFrame ||
                    keyboard.leftShiftKey.wasPressedThisFrame ||
                    keyboard.rightShiftKey.wasPressedThisFrame;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.GetKeyDown(legacyDashKey) ||
                UnityEngine.Input.GetKeyDown(legacyAlternateDashKey);
#else
            return false;
#endif
        }

        private bool ReadPausePressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                return keyboard.escapeKey.wasPressedThisFrame;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.GetKeyDown(legacyPauseKey);
#else
            return false;
#endif
        }

        private static bool ReadPrimaryFireHeld()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                return Mouse.current.leftButton.isPressed;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.GetMouseButton(0);
#else
            return false;
#endif
        }

        private static bool ReadPrimaryFirePressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                return Mouse.current.leftButton.wasPressedThisFrame;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.GetMouseButtonDown(0);
#else
            return false;
#endif
        }

        private static bool ReadPrimaryFireReleasedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                return Mouse.current.leftButton.wasReleasedThisFrame;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.GetMouseButtonUp(0);
#else
            return false;
#endif
        }

        private static bool ReadActiveSkillPressedThisFrame(int slotIndex)
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                return slotIndex switch
                {
                    0 => keyboard.qKey.wasPressedThisFrame || keyboard.digit1Key.wasPressedThisFrame,
                    1 => keyboard.eKey.wasPressedThisFrame || keyboard.digit2Key.wasPressedThisFrame,
                    2 => keyboard.rKey.wasPressedThisFrame || keyboard.digit3Key.wasPressedThisFrame,
                    3 => keyboard.fKey.wasPressedThisFrame || keyboard.digit4Key.wasPressedThisFrame,
                    _ => false
                };
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return slotIndex switch
            {
                0 => UnityEngine.Input.GetKeyDown(KeyCode.Q) || UnityEngine.Input.GetKeyDown(KeyCode.Alpha1),
                1 => UnityEngine.Input.GetKeyDown(KeyCode.E) || UnityEngine.Input.GetKeyDown(KeyCode.Alpha2),
                2 => UnityEngine.Input.GetKeyDown(KeyCode.R) || UnityEngine.Input.GetKeyDown(KeyCode.Alpha3),
                3 => UnityEngine.Input.GetKeyDown(KeyCode.F) || UnityEngine.Input.GetKeyDown(KeyCode.Alpha4),
                _ => false
            };
#else
            return false;
#endif
        }
    }
}
