using System;
using Godot;
using WordProcessing.Models.DiacriticalMarks;

public sealed partial class EnemyWord : CharacterBody2D
{
	[Export]
	public WordBuilderComponent WordBuilderComponent { get; set; }
	[Export]
	public EnemySpawner EnemySpawnerRight { get; set; }
	[Export]
	public EnemySpawner EnemySpawnerLeft { get; set; }
	[Export]
	public TurrentWing RightTurrentWing { get; set; }
	[Export]
	public TurrentWing LeftTurrentWing { get; set; }
	[Export]
	public VisibleOnScreenNotifier2D VisibleOnScreenNotifierUpper { get; set; }
	[Export]
	public VisibleOnScreenNotifier2D VisibleOnScreenNotifierBottom { get; set; }
	[Export]
	public AnimationPlayer AnimationPlayer { get; set; }
	[Export]
	public float HorizontalSpeedModulus { get; set; } = 30.0f;
	[Export]
	public float VerticalVelocityModulus { get; set; } = 10.0f;

	[Signal]
	public delegate void OnQueueFreeSignalEventHandler();

	public Word Word { get; set; }

	private WordInfo _wordInfo;

	private Vector2 _velocity;

	public override void _Ready()
	{
		_wordInfo = Global.Instance.MarkedWords.Dequeue();

		this.SetVisibilityZOrdering(VisibilityZOrdering.WordEnemy);

		this.ActivateCollisionLayer(CollisionLayers.WordEnemy);
		this.ActivateCollisionLayer(CollisionLayers.WordEnemyHitBox);
		this.ActivateCollisionLayer(CollisionLayers.WordEnemyHurtBox);

		// Collision Masks to observe
		this.ActivateCollisionMask(CollisionLayers.PlayerSpecialHurtBox);
		this.ActivateCollisionMask(CollisionLayers.Player);

		BuildWordBlocks();
		SetUpInitialStates();
		SetUpSignals();
	}

	public override void _Notification(int what)
	{
		if (what == NotificationPredelete)
		{
			_ = EmitSignal(nameof(OnQueueFreeSignal));
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = _velocity * (float)delta;
		_ = MoveAndCollide(velocity);
	}

	private void OnAnimationFinished(StringName animationName)
	{
		if (animationName == EnemyAnimations.EnemyWordDeath)
		{
			QueueFree();
		}
	}

	private void BuildWordBlocks()
	{
		Word = WordBuilderComponent.BuildWord(_wordInfo, new Vector2(0, 0));
		AddChild(Word);
	}

	private void SetUpInitialStates()
	{
		RightTurrentWing.Position += new Vector2(Word.CenterOffset, 0);
		LeftTurrentWing.Position -= new Vector2(Word.CenterOffset, 0);

		_velocity = new Vector2(
			Mathf.Abs(VerticalVelocityModulus) * Mathf.Pow(-1, GD.Randi() % 2.0f),
			Math.Abs(HorizontalSpeedModulus)
		);
	}

	private void SetUpSignals()
	{
		VisibleOnScreenNotifierUpper.ScreenEntered += () =>
		{
			EnemySpawnerLeft.AllowSpawn();
			EnemySpawnerRight.AllowSpawn();
			RightTurrentWing.AllowShoot();
			LeftTurrentWing.AllowShoot();
		};

		VisibleOnScreenNotifierUpper.ScreenExited += () =>
		{
			_velocity = new Vector2(
					_velocity.X,
					Mathf.Abs(VerticalVelocityModulus)
				);
		};

		VisibleOnScreenNotifierBottom.ScreenExited += () =>
		{
			_velocity = new Vector2(
					_velocity.X,
					-Mathf.Abs(VerticalVelocityModulus)
				);
		};

		LeftTurrentWing.VisibleOnScreenNotifier2D.ScreenExited += () =>
		{
			_velocity = new Vector2(
					Mathf.Abs(HorizontalSpeedModulus),
					_velocity.Y
				);
		};

		RightTurrentWing.VisibleOnScreenNotifier2D.ScreenExited += () =>
		{
			_velocity = new Vector2(
					-Mathf.Abs(HorizontalSpeedModulus),
					_velocity.Y
				);
		};

		Word.OnTargetBlockDestructedSignal += () =>
		{
			EnemySpawnerLeft.DesallowSpawn();
			EnemySpawnerRight.DesallowSpawn();
			RightTurrentWing.DesallowShoot();
			LeftTurrentWing.DesallowShoot();
			AnimationPlayer.Play(EnemyAnimations.EnemyWordDying);
		};

		Word.ReadyToDequeueSignal += () =>
		{
			AnimationPlayer.Play(EnemyAnimations.EnemyWordDeath);
		};

		AnimationPlayer.AnimationFinished += OnAnimationFinished;
	}
}

