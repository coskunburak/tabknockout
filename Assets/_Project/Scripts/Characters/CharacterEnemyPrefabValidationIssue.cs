namespace TapKnockout.Characters
{
    public readonly struct CharacterEnemyPrefabValidationIssue
    {
        public CharacterEnemyPrefabValidationIssue(string code, string message)
        {
            Code = code ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public string Code { get; }
        public string Message { get; }
    }
}
