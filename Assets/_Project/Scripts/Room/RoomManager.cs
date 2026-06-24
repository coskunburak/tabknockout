using System;
using System.Collections;
using TapKnockout.Wave;
using UnityEngine;

namespace TapKnockout.Room
{
    [DisallowMultipleComponent]
    public sealed class RoomManager : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private RoomTemplateConfig config;

        [Header("References")]
        [SerializeField] private WaveManager waveManager;

        [Header("Runtime")]
        [SerializeField] private bool startConfiguredRoomOnStart;

        [Header("Debug")]
        [SerializeField] private bool logLifecycle;

        private Coroutine startRoutine;

        public event Action<RoomStartedEventArgs> OnRoomStarted;
        public event Action<RoomCompletedEventArgs> OnRoomCompleted;

        public bool IsRoomRunning { get; private set; }
        public bool IsRoomComplete { get; private set; }
        public int CurrentWaveIndex { get; private set; } = -1;
        public RoomTemplateConfig CurrentRoom => config;

        private void Reset()
        {
            waveManager = GetComponent<WaveManager>();
        }

        private void Awake()
        {
            if (waveManager == null)
            {
                waveManager = GetComponent<WaveManager>();
            }
        }

        private void OnEnable()
        {
            if (waveManager != null)
            {
                waveManager.OnWaveCompleted += HandleWaveCompleted;
            }
        }

        private void Start()
        {
            if (startConfiguredRoomOnStart && config != null)
            {
                StartRoom(config);
            }
        }

        private void OnDisable()
        {
            if (waveManager != null)
            {
                waveManager.OnWaveCompleted -= HandleWaveCompleted;
            }

            if (startRoutine != null)
            {
                StopCoroutine(startRoutine);
                startRoutine = null;
            }
        }

        public void StartRoom()
        {
            StartRoom(config);
        }

        public void StartRoom(RoomTemplateConfig roomConfig)
        {
            ResetRoomState();

            config = roomConfig;
            CurrentWaveIndex = -1;
            IsRoomRunning = true;
            IsRoomComplete = false;

            var startedArgs = new RoomStartedEventArgs(this, config);
            OnRoomStarted?.Invoke(startedArgs);
            RoomEvents.RaiseRoomStarted(startedArgs);

            if (logLifecycle)
            {
                Debug.Log($"{nameof(RoomManager)} started room {config?.RoomId ?? "<null>"}.", this);
            }

            startRoutine = StartCoroutine(StartRoomRoutine());
        }

        public void StartNextWave()
        {
            if (!IsRoomRunning)
            {
                return;
            }

            if (config == null)
            {
                CompleteRoom();
                return;
            }

            CurrentWaveIndex++;
            if (config.Waves == null || CurrentWaveIndex >= config.Waves.Count)
            {
                CompleteRoom();
                return;
            }

            if (waveManager == null)
            {
                Debug.LogWarning($"{nameof(RoomManager)} on {name} cannot start wave because no WaveManager is assigned.", this);
                return;
            }

            waveManager.RunWave(config.Waves[CurrentWaveIndex], CurrentWaveIndex);
        }

        public void ResetRoomState()
        {
            if (startRoutine != null)
            {
                StopCoroutine(startRoutine);
                startRoutine = null;
            }

            if (waveManager != null)
            {
                waveManager.ResetWaveState();
            }

            CurrentWaveIndex = -1;
            IsRoomRunning = false;
            IsRoomComplete = false;
        }

        [ContextMenu("Force Complete Room")]
        public void ForceCompleteRoom()
        {
            CompleteRoom();
        }

        private IEnumerator StartRoomRoutine()
        {
            if (config != null && config.StartDelay > 0f)
            {
                yield return new WaitForSeconds(config.StartDelay);
            }

            startRoutine = null;
            StartNextWave();
        }

        private void HandleWaveCompleted(WaveCompletedEventArgs eventArgs)
        {
            if (!IsRoomRunning)
            {
                return;
            }

            StartNextWave();
        }

        private void CompleteRoom()
        {
            if (IsRoomComplete)
            {
                return;
            }

            IsRoomRunning = false;
            IsRoomComplete = true;

            var completedArgs = new RoomCompletedEventArgs(this, config, Mathf.Max(0, CurrentWaveIndex));
            OnRoomCompleted?.Invoke(completedArgs);
            RoomEvents.RaiseRoomCompleted(completedArgs);

            if (logLifecycle)
            {
                Debug.Log($"{nameof(RoomManager)} completed room {config?.RoomId ?? "<null>"}.", this);
            }
        }
    }
}
