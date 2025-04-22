using System;
using System.Collections.Generic;
using System.Linq;
using AlfaEBetto.Components;
using AlfaEBetto.Enemies;
using AlfaEBetto.Enemies.Parts;
using Godot;
// Assuming EnemyBase and EnemySpawner exist in the namespaces above

namespace Alfaebeto.Components;

/// <summary>
/// Controls an associated EnemySpawner part, managing cooldown timers and
/// handling the instantiation of enemies via an EnemyBuilder when signaled.
/// Limits the maximum number of active enemies spawned by this component
/// and cleans them up when this component is freed.
/// </summary>
public sealed partial class EnemySpawnerControllerComponent : Node
{
	#region Exports
	[Export] public PackedScene EnemyPackedScene { get; set; }
	[Export] public Timer CooldownTimer { get; set; }

	/// <summary>
	/// The maximum number of enemies spawned by this component that can be active at once.
	/// Set to 0 or less for no limit.
	/// </summary>
	[Export(PropertyHint.Range, "0, 50, 1")] // Moved to top export group
	public int MaxSpawnedEnemies { get; set; } = 2; // Example limit

	[ExportGroup("Timing")]
	[Export] public float BaseCooldown { get; set; } = 5.0f;
	[Export] public float CooldownVariance { get; set; } = 1f;

	[ExportGroup("Spawn Properties")]
	[Export] public float SpawnSpeed { get; set; } = 100.0f;
	#endregion

	#region Private Fields
	private EnemyBuilder _enemyBuilder;
	private EnemySpawner _enemySpawner;
	private Node _cachedSceneRoot;
	private bool _isInitialized = false;

	// State Flags
	private bool _externalPermissionToShoot = true; // Tracks permission from parent spawner's signal
	private bool _allowToShoot = false; // Actual permission considering external AND max count
	private bool _disallowedByMaxCount = false; // Tracks if disallowed SPECIFICALLY by max count

	// Tracking
	private readonly List<EnemyBase> _spawnedEnemies = [];
	#endregion

	#region Godot Methods & Initialization
	public override void _Ready() => Initialize();

	private void Initialize()
	{
		if (_isInitialized)
		{
			return;
		}

		if (!ValidateExports())
		{
			GD.PrintErr($"{Name} ({GetPath()}): Initialization failed due to missing exports.");
			SetProcess(false); // Deactivate
			_isInitialized = false; // Mark as not initialized
			return;
		}

		// --- Get the parent node ---
		_enemySpawner = GetParent<EnemySpawner>();

		// *** ADD LOGGING HERE ***
		if (_enemySpawner != null)
		{
			// Log details about the identified parent
			GD.Print($"@@@ {Name} ({GetInstanceId()}): Initializing... Found Parent: '{_enemySpawner.Name}' (ID: {_enemySpawner.GetInstanceId()}, Path: '{_enemySpawner.GetPath()}') @@@");
		}
		else
		{
			// Log an error if the parent is NOT an EnemySpawner or is null
			Node potentialParent = GetParent();
			string parentInfo = (potentialParent != null)
				? $"'{potentialParent.Name}' (Type: {potentialParent.GetType().Name}, Path: '{potentialParent.GetPath()}')"
				: "null";
			GD.PrintErr($"@@@ {Name} ({GetInstanceId()}): Initialization failed! Parent is not EnemySpawner or is null. Found: {parentInfo} @@@");
			SetProcess(false); // Deactivate
			_isInitialized = false;
			return;
		}
		// *** END LOGGING SECTION ***

		// --- Continue with other initialization checks ---
		if (_enemySpawner.Muzzle == null)
		{
			GD.PrintErr($"{Name} ({GetPath()}): Initialization failed! Parent EnemySpawner '{_enemySpawner.Name}' is missing Muzzle node.");
			SetProcess(false);
			_isInitialized = false;
			return;
		}

		try
		{
			_enemyBuilder = new EnemyBuilder(EnemyPackedScene);
		}
		catch (Exception ex)
		{
			GD.PrintErr($"{Name} ({GetPath()}): Initialization failed! Error creating EnemyBuilder: {ex.Message}");
			SetProcess(false);
			_isInitialized = false;
			return;
		}

		_cachedSceneRoot = GetTree()?.CurrentScene;
		if (_cachedSceneRoot == null)
		{
			GD.PrintErr($"{Name} ({GetPath()}): Initialization failed! Cannot find current scene root.");
			SetProcess(false);
			_isInitialized = false;
			return;
		}

		// --- Finish initialization ---
		ConnectSignals();
		CooldownTimer.WaitTime = GetRandomCooldownTime();
		_isInitialized = true;
		GD.Print($"{Name}: Initialized. Max Enemies: {MaxSpawnedEnemies}"); // Existing log
		UpdateShootingPermission(); // Calculate initial permission state
	}

	private bool ValidateExports()
	{
		// ... (Validation for Scene, Timer remains the same) ...
		bool isValid = true;
		if (EnemyPackedScene == null) { GD.PrintErr($"{Name} ({GetPath()}): Missing {nameof(EnemyPackedScene)} export."); isValid = false; }

		if (CooldownTimer == null) { GD.PrintErr($"{Name} ({GetPath()}): Missing {nameof(CooldownTimer)} export."); isValid = false; }

		if (MaxSpawnedEnemies < 0)
		{
			GD.PushWarning($"{Name} ({GetPath()}): {nameof(MaxSpawnedEnemies)} is negative ({MaxSpawnedEnemies}). Treating as 0 (no limit).");
			MaxSpawnedEnemies = 0;
		}

		return isValid;
	}

	public override void _Notification(int what)
	{
		base._Notification(what); // Call base implementation first
		if (what == NotificationPredelete)
		{
			// Use PrintErr for high visibility in logs
			// Current cleanup logic:
			foreach (EnemyBase enemy in _spawnedEnemies.ToList())
			{
				if (IsInstanceValid(enemy)) { enemy.QueueFree(); } // Use QueueFree for standard cleanup
			}

			_spawnedEnemies.Clear();
		}
	}

	public override void _ExitTree()
	{
		// Use PrintErr for high visibility in logs
		DisconnectSignals();
		CooldownTimer?.Stop();
		// Make sure to call base if you override _ExitTree in a class deriving from Node
		base._ExitTree();
	}
	#endregion

	#region Signal Handling & State Updates

	private void ConnectSignals()
	{
		// ... (Connections FROM EnemySpawner remain the same) ...
		if (_enemySpawner != null)
		{
			if (_enemySpawner.HasSignal(EnemySpawner.SignalName.OnSpawnEnemyReadySignal))
			{
				_enemySpawner.OnSpawnEnemyReadySignal += SpawnProjectile;
			}
			else
			{
				GD.PrintErr($"{Name}: EnemySpawner missing signal {EnemySpawner.SignalName.OnSpawnEnemyReadySignal}");
			}

			if (_enemySpawner.HasSignal(EnemySpawner.SignalName.OnSpawnProcessingFinishedSignal))
			{
				_enemySpawner.OnSpawnProcessingFinishedSignal += OnReadyToRestartTimer;
			}
			else
			{
				GD.PrintErr($"{Name}: EnemySpawner missing signal {EnemySpawner.SignalName.OnSpawnProcessingFinishedSignal}");
			}

			if (_enemySpawner.HasSignal(EnemySpawner.SignalName.OnSpawnerPermissionChangeSignal))
			{
				_enemySpawner.OnSpawnerPermissionChangeSignal += OnExternalPermissionChange; // Renamed handler
			}
			else
			{
				GD.PrintErr($"{Name}: EnemySpawner missing signal {EnemySpawner.SignalName.OnSpawnerPermissionChangeSignal}");
			}
		}
		// --- Timer connection still goes to local handler ---
		if (CooldownTimer != null)
		{
			CooldownTimer.Timeout += HandleCooldownTimerTimeout;
		}
	}

	private void DisconnectSignals()
	{
		// ... (Disconnections FROM EnemySpawner remain the same, adjust handler name) ...
		if (IsInstanceValid(_enemySpawner))
		{
			if (_enemySpawner.HasSignal(EnemySpawner.SignalName.OnSpawnEnemyReadySignal))
			{
				_enemySpawner.OnSpawnEnemyReadySignal -= SpawnProjectile;
			}

			if (_enemySpawner.HasSignal(EnemySpawner.SignalName.OnSpawnProcessingFinishedSignal))
			{
				_enemySpawner.OnSpawnProcessingFinishedSignal -= OnReadyToRestartTimer;
			}

			if (_enemySpawner.HasSignal(EnemySpawner.SignalName.OnSpawnerPermissionChangeSignal))
			{
				_enemySpawner.OnSpawnerPermissionChangeSignal -= OnExternalPermissionChange; // Renamed handler
			}
		}

		if (IsInstanceValid(CooldownTimer))
		{
			CooldownTimer.Timeout -= HandleCooldownTimerTimeout;
		}
	}

	/// <summary>
	/// Handles permission changes signaled EXTERNALLY by the parent EnemySpawner.
	/// Stores this external permission and updates the overall shooting permission.
	/// </summary>
	private void OnExternalPermissionChange(bool isAllowedExternally)
	{
		if (!_isInitialized)
		{
			return;
		}

		_externalPermissionToShoot = isAllowedExternally;
		UpdateShootingPermission(); // Recalculate overall permission
	}

	/// <summary>
	/// Determines the actual _allowToShoot state based on external permission
	/// AND the max enemy count. Starts/stops the timer accordingly.
	/// </summary>
	private void UpdateShootingPermission()
	{
		bool canShootNow = _externalPermissionToShoot && !_disallowedByMaxCount;

		// FIX: Only proceed if the state needs changing OR the timer is invalid
		// if (canShootNow == _allowToShoot || IsInstanceValid(CooldownTimer)) // OLD INCORRECT
		if (canShootNow == _allowToShoot || !IsInstanceValid(CooldownTimer)) // CORRECTED CHECK
		{
			// No change in permission needed, or timer is invalid anyway.
			GD.Print($"{Name}: No change in permission or timer invalid. Current: {canShootNow}, Previous: {_allowToShoot}");
			return;
		}

		_allowToShoot = canShootNow; // Update the actual permission flag

		// Start or stop timer based on the NEW state
		if (IsInstanceValid(CooldownTimer)) // Extra safety check
		{
			if (_allowToShoot && CooldownTimer.IsStopped())
			{
				CooldownTimer.WaitTime = GetRandomCooldownTime();
				CooldownTimer.Start();
				GD.Print($"{Name}: Timer STARTED by UpdateShootingPermission."); // Log start
			}
			else if (!_allowToShoot && !CooldownTimer.IsStopped())
			{
				CooldownTimer.Stop();
				GD.Print($"{Name}: Timer STOPPED by UpdateShootingPermission."); // Log stop
			}
		}
	}

	/// <summary>
	/// Called when the parent EnemySpawner signals it has finished its spawn process.
	/// Restarts the cooldown timer *if* still allowed to shoot.
	/// </summary>
	private void OnReadyToRestartTimer()
	{
		if (!_isInitialized)
		{
			return;
		}
		// Only restart if overall permission is still true
		if (_allowToShoot)
		{
			CooldownTimer.WaitTime = GetRandomCooldownTime();
			CooldownTimer.Start();
		}
	}

	private double GetRandomCooldownTime() => Mathf.Max(0.1, GD.Randfn(BaseCooldown, CooldownVariance));

	/// <summary>
	/// Called when the CooldownTimer times out. Checks permission and max count
	/// before telling the EnemySpawner part to start its spawn process.
	/// </summary>
	private void HandleCooldownTimerTimeout()
	{
		if (!_isInitialized || !IsInstanceValid(_enemySpawner))
		{
			return;
		}

		// Clean list before check
		_spawnedEnemies.RemoveAll(enemy => !IsInstanceValid(enemy));

		// Check external permission first
		if (!_externalPermissionToShoot)
		{
			_allowToShoot = false; // Ensure internal state matches
								   // Timer should already be stopped by OnExternalPermissionChange, but check just in case
			if (!CooldownTimer.IsStopped())
			{
				CooldownTimer.Stop();
			}
			// GD.Print($"{Name}: Timer fired but external permission denied.");
			return;
		}

		// Check max enemy count
		if (MaxSpawnedEnemies > 0 && _spawnedEnemies.Count >= MaxSpawnedEnemies)
		{
			// GD.Print($"{Name}: Timer fired but max enemy limit ({MaxSpawnedEnemies}) reached.");
			_disallowedByMaxCount = true; // Mark reason for disallowing
			UpdateShootingPermission(); // Update state (will set _allowToShoot false & stop timer)
			return; // Do not proceed to spawn
		}

		// If we reach here, conditions are met
		_disallowedByMaxCount = false; // Ensure this is false if we are allowed to proceed
		_allowToShoot = true; // Ensure internal state matches

		// Tell the spawner part to start its process (animation etc.)
		// GD.Print($"{Name}: Timer fired, permission OK, limit OK. Calling StartSpawn().");
		_enemySpawner.StartSpawn(); // This will eventually trigger SpawnProjectile via signal
	}

	#endregion

	#region Spawning
	// SpawnProjectile remains mostly the same, but the MaxEnemyCheck is removed from here
	// as it's now handled before StartSpawn is even called.
	private void SpawnProjectile()
	{
		if (!_isInitialized || !IsInstanceValid(_cachedSceneRoot) || !IsInstanceValid(_enemySpawner) || !IsInstanceValid(_enemySpawner.Muzzle))
		{
			// Log error if needed
			return;
		}
		// MAX COUNT CHECK REMOVED FROM HERE

		Vector2 direction = _enemySpawner.GlobalPosition.DirectionTo(_enemySpawner.Muzzle.GlobalPosition);
		Vector2 velocity = direction.Normalized() * SpawnSpeed;
		Vector2 spawnPosition = _enemySpawner.Muzzle.GlobalPosition;

		EnemyBase enemy = _enemyBuilder.Create(spawnPosition, velocity);

		if (enemy == null)
		{
			GD.PrintErr($"{Name}: EnemyBuilder failed to create enemy instance.");
			return;
		}

		// Add to tracking list BEFORE connecting signals from ita
		_spawnedEnemies.Add(enemy);
		// Connect TreeExiting immediately after adding to list
		enemy.TreeExiting += () => OnSpawnedEnemyExiting(enemy);
		// GD.Print($"{Name}: Added {enemy.Name} to tracking. Count: {_spawnedEnemies.Count}");

		// Configure spawning state AFTER adding to list/connecting signal
		if (enemy.HasMethod("SetAsSpawning"))
		{
			enemy.Call("SetAsSpawning", true);
		}

		// Add to scene
		_cachedSceneRoot.CallDeferred(Node.MethodName.AddChild, enemy);

		// NEW: Check if limit is NOW reached after this spawn
		CheckMaxEnemyCountAndSetPermission();
	}
	#endregion

	#region Enemy Tracking
	// _spawnedEnemies list definition moved near top with other fields

	// OnNotificationPredelete remains the same

	/// <summary>
	/// Called when a spawned enemy's TreeExiting signal is emitted.
	/// Removes the enemy from the tracking list and potentially re-enables spawning.
	/// </summary>
	private void OnSpawnedEnemyExiting(EnemyBase enemy)
	{
		// This check is still crucial, especially during the forced cleanup above.
		if (!IsInstanceValid(this))
		{
			// This might now get logged when PrepareForCleanup force-frees enemies,
			// if the enemy's exit signal is deferred slightly after this controller is freed.
			GD.PrintErr($"{Name ?? "DisposedController"}: OnSpawnedEnemyExiting called but 'this' controller is invalid!");
			return;
		}

		if (enemy == null) { return; }

		// This Remove call might return false if the list was already cleared
		// in PrepareForCleanup, which is okay.
		bool removed = _spawnedEnemies.Remove(enemy);
		if (removed)
		{
			GD.Print($"{Name}: OnSpawnedEnemyExiting removed '{enemy.Name}'.");
		}
		else
		{
			GD.Print($"{Name}: OnSpawnedEnemyExiting called for '{enemy.Name}', but it was already removed/list cleared.");
		}

		// Check again before potentially calling UpdateShootingPermission
		if (!IsInstanceValid(this))
		{
			return;
		}

		// The rest of the logic might be less relevant now but safe with checks:
		if (_disallowedByMaxCount && MaxSpawnedEnemies > 0 && _spawnedEnemies.Count < MaxSpawnedEnemies)
		{
			if (!IsInstanceValid(this))
			{
				return;
			}

			_disallowedByMaxCount = false;

			if (!IsInstanceValid(this))
			{
				return;
			}

			UpdateShootingPermission();
		}
	}

	/// <summary>
	/// Checks if the max enemy count has been reached and updates permission state.
	/// </summary>
	private void CheckMaxEnemyCountAndSetPermission()
	{
		if (MaxSpawnedEnemies > 0 && _spawnedEnemies.Count >= MaxSpawnedEnemies)
		{
			if (!_disallowedByMaxCount) // Only update if not already disallowed by count
			{
				// GD.Print($"{Name}: Max enemy limit ({MaxSpawnedEnemies}) reached AFTER spawn.");
				_disallowedByMaxCount = true;
				UpdateShootingPermission(); // Update state (will set _allowToShoot false & stop timer)
			}
		}
	}

	#endregion

	#region Cleanup

	/// <summary>
	/// Call this method BEFORE the controller or its parent is freed.
	/// Stops the timer and disconnects signals from currently tracked enemies.
	/// </summary>
	/// <summary>
	/// Call this method BEFORE the controller or its parent is freed.
	/// Stops the timer and FORCES cleanup of currently tracked enemies.
	/// </summary>
	public void PrepareForCleanup()
	{
		if (!IsInstanceValid(this))
		{
			return; // Safety check
		}

		GD.Print($"{Name} ({GetInstanceId()}): PrepareForCleanup called.");

		// Stop any future spawning
		CooldownTimer?.Stop();
		_allowToShoot = false; // Prevent restarts or accidental triggers
		_externalPermissionToShoot = false; // Assume permission revoked

		// *** ADDED: Force despawn tracked enemies ***
		if (_spawnedEnemies != null)
		{
			GD.Print($"{Name}: Force despawning {_spawnedEnemies.Count} tracked enemies during PrepareForCleanup.");
			// Iterate on a copy (.ToList()) because QueueFree inside the loop
			// will trigger OnSpawnedEnemyExiting, which might modify the original list.
			foreach (EnemyBase enemy in _spawnedEnemies.ToList())
			{
				if (IsInstanceValid(enemy))
				{
					GD.Print($"{Name}: QueueFreeing tracked enemy '{enemy.Name}' ({enemy.GetInstanceId()}).");
					// Calling QueueFree here triggers the enemy's TreeExiting NOW,
					// while 'this' controller is still valid.
					enemy.QueueFree();
				}
			}
			// Clear the list AFTER iterating and queuing free.
			// OnSpawnedEnemyExiting will run for each enemy above, but the
			// IsInstanceValid(this) check inside it should keep it safe.
			// Clearing the list ensures it's empty before the controller itself is freed.
			_spawnedEnemies.Clear();
			GD.Print($"{Name}: Spawned enemies list cleared after QueueFree loop.");
		}
		// *** END ADDED SECTION ***

		// Ensure the node stops processing if it hasn't already
		if (this.HasMethod("SetProcess"))
		{
			SetProcess(false);
		}

		if (this.HasMethod("SetPhysicsProcess"))
		{
			SetPhysicsProcess(false);
		}
	}
	#endregion

}
