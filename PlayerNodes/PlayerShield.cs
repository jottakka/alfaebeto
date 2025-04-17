using AlfaEBetto.CustomNodes;
using AlfaEBetto.Extensions;
using Godot;

namespace AlfaEBetto.PlayerNodes;

// Assuming these exist and are accessible:
// using YourProject.Global; // VisibilityZOrdering, CollisionLayers
// using YourProject.Actors; // Player
// using YourProject.Components; // HitBox
// using YourProject.Extensions; // SetVisibilityZOrdering, ActivateCollisionLayer, ActivateCollisionMask, ResetCollisionLayerAndMask

// Note: This inherits from CharacterBody2D, but doesn't seem to use MoveAndSlide.
// If it only needs to detect collisions and be positioned by the Player,
// Area2D might be a more suitable base class. Changing it might break things if
// external code relies on CharacterBody2D methods/properties, so kept as is per guidelines.
public sealed partial class PlayerShield : CharacterBody2D
{
	// --- Exports ---
	[Export] public AnimationPlayer AnimationPlayer { get; set; }
	[Export] public HitBox HitBox { get; set; } // Assumed to be an Area2D for detecting hits against the shield
	[Export] public int MaxShieldPoints { get; set; } = 60;
	[Export(PropertyHint.Range, "1, 100, 1")] // Damage shield takes per hit
	public int ShieldDamagePerHit { get; set; } = 10;

	// --- Public Properties ---
	[Export] public bool IsActive { get; private set; } = false; // Make setter private if only controlled internally
	public int CurrentShieldPoints { get; private set; }

	// --- Signals ---
	// Emitted when shield points change (value, increased/decreased)
	[Signal] public delegate void OnShieldPointsChangedSignalEventHandler(int currentValue, bool isIncrease);

	// --- Private Fields ---
	private Player _cachedPlayer; // Cache reference to parent player

	public override void _Ready()
	{
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name}: Missing required exported nodes. Shield may not function correctly.");
			// Consider disabling physics/processing if exports are missing
			SetPhysicsProcess(false);
			return;
		}

		// Cache parent Player reference
		_cachedPlayer = GetParent<Player>();
		if (!IsInstanceValid(_cachedPlayer))
		{
			GD.PrintErr($"{Name}: Parent node is not a valid Player. Shield cannot function correctly.");
			SetPhysicsProcess(false);
			return;
		}

		CurrentShieldPoints = 0; // Start with 0 points until activated/points added
								 // MotionMode = MotionModeEnum.Floating; // Likely unnecessary if not using MoveAndSlide

		// Assuming SetVisibilityZOrdering extension method exists
		this.SetVisibilityZOrdering(VisibilityZOrdering.PlayerAndEnemies); // Or a dedicated Shield layer?

		Visible = false; // Start invisible
		DeactivateCollisions(); // Start with collisions off

		// --- Connect Signals ---
		AnimationPlayer.AnimationFinished += OnAnimationFinished;
		// Connect both AreaEntered and BodyEntered from the HitBox to trigger the same logic
		HitBox.AreaEntered += OnHitBoxCollisionDetected;
		HitBox.BodyEntered += OnHitBoxCollisionDetected; // Assuming BodyEntered takes Node body param
	}

	public override void _ExitTree()
	{
		// Disconnect signals when removed from the tree
		if (IsInstanceValid(AnimationPlayer))
		{
			AnimationPlayer.AnimationFinished -= OnAnimationFinished;
		}

		if (IsInstanceValid(HitBox))
		{
			HitBox.AreaEntered -= OnHitBoxCollisionDetected;
			HitBox.BodyEntered -= OnHitBoxCollisionDetected;
		}
		// No signals connected to _cachedPlayer directly here, Player connects to this shield's signal
	}

	// --- Public Methods ---

	/// <summary>
	/// Starts the shield deactivation sequence (animation).
	/// Actual deactivation happens when animation finishes.
	/// </summary>
	public void Deactivate()
	{
		// Don't try to deactivate if already inactive or animation player invalid
		if (!IsActive || !IsInstanceValid(AnimationPlayer))
		{
			return;
		}

		// Setting IsActive = false happens in OnShieldDownFinished
		AnimationPlayer.Play(PlayerAnimations.OnPlayerShieldDown);
	}

	/// <summary>
	/// Starts the shield activation sequence (animation).
	/// Actual activation happens when animation finishes.
	/// </summary>
	public void Activate()
	{
		// Don't try to activate if already active or animation player invalid
		if (IsActive || !IsInstanceValid(AnimationPlayer))
		{
			return;
		}

		Visible = true;
		// Collisions enabled in OnShieldUpFinished after animation completes
		AnimationPlayer.Play(PlayerAnimations.OnPlayerShieldUp);
	}

	/// <summary>
	/// Adds points to the shield, activating it if necessary.
	/// </summary>
	/// <param name="points">Amount of points to add (positive).</param>
	public void AddShieldPoints(int points)
	{
		if (points <= 0)
		{
			return; // Only add positive points
		}

		int previousPoints = CurrentShieldPoints;

		// Activate shield visually first if it's currently down and gaining points
		if (!IsActive && CurrentShieldPoints <= 0)
		{
			Activate(); // Start activation animation
		}

		CurrentShieldPoints += points;
		CurrentShieldPoints = Mathf.Clamp(CurrentShieldPoints, 0, MaxShieldPoints);

		// Emit signal only if points actually changed
		if (CurrentShieldPoints != previousPoints)
		{
			EmitSignal(nameof(OnShieldPointsChangedSignalEventHandler), CurrentShieldPoints, true);
		}
	}

	/// <summary>
	/// Removes points from the shield, deactivating it if points reach zero.
	/// </summary>
	/// <param name="points">Amount of points to remove (positive).</param>
	public void RemoveShieldPoints(int points)
	{
		// Cannot remove points if inactive or points <= 0
		if (!IsActive || points <= 0)
		{
			return;
		}

		int previousPoints = CurrentShieldPoints;
		CurrentShieldPoints -= points;
		CurrentShieldPoints = Mathf.Clamp(CurrentShieldPoints, 0, MaxShieldPoints);

		// Emit signal only if points actually changed
		if (CurrentShieldPoints != previousPoints)
		{
			EmitSignal(nameof(OnShieldPointsChangedSignalEventHandler), CurrentShieldPoints, false);
		}

		// Check for deactivation *after* emitting the signal
		if (CurrentShieldPoints <= 0)
		{
			Deactivate(); // Start deactivation animation
		}
	}

	// --- Private Methods ---

	/// <summary>
	/// Validates that essential exported nodes are assigned.
	/// </summary>
	private bool ValidateExports()
	{
		bool isValid = true;
		if (AnimationPlayer == null) { GD.PrintErr($"{Name}: Missing AnimationPlayer!"); isValid = false; }

		if (HitBox == null) { GD.PrintErr($"{Name}: Missing HitBox!"); isValid = false; }
		// Add checks for other exports if they become critical
		return isValid;
	}

	/// <summary>
	/// Generic handler for collision signals from the HitBox.
	/// </summary>
	private void OnHitBoxCollisionDetected(Node /*Area2D or Node2D*/ collider)
	{
		// Check validity of this node first
		if (!IsInstanceValid(this))
		{
			return;
		}

		// Ignore collisions if shield isn't fully active
		if (!IsActive)
		{
			return;
		}

		// Play hit animation (check validity)
		AnimationPlayer?.Play(PlayerAnimations.OnPlayerShieldHit);

		// The actual point removal now happens when the OnPlayerShieldHit animation finishes
		// This prevents losing multiple points from a single projectile that stays in the area.
	}

	/// <summary>
	/// Activates collision shapes for the shield and its hitbox.
	/// </summary>
	private void ActivateCollisions()
	{
		// Assuming extension methods exist and CollisionLayers enum exists
		this.ActivateCollisionLayer(CollisionLayers.PlayerShield); // What the shield IS
		this.ActivateCollisionLayer(CollisionLayers.PlayerShieldHurtBox); // What can BE HIT (shield itself)

		// What the shield body MASKs (what it bumps into - maybe nothing if player handles movement?)
		// this.ActivateCollisionMask(CollisionLayers.Enemy); // Example if shield should push enemies

		// Activate the hitbox component as well (check validity)
		HitBox?.ActivateCollisionsMasks(); // Hitbox determines what *it* detects
	}

	/// <summary>
	/// Deactivates collision shapes for the shield and its hitbox.
	/// </summary>
	private void DeactivateCollisions()
	{
		// Assuming extension methods exist
		this.ResetCollisionLayerAndMask(); // Reset shield body collision
		HitBox?.DeactivateCollisions(); // Deactivate hitbox component
	}

	/// <summary>
	/// Handles the AnimationFinished signal from the AnimationPlayer.
	/// Manages state changes after specific animations complete.
	/// </summary>
	private void OnAnimationFinished(StringName animationName)
	{
		// Check validity of this node first
		if (!IsInstanceValid(this))
		{
			return;
		}

		// Use string comparisons with constants
		string animNameStr = animationName.ToString();

		if (animNameStr == PlayerAnimations.OnPlayerShieldUp)
		{
			OnShieldUpFinished();
		}
		else if (animNameStr == PlayerAnimations.OnPlayerShieldDown)
		{
			OnShieldDownFinished();
		}
		else if (animNameStr == PlayerAnimations.OnPlayerShieldHit)
		{
			RemoveShieldPoints(ShieldDamagePerHit);
			if (IsActive)
			{
				AnimationPlayer?.Play(PlayerAnimations.RESET);
			}
		}
		else if (animNameStr == PlayerAnimations.RESET)
		{
			// Optionally do something
		}
	}

	/// <summary>
	/// Called when the shield up animation finishes. Activates shield state.
	/// </summary>
	private void OnShieldUpFinished()
	{
		IsActive = true;
		ActivateCollisions(); // Activate collisions only when fully up
		GD.Print($"{Name}: Shield Activated.");
		// AnimationPlayer?.Play(PlayerAnimations.RESET); // RESET might be played in OnAnimationFinished now
	}

	/// <summary>
	/// Called when the shield down animation finishes. Cleans up state.
	/// </summary>
	private void OnShieldDownFinished()
	{
		IsActive = false; // State is now inactive
		Visible = false; // Hide the shield
		DeactivateCollisions(); // Ensure collisions are off
		GD.Print($"{Name}: Shield Deactivated.");
	}
}
