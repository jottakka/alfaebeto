using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Timer = Godot.Timer; // Explicit alias

// Define supported languages (Add more as needed)
public enum SupportedLanguage
{
	English,
	German,
	Portuguese,
	Japanese
	// Add others as your game supports them
}

// Now expects an "OnQueueFreeSignal" signal from special enemies

public sealed partial class SceneEnemySpawnerComponent : Node
{
	[ExportGroup("General Configuration")]
	[Export] public bool IsDeactived { get; set; } = true;
	[Export] public SupportedLanguage CurrentLanguage { get; set; } = SupportedLanguage.English;

	[ExportGroup("Spawn Points & Timers")]
	[Export] public Marker2D SpecialSpawnerPosition { get; set; }
	[Export] public PathFollow2D SpawnFollowPath { get; set; }
	[Export] public Timer RegularEnemySpawnTimer { get; set; }
	[Export] public Timer WordMeteorSpawnTimer { get; set; }
	[Export] public Timer SpecialEnemySpawnTimer { get; set; }

	[ExportGroup("Difficulty - Timings (Seconds)")]
	[Export] public double MinRegularEnemySpawnInterval { get; set; } = 3.0;
	[Export] public double MaxRegularEnemySpawnInterval { get; set; } = 6.0;
	[Export] public double MinWordMeteorSpawnInterval { get; set; } = 8.0;
	[Export] public double MaxWordMeteorSpawnInterval { get; set; } = 15.0;
	[Export] public double MinSpecialEnemySpawnInterval { get; set; } = 10.0;
	[Export] public double MaxSpecialEnemySpawnInterval { get; set; } = 20.0;

	// --- Language Specific Enemy Definitions ---
	[ExportGroup("English Enemies")]
	[Export] public PackedScene EnglishSpecialEnemy { get; set; }
	[Export] public PackedScene EnglishWordMeteor { get; set; }
	[Export] public PackedScene[] EnglishRegularEnemies { get; set; } = Array.Empty<PackedScene>();

	[ExportGroup("German Enemies")]
	[Export] public PackedScene GermanSpecialEnemy { get; set; }
	[Export] public PackedScene GermanWordMeteor { get; set; }
	[Export] public PackedScene[] GermanRegularEnemies { get; set; } = Array.Empty<PackedScene>();

	[ExportGroup("Portuguese Enemies")]
	[Export] public PackedScene PortugueseSpecialEnemy { get; set; }
	[Export] public PackedScene PortugueseWordMeteor { get; set; }
	[Export] public PackedScene[] PortugueseRegularEnemies { get; set; } = Array.Empty<PackedScene>();

	[ExportGroup("Japanese Enemies")]
	[Export] public PackedScene JapaneseSpecialEnemy { get; set; }
	[Export] public PackedScene JapaneseWordMeteor { get; set; }
	[Export] public PackedScene[] JapaneseRegularEnemies { get; set; } = Array.Empty<PackedScene>();

	// --- General Ambient Enemies (Language Agnostic) ---
	[ExportGroup("General Ambient Enemies")]
	[Export] public PackedScene[] MeteorPackedScenes { get; set; } = Array.Empty<PackedScene>();

	// --- Signals (Keep original public API) ---
	[Signal]
	public delegate void OnSpawnNextRequestedSignalEventHandler();

	// --- Private Fields ---
	private States _currentState = States.NoSpecialEnemy;
	private StageBase _parent; // Cache parent reference

	// Active scenes based on configuration - determined in _Ready
	private PackedScene _activeSpecialEnemyScene;
	private PackedScene _activeWordMeteorScene;
	private PackedScene[] _activeRegularEnemyScenes;

	// --- Godot Methods ---
	public override void _Ready()
	{
		if (IsDeactived)
		{
			SetPhysicsProcess(false); SetProcess(false);
			GD.Print($"{Name}: Deactivated.");
			return;
		}

		if (!ValidateCoreExports())
		{
			DeactivateSpawner("Missing core nodes/timers.");
			return;
		}

		_parent = GetParent<StageBase>();
		if (_parent == null)
		{
			DeactivateSpawner("Parent node is not or does not inherit from StageBase!");
			return;
		}

		SpawnFollowPath.Loop = false;

		if (!ConfigureActiveScenesForLanguage())
		{
			DeactivateSpawner($"Scene configuration invalid for language {CurrentLanguage}.");
			return;
		}

		// Connect Signals
		RegularEnemySpawnTimer.Timeout += OnRegularEnemySpawnTimerTimeout;
		WordMeteorSpawnTimer.Timeout += OnWordMeteorTimerTimeout;
		SpecialEnemySpawnTimer.Timeout += OnSpecialEnemyTimerTimeout;

		// Start Initial Spawns and Timers
		SpawnConfiguredSpecialEnemy();

		StartTimerRandomized(RegularEnemySpawnTimer, MinRegularEnemySpawnInterval, MaxRegularEnemySpawnInterval);
		StartTimerRandomized(WordMeteorSpawnTimer, MinWordMeteorSpawnInterval, MaxWordMeteorSpawnInterval);
		// Special enemy timer starts via OnSpecialEnemyFreed

		GD.Print($"{Name}: Initialized. Lang: {CurrentLanguage}");
	}

	// --- Configuration & Validation ---

	private void DeactivateSpawner(string reason)
	{
		GD.PrintErr($"{Name}: Deactivated! Reason: {reason}");
		IsDeactived = true;
		SetPhysicsProcess(false);
		SetProcess(false);
		RegularEnemySpawnTimer?.Stop();
		WordMeteorSpawnTimer?.Stop();
		SpecialEnemySpawnTimer?.Stop();
	}

	private bool ValidateCoreExports()
	{
		bool isValid = true;
		if (SpecialSpawnerPosition == null) { GD.PrintErr($"{Name}: Missing SpecialSpawnerPosition!"); isValid = false; }
		if (SpawnFollowPath == null) { GD.PrintErr($"{Name}: Missing SpawnFollowPath!"); isValid = false; }
		if (RegularEnemySpawnTimer == null) { GD.PrintErr($"{Name}: Missing RegularEnemySpawnTimer!"); isValid = false; }
		if (WordMeteorSpawnTimer == null) { GD.PrintErr($"{Name}: Missing WordMeteorSpawnTimer!"); isValid = false; }
		if (SpecialEnemySpawnTimer == null) { GD.PrintErr($"{Name}: Missing SpecialEnemySpawnTimer!"); isValid = false; }
		return isValid;
	}

	private bool ConfigureActiveScenesForLanguage()
	{
		GD.Print($"Configuring for Language: {CurrentLanguage}");

		switch (CurrentLanguage)
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
				GD.PrintErr($"{Name}: Language '{CurrentLanguage}' not handled in configuration!");
				return false;
		}

		bool scenesValid = true;
		if (_activeSpecialEnemyScene == null) { GD.PrintErr($"{Name}: Missing Special Enemy scene for {CurrentLanguage}!"); scenesValid = false; }
		if (_activeWordMeteorScene == null) { GD.PrintErr($"{Name}: Missing Word Meteor scene for {CurrentLanguage}!"); scenesValid = false; }
		if (_activeRegularEnemyScenes == null || _activeRegularEnemyScenes.Length == 0) { GD.PushWarning($"{Name}: Regular Enemies array is null or empty for {CurrentLanguage}. No regular enemies will spawn."); }

		return scenesValid;
	}

	// --- Timer Control ---

	private void StartTimerRandomized(Timer timer, double minWait, double maxWait)
	{
		if (timer == null || IsDeactived) return;
		timer.WaitTime = GD.RandRange(minWait, maxWait);
		timer.Start();
	}

	// --- Spawning Methods ---

	private void SpawnRegularEnemy()
	{
		SpawnRandomFromPack(_activeRegularEnemyScenes, GetNewTopSpawnPathRandPosition());
	}

	private void SpawnAmbientMeteor()
	{
		SpawnRandomFromPack(MeteorPackedScenes, GetNewTopSpawnPathRandPosition());
	}


	private void SpawnConfiguredSpecialEnemy()
	{
		if (_parent == null || IsDeactived || _activeSpecialEnemyScene == null) return;

		SpecialEnemySpawnTimer.Stop();

		Node enemyNode = null;
		try
		{
			enemyNode = _activeSpecialEnemyScene.Instantiate();
		}
		catch (Exception ex)
		{
			GD.PrintErr($"{Name}: Failed to instantiate Special Enemy scene '{_activeSpecialEnemyScene.ResourcePath}'. Error: {ex.Message}");
			StartTimerRandomized(SpecialEnemySpawnTimer, MinSpecialEnemySpawnInterval, MaxSpecialEnemySpawnInterval);
			return;
		}

		// --- Signal Connection (Using specific OnQueueFreeSignal name) ---
		// IMPORTANT: Ensure ALL your special enemy scenes emit this EXACT signal name.
		string signalNameToConnect = "OnQueueFreeSignal"; // Using the specific signal name you confirmed

		if (enemyNode != null)
		{
			if (enemyNode.HasSignal(signalNameToConnect))
			{
				// Connect the specified signal to the OnSpecialEnemyFreed handler
				Error connectionError = enemyNode.Connect(signalNameToConnect, Callable.From(OnSpecialEnemyFreed));
				if (connectionError != Error.Ok)
				{
					GD.PrintErr($"{Name}: Failed to connect signal '{signalNameToConnect}' on node from '{_activeSpecialEnemyScene.ResourcePath}'. Error: {connectionError}");
				}
			}
			else
			{
				// Critical problem if the expected signal is missing
				GD.PrintErr($"{Name}: Spawned Special Enemy from '{_activeSpecialEnemyScene.ResourcePath}' does not have the expected signal '{signalNameToConnect}'. Spawner logic WILL break!");
				enemyNode.QueueFree();
				StartTimerRandomized(SpecialEnemySpawnTimer, MinSpecialEnemySpawnInterval, MaxSpecialEnemySpawnInterval);
				return; // Do not add broken enemy
			}
		}
		// --- End Signal Connection Section ---


		if (enemyNode != null)
		{
			if (enemyNode is Node2D enemyNode2D)
			{
				if (SpecialSpawnerPosition == null) { GD.PrintErr("SpecialSpawnerPosition is null!"); enemyNode.QueueFree(); return; }
				enemyNode2D.Position = SpecialSpawnerPosition.Position;
				_parent.CallDeferred(Node.MethodName.AddChild, enemyNode);
				_currentState = States.SpecialEnemyAlive;
			}
			else
			{
				GD.PushWarning($"Spawned special enemy from {_activeSpecialEnemyScene.ResourcePath} is not a Node2D, cannot set position.");
				_parent.CallDeferred(Node.MethodName.AddChild, enemyNode);
				_currentState = States.SpecialEnemyAlive;
			}
		}
	}

	private void SpawnConfiguredWordMeteor()
	{
		if (_parent == null || IsDeactived || _currentState == States.SpecialEnemyAlive || _activeWordMeteorScene == null)
		{
			return;
		}

		Node meteorNode = null;
		try
		{
			meteorNode = _activeWordMeteorScene.Instantiate();
		}
		catch (Exception ex)
		{
			GD.PrintErr($"{Name}: Failed to instantiate Word Meteor scene '{_activeWordMeteorScene.ResourcePath}'. Error: {ex.Message}");
			return;
		}

		if (meteorNode is Node2D meteorNode2D)
		{
			if (SpecialSpawnerPosition == null) { GD.PrintErr("SpecialSpawnerPosition is null!"); meteorNode.QueueFree(); return; }
			Vector2 randomHorizontalVariation = new Vector2((float)GD.RandRange(-75.0f, 75.0f), 0.0f);
			meteorNode2D.Position = SpecialSpawnerPosition.Position + randomHorizontalVariation;
			_parent.CallDeferred(Node.MethodName.AddChild, meteorNode);
		}
		else if (meteorNode != null)
		{
			GD.PushWarning($"Spawned word/guess meteor from {_activeWordMeteorScene.ResourcePath} is not a Node2D, cannot set position.");
			meteorNode.QueueFree();
		}
	}

	private void SpawnRandomFromPack(PackedScene[] packedScenes, Vector2 spawnPosition)
	{
		if (_parent == null || IsDeactived || packedScenes == null || packedScenes.Length == 0)
		{
			return;
		}

		int idx = GD.RandRange(0, packedScenes.Length - 1);
		PackedScene chosenScene = packedScenes[idx];

		if (chosenScene == null)
		{
			GD.PushWarning($"{Name}: Null PackedScene found in array at index {idx} for language {CurrentLanguage}.");
			return;
		}

		Node enemySceneInstantiated = null;
		try
		{
			enemySceneInstantiated = chosenScene.Instantiate();
		}
		catch (Exception ex)
		{
			GD.PrintErr($"{Name}: Failed to instantiate scene '{chosenScene.ResourcePath}'. Error: {ex.Message}");
			return;
		}

		if (enemySceneInstantiated is Node2D enemyNode2D)
		{
			enemyNode2D.Position = spawnPosition;
			_parent.CallDeferred(Node.MethodName.AddChild, enemyNode2D);
		}
		else if (enemySceneInstantiated != null)
		{
			GD.PushWarning($"Spawned node from {chosenScene.ResourcePath} is not Node2D. Adding without setting position.");
			_parent.CallDeferred(Node.MethodName.AddChild, enemySceneInstantiated);
		}
	}

	private Vector2 GetNewTopSpawnPathRandPosition()
	{
		if (SpawnFollowPath == null) return Vector2.Zero;
		SpawnFollowPath.ProgressRatio = GD.Randf();
		return SpawnFollowPath.GlobalPosition;
	}


	// --- Signal Handlers / Timer Callbacks ---

	// Renamed handler to match original convention when using OnQueueFreeSignal
	private void OnSpecialEnemyFreed() // Connected to "OnQueueFreeSignal"
	{
		if (IsDeactived) return;

		if (_currentState == States.SpecialEnemyAlive)
		{
			_currentState = States.NoSpecialEnemy;
			StartTimerRandomized(SpecialEnemySpawnTimer, MinSpecialEnemySpawnInterval, MaxSpecialEnemySpawnInterval);
		}
		else
		{
			GD.PushWarning($"{Name}: {nameof(OnSpecialEnemyFreed)} called when state was not SpecialEnemyAlive. Check signal logic.");
		}
	}

	private void OnRegularEnemySpawnTimerTimeout()
	{
		if (IsDeactived) return;
		SpawnRegularEnemy();
		StartTimerRandomized(RegularEnemySpawnTimer, MinRegularEnemySpawnInterval, MaxRegularEnemySpawnInterval);
	}

	private void OnWordMeteorTimerTimeout()
	{
		if (IsDeactived) return;
		SpawnConfiguredWordMeteor();
		StartTimerRandomized(WordMeteorSpawnTimer, MinWordMeteorSpawnInterval, MaxWordMeteorSpawnInterval);
	}

	private void OnSpecialEnemyTimerTimeout()
	{
		if (IsDeactived) return;
		if (_currentState == States.NoSpecialEnemy)
		{
			SpawnConfiguredSpecialEnemy();
			// Timer is restarted by OnSpecialEnemyFreed
		}
		else
		{
			GD.PushWarning($"{Name}: SpecialEnemyTimer fired while _currentState was unexpectedly '{_currentState}'. Check logic.");
		}
	}


	// --- Internal State ---
	private enum States
	{
		SpecialEnemyAlive,
		NoSpecialEnemy,
	}
}
