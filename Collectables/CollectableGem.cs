using Godot;

public sealed partial class CollectableGem : CollectableItemBase
{
	[Export]
	public GemsType GemsType { get; set; } = GemsType.Red;

	public override void _Ready()
	{
		Sprite.Frame = (int)GemsType;
		base._Ready();
	}
}
