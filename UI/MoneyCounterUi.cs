using Godot;

public sealed partial class MoneyCounterUi : Control
{
	[Export]
	public Label Label { get; set; }
	[Export]
	public AnimationPlayer AnimationPlayer { get; set; }
	[Export]
	public long Money { get; set; } = 0;

	private Player _player => Global.Instance.Player;

	public override void _Ready()
	{
		this.SetVisibilityZOrdering(VisibilityZOrdering.UI);
		SetMoney(Money);

		Global.Instance.OnMainNodeSetupFinishedSignal += OnMainNodeReady;
	}

	private void OnMainNodeReady()
	{
		_player.OnMoneyChangedSignal += SetMoney;
	}

	public void SetMoney(long money)
	{
		Label.Text = $"$  {money:0000000000.00}";
		AnimationPlayer.Play(UiAnimations.OnAddMoney);
	}
}
