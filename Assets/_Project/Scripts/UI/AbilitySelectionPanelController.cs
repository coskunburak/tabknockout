using System.Collections.Generic;
using TapKnockout.Ability;
using TapKnockout.Level;
using UnityEngine;

namespace TapKnockout.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class AbilitySelectionPanelController : MonoBehaviour, IAbilitySelectionPanelView
    {
        [Header("References")]
        [SerializeField] private AbilitySelectionController abilitySelectionController;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private List<AbilityChoiceCardView> cardViews = new List<AbilityChoiceCardView>();

        [Header("Runtime")]
        [SerializeField] private bool hideOnAwake = true;
        [SerializeField] private bool pauseGameWhileOpen = true;

        private float previousTimeScale = 1f;
        private bool isOpen;

        public bool PauseGameWhileOpen => pauseGameWhileOpen;
        public bool IsOpen => isOpen;

        private void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Awake()
        {
            EnsureCanvasGroup();

            if (hideOnAwake)
            {
                Hide();
            }
        }

        private void OnEnable()
        {
            if (abilitySelectionController != null)
            {
                abilitySelectionController.OnAbilityOfferGenerated += HandleOfferGenerated;
                abilitySelectionController.OnAbilitySelected += HandleAbilitySelected;
                abilitySelectionController.OnAbilityOfferCleared += HandleOfferCleared;
            }
        }

        private void OnDisable()
        {
            if (abilitySelectionController != null)
            {
                abilitySelectionController.OnAbilityOfferGenerated -= HandleOfferGenerated;
                abilitySelectionController.OnAbilitySelected -= HandleAbilitySelected;
                abilitySelectionController.OnAbilityOfferCleared -= HandleOfferCleared;
            }

            ResumeIfPaused();
        }

        public void SetAbilitySelectionController(AbilitySelectionController controller)
        {
            if (abilitySelectionController == controller)
            {
                return;
            }

            if (isActiveAndEnabled && abilitySelectionController != null)
            {
                abilitySelectionController.OnAbilityOfferGenerated -= HandleOfferGenerated;
                abilitySelectionController.OnAbilitySelected -= HandleAbilitySelected;
                abilitySelectionController.OnAbilityOfferCleared -= HandleOfferCleared;
            }

            abilitySelectionController = controller;

            if (isActiveAndEnabled && abilitySelectionController != null)
            {
                abilitySelectionController.OnAbilityOfferGenerated += HandleOfferGenerated;
                abilitySelectionController.OnAbilitySelected += HandleAbilitySelected;
                abilitySelectionController.OnAbilityOfferCleared += HandleOfferCleared;
            }
        }

        public void GenerateOffer()
        {
            if (abilitySelectionController != null)
            {
                abilitySelectionController.GenerateOffer();
            }
        }

        public void SetPauseGameWhileOpen(bool shouldPause)
        {
            if (pauseGameWhileOpen == shouldPause)
            {
                return;
            }

            if (isOpen && pauseGameWhileOpen)
            {
                ResumeIfPaused();
            }

            pauseGameWhileOpen = shouldPause;
        }

        [ContextMenu("Generate Ability Offer")]
        private void GenerateOfferFromContextMenu()
        {
            GenerateOffer();
        }

        private void HandleOfferGenerated(AbilityOfferEventArgs eventArgs)
        {
            Show(eventArgs.Choices);
        }

        private void HandleAbilitySelected(AbilitySelectedEventArgs eventArgs)
        {
            Hide();
        }

        private void HandleOfferCleared(AbilityOfferEventArgs eventArgs)
        {
            Hide();
        }

        private void Show(IReadOnlyList<AbilityDefinition> choices)
        {
            if (choices == null || choices.Count == 0)
            {
                Hide();
                return;
            }

            if (!isOpen && pauseGameWhileOpen)
            {
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }

            isOpen = true;
            SetVisible(true);

            for (var i = 0; i < cardViews.Count; i++)
            {
                if (i < choices.Count)
                {
                    var ability = choices[i];
                    var stackCount = abilitySelectionController != null ? abilitySelectionController.RunState.GetStackCount(ability) : 0;
                    cardViews[i].Bind(ability, i, stackCount, SelectChoice);
                }
                else
                {
                    cardViews[i].Clear();
                }
            }
        }

        private void Hide()
        {
            for (var i = 0; i < cardViews.Count; i++)
            {
                if (cardViews[i] != null)
                {
                    cardViews[i].Clear();
                }
            }

            SetVisible(false);
            ResumeIfPaused();
            isOpen = false;
        }

        private void SelectChoice(int index)
        {
            if (abilitySelectionController != null)
            {
                abilitySelectionController.SelectOffer(index);
            }
        }

        private void ResumeIfPaused()
        {
            if (isOpen && pauseGameWhileOpen)
            {
                Time.timeScale = previousTimeScale;
            }
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
