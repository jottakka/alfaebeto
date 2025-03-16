using Godot;

public partial class WordBlock : StaticBody2D
{
	[Export]
	public Sprite2D Sprite { get; set; }
	[Export]
	public Label Label { get; set; }
	[Export]
	public CollisionShape2D CollisionShape { get; set; }
	[Export]
	private AnimationPlayer AnimationPlayer { get; set; }
	[Export]
	public HitBox HitBox { get; set; }
	[Export]
	public HurtComponent HurtComponent { get; set; }
	[Export]
	public bool IsTarget { get; set; }
	[Export]
	public Sprite2D DeathSpriteEffect { get; set; }
	[Export]
	public Sprite2D ExplosionsSprite2D { get; set; }
	[Export]
	private HealthComponent HealthComponent { get; set; }

	[Signal]
	public delegate void OnTargetBlockCalledDestructionSignalEventHandler();
	[Signal]
	public delegate void OnLetterDestructedSignalEventHandler(bool isTarget);
	[Signal]
	public delegate void OnReadyToDequeueSignalEventHandler();

	public bool IsDead { get; private set; }

	private int _currenSpriteFrame = 0;

	public override void _Ready()
	{

		ExplosionsSprite2D.Frame = GD.RandRange(0, 8);
		Sprite.Frame = _currenSpriteFrame;
		DeathSpriteEffect.Visible = false;

		HurtComponent.OnHurtSignal += OnHurt;
		AnimationPlayer.AnimationFinished += OnAnimationFinished;

		this.ActivateCollisionLayer(CollisionLayers.WordEnemy);

		// Collision Masks to observe
		this.ActivateCollisionMask(CollisionLayers.Player);

		SetUpHealthComponent();

	}

	public void SetLabel(char letter)
	{
		Label.Text = letter.ToString();
	}

	public void SetBlockPosition(Vector2 position)
	{
		Position = position;
	}

	public void Destroy()
	{
		AnimationPlayer.Play(LetterBlockAnimations.OnLetterBlockExplode);
	}

	public void DisableCollisions()
	{
		HitBox.DeactivateCollisions();
		this.ResetCollisionLayerAndMask();
	}

	private void OnHealthLevelChanged(int healthLevel)
	{
		_currenSpriteFrame = healthLevel;
		Sprite.Frame = _currenSpriteFrame;
	}

	private void OnAnimationFinished(StringName animationName)
	{

		if (animationName == LetterBlockAnimations.OnLetterBlockExplode)
		{
			_ = EmitSignal(nameof(OnReadyToDequeueSignal));
			QueueFree();
		}

		if (animationName == LetterBlockAnimations.OnLetterBlockDyingTarget)
		{
			_ = EmitSignal(nameof(OnTargetBlockCalledDestructionSignal));
		}
		else
		{
			AnimationPlayer.Play(LetterBlockAnimations.RESET);
		}
	}

	private void OnHurt(Area2D enemyArea)
	{
		if (IsDead is false)
		{
			TakeDamage();
		}
		else
		{
			AnimationPlayer.Play(LetterBlockAnimations.OnHurtDeadLetterBlock);
		}
	}

	private void TakeDamage()
	{
		HealthComponent.TakeDamage(10);
		if (HealthComponent.IsDead is false)
		{

			AnimationPlayer.Play(LetterBlockAnimations.OnHurtLetterBlock);
		}
	}

	private void StartDestruction()
	{
		IsDead = true;
		DeathSpriteEffect.Visible = true;
		_ = EmitSignal(nameof(OnLetterDestructedSignal), IsTarget);
		if (IsTarget)
		{
			DisableCollisions();
			AnimationPlayer.Play(LetterBlockAnimations.OnLetterBlockDyingTarget);
		}
		else
		{
			AnimationPlayer.Play(LetterBlockAnimations.OnLetterBlockDyingNotTarget);
		}
	}

	private void SetUpHealthComponent()
	{
		HealthComponent.EmmitInBetweenSignals = true;
		HealthComponent.HeathLevelSignalsIntervals = 3;
		HealthComponent.OnHealthDepletedSignal += StartDestruction;
		HealthComponent.OnHealthLevelChangeSignal += OnHealthLevelChanged;
	}
}
