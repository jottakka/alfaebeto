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
	[Export]
	public Sprite2D NotAllowed { get; set; }

	private int _gemsCost;
	[Signal]
	public delegate void RuleBoughtSignalEventHandler(int gems);

	private int _availabeGems;
	private DiactricalMarkRuleItemResource _diactricalMarkRuleItemResource;
	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		BuyButton.Pressed += BuyRule;
	}

	private void BuyRule()
	{
		if (_availabeGems < _gemsCost)
		{
			return;
		}

		BoughtColorRect.Show();
		_diactricalMarkRuleItemResource.Unlock();
		_ = EmitSignal(nameof(RuleBoughtSignal), _gemsCost);
	}

	public void SetData(DiactricalMarkRuleItemResource diactricalMarkRuleItem, int totalGems)
	{
		RuleSetLabel.Text = diactricalMarkRuleItem.RuleSet;
		RuleLabel.Text = diactricalMarkRuleItem.Name;
		BoughtColorRect.Visible = diactricalMarkRuleItem.IsUnlocked;
		BuyButton.Disabled = diactricalMarkRuleItem.IsUnlocked;
		_gemsCost = diactricalMarkRuleItem.KeyGemCost;
		CostLabel.Text = _gemsCost.ToString();
		_availabeGems = totalGems;
		_diactricalMarkRuleItemResource = diactricalMarkRuleItem;
		VerifyIfEnoughtGems(totalGems);
	}

	public void OnMaxGemsAvailableAmmountChanged(int totalGems)
	{
		VerifyIfEnoughtGems(totalGems);
	}

	private void VerifyIfEnoughtGems(int totalGems)
	{
		if (totalGems < _gemsCost && BoughtColorRect.Visible is false)
		{
			NotAllowed.Show();
			BuyButton.Disabled = true;
			return;
		}
		else
		{
			NotAllowed.Hide();
		}

		_availabeGems = totalGems;
	}
}
