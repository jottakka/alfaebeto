using Godot;
public sealed partial class HitBox : Area2D
{
	[Export]
	public bool SetOnReady { get; set; } = true;

	// Cache the parent node for efficiency and safer access
	private Node _cachedParent;

	public override void _Ready()
	{
		// It's generally safer to get the parent *after* the node is ready in the tree.
		_cachedParent = GetParent();
		if (_cachedParent == null)
		{
			// Log an error if the parent is unexpectedly null.
			// Depending on your design, this might be a critical error.
			GD.PrintErr($"{Name}: Parent node is null in _Ready(). HitBox may not function correctly.");
			// Consider SetProcess(false) or other error handling if a parent is always required.
		}

		// Assuming ResetCollisionLayerAndMask is an extension method or defined elsewhere
		this.ResetCollisionLayerAndMask();

		if (SetOnReady)
		{
			ActivateCollisionsMasks();
		}
	}

	/// <summary>
	/// Deactivates collisions by resetting layers and masks.
	/// </summary>
	public void DeactivateCollisions()
	{
		// Assuming ResetCollisionLayerAndMask is an extension method or defined elsewhere
		this.ResetCollisionLayerAndMask();
	}

	/// <summary>
	/// Activates the appropriate collision layer and mask based on the parent node's type.
	/// </summary>
	public void ActivateCollisionsMasks()
	{
		// Use the cached parent. If it wasn't found in _Ready, log an error and exit.
		if (_cachedParent == null)
		{
			GD.PrintErr($"{Name}: Cannot activate collisions, parent is null.");
			return;
		}

		// Determine collision setup based on the cached parent's type
		switch (_cachedParent)
		{
			case Player _:
				SetHitBoxForPlayer();
				break;
			case PlayerShield _:
				SetHitBoxForPlayerShield();
				break;
			case EnemyBase _: // Assuming EnemyBase is a common base class for regular enemies
				SetHitBoxForRegularEnemy();
				break;
			case MeteorEnemyBase _: // Assuming MeteorEnemyBase is a common base class for meteor enemies
				SetHitBoxForMeteorEnemy();
				break;
			// Combined case for various "word" type enemies using 'or' pattern
			case LetterBlock _ or EnemyWord _ or AnswerMeteor _:
				SetHitBoxForWordsEnemy();
				break;
			case Laser _:
				SetHitBoxForLaser();
				break;
			case AmmoBase _: // Assuming AmmoBase is a common base class for ammo
				SetHitBoxForAmmo(); // Call the setup method for ammo
				break;
			default:
				// Log an error if the parent type isn't recognized for specific setup.
				GD.PrintErr($"{Name}: HitBox parent type '{_cachedParent.GetType().Name}' is not recognized for collision setup.");
				// Consider setting default collision layers/masks here if appropriate.
				break;
		}
	}

	// --- Private Setup Methods ---
	// These methods configure the CollisionLayer and CollisionMask properties.
	// They rely on the existence of the CollisionLayers enum and extension methods
	// like ActivateCollisionLayer and ActivateCollisionMask.

	private void SetHitBoxForPlayerShield()
	{
		// This hitbox belongs to a PlayerShield
		this.ActivateCollisionLayer(CollisionLayers.PlayerShieldHitBox);
		// Also consider it a hurtbox for simplicity? Or maybe shield has separate hurtbox.
		this.ActivateCollisionLayer(CollisionLayers.PlayerShieldHurtBox);

		// What should the shield hitbox detect collisions with?
		this.ActivateCollisionMask(CollisionLayers.WordEnemyHurtBox);
		this.ActivateCollisionMask(CollisionLayers.MeteorEnemyHurtBox);
		this.ActivateCollisionMask(CollisionLayers.RegularEnemyHurtBox);
		this.ActivateCollisionMask(CollisionLayers.EnemyAmmo);
	}

	private void SetHitBoxForPlayer()
	{
		// This hitbox belongs to the Player (likely for dealing damage, e.g., ramming)
		this.ActivateCollisionLayer(CollisionLayers.PlayerHitBox);

		// What should the player hitbox detect collisions with? (Usually HurtBoxes)
		this.ActivateCollisionMask(CollisionLayers.WordEnemyHurtBox);
		this.ActivateCollisionMask(CollisionLayers.MeteorEnemyHurtBox);
		this.ActivateCollisionMask(CollisionLayers.RegularEnemyHurtBox);
		this.ActivateCollisionMask(CollisionLayers.EnemyAmmo); // Maybe player can destroy ammo?
	}

	private void SetHitBoxForRegularEnemy()
	{
		// This hitbox belongs to a Regular Enemy
		this.ActivateCollisionLayer(CollisionLayers.RegularEnemyHitBox);

		// What should a regular enemy hitbox detect? (Player/Shield HurtBoxes)
		this.ActivateCollisionMask(CollisionLayers.PlayerRegularHurtBox); // Assuming player has different hurtboxes
		this.ActivateCollisionMask(CollisionLayers.PlayerSpecialHurtBox);
		this.ActivateCollisionMask(CollisionLayers.PlayerShieldHurtBox);
		// Maybe other enemies?
		// this.ActivateCollisionMask(CollisionLayers.MeteorEnemyHurtBox);
	}

	private void SetHitBoxForWordsEnemy()
	{
		// This hitbox belongs to a Word-type Enemy (LetterBlock, EnemyWord, AnswerMeteor)
		this.ActivateCollisionLayer(CollisionLayers.WordEnemyHitBox);

		// What should a word enemy hitbox detect? (Specific player hurtbox?)
		this.ActivateCollisionMask(CollisionLayers.PlayerSpecialHurtBox); // Example: Only special shots hit these
	}

	private void SetHitBoxForMeteorEnemy()
	{
		// This hitbox belongs to a Meteor Enemy
		this.ActivateCollisionLayer(CollisionLayers.MeteorEnemyHitBox);

		// What should a meteor enemy hitbox detect? (Player/Shield HurtBoxes)
		this.ActivateCollisionMask(CollisionLayers.PlayerSpecialHurtBox);
		this.ActivateCollisionMask(CollisionLayers.PlayerRegularHurtBox);
		this.ActivateCollisionMask(CollisionLayers.PlayerShieldHurtBox);
	}

	private void SetHitBoxForLaser()
	{
		// This hitbox belongs to a Laser (presumably player's laser)
		// Lasers usually don't have a layer themselves (they ARE the hit)
		// They only need masks to detect what they hit.
		// Resetting layer might be good practice if ActivateCollisionLayer sets it.
		// this.ResetCollisionLayer(); // Assuming such a method exists

		// What should the laser detect? (Enemy HurtBoxes)
		this.ActivateCollisionMask(CollisionLayers.MeteorEnemyHurtBox);
		this.ActivateCollisionMask(CollisionLayers.WordEnemyHurtBox);
		this.ActivateCollisionMask(CollisionLayers.RegularEnemyHurtBox);
		// Maybe other things like asteroids?
	}

	private void SetHitBoxForAmmo()
	{
		// This hitbox belongs to Enemy Ammo
		this.ActivateCollisionLayer(CollisionLayers.EnemyAmmo);

		// What should enemy ammo detect? (Player/Shield HurtBoxes)
		this.ActivateCollisionMask(CollisionLayers.PlayerRegularHurtBox);
		this.ActivateCollisionMask(CollisionLayers.PlayerSpecialHurtBox);
		this.ActivateCollisionMask(CollisionLayers.PlayerShieldHurtBox);
	}
}

