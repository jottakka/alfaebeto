using Godot;
using WordProcessing.Models.DiacriticalMarks;

public sealed partial class DiactricalMarkRuleItemResource : BaseRuleItemResource
{
	[Export]
	public DiactricalMarkRuleSetType RuleSetType { get; set; }
	[Export]
	public DiactricalMarkRuleType RuleType { get; set; }
}
