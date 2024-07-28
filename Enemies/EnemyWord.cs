using Godot;
using System;
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
	public float HorizontalSpeedModulus { get; set; } = 30.0f;
	[Export]
	public float VerticalVelocityModulus { get; set; } = 10.0f;

	public Word Word { get; set; }

	private WordInfo _wordInfo;

	private Vector2 _velocity;
	public override void _Ready()
	{
		_wordInfo = Global.Instance.MarkedWords.Dequeue();

		this.SetVisibilityZOrdering(VisibilityZOrdering.WordEnemy);
		// Collidion layer to act upon
		this.ActivateCollisionLayer(CollisionLayers.WordEnemyHurtBox);
		this.ActivateCollisionLayer(CollisionLayers.WordEnemyHitBox);
		this.ActivateCollisionLayer(CollisionLayers.WordEnemy);


		BuildWordBlocks();
		SetUpInitialStates();
		SetUpSignals();
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
			Mathf.Abs(VerticalVelocityModulus) * Mathf.Pow(-1, (GD.Randi() % 2.0f)),
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

		Word.OnTargetBlockDestructedSignal += QueueFree;
	}

	public override void _PhysicsProcess(double delta)
	{
		Velocity = _velocity * (float)delta;
		MoveAndCollide(Velocity);
	}
}

