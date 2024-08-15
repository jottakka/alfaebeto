using Godot;

public partial class BaseRuleSetItemResource : Resource
{
	[Export]
	public string RuleSet { get; set; }
	[Export]
	public string Description { get; set; }
}
