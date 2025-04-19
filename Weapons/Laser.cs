using AlfaEBetto.CustomNodes;
using AlfaEBetto.Extensions;
using Godot;

namespace AlfaEBetto.Weapons;
public sealed partial class Laser : Area2D
{
	// --- Exports ---
	[Export] public Sprite2D Sprite2D { get; set; }
	[Export] public VisibleOnScreenNotifier2D VisibleOnScreenNotifier { get; set; }
	[Export] public AnimationPlayer AnimationPlayer { get; set; } // Renamed export
	[Export] public PlayerSpecialHurtBox PlayerSpecialHurtBox { get; set; }
	[Export] public HitBox HitBox { get; set; } // Assumed to be an Area2D child for detecting hits
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
			GD.PrintErr($"{Name} ({GetPath()}): Missing required exported nodes. Laser may not function correctly.");
			QueueFree();
			return;
		}

		this.SetVisibilityZOrdering(VisibilityZOrdering.Ammo); // Set parent Area2D ZIndex

		// --- *** ADDED/MODIFIED COLLISION SETUP FOR LASER'S HITBOX *** ---
		if (HitBox != null) // Check HitBox validity
		{
			HitBox.ResetCollisionLayerAndMask(); // Start clean

			// Set the LAYER the laser's hitbox IS ON (e.g., PlayerHitBox)
			// This layer is what enemy HurtComponents will need in their MASK to detect the laser.
			HitBox.ActivateCollisionLayer(CollisionLayers.PlayerHitBox); // Or PlayerAmmo if you add it

			// Set the MASK for the laser's hitbox (What layers does IT detect?)
			// This determines when the laser's OnHitBoxAreaEntered signal fires.
			HitBox.ActivateCollisionMask(CollisionLayers.WordEnemyHurtBox);   // DETECT WORD BLOCKS!
			HitBox.ActivateCollisionMask(CollisionLayers.MeteorEnemyHurtBox); // DETECT METEORS!
			HitBox.ActivateCollisionMask(CollisionLayers.RegularEnemyHurtBox); // DETECT REGULAR ENEMIES!
																			   // Add any other layers the laser should collide with and explode upon hitting.
		}
		else
		{
			GD.PrintErr($"{Name} ({GetPath()}): Cannot setup collisions, HitBox is null!");
			QueueFree(); // Cannot function without HitBox
			return;
		}
		// --- *** END OF COLLISION SETUP *** ---

		// --- Connect Signals ---
		VisibleOnScreenNotifier.ScreenExited += OnScreenExited;
		AnimationPlayer.AnimationFinished += OnAnimationFinished; // Use renamed export
		HitBox.AreaEntered += OnHitBoxAreaEntered; // Connect hit detection
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
			AnimationPlayer.AnimationFinished -= OnAnimationFinished; // Use renamed export
		}

		if (IsInstanceValid(HitBox))
		{
			HitBox.AreaEntered -= OnHitBoxAreaEntered;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_hitOccurred)
		{
			return;
		}

		if (_distanceTraveled >= LaserRange) { QueueFree(); return; }

		float moveAmount = Speed * (float)delta;
		Vector2 moveDirection = Vector2.Up.Rotated(Rotation);
		Vector2 moveVector = moveDirection * moveAmount;
		Position += moveVector;
		_distanceTraveled += moveAmount;
	}

	// --- Signal Handlers ---
	private void OnHitBoxAreaEntered(Area2D area)
	{
		if (_hitOccurred || !IsInstanceValid(this))
		{
			return;
		}

		// We already filtered what the HitBox *can* detect using its Collision Mask in _Ready.
		// Therefore, any 'area' entering here should trigger the explosion.
		// Optional: Add extra checks here if needed (e.g., ensure area isn't another laser)
		// if (area.CollisionLayer == (uint)CollisionLayers.PlayerHitBox) return; // Don't hit other player lasers

		GD.Print($"{Name} HitBox entered by: {area.Name} on layer {area.CollisionLayer}. Triggering HandleHit."); // Debug print
		HandleHit(); // Trigger hit logic
	}

	private void OnScreenExited()
	{
		if (!IsInstanceValid(this))
		{
			return;
		}

		CallDeferred(MethodName.QueueFree);
	}

	private void OnAnimationFinished(StringName animationName)
	{
		if (!IsInstanceValid(this))
		{
			return;
		}

		if (animationName == WeaponAnimations.LaserOnHit)
		{
			QueueFree();
		}
	}

	// --- Private Methods ---
	private bool ValidateExports()
	{
		bool isValid = true;
		void CheckNode(Node node, string name) { if (node == null) { GD.PrintErr($"{Name} ({GetPath()}): Exported node '{name}' is null!"); isValid = false; } }

		CheckNode(Sprite2D, nameof(Sprite2D)); // Check sprite even if unused by script, needed visually
		CheckNode(VisibleOnScreenNotifier, nameof(VisibleOnScreenNotifier));
		CheckNode(AnimationPlayer, nameof(AnimationPlayer)); // Use renamed export
		CheckNode(HitBox, nameof(HitBox));
		// Ignore unused exports PlayerSpecialHurtBox, CooldownSecs for validation
		return isValid;
	}

	private void HandleHit()
	{
		if (_hitOccurred)
		{
			return; // Ensure only called once
		}

		_hitOccurred = true;
		Speed = 0;
		SetPhysicsProcess(false);

		// Play hit animation safely
		if (IsInstanceValid(AnimationPlayer))
		{
			if (AnimationPlayer.HasAnimation(WeaponAnimations.LaserOnHit))
			{
				AnimationPlayer.Play(WeaponAnimations.LaserOnHit);
			}
			else
			{
				GD.PrintErr($"{Name} ({GetPath()}): Animation '{WeaponAnimations.LaserOnHit}' not found! Freeing immediately.");
				QueueFree(); // Free if animation missing
			}
		}
		else
		{
			GD.PrintErr($"{Name} ({GetPath()}): No AnimationPlayer found in HandleHit. Queuing free immediately.");
			QueueFree(); // Free if no player
		}

		// Disable hitbox safely using deferred calls
		if (IsInstanceValid(HitBox))
		{
			HitBox.SetDeferred(Area2D.PropertyName.Monitoring, false);
			HitBox.SetDeferred(Area2D.PropertyName.Monitorable, false);
		}
	}
}