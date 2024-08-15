using Godot;
using WordProcessing.Models.DiacriticalMarks;

[GlobalClass]
public partial class DiactricalMarkRuleItemResource : BaseRuleItemResource
{
	[Export]
	public DiactricalMarkRuleSetType RuleSetType { get; set; }
	[Export]
	public DiactricalMarkRuleType RuleType { get; set; }
}
