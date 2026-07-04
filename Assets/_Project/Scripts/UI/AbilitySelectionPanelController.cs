using System.Collections.Generic;
using TapKnockout.Ability;
using TapKnockout.Level;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
        private bool panelOwnsPause;

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

        private void Update()
        {
            if (!isOpen)
            {
                return;
            }

            if (TryReadChoiceHotkey(out var choiceIndex))
            {
                SelectChoice(choiceIndex);
            }
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

            if (!isOpen && pauseGameWhileOpen && Time.timeScale > 0f)
            {
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                panelOwnsPause = true;
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
            if (isOpen && pauseGameWhileOpen && panelOwnsPause)
            {
                Time.timeScale = previousTimeScale;
            }

            panelOwnsPause = false;
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

        private static bool TryReadChoiceHotkey(out int choiceIndex)
        {
            choiceIndex = -1;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
                {
                    choiceIndex = 0;
                    return true;
                }

                if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
                {
                    choiceIndex = 1;
                    return true;
                }

                if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
                {
                    choiceIndex = 2;
                    return true;
                }
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad1))
            {
                choiceIndex = 0;
                return true;
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad2))
            {
                choiceIndex = 1;
                return true;
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad3))
            {
                choiceIndex = 2;
                return true;
            }
#endif

            return false;
        }
    }
}
