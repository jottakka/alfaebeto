using AlfaEBetto.Extensions;
using Godot;

namespace AlfaEBetto.Ammo
{
	public partial class AmmoBase : Area2D
	{
		[Export]
		public Sprite2D Sprite { get; set; }
		[Export]
		public CollisionShape2D CollisionShape { get; set; }
		[Export]
		public VisibleOnScreenNotifier2D VisibleOnScreenNotifier { get; set; }
		[Export]
		public AnimationPlayer AnimationPlayer { get; set; }
		[Export]
		public Sprite2D ExplosionSprite { get; set; }
		[Export]
		public float ShootRadAngle { get; set; }
		[Export]
		public Vector2 InitialPosition { get; set; }
		[Export]
		public float Speed { get; set; } = 300.0f;

		private Vector2 _direction;

		public override void _Ready()
		{
			VisibleOnScreenNotifier.ScreenExited += OnScreenExited;
			AnimationPlayer.AnimationFinished += OnAnimationFinished;
			AreaEntered += OnAreaEntered;

			ExplosionSprite.Frame = (int)(GD.Randi() % 4);

			Rotate(ShootRadAngle);

			_direction = new Vector2(1, 0).Rotated(ShootRadAngle);
			GlobalPosition = InitialPosition;
			ZIndex = (int)VisibilityZOrdering.Ammo;

			this.ResetCollisionLayerAndMask();
			this.ActivateCollisionLayer(CollisionLayers.EnemyAmmo);

			this.ActivateCollisionMask(CollisionLayers.PlayerHitBox);
			this.ActivateCollisionMask(CollisionLayers.PlayerShieldHitBox);
			this.ActivateCollisionMask(CollisionLayers.MeteorEnemyHitBox);
		}

		public override void _PhysicsProcess(double delta) => Position += _direction * Speed * (float)delta;

		private void OnAreaEntered(Area2D area)
		{
			Speed = 0;
			AnimationPlayer.Play(AmmoAnimations.AmmoExplosion);
		}

		private void OnAnimationFinished(StringName animationName)
		{
			if (animationName == AmmoAnimations.AmmoExplosion)
			{
				Visible = false;
				QueueFree();
			}
		}

		private void OnScreenExited() => QueueFree();
	}
}
