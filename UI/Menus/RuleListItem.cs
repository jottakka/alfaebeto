using Godot;
using WordProcessing.Models.Rules;

public sealed partial class RuleListItem : MarginContainer
{
	[Export]
	public Label RuleNameLabel { get; set; }
	[Export]
	public Button GoToRuleButton { get; set; }
	[Export]
	public PackedScene RuleDescriptionUiPackedScene { get; set; }

	public void SetData(RuleListItemViewModel ruleSetListItemViewModel)
	{
		RuleNameLabel.Text = ruleSetListItemViewModel.Name;
		GoToRuleButton.Pressed += () => BuildRuleDescriptionScene(ruleSetListItemViewModel.DetailedModel);
	}

	private void BuildRuleDescriptionScene(DetailedRuleViewModel detailedRuleViewModel)
	{
		RuleDescriptionUi detailedRule = RuleDescriptionUiPackedScene.Instantiate<RuleDescriptionUi>();
		detailedRule.SetData(detailedRuleViewModel);
		GetTree().Root.AddChild(detailedRule);
	}
}
