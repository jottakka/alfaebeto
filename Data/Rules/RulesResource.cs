using Godot;

namespace AlfaEBetto.Data.Rules;

[GlobalClass]
public sealed partial class RulesResource : BaseDataResource
{
	[Export]
	public DiactricalMarkRuleSetItemResource[] DiactricalMarkRuleSets { get; set; }

	[Export]
	public SpellingRuleRuleSetItemResource[] SpellingRuleRuleSets { get; set; }
}
