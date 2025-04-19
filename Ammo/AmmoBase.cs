using AlfaEBetto.Extensions;
using Godot;

namespace AlfaEBetto.Ammo;

public partial class AmmoBase : Area2D
{
	#region Exports
	[Export] public Sprite2D Sprite { get; set; }
	[Export] public CollisionShape2D CollisionShape { get; set; }
	[Export] public VisibleOnScreenNotifier2D VisibleOnScreenNotifier { get; set; }
	[Export] public AnimationPlayer AnimationPlayer { get; set; } // Renamed export
	[Export] public Sprite2D ExplosionSprite { get; set; }

	[ExportGroup("Movement & Spawning")]
	[Export] public float ShootRadAngle { get; set; } // Angle in radians
	[Export] public Vector2 InitialPosition { get; set; } // Set by spawner
	[Export] public float Speed { get; set; } = 300.0f;
	#endregion

	#region Private Fields
	private Vector2 _direction;
	private bool _isExploding = false; // State flag
	#endregion

	#region Godot Methods
	public override void _Ready()
	{
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name}: Missing required exported nodes. Queuing free.");
			QueueFree();
			return;
		}

		// Initial Setup
		GlobalPosition = InitialPosition;
		ZIndex = (int)VisibilityZOrdering.Ammo; // Assuming VisibilityZOrdering enum exists

		// Calculate direction based on angle.
		// Vector2.Right corresponds to 0 rotation angle.
		_direction = Vector2.Right.Rotated(ShootRadAngle);
		// Apply initial rotation to the node itself for visual alignment.
		Rotation = ShootRadAngle;

		// Configure collision
		this.ResetCollisionLayerAndMask();
		this.ActivateCollisionLayer(CollisionLayers.EnemyAmmo); // Belongs to EnemyAmmo layer
																// Define what it can hit
		this.ActivateCollisionMask(CollisionLayers.PlayerHitBox);
		this.ActivateCollisionMask(CollisionLayers.PlayerShieldHitBox);
		this.ActivateCollisionMask(CollisionLayers.MeteorEnemyHitBox); // Assuming this is a valid layer

		// Setup explosion visual
		if (ExplosionSprite.Hframes > 0) // Check if sprite has frames
		{
			ExplosionSprite.Frame = (int)(GD.Randi() % ExplosionSprite.Hframes); // Use Hframes
		}

		ExplosionSprite.Visible = false; // Start hidden

		// Connect signals
		ConnectSignals();
	}

	public override void _ExitTree() => DisconnectSignals();

	public override void _PhysicsProcess(double delta)
	{
		// Stop moving if exploding
		if (_isExploding)
		{
			return;
		}

		// Standard linear movement
		Position += _direction * Speed * (float)delta;
	}
	#endregion

	#region Signal Handling
	private void ConnectSignals()
	{
		if (VisibleOnScreenNotifier != null)
		{
			VisibleOnScreenNotifier.ScreenExited += OnScreenExited;
		}

		if (AnimationPlayer != null)
		{
			AnimationPlayer.AnimationFinished += OnAnimationFinished;
		}

		// Connect AreaEntered last
		AreaEntered += OnAreaEntered;
	}

	private void DisconnectSignals()
	{
		// Disconnect other signals
		if (IsInstanceValid(VisibleOnScreenNotifier))
		{
			VisibleOnScreenNotifier.ScreenExited -= OnScreenExited;
		}

		if (IsInstanceValid(AnimationPlayer))
		{
			AnimationPlayer.AnimationFinished -= OnAnimationFinished;
		}

		// --- REMOVED this line ---
		// AreaEntered -= OnAreaEntered;
		// Reason: This signal is explicitly disconnected within StartExplosion()
		// to prevent multiple triggers. Attempting to disconnect again here
		// causes the "nonexistent connection" error if an explosion occurred.
		// If no explosion occurred (e.g., ScreenExited), removing a non-existent
		// connection here wouldn't error, but it's cleaner to omit it since
		// StartExplosion *always* disconnects it upon a hit.
	}

	private void OnAreaEntered(Area2D area)
	{
		if (_isExploding)
		{
			return;
		}

		StartExplosion();
	}

	private void OnAnimationFinished(StringName animationName)
	{
		if (animationName == AmmoAnimations.AmmoExplosion)
		{
			QueueFree();
		}
	}

	private void OnScreenExited() => QueueFree();
	#endregion

	#region Internal Logic
	private void StartExplosion()
	{
		if (_isExploding)
		{
			return;
		}

		_isExploding = true;

		Speed = 0;

		// Disable further collision checks *AND* disconnect the signal here
		CollisionShape?.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		AreaEntered -= OnAreaEntered; // Disconnect immediately

		if (Sprite != null)
		{
			Sprite.Visible = false;
		}

		if (ExplosionSprite != null)
		{
			ExplosionSprite.Visible = true;
		}

		AnimationPlayer?.Play(AmmoAnimations.AmmoExplosion);

		if (AnimationPlayer == null)
		{
			GD.PrintErr($"{Name}: Missing AnimationPlayer, cannot play explosion. Queuing free.");
			QueueFree();
		}
	}

	private bool ValidateExports()
	{
		// ...(Validation logic remains the same)...
		bool isValid = true;
		void CheckNode(Node node, string name)
		{
			if (node == null) { GD.PrintErr($"{Name} ({GetPath()}): Exported node '{name}' is null!"); isValid = false; }
		}

		CheckNode(Sprite, nameof(Sprite));
		CheckNode(CollisionShape, nameof(CollisionShape));
		CheckNode(VisibleOnScreenNotifier, nameof(VisibleOnScreenNotifier));
		CheckNode(AnimationPlayer, nameof(AnimationPlayer)); // Use renamed property
		CheckNode(ExplosionSprite, nameof(ExplosionSprite));

		return isValid;
	}
	#endregion
}
