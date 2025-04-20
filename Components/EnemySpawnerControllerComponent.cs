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
		// ... (Validation for Exports, Parent, Muzzle, Builder, SceneRoot remains the same) ...
		if (_isInitialized)
		{
			return;
		}

		if (!ValidateExports()) { /* Error Log + Deactivate */ _isInitialized = false; return; }

		_enemySpawner = GetParent<EnemySpawner>();
		if (_enemySpawner == null) { /* Error Log + Deactivate */ _isInitialized = false; return; }

		if (_enemySpawner.Muzzle == null) { /* Error Log + Deactivate */ _isInitialized = false; return; }

		try { _enemyBuilder = new EnemyBuilder(EnemyPackedScene); }
		catch (Exception ex) { /* Error Log + Deactivate */ _isInitialized = false; return; }

		_cachedSceneRoot = GetTree()?.CurrentScene;
		if (_cachedSceneRoot == null) { /* Error Log + Deactivate */ _isInitialized = false; return; }

		ConnectSignals();
		CooldownTimer.WaitTime = GetRandomCooldownTime();
		_isInitialized = true;
		GD.Print($"{Name}: Initialized. Max Enemies: {MaxSpawnedEnemies}");
		// Start disabled until permission is granted
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
		// ... (NotificationPredelete cleanup logic remains the same) ...
		if (what == NotificationPredelete)
		{
			// GD.Print($"{Name} ({GetPath()}): Cleaning up spawned enemies on Predelete.");
			foreach (EnemyBase enemy in _spawnedEnemies.ToList())
			{
				if (IsInstanceValid(enemy)) { enemy.ForceDespawn(); }
			}

			_spawnedEnemies.Clear();
		}
	}

	public override void _ExitTree()
	{
		DisconnectSignals();
		CooldownTimer?.Stop();
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
		// Calculate new permission state
		bool canShootNow = _externalPermissionToShoot && !_disallowedByMaxCount;

		// Check if state actually changed
		if (canShootNow == _allowToShoot)
		{
			return; // No change needed
		}

		_allowToShoot = canShootNow; // Update the actual permission flag

		// Start or stop timer based on the NEW state
		if (_allowToShoot && CooldownTimer.IsStopped())
		{
			CooldownTimer.WaitTime = GetRandomCooldownTime();
			CooldownTimer.Start();
			// GD.Print($"{Name}: Timer STARTED due to permission update.");
		}
		else if (!_allowToShoot && !CooldownTimer.IsStopped())
		{
			CooldownTimer.Stop();
			// GD.Print($"{Name}: Timer STOPPED due to permission update.");
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

		// Add to tracking list BEFORE connecting signals from it
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
		if (enemy == null)
		{
			return;
		}

		bool removed = _spawnedEnemies.Remove(enemy);
		// if (removed) GD.Print($"{Name}: Removed {enemy.Name} from tracking. Count: {_spawnedEnemies.Count}");

		// If spawning was disallowed *because* of the max count,
		// check if we are now below the limit and can re-allow shooting.
		if (_disallowedByMaxCount && MaxSpawnedEnemies > 0 && _spawnedEnemies.Count < MaxSpawnedEnemies)
		{
			// GD.Print($"{Name}: Enemy count dropped below limit. Re-evaluating permission.");
			_disallowedByMaxCount = false; // No longer disallowed by count
										   // Recalculate overall permission (respecting external permission)
			UpdateShootingPermission();
			// UpdateShootingPermission will restart the timer if _allowToShoot becomes true
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
}