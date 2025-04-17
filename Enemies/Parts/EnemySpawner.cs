using AlfaEBetto.Components;
using Godot;

namespace AlfaEBetto.Enemies.Parts;

public sealed partial class EnemySpawner : Area2D
{
	// --- Exports ---
	[Export] public EnemySpawnerControllerComponent SpawnerController { get; set; }
	[Export] public Marker2D Muzzle { get; set; } // Position where enemy should appear?
	[Export] public AnimationPlayer AnimationPlayer { get; set; }

	// --- Signals ---
	[Signal] public delegate void OnSpawnerPermissionChangeSignalEventHandler(bool isAllowed);
	[Signal] public delegate void OnSpawnEnemyReadySignalEventHandler(); // Emitted at the point in animation where enemy should be instanced
	[Signal] public delegate void OnSpawnProcessingFinishedSignalEventHandler(); // Emitted when spawn animation cycle completes

	public override void _Ready()
	{
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name}: Missing required exported nodes. Deactivating.");
			SetProcess(false);
			SetPhysicsProcess(false); // If it uses physics process
			return;
		}

		// Connect signals
		AnimationPlayer.AnimationFinished += OnAnimationPlayerFinished;
	}

	public override void _ExitTree()
	{
		// Disconnect signals when removed from the tree
		if (IsInstanceValid(AnimationPlayer))
		{
			AnimationPlayer.AnimationFinished -= OnAnimationPlayerFinished;
		}
	}

	// --- Public API Methods ---

	/// <summary>
	/// Signals that this spawner is allowed to start spawning.
	/// </summary>
	public void AllowSpawn() => EmitSignal(SignalName.OnSpawnerPermissionChangeSignal, true);

	/// <summary>
	/// Signals that this spawner is not allowed to start spawning.
	/// </summary>
	public void DisallowSpawn() => EmitSignal(SignalName.OnSpawnerPermissionChangeSignal, false);

	/// <summary>
	/// Starts the spawning animation sequence.
	/// </summary>
	public void StartSpawn() => AnimationPlayer?.Play(EnemyPartAnimations.SpawnEnemy);

	/// <summary>
	/// Intended to be called by an Animation Method Track at the exact frame
	/// where the enemy should be instantiated/placed. Emits the corresponding signal.
	/// </summary>
	public void OnSpawnEnemyAnimationPointReady() => EmitSignal(SignalName.OnSpawnEnemyReadySignal);

	// --- Private Methods ---

	/// <summary>
	/// Validates that essential exported nodes are assigned.
	/// </summary>
	private bool ValidateExports()
	{
		bool isValid = true;
		// SpawnerController might be optional depending on design? Add check if required.
		// if (SpawnerController == null) { GD.PrintErr($"{Name}: Missing SpawnerController!"); isValid = false; }
		if (Muzzle == null) { GD.PrintErr($"{Name}: Missing Muzzle (Marker2D)!"); isValid = false; }

		if (AnimationPlayer == null) { GD.PrintErr($"{Name}: Missing AnimationPlayer!"); isValid = false; }

		return isValid;
	}

	/// <summary>
	/// Handles the AnimationFinished signal from the AnimationPlayer.
	/// </summary>
	/// <param name="animationName">The name of the animation that finished.</param>
	private void OnAnimationPlayerFinished(StringName animationName)
	{
		// Check if the instance is still valid (good practice in signal handlers)
		if (!IsInstanceValid(this))
		{
			return;
		}

		// Check if the finished animation was the spawn animation
		if (animationName == EnemyPartAnimations.SpawnEnemy)
		{
			// Signal that the entire spawn animation process (including potential enemy appearance) is done.
			EmitSignal(SignalName.OnSpawnProcessingFinishedSignal);
		}
	}
}
