using Godot;

public sealed partial class MoneyCounterUi : Control
{
	[Export]
	public Label Label { get; set; }
	[Export]
	public long Money { get; set; } = 0;

	public override void _Ready()
	{
		this.SetVisibilityZOrdering(VisibilityZOrdering.UI);
		SetMoney(Money);
		GetParent().Ready += OnParentReady;
	}

	private void OnParentReady()
	{
		Global.Instance.Player.OnMoneyChangedSignal += SetMoney;
	}

	public void SetMoney(long money)
	{
		Label.Text = $"$  {money:0000000000.00}";
	}
}
