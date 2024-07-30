using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
public partial class MeteorEnemyBase : Node2D
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
	public float MaxSizeProportion { get; set; } = 1.0f;
	[Export]
	public float MinSizeProportion { get; set; } = 0.6f;
	[Export]
	public float MaxSpeed { get; set; } = 150.0f;
	[Export]
	public float MinSpeed { get; set; } = 100.0f;
	[Export]
	public HitBox HitBox { get; set; }
	[Export]
	public EnemyHurtBox EnemyHurtBox { get; set; }

	private Vector2 _velocity;

	public override void _Ready()
	{
		HurtComponent.OnHurtSignal += OnHurt;
		SetUpSpinAnimation();

		HealthComponent.OnHealthDepletedSignal += QueueFree;

		VisibleOnScreenNotifier.ScreenExited += QueueFree;
		var scale = (float)GD.RandRange(MinSizeProportion, MaxSizeProportion);
		var speed = (float)GD.RandRange(MinSpeed, MaxSpeed);
		_velocity = new Vector2(0, 1) * speed;
		Scale = Vector2.One * scale;
	}

	public override void _PhysicsProcess(double delta)
	{
		Position += _velocity * (float)delta;
	}

	private void SetUpSpinAnimation()
	{
		var spinDirection = GD.Randi() % 2 == 0;

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
}
