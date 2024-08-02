using Godot;

public sealed partial class PlayerShield : CharacterBody2D
{
	[Export]
	public AnimationPlayer AnimationPlayer { get; set; }
	[Export]
	public HitBox HitBox { get; set; }
	[Export]
	public bool IsActive { get; set; } = true;
	[Export]
	public int MaxShieldPoints { get; set; } = 100;

	private int _shieldPoints;

	private Player _player => GetParent<Player>();

	public override void _Ready()
	{
		_shieldPoints = MaxShieldPoints;
		MotionMode = MotionModeEnum.Floating;
		this.ResetCollisionLayerAndMask();

		this.SetVisibilityZOrdering(VisibilityZOrdering.PlayerAndEnemies);

		ActivateCollisions();

		AnimationPlayer.Play(PlayerAnimations.RESET);

		AnimationPlayer.AnimationFinished += OnAnimationFinished;

		HitBox.AreaEntered += (_) =>
		{
			OnCollision();

		};
		HitBox.BodyEntered += (_) =>
		{
			OnCollision();

		};
	}

	public void AddShieldPoints(int points)
	{
		if (!IsActive)
		{
			IsActive = true;
		}

		_shieldPoints += points;
		_shieldPoints = Mathf.Clamp(_shieldPoints, 0, MaxShieldPoints);
	}

	public void RemoveShieldPoints(int points)
	{
		_shieldPoints -= points;
		_shieldPoints = Mathf.Clamp(_shieldPoints, 0, MaxShieldPoints);
		if (_shieldPoints == 0)
		{
			IsActive = false;
			this.ResetCollisionLayerAndMask();
		}
	}

	public void OnCollision()
	{
		AnimationPlayer.Play(PlayerAnimations.OnPlayerShieldHit);
	}

	private void ActivateCollisions()
	{
		this.ActivateCollisionLayer(CollisionLayers.PlayerShield);
		this.ActivateCollisionLayer(CollisionLayers.PlayerShieldHurtBox);

		this.ActivateCollisionMask(CollisionLayers.RegularEnemy);
		this.ActivateCollisionMask(CollisionLayers.WordEnemy);
		this.ActivateCollisionMask(CollisionLayers.MeteorEnemy);

		HitBox.ActivateCollisionsMasks();
	}

	private void DeactivateCollisions()
	{
		this.ResetCollisionLayerAndMask();
		HitBox.DeactivateCollisionMasks();
	}

	private void OnAnimationFinished(StringName animationName)
	{
		if (animationName == PlayerAnimations.OnPlayerShieldHit)
		{
			RemoveShieldPoints(10);
			AnimationPlayer.Play(PlayerAnimations.RESET);
		}
	}
}
