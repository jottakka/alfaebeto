using System.Linq;
using Godot;

public sealed partial class OwlFriend : CharacterBody2D
{
	[Export]
	public Area2D DetectionArea { get; set; }
	[Export]
	public Timer CooldownTimer { get; set; }
	[Export]
	public Area2D HurtBox { get; set; }
	[Export]
	public AnimationPlayer AnimationPlayer { get; set; }
	[Export]
	public int HitPoints = 100;
	[Export]
	public float AttackSpeed = 400.0f;
	[Export]
	public float RegularSpeed = 300.0f;

	public EnemyBase Target { get; private set; } = null;

	public bool HasTarget => IsInstanceValid(Target);
	private Player _player => Global.Instance.Player;

	private bool _attackReady = true;

	public override void _Ready()
	{
		this.ResetCollisionLayerAndMask();
		DetectionArea.ResetCollisionLayerAndMask();
		this.SetVisibilityZOrdering(VisibilityZOrdering.PlayerAndEnemies);

		DetectionArea.ActivateCollisionMask(CollisionLayers.RegularEnemyHitBox);

		ActivateCollisions();

		DetectionArea.AreaEntered += OnDetectionAreaEntered;
		DetectionArea.AreaExited += OnDetectionAreaExited;

		HurtBox.AreaEntered += OnHurtBoxEntered;

		CooldownTimer.OneShot = true;
		CooldownTimer.Timeout += () => _attackReady = true;
	}
	public override void _PhysicsProcess(double delta)
	{
		if (HasTarget is false)
		{
			DeactivateCollisions();
			EnemyBase monster = TryGetMonsterInsideDetctionArea();
			if (monster is not null)
			{
				Target = monster;
			}
			else
			{
				ProcessIfNoTarget();
			}

			AnimationPlayer.Play(WeaponAnimations.RESET);
		}
		else
		{

			ProcessIfHasTarget();
			AnimationPlayer.Play(WeaponAnimations.OnOwlFriendEyesOnAttack);
		}

		_ = MoveAndSlide();
	}

	private EnemyBase TryGetMonsterInsideDetctionArea()
	{
		return DetectionArea
			.GetOverlappingAreas()
			.OfType<HitBox>()
			.FirstOrDefault()
			?.GetParent<EnemyBase>();
	}

	private void OnHurtBoxEntered(Area2D area2D)
	{
		if (area2D.GetParent() is EnemyBase)
		{
			_attackReady = false;
			DeactivateCollisions();
			CooldownTimer.Start();
		}
	}

	private void ProcessIfHasTarget()
	{
		if (_attackReady)
		{
			ActivateCollisions();
			Vector2 direction = GlobalPosition.DirectionTo(Target.GlobalPosition);
			Velocity = direction * AttackSpeed;

			LookAtCompensated(Target.GlobalPosition);
			Rotate(Mathf.Pi / 2.0f);

			_ = MoveAndSlide();
		}
		else
		{
			ProcessOnJustAfterAttack();
		}
	}

	private void ProcessOnJustAfterAttack()
	{
		Velocity = Vector2.Zero;
		LookAtUp();

		if (GlobalPosition.DistanceTo(Target.GlobalPosition) <= 100.0f)
		{
			Vector2 direction = -GlobalPosition.DirectionTo(Target.GlobalPosition);
			Velocity = direction * RegularSpeed;
		}

		AddSinToMovement();
	}

	private void ProcessIfNoTarget()
	{
		Velocity = Vector2.Zero;
		LookAtUp();
		if (GlobalPosition.DistanceTo(_player.GlobalPosition) >= 100.0f)
		{
			Vector2 direction = GlobalPosition.DirectionTo(_player.GlobalPosition);
			Velocity = direction * RegularSpeed;
		}

		AddSinToMovement();
	}

	private void AddSinToMovement()
	{
		Velocity += new Vector2(
			0,
			20.0f * Mathf.Sin(Time.GetTicksUsec() / 500000.0f)
		);
	}

	private void OnDetectionAreaExited(Area2D area)
	{
		if (area.GetParent() is EnemyBase characterBody)
		{
			if (Target == characterBody)
			{
				DeactivateTarget();
			}
		}
	}
	private void OnDetectionAreaEntered(Area2D area)
	{
		if (HasTarget)
		{
			return;
		}

		if (area.GetParent() is EnemyBase characterBody)
		{
			Target = characterBody;
		}
	}

	private void ActivateCollisions()
	{
		HurtBox.ActivateCollisionLayer(CollisionLayers.PlayerRegularHurtBox);
		HurtBox.ActivateCollisionMask(CollisionLayers.RegularEnemyHitBox);
	}

	private void DeactivateCollisions()
	{
		HurtBox.ResetCollisionLayerAndMask();
		this.ResetCollisionLayerAndMask();
	}

	private void DeactivateTarget()
	{
		Target = null;
	}

	private void LookAtUp()
	{
		Rotation = 0;
	}

	private void LookAtCompensated(Vector2 targetGlobalPosition)
	{
		LookAt(targetGlobalPosition);
	}
}
