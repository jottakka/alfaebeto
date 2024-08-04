using Godot;

public partial class LetterBlock : StaticBody2D
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
	public delegate void OnTargetDestructedSignalEventHandler();
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
		AnimationPlayer.AnimationFinished += OnHurtAnimationFinished;

		this.ActivateCollisionLayer(CollisionLayers.WordEnemy);

		// Collision Masks to observe
		this.ActivateCollisionMask(CollisionLayers.Player);

		SetUpHealthComponent();

	}

	public void SetLabel(char letter)
	{
		Label.Text = letter.ToString();
	}

	public void SetPosition(Vector2 position)
	{
		Position = position;
	}

	public void Destroy()
	{
		HitBox.DeactivateCollisionMasks();
		this.ResetCollisionLayerAndMask();
		AnimationPlayer.Play(LetterBlockAnimations.OnLetterBlockExplode);
	}

	private void OnHealthLevelChanged(int healthLevel)
	{
		_currenSpriteFrame = healthLevel;
		Sprite.Frame = _currenSpriteFrame;
	}

	private void OnDeath()
	{
		IsDead = true;
		DeathSpriteEffect.Visible = true;
		if (IsTarget)
		{
			_ = EmitSignal(nameof(OnTargetDestructedSignal));
		}
	}

	private void OnHurtAnimationFinished(StringName animationName)
	{
		if (animationName == LetterBlockAnimations.OnLetterBlockExplode)
		{
			_ = EmitSignal(nameof(OnReadyToDequeueSignal));
			QueueFree();
		}

		AnimationPlayer.Play(LetterBlockAnimations.RESET);
	}

	private void OnHurt(Area2D enemyArea)
	{
		if (IsDead is false)
		{
			HealthComponent.TakeDamage(10);
			AnimationPlayer.Play(LetterBlockAnimations.OnHurtLetterBlock);
		}
		else
		{
			AnimationPlayer.Play(LetterBlockAnimations.OnHurtDeadLetterBlock);
		}
	}

	private void SetUpHealthComponent()
	{
		HealthComponent.EmmitInBetweenSignals = true;
		HealthComponent.HeathLevelSignalsIntervals = 3;
		HealthComponent.OnHealthDepletedSignal += OnDeath;
		HealthComponent.OnHealthLevelChangeSignal += OnHealthLevelChanged;
	}
}
