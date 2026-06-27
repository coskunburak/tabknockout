using TapKnockout.Level;
using UnityEngine;
using UnityEngine.UI;

namespace TapKnockout.UI
{
    [DisallowMultipleComponent]
    public sealed class RunStatusHudController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ChapterRunner chapterRunner;
        [SerializeField] private Text roomLabel;
        [SerializeField] private Text statusLabel;

        [Header("Labels")]
        [SerializeField] private string idleText = "Chapter Ready";
        [SerializeField] private string runningText = "Room";
        [SerializeField] private string completedText = "Chapter Completed";
        [SerializeField] private string failedText = "Chapter Failed";

        public string CurrentRoomText { get; private set; } = string.Empty;
        public string CurrentStatusText { get; private set; } = string.Empty;

        private void OnEnable()
        {
            ChapterEvents.OnChapterStarted -= HandleChapterStarted;
            ChapterEvents.OnChapterStarted += HandleChapterStarted;
            ChapterEvents.OnChapterRoomChanged -= HandleChapterRoomChanged;
            ChapterEvents.OnChapterRoomChanged += HandleChapterRoomChanged;
            ChapterProgressionEvents.OnChapterCompleted -= HandleChapterCompleted;
            ChapterProgressionEvents.OnChapterCompleted += HandleChapterCompleted;
            ChapterProgressionEvents.OnChapterFailed -= HandleChapterFailed;
            ChapterProgressionEvents.OnChapterFailed += HandleChapterFailed;

            Refresh();
        }

        private void OnDisable()
        {
            ChapterEvents.OnChapterStarted -= HandleChapterStarted;
            ChapterEvents.OnChapterRoomChanged -= HandleChapterRoomChanged;
            ChapterProgressionEvents.OnChapterCompleted -= HandleChapterCompleted;
            ChapterProgressionEvents.OnChapterFailed -= HandleChapterFailed;
        }

        public void SetChapterRunner(ChapterRunner runner)
        {
            chapterRunner = runner;
            Refresh();
        }

        public void Refresh()
        {
            if (chapterRunner == null || !chapterRunner.IsChapterRunning && !chapterRunner.IsChapterCompleted && !chapterRunner.IsChapterFailed)
            {
                SetText(string.Empty, idleText);
                return;
            }

            if (chapterRunner.IsChapterCompleted)
            {
                SetText(BuildRoomText(chapterRunner.CurrentRoomIndex, chapterRunner.TotalRoomCount), completedText);
                return;
            }

            if (chapterRunner.IsChapterFailed)
            {
                SetText(BuildRoomText(chapterRunner.CurrentRoomIndex, chapterRunner.TotalRoomCount), failedText);
                return;
            }

            SetText(BuildRoomText(chapterRunner.CurrentRoomIndex, chapterRunner.TotalRoomCount), runningText);
        }

        private void HandleChapterStarted(ChapterStartedEventArgs eventArgs)
        {
            if (chapterRunner == null)
            {
                chapterRunner = eventArgs.Source;
            }

            Refresh();
        }

        private void HandleChapterRoomChanged(ChapterRoomChangedEventArgs eventArgs)
        {
            if (chapterRunner != null && eventArgs.Source != chapterRunner)
            {
                return;
            }

            SetText(BuildRoomText(eventArgs.RoomIndex, eventArgs.Source != null ? eventArgs.Source.TotalRoomCount : 0), runningText);
        }

        private void HandleChapterCompleted(ChapterCompletedEventArgs eventArgs)
        {
            if (chapterRunner != null && eventArgs.Source != chapterRunner)
            {
                return;
            }

            SetText(BuildRoomText(eventArgs.CompletedRoomCount - 1, eventArgs.Source != null ? eventArgs.Source.TotalRoomCount : eventArgs.CompletedRoomCount), completedText);
        }

        private void HandleChapterFailed(ChapterFailedEventArgs eventArgs)
        {
            if (chapterRunner != null && eventArgs.Source != chapterRunner)
            {
                return;
            }

            SetText(BuildRoomText(eventArgs.CompletedRoomCount, eventArgs.Source != null ? eventArgs.Source.TotalRoomCount : eventArgs.CompletedRoomCount), failedText);
        }

        private void SetText(string roomText, string statusText)
        {
            CurrentRoomText = roomText ?? string.Empty;
            CurrentStatusText = statusText ?? string.Empty;

            if (roomLabel != null)
            {
                roomLabel.text = CurrentRoomText;
            }

            if (statusLabel != null)
            {
                statusLabel.text = CurrentStatusText;
            }
        }

        private static string BuildRoomText(int zeroBasedRoomIndex, int totalRooms)
        {
            if (totalRooms <= 0)
            {
                return string.Empty;
            }

            var roomNumber = Mathf.Clamp(zeroBasedRoomIndex + 1, 1, totalRooms);
            return $"Room {roomNumber} / {totalRooms}";
        }
    }
}
