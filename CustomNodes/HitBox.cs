using Godot;
public sealed partial class HitBox : Area2D
{
	[Export]
	public bool SetOnReady { get; set; } = true;

	public Node Parent => GetParent();

	public override void _Ready()
	{
		this.ResetCollisionLanyerAndMask();

		if (SetOnReady)
		{
			ActivateCollisionsMasks();
		}
	}

	public void DeactivateCollisionMasks()
	{
		this.ResetCollisionLanyerAndMask();
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
		this.ActivateCollisionLayer(CollisionLayers.Player);
		this.ActivateCollisionLayer(CollisionLayers.PlayerHitBox);

		this.ActivateCollisionMask(CollisionLayers.WordEnemyHurtBox);
		this.ActivateCollisionMask(CollisionLayers.WordEnemy);
		this.ActivateCollisionMask(CollisionLayers.MeteorEnemyHitBox);
	}

	private void SetHitBoxForRegularEnemy()
	{
		// Collision Masks to observe
		this.ActivateCollisionMask(CollisionLayers.PlayerRegularHurtBox);
		this.ActivateCollisionMask(CollisionLayers.PlayerSpecialHurtBox);
		this.ActivateCollisionMask(CollisionLayers.Player);
	}

	private void SetHitBoxForWordsEnemy()
	{
		this.ActivateCollisionLayer(CollisionLayers.WordEnemy);
		this.ActivateCollisionLayer(CollisionLayers.WordEnemyHitBox);
		this.ActivateCollisionLayer(CollisionLayers.WordEnemyHurtBox);


		// Collision Masks to observe
		this.ActivateCollisionMask(CollisionLayers.PlayerSpecialHurtBox);
		this.ActivateCollisionMask(CollisionLayers.Player);
	}

	private void SetHitBoxForMeteorEnemy()
	{
		this.ActivateCollisionLayer(CollisionLayers.MeteorEnemy);
		this.ActivateCollisionLayer(CollisionLayers.MeteorEnemyHitBox);

		// Collision Masks to observe
		this.ActivateCollisionMask(CollisionLayers.PlayerSpecialHurtBox);
		this.ActivateCollisionMask(CollisionLayers.PlayerRegularHurtBox);
		this.ActivateCollisionMask(CollisionLayers.Player);

	}

	private void SetHitBoxForLaser()
	{
		this.ActivateCollisionMask(CollisionLayers.MeteorEnemyHitBox);
		this.ActivateCollisionMask(CollisionLayers.WordEnemyHitBox);
	}
}
