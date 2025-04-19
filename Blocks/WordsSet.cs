using System.Linq;
using AlfaEBetto.Blocks;
using AlfaEBetto.Data.Words;
using Godot;

namespace Alfaebeto.Blocks;

public partial class WordsSet : BlockSetBase // Inherit from base
{
	#region Exports (Specific to WordsSet)
	[Export] public PackedScene WordBlockPackedScene { get; set; } // Note: Still seems unused?
	[Export] public PackedScene ThreeArticleBlockPackedScene { get; set; }

	[ExportGroup("Layout")]
	[Export] public float ArticleBlockStretchFactor { get; set; } = 1.5f; // e.g., 1.5 makes it 50% wider
	#endregion


	#region Specific Data & Config
	public GuessBlockWordResource GuessBlockInfo { get; set; }
	public bool IsGermanArticle { get; set; } = false;
	#endregion

	#region Private Fields
	// LetterBlockBuilder is now initialized in ValidateAndLoadData
	#endregion

	#region Abstract Method Implementations

	protected override bool ValidateAndLoadData()
	{
		// 1. Validate Data Source
		if (GuessBlockInfo == null)
		{
			GD.PrintErr($"{GetType().Name} '{Name}': {nameof(GuessBlockInfo)} is required but was not set.");
			return false;
		}

		if (GuessBlockInfo.ShuffledOptions == null || !GuessBlockInfo.ShuffledOptions.Any())
		{
			GD.PrintErr($"{GetType().Name} '{Name}': {nameof(GuessBlockInfo.ShuffledOptions)} is null or empty.");
			return false;
		}

		// 2. Validate PackedScene based on mode
		PackedScene blockScene = IsGermanArticle ? ThreeArticleBlockPackedScene : LetterBlockPackedScene;
		string requiredSceneName = IsGermanArticle ? nameof(ThreeArticleBlockPackedScene) : nameof(LetterBlockPackedScene);

		if (blockScene == null)
		{
			GD.PrintErr($"{GetType().Name} '{Name}': Required PackedScene '{requiredSceneName}' is not assigned.");
			return false;
		}

		// 3. Initialize Instance-Specific Builder
		_letterBuilder = new LetterBlockBuilder(blockScene, null); // Pass required scene

		return true; // Validation passed
	}

	protected override void BuildAndPositionBlocksInternal()
	{
		float currentX = 0f;

		// Build blocks based on ShuffledOptions
		foreach ((string word, int idx) in GuessBlockInfo.ShuffledOptions.Select((word, idx) => (word, idx)))
		{
			currentX = BuildSingleBlock(currentX, word, idx); // Pass string as blockData
		}

		// Center the layout
		CenterBlocksLayout(currentX);
	}

	protected override float BuildSingleBlock(float currentX, object blockData, int index)
	{
		if (blockData is not string word)
		{
			GD.PrintErr($"{GetType().Name} '{Name}': Invalid blockData type provided to BuildSingleBlock. Expected string.");
			return currentX;
		}

		bool isTarget = GuessBlockInfo.AnswerIdx == index;
		Color? blockColor = IsGermanArticle ? GetColorFromGermanArticle(word) : null;

		LetterBlock letterBlock = _letterBuilder.BuildLetterBlock(
			word,
			new Vector2(currentX, 0), // Initial position before potential scaling adjustment
			isTarget,
			blockColor
		);

		// --- Apply Stretching ---
		if (IsGermanArticle)
		{
			// Ensure factor is positive
			float stretchFactor = Mathf.Max(1.0f, ArticleBlockStretchFactor);
			letterBlock.Scale = new Vector2(stretchFactor, letterBlock.Scale.Y); // Apply horizontal scale
																				 // Note: This scales the entire block node, including sprite, label, and collision shape.
		}
		// ------------------------

		if (isTarget)
		{
			Target = letterBlock;
			TargetIdx = index;
		}

		ConfigureBlockSignals(letterBlock, index);

		AddChild(letterBlock);
		LetterBlocks.Enqueue(letterBlock);

		// Use the modified base helper which now considers scale
		return CalculateNextBlockStartPosition(letterBlock, currentX);
	}

	protected override void ConfigureBlockSignals(LetterBlock letterBlock, int index)
	{
		// Connect common destruction signal
		letterBlock.OnLetterDestructedSignal += OnLetterBlockDestroyed; // Use base handler

		// Emit final signal when the *first* block is ready to dequeue (matches original logic)
		if (index == 0)
		{
			letterBlock.OnReadyToDequeueSignal += () => EmitSignal(SignalName.ReadyToDequeueSignal);
		}

		// Connect internal collision disabling signal for non-targets
		if (!letterBlock.IsTarget)
		{
			OnDisableChildrenCollisionsInternalSignal += letterBlock.DisableCollisions;
		}
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

	#region Specific Helper Methods (WordsSet Class)

	private Color GetColorFromGermanArticle(string germanArticle)
	{
		// Assuming WordGender and ToColor() extension exist
		return germanArticle.ToLowerInvariant() switch
		{
			"der" => WordGender.Masculine.ToColor(),
			"die" => WordGender.Feminine.ToColor(),
			"das" => WordGender.Neuter.ToColor(),
			_ => Colors.White // Default color if article not recognized
		};
	}

	#endregion
}
