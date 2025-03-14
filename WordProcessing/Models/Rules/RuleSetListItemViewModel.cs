namespace WordProcessing.Models.Rules;
public sealed record RuleSetListItemViewModel(
    string RuleSetName,
    int TotalCount,
    int UnlockedCount,
    IReadOnlyList<RuleListItemViewModel> Rules,
    string? RichTextDescription = null
    );
