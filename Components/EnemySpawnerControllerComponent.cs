using System; // For Exception
using AlfaEBetto.Components;
using AlfaEBetto.Enemies;
using AlfaEBetto.Enemies.Parts;
using Godot;

namespace Alfaebeto.Components; // Corrected namespace

/// <summary>
/// Controls an associated EnemySpawner part, managing cooldown timers and
/// handling the instantiation of enemies via an EnemyBuilder when signaled.
/// </summary>
public sealed partial class EnemySpawnerControllerComponent : Node
{
	#region Exports
	[Export] public PackedScene EnemyPackedScene { get; set; }
	[Export] public Timer CooldownTimer { get; set; }

	[ExportGroup("Timing")]
	[Export] public float BaseCooldown { get; set; } = 1.5f;
	[Export] public float CooldownVariance { get; set; } = 0.2f; // Standard deviation for Randfn

	[ExportGroup("Spawn Properties")]
	[Export] public float SpawnSpeed { get; set; } = 100.0f;
	#endregion

	#region Private Fields
	private EnemyBuilder _enemyBuilder;
	private EnemySpawner _enemySpawner; // Reference to the controlled spawner part
	private Node _cachedSceneRoot; // Cache scene root for adding children
	private bool _allowToShoot = false;
	private bool _isInitialized = false;
	#endregion

	#region Godot Methods
	public override void _Ready() => Initialize();

	public override void _ExitTree()
	{
		DisconnectSignals();
		// Stop timer if node is removed prematurely
		CooldownTimer?.Stop();
	}
	#endregion

	#region Initialization and Validation
	private void Initialize()
	{
		if (_isInitialized)
		{
			return;
		}

		// 1. Validate Exports
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name}: Missing required exported nodes/scenes. Deactivating controller.");
			SetProcess(false); SetPhysicsProcess(false); // Deactivate
			_isInitialized = false;
			return;
		}

		// 2. Get Parent Spawner
		_enemySpawner = GetParent<EnemySpawner>();
		if (_enemySpawner == null)
		{
			GD.PrintErr($"{Name}: Parent node is not or does not inherit from EnemySpawner. Deactivating.");
			SetProcess(false); SetPhysicsProcess(false);
			_isInitialized = false;
			return;
		}

		if (_enemySpawner.Muzzle == null) // Also check if Muzzle exists on parent
		{
			GD.PrintErr($"{Name}: Parent EnemySpawner '{_enemySpawner.Name}' is missing required Muzzle node. Deactivating.");
			SetProcess(false); SetPhysicsProcess(false);
			_isInitialized = false;
			return;
		}

		// 3. Create Enemy Builder
		try
		{
			_enemyBuilder = new EnemyBuilder(EnemyPackedScene); // Uses constructor validation for PackedScene
		}
		catch (Exception ex)
		{
			GD.PrintErr($"{Name}: Failed to initialize EnemyBuilder. Error: {ex.Message}. Deactivating.");
			SetProcess(false); SetPhysicsProcess(false);
			_isInitialized = false;
			return;
		}

		// 4. Cache Scene Root
		// Consider if GetTree().CurrentScene is appropriate, or if enemies
		// should be added as children of _enemySpawner or another specific node.
		// Using CurrentScene for now as per original logic.
		_cachedSceneRoot = GetTree()?.CurrentScene;
		if (_cachedSceneRoot == null)
		{
			GD.PrintErr($"{Name}: Could not cache scene root. Deactivating.");
			SetProcess(false); SetPhysicsProcess(false);
			_isInitialized = false;
			return;
		}

		// 5. Connect Signals
		ConnectSignals();

		// 6. Initial Timer Setup (but don't start automatically)
		CooldownTimer.WaitTime = GetRandomCooldownTime();

		_isInitialized = true;
		GD.Print($"{Name}: Initialized successfully.");
		// Timer is started/stopped via OnSpawnerPermissionChange
	}

	private bool ValidateExports()
	{
		bool isValid = true;
		if (EnemyPackedScene == null) { GD.PrintErr($"{Name}: Missing {nameof(EnemyPackedScene)} export."); isValid = false; }

		if (CooldownTimer == null) { GD.PrintErr($"{Name}: Missing {nameof(CooldownTimer)} export."); isValid = false; }
		// Add checks for BaseCooldown, CooldownVariance > 0?
		return isValid;
	}
	#endregion

	#region Signal Handling
	private void ConnectSignals()
	{
		// Connect signals FROM parent EnemySpawner
		if (_enemySpawner != null)
		{
			// Ensure signals exist before connecting (optional but safer)
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
				_enemySpawner.OnSpawnProcessingFinishedSignal += OnReadyToRestartTimer; // Corrected typo
			}
			else
			{
				GD.PrintErr($"{Name}: EnemySpawner missing signal {EnemySpawner.SignalName.OnSpawnProcessingFinishedSignal}");
			}

			if (_enemySpawner.HasSignal(EnemySpawner.SignalName.OnSpawnerPermissionChangeSignal))
			{
				_enemySpawner.OnSpawnerPermissionChangeSignal += OnSpawnerPermissionChange;
			}
			else
			{
				GD.PrintErr($"{Name}: EnemySpawner missing signal {EnemySpawner.SignalName.OnSpawnerPermissionChangeSignal}");
			}
		}

		// Connect signals FROM Timer component
		if (CooldownTimer != null)
		{
			// Connect timer timeout to the PARENT spawner's StartSpawn method.
			// This assumes the parent EnemySpawner node has the visual logic (muzzle flash, etc.)
			// and will emit OnSpawnEnemyReadySignal when ready for projectile creation.
			CooldownTimer.Timeout += _enemySpawner.StartSpawn; // Keep original logic, ensure StartSpawn exists on EnemySpawner
		}
	}

	private void DisconnectSignals()
	{
		// Use IsInstanceValid before accessing potentially freed nodes
		if (IsInstanceValid(_enemySpawner))
		{
			// Check if signals exist before trying to disconnect (optional but avoids potential errors if signals changed)
			if (_enemySpawner.HasSignal(EnemySpawner.SignalName.OnSpawnEnemyReadySignal))
			{
				_enemySpawner.OnSpawnEnemyReadySignal -= SpawnProjectile;
			}

			if (_enemySpawner.HasSignal(EnemySpawner.SignalName.OnSpawnProcessingFinishedSignal))
			{
				_enemySpawner.OnSpawnProcessingFinishedSignal -= OnReadyToRestartTimer; // Corrected typo
			}

			if (_enemySpawner.HasSignal(EnemySpawner.SignalName.OnSpawnerPermissionChangeSignal))
			{
				_enemySpawner.OnSpawnerPermissionChangeSignal -= OnSpawnerPermissionChange;
			}
		}

		if (IsInstanceValid(CooldownTimer) && IsInstanceValid(_enemySpawner))
		{
			CooldownTimer.Timeout -= _enemySpawner.StartSpawn;
		}
	}

	/// <summary>
	/// Handles permission changes signaled by the parent EnemySpawner.
	/// Starts or stops the cooldown timer accordingly.
	/// </summary>
	private void OnSpawnerPermissionChange(bool isAllowedToShoot)
	{
		if (!_isInitialized)
		{
			return; // Don't respond if not ready
		}

		_allowToShoot = isAllowedToShoot;
		if (_allowToShoot && CooldownTimer.IsStopped()) // Start only if stopped
		{
			// Reset and start timer immediately when allowed
			CooldownTimer.WaitTime = GetRandomCooldownTime();
			CooldownTimer.Start();
			// GD.Print($"{Name}: CooldownTimer started.");
		}
		else if (!_allowToShoot && !CooldownTimer.IsStopped()) // Stop only if running
		{
			CooldownTimer.Stop();
			// GD.Print($"{Name}: CooldownTimer stopped.");
		}
	}

	/// <summary>
	/// Called when the parent EnemySpawner signals it has finished its spawn process.
	/// Resets and starts the cooldown timer if permission is still granted.
	/// </summary>
	private void OnReadyToRestartTimer() // Corrected typo
	{
		if (!_isInitialized)
		{
			return;
		}

		// Only restart if still allowed to shoot
		if (_allowToShoot)
		{
			CooldownTimer.WaitTime = GetRandomCooldownTime();
			CooldownTimer.Start();
		}
	}

	/// <summary>
	/// Calculates a random cooldown time using normal distribution.
	/// </summary>
	private double GetRandomCooldownTime() => Mathf.Max(0.1, GD.Randfn(BaseCooldown, CooldownVariance)); // Ensure minimum wait time
	#endregion

	#region Spawning
	/// <summary>
	/// Called when the parent EnemySpawner signals it's ready to spawn the projectile visually.
	/// Creates the enemy instance and adds it to the scene.
	/// </summary>
	private void SpawnProjectile()
	{
		if (!_isInitialized || !IsInstanceValid(_cachedSceneRoot) || !IsInstanceValid(_enemySpawner) || !IsInstanceValid(_enemySpawner.Muzzle))
		{
			GD.PrintErr($"{Name}: SpawnProjectile called but component/nodes are not ready or valid.");
			return;
		}

		// Calculate initial velocity based on spawner's orientation
		Vector2 direction = _enemySpawner.GlobalTransform.X; // Use spawner's forward direction (X-axis)
															 // Or if muzzle orientation matters:
															 // Vector2 direction = _enemySpawner.Muzzle.GlobalTransform.X;
															 // Or direction from spawner center to muzzle:
															 // Vector2 direction = _enemySpawner.GlobalPosition.DirectionTo(_enemySpawner.Muzzle.GlobalPosition);

		Vector2 velocity = direction.Normalized() * SpawnSpeed;
		Vector2 spawnPosition = _enemySpawner.Muzzle.GlobalPosition;

		// Create enemy instance using the builder
		EnemyBase enemy = _enemyBuilder.Create(spawnPosition, velocity); // Velocity passed but likely unused by improved builder

		// Check if creation succeeded before proceeding
		if (enemy == null)
		{
			GD.PrintErr($"{Name}: EnemyBuilder failed to create enemy instance.");
			// Optional: Maybe restart cooldown timer here? Depends on desired flow.
			// OnReadyToRestartTimer();
			return;
		}

		// Configure the spawned enemy (if needed beyond builder)
		// Check if method exists before calling
		if (enemy.HasMethod("SetAsSpawning"))
		{
			enemy.Call("SetAsSpawning", true); // Assuming SetAsSpawning takes a bool
		}
		else
		{
			// This might be okay if not all enemies have this method
			// GD.PushWarning($"{Name}: Spawned enemy {enemy.Name} does not have SetAsSpawning method.");
		}

		// Add to the scene using deferred call
		_cachedSceneRoot.CallDeferred(Node.MethodName.AddChild, enemy);

		// AddChildDefered(enemy); // If using extension method
	}
	#endregion
}