using Godot;
public sealed partial class AmmoComponent : Node
{
	public PackedScene PackedScene { get; set; }

	public AmmoBase Create(
		float rotation,
		Vector2 position
	)
	{
		var ammo = PackedScene.Instantiate<AmmoBase>();
		ammo.ShootRadAngle = rotation;
		ammo.InitialPosition = position;
		return ammo;
	}
}

