using Godot;
using Godot.Collections;
using WordProcessing.Models.SpellingRules;

namespace AlfaEBetto.Data.Words;

public sealed partial class SpellingRulesResource : Resource
{
	[Export]
	public Dictionary<SpellingRuleRuleType, Array<SpellingRuleWordResource>> WordsByRule { get; set; } = [];
}
