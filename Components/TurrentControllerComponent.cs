using AlfaEBetto.Ammo;
using AlfaEBetto.EnemyWeapons;
using AlfaEBetto.Extensions;
using AlfaEBetto.PlayerNodes;
using Godot;

namespace AlfaEBetto.Components;

public sealed partial class TurrentControllerComponent : Node
{
	// --- Exports ---
	[Export] public AmmoComponent AmmoComponent { get; set; }
	[Export] public PackedScene AmmoPackedScene { get; set; }
	[Export(PropertyHint.Range, "0, 3.14159, 0.01")] // Allow Pi radians tolerance
	public float PointingTolerance { get; set; } = Mathf.Pi / 15.0f;
	[Export(PropertyHint.Range, "0, 3.14159, 0.01")] // Allow up to Pi radians rotation
	public float MaxAroundAxisRotation { get; set; } = Mathf.Pi / 2.0f; // Note: Wrap uses +/- Pi, maybe adjust this?

	// --- Cached Nodes ---
	private TurrentBase _turrent; // The parent turret node
	private Player _player;       // Reference to the player from Global
	private Node _scene;          // Reference to the current stage/scene from Global

	// --- Godot Methods ---

	public override void _Ready()
	{
		// Validate required exported nodes
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name}: Missing required exported nodes. Deactivating.");
			SetProcess(false); // Disable _Process if setup fails
			return;
		}

		// Get and validate parent Turret
		_turrent = GetParent<TurrentBase>();
		if (!IsInstanceValid(_turrent))
		{
			GD.PrintErr($"{Name}: Parent node is not a valid TurrentBase. Deactivating.");
			SetProcess(false);
			return;
		}
		// Also check if the Muzzle node exists within the turret
		if (!IsInstanceValid(_turrent.Muzzle))
		{
			GD.PrintErr($"{Name}: Parent TurrentBase '{_turrent.Name}' is missing its Muzzle node. Deactivating.");
			SetProcess(false);
			return;
		}

		// Get and validate global references
		if (!IsInstanceValid(Global.Instance))
		{
			GD.PrintErr($"{Name}: Global.Instance is not valid. Cannot get Player/Scene. Deactivating.");
			SetProcess(false);
			return;
		}

		_player = Global.Instance.Player;
		_scene = Global.Instance.Scene; // Assuming Scene property holds the Node to add ammo to

		if (!IsInstanceValid(_player))
		{
			GD.PrintErr($"{Name}: Global.Instance.Player is not valid. Turret cannot target. Deactivating.");
			SetProcess(false); // Cannot function without a player to target
			return;
		}

		if (!IsInstanceValid(_scene))
		{
			GD.PrintErr($"{Name}: Global.Instance.Scene is not valid. Cannot spawn ammo. Deactivating.");
			SetProcess(false); // Cannot function without a scene to spawn into
			return;
		}

		// Setup AmmoComponent
		AmmoComponent.PackedScene = AmmoPackedScene;

		// Connect signal using strongly-typed delegate access
		// Assumes TurrentBase defines: [Signal] public delegate void ShootPointReachedSignalEventHandler();
		_turrent.ShootPointReachedSignal += SpawnProjectile;
	}

	public override void _ExitTree()
	{
		// Disconnect signal when removed from tree
		if (IsInstanceValid(_turrent))
		{
			_turrent.ShootPointReachedSignal -= SpawnProjectile;
		}
	}

	public override void _Process(double delta)
	{
		// Ensure player and turret are still valid each frame
		if (!IsInstanceValid(_player) || !IsInstanceValid(_turrent))
		{
			// Consider disabling process if references become invalid?
			// SetProcess(false);
			return;
		}

		// Calculate direction and angle to player
		Vector2 direction = _player.GlobalPosition - _turrent.GlobalPosition;
		// Avoid Atan2(0,0) -> results in 0 angle, might be okay, but check if needed
		if (direction.LengthSquared() < 0.001f)
		{
			return; // Too close, skip rotation
		}

		float targetAngle = direction.Angle(); // Equivalent to Atan2(direction.Y, direction.X)

		// Get current muzzle angle
		float currentMuzzleAngle = GetMuzzleAngle();
		// If GetMuzzleAngle returns NaN or similar due to invalid Muzzle, handle it?
		// (Added checks in GetMuzzleAngle)

		// Calculate interpolated angle for smooth rotation
		// Ensure RotationSpeed is defined and positive in TurrentBase
		float rotationSpeed = _turrent.RotationSpeed > 0 ? _turrent.RotationSpeed : 1.0f; // Default speed if invalid
		float interpolatedAngle = Mathf.LerpAngle(currentMuzzleAngle, targetAngle, rotationSpeed * (float)delta);

		// Apply rotation difference
		_turrent.Rotate(interpolatedAngle - currentMuzzleAngle);

		// --- Aiming Check ---
		// Calculate the shortest angle difference between current muzzle angle and target angle
		float angleDifference = Mathf.AngleDifference(currentMuzzleAngle, targetAngle);

		// Check if the absolute difference is within the tolerance
		if (Mathf.Abs(angleDifference) <= PointingTolerance)
		{
			// If aimed correctly, tell the turret to shoot (turret handles cooldown etc.)
			_turrent.Shoot();
		}

		// --- Optional: Clamp Rotation ---
		// If you want to limit the turret's total rotation relative to its parent or initial state,
		// you would need to store the initial rotation or use the parent's rotation
		// and clamp _turrent.Rotation here. The MaxAroundAxisRotation export isn't used currently.
		// Example (simple clamp relative to 0):
		// _turrent.Rotation = Mathf.Clamp(_turrent.Rotation, -MaxAroundAxisRotation, MaxAroundAxisRotation);
	}

	/// <summary>
	/// Calculates the current global angle of the turret's muzzle.
	/// </summary>
	/// <returns>The global angle in radians, or 0 if muzzle is invalid.</returns>
	private float GetMuzzleAngle()
	{
		// Ensure turret and muzzle are valid
		if (!IsInstanceValid(_turrent) || !IsInstanceValid(_turrent.Muzzle))
		{
			// GD.PrintErr($"{Name}: Turret or Muzzle invalid in GetMuzzleAngle.");
			return 0f; // Return a default angle
		}
		// Calculate direction from turret origin to muzzle position
		Vector2 muzzleDirection = _turrent.Muzzle.GlobalPosition - _turrent.GlobalPosition;
		// Avoid Atan2(0,0) if muzzle is exactly at turret origin
		if (muzzleDirection.LengthSquared() < 0.001f)
		{
			return _turrent.GlobalRotation; // Return turret's rotation if no offset
		}

		return muzzleDirection.Angle();
	}

	/// <summary>
	/// Spawns a projectile instance using the AmmoComponent.
	/// Called when the turret's ShootPointReachedSignal is emitted.
	/// </summary>
	public void SpawnProjectile()
	{
		// Ensure all required components/nodes are still valid
		if (!IsInstanceValid(_turrent) || !IsInstanceValid(_turrent.Muzzle) ||
			!IsInstanceValid(AmmoComponent) || !IsInstanceValid(_scene))
		{
			GD.PrintErr($"{Name}: Cannot spawn projectile, required nodes/components are invalid.");
			return;
		}

		// Get current angle and position at the moment the signal is received
		float currentAngle = GetMuzzleAngle();
		Vector2 spawnPosition = _turrent.Muzzle.GlobalPosition;

		// Create ammo instance via AmmoComponent
		AmmoBase ammo = AmmoComponent.Create(currentAngle, spawnPosition);

		if (IsInstanceValid(ammo))
		{
			// Add to the main scene using the assumed extension method AddChildDeffered
			// If AddChildDeffered is not an extension, call it appropriately:
			// e.g., someNode.AddChildDeffered(ammo);
			_scene.AddChildDeffered(ammo); // Assuming extension method on Node
		}
		else
		{
			GD.PrintErr($"{Name}: AmmoComponent failed to create a valid ammo instance.");
		}
	}

	/// <summary>
	/// Validates that essential exported nodes are assigned.
	/// </summary>
	private bool ValidateExports()
	{
		bool isValid = true;
		if (AmmoComponent == null) { GD.PrintErr($"{Name}: Missing AmmoComponent!"); isValid = false; }

		if (AmmoPackedScene == null) { GD.PrintErr($"{Name}: Missing AmmoPackedScene!"); isValid = false; }
		// Add checks for other exports if they become critical
		return isValid;
	}
}
