using Alfaebeto.Collectables;
using AlfaEBetto.Collectables;
using AlfaEBetto.Extensions;
using AlfaEBetto.PlayerNodes;
using Godot;

namespace AlfaEBetto.Components;

public sealed partial class RandomItemDropComponent : Node
{
	[Export]
	public PackedScene[] CollectableItemScenes { get; set; }

	private Node _scene => Global.Instance.Scene;

	private Player Player => Global.Instance.Player;

	public void DropRandomItem(Vector2 position)
	{
		int itemIdx = GD.RandRange(0, CollectableItemScenes.Length - 1);
		PackedScene itemPackedScene = CollectableItemScenes[itemIdx];
		CollectableItemBase item = itemPackedScene.Instantiate<CollectableItemBase>();
		item.GlobalPosition = position;
		_scene.AddChildDeffered(item);
	}
}
