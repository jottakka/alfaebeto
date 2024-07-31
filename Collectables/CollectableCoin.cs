public sealed partial class CollectableCoin : CollectableItemBase
{
    public override void _Ready()
    {
        base._Ready();
        this.AnimationPlayer.Play(CollectableAnimations.CoinSpinning);
    }
}
