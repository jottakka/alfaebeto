using AlfaEBetto.CustomNodes;
using AlfaEBetto.Extensions;
using Godot;

namespace AlfaEBetto.Weapons
{
	public sealed partial class Laser : Area2D
	{
		// --- Exports ---
		[Export] public Sprite2D Sprite2D { get; set; }
		[Export] public VisibleOnScreenNotifier2D VisibleOnScreenNotifier { get; set; }
		[Export] public AnimationPlayer AnimationPlayer { get; set; }
		// Note: PlayerSpecialHurtBox is exported but not used in this script currently.
		[Export] public PlayerSpecialHurtBox PlayerSpecialHurtBox { get; set; }
		[Export] public HitBox HitBox { get; set; } // Assumed to be an Area2D child for detecting hits
													// Note: CooldownSecs is exported but not used in this script currently.
		[Export] public float CooldownSecs { get; set; } = 0.1f;
		[Export] public float Speed { get; set; } = 900.0f;
		[Export] public float LaserRange { get; set; } = 500.0f;

		// --- Private Fields ---
		private float _distanceTraveled = 0.0f;
		private bool _hitOccurred = false; // Flag to prevent multiple hit processing

		public override void _Ready()
		{
			if (!ValidateExports())
			{
				GD.PrintErr($"{Name}: Missing required exported nodes. Laser may not function correctly.");
				QueueFree();
				return;
			}

			this.SetVisibilityZOrdering(VisibilityZOrdering.Ammo);

			// --- Connect Signals ---
			VisibleOnScreenNotifier.ScreenExited += OnScreenExited;
			AnimationPlayer.AnimationFinished += OnAnimationFinished;
			// Connect HitBox signal for hit detection
			HitBox.AreaEntered += OnHitBoxAreaEntered;
			// ---------------------
		}

		public override void _ExitTree()
		{
			// Disconnect signals when removed from the tree
			if (IsInstanceValid(VisibleOnScreenNotifier))
			{
				VisibleOnScreenNotifier.ScreenExited -= OnScreenExited;
			}

			if (IsInstanceValid(AnimationPlayer))
			{
				AnimationPlayer.AnimationFinished -= OnAnimationFinished;
			}
			// Disconnect HitBox signal
			if (IsInstanceValid(HitBox))
			{
				HitBox.AreaEntered -= OnHitBoxAreaEntered;
			}
		}

		public override void _PhysicsProcess(double delta)
		{
			// Don't process movement or range check if a hit has already occurred
			if (_hitOccurred)
			{
				return;
			}

			// --- Range Check ---
			if (_distanceTraveled >= LaserRange)
			{
				QueueFree(); // Remove laser if it exceeds its range
				return;
			}
			// -----------------

			// --- Movement ---
			float moveAmount = Speed * (float)delta;

			// ** FIX: Calculate movement based on the node's rotation **
			// Option 1 (Recommended if sprite points UP at rotation 0): Use rotated UP vector
			Vector2 moveDirection = Vector2.Up.Rotated(Rotation); // Vector2.Up is (0, -1)

			// Option 2 (If sprite points RIGHT at rotation 0 and you set initial rotation correctly): Use Transform.X
			// Vector2 moveDirection = Transform.X;

			Vector2 moveVector = moveDirection * moveAmount;
			Position += moveVector;
			_distanceTraveled += moveAmount; // Track distance traveled
											 // ----------------
		}

		// --- Signal Handlers ---

		/// <summary>
		/// Handles the AreaEntered signal from the HitBox child node.
		/// </summary>
		private void OnHitBoxAreaEntered(Area2D area)
		{
			// Check validity and ensure hit hasn't already been processed
			if (_hitOccurred || !IsInstanceValid(this))
			{
				return;
			}

			// GD.Print($"{Name} HitBox entered by: {area.Name}"); // Optional debug
			HandleHit(); // Trigger hit logic
		}

		/// <summary>
		/// Handles the screen exited signal from the notifier.
		/// </summary>
		private void OnScreenExited()
		{
			// Check validity before queueing free
			if (!IsInstanceValid(this))
			{
				return;
			}

			CallDeferred(MethodName.QueueFree);
		}

		/// <summary>
		/// Handles the animation finished signal, specifically for the hit animation.
		/// </summary>
		private void OnAnimationFinished(StringName animationName)
		{
			// Check validity before queueing free
			if (!IsInstanceValid(this))
			{
				return;
			}

			// QueueFree after the hit animation completes
			if (animationName == WeaponAnimations.LaserOnHit)
			{
				QueueFree();
			}
		}

		// --- Private Methods ---

		/// <summary>
		/// Validates that essential exported nodes are assigned.
		/// </summary>
		private bool ValidateExports()
		{
			bool isValid = true;
			if (VisibleOnScreenNotifier == null) { GD.PrintErr($"{Name}: Missing VisibleOnScreenNotifier!"); isValid = false; }

			if (AnimationPlayer == null) { GD.PrintErr($"{Name}: Missing AnimationPlayer!"); isValid = false; }

			if (HitBox == null) { GD.PrintErr($"{Name}: Missing HitBox!"); isValid = false; }
			// Sprite2D, PlayerSpecialHurtBox, CooldownSecs are currently unused by this script's logic.
			return isValid;
		}

		/// <summary>
		/// Handles the logic when the laser hits something.
		/// Stops movement, disables physics, plays hit animation.
		/// </summary>
		private void HandleHit()
		{
			_hitOccurred = true; // Set flag to prevent repeated hit logic
			Speed = 0; // Stop potential future movement calculations (though physics stops)
			SetPhysicsProcess(false); // Stop processing physics for this laser

			// Play hit animation (check validity)
			AnimationPlayer?.Play(WeaponAnimations.LaserOnHit);

			// If no animation player, queue free immediately
			if (AnimationPlayer == null)
			{
				GD.PrintErr($"{Name}: No AnimationPlayer found in HandleHit. Queuing free immediately.");
				QueueFree();
			}

			// Optionally disable collision shapes immediately after hit
			if (IsInstanceValid(HitBox))
			{
				// Disabling monitoring/monitorable via SetDeferred is safer if physics state might be locked
				HitBox.SetDeferred(Area2D.PropertyName.Monitoring, false);
				HitBox.SetDeferred(Area2D.PropertyName.Monitorable, false);
			}
		}
	}
}
