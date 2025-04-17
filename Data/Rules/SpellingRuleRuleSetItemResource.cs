using System.Linq;
using Godot;
using Godot.Collections;
using WordProcessing.Models.SpellingRules;

namespace AlfaEBetto.Data.Rules;

public sealed partial class SpellingRuleRuleSetItemResource : BaseRuleSetItemResource
{
	[Export]
	public SpellingRuleRuleSetType RuleSetType { get; set; }
	[Export]
	public Array<SpellingRuleRuleItemResource> Rules { get; set; }

	public override BaseRuleItemResource[] RulesAsBaseItemResource =>
		Rules.Select(r => r as BaseRuleItemResource).ToArray();
}
