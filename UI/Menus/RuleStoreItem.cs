using Godot;

public sealed partial class RuleStoreItem : MarginContainer
{
	[Export]
	public Label CostLabel { get; set; }
	[Export]
	public Label RuleSetLabel { get; set; }
	[Export]
	public Label RuleLabel { get; set; }
	[Export]
	public ColorRect BoughtColorRect { get; set; }
	[Export]
	public Button BuyButton { get; set; }

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		BuyButton.Pressed += () => BoughtColorRect.Show();
	}

	public void SetData(DiactricalMarkRuleItemResource diactricalMarkRuleItem)
	{
		RuleSetLabel.Text = diactricalMarkRuleItem.RuleSet;
		RuleLabel.Text = diactricalMarkRuleItem.Name;
		BoughtColorRect.Visible = diactricalMarkRuleItem.IsUnlocked;
		CostLabel.Text = diactricalMarkRuleItem.KeyGemCost.ToString();
		BuyButton.Disabled = diactricalMarkRuleItem.IsUnlocked;
	}
}
