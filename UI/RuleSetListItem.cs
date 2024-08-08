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
	public PackedScene RuleDescriptionUiPackedScene { get; set; }

	public void SetData(RuleSetListItemViewModel ruleSetListItemViewModel)
	{
		RuleNameLabel.Text = ruleSetListItemViewModel.RuleSetName;
		TotalRulesCountLabel.Text = ruleSetListItemViewModel.TotalCount.ToString();
		UnlockedRulesCountLabel.Text = ruleSetListItemViewModel.UnlockedCount.ToString();
	}
}
