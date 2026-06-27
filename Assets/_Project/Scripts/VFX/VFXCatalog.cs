using System.Collections.Generic;
using UnityEngine;

namespace TapKnockout.VFX
{
    [CreateAssetMenu(fileName = "VFXCatalog", menuName = "Tap Knockout/VFX/VFX Catalog")]
    public sealed class VFXCatalog : ScriptableObject
    {
        [SerializeField] private List<VFXDefinition> definitions = new List<VFXDefinition>();
        [SerializeField] private bool logDuplicateDefinitions;

        private readonly Dictionary<VFXEventType, VFXDefinition> lookup = new Dictionary<VFXEventType, VFXDefinition>();

        public IReadOnlyList<VFXDefinition> Definitions => definitions;

        private void OnEnable()
        {
            RebuildLookup();
        }

        private void OnValidate()
        {
            for (var i = 0; i < definitions.Count; i++)
            {
                definitions[i]?.ClampValues();
            }

            RebuildLookup();
        }

        public bool TryGetDefinition(VFXEventType eventType, out VFXDefinition definition)
        {
            if (lookup.Count != definitions.Count)
            {
                RebuildLookup();
            }

            return lookup.TryGetValue(eventType, out definition) && definition != null;
        }

        public void SetDefinitions(IEnumerable<VFXDefinition> newDefinitions)
        {
            definitions.Clear();

            if (newDefinitions != null)
            {
                foreach (var definition in newDefinitions)
                {
                    if (definition == null)
                    {
                        continue;
                    }

                    definition.ClampValues();
                    definitions.Add(definition);
                }
            }

            RebuildLookup();
        }

        public void RebuildLookup()
        {
            lookup.Clear();

            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                if (definition == null)
                {
                    continue;
                }

                definition.ClampValues();

                if (lookup.ContainsKey(definition.EventType))
                {
                    if (logDuplicateDefinitions)
                    {
                        Debug.LogWarning($"{nameof(VFXCatalog)} {name} has a duplicate definition for {definition.EventType}. The first definition will be used.", this);
                    }

                    continue;
                }

                lookup.Add(definition.EventType, definition);
            }
        }
    }
}
