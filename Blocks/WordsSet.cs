using System.Collections.Generic;
using System.Linq;
using AlfaEBetto.Data.Words;
using Godot;

namespace AlfaEBetto.Blocks;

/// <summary>
/// Manages a set of interactive letter/word blocks for a guessing activity,
/// potentially involving German articles. Handles block creation, layout,
/// interaction, and destruction animations.
/// Public members retain original names and accessibility.
/// </summary>
public partial class WordsSet : Node2D
{
	// --- Exports (Public by nature) ---
	[Export]
	public PackedScene LetterBlockPackedScene { get; set; }

	[Export]
	public PackedScene WordBlockPackedScene { get; set; } // Note: Still unused in this logic.

	[Export]
	public PackedScene ThreeArticleBlockPackedScene { get; set; }

	// --- Signals (Public delegates, names preserved, typo fixed) ---
	[Signal]
	public delegate void ReadyToDequeueSignalEventHandler(); // Original Name

	[Signal]
	public delegate void OnLetterDestructedSignalEventHandler(bool isTarget); // Original Name

	[Signal]
	public delegate void OnDisableChildrenCollisionsInternalSignalEventHandler(); // Original Name (Typo corrected)

	// --- Public Properties (Names and accessibility preserved) ---
	public GuessBlockWordResource GuessBlockInfo { get; set; }

	public bool IsGermanArticle { get; set; } = false;

	public float CenterOffset { get; set; } = 0.0f; // Public setter retained

	public int TargetIdx { get; set; }

	public Queue<LetterBlock> LetterBlocks { get; set; } = new(); // Public setter retained

	public LetterBlock Target { get; set; } // Public setter retained

	// --- Private Fields ---
	private LetterBlockBuilder _letterBuilder; // Instance field (non-static)
	private readonly Timer _destructionTimer = new();
	private const float DestructionInterval = 0.25f;

	// --- Godot Methods ---
	public override void _Ready()
	{
		// Input Validation
		if (GuessBlockInfo == null)
		{
			GD.PrintErr($"{nameof(WordsSet)} requires a {nameof(GuessBlockInfo)} to be set.");
			return;
		}

		PackedScene blockScene = IsGermanArticle ? ThreeArticleBlockPackedScene : LetterBlockPackedScene;

		if (blockScene == null)
		{
			GD.PrintErr($"{nameof(WordsSet)} is missing a required PackedScene. Assign {(IsGermanArticle ? nameof(ThreeArticleBlockPackedScene) : nameof(LetterBlockPackedScene))} in the inspector.");
			return;
		}

		// Initialize the instance-specific builder
		_letterBuilder = new LetterBlockBuilder(blockScene, null); // Adjust constructor if needed

		AddChild(_destructionTimer);
		_destructionTimer.Timeout += OnDestructionTimerTimeout;

		// Renamed internal setup method, remains private
		BuildAndPositionBlocks();
	}

	// --- Public Methods (Names preserved) ---

	/// <summary>
	/// Initiates the destruction sequence for the blocks.
	/// Starts by destroying the first block, then subsequent blocks via timer.
	/// Original public method name preserved.
	/// </summary>
	public void Destroy() // Original Name
	{
		if (LetterBlocks.TryDequeue(out LetterBlock firstBlock))
		{
			firstBlock.Destroy();
			if (LetterBlocks.Any())
			{
				_destructionTimer.Start(DestructionInterval);
			}
		}
		else
		{
			GD.Print("Destroy called but no letter blocks remain.");
			_destructionTimer.Stop();
		}
	}

	// --- Private Helper Methods (Internal improvements) ---

	/// <summary>
	/// Creates, configures, and positions all letter/word blocks based on GuessBlockInfo.
	/// (Internal method, previously implicitly private, now explicitly private)
	/// </summary>
	private void BuildAndPositionBlocks() // Was BuildWordBlocks implicitly private, now explicitly private with clearer name
	{
		if (_letterBuilder == null)
		{
			GD.PrintErr($"Cannot build blocks: {nameof(_letterBuilder)} was not initialized.");
			return;
		}

		float currentX = 0f;

		// Using original LINQ approach as it was concise
		foreach ((string word, int idx) in GuessBlockInfo.ShuffledOptions.Select((letter, idx) => (letter, idx)))
		{
			currentX = BuildSingleBlock(currentX, word, idx, GuessBlockInfo.AnswerIdx);
		}

		if (Target == null)
		{
			GD.PrintErr("Target block was not set after building blocks. Check AnswerIdx validity.");
		}
		else
		{
			// Connect signal only after all blocks are potentially created
			// Using original signal name from LetterBlock (assuming it exists)
			Target.OnTargetBlockCalledDestructionSignal += Destroy;
		}

		CenterBlocksLayout(currentX); // Renamed internal method
	}

	/// <summary>
	/// Builds and configures a single letter/word block.
	/// </summary>
	/// <returns>The X position for the start of the *next* block.</returns>
	private float BuildSingleBlock(float currentX, string word, int index, int answerIndex)
	{
		bool isTarget = answerIndex == index;
		Color? blockColor = IsGermanArticle ? GetColorFromGermanArticle(word) : null;

		LetterBlock letterBlock = _letterBuilder.BuildLetterBlock(
			word,
			new Vector2(currentX, 0),
			isTarget,
			blockColor
		);

		ConfigureBlockSignals(letterBlock, index, isTarget); // Pass isTarget

		AddChild(letterBlock);
		LetterBlocks.Enqueue(letterBlock); // Uses the public LetterBlocks property

		// Update public Target property if this is the target block
		if (isTarget)
		{
			Target = letterBlock; // Uses the public Target property
			TargetIdx = index;    // Uses the public TargetIdx property
		}

		return CalculateNextBlockStartPosition(letterBlock, currentX); // Renamed internal method
	}

	/// <summary>
	/// Connects signals for a newly created LetterBlock.
	/// </summary>
	private void ConfigureBlockSignals(LetterBlock letterBlock, int index, bool isTarget)
	{
		// Connect signals using original public delegate names
		if (index == 0)
		{
			letterBlock.OnReadyToDequeueSignal += () => EmitSignal(SignalName.ReadyToDequeueSignal); // Original Signal Name
		}

		letterBlock.OnLetterDestructedSignal += OnLetterBlockDestroyed; // Connect to private handler

		if (!isTarget)
		{
			// Connect non-target blocks to the internal signal for disabling collisions
			// Using original signal name (with typo fixed)
			OnDisableChildrenCollisionsInternalSignal += letterBlock.DisableCollisions;
		}
	}

	/// <summary>
	/// Calculates the starting X position for the next block based on the current block's size.
	/// </summary>
	private static float CalculateNextBlockStartPosition(LetterBlock letterBlock, float currentBlockX)
	{
		if (letterBlock.CollisionShape == null || letterBlock.CollisionShape.Shape is not RectangleShape2D shape)
		{
			GD.PrintErr($"LetterBlock '{letterBlock.Name ?? "Unnamed"}' lacks a valid RectangleShape2D CollisionShape.");
			return currentBlockX + 50; // Fallback spacing
		}

		float blockWidth = shape.Size.X * 2;
		const float gap = 5.0f; // Optional gap
		return currentBlockX + blockWidth + gap;
	}

	/// <summary>
	/// Centers the entire set of blocks horizontally relative to this node's origin.
	/// Uses the public CenterOffset property.
	/// </summary>
	private void CenterBlocksLayout(float totalWidth) // Renamed internal method
	{
		CenterOffset = totalWidth / 2.0f; // Uses the public CenterOffset property
		Position = new Vector2(-CenterOffset, Position.Y);
	}

	/// <summary>
	/// Handles the Timeout signal from the destruction timer.
	/// </summary>
	private void OnDestructionTimerTimeout()
	{
		if (LetterBlocks.TryDequeue(out LetterBlock nextBlock)) // Uses public LetterBlocks property
		{
			nextBlock.Destroy();
			if (!LetterBlocks.Any()) // Uses public LetterBlocks property
			{
				_destructionTimer.Stop();
			}
		}
		else
		{
			_destructionTimer.Stop();
		}
	}

	/// <summary>
	/// Handles the destruction signal from individual letter blocks.
	/// Emits public signals using original names.
	/// </summary>
	private void OnLetterBlockDestroyed(bool isTarget) // Private handler
	{
		if (isTarget)
		{
			// Emit the internal signal using original name (typo fixed)
			EmitSignal(SignalName.OnDisableChildrenCollisionsInternalSignal);
		}
		// Forward the signal externally using original name
		EmitSignal(SignalName.OnLetterDestructedSignal, isTarget);
	}

	/// <summary>
	/// Determines the color based on the German article. (Internal helper)
	/// </summary>
	private Color GetColorFromGermanArticle(string germanArticle)
	{
		return germanArticle.ToLowerInvariant() switch
		{
			"der" => WordGender.Masculine.ToColor(),
			"die" => WordGender.Feminine.ToColor(),
			"das" => WordGender.Neuter.ToColor(),
			_ => Colors.Black
		};
	}
}
