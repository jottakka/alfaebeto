using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

