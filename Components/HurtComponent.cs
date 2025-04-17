using AlfaEBetto.CustomNodes;
using Godot;

namespace AlfaEBetto.Components
{
	public sealed partial class HurtComponent : Node
	{
		[Export]
		public Timer HurtCooldownTimer { get; set; }
		[Export]
		public HitBox HitBox { get; set; }
		[Signal]
		public delegate void OnHurtSignalEventHandler(Area2D enemyArea2D);
		public Vector2 HitEnemyVelocity { get; private set; } = Vector2.Zero;

		public bool IsHurt { get; private set; } = false;

		public override void _Ready()
		{
			if (HurtCooldownTimer is not null)
			{
				HurtCooldownTimer.Timeout += OnHurtStateFinished;
			}

			HitBox.AreaEntered += OnHitBoxAreaEntered;
		}

		public void OnHitBoxAreaEntered(Area2D area)
		{
			if (IsHurt is false)
			{
				IsHurt = true;
				HurtCooldownTimer?.Start();
				_ = EmitSignal(nameof(OnHurtSignal), area);
			}
		}

		public void OnHurtStateFinished() => IsHurt = false;
	}
}
