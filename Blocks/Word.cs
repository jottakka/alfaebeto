using System.Linq;
using AlfaEBetto.Blocks;
using AlfaEBetto.Data.Words;
using Godot;

namespace Alfaebeto.Blocks;

public partial class Word : BlockSetBase // Inherit from base
{
	#region Exports (Specific to Word)
	[Export] public PackedScene NoLetterBlockPackedScene { get; set; }
	#endregion

	#region Specific Data
	public DiactricalMarkWordResource WordInfo { get; set; }
	#endregion

	#region Private Fields
	// LetterBlockBuilder is now initialized in ValidateAndLoadData
	private NoLetterBlock _noLetterBlockInstance; // Keep track if needed
	#endregion

	#region Abstract Method Implementations

	protected override bool ValidateAndLoadData()
	{
		// 1. Validate Data Source
		if (WordInfo == null)
		{
			GD.PrintErr($"{GetType().Name} '{Name}': {nameof(WordInfo)} is required but was not set.");
			return false;
		}

		// 2. Validate PackedScenes
		if (LetterBlockPackedScene == null)
		{
			GD.PrintErr($"{GetType().Name} '{Name}': {nameof(LetterBlockPackedScene)} is required.");
			return false;
		}

		if (NoLetterBlockPackedScene == null)
		{
			GD.PrintErr($"{GetType().Name} '{Name}': {nameof(NoLetterBlockPackedScene)} is required for {GetType().Name}.");
			return false;
		}

		// 3. Initialize Instance-Specific Builder (Fixing static issue)
		// Pass both scenes needed by this derived class
		_letterBuilder = new LetterBlockBuilder(LetterBlockPackedScene, NoLetterBlockPackedScene);

		return true; // Validation passed
	}

	protected override void BuildAndPositionBlocksInternal()
	{
		// 1. Build the initial "No Mark" block
		float currentX = BuildNoLetterBlock();

		// 2. Build the actual letter blocks
		if (WordInfo?.WithoutMark != null) // Null check for safety
		{
			foreach ((char letter, int idx) in WordInfo.WithoutMark.Select((letter, idx) => (letter, idx)))
			{
				currentX = BuildSingleBlock(currentX, letter, idx); // Pass char as blockData
			}
		}

		// 3. Center the layout
		CenterBlocksLayout(currentX);
	}

	protected override float BuildSingleBlock(float currentX, object blockData, int index)
	{
		if (blockData is not char letter) // Ensure correct data type
		{
			GD.PrintErr($"{GetType().Name} '{Name}': Invalid blockData type provided to BuildSingleBlock. Expected char.");
			return currentX;
		}

		bool isTarget = WordInfo.HasMark && WordInfo.MarkIndex == index;

		LetterBlock letterBlock = _letterBuilder.BuildLetterBlock(
			letter, // Pass char directly
			new Vector2(currentX, 0),
			isTarget
		);

		if (isTarget)
		{
			Target = letterBlock; // Update protected property
			TargetIdx = index;   // Update protected property
		}

		ConfigureBlockSignals(letterBlock, index); // Call common signal config

		AddChild(letterBlock);
		LetterBlocks.Enqueue(letterBlock); // Add to protected queue

		return CalculateNextBlockStartPosition(letterBlock, currentX); // Use base helper
	}

	protected override void ConfigureBlockSignals(LetterBlock letterBlock, int index)
	{
		// Connect common destruction signal
		letterBlock.OnLetterDestructedSignal += OnLetterBlockDestroyed; // Use base handler

		// Connect internal collision disabling signal for non-targets
		if (!letterBlock.IsTarget)
		{
			// Use base signal name (ensure typo is fixed if base definition had it)
			OnDisableChildrenCollisionsInternalSignal += letterBlock.DisableCollisions;
		}

		// Word.cs doesn't emit ReadyToDequeueSignal based on letter index,
		// it uses the NoLetterBlock for that (handled in BuildNoLetterBlock).
	}

	protected override void ConnectTargetSignal()
	{
		// Connect the target block's signal (assuming name) to the base Destroy method
		if (Target != null)
		{
			Target.OnTargetBlockCalledDestructionSignal += Destroy;
			// _targetSignalConnected = true; // Set flag if needed by base Disconnect
		}
	}

	#endregion

	#region Specific Helper Methods (Word Class)

	private float BuildNoLetterBlock()
	{
		bool isTarget = WordInfo.HasMark is false; // Target if word has no mark

		_noLetterBlockInstance = _letterBuilder.BuildNoLetterBlock(
			new Vector2(0.0f, 0.0f),
			isTarget
		);

		if (isTarget)
		{
			Target = _noLetterBlockInstance; // Can be the target
			TargetIdx = -1; // Special index for no-letter block
		}

		// The 'NoLetterBlock' is always the last destroyed, so it emits the final signal
		_noLetterBlockInstance.OnReadyToDequeueSignal += () => EmitSignal(SignalName.ReadyToDequeueSignal);

		ConfigureBlockSignals(_noLetterBlockInstance, -1); // Configure its signals too

		AddChild(_noLetterBlockInstance);
		LetterBlocks.Enqueue(_noLetterBlockInstance);

		return CalculateNextBlockStartPosition(_noLetterBlockInstance, 0.0f);
	}

	#endregion

	#region Overrides (Optional)
	// Override Destroy or other virtual methods from base class if needed
	#endregion
}