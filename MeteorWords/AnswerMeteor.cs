using Godot;

public sealed partial class AnswerMeteor : StaticBody2D
{
	[Export]
	public HitBox HitBox { get; set; }
	[Export]
	public HurtComponent HurtComponent { get; set; }
	[Export]
	public EnemyHurtBox HurtBox { get; set; }
	[Export]
	public Sprite2D CrackSprite2D { get; set; }
	[Export]
	public HealthComponent HealthComponent { get; set; }
	[Export]
	public AnimationPlayer AnimationPlayer { get; set; }
	[Export]
	public AnimationPlayer EffectsPlayer { get; set; }
	[Export]
	public Label OptionText { get; set; }

	[Signal]
	public delegate void OnTargetDestroiedSignalEventHandler();

	public bool IsTarget { get; set; } = false;

	private int _healthLevels = 4;

	public override void _Ready()
	{
		ActivateBodyCollisions();

		SetUpHealthComponent();

		HurtComponent.OnHurtSignal += OnHurt;
		AnimationPlayer.Play(MeteorAnimations.AnswerMeteorMoving);
	}

	public void DeactivateCollisions()
	{
		HitBox.DeactivateCollisionMasks();
		HurtBox.DeactivateCollisionMasks();
		DeactivateBodyCollisions();
	}

	public void ActivateCollisions()
	{
		HitBox.ActivateCollisionsMasks();
		HurtBox.ActivateCollisionsMasks();
		ActivateBodyCollisions();
	}

	private void DeactivateBodyCollisions()
	{
		this.ResetCollisionLayerAndMask();
	}

	private void ActivateBodyCollisions()
	{
		this.ActivateCollisionLayer(CollisionLayers.MeteorEnemy);
	}

	private void OnHealthDepleted()
	{
		if (IsTarget)
		{
			_ = EmitSignal(nameof(OnTargetDestroiedSignal));
		}
	}

	private void OnHurt(Area2D enemyArea)
	{
		HealthComponent.TakeDamage(10);
		EffectsPlayer.Play(MeteorAnimations.AnswerMeteorHurt);
	}

	private void OnHealthLevelChanged(int healthLevel)
	{
		Color newCollor = CrackSprite2D.Modulate;
		newCollor.A = 1.0f / _healthLevels * healthLevel;
		CrackSprite2D.Modulate = newCollor;
	}

	private void SetUpHealthComponent()
	{
		HealthComponent.EmmitInBetweenSignals = true;
		HealthComponent.HeathLevelSignalsIntervals = _healthLevels;
		HealthComponent.OnHealthDepletedSignal += OnHealthDepleted;
		HealthComponent.OnHealthLevelChangeSignal += OnHealthLevelChanged;
	}
}

