using Godot;
using WordProcessing.Models.Rules;

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

	public void SetData(RuleSetListItemViewModel ruleSetListItemViewModel)
	{
		RuleNameLabel.Text = ruleSetListItemViewModel.RuleSetName;
		TotalRulesCountLabel.Text = ruleSetListItemViewModel.TotalCount.ToString();
		UnlockedRulesCountLabel.Text = ruleSetListItemViewModel.UnlockedCount.ToString();
		GoToRuleButton.Pressed += () => BuildRuleListItemScene(ruleSetListItemViewModel);
	}

	private void BuildRuleListItemScene(RuleSetListItemViewModel ruleListItem)
	{
		RulesViewingUi rulesViewing = RuleViewingUiPackedScene.Instantiate<RulesViewingUi>();
		rulesViewing.SetData(ruleListItem);
		GetTree().Root.AddChild(rulesViewing);
	}
}
