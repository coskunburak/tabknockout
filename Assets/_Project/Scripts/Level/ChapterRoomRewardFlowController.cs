using TapKnockout.Ability;
using TapKnockout.Combat;
using TapKnockout.Player;
using TapKnockout.Room;
using UnityEngine;

namespace TapKnockout.Level
{
    [DisallowMultipleComponent]
    public sealed class ChapterRoomRewardFlowController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ChapterRunner chapterRunner;
        [SerializeField] private RoomManager roomManager;
        [SerializeField] private AbilitySelectionController abilitySelectionController;
        [SerializeField] private MonoBehaviour abilitySelectionPanel;
        [SerializeField] private PlayerHealth playerHealth;

        [Header("Runtime")]
        [SerializeField] private bool pauseDuringAbilitySelection = true;
        [SerializeField] private bool autoContinueAfterAbilitySelection;

        [Header("Debug")]
        [SerializeField] private bool logDebug;

        private RoomManager subscribedRoomManager;
        private bool isWaitingForAbilitySelection;
        private bool isPausedByController;
        private bool resolvingAbilitySelection;
        private float previousTimeScale = 1f;
        private int handledRoomIndex = -1;
        private RoomCompletionDecisionResult pendingDecision;

        public ChapterFlowState FlowState => chapterRunner != null ? chapterRunner.FlowState : ChapterFlowState.Idle;
        public bool IsWaitingForAbilitySelection => isWaitingForAbilitySelection;
        public bool IsRewardPending => chapterRunner != null && chapterRunner.RunState.IsRewardPending;
        public bool AutoContinueAfterAbilitySelection => autoContinueAfterAbilitySelection;
        public bool CanContinueAfterReward => CanContinueAfterRewardNow();

        private void Reset()
        {
            chapterRunner = GetComponent<ChapterRunner>();
            roomManager = GetComponent<RoomManager>();
        }

        private void Awake()
        {
            if (chapterRunner == null)
            {
                chapterRunner = GetComponent<ChapterRunner>();
            }

            if (roomManager == null)
            {
                roomManager = GetComponent<RoomManager>();
            }
        }

        private void OnEnable()
        {
            Subscribe();
            ConfigurePanel();
            if (chapterRunner != null)
            {
                chapterRunner.SetExternalProgressionControl(true);
            }
        }

        private void OnDisable()
        {
            Unsubscribe();
            ResumeGameIfPaused();
            if (chapterRunner != null)
            {
                chapterRunner.SetExternalProgressionControl(false);
            }
        }

        public void SetReferences(
            ChapterRunner runner,
            RoomManager room,
            AbilitySelectionController selectionController,
            MonoBehaviour panel,
            PlayerHealth health)
        {
            Unsubscribe();
            chapterRunner = runner;
            roomManager = room;
            abilitySelectionController = selectionController;
            abilitySelectionPanel = panel;
            playerHealth = health;
            Subscribe();
            ConfigurePanel();

            if (chapterRunner != null)
            {
                chapterRunner.SetExternalProgressionControl(true);
            }
        }

        [ContextMenu("Continue After Reward")]
        public void ContinueAfterReward()
        {
            TryContinueAfterReward();
        }

        public bool TryContinueAfterReward()
        {
            if (!CanContinueAfterRewardNow())
            {
                if (logDebug)
                {
                    Debug.LogWarning($"{nameof(ChapterRoomRewardFlowController)} cannot continue after reward: {BuildContinueBlockReason()}", this);
                }

                return false;
            }

            ProceedToNextRoom();
            return true;
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
                    Debug.LogWarning($"{nameof(ChapterRoomRewardFlowController)} on {name} cannot generate an ability offer because no {nameof(AbilitySelectionController)} is assigned.", this);
                }

                return false;
            }

            ConfigurePanel();
            PauseGameIfNeeded();
            var choices = abilitySelectionController.GenerateOffer();
            if (choices == null || choices.Count == 0)
            {
                isWaitingForAbilitySelection = false;
                ResumeGameIfPaused();
                return false;
            }

            isWaitingForAbilitySelection = true;
            return true;
        }

        private void Subscribe()
        {
            if (chapterRunner != null)
            {
                chapterRunner.OnChapterRoomChanged -= HandleChapterRoomChanged;
                chapterRunner.OnChapterRoomChanged += HandleChapterRoomChanged;
            }

            SubscribeRoomManager(roomManager ?? chapterRunner?.ActiveRoomManager);

            if (abilitySelectionController != null)
            {
                abilitySelectionController.OnAbilitySelected -= HandleAbilitySelected;
                abilitySelectionController.OnAbilitySelected += HandleAbilitySelected;
                abilitySelectionController.OnAbilityOfferCleared -= HandleAbilityOfferCleared;
                abilitySelectionController.OnAbilityOfferCleared += HandleAbilityOfferCleared;
            }

            if (playerHealth != null)
            {
                playerHealth.OnPlayerDied -= HandlePlayerDied;
                playerHealth.OnPlayerDied += HandlePlayerDied;
            }
        }

        private void Unsubscribe()
        {
            if (chapterRunner != null)
            {
                chapterRunner.OnChapterRoomChanged -= HandleChapterRoomChanged;
            }

            SubscribeRoomManager(null);

            if (abilitySelectionController != null)
            {
                abilitySelectionController.OnAbilitySelected -= HandleAbilitySelected;
                abilitySelectionController.OnAbilityOfferCleared -= HandleAbilityOfferCleared;
            }

            if (playerHealth != null)
            {
                playerHealth.OnPlayerDied -= HandlePlayerDied;
            }
        }

        private void SubscribeRoomManager(RoomManager nextRoomManager)
        {
            if (subscribedRoomManager == nextRoomManager)
            {
                return;
            }

            if (subscribedRoomManager != null)
            {
                subscribedRoomManager.OnRoomCompleted -= HandleRoomCompleted;
            }

            subscribedRoomManager = nextRoomManager;

            if (subscribedRoomManager != null)
            {
                subscribedRoomManager.OnRoomCompleted -= HandleRoomCompleted;
                subscribedRoomManager.OnRoomCompleted += HandleRoomCompleted;
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
            panelView.SetPauseGameWhileOpen(!pauseDuringAbilitySelection);
        }

        private void HandleChapterRoomChanged(ChapterRoomChangedEventArgs eventArgs)
        {
            handledRoomIndex = -1;
            SubscribeRoomManager(chapterRunner != null ? chapterRunner.ActiveRoomManager : roomManager);
        }

        private void HandleRoomCompleted(RoomCompletedEventArgs eventArgs)
        {
            if (chapterRunner == null || chapterRunner.IsChapterCompleted || chapterRunner.IsChapterFailed)
            {
                return;
            }

            var roomIndex = chapterRunner.CurrentRoomIndex;
            if (handledRoomIndex == roomIndex)
            {
                return;
            }

            handledRoomIndex = roomIndex;
            var roomConfig = eventArgs.RoomConfig != null ? eventArgs.RoomConfig : chapterRunner.CurrentRoomConfig;
            var playerAlive = playerHealth == null || playerHealth.IsAlive;
            pendingDecision = RoomCompletionDecision.Evaluate(roomConfig, roomIndex, chapterRunner.TotalRoomCount, playerAlive);
            chapterRunner.RunState.MarkRoomCompleted();

            if (pendingDecision.ShouldFailChapter)
            {
                FailChapter("player_dead");
                return;
            }

            if (pendingDecision.ShouldCompleteChapter)
            {
                chapterRunner.CompleteChapter();
                return;
            }

            if (pendingDecision.ShouldOpenAbilitySelection)
            {
                chapterRunner.RunState.MarkAbilitySelectionPending();
                chapterRunner.SetFlowState(ChapterFlowState.AbilitySelectionPending);

                if (!RequestAbilityOffer())
                {
                    ResolveRewardWithoutAbility();
                }

                return;
            }

            if (pendingDecision.ShouldWaitForContinue)
            {
                chapterRunner.RunState.MarkWaitingForContinue();
                chapterRunner.SetFlowState(ChapterFlowState.WaitingForContinue);
                UnlockRoomExit(pendingDecision.RewardType);
                return;
            }

            if (pendingDecision.ShouldAutoAdvance)
            {
                UnlockRoomExit(pendingDecision.RewardType);
                ProceedToNextRoom();
            }
        }

        private void HandleAbilitySelected(AbilitySelectedEventArgs eventArgs)
        {
            if (!isWaitingForAbilitySelection || resolvingAbilitySelection)
            {
                return;
            }

            if (chapterRunner == null)
            {
                isWaitingForAbilitySelection = false;
                ResumeGameIfPaused();
                return;
            }

            resolvingAbilitySelection = true;
            isWaitingForAbilitySelection = false;
            ResumeGameIfPaused();

            if (autoContinueAfterAbilitySelection)
            {
                UnlockRoomExit(pendingDecision.RewardType);
                ProceedToNextRoom();
            }
            else
            {
                chapterRunner.RunState.MarkWaitingForContinue();
                chapterRunner.SetFlowState(ChapterFlowState.WaitingForContinue);
                UnlockRoomExit(pendingDecision.RewardType);
            }

            resolvingAbilitySelection = false;
        }

        private void HandleAbilityOfferCleared(AbilityOfferEventArgs eventArgs)
        {
            if (!isWaitingForAbilitySelection || resolvingAbilitySelection)
            {
                return;
            }

            isWaitingForAbilitySelection = false;
            ResumeGameIfPaused();
            if (chapterRunner != null)
            {
                chapterRunner.RunState.MarkRewardPending();
                chapterRunner.SetFlowState(ChapterFlowState.RewardPending);
            }

            if (logDebug)
            {
                Debug.LogWarning($"{nameof(ChapterRoomRewardFlowController)} ability offer was cleared before selection; next room remains gated.", this);
            }
        }

        private void HandlePlayerDied(HitContext hitContext)
        {
            FailChapter("player_dead");
        }

        private void ResolveRewardWithoutAbility()
        {
            chapterRunner.RunState.MarkWaitingForContinue();
            chapterRunner.SetFlowState(ChapterFlowState.WaitingForContinue);

            if (autoContinueAfterAbilitySelection)
            {
                UnlockRoomExit(pendingDecision.RewardType);
                ProceedToNextRoom();
                return;
            }

            UnlockRoomExit(pendingDecision.RewardType);
        }

        private void ProceedToNextRoom()
        {
            if (chapterRunner == null || chapterRunner.IsChapterCompleted || chapterRunner.IsChapterFailed)
            {
                return;
            }

            var fromRoom = chapterRunner.CurrentRoomConfig;
            var fromRoomIndex = chapterRunner.CurrentRoomIndex;
            var toRoomIndex = fromRoomIndex + 1;
            var progressionArgs = new ChapterRoomProgressionEventArgs(chapterRunner, fromRoom, fromRoomIndex, pendingDecision.RewardType);
            var transitionArgs = new ChapterRoomTransitionEventArgs(chapterRunner, fromRoom, fromRoomIndex, toRoomIndex);

            ChapterProgressionEvents.RaiseNextRoomRequested(progressionArgs);
            chapterRunner.RunState.MarkTransitioning();
            chapterRunner.SetFlowState(ChapterFlowState.TransitioningToNextRoom);
            CleanupBeforeNextRoom();
            ChapterProgressionEvents.RaiseRoomTransitionStarted(transitionArgs);

            var didStartNextRoom = chapterRunner.StartNextRoom();
            if (didStartNextRoom)
            {
                ChapterProgressionEvents.RaiseRoomTransitionCompleted(transitionArgs);
                return;
            }

            if (chapterRunner.IsChapterCompleted || chapterRunner.IsChapterFailed)
            {
                return;
            }

            chapterRunner.RunState.MarkWaitingForContinue();
            chapterRunner.SetFlowState(ChapterFlowState.WaitingForContinue);
            UnlockRoomExit(pendingDecision.RewardType);

            Debug.LogWarning(
                $"{nameof(ChapterRoomRewardFlowController)} requested next room from index {fromRoomIndex}, but {nameof(ChapterRunner)} did not start room {toRoomIndex}. " +
                $"State was restored to {nameof(ChapterFlowState.WaitingForContinue)}. Check ChapterRunner config and RoomManager references.",
                this);
        }

        private bool TryRepairContinueState()
        {
            if (chapterRunner == null
                || !chapterRunner.RunState.IsRewardPending
                || chapterRunner.RunState.IsAbilitySelectionPending
                || isWaitingForAbilitySelection
                || chapterRunner.RunState.IsWaitingForContinue
                || chapterRunner.RunState.IsTransitioning
                || chapterRunner.FlowState == ChapterFlowState.TransitioningToNextRoom)
            {
                return false;
            }

            if (abilitySelectionController != null && abilitySelectionController.HasCurrentOffer)
            {
                return false;
            }

            chapterRunner.RunState.MarkWaitingForContinue();
            chapterRunner.SetFlowState(ChapterFlowState.WaitingForContinue);
            UnlockRoomExit(pendingDecision.RewardType);

            if (logDebug)
            {
                Debug.LogWarning($"{nameof(ChapterRoomRewardFlowController)} repaired stale reward state to {nameof(ChapterFlowState.WaitingForContinue)}.", this);
            }

            return true;
        }

        private void CleanupBeforeNextRoom()
        {
            isWaitingForAbilitySelection = false;
            ResumeGameIfPaused();

            if (abilitySelectionController != null && abilitySelectionController.HasCurrentOffer)
            {
                abilitySelectionController.ClearCurrentOffer();
            }

            if (subscribedRoomManager != null)
            {
                subscribedRoomManager.ResetRoomState();
            }

            chapterRunner.RunState.ClearRewardState();
        }

        private void UnlockRoomExit(RoomRewardType rewardType)
        {
            if (chapterRunner == null)
            {
                return;
            }

            var args = new ChapterRoomProgressionEventArgs(
                chapterRunner,
                chapterRunner.CurrentRoomConfig,
                chapterRunner.CurrentRoomIndex,
                rewardType);
            var room = subscribedRoomManager != null ? subscribedRoomManager : chapterRunner.ActiveRoomManager;
            room?.UnlockRoomExits(rewardType);
            ChapterProgressionEvents.RaiseRoomExitUnlocked(args);
        }

        private void FailChapter(string reason)
        {
            isWaitingForAbilitySelection = false;
            ResumeGameIfPaused();

            if (abilitySelectionController != null && abilitySelectionController.HasCurrentOffer)
            {
                abilitySelectionController.ClearCurrentOffer();
            }

            if (chapterRunner != null)
            {
                chapterRunner.FailChapter(reason, playerHealth != null ? playerHealth.gameObject : null);
            }
        }

        private void PauseGameIfNeeded()
        {
            if (!pauseDuringAbilitySelection || isPausedByController)
            {
                return;
            }

            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            isPausedByController = true;
        }

        private void ResumeGameIfPaused()
        {
            if (!isPausedByController)
            {
                return;
            }

            Time.timeScale = previousTimeScale;
            isPausedByController = false;
        }

        private bool CanContinueAfterRewardNow()
        {
            if (chapterRunner == null || chapterRunner.IsChapterCompleted || chapterRunner.IsChapterFailed)
            {
                return false;
            }

            if (isWaitingForAbilitySelection || chapterRunner.RunState.IsAbilitySelectionPending)
            {
                return false;
            }

            if (chapterRunner.RunState.IsTransitioning || chapterRunner.FlowState == ChapterFlowState.TransitioningToNextRoom)
            {
                return false;
            }

            if (TryRepairContinueState())
            {
                return true;
            }

            return chapterRunner.RunState.IsWaitingForContinue && chapterRunner.FlowState == ChapterFlowState.WaitingForContinue;
        }

        private string BuildContinueBlockReason()
        {
            if (chapterRunner == null)
            {
                return "ChapterRunner reference is missing.";
            }

            if (chapterRunner.IsChapterCompleted)
            {
                return "chapter is already completed.";
            }

            if (chapterRunner.IsChapterFailed)
            {
                return "chapter is failed.";
            }

            if (isWaitingForAbilitySelection || chapterRunner.RunState.IsAbilitySelectionPending)
            {
                return "ability selection is still pending.";
            }

            if (chapterRunner.RunState.IsTransitioning || chapterRunner.FlowState == ChapterFlowState.TransitioningToNextRoom)
            {
                return "room transition is already in progress.";
            }

            return $"RunState waiting={chapterRunner.RunState.IsWaitingForContinue}, reward={chapterRunner.RunState.IsRewardPending}, flow={chapterRunner.FlowState}.";
        }
    }
}
