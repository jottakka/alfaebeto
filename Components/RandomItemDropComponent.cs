using Godot;

public sealed partial class RandomItemDropComponent : Node
{
	[Export]
	public PackedScene[] CollectableItemScenes { get; set; }

	private Node _scene => Global.Instance.Scene;

	public void DropRandomItem(Vector2 position)
	{
		int itemIdx = GD.RandRange(0, CollectableItemScenes.Length);
		PackedScene itemPackedScene = CollectableItemScenes[itemIdx];
		CollectableItemBase item = itemPackedScene.Instantiate<CollectableItemBase>();
		item.GlobalPosition = position;
		_scene.AddChildDeffered(item);
	}
}
