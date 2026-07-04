using System;
using TapKnockout.Ability;
using TapKnockout.Survivor;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Editor.Tools
{
    public static class AbilityCatalogBuilder
    {
        public const string VerticalSliceFolder = "Assets/_Project/ScriptableObjects/Abilities/VerticalSlice";
        private static readonly string[] RunConfigPaths =
        {
            "Assets/_Project/ScriptableObjects/Runs/RunConfig_DesktopSurvivorPrototype.asset",
            "Assets/_Project/ScriptableObjects/Runs/RunConfig_ForestSurvivorArena.asset"
        };

        [MenuItem("Tools/Tap Knockout/Abilities/Create Vertical Slice Ability Catalog")]
        public static void CreateVerticalSliceAbilityCatalog()
        {
            var createdOrUpdated = CreateOrUpdateVerticalSliceCatalog();
            var wiredRunConfigs = WireRunConfigsToVerticalSliceCatalog();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Created/updated {createdOrUpdated} Tap Knockout vertical slice ability assets in {VerticalSliceFolder}. Wired {wiredRunConfigs} run config ability pool(s).");
        }

        public static int CreateOrUpdateVerticalSliceCatalog()
        {
            EnsureFolder(VerticalSliceFolder);

            var entries = VerticalSliceAbilityCatalog.Entries;
            for (var i = 0; i < entries.Length; i++)
            {
                CreateOrUpdateAbility(entries[i]);
            }

            return entries.Length;
        }

        public static int WireRunConfigsToVerticalSliceCatalog()
        {
            var abilities = LoadVerticalSliceAbilities();
            if (abilities.Count == 0)
            {
                return 0;
            }

            var wiredCount = 0;
            for (var i = 0; i < RunConfigPaths.Length; i++)
            {
                var runConfig = AssetDatabase.LoadAssetAtPath<RunConfig>(RunConfigPaths[i]);
                if (runConfig == null)
                {
                    continue;
                }

                var serializedObject = new SerializedObject(runConfig);
                var pool = serializedObject.FindProperty("startingAbilityPool");
                if (pool == null)
                {
                    continue;
                }

                pool.arraySize = abilities.Count;
                for (var abilityIndex = 0; abilityIndex < abilities.Count; abilityIndex++)
                {
                    pool.GetArrayElementAtIndex(abilityIndex).objectReferenceValue = abilities[abilityIndex];
                }

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(runConfig);
                wiredCount++;
            }

            return wiredCount;
        }

        public static string BuildAssetPath(AbilityCatalogEntry entry)
        {
            return $"{VerticalSliceFolder}/{entry.AssetName}.asset";
        }

        private static void CreateOrUpdateAbility(AbilityCatalogEntry entry)
        {
            var assetPath = BuildAssetPath(entry);
            var ability = AssetDatabase.LoadAssetAtPath<AbilityDefinition>(assetPath);
            if (ability == null)
            {
                ability = ScriptableObject.CreateInstance<AbilityDefinition>();
                AssetDatabase.CreateAsset(ability, assetPath);
            }

            ability.name = entry.AssetName;
            var serializedObject = new SerializedObject(ability);
            SetString(serializedObject, "abilityId", entry.AbilityId);
            SetString(serializedObject, "displayName", entry.DisplayName);
            SetString(serializedObject, "description", entry.Description);
            SetEnum(serializedObject, "rarity", (int)entry.Rarity);
            SetEnum(serializedObject, "category", (int)entry.Category);
            SetEnum(serializedObject, "effectType", (int)entry.EffectType);
            SetStringArray(serializedObject.FindProperty("tags"), BuildLegacyTagIds(entry.AbilityTags));
            SetEnumArray(serializedObject.FindProperty("abilityTags"), entry.AbilityTags);
            SetInt(serializedObject, "maxStacks", entry.MaxStacks);
            SetFloat(serializedObject, "weight", entry.Weight);
            SetBool(serializedObject, "allowDuplicateInOffer", false);
            SetBool(serializedObject, "isEnabled", true);
            SetEnumArray(serializedObject.FindProperty("requiredTags"), entry.RequiredTags);
            SetEnumArray(serializedObject.FindProperty("blockedTags"), entry.BlockedTags);
            SetStringArray(serializedObject.FindProperty("prerequisiteAbilityIds"), entry.PrerequisiteAbilityIds);
            SetString(serializedObject, "upgradeGroupId", entry.UpgradeGroupId);
            SetString(serializedObject, "mutuallyExclusiveGroupId", entry.MutuallyExclusiveGroupId);
            SetBool(serializedObject, "isPlaceholder", entry.IsPlaceholder);
            SetEnum(serializedObject, "implementationStatus", (int)entry.ImplementationStatus);
            SetFloat(serializedObject, "value", entry.Value);
            SetFloat(serializedObject, "secondaryValue", entry.SecondaryValue);
            SetFloat(serializedObject, "duration", entry.Duration);
            SetFloat(serializedObject, "cooldown", entry.Cooldown);
            SetFloat(serializedObject, "procChance", entry.ProcChance);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(ability);
        }

        public static System.Collections.Generic.List<AbilityDefinition> LoadVerticalSliceAbilities()
        {
            var entries = VerticalSliceAbilityCatalog.Entries;
            var abilities = new System.Collections.Generic.List<AbilityDefinition>(entries.Length);
            for (var i = 0; i < entries.Length; i++)
            {
                var ability = AssetDatabase.LoadAssetAtPath<AbilityDefinition>(BuildAssetPath(entries[i]));
                if (ability != null)
                {
                    abilities.Add(ability);
                }
            }

            return abilities;
        }

        public static AbilityDefinition LoadVerticalSliceAbility(string abilityId)
        {
            if (string.IsNullOrWhiteSpace(abilityId))
            {
                return null;
            }

            var entries = VerticalSliceAbilityCatalog.Entries;
            for (var i = 0; i < entries.Length; i++)
            {
                if (string.Equals(entries[i].AbilityId, abilityId, StringComparison.Ordinal))
                {
                    return AssetDatabase.LoadAssetAtPath<AbilityDefinition>(BuildAssetPath(entries[i]));
                }
            }

            return null;
        }

        private static void EnsureFolder(string folderPath)
        {
            var parts = folderPath.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static string[] BuildLegacyTagIds(AbilityTag[] tags)
        {
            var result = new string[tags.Length];
            for (var i = 0; i < tags.Length; i++)
            {
                result[i] = ToLegacyTagId(tags[i]);
            }

            return result;
        }

        private static string ToLegacyTagId(AbilityTag tag)
        {
            return tag switch
            {
                AbilityTag.Attack => "attack",
                AbilityTag.Dash => "dash",
                AbilityTag.Projectile => "projectile",
                AbilityTag.ElementFire => "element_fire",
                AbilityTag.ElementPoison => "element_poison",
                AbilityTag.ElementLightning => "element_lightning",
                AbilityTag.ElementIce => "element_ice",
                AbilityTag.Orbital => "orbital",
                AbilityTag.Drone => "drone",
                AbilityTag.Strike => "strike",
                AbilityTag.Meteor => "meteor",
                AbilityTag.Defensive => "defense",
                AbilityTag.Healing => "healing",
                AbilityTag.Boss => "boss",
                AbilityTag.Pickup => "pickup",
                AbilityTag.Economy => "economy",
                AbilityTag.RiskReward => "risk_reward",
                AbilityTag.SuperUpgrade => "super_upgrade",
                AbilityTag.Utility => "utility",
                AbilityTag.Active => "active",
                AbilityTag.Area => "area",
                _ => tag.ToString().ToLowerInvariant()
            };
        }

        private static void SetString(SerializedObject serializedObject, string propertyName, string value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.stringValue = value ?? string.Empty;
            }
        }

        private static void SetInt(SerializedObject serializedObject, string propertyName, int value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.intValue = value;
            }
        }

        private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.boolValue = value;
            }
        }

        private static void SetEnum(SerializedObject serializedObject, string propertyName, int value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.intValue = value;
            }
        }

        private static void SetStringArray(SerializedProperty property, string[] values)
        {
            if (property == null)
            {
                return;
            }

            values ??= Array.Empty<string>();
            property.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).stringValue = values[i] ?? string.Empty;
            }
        }

        private static void SetEnumArray(SerializedProperty property, AbilityTag[] values)
        {
            if (property == null)
            {
                return;
            }

            values ??= Array.Empty<AbilityTag>();
            property.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).intValue = (int)values[i];
            }
        }
    }
}
