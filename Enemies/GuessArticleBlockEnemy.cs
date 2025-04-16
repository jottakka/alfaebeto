using System;
using Godot;
using WordProcessing.Util;

public sealed partial class GuessArticleBlockEnemy : CharacterBody2D
{
	[Export]
	public WordsSetBuilderComponent WordsSetBuilderComponent { get; set; }
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
	public GemSpawnerComponent GemSpawnerComponent { get; set; }
	[Export]
	public Label GuessBlockLabel { get; set; }
	[Export]
	public float HorizontalSpeedModulus { get; set; } = 30.0f;
	[Export]
	public float VerticalVelocityModulus { get; set; } = 10.0f;
	[Export]
	public int NumberOfWrongOptions { get; set; } = 3;

	[Signal]
	public delegate void OnQueueFreeSignalEventHandler();

	public WordsSet WordSetBlocks { get; set; }

	private GuessBlockWordResource _guessBlockWordInfo;

	private Vector2 _velocity;

	private int _errorCount = 0;

	public override void _Ready()
	{
		_guessBlockWordInfo = GermanArticleResourceProvider.GetRandomResource();

		GuessBlockLabel.Text = _guessBlockWordInfo.ToBeGuessed;

		this.SetVisibilityZOrdering(VisibilityZOrdering.WordEnemy);

		this.ActivateCollisionLayer(CollisionLayers.WordEnemy);
		this.ActivateCollisionLayer(CollisionLayers.WordEnemyHitBox);
		this.ActivateCollisionLayer(CollisionLayers.WordEnemyHurtBox);

		// Collision Masks to observe
		this.ActivateCollisionMask(CollisionLayers.PlayerSpecialHurtBox);
		this.ActivateCollisionMask(CollisionLayers.Player);

		BuildArticleSetBlocks();
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

	private void BuildArticleSetBlocks()
	{
		WordSetBlocks = WordsSetBuilderComponent.BuildArticleSet(_guessBlockWordInfo, new Vector2(0, 0), NumberOfWrongOptions);
		AddChild(WordSetBlocks);
	}

	private void SetUpInitialStates()
	{
		RightTurrentWing.Position += new Vector2(WordSetBlocks.CenterOffset, 0);
		LeftTurrentWing.Position -= new Vector2(WordSetBlocks.CenterOffset, 0);

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

		WordSetBlocks.OnLetterDestructedSignal += OnLetterBlockDestruct;

		WordSetBlocks.ReadyToDequeueSignal += () =>
		{
			AnimationPlayer.Play(EnemyAnimations.EnemyWordDeath, customSpeed: 2);

		};

		AnimationPlayer.AnimationFinished += OnAnimationFinished;
	}

	private void OnLetterBlockDestruct(bool isTarget)
	{
		if (isTarget)
		{
			EnemySpawnerLeft.DisallowSpawn();
			EnemySpawnerRight.DisallowSpawn();
			RightTurrentWing.DesallowShoot();
			LeftTurrentWing.DesallowShoot();
			GemSpawnerComponent.SpawnGem(
				GlobalPosition,
				GemType.Red,
				GetSpawnGemsQuantity()
			);
			AnimationPlayer.Play(EnemyAnimations.EnemyWordDying);
		}
		else
		{
			_errorCount++;
		}
	}

	private int GetSpawnGemsQuantity()
	{
		float percentage =
			(NumberOfWrongOptions + 1 - _errorCount) / (float)(NumberOfWrongOptions + 1);

		return percentage switch
		{
			1.0f => 4,
			>= 0.8f => 3,
			>= 0.4f => 2,
			_ => 1
		};
	}

	private static class GermanArticleResourceProvider
	{
		public static GuessBlockWordResource GetRandomResource()
		{
			GuessArticleWordResource germanWord = GermanGenderResourceProvider.GetRandomResource();

			return new()
			{
				AnswerIdx = germanWord.AnswerIdx,
				ShuffledOptions = germanWord.ArticleOptions,
				ToBeGuessed = germanWord.ToBeGuessed
			};
		}
	}
}
