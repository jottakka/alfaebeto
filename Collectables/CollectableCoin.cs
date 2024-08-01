public sealed partial class CollectableCoin : CollectableItemBase
{
	public override void _Ready()
	{
		base._Ready();
		AnimationPlayer.Play(CollectableAnimations.CoinSpinning);
	}
}
