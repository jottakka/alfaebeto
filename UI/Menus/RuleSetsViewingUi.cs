using Godot;

public sealed partial class RuleSetsViewingUi : Control
{
	[Export]
	public PackedScene RuleSetListItemPackedScene { get; set; }
	[Export]
	public VBoxContainer RuleListVBoxContainer { get; set; }
	[Export]
	public Button ExitButton { get; set; }
	private RulesResource _rulesResource => Global.Instance.RulesResource;

	public override void _Ready()
	{
		BuildItens();
		ExitButton.Pressed += QueueFree;
	}

	private void BuildItens()
	{
		foreach (DiactricalMarkRuleSetItemResource ruleSet in _rulesResource.DiactricalMarkRuleSets)
		{
			AddItensToVBox(ruleSet);
		}
	}

	private void AddItensToVBox(DiactricalMarkRuleSetItemResource ruleSet)
	{
		RuleSetListItem ruleListItem = RuleSetListItemPackedScene.Instantiate<RuleSetListItem>();
		ruleListItem.SetData(ruleSet);
		RuleListVBoxContainer.AddChildDeffered(ruleListItem);
	}
}
