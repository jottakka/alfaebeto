using System.Linq;
using Godot;
using Godot.Collections;
using WordProcessing.Models.DiacriticalMarks;

[GlobalClass]
public partial class DiactricalMarkRuleSetItemResource : BaseRuleSetItemResource
{
	[Export]
	public DiactricalMarkRuleSetType RuleSetType { get; set; }
	[Export]
	public Array<DiactricalMarkRuleItemResource> Rules { get; set; }

	public int UnlockedRulesCount => Rules.Count(rule => rule.IsUnlocked);

	public int TotalRulesCount => Rules.Count();
}
