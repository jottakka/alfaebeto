using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public sealed partial class PlayerSpecialHurtBox : Area2D
{
	public override void _Ready()
	{
		this.ResetCollisionLanyerAndMask();
		this.ActivateCollisionLayer(CollisionLayers.RegularEnemyHurtBox);
		this.ActivateCollisionLayer(CollisionLayers.PlayerSpecialHurtBox);
	}
}
