using Godot;

public sealed partial class CollectableCoin : CollectableItemBase
{
	[Export]
	public long Value { get; set; } = 1000;

	private Player _player => Global.Instance.Player;

	public override void _Ready()
	{
		base._Ready();
		AnimationPlayer.Play(CollectableAnimations.CoinSpinning);
	}
}
