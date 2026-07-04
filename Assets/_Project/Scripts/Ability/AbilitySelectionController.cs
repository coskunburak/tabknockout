using System.Collections.Generic;
using UnityEngine;

namespace TapKnockout.Ability
{
    [DisallowMultipleComponent]
    public sealed class AbilitySelectionController : MonoBehaviour
    {
        [Header("Ability Pool")]
        [SerializeField] private List<AbilityDefinition> abilityPool = new List<AbilityDefinition>();
        [SerializeField, Min(1)] private int choiceCount = 3;

        [Header("Runtime")]
        [SerializeField] private bool generateOfferOnStart;
        [SerializeField] private bool clearOfferAfterSelection = true;
        [SerializeField] private bool allowPlaceholderAbilitiesInOffers;

        [Header("Future Hooks")]
        [Tooltip("Optional MonoBehaviour implementing IAbilityEffectApplier. Full gameplay effects are intentionally out of scope for this foundation.")]
        [SerializeField] private MonoBehaviour abilityEffectApplier;

        [Header("Debug")]
        [SerializeField] private bool logDebugEvents;

        private readonly RunAbilityState runAbilityState = new RunAbilityState();
        private readonly List<AbilityDefinition> currentOffer = new List<AbilityDefinition>();
        private AbilityChoiceProvider choiceProvider;

        public event System.Action<AbilityOfferEventArgs> OnAbilityOfferGenerated;
        public event System.Action<AbilitySelectedEventArgs> OnAbilitySelected;
        public event System.Action<AbilityOfferEventArgs> OnAbilityOfferCleared;

        public IReadOnlyList<AbilityDefinition> AbilityPool => abilityPool;
        public IReadOnlyList<AbilityDefinition> CurrentOffer => currentOffer;
        public RunAbilityState RunState => runAbilityState;
        public int ChoiceCount => choiceCount;
        public bool HasCurrentOffer => currentOffer.Count > 0;
        public bool AllowPlaceholderAbilitiesInOffers => allowPlaceholderAbilitiesInOffers;
        public IAbilityEffectApplier AbilityEffectApplier => ResolveEffectApplier(false);
        public bool HasOfferPresentationListeners => OnAbilityOfferGenerated != null || AbilityEvents.HasOfferGeneratedListeners;

        private void Awake()
        {
            EnsureChoiceProvider();
        }

        private void Start()
        {
            if (generateOfferOnStart)
            {
                GenerateOffer();
            }
        }

        private void OnValidate()
        {
            choiceCount = Mathf.Max(1, choiceCount);
            abilityPool ??= new List<AbilityDefinition>();
        }

        public void SetRandomSeed(int seed)
        {
            choiceProvider = new AbilityChoiceProvider(seed);
            choiceProvider.AllowPlaceholderAbilitiesInOffers = allowPlaceholderAbilitiesInOffers;
        }

        public void SetAbilityPool(IReadOnlyList<AbilityDefinition> definitions)
        {
            abilityPool.Clear();
            if (definitions == null)
            {
                return;
            }

            for (var i = 0; i < definitions.Count; i++)
            {
                if (definitions[i] != null)
                {
                    abilityPool.Add(definitions[i]);
                }
            }
        }

        public void SetAbilityEffectApplier(MonoBehaviour applier)
        {
            abilityEffectApplier = applier;
        }

        public IReadOnlyList<AbilityDefinition> GenerateOffer()
        {
            EnsureChoiceProvider();
            choiceProvider.AllowPlaceholderAbilitiesInOffers = allowPlaceholderAbilitiesInOffers;

            currentOffer.Clear();
            var generatedChoices = choiceProvider.GenerateChoices(abilityPool, runAbilityState, choiceCount);
            for (var i = 0; i < generatedChoices.Count; i++)
            {
                currentOffer.Add(generatedChoices[i]);
            }

            var eventArgs = new AbilityOfferEventArgs(this, currentOffer);
            OnAbilityOfferGenerated?.Invoke(eventArgs);
            AbilityEvents.RaiseAbilityOfferGenerated(eventArgs);

            if (logDebugEvents)
            {
                Debug.Log($"{nameof(AbilitySelectionController)} generated {currentOffer.Count} ability choices.", this);
            }

            return CurrentOffer;
        }

        public bool SelectOffer(int index)
        {
            if (index < 0 || index >= currentOffer.Count)
            {
                return false;
            }

            return SelectAbility(currentOffer[index], index);
        }

        public bool SelectAbility(AbilityDefinition definition)
        {
            return SelectAbility(definition, -1);
        }

        public void ClearCurrentOffer()
        {
            if (currentOffer.Count == 0)
            {
                return;
            }

            var clearedArgs = new AbilityOfferEventArgs(this, currentOffer);
            currentOffer.Clear();
            OnAbilityOfferCleared?.Invoke(clearedArgs);
            AbilityEvents.RaiseAbilityOfferCleared(clearedArgs);
        }

        public void ClearRunState()
        {
            runAbilityState.Clear();
            ClearCurrentOffer();
        }

        [ContextMenu("Generate Ability Offer")]
        private void GenerateOfferFromContextMenu()
        {
            GenerateOffer();
        }

        [ContextMenu("Select First Ability Offer")]
        private void SelectFirstOfferFromContextMenu()
        {
            SelectOffer(0);
        }

        private bool SelectAbility(AbilityDefinition definition, int selectedIndex)
        {
            if (!runAbilityState.AddSelectedAbility(definition, allowPlaceholderAbilitiesInOffers))
            {
                return false;
            }

            var stackCount = runAbilityState.GetStackCount(definition);
            var eventArgs = new AbilitySelectedEventArgs(this, definition, selectedIndex, stackCount);
            OnAbilitySelected?.Invoke(eventArgs);
            AbilityEvents.RaiseAbilitySelected(eventArgs);

            ResolveEffectApplier(true)?.ApplyAbility(new AbilityEffectContext(this, definition, runAbilityState, stackCount));

            if (logDebugEvents)
            {
                Debug.Log($"{nameof(AbilitySelectionController)} selected {definition.AbilityId} at stack {stackCount}.", this);
            }

            if (clearOfferAfterSelection)
            {
                ClearCurrentOffer();
            }

            return true;
        }

        private void EnsureChoiceProvider()
        {
            choiceProvider ??= new AbilityChoiceProvider();
            choiceProvider.AllowPlaceholderAbilitiesInOffers = allowPlaceholderAbilitiesInOffers;
        }

        private IAbilityEffectApplier ResolveEffectApplier(bool logWarning)
        {
            if (abilityEffectApplier == null)
            {
                abilityEffectApplier = FindFirstAbilityEffectApplier();
            }

            if (abilityEffectApplier == null)
            {
                return null;
            }

            var applier = abilityEffectApplier as IAbilityEffectApplier;
            if (applier == null && logWarning)
            {
                Debug.LogWarning($"{abilityEffectApplier.name} is assigned as an ability effect applier but does not implement {nameof(IAbilityEffectApplier)}.", this);
            }

            return applier;
        }

        private static MonoBehaviour FindFirstAbilityEffectApplier()
        {
            var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IAbilityEffectApplier)
                {
                    return behaviours[i];
                }
            }

            return null;
        }
    }
}
