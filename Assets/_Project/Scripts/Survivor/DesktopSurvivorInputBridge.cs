using System;
using TapKnockout.Input;
using TapKnockout.Player;
using UnityEngine;

namespace TapKnockout.Survivor
{
    [DefaultExecutionOrder(-40)]
    [DisallowMultipleComponent]
    public sealed class DesktopSurvivorInputBridge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DesktopInputReader inputReader;
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private PlayerDashController dashController;
        [SerializeField] private ArenaRunDirector runDirector;
        [SerializeField] private ActiveSkillController activeSkillController;

        [Header("Runtime")]
        [SerializeField] private bool resetTimeScaleOnEnable = true;

        [Header("Debug")]
        [SerializeField] private bool logActiveSkillPlaceholders = true;

        public event Action<int> OnActiveSkillPressed;

        private void Reset()
        {
            inputReader = GetComponent<DesktopInputReader>();
            movementController = GetComponent<PlayerMovementController>();
            dashController = GetComponent<PlayerDashController>();
        }

        private void OnEnable()
        {
            if (resetTimeScaleOnEnable && Time.timeScale <= 0f)
            {
                Time.timeScale = 1f;
            }
        }

        private void Awake()
        {
            if (inputReader == null)
            {
                inputReader = GetComponent<DesktopInputReader>();
                if (inputReader == null)
                {
                    inputReader = gameObject.AddComponent<DesktopInputReader>();
                }
            }

            if (movementController == null)
            {
                movementController = GetComponent<PlayerMovementController>();
            }

            if (dashController == null)
            {
                dashController = GetComponent<PlayerDashController>();
            }

            if (activeSkillController == null)
            {
                activeSkillController = GetComponent<ActiveSkillController>();
            }

            movementController?.SetInputSource(inputReader);
            movementController?.SetRotateTowardMovement(false);
        }

        public void Configure(
            DesktopInputReader desktopInputReader,
            PlayerMovementController playerMovementController,
            PlayerDashController playerDashController,
            ArenaRunDirector arenaRunDirector)
        {
            inputReader = desktopInputReader;
            movementController = playerMovementController;
            dashController = playerDashController;
            runDirector = arenaRunDirector;
            activeSkillController = GetComponent<ActiveSkillController>();
            movementController?.SetInputSource(inputReader);
            movementController?.SetRotateTowardMovement(false);
        }

        public void SetActiveSkillController(ActiveSkillController controller)
        {
            activeSkillController = controller;
        }

        private void Update()
        {
            if (inputReader == null)
            {
                return;
            }

            if (inputReader.DashPressedThisFrame)
            {
                dashController?.TryDash();
            }

            if (inputReader.PausePressedThisFrame)
            {
                runDirector?.ToggleManualPause();
            }

            for (var slot = 0; slot < 4; slot++)
            {
                if (!inputReader.WasActiveSkillPressedThisFrame(slot))
                {
                    continue;
                }

                OnActiveSkillPressed?.Invoke(slot);
                if (activeSkillController == null && logActiveSkillPlaceholders)
                {
                    Debug.Log($"Active skill slot {slot + 1} pressed, but no ActiveSkillController is assigned.", this);
                }
            }
        }
    }
}
