using AlfaEBetto.Extensions;
using Godot;

namespace AlfaEBetto.CustomNodes;

public sealed partial class PlayerSpecialHurtBox : Area2D
{
	public override void _Ready()
	{
		this.ResetCollisionLayerAndMask();
		this.ActivateCollisionLayer(CollisionLayers.PlayerSpecialHurtBox);
	}
}
