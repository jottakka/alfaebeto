using Godot;

public sealed partial class PlayerCollectableArea : Area2D
{
	[Export]
	public CollisionShape2D CollisionShape2D { get; set; }

	public override void _Ready()
	{
		this.ResetCollisionLayerAndMask();
		this.ActivateCollisionLayer(CollisionLayers.PlayerCollectionArea);
	}
}

