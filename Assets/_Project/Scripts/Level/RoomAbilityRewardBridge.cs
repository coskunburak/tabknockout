using TapKnockout.Ability;
using TapKnockout.Room;
using UnityEngine;

namespace TapKnockout.Level
{
    [DisallowMultipleComponent]
    public sealed class RoomAbilityRewardBridge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RoomManager roomManager;
        [SerializeField] private AbilitySelectionController abilitySelectionController;
        [SerializeField] private MonoBehaviour abilitySelectionPanel;

        [Header("Runtime")]
        [SerializeField] private bool pauseGameWhileSelecting = true;
        [Tooltip("Legacy bridge. Keep disabled when ChapterRoomRewardFlowController is present.")]
        [SerializeField] private bool generateOfferWhenRoomCompletes;

        [Header("Debug")]
        [SerializeField] private bool logDebug;

        private bool isWaitingForSelection;
        private bool isPausedByBridge;
        private float previousTimeScale = 1f;

        public bool IsWaitingForSelection => isWaitingForSelection;

        private void Reset()
        {
            roomManager = GetComponent<RoomManager>();
        }

        private void OnEnable()
        {
            Subscribe();
            ConfigurePanel();
        }

        private void OnDisable()
        {
            Unsubscribe();
            ResumeGameIfPaused();
        }

        public void SetReferences(
            RoomManager room,
            AbilitySelectionController selectionController,
            MonoBehaviour panel)
        {
            Unsubscribe();
            roomManager = room;
            abilitySelectionController = selectionController;
            abilitySelectionPanel = panel;
            Subscribe();
            ConfigurePanel();
        }

        [ContextMenu("Request Ability Offer")]
        public void RequestAbilityOfferFromContextMenu()
        {
            RequestAbilityOffer();
        }

        public bool RequestAbilityOffer()
        {
            if (abilitySelectionController == null)
            {
                if (logDebug)
                {
                    Debug.LogWarning($"{nameof(RoomAbilityRewardBridge)} on {name} cannot generate an ability offer because no {nameof(AbilitySelectionController)} is assigned.", this);
                }

                return false;
            }

            ConfigurePanel();
            PauseGameIfNeeded();

            var choices = abilitySelectionController.GenerateOffer();
            if (choices == null || choices.Count == 0)
            {
                isWaitingForSelection = false;
                ResumeGameIfPaused();
                return false;
            }

            isWaitingForSelection = true;

            if (logDebug)
            {
                Debug.Log($"{nameof(RoomAbilityRewardBridge)} generated an ability offer after room completion.", this);
            }

            return true;
        }

        private void Subscribe()
        {
            if (roomManager != null)
            {
                roomManager.OnRoomCompleted -= HandleRoomCompleted;
                roomManager.OnRoomCompleted += HandleRoomCompleted;
            }

            if (abilitySelectionController != null)
            {
                abilitySelectionController.OnAbilitySelected -= HandleAbilitySelected;
                abilitySelectionController.OnAbilitySelected += HandleAbilitySelected;
                abilitySelectionController.OnAbilityOfferCleared -= HandleOfferCleared;
                abilitySelectionController.OnAbilityOfferCleared += HandleOfferCleared;
            }
        }

        private void Unsubscribe()
        {
            if (roomManager != null)
            {
                roomManager.OnRoomCompleted -= HandleRoomCompleted;
            }

            if (abilitySelectionController != null)
            {
                abilitySelectionController.OnAbilitySelected -= HandleAbilitySelected;
                abilitySelectionController.OnAbilityOfferCleared -= HandleOfferCleared;
            }
        }

        private void ConfigurePanel()
        {
            if (abilitySelectionPanel == null || abilitySelectionController == null)
            {
                return;
            }

            var panelView = abilitySelectionPanel as IAbilitySelectionPanelView;
            if (panelView == null)
            {
                if (logDebug)
                {
                    Debug.LogWarning($"{abilitySelectionPanel.name} is assigned as an ability selection panel but does not implement {nameof(IAbilitySelectionPanelView)}.", this);
                }

                return;
            }

            panelView.SetAbilitySelectionController(abilitySelectionController);
            panelView.SetPauseGameWhileOpen(!pauseGameWhileSelecting);
        }

        private void HandleRoomCompleted(RoomCompletedEventArgs eventArgs)
        {
            if (!generateOfferWhenRoomCompletes)
            {
                return;
            }

            RequestAbilityOffer();
        }

        private void HandleAbilitySelected(AbilitySelectedEventArgs eventArgs)
        {
            isWaitingForSelection = false;
            ResumeGameIfPaused();
        }

        private void HandleOfferCleared(AbilityOfferEventArgs eventArgs)
        {
            isWaitingForSelection = false;
            ResumeGameIfPaused();
        }

        private void PauseGameIfNeeded()
        {
            if (!pauseGameWhileSelecting || isPausedByBridge)
            {
                return;
            }

            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            isPausedByBridge = true;
        }

        private void ResumeGameIfPaused()
        {
            if (!isPausedByBridge)
            {
                return;
            }

            Time.timeScale = previousTimeScale;
            isPausedByBridge = false;
        }
    }
}
