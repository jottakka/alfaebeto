using Godot;

public sealed partial class RuleSetListItem : MarginContainer
{
	[Export]
	public Label RuleNameLabel { get; set; }
	[Export]
	public Label TotalRulesCountLabel { get; set; }
	[Export]
	public Label UnlockedRulesCountLabel { get; set; }
	[Export]
	public Button GoToRuleButton { get; set; }
	[Export]
	public PackedScene RuleViewingUiPackedScene { get; set; }

	public void SetData(DiactricalMarkRuleSetItemResource ruleSet)
	{
		ProcessMode = ProcessModeEnum.Always;
		RuleNameLabel.Text = ruleSet.Name;
		TotalRulesCountLabel.Text = ruleSet.TotalRulesCount.ToString();
		UnlockedRulesCountLabel.Text = ruleSet.UnlockedRulesCount.ToString();
		GoToRuleButton.Pressed += () => BuildRuleListItemScene(ruleSet);
	}

	private void BuildRuleListItemScene(DiactricalMarkRuleSetItemResource ruleSet)
	{
		RulesViewingUi rulesViewing = RuleViewingUiPackedScene.Instantiate<RulesViewingUi>();
		rulesViewing.SetData(ruleSet);
		GetTree().Root.AddChild(rulesViewing);
	}
}
