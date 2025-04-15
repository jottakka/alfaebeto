using Godot;
using Godot.Collections;
using WordProcessing.Models.SpellingRules;

public sealed partial class SpellingRulesResource : Resource
{
	[Export]
	public Dictionary<SpellingRuleRuleType, Array<SpellingRuleWordResource>> WordsByRule { get; set; } = [];
}
