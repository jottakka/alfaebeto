using Alfaebeto.EnemyWeapons; // Corrected namespace
using AlfaEBetto;
using AlfaEBetto.Ammo;
using AlfaEBetto.PlayerNodes;
using Godot;

namespace Alfaebeto.Components; // Corrected namespace

/// <summary>
/// Controls a TurretBase node, handling aiming towards the player and initiating
/// the shooting sequence. It also spawns projectiles when signaled by the TurretBase.
/// </summary>
public sealed partial class TurretControllerComponent : Node
{
	#region Exports
	/// <summary>
	/// The AmmoComponent used to create projectile instances. Assign in Inspector.
	/// </summary>
	[Export] public AmmoComponent AmmoComponent { get; set; }

	/// <summary>
	/// The PackedScene for the ammo this turret fires. Assign in Inspector.
	/// (Passed to the AmmoComponent).
	/// </summary>
	[Export] public PackedScene AmmoPackedScene { get; set; }

	/// <summary>
	/// Allowed angle difference (radians) between muzzle and target for firing.
	/// </summary>
	[Export(PropertyHint.Range, "0, 3.14159, 0.01")]
	public float PointingTolerance { get; set; } = Mathf.Pi / 10.0f;

	// Removed MaxAroundAxisRotation as it wasn't used
	// [Export(PropertyHint.Range, "0, 3.14159, 0.01")]
	// public float MaxAroundAxisRotation { get; set; } = Mathf.Pi / 2.0f;
	#endregion

	#region Cached Nodes
	private TurretBase _turret; // Corrected typo: The parent turret node
	private Player _player;     // Reference to the player from Global
	private Node _sceneRoot;  // Reference to the current stage/scene root
	private bool _isInitialized = false;
	#endregion

	#region Godot Methods
	public override void _Ready() => Initialize();

	private void Initialize()
	{
		if (_isInitialized)
		{
			return;
		}

		// 1. Validate Exports
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name} ({GetPath()}): Missing required exported nodes. Deactivating.");
			SetProcess(false);
			_isInitialized = false;
			return;
		}

		// 2. Get and validate parent Turret
		_turret = GetParent<TurretBase>(); // Corrected Type
		if (!IsInstanceValid(_turret))
		{
			GD.PrintErr($"{Name} ({GetPath()}): Parent node is not a valid TurretBase. Deactivating.");
			SetProcess(false);
			_isInitialized = false;
			return;
		}

		if (!IsInstanceValid(_turret.Muzzle)) // Check Muzzle on the correct reference
		{
			GD.PrintErr($"{Name} ({GetPath()}): Parent TurretBase '{_turret.Name}' is missing its Muzzle node. Deactivating.");
			SetProcess(false);
			_isInitialized = false;
			return;
		}

		// 3. Get and validate global references
		if (!IsInstanceValid(Global.Instance))
		{
			GD.PrintErr($"{Name} ({GetPath()}): Global.Instance is not valid. Cannot get Player/Scene. Deactivating.");
			SetProcess(false);
			_isInitialized = false;
			return;
		}

		_player = Global.Instance.Player;
		_sceneRoot = GetTree()?.CurrentScene ?? Global.Instance.CurrentSceneNode; // Prefer current scene

		if (!IsInstanceValid(_player))
		{
			GD.PrintErr($"{Name} ({GetPath()}): Global.Instance.Player is not valid. Turret cannot target. Deactivating.");
			SetProcess(false);
			_isInitialized = false;
			return;
		}

		if (!IsInstanceValid(_sceneRoot))
		{
			GD.PrintErr($"{Name} ({GetPath()}): Could not find valid Scene Root. Cannot spawn ammo. Deactivating.");
			SetProcess(false);
			_isInitialized = false;
			return;
		}

		// 4. Setup AmmoComponent
		// This assumes AmmoComponent validates its own scene in its Ready method
		AmmoComponent.PackedScene = AmmoPackedScene;

		// 5. Connect signal
		// Use SignalName constant for safety
		_turret.ShootPointReachedSignal += SpawnProjectile;
		// GD.Print($"{Name}: Connected SpawnProjectile to _turret.ShootPointReachedSignal"); // Debug print

		_isInitialized = true;
	}

	public override void _ExitTree()
	{
		// Disconnect signal when removed from tree
		if (IsInstanceValid(_turret))
		{
			// Check if signal still connected before removing (optional but safer)
			if (_turret.IsConnected(TurretBase.SignalName.ShootPointReachedSignal, Callable.From(SpawnProjectile)))
			{
				_turret.ShootPointReachedSignal -= SpawnProjectile;
			}
		}
	}

	public override void _Process(double delta)
	{
		// Ensure player and turret are still valid each frame
		if (!IsInstanceValid(_player) || !IsInstanceValid(_turret))
		{
			// Consider disabling process if references become invalid?
			// SetProcess(false);
			return;
		}

		// Calculate direction and angle to player
		Vector2 direction = _player.GlobalPosition - _turret.GlobalPosition;
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
		float rotationSpeed = _turret.RotationSpeed > 0 ? _turret.RotationSpeed : 1.0f; // Default speed if invalid
		float interpolatedAngle = Mathf.LerpAngle(currentMuzzleAngle, targetAngle, rotationSpeed * (float)delta);

		// Apply rotation difference
		_turret.Rotate(interpolatedAngle - currentMuzzleAngle);

		// --- Aiming Check ---
		// Calculate the shortest angle difference between current muzzle angle and target angle
		float angleDifference = Mathf.AngleDifference(currentMuzzleAngle, targetAngle);

		// Check if the absolute difference is within the tolerance
		if (Mathf.Abs(angleDifference) <= PointingTolerance)
		{
			// If aimed correctly, tell the turret to shoot (turret handles cooldown etc.)
			_turret.Shoot();
		}

		// --- Optional: Clamp Rotation ---
		// If you want to limit the turret's total rotation relative to its parent or initial state,
		// you would need to store the initial rotation or use the parent's rotation
		// and clamp _turret.Rotation here. The MaxAroundAxisRotation export isn't used currently.
		// Example (simple clamp relative to 0):
		// _turret.Rotation = Mathf.Clamp(_turret.Rotation, -MaxAroundAxisRotation, MaxAroundAxisRotation);
	}

	/// <summary>
	/// Calculates the current global angle of the turret's muzzle.
	/// </summary>
	/// <returns>The global angle in radians, or 0 if muzzle is invalid.</returns>
	private float GetMuzzleAngle()
	{
		// Ensure turret and muzzle are valid
		if (!IsInstanceValid(_turret) || !IsInstanceValid(_turret.Muzzle))
		{
			// GD.PrintErr($"{Name}: Turret or Muzzle invalid in GetMuzzleAngle.");
			return 0f; // Return a default angle
		}
		// Calculate direction from turret origin to muzzle position
		Vector2 muzzleDirection = _turret.Muzzle.GlobalPosition - _turret.GlobalPosition;
		// Avoid Atan2(0,0) if muzzle is exactly at turret origin
		if (muzzleDirection.LengthSquared() < 0.001f)
		{
			return _turret.GlobalRotation; // Return turret's rotation if no offset
		}

		return muzzleDirection.Angle();
	}

	/// <summary>
	/// Spawns a projectile instance using the AmmoComponent.
	/// Called when the turret's ShootPointReachedSignal is emitted.
	/// </summary>
	public void SpawnProjectile() // Made public just in case, but primarily for signal
	{
		// --- DEBUG PRINT ---
		// GD.Print($"{Name}: SpawnProjectile() called!");

		// Ensure all required components/nodes are still valid
		if (!_isInitialized || !IsInstanceValid(_turret) || !IsInstanceValid(_turret.Muzzle) ||
			!IsInstanceValid(AmmoComponent) || !IsInstanceValid(_sceneRoot))
		{
			GD.PrintErr($"{Name}: Cannot spawn projectile, required nodes/components are invalid.");
			return;
		}

		// Get current angle and position at the moment the signal is received
		float currentAngle = GetMuzzleAngle();
		Vector2 spawnPosition = _turret.Muzzle.GlobalPosition;

		// Create ammo instance via AmmoComponent
		AmmoBase ammo = AmmoComponent.Create(currentAngle, spawnPosition);

		if (IsInstanceValid(ammo))
		{
			// --- DEBUG PRINT ---
			// GD.Print($"{Name}: Ammo instance created, adding to scene.");
			_sceneRoot.CallDeferred(Node.MethodName.AddChild, ammo);
			// _sceneRoot.AddChildDeferred(ammo); // If using your extension method
		}
		else
		{
			GD.PrintErr($"{Name}: AmmoComponent failed to create a valid ammo instance (returned null).");
		}
	}

	/// <summary>
	/// Validates that essential exported nodes are assigned.
	/// </summary>
	private bool ValidateExports()
	{
		bool isValid = true;
		if (AmmoComponent == null) { GD.PrintErr($"{Name} ({GetPath()}): Missing {nameof(AmmoComponent)} export!"); isValid = false; }

		if (AmmoPackedScene == null) { GD.PrintErr($"{Name} ({GetPath()}): Missing {nameof(AmmoPackedScene)} export!"); isValid = false; }

		return isValid;
	}
	#endregion
}
