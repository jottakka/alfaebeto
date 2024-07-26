using Godot;

public sealed partial class PlayerSpecialHurtBox : Area2D
{
	public override void _Ready()
	{
		this.ResetCollisionLanyerAndMask();
		this.ActivateCollisionLayer(CollisionLayers.RegularEnemyHurtBox);
		this.ActivateCollisionLayer(CollisionLayers.PlayerSpecialHurtBox);
	}
}
