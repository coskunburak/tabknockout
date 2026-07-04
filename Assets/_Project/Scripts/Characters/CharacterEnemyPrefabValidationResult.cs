using System.Collections.Generic;

namespace TapKnockout.Characters
{
    public sealed class CharacterEnemyPrefabValidationResult
    {
        private readonly List<CharacterEnemyPrefabValidationIssue> issues = new List<CharacterEnemyPrefabValidationIssue>();

        public IReadOnlyList<CharacterEnemyPrefabValidationIssue> Issues => issues;
        public int IssueCount => issues.Count;
        public bool IsValid => issues.Count == 0;

        public bool HasIssue(string code)
        {
            for (var i = 0; i < issues.Count; i++)
            {
                if (issues[i].Code == code)
                {
                    return true;
                }
            }

            return false;
        }

        public void Add(string code, string message)
        {
            issues.Add(new CharacterEnemyPrefabValidationIssue(code, message));
        }
    }
}
