using Godot;

public sealed partial class Player : CharacterBody2D
{
	[Export]
	public Sprite2D Sprite2D { get; set; }
	[Export]
	public CollisionPolygon2D CollisionPolygon2D { get; set; }
	[Export]
	public PlayerInputProcessor PlayerInputProcessor { get; set; }
	[Export]
	public Marker2D MuzzlePosition { get; set; }
	[Export]
	public WeaponComponent WeaponComponent { get; set; }
	[Export]
	public Area2D HurtBox { get; set; }
	[Export]
	public HitBox HitBox { get; set; }
	[Export]
	public HurtComponent HurtComponent { get; set; }
	[Export]
	public HealthComponent HealthComponent { get; set; }
	[Export]
	public PlayerShield PlayerShield { get; set; }
	[Export]
	public float Speed { get; set; } = 600.0f;

	public override void _Ready()
	{
		MotionMode = MotionModeEnum.Floating;
		this.SetVisibilityZOrdering(VisibilityZOrdering.PlayerAndEnemies);

		this.ResetCollisionLayerAndMask();
		this.ActivateCollisionLayer(CollisionLayers.Player);

		this.ActivateCollisionMask(CollisionLayers.Collectables);
		this.ActivateCollisionMask(CollisionLayers.MeteorEnemy);
		this.ActivateCollisionMask(CollisionLayers.WordEnemy);
	}

	public override void _PhysicsProcess(double delta)
	{
		PhysicsProcessInternal((float)delta);
	}

	private void PhysicsProcessInternal(float delta)
	{
		Vector2 movementDirection = PlayerInputProcessor.MovementDirection;
		Vector2 velocity = movementDirection * (Speed * delta);
		GlobalPosition = new Vector2(
			Mathf.Clamp(GlobalPosition.X, 0, GetViewportRect().Size.X),
			Mathf.Clamp(GlobalPosition.Y, 0, GetViewportRect().Size.Y)
		);

		KinematicCollision2D collisionInfo = PlayerShield.IsActive ?
			PlayerShield.MoveAndCollide(velocity) :
			MoveAndCollide(velocity);

		UpdatePlayerPositionWhenShieldActive();

		if (collisionInfo is not null)
		{
			if (PlayerShield.IsActive)
			{
				OnCollisionHandler(collisionInfo, PlayerShield);
			}
			else
			{
				OnCollisionHandler(collisionInfo, this);
			}

			UpdatePlayerPositionWhenShieldActive();
		}

		UpdateShieldPositionWhenActive();
	}

	private void OnCollisionHandler(KinematicCollision2D collisionInfo, CharacterBody2D body)
	{
		body.GlobalPosition += collisionInfo.GetNormal() * 40.0f;
	}

	private void UpdatePlayerPositionWhenShieldActive()
	{
		if (PlayerShield.IsActive)
		{
			GlobalPosition = PlayerShield.GlobalPosition;
		}
		else
		{
			PlayerShield.GlobalPosition = GlobalPosition;
		}
	}

	private void UpdateShieldPositionWhenActive()
	{
		if (PlayerShield.IsActive)
		{
			PlayerShield.GlobalPosition = GlobalPosition;
		}
	}
}
