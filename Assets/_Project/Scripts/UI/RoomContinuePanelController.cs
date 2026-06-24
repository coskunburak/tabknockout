using TapKnockout.Level;
using UnityEngine;
using UnityEngine.UI;

namespace TapKnockout.UI
{
    [DisallowMultipleComponent]
    public sealed class RoomContinuePanelController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject root;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button continueButton;
        [SerializeField] private Text continueLabel;
        [SerializeField] private ChapterRoomRewardFlowController flowController;

        [Header("Runtime")]
        [SerializeField] private bool hideOnStart = true;

        [Header("Debug")]
        [SerializeField] private bool logDebug;

        public bool IsVisible { get; private set; }
        public ChapterRoomRewardFlowController FlowController => flowController;

        private void Reset()
        {
            root = gameObject;
            canvasGroup = GetComponent<CanvasGroup>();
            continueButton = GetComponentInChildren<Button>(true);
            continueLabel = GetComponentInChildren<Text>(true);
        }

        private void Awake()
        {
            ResolveLocalReferences();

            if (continueLabel != null && string.IsNullOrWhiteSpace(continueLabel.text))
            {
                continueLabel.text = "Continue";
            }
        }

        private void OnEnable()
        {
            ChapterProgressionEvents.OnRoomExitUnlocked += HandleRoomExitUnlocked;
            ChapterProgressionEvents.OnRoomTransitionStarted += HandleRoomTransitionStarted;
            ChapterProgressionEvents.OnChapterCompleted += HandleChapterCompleted;
            ChapterProgressionEvents.OnChapterFailed += HandleChapterFailed;

            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(HandleContinueClicked);
                continueButton.onClick.AddListener(HandleContinueClicked);
            }
        }

        private void Start()
        {
            if (hideOnStart)
            {
                Hide();
            }
        }

        private void OnDisable()
        {
            ChapterProgressionEvents.OnRoomExitUnlocked -= HandleRoomExitUnlocked;
            ChapterProgressionEvents.OnRoomTransitionStarted -= HandleRoomTransitionStarted;
            ChapterProgressionEvents.OnChapterCompleted -= HandleChapterCompleted;
            ChapterProgressionEvents.OnChapterFailed -= HandleChapterFailed;

            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(HandleContinueClicked);
            }
        }

        public void SetFlowController(ChapterRoomRewardFlowController controller)
        {
            flowController = controller;
        }

        public void Show()
        {
            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
        }

        private void HandleRoomExitUnlocked(ChapterRoomProgressionEventArgs eventArgs)
        {
            if (CanShowForContinue())
            {
                Show();
                return;
            }

            Hide();
        }

        private void HandleRoomTransitionStarted(ChapterRoomTransitionEventArgs eventArgs)
        {
            Hide();
        }

        private void HandleChapterCompleted(ChapterCompletedEventArgs eventArgs)
        {
            Hide();
        }

        private void HandleChapterFailed(ChapterFailedEventArgs eventArgs)
        {
            Hide();
        }

        private void HandleContinueClicked()
        {
            if (flowController == null)
            {
                if (logDebug)
                {
                    Debug.LogWarning($"{nameof(RoomContinuePanelController)} on {name} has no {nameof(ChapterRoomRewardFlowController)} assigned.", this);
                }

                return;
            }

            if (!flowController.CanContinueAfterReward)
            {
                Hide();
                return;
            }

            Hide();
            flowController.ContinueAfterReward();
        }

        private bool CanShowForContinue()
        {
            return flowController != null && flowController.CanContinueAfterReward;
        }

        private void ResolveLocalReferences()
        {
            if (root == null)
            {
                root = gameObject;
            }

            if (canvasGroup == null && root != null)
            {
                canvasGroup = root.GetComponent<CanvasGroup>();
            }

            if (continueButton == null)
            {
                continueButton = GetComponentInChildren<Button>(true);
            }

            if (continueLabel == null)
            {
                continueLabel = GetComponentInChildren<Text>(true);
            }
        }

        private void SetVisible(bool visible)
        {
            IsVisible = visible;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
            else if (root != null && root != gameObject)
            {
                root.SetActive(visible);
            }

            if (continueButton != null)
            {
                continueButton.interactable = visible;
            }
        }
    }
}
