using Alfaebeto.Collectables;
using Godot;

namespace AlfaEBetto.Collectables;

public sealed partial class CollectableGem : CollectableItemBase
{
	[Export]
	public GemType GemType { get; set; } = GemType.Red;

	public override void _Ready()
	{
		Sprite.Frame = (int)GemType;
		base._Ready();
	}
}
