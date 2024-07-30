using Godot;
public sealed partial class HitBox : Area2D
{
	[Export]
	public bool SetOnReady { get; set; } = true;

	public Node Parent => GetParent();

	public override void _Ready()
	{
		this.ResetCollisionLayerAndMask();

		if (SetOnReady)
		{
			ActivateCollisionsMasks();
		}
	}

	public void DeactivateCollisionMasks()
	{
		this.ResetCollisionLayerAndMask();
	}

	public void ActivateCollisionsMasks()
	{
		switch (Parent)
		{
			case Player _:
				SetHitBoxForPlayer();
				break;
			case EnemyBase _:
				SetHitBoxForRegularEnemy();
				break;
			case MeteorEnemyBase _:
				SetHitBoxForMeteorEnemy();
				break;
			case LetterBlock _ or EnemyWord _ or AnswerMeteor _:
				SetHitBoxForWordsEnemy();
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
		this.ActivateCollisionLayer(CollisionLayers.PlayerHitBox);

		this.ActivateCollisionMask(CollisionLayers.WordEnemyHurtBox);
		this.ActivateCollisionMask(CollisionLayers.MeteorEnemyHurtBox);
		this.ActivateCollisionLayer(CollisionLayers.RegularEnemyHurtBox);
	}

	private void SetHitBoxForRegularEnemy()
	{
		this.ActivateCollisionLayer(CollisionLayers.RegularEnemyHitBox);

		// Collision Masks to observe
		this.ActivateCollisionMask(CollisionLayers.PlayerRegularHurtBox);
		this.ActivateCollisionMask(CollisionLayers.PlayerSpecialHurtBox);
	}

	private void SetHitBoxForWordsEnemy()
	{
		this.ActivateCollisionLayer(CollisionLayers.WordEnemyHitBox);

		// Collision Masks to observe
		this.ActivateCollisionMask(CollisionLayers.PlayerSpecialHurtBox);
	}

	private void SetHitBoxForMeteorEnemy()
	{
		this.ActivateCollisionLayer(CollisionLayers.MeteorEnemyHitBox);

		// Collision Masks to observe
		this.ActivateCollisionMask(CollisionLayers.PlayerSpecialHurtBox);
		this.ActivateCollisionMask(CollisionLayers.PlayerRegularHurtBox);
	}

	private void SetHitBoxForLaser()
	{
		this.ActivateCollisionMask(CollisionLayers.MeteorEnemyHitBox);
		this.ActivateCollisionMask(CollisionLayers.WordEnemyHitBox);
	}
}
