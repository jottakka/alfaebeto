namespace WordProcessing.Models.Rules;
public sealed record RuleListItemViewModel(
	string Name,
	DetailedRuleViewModel DetailedModel,
	string RichTextDescription,
	bool IsUnlocked = true
	);
