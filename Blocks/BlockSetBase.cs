using System.Collections.Generic;
using System.Linq;
using AlfaEBetto.Blocks;
using Godot;

namespace Alfaebeto.Blocks;

public abstract partial class BlockSetBase : Node2D
{
	#region Exports (Common or potentially needed by base)
	[Export] public PackedScene LetterBlockPackedScene { get; protected set; }
	// Derived classes will export their specific scenes (NoLetterBlock, ThreeArticle)
	#endregion

	#region Signals (Common definitions)
	[Signal] public delegate void ReadyToDequeueSignalEventHandler();
	[Signal] public delegate void OnLetterDestructedSignalEventHandler(bool isTarget);
	// Internal signal used to trigger actions on non-target blocks when target is hit
	[Signal] public delegate void OnDisableChildrenCollisionsInternalSignalEventHandler();
	#endregion

	#region Public Properties (Common state)
	public float CenterOffset { get; protected set; } = 0.0f;
	public int TargetIdx { get; protected set; } = -1; // Default to invalid index
	public Queue<LetterBlock> LetterBlocks { get; protected set; } = new(); // Use protected set
	public LetterBlock Target { get; protected set; } // Use protected set
	#endregion

	#region Protected Fields (For derived class use)
	protected LetterBlockBuilder _letterBuilder; // Must be initialized by derived class
	#endregion

	#region Private Fields
	private Timer _destructionTimer;
	private const float DestructionInterval = 0.25f;
	private bool _targetSignalConnected = false;
	#endregion

	#region Abstract Methods (Must be implemented by derived classes)

	/// <summary>
	/// Validates required exports and loads the specific data source
	/// (e.g., WordInfo, GuessBlockInfo) for the derived class.
	/// </summary>
	/// <returns>True if validation and data loading succeeded, false otherwise.</returns>
	protected abstract bool ValidateAndLoadData();

	/// <summary>
	/// Implements the specific logic loop for building and positioning blocks
	/// based on the loaded data source. Should call BuildSingleBlock internally.
	/// </summary>
	protected abstract void BuildAndPositionBlocksInternal();

	/// <summary>
	/// Creates, configures (label, color, target status), and adds a single block
	/// at the given X position. Should handle adding to AddChild and LetterBlocks queue.
	/// Should update Target and TargetIdx if applicable.
	/// Should call ConfigureBlockSignals.
	/// </summary>
	/// <param name="currentX">The starting X position for this block.</param>
	/// <param name="blockData">The specific data item for this block (e.g., char, string).</param>
	/// <param name="index">The index of this block in the sequence.</param>
	/// <returns>The calculated starting X position for the *next* block.</returns>
	protected abstract float BuildSingleBlock(float currentX, object blockData, int index);

	/// <summary>
	/// Connects necessary signals for a newly created LetterBlock.
	/// </summary>
	protected abstract void ConfigureBlockSignals(LetterBlock letterBlock, int index);

	/// <summary>
	/// Connects the appropriate signal from the determined Target block
	/// to the destruction sequence trigger (this.Destroy).
	/// </summary>
	protected abstract void ConnectTargetSignal(); // Target block might have different signal names

	#endregion

	#region Godot Methods
	public override void _Ready()
	{
		// 1. Validate and Load Data (Implemented by derived class)
		if (!ValidateAndLoadData())
		{
			GD.PrintErr($"{GetType().Name} '{Name}': Failed initial data validation or loading. Aborting setup.");
			// Optionally queue free, but returning prevents further setup
			return;
		}

		// 2. Setup Destruction Timer (Common)
		SetupDestructionTimer();

		// 3. Build and Position Blocks (Implemented by derived class)
		BuildAndPositionBlocksInternal(); // This calls BuildSingleBlock, ConfigureBlockSignals

		// 4. Check if Target was set by the build process
		if (Target == null)
		{
			// This might be valid (e.g., Word with HasMark=false) or an error.
			// Derived class logic should handle this possibility.
			GD.PrintRich($"[color=orange]{GetType().Name} '{Name}': Target block was not set after building.[/color]");
		}
		else
		{
			// 5. Connect Target Signal (Implemented by derived class)
			ConnectTargetSignal();
		}
	}

	public override void _ExitTree()
	{
		DisconnectSignals();
		// Clean up timer if it hasn't been freed automatically
		_destructionTimer?.QueueFree();
	}
	#endregion

	#region Public API (Common)
	/// <summary>
	/// Initiates the sequential destruction animation of the blocks.
	/// </summary>
	public virtual void Destroy() // Make virtual if derived classes need to override
	{
		// Prevent timer restart if already running, or handle edge cases if needed
		if (_destructionTimer.IsStopped() && LetterBlocks.Any())
		{
			if (LetterBlocks.TryDequeue(out LetterBlock firstBlock))
			{
				firstBlock.TriggerExplosion(); // Use the improved method name
				if (LetterBlocks.Any())
				{
					_destructionTimer.Start(DestructionInterval);
				}
			}
		}
		else if (!LetterBlocks.Any())
		{
			// This might happen if Destroy is called after all blocks are gone
			// GD.Print($"{GetType().Name} '{Name}': Destroy called but no letter blocks remain.");
			_destructionTimer?.Stop(); // Ensure timer is stopped
		}
	}
	#endregion

	#region Protected Helpers (Common logic usable by derived classes)

	/// <summary>
	/// Calculates the starting X position for the next block based on the current block's size.
	/// </summary>
	protected virtual float CalculateNextBlockStartPosition(LetterBlock letterBlock, float currentBlockX)
	{
		// Validate shape before accessing
		if (letterBlock?.CollisionShape?.Shape is not RectangleShape2D shape)
		{
			GD.PrintErr($"{GetType().Name} '{Name}': LetterBlock '{letterBlock?.Name ?? "Unnamed"}' lacks a valid RectangleShape2D CollisionShape for layout calculation.");
			return currentBlockX + 50; // Fallback spacing
		}

		// Using Size.X should be correct for default RectangleShape2D centered origin
		float blockWidth = shape.Size.X; // Shape size is half-extents, so X is half-width
		const float gap = 5.0f; // Configurable gap between blocks
		return currentBlockX + (blockWidth * 2) + gap; // Position + Full Width + Gap
	}

	/// <summary>
	/// Centers the entire set of blocks horizontally relative to this node's origin.
	/// </summary>
	protected virtual void CenterBlocksLayout(float totalWidth)
	{
		CenterOffset = totalWidth / 2.0f;
		// Adjust position based on the calculated center offset
		Position = new Vector2(-CenterOffset, Position.Y);
	}

	/// <summary>
	/// Handles the destruction signal from individual letter blocks.
	/// Emits necessary signals externally and internally.
	/// </summary>
	protected virtual void OnLetterBlockDestroyed(bool isTarget)
	{
		if (isTarget)
		{
			EmitSignal(SignalName.OnDisableChildrenCollisionsInternalSignal);
			// Optional: Stop destruction timer? Depends on desired game logic.
			// _destructionTimer?.Stop();
		}
		// Forward the signal externally
		EmitSignal(SignalName.OnLetterDestructedSignal, isTarget);
	}
	#endregion

	#region Private Helpers
	private void SetupDestructionTimer()
	{
		_destructionTimer = new Timer
		{
			Name = "DestructionTimer", // Good practice for debugging
			OneShot = false, // Keep firing until stopped or queue is empty
			WaitTime = DestructionInterval
		}; // Create instance
		AddChild(_destructionTimer); // Add to scene tree
		_destructionTimer.Timeout += OnDestructionTimerTimeout;
	}

	private void DisconnectSignals()
	{
		// Disconnect from Timer
		if (IsInstanceValid(_destructionTimer))
		{
			_destructionTimer.Timeout -= OnDestructionTimerTimeout;
		}

		// Disconnect from Target Block
		if (_targetSignalConnected && IsInstanceValid(Target))
		{
			// Derived class knows the specific signal name, ideally provide a way to unhook
			// For now, we assume the derived class handles specific unhook if needed,
			// or we rely on the Target node being freed.
			// Example (if target signal was always the same):
			// Target.OnTargetBlockCalledDestructionSignal -= Destroy;
		}

		_targetSignalConnected = false;

		// Signals connected *to* letter blocks don't need explicit disconnection here,
		// as the blocks will be freed. However, the connections *from* letter blocks
		// to *this* node (like OnLetterDestructedSignal) need careful handling if
		// *this* node might persist longer than the blocks (unlikely here).
	}

	private void OnDestructionTimerTimeout()
	{
		if (LetterBlocks.TryDequeue(out LetterBlock nextBlock))
		{
			nextBlock.TriggerExplosion();
			if (!LetterBlocks.Any()) // Stop timer if queue is now empty
			{
				_destructionTimer.Stop();
			}
		}
		else
		{
			// Queue empty, ensure timer stops
			_destructionTimer.Stop();
		}
	}
	#endregion
}