using Godot;
using WordProcessing.Models.SpellingRules;

namespace AlfaEBetto.Data.Rules
{
	public sealed partial class SpellingRuleRuleItemResource : BaseRuleItemResource
	{
		[Export]
		public SpellingRuleRuleSetType RuleSetType { get; set; }
		[Export]
		public SpellingRuleRuleType RuleType { get; set; }
	}
}
