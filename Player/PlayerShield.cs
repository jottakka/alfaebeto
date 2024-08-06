using Godot;

public sealed partial class PlayerShield : CharacterBody2D
{
	[Export]
	public AnimationPlayer AnimationPlayer { get; set; }
	[Export]
	public HitBox HitBox { get; set; }
	[Export]
	public bool IsActive { get; set; } = false;
	[Export]
	public int MaxShieldPoints { get; set; } = 60;

	[Signal]
	public delegate void OnShieldPointsChangedSignalEventHandler(int currentValue, bool isIncrease);

	public int CurrentShieldPoints { get; private set; }

	private bool _isShieldUp = false;
	private Player _player => GetParent<Player>();

	public override void _Ready()
	{
		CurrentShieldPoints = 0;
		MotionMode = MotionModeEnum.Floating;
		this.SetVisibilityZOrdering(VisibilityZOrdering.PlayerAndEnemies);

		Deactivate();

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

	public void Deactivate()
	{
		IsActive = false;
		Visible = false;
		_isShieldUp = false;
		DeactivateCollisions();
	}

	public void Activate()
	{
		IsActive = true;
		Visible = true;
		ActivateCollisions();
		AnimationPlayer.Play(PlayerAnimations.OnPlayerShieldUp);

	}

	public void AddShieldPoints(int points)
	{
		if (CurrentShieldPoints == MaxShieldPoints)
		{
			return;
		}

		if (IsActive is false)
		{
			Activate();
		}

		CurrentShieldPoints += points;
		CurrentShieldPoints = Mathf.Clamp(CurrentShieldPoints, 0, MaxShieldPoints);
		_ = EmitSignal(nameof(OnShieldPointsChangedSignal), CurrentShieldPoints, true);
	}

	public void RemoveShieldPoints(int points)
	{
		if (_isShieldUp is false)
		{
			return;
		}

		CurrentShieldPoints -= points;
		CurrentShieldPoints = Mathf.Clamp(CurrentShieldPoints, 0, MaxShieldPoints);
		if (CurrentShieldPoints == 0)
		{
			Deactivate();
		}

		_ = EmitSignal(nameof(OnShieldPointsChangedSignal), CurrentShieldPoints, false);
	}

	private void OnCollision()
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
		HitBox.DeactivateCollisions();
	}

	private void OnAnimationFinished(StringName animationName)
	{
		if (animationName == PlayerAnimations.OnPlayerShieldUp)
		{
			OnShieldUpFinished();
			return;
		}

		if (animationName == PlayerAnimations.OnPlayerShieldHit)
		{
			RemoveShieldPoints(10);
			AnimationPlayer.Play(PlayerAnimations.RESET);
		}
	}

	private void OnShieldUpFinished()
	{
		_isShieldUp = true;
		AnimationPlayer.Play(PlayerAnimations.RESET);
	}
}
