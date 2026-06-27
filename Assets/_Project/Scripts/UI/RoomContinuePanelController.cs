using TapKnockout.Level;
using UnityEngine;
using UnityEngine.UI;

namespace TapKnockout.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
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
        [SerializeField] private bool pollFlowControllerState = true;

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
            EnsureCanvasGroup();

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

        private void Update()
        {
            if (pollFlowControllerState)
            {
                RefreshVisibilityFromFlow();
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

        public void RefreshVisibilityFromFlow()
        {
            if (CanShowForContinue())
            {
                if (!IsVisible)
                {
                    Show();
                }

                return;
            }

            if (IsVisible)
            {
                Hide();
            }
        }

        private void HandleRoomExitUnlocked(ChapterRoomProgressionEventArgs eventArgs)
        {
            RefreshVisibilityFromFlow();
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

            if (flowController.TryContinueAfterReward())
            {
                Hide();
                return;
            }

            RefreshVisibilityFromFlow();
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

            EnsureCanvasGroup();

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
            EnsureCanvasGroup();

            if (visible && root != null && !root.activeSelf)
            {
                root.SetActive(true);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }

            if (continueButton != null)
            {
                continueButton.interactable = visible;
            }
        }

        private void EnsureCanvasGroup()
        {
            if (canvasGroup != null)
            {
                return;
            }

            var target = root != null ? root : gameObject;
            if (!target.TryGetComponent(out canvasGroup))
            {
                canvasGroup = target.AddComponent<CanvasGroup>();
            }
        }
    }
}
