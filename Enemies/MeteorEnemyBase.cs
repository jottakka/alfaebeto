using Godot;
public partial class MeteorEnemyBase : StaticBody2D
{
	[Export]
	public AnimationPlayer AnimationPlayer { get; set; }
	[Export]
	public VisibleOnScreenNotifier2D VisibleOnScreenNotifier { get; set; }
	[Export]
	public HealthComponent HealthComponent { get; set; }
	[Export]
	public HurtComponent HurtComponent { get; set; }
	[Export]
	public Sprite2D Sprite2D { get; set; }
	[Export]
	public float MaxSizeProportion { get; set; } = 1.5f;
	[Export]
	public float MinSizeProportion { get; set; } = 1.0f;
	[Export]
	public float MaxSpeed { get; set; } = 60.0f;
	[Export]
	public float MinSpeed { get; set; } = 40.0f;
	[Export]
	public HitBox HitBox { get; set; }
	[Export]
	public EnemyHurtBox EnemyHurtBox { get; set; }

	private Vector2 _velocity;
	private int _currenSpriteFrame = 0;

	public override void _Ready()
	{
		HurtComponent.OnHurtSignal += OnHurt;
		SetUpHealthComponent();
		VisibleOnScreenNotifier.ScreenExited += QueueFree;
		float scale = (float)GD.RandRange(MinSizeProportion, MaxSizeProportion);
		float speed = (float)GD.RandRange(MinSpeed, MaxSpeed);
		_velocity = new Vector2(0, 1) * speed;
		Scale = Vector2.One * scale;

		this.ActivateCollisionLayer(CollisionLayers.MeteorEnemy);

		// Collision Masks to observe
		this.ActivateCollisionMask(CollisionLayers.Player);

		SetUpSpinAnimation();
	}

	public override void _PhysicsProcess(double delta)
	{
		Position += _velocity * (float)delta;
	}

	private void SetUpSpinAnimation()
	{
		bool spinDirection = GD.Randi() % 2 == 0;

		if (spinDirection)
		{
			AnimationPlayer.Play(EnemyAnimations.MeteorEnemySpin);
		}
		else
		{
			AnimationPlayer.PlayBackwards(EnemyAnimations.MeteorEnemySpin);
		}
	}

	private void OnHurt(Area2D enemyArea)
	{
		if (enemyArea is PlayerSpecialHurtBox)
		{
			HealthComponent.TakeDamage(10);
		}
	}

	private void OnHealthLevelChanged(int healthLevel)
	{
		_currenSpriteFrame = healthLevel;
		Sprite2D.Frame = _currenSpriteFrame;
	}

	private void SetUpHealthComponent()
	{
		HealthComponent.EmmitInBetweenSignals = true;
		HealthComponent.HeathLevelSignalsIntervals = 6;
		HealthComponent.OnHealthDepletedSignal += QueueFree;
		HealthComponent.OnHealthLevelChangeSignal += OnHealthLevelChanged;
	}
}
