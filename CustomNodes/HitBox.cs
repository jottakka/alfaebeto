using AlfaEBetto.CustomNodes;
using Godot;
using System;
public sealed partial class HitBox : Area2D
{
	private Type ParentType;
	public override void _Ready()
	{
		this.ResetCollisionLanyerAndMask();
		var parent = GetParent();
		ParentType = parent.GetType();
		switch (parent)
		{
			case Player _:
				SetHitBoxForPlayer();
				break;
			case EnemyBase _:
				SetHitBoxForRegularEnemy();
				break;
			case EnemyWord _:
				SetHitBoxForWordEnemy();
				break;
			case LetterBlock _:
				SetHitBoxForWordEnemy();
				break;
			case Laser _:
				SetHitBoxForLaser();
				break;
			case AmmoBase _:
				break;
			default:
				GD.PrintErr("HitBox parent is not recognized");
				break;
		}
	}

	private void SetHitBoxForPlayer()
	{
		this.ActivateCollisionLayer(CollisionLayers.Player);
		this.ActivateCollisionLayer(CollisionLayers.PlayerHitBox);

		//this.ActivateCollisionMask(CollisionLayers.WordEnemyHurtBox);
		//this.ActivateCollisionMask(CollisionLayers.RegularEnemyHurtBox);
	}

	public void SetHitBoxForRegularEnemy()
	{
		// Collision Masks to observe
		this.ActivateCollisionMask(CollisionLayers.PlayerRegularHurtBox);
		this.ActivateCollisionMask(CollisionLayers.PlayerSpecialHurtBox);
		this.ActivateCollisionMask(CollisionLayers.PlayerHitBox);
	}

	public void SetHitBoxForWordEnemy()
	{
		this.ActivateCollisionLayer(CollisionLayers.WordEnemy);

		// Collision Masks to observe
		this.ActivateCollisionMask(CollisionLayers.PlayerSpecialHurtBox);
	}

	public void SetHitBoxForLaser()
	{
		this.ActivateCollisionMask(CollisionLayers.WordEnemy);
	}
}
