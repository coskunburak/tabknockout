using System.Collections.Generic;
using NUnit.Framework;

namespace TapKnockout.Ability.Tests
{
    public sealed class AbilityCatalogBuilderTests
    {
        [Test]
        public void VerticalSliceCatalog_HasExpectedUniqueAbilityIds()
        {
            var ids = new HashSet<string>();
            var entries = VerticalSliceAbilityCatalog.Entries;

            Assert.That(entries.Length, Is.EqualTo(VerticalSliceAbilityCatalog.ExpectedAbilityCount));
            for (var i = 0; i < entries.Length; i++)
            {
                Assert.That(entries[i].AbilityId, Is.Not.Empty);
                Assert.That(ids.Add(entries[i].AbilityId), Is.True);
            }
        }

        [Test]
        public void VerticalSliceCatalog_UsesOriginalDisplayNames()
        {
            var forbiddenNames = new HashSet<string>
            {
                "front arrow",
                "diagonal arrow",
                "side arrow",
                "rear arrow",
                "multishot"
            };

            var entries = VerticalSliceAbilityCatalog.Entries;
            for (var i = 0; i < entries.Length; i++)
            {
                Assert.That(forbiddenNames.Contains(entries[i].DisplayName.ToLowerInvariant()), Is.False);
            }
        }

        [Test]
        public void VerticalSliceCatalog_NormalOfferEntriesAreImplementedOrPartial()
        {
            var entries = VerticalSliceAbilityCatalog.Entries;
            for (var i = 0; i < entries.Length; i++)
            {
                Assert.That(entries[i].ImplementationStatus, Is.Not.EqualTo(AbilityImplementationStatus.Placeholder));
                Assert.That(entries[i].ImplementationStatus, Is.Not.EqualTo(AbilityImplementationStatus.Deferred));
            }
        }

        [Test]
        public void VerticalSliceCatalog_IncludesCooldownActiveSkills()
        {
            var arcBlast = FindEntry("skill_arc_blast");
            var groundSlam = FindEntry("skill_ground_slam");

            Assert.That(arcBlast.EffectType, Is.EqualTo(AbilityEffectType.EnergyBeam));
            Assert.That(arcBlast.Cooldown, Is.GreaterThan(0f));
            Assert.That(HasTag(arcBlast, AbilityTag.Active), Is.True);
            Assert.That(HasTag(arcBlast, AbilityTag.Area), Is.True);

            Assert.That(groundSlam.EffectType, Is.EqualTo(AbilityEffectType.EnergyRing));
            Assert.That(groundSlam.Cooldown, Is.GreaterThan(0f));
            Assert.That(HasTag(groundSlam, AbilityTag.Active), Is.True);
            Assert.That(HasTag(groundSlam, AbilityTag.Area), Is.True);
        }

        private static AbilityCatalogEntry FindEntry(string abilityId)
        {
            var entries = VerticalSliceAbilityCatalog.Entries;
            for (var i = 0; i < entries.Length; i++)
            {
                if (entries[i].AbilityId == abilityId)
                {
                    return entries[i];
                }
            }

            Assert.Fail($"Missing ability catalog entry '{abilityId}'.");
            return default;
        }

        private static bool HasTag(AbilityCatalogEntry entry, AbilityTag tag)
        {
            var tags = entry.AbilityTags;
            for (var i = 0; i < tags.Length; i++)
            {
                if (tags[i] == tag)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
