using Godot;
public sealed partial class GemSpawnerComponent : Node
{
	[Export]
	public PackedScene CollectableGemScene { get; set; }
	[Export]
	public float SpawnRadium { get; set; } = 30.0f;

	private Node _scene => Global.Instance.Scene;

	public void SpawnGem(Vector2 position, GemsType gemType, int quantity = 1)
	{
		Vector2 rotationVector = quantity > 1 ? GetRandomDirection2D() : default;
		float rotation = 2 * Mathf.Pi / quantity;
		for (int i = 0; i < quantity; i++)
		{
			CollectableGem gem = CollectableGemScene.Instantiate<CollectableGem>();
			gem.GemsType = gemType;
			gem.GlobalPosition = quantity > 1
				? position + (rotationVector.Rotated(rotation * i) * SpawnRadium)
				: position; ;

			_scene.AddChildDeffered(gem);
		}
	}

	private Vector2 GetRandomDirection2D()
	{
		float angle = (float)GD.RandRange(0, 2 * Mathf.Pi); // Random angle between 0 and 2*PI
		return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).Normalized();
	}
}
