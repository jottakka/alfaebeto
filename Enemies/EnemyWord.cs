using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public sealed partial class EnemyWord : CharacterBody2D
{
	[Export]
	public WordBuilderComponent WordBuilderComponent { get; set; }
	[Export]
	public TurrentWing RightTurrentWing { get; set; }
	[Export]
	public TurrentWing LeftTurrentWing { get; set; }
	[Export]
	public VisibleOnScreenNotifier2D VisibleOnScreenNotifier2D  { get; set; }
	[Export]
	public float HorizontalSpeedModulus { get; set; } = 30.0f;
	[Export]
	public float VerticalVelocityModulus { get; set; } = 10.0f;

	[Export]
	public Word Word { get; set; }

	private Vector2 _velocity;
	public override void _Ready()
	{
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
		Word = WordBuilderComponent.BuildWord("PALAVRA", new Vector2(0, 0));
		AddChild(Word);
	}

	private void SetUpInitialStates()
	{
		RightTurrentWing.Position += new Vector2(Word.CenterOffset, 0);
		LeftTurrentWing.Position -= new Vector2(Word.CenterOffset, 0);
		_velocity = new Vector2(
			Mathf.Abs(VerticalVelocityModulus),
			Math.Abs(HorizontalSpeedModulus)
		);
	}

	private void SetUpSignals()
	{
		VisibleOnScreenNotifier2D.ScreenExited += QueueFree;

		LeftTurrentWing.VisibleOnScreenNotifier2D.ScreenExited += () =>
		{
			_velocity = new Vector2(
					Mathf.Abs(HorizontalSpeedModulus),
					Math.Abs(VerticalVelocityModulus)
				);
		};
		RightTurrentWing.VisibleOnScreenNotifier2D.ScreenExited += () =>
		{
			_velocity = new Vector2(
					-Mathf.Abs(HorizontalSpeedModulus),
					Math.Abs(VerticalVelocityModulus)
				);
		};
	}

	public override void _PhysicsProcess(double delta)
	{
		Velocity = _velocity*(float)delta;
		MoveAndCollide(Velocity);
	}
}

