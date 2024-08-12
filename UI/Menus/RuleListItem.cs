using Godot;

public sealed partial class RuleListItem : MarginContainer
{
	[Export]
	public Label RuleNameLabel { get; set; }
	[Export]
	public Button GoToRuleButton { get; set; }
	[Export]
	public PackedScene RuleDescriptionUiPackedScene { get; set; }

	public void SetData(DiactricalMarkRuleItemResource detailedRule)
	{
		RuleNameLabel.Text = detailedRule.Name;
		GoToRuleButton.Pressed += () => BuildRuleDescriptionScene(detailedRule);
	}

	private void BuildRuleDescriptionScene(DiactricalMarkRuleItemResource detailedRule)
	{
		RuleDescriptionUi detailedRuleUi = RuleDescriptionUiPackedScene.Instantiate<RuleDescriptionUi>();
		detailedRuleUi.SetData(detailedRule);
		GetTree().Root.AddChild(detailedRuleUi);
	}
}
