using System.Text.Json.Serialization;

namespace WordProcessing.Models.SpellingRules;
public record SpellingRuleRoot(
	[property: JsonPropertyName("RuleCategories")] IReadOnlyList<SpellingRuleRuleCategory> RuleCategories
);