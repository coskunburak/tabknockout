using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TapKnockout.Input
{
    /// <summary>
    /// Keyboard and pointer-backed input source for early Editor validation and future mobile drag-anywhere control.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public sealed class PlayerInputReader : MonoBehaviour, IPlayerInputSource
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float movementDeadZone = 0.12f;

        [Header("Editor / Development Input")]
        [SerializeField] private bool enableKeyboardInput = true;
        [SerializeField] private bool enablePointerDragInput = true;
        [SerializeField] private bool enableMouseDragInput;
        [SerializeField, Min(1f)] private float pointerPixelsForFullInput = 120f;

        private Vector2 pointerStartPosition;
        private bool isPointerActive;

        public PlayerInputState CurrentInput { get; private set; } = PlayerInputState.None();
        public Vector2 MoveInput => CurrentInput.MoveInput;
        public Vector2 LastNonZeroMoveInput => CurrentInput.LastNonZeroMoveInput;
        public bool IsMovePressed => CurrentInput.IsMovePressed;
        public bool IsMovingAboveThreshold => CurrentInput.IsMovingAboveThreshold;

        public void SetMovementDeadZone(float newMovementDeadZone)
        {
            movementDeadZone = Mathf.Max(0f, newMovementDeadZone);
        }

        private void Update()
        {
            var rawMoveInput = ReadRawMoveInput();
            CurrentInput = new PlayerInputState(rawMoveInput, CurrentInput.LastNonZeroMoveInput, movementDeadZone);
        }

        private void OnDisable()
        {
            isPointerActive = false;
            CurrentInput = PlayerInputState.None(movementDeadZone);
        }

        private Vector2 ReadRawMoveInput()
        {
            var keyboardInput = enableKeyboardInput ? ReadKeyboardInput() : Vector2.zero;
            var pointerInput = enablePointerDragInput ? ReadPointerDragInput() : Vector2.zero;

            return pointerInput.sqrMagnitude > keyboardInput.sqrMagnitude ? pointerInput : keyboardInput;
        }

        private static Vector2 ClampInput(Vector2 input)
        {
            return Vector2.ClampMagnitude(input, 1f);
        }

        private Vector2 ReadKeyboardInput()
        {
            var input = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                {
                    input.x -= 1f;
                }

                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                {
                    input.x += 1f;
                }

                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                {
                    input.y -= 1f;
                }

                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                {
                    input.y += 1f;
                }
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            input += new Vector2(UnityEngine.Input.GetAxisRaw("Horizontal"), UnityEngine.Input.GetAxisRaw("Vertical"));
#endif

            return ClampInput(input);
        }

        private Vector2 ReadPointerDragInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                var touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                return ReadPointerPosition(touchPosition, Touchscreen.current.primaryTouch.press.wasPressedThisFrame);
            }

            if (enableMouseDragInput &&
                Mouse.current != null &&
                Mouse.current.leftButton.isPressed)
            {
                var mousePosition = Mouse.current.position.ReadValue();
                return ReadPointerPosition(mousePosition, Mouse.current.leftButton.wasPressedThisFrame);
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (UnityEngine.Input.touchCount > 0)
            {
                var touch = UnityEngine.Input.GetTouch(0);
                return ReadPointerPosition(touch.position, touch.phase == TouchPhase.Began);
            }

            if (enableMouseDragInput && UnityEngine.Input.GetMouseButton(0))
            {
                return ReadPointerPosition(UnityEngine.Input.mousePosition, UnityEngine.Input.GetMouseButtonDown(0));
            }
#endif

            isPointerActive = false;
            return Vector2.zero;
        }

        private Vector2 ReadPointerPosition(Vector2 currentPosition, bool startedThisFrame)
        {
            if (startedThisFrame || !isPointerActive)
            {
                pointerStartPosition = currentPosition;
                isPointerActive = true;
            }

            var dragDelta = currentPosition - pointerStartPosition;
            return ClampInput(dragDelta / pointerPixelsForFullInput);
        }
    }
}
