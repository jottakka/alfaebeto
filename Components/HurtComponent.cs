using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public sealed partial class HurtComponent : Node
{
	[Export]
	public Timer HurtCooldownTimer { get; set; }
	[Export]
	public HitBox HitBox { get; set; }
	[Signal]
	public delegate void OnHurtSignalEventHandler(Area2D enemyArea2D);
	public Vector2 HitEnemyVelocity { get; private set; } = Vector2.Zero;

	public bool IsHurt { get; private set;  } = false;

	public override void _Ready()
	{
		if(HurtCooldownTimer is not null)
		{
			HurtCooldownTimer.Timeout += OnHurtStateFinished;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (IsHurt is false)
		{
			var overlapingArea = HitBox.GetOverlappingAreas().FirstOrDefault();

			if (overlapingArea is not null)
			{
				OnHitBoxBodyEntered();
				//HitEnemyVelocity = overlapingArea.GetParent<CharacterBody2D>()?.Velocity ?? Vector2.Zero;
				EmitSignal(nameof(OnHurtSignal), overlapingArea);
			}
		}
	}

	public void OnHitBoxBodyEntered()
	{
		IsHurt = true;
		HurtCooldownTimer?.Start();
	}

	public void OnHurtStateFinished()
	{
		IsHurt = false;
	}
}

