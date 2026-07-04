using TapKnockout.Survivor;
using UnityEngine;
using UnityEngine.UI;

namespace TapKnockout.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class RunResultController : MonoBehaviour
    {
        [SerializeField] private ArenaRunDirector runDirector;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text resultText;
        [SerializeField] private Text summaryText;
        [SerializeField] private bool hideOnAwake = true;

        private void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Awake()
        {
            EnsureCanvasGroup();
            if (hideOnAwake)
            {
                SetVisible(false);
            }
        }

        private void OnEnable()
        {
            if (runDirector != null)
            {
                runDirector.OnRunEnded += HandleRunEnded;
            }
        }

        private void OnDisable()
        {
            if (runDirector != null)
            {
                runDirector.OnRunEnded -= HandleRunEnded;
            }
        }

        public void SetRunDirector(ArenaRunDirector director)
        {
            if (runDirector == director)
            {
                return;
            }

            if (isActiveAndEnabled && runDirector != null)
            {
                runDirector.OnRunEnded -= HandleRunEnded;
            }

            runDirector = director;

            if (isActiveAndEnabled && runDirector != null)
            {
                runDirector.OnRunEnded += HandleRunEnded;
            }
        }

        private void HandleRunEnded(SurvivorRunSummary summary)
        {
            if (resultText != null)
            {
                resultText.text = summary.ResultState == SurvivorRunState.Victory ? "Victory" : "Defeat";
            }

            if (summaryText != null)
            {
                var minutes = Mathf.FloorToInt(summary.ElapsedSeconds / 60f);
                var seconds = Mathf.FloorToInt(summary.ElapsedSeconds % 60f);
                summaryText.text = $"Time {minutes:00}:{seconds:00}\nLevel {summary.PlayerLevel}\nKills {summary.EnemiesKilled}";
            }

            SetVisible(true);
        }

        private void SetVisible(bool visible)
        {
            EnsureCanvasGroup();
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        private void EnsureCanvasGroup()
        {
            if (canvasGroup == null && !TryGetComponent(out canvasGroup))
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }
}
