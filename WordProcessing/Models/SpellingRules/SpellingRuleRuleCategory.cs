using System.Text.Json.Serialization;
using WordProcessing.Models.Rules;

namespace WordProcessing.Models.SpellingRules;

public record SpellingRuleRuleCategory(
	[property: JsonPropertyName("RuleCategory")] string RuleCategoryTypeString,
	[property: JsonPropertyName("Name")] string Name,
	[property: JsonPropertyName("Description")] string Description,
	[property: JsonPropertyName("RuleSets")] IReadOnlyList<SpellingRuleRuleSet> RuleSets
)
{
	[JsonIgnore]
	public CategoryType RuleCategoryType =>
		RuleCategoryTypeString switch
		{
			"SorZ" => CategoryType.SorZ,
			"XorCH" => CategoryType.XorCH,
			_ => throw new ArgumentException("Invalid RuleSetType")
		};
}
