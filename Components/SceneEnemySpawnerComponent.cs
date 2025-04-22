using System;
using System.Collections.Generic;
using System.Linq;
// Assuming Global class is accessible
using AlfaEBetto.Stages;
using Godot;
using WordProcessing.Enums;
using Timer = Godot.Timer;  // Explicit alias
							// Assuming EnemyBase is accessible
							// Assuming StageBase is accessible

namespace Alfaebeto.Components; // Corrected namespace

/// <summary>
/// Manages spawning enemies based on timers and game state.
/// Limits major enemies (Special/WordMeteor) to one at a time.
/// Implements a cooldown for regular enemy spawns after any spawn.
/// Clears recent regular enemies if a major enemy spawns shortly after using QueueFree.
/// Adjusts regular spawn rate based on major enemy presence.
/// Cleans up spawned enemies on exit using QueueFree.
/// Spawns an initial Special Enemy immediately.
/// </summary>
public sealed partial class SceneEnemySpawnerComponent : Node
{
	#region Constants
	private const ulong REGULAR_SPAWN_COOLDOWN_MS = 1000; // 5 seconds
	private const ulong MAJOR_SPAWN_CLEAR_WINDOW_MS = 50; // 100 milliseconds
	#endregion

	#region Exports
	[ExportGroup("General Configuration")]
	[Export] public bool IsDeactived { get; set; } = false;

	[ExportGroup("Spawn Points & Timers")]
	[Export] public Marker2D SpecialSpawnerPosition { get; set; }
	[Export] public PathFollow2D SpawnFollowPath { get; set; }
	[Export] public Timer RegularEnemySpawnTimer { get; set; }
	[Export] public Timer WordMeteorSpawnTimer { get; set; }
	[Export] public Timer SpecialEnemySpawnTimer { get; set; }

	[ExportGroup("Difficulty - Regular Timings (Fast)")]
	[Export] public double MinRegularEnemySpawnInterval { get; set; } = 5.0;
	[Export] public double MaxRegularEnemySpawnInterval { get; set; } = 8.0;

	[ExportGroup("Difficulty - Regular Timings (Slow)")]
	[Export] public double SlowMinRegularInterval { get; set; } = 5.0;
	[Export] public double SlowMaxRegularInterval { get; set; } = 8.0;

	[ExportGroup("Difficulty - Other Timings")]
	[Export] public double MinWordMeteorSpawnInterval { get; set; } = 9.0;
	[Export] public double MaxWordMeteorSpawnInterval { get; set; } = 17.0;
	[Export] public double MinSpecialEnemySpawnInterval { get; set; } = 13.0;
	[Export] public double MaxSpecialEnemySpawnInterval { get; set; } = 21.0;

	// --- Enemy Scenes (grouped by language) ---
	[ExportGroup("Enemy Scenes - English")]
	[Export] public PackedScene EnglishSpecialEnemy { get; set; }
	[Export] public PackedScene EnglishWordMeteor { get; set; }
	[Export] public PackedScene[] EnglishRegularEnemies { get; set; } = [];

	[ExportGroup("Enemy Scenes - German")]
	[Export] public PackedScene GermanSpecialEnemy { get; set; }
	[Export] public PackedScene GermanWordMeteor { get; set; }
	[Export] public PackedScene[] GermanRegularEnemies { get; set; } = [];

	[ExportGroup("Enemy Scenes - Portuguese")]
	[Export] public PackedScene PortugueseSpecialEnemy { get; set; }
	[Export] public PackedScene PortugueseWordMeteor { get; set; }
	[Export] public PackedScene[] PortugueseRegularEnemies { get; set; } = [];

	[ExportGroup("Enemy Scenes - Japanese")]
	[Export] public PackedScene JapaneseSpecialEnemy { get; set; }
	[Export] public PackedScene JapaneseWordMeteor { get; set; }
	[Export] public PackedScene[] JapaneseRegularEnemies { get; set; } = [];

	[ExportGroup("Enemy Scenes - Ambient")]
	[Export] public PackedScene[] MeteorPackedScenes { get; set; } = [];
	#endregion

	#region Signals
	[Signal] public delegate void OnSpawnNextRequestedSignalEventHandler();
	#endregion

	#region Private Fields
	private SupportedLanguage _currentLanguage =>
		Global.Instance?.CurrentLanguage ?? SupportedLanguage.Japanese;
	private StageBase _parentStage;
	private Node _cachedSceneRoot;
	private PackedScene _activeSpecialEnemyScene;
	private PackedScene _activeWordMeteorScene;
	private PackedScene[] _activeRegularEnemyScenes;
	private Node _activeMajorEnemy = null;
	private readonly List<Node> _allSpawnedEnemies = [];
	private readonly Dictionary<Node, ulong> _spawnedEnemyTimestamps = []; // Tracks spawn time per enemy
	private ulong _lastSpawnTimeMs = 0; // Timestamp of the most recent spawn
	private bool _isInitialized = false;
	#endregion

	#region Godot Methods & Initialization
	public override void _Ready() => Initialize();

	private void Initialize()
	{
		if (_isInitialized)
		{
			return;
		}

		if (IsDeactived)
		{
			GD.Print($"{Name}: Deactivated via export.");
			return;
		}

		if (!ValidateCoreExports())
		{
			DeactivateSpawner("Missing core nodes/timers.");
			return;
		}

		_parentStage = GetParent<StageBase>();
		if (!IsInstanceValid(_parentStage))
		{
			DeactivateSpawner("Parent node is not StageBase.");
			return;
		}

		_cachedSceneRoot = GetTree()?.CurrentScene;
		if (_cachedSceneRoot == null)
		{
			DeactivateSpawner("Cannot find current scene root.");
			return;
		}

		if (SpawnFollowPath != null)
		{
			SpawnFollowPath.Loop = false;
		}

		if (!ConfigureActiveScenesForLanguage())
		{
			DeactivateSpawner($"Scene config invalid for lang {_currentLanguage}.");
			return;
		}

		ConnectSignals();

		// --- Initial Spawn & Timer Logic ---
		GD.Print($"{Name}: Spawning initial special enemy.");
		SpawnConfiguredSpecialEnemy(); // Sets _activeMajorEnemy & _lastSpawnTimeMs if successful

		bool initialMajorEnemyActive = IsInstanceValid(_activeMajorEnemy);
		GD.Print($"{Name}: Initial Major Enemy Active: {initialMajorEnemyActive}. Setting timers.");

		// Start all timers, regular timer interval depends on initial major spawn success
		RestartRegularTimerBasedOnMajorEnemy(); // Sets correct initial interval & starts
		StartTimerRandomized(WordMeteorSpawnTimer, MinWordMeteorSpawnInterval, MaxWordMeteorSpawnInterval);
		StartTimerRandomized(SpecialEnemySpawnTimer, MinSpecialEnemySpawnInterval, MaxSpecialEnemySpawnInterval);

		_isInitialized = true;
		GD.Print($"{Name}: Initialized. Lang: {_currentLanguage}");
	}

	public override void _Notification(int what)
	{
		if (what == NotificationPredelete)
		{
			CleanupAllSpawnedEnemies();
		}
	}

	public override void _ExitTree()
	{
		DisconnectSignals();
		RegularEnemySpawnTimer?.Stop();
		WordMeteorSpawnTimer?.Stop();
		SpecialEnemySpawnTimer?.Stop();
		// CleanupAllSpawnedEnemies is called by NotificationPredelete
	}

	private bool ValidateCoreExports()
	{
		bool isValid = true;
		void CheckNode(GodotObject node, string name)
		{
			if (node == null)
			{
				GD.PrintErr($"{Name} ({GetPath()}): Missing export '{name}'!");
				isValid = false;
			}
		}

		CheckNode(SpecialSpawnerPosition, nameof(SpecialSpawnerPosition));
		CheckNode(SpawnFollowPath, nameof(SpawnFollowPath));
		CheckNode(RegularEnemySpawnTimer, nameof(RegularEnemySpawnTimer));
		CheckNode(WordMeteorSpawnTimer, nameof(WordMeteorSpawnTimer));
		CheckNode(SpecialEnemySpawnTimer, nameof(SpecialEnemySpawnTimer));
		return isValid;
	}

	private bool ConfigureActiveScenesForLanguage()
	{
		GD.Print($"Configuring for Language: {_currentLanguage}");
		switch (_currentLanguage)
		{
			case SupportedLanguage.English:
				_activeSpecialEnemyScene = EnglishSpecialEnemy;
				_activeWordMeteorScene = EnglishWordMeteor;
				_activeRegularEnemyScenes = EnglishRegularEnemies;
				break;
			case SupportedLanguage.German:
				_activeSpecialEnemyScene = GermanSpecialEnemy;
				_activeWordMeteorScene = GermanWordMeteor;
				_activeRegularEnemyScenes = GermanRegularEnemies;
				break;
			case SupportedLanguage.Portuguese:
				_activeSpecialEnemyScene = PortugueseSpecialEnemy;
				_activeWordMeteorScene = PortugueseWordMeteor;
				_activeRegularEnemyScenes = PortugueseRegularEnemies;
				break;
			case SupportedLanguage.Japanese:
				_activeSpecialEnemyScene = JapaneseSpecialEnemy;
				_activeWordMeteorScene = JapaneseWordMeteor;
				_activeRegularEnemyScenes = JapaneseRegularEnemies;
				break;
			default:
				GD.PrintErr($"{Name}: Language '{_currentLanguage}' not handled!");
				return false;
		}

		bool scenesValid = true;
		string langStr = _currentLanguage.ToString();
		if (_activeSpecialEnemyScene == null)
		{
			GD.PrintErr($"{Name}: Missing Special Enemy scene for {langStr}!");
			scenesValid = false;
		}

		if (_activeWordMeteorScene == null)
		{
			GD.PrintErr($"{Name}: Missing Word Meteor scene for {langStr}!");
			scenesValid = false;
		}

		if (_activeRegularEnemyScenes == null || !_activeRegularEnemyScenes.Any())
		{
			GD.PushWarning($"{Name}: Regular Enemies array null/empty for {langStr}.");
		}

		if (MeteorPackedScenes == null || !MeteorPackedScenes.Any())
		{
			GD.PushWarning($"{Name}: Ambient Meteor array null/empty.");
		}

		return scenesValid;
	}

	private void DeactivateSpawner(string reason)
	{
		GD.PrintErr($"{Name} ({GetPath()}): Deactivated! Reason: {reason}");
		IsDeactived = true;
		SetProcess(false);
		SetPhysicsProcess(false);
		RegularEnemySpawnTimer?.Stop();
		WordMeteorSpawnTimer?.Stop();
		SpecialEnemySpawnTimer?.Stop();
		DisconnectSignals();
	}
	#endregion

	#region Signal Handling & Timer Control
	private void ConnectSignals()
	{
		if (!IsInstanceValid(RegularEnemySpawnTimer) ||
			!IsInstanceValid(WordMeteorSpawnTimer) ||
			!IsInstanceValid(SpecialEnemySpawnTimer))
		{
			GD.PrintErr($"{Name}: One or more timers are null during ConnectSignals!");
			DeactivateSpawner("Timer instance became invalid before connection.");
			return;
		}

		RegularEnemySpawnTimer.Timeout += OnRegularEnemySpawnTimerTimeout;
		WordMeteorSpawnTimer.Timeout += OnWordMeteorTimerTimeout;
		SpecialEnemySpawnTimer.Timeout += OnSpecialEnemyTimerTimeout;
	}

	private void DisconnectSignals()
	{
		if (IsInstanceValid(RegularEnemySpawnTimer))
		{
			RegularEnemySpawnTimer.Timeout -= OnRegularEnemySpawnTimerTimeout;
		}

		if (IsInstanceValid(WordMeteorSpawnTimer))
		{
			WordMeteorSpawnTimer.Timeout -= OnWordMeteorTimerTimeout;
		}

		if (IsInstanceValid(SpecialEnemySpawnTimer))
		{
			SpecialEnemySpawnTimer.Timeout -= OnSpecialEnemyTimerTimeout;
		}
	}

	private void StartTimerRandomized(Timer timer, double minWait, double maxWait)
	{
		if (!IsInstanceValid(timer) || IsDeactived)
		{
			return;
		}

		timer.WaitTime = GD.RandRange(minWait, maxWait);
		timer.Start();
	}

	// --- Timer Timeout Handlers ---
	private void OnRegularEnemySpawnTimerTimeout()
	{
		if (IsDeactived || !IsInstanceValid(_parentStage))
		{
			return;
		}

		CheckAndClearInvalidMajorEnemyReference("Regular Timer"); // Keep failsafe check

		ulong currentTimeMs = Time.GetTicksMsec();
		if (currentTimeMs - _lastSpawnTimeMs < REGULAR_SPAWN_COOLDOWN_MS)
		{
			RestartRegularTimerBasedOnMajorEnemy();
			return;
		}

		SpawnRegularEnemy();
		RestartRegularTimerBasedOnMajorEnemy();
	}

	private void OnWordMeteorTimerTimeout()
	{
		if (IsDeactived || !IsInstanceValid(_parentStage))
		{
			return;
		}

		CheckAndClearInvalidMajorEnemyReference("Word Meteor Timer"); // Keep failsafe check

		if (!IsInstanceValid(_activeMajorEnemy))
		{
			SpawnConfiguredWordMeteor();
		}
		// Timer restarts handled by HandleEnemyExitCleanup or spawn failure
	}

	private void OnSpecialEnemyTimerTimeout()
	{
		if (IsDeactived || !IsInstanceValid(_parentStage))
		{
			return;
		}

		CheckAndClearInvalidMajorEnemyReference("Special Timer"); // Keep failsafe check

		if (!IsInstanceValid(_activeMajorEnemy))
		{
			SpawnConfiguredSpecialEnemy();
		}
		// Timer restarts handled by HandleEnemyExitCleanup or spawn failure
	}

	/// <summary>
	/// Failsafe check for invalid major enemy reference.
	/// </summary>
	private void CheckAndClearInvalidMajorEnemyReference(string callerContext)
	{
		if (_activeMajorEnemy != null)
		{
			if (!IsInstanceValid(_activeMajorEnemy))
			{
				// FIX: Log generically FIRST, then clear the reference.
				GD.Print($"{Name}: {callerContext} - Found invalid Active Major Enemy reference. Clearing.");
				_activeMajorEnemy = null; // Clear the invalid reference

				// Restart relevant timers now that the slot is confirmed free
				StartTimerRandomized(SpecialEnemySpawnTimer, MinSpecialEnemySpawnInterval, MaxSpecialEnemySpawnInterval);
				StartTimerRandomized(WordMeteorSpawnTimer, MinWordMeteorSpawnInterval, MaxWordMeteorSpawnInterval);
				RestartRegularTimerBasedOnMajorEnemy(); // Update regular timer speed
			}
		}
	}

	private void RestartRegularTimerBasedOnMajorEnemy()
	{
		bool majorEnemyActive = IsInstanceValid(_activeMajorEnemy); // This check is now safer
		double minInterval = majorEnemyActive ? SlowMinRegularInterval : MinRegularEnemySpawnInterval;
		double maxInterval = majorEnemyActive ? SlowMaxRegularInterval : MaxRegularEnemySpawnInterval;
		StartTimerRandomized(RegularEnemySpawnTimer, minInterval, maxInterval);
	}
	#endregion

	#region Spawning Methods
	private void SpawnRegularEnemy() =>
		SpawnRandomFromPack(_activeRegularEnemyScenes, GetNewTopSpawnPathRandPosition());

	private void SpawnAmbientMeteor() =>
		SpawnRandomFromPack(MeteorPackedScenes, GetNewTopSpawnPathRandPosition());

	private void SpawnConfiguredSpecialEnemy()
	{
		if (!CanSpawnMajorEnemy(_activeSpecialEnemyScene))
		{
			GD.Print($"{Name}: SpawnConfiguredSpecialEnemy PREVENTED.");
			return;
		}

		SpecialEnemySpawnTimer?.Stop();

		Node enemyNode = InstantiateAndConfigureEnemy(
			_activeSpecialEnemyScene,
			SpecialSpawnerPosition?.Position ?? Vector2.Zero
		);

		if (enemyNode != null)
		{
			_activeMajorEnemy = enemyNode;
			ulong spawnTimeMs = _spawnedEnemyTimestamps[enemyNode];
			ClearRecentRegularEnemies(enemyNode, spawnTimeMs);
			GD.Print($"{Name}: Special Spawned: {enemyNode.Name}.");
		}
		else
		{
			GD.PrintErr($"{Name}: Special Spawn Failed. Restarting timer.");
			StartTimerRandomized(
				SpecialEnemySpawnTimer,
				MinSpecialEnemySpawnInterval,
				MaxSpecialEnemySpawnInterval);
		}
	}

	private void SpawnConfiguredWordMeteor()
	{
		if (!CanSpawnMajorEnemy(_activeWordMeteorScene))
		{
			GD.Print($"{Name}: SpawnConfiguredWordMeteor PREVENTED.");
			return;
		}

		WordMeteorSpawnTimer?.Stop();

		Vector2 spawnPos = SpecialSpawnerPosition?.Position ?? Vector2.Zero;
		spawnPos += new Vector2((float)GD.RandRange(-75.0f, 75.0f), 0.0f);
		Node meteorNode = InstantiateAndConfigureEnemy(_activeWordMeteorScene, spawnPos);

		if (meteorNode != null)
		{
			_activeMajorEnemy = meteorNode;
			ulong spawnTimeMs = _spawnedEnemyTimestamps[meteorNode];
			ClearRecentRegularEnemies(meteorNode, spawnTimeMs);
			GD.Print($"{Name}: Word Meteor Spawned: {meteorNode.Name}.");
		}
		else
		{
			GD.PrintErr($"{Name}: Word Meteor Spawn Failed. Restarting timer.");
			StartTimerRandomized(
				WordMeteorSpawnTimer,
				MinWordMeteorSpawnInterval,
				MaxWordMeteorSpawnInterval);
		}
	}

	private bool CanSpawnMajorEnemy(PackedScene sceneToSpawn)
	{
		return IsInstanceValid(_parentStage)
			&& !IsDeactived
			&& sceneToSpawn != null
			&& !IsInstanceValid(_activeMajorEnemy);
	}

	private void SpawnRandomFromPack(PackedScene[] packedScenes, Vector2 spawnPosition)
	{
		if (!IsInstanceValid(_parentStage) || IsDeactived || packedScenes == null || packedScenes.Length == 0)
		{
			return;
		}

		int idx = GD.RandRange(0, packedScenes.Length - 1);
		PackedScene chosenScene = packedScenes[idx];
		if (chosenScene == null)
		{
			GD.PushWarning($"{Name}: Null PackedScene at index {idx}.");
			return;
		}

		InstantiateAndConfigureEnemy(chosenScene, spawnPosition);
	}

	private Node InstantiateAndConfigureEnemy(PackedScene scene, Vector2 spawnPos)
	{
		Node enemyNode = null;
		try
		{
			enemyNode = scene.Instantiate();
		}
		catch (Exception ex)
		{
			GD.PrintErr($"{Name}: Instantiate exception for " +
					   $"'{scene?.ResourcePath ?? "NULL"}': {ex.Message}");
			return null;
		}

		if (enemyNode == null)
		{
			// Handle instantiation failure
			return null;
		}

		if (enemyNode is Node2D enemyNode2D)
		{
			enemyNode2D.GlobalPosition = spawnPos;
		}
		else if (enemyNode != null)
		{
			GD.PushWarning($"Spawned node {scene.ResourcePath} is not Node2D. Position not set.");
		}
		else
		{
			GD.PrintErr($"{Name}: Instantiation failed for '{scene?.ResourcePath ?? "NULL"}'");
			return null;
		}

		ulong spawnTimeMs = Time.GetTicksMsec();
		_lastSpawnTimeMs = spawnTimeMs;
		_allSpawnedEnemies.Add(enemyNode);
		_spawnedEnemyTimestamps.Add(enemyNode, spawnTimeMs);

		// CRUCIAL: Ensure the lambda captures the correct node instance
		// This connection triggers the cleanup process.
		Node capturedEnemyNode = enemyNode;
		enemyNode.TreeExiting += () => OnSpawnedEnemyExiting(capturedEnemyNode); // This MUST call the correct OnSpawnedEnemyExiting

		_parentStage.CallDeferred(Node.MethodName.AddChild, enemyNode);
		return enemyNode;
	}

	private Vector2 GetNewTopSpawnPathRandPosition()
	{
		if (SpawnFollowPath == null)
		{
			return Vector2.Zero;
		}

		SpawnFollowPath.ProgressRatio = GD.Randf();
		return SpawnFollowPath.GlobalPosition;
	}
	#endregion

	#region Enemy Tracking & Cleanup

	/// <summary>
	/// Removes regular enemies that spawned within a very short window (100ms)
	/// *before* the specified major enemy was spawned by calling QueueFree.
	/// </summary>
	private void ClearRecentRegularEnemies(Node justSpawnedMajorEnemy, ulong majorSpawnTimeMs)
	{
		// Iterate through a copy of the keys to allow removal during iteration
		foreach (Node existingEnemy in _spawnedEnemyTimestamps.Keys.ToList())
		{
			if (existingEnemy == justSpawnedMajorEnemy || !IsInstanceValid(existingEnemy))
			{
				continue;
			}

			if (_spawnedEnemyTimestamps.TryGetValue(existingEnemy, out ulong existingSpawnTimeMs) &&
				existingSpawnTimeMs <= majorSpawnTimeMs &&
				majorSpawnTimeMs - existingSpawnTimeMs <= MAJOR_SPAWN_CLEAR_WINDOW_MS)
			{
				GD.Print($"{Name}: QueueFree-ing recently spawned enemy '{existingEnemy.Name}' " +
						$"due to major enemy '{justSpawnedMajorEnemy.Name}' spawn.");
				// Use QueueFree to trigger standard Godot cleanup, including _ExitTree
				existingEnemy.QueueFree();
				// Note: OnSpawnedEnemyExiting will handle list removal when the node actually exits.
			}
		}
	}
	/// <summary>
	/// Called DIRECTLY by the TreeExiting signal lambda.
	/// Gets the Instance ID and defers the actual cleanup.
	/// </summary>
	private void OnSpawnedEnemyExiting(Node enemy)
	{
		// Basic check if the component or the enemy is already invalid
		if (!IsInstanceValid(this) || !IsInstanceValid(enemy))
		{
			// GD.Print($"{Name}: OnSpawnedEnemyExiting skipped - Component or Enemy node invalid.");
			return;
		}

		ulong instanceId = enemy.GetInstanceId();
		// GD.Print($"{Name}: Enemy '{enemy.Name}' (ID: {instanceId}) TreeExiting. Deferring cleanup.");

		// MUST pass the ID wrapped in a Variant
		CallDeferred(nameof(HandleEnemyExitCleanup), Variant.From(instanceId));
	}

	/// <summary>
	/// Performs the actual cleanup, called deferred.
	/// MUST accept a Variant containing the Instance ID.
	/// </summary>
	private void HandleEnemyExitCleanup(Variant instanceIdVariant) // SIGNATURE MUST BE THIS
	{
		if (!IsInstanceValid(this))
		{
			return; // Spawner itself is gone
		}

		// --- Safely get Instance ID ---
		ulong instanceId = 0;
		try
		{
			// Explicit cast needed
			instanceId = (ulong)instanceIdVariant;
		}
		catch (InvalidCastException)
		{
			GD.PrintErr($"{Name}: HandleEnemyExitCleanup received invalid Variant type for Instance ID: {instanceIdVariant.VariantType}. Aborting cleanup for this call.");
			return;
		}
		catch (Exception ex) // Catch other potential issues
		{
			GD.PrintErr($"{Name}: HandleEnemyExitCleanup error converting Instance ID Variant: {ex.Message}. Aborting cleanup for this call.");
			return;
		}

		if (instanceId == 0)
		{
			GD.PrintErr($"{Name}: HandleEnemyExitCleanup received Instance ID 0. Aborting cleanup.");
			return;
		}

		// --- Check if it was the Major Enemy ---
		ulong activeMajorId = 0;
		// Safely check and get ID of current major enemy
		if (_activeMajorEnemy != null && IsInstanceValid(_activeMajorEnemy))
		{
			activeMajorId = _activeMajorEnemy.GetInstanceId();
		}

		if (activeMajorId != 0 && instanceId == activeMajorId)
		{
			GD.Print($"{Name}: Active Major Enemy (ID: {instanceId}) exited. Clearing reference and restarting major timers.");
			_activeMajorEnemy = null; // <<<< THE CRUCIAL CLEARING STEP

			// Restart timers now that the major slot is free
			StartTimerRandomized(SpecialEnemySpawnTimer, MinSpecialEnemySpawnInterval, MaxSpecialEnemySpawnInterval);
			StartTimerRandomized(WordMeteorSpawnTimer, MinWordMeteorSpawnInterval, MaxWordMeteorSpawnInterval);
			RestartRegularTimerBasedOnMajorEnemy(); // Update regular timer speed
		}

		// --- List/Dictionary Cleanup (Iterate based on ID) ---
		// Use ToList() to allow safe removal while iterating
		foreach (Node nodeInList in _allSpawnedEnemies.ToList())
		{
			// Check if the node reference in the list is still valid before trying to access it
			if (!IsInstanceValid(nodeInList))
			{
				// Optional: Clean up stale entries if found
				// GD.Print($"{Name}: Removing stale invalid node reference during cleanup iteration.");
				_spawnedEnemyTimestamps.Remove(nodeInList); // Attempt removal from dict too
				_allSpawnedEnemies.Remove(nodeInList);
				continue; // Move to the next node in the list
			}

			// If the node is valid, compare its instance ID
			if (nodeInList.GetInstanceId() == instanceId)
			{
				// Found the node corresponding to the exiting ID
				_spawnedEnemyTimestamps.Remove(nodeInList); // Remove from timestamp dict
				_allSpawnedEnemies.Remove(nodeInList);      // Remove from main list
															// GD.Print($"{Name}: Removed enemy (ID: {instanceId}) from tracking lists.");
				break; // Stop searching once found and removed
			}
		}

		// Note: We don't need GodotObject.InstanceFromId(instanceId) anymore unless
		// you specifically need to interact with the Node object during cleanup,
		// which is generally unsafe as it might be disposed. The ID comparison is sufficient.
	}

	/// <summary>
	/// Cleans up all currently tracked spawned enemies by calling QueueFree,
	/// which triggers their standard exit process (_ExitTree).
	/// </summary>
	private void CleanupAllSpawnedEnemies()
	{
		// GD.Print($"{Name} ({GetPath()}): QueueFree-ing all spawned enemies.");
		// Iterate on a copy because QueueFree leads to OnSpawnedEnemyExiting which modifies lists (deferred).
		foreach (Node enemyNode in _allSpawnedEnemies.ToList())
		{
			if (IsInstanceValid(enemyNode))
			{
				// Use QueueFree to trigger standard Godot cleanup, including _ExitTree
				enemyNode.QueueFree();
			}
		}
		// Clear lists immediately; OnSpawnedEnemyExiting will handle individual removals
		// but this ensures lists are cleared if spawner is removed abruptly.
		_allSpawnedEnemies.Clear();
		_spawnedEnemyTimestamps.Clear();
	}

	// Removed the ForceRemoveEnemy method as it's replaced by direct QueueFree calls.

	#endregion
}
