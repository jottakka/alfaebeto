using Godot;
using WordProcessing.Models.SpellingRules;

[GlobalClass]
public partial class SpellingRuleRuleItemResource : BaseRuleItemResource
{
	[Export]
	public SpellingRuleRuleSetType RuleSetType { get; set; }
	[Export]
	public SpellingRuleRuleType RuleType { get; set; }
}
