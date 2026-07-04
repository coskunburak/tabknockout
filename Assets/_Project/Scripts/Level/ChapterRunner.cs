using System;
using TapKnockout.Room;
using UnityEngine;

namespace TapKnockout.Level
{
    [DisallowMultipleComponent]
    public sealed class ChapterRunner : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private ChapterConfig config;

        [Header("Room Runtime")]
        [SerializeField] private RoomManager roomManager;
        [SerializeField] private RoomManager[] roomManagers = Array.Empty<RoomManager>();
        [SerializeField] private RuntimeRoomLoader runtimeRoomLoader;

        [Header("Runtime")]
        [SerializeField] private bool startChapterOnStart;
        [SerializeField] private bool autoStartFirstRoom = true;
        [SerializeField] private bool autoAdvanceRooms;
        [SerializeField] private bool resetPausedTimeScaleOnChapterStart = true;

        [Header("Debug")]
        [SerializeField] private bool logLifecycle;

        private readonly ChapterRunState runState = new ChapterRunState();
        private RoomManager activeRoomManager;
        private bool externalProgressionControl;

        public event Action<ChapterStartedEventArgs> OnChapterStarted;
        public event Action<ChapterRoomChangedEventArgs> OnChapterRoomChanged;
        public event Action<ChapterCompletedEventArgs> OnChapterCompleted;
        public event Action<ChapterFailedEventArgs> OnChapterFailed;

        public bool IsChapterRunning { get; private set; }
        public bool IsChapterComplete { get; private set; }
        public bool IsChapterCompleted => IsChapterComplete;
        public bool IsChapterFailed { get; private set; }
        public bool IsRunActive => IsChapterRunning;
        public int CurrentRoomIndex { get; private set; } = -1;
        public int TotalRoomCount => config != null && config.Rooms != null ? config.Rooms.Count : 0;
        public bool IsLastRoom => TotalRoomCount > 0 && CurrentRoomIndex >= TotalRoomCount - 1;
        public bool IsExternalProgressionControlEnabled => externalProgressionControl;
        public ChapterConfig CurrentChapter => config;
        public RoomTemplateConfig CurrentRoomConfig => GetRoomConfig(CurrentRoomIndex);
        public RoomManager ActiveRoomManager => activeRoomManager;
        public ChapterRunState RunState => runState;
        public ChapterFlowState FlowState { get; private set; } = ChapterFlowState.Idle;

        private void Reset()
        {
            roomManager = GetComponent<RoomManager>();
            runtimeRoomLoader = GetComponent<RuntimeRoomLoader>();
        }

        private void Awake()
        {
            if (roomManager == null)
            {
                roomManager = GetComponent<RoomManager>();
            }

            if (runtimeRoomLoader == null)
            {
                runtimeRoomLoader = GetComponent<RuntimeRoomLoader>();
            }
        }

        private void Start()
        {
            if (startChapterOnStart)
            {
                StartChapter();
            }
        }

        private void OnDisable()
        {
            UnsubscribeActiveRoom();
        }

        public void SetReferences(ChapterConfig chapterConfig, RoomManager defaultRoomManager)
        {
            config = chapterConfig;
            roomManager = defaultRoomManager;
        }

        public void SetReferences(ChapterConfig chapterConfig, RoomManager defaultRoomManager, RuntimeRoomLoader roomLoader)
        {
            config = chapterConfig;
            roomManager = defaultRoomManager;
            runtimeRoomLoader = roomLoader;
        }

        public void SetRoomManagers(RoomManager[] managers)
        {
            roomManagers = managers ?? Array.Empty<RoomManager>();
        }

        public void SetExternalProgressionControl(bool isEnabled)
        {
            externalProgressionControl = isEnabled;
        }

        public void SetFlowState(ChapterFlowState state)
        {
            FlowState = state;
        }

        public void StartChapter()
        {
            UnsubscribeActiveRoom();

            if (resetPausedTimeScaleOnChapterStart && Time.timeScale <= 0f)
            {
                Time.timeScale = 1f;
            }

            CurrentRoomIndex = -1;
            IsChapterRunning = true;
            IsChapterComplete = false;
            IsChapterFailed = false;
            runState.StartRun(TotalRoomCount);
            SetFlowState(ChapterFlowState.PreparingRoom);

            var startedArgs = new ChapterStartedEventArgs(this, config);
            OnChapterStarted?.Invoke(startedArgs);
            ChapterEvents.RaiseChapterStarted(startedArgs);

            if (logLifecycle)
            {
                Debug.Log($"{nameof(ChapterRunner)} started chapter {config?.ChapterId ?? "<null>"}.", this);
            }

            if (autoStartFirstRoom)
            {
                StartNextRoom();
            }
        }

        public bool StartNextRoom()
        {
            if (IsChapterComplete || IsChapterFailed)
            {
                return false;
            }

            if (!IsChapterRunning)
            {
                StartChapter();
                return IsChapterRunning && CurrentRoomIndex >= 0;
            }

            if (runState.IsRewardPending || runState.IsAbilitySelectionPending || runState.IsWaitingForContinue)
            {
                return false;
            }

            if (config == null || config.Rooms == null || CurrentRoomIndex + 1 >= config.Rooms.Count)
            {
                CompleteChapter();
                return false;
            }

            UnsubscribeActiveRoom();

            var nextRoomIndex = CurrentRoomIndex + 1;
            var roomConfig = config.Rooms[nextRoomIndex];
            activeRoomManager = ResolveRoomManager(nextRoomIndex);
            if (activeRoomManager == null)
            {
                Debug.LogWarning($"{nameof(ChapterRunner)} on {name} has no RoomManager for room index {nextRoomIndex}.", this);
                return false;
            }

            CurrentRoomIndex = nextRoomIndex;
            runState.MarkRoomStarted(CurrentRoomIndex, roomConfig != null && roomConfig.IsBossRoom);
            SetFlowState(ChapterFlowState.RoomStarting);

            var roomChangedArgs = new ChapterRoomChangedEventArgs(this, config, roomConfig, CurrentRoomIndex);
            OnChapterRoomChanged?.Invoke(roomChangedArgs);
            ChapterEvents.RaiseChapterRoomChanged(roomChangedArgs);

            if (!externalProgressionControl)
            {
                activeRoomManager.OnRoomCompleted -= HandleRoomCompleted;
                activeRoomManager.OnRoomCompleted += HandleRoomCompleted;
            }

            var activeRoomInstance = runtimeRoomLoader != null ? runtimeRoomLoader.LoadRoom(roomConfig) : null;
            activeRoomManager.SetActiveRoomInstance(activeRoomInstance);
            activeRoomManager.StartRoom(roomConfig);
            SetFlowState(ChapterFlowState.CombatRunning);
            return true;
        }

        public void CompleteChapter()
        {
            if (IsChapterComplete)
            {
                return;
            }

            IsChapterRunning = false;
            IsChapterComplete = true;
            IsChapterFailed = false;
            runState.MarkChapterCompleted();
            SetFlowState(ChapterFlowState.ChapterCompleted);
            UnsubscribeActiveRoom();

            var completedArgs = new ChapterCompletedEventArgs(this, config, Mathf.Max(0, CurrentRoomIndex + 1));
            OnChapterCompleted?.Invoke(completedArgs);
            ChapterEvents.RaiseChapterCompleted(completedArgs);
            ChapterProgressionEvents.RaiseChapterCompleted(completedArgs);

            if (logLifecycle)
            {
                Debug.Log($"{nameof(ChapterRunner)} completed chapter {config?.ChapterId ?? "<null>"}.", this);
            }
        }

        public void FailChapter(string reason = "unknown", GameObject playerObject = null)
        {
            if (IsChapterFailed || IsChapterComplete)
            {
                return;
            }

            IsChapterRunning = false;
            IsChapterFailed = true;
            IsChapterComplete = false;
            runState.MarkChapterFailed();
            SetFlowState(ChapterFlowState.Failed);
            UnsubscribeActiveRoom();

            var completedRoomCount = runState.CurrentRoomCompleted ? CurrentRoomIndex + 1 : CurrentRoomIndex;
            var failedArgs = new ChapterFailedEventArgs(this, config, Mathf.Max(0, completedRoomCount), reason, playerObject);
            OnChapterFailed?.Invoke(failedArgs);
            ChapterEvents.RaiseChapterFailed(failedArgs);
            ChapterProgressionEvents.RaiseChapterFailed(failedArgs);

            if (logLifecycle)
            {
                Debug.Log($"{nameof(ChapterRunner)} failed chapter {config?.ChapterId ?? "<null>"}: {failedArgs.Reason}.", this);
            }
        }

        private RoomManager ResolveRoomManager(int roomIndex)
        {
            if (roomManagers != null && roomIndex >= 0 && roomIndex < roomManagers.Length && roomManagers[roomIndex] != null)
            {
                return roomManagers[roomIndex];
            }

            return roomManager;
        }

        private RoomTemplateConfig GetRoomConfig(int roomIndex)
        {
            if (config == null || config.Rooms == null || roomIndex < 0 || roomIndex >= config.Rooms.Count)
            {
                return null;
            }

            return config.Rooms[roomIndex];
        }

        private void HandleRoomCompleted(RoomCompletedEventArgs eventArgs)
        {
            UnsubscribeActiveRoom();
            runState.MarkRoomCompleted();

            if (externalProgressionControl)
            {
                return;
            }

            SetFlowState(ChapterFlowState.RoomCompleted);

            var decision = RoomCompletionDecision.Evaluate(eventArgs.RoomConfig, CurrentRoomIndex, TotalRoomCount);
            if (decision.ShouldCompleteChapter)
            {
                CompleteChapter();
                return;
            }

            if (!autoAdvanceRooms || !decision.ShouldAutoAdvance)
            {
                if (decision.ShouldOpenAbilitySelection || decision.ShouldWaitForContinue)
                {
                    runState.MarkRewardPending();
                    SetFlowState(ChapterFlowState.RewardPending);
                }

                return;
            }

            StartNextRoom();
        }

        private void UnsubscribeActiveRoom()
        {
            if (activeRoomManager != null)
            {
                activeRoomManager.OnRoomCompleted -= HandleRoomCompleted;
                activeRoomManager = null;
            }
        }
    }
}
