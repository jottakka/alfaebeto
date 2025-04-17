using AlfaEBetto.Ammo;
using Godot;

namespace AlfaEBetto.Components;

public sealed partial class AmmoComponent : Node
{
	public PackedScene PackedScene { get; set; }

	public AmmoBase Create(
		float rotation,
		Vector2 position
	)
	{
		AmmoBase ammo = PackedScene.Instantiate<AmmoBase>();
		ammo.ShootRadAngle = rotation;
		ammo.InitialPosition = position;
		return ammo;
	}
}
