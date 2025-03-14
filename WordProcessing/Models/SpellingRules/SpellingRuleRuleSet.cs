using System.Text.Json.Serialization;

namespace WordProcessing.Models.SpellingRules;

public record SpellingRuleRuleSet(
	[property: JsonPropertyName("RuleSet")] string RuleSetTypeString,
	[property: JsonPropertyName("Name")] string Name,
	[property: JsonPropertyName("Rules")] IReadOnlyList<SpellingRuleRule> Rules
)
{
	[JsonIgnore]
	public SpellingRuleRuleSetType RuleSetType =>
	RuleSetTypeString switch
	{
		"SRulesSorZ" => SpellingRuleRuleSetType.SRulesSorZ,
		"ZRulesSorZ" => SpellingRuleRuleSetType.ZRulesSorZ,
		"XRulesXorCH" => SpellingRuleRuleSetType.XRulesXorCH,
		"CHRulesXorCH" => SpellingRuleRuleSetType.CHRulesXorCH,
		_ => throw new ArgumentException("Invalid RuleSetType")
	};
}
