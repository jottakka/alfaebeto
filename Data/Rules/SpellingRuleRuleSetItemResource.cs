using System.Linq;
using Godot;
using Godot.Collections;
using WordProcessing.Models.SpellingRules;

[GlobalClass]
public partial class SpellingRuleRuleSetItemResource : BaseRuleSetItemResource
{
	[Export]
	public SpellingRuleRuleSetType RuleSetType { get; set; }
	[Export]
	public Array<SpellingRuleRuleItemResource> Rules { get; set; }

	public int UnlockedRulesCount => Rules.Count(rule => rule.IsUnlocked);

	public int TotalRulesCount => Rules.Count();
}
