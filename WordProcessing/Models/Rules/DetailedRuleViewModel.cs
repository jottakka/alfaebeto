namespace WordProcessing.Models.Rules;
public sealed record DetailedRuleViewModel(
    string RuleName,
    string RuleDescriptionRichText,
    string[] Examples,
    bool IsUnlocked,
    CategoryType RuleType,
    string[]? Exceptions = null
 );
