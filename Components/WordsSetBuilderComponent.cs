using System; // For Exception
using Alfaebeto.Blocks; // Assuming WordsSet and WordsSetBuilder are here
using AlfaEBetto.Blocks;
using AlfaEBetto.Data.Words;
using Godot;

namespace AlfaEBetto.Components; // Corrected namespace

/// <summary>
/// Component responsible for building WordsSet instances using a configured PackedScene.
/// Handles building standard word sets and German article sets.
/// </summary>
public partial class WordsSetBuilderComponent : Node
{
	[Export] public PackedScene WordsSetPackedScene { get; set; }

	// Instance field - each component gets its own builder
	private WordsSetBuilder _wordsSetBuilder;
	private bool _isInitialized = false; // Flag to track initialization

	public override void _Ready() => InitializeBuilder();

	private void InitializeBuilder()
	{
		if (_isInitialized)
		{
			return;
		}

		// Validate required export
		if (WordsSetPackedScene == null)
		{
			GD.PrintErr($"{Name}: Exported property '{nameof(WordsSetPackedScene)}' is not assigned. WordsSetBuilderComponent cannot function.");
			SetProcess(false);
			SetPhysicsProcess(false);
			return;
		}

		// Create an instance builder, passing the specific PackedScene for this component
		// Assuming WordsSetBuilder constructor takes the PackedScene
		try
		{
			_wordsSetBuilder = new WordsSetBuilder(WordsSetPackedScene);
			_isInitialized = true;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"{Name}: Failed to initialize WordsSetBuilder. Error: {ex.Message}");
			SetProcess(false);
			SetPhysicsProcess(false);
		}
	}

	/// <summary>
	/// Builds a new standard WordsSet instance.
	/// </summary>
	/// <param name="guessBlockInfo">Data for the word set.</param>
	/// <param name="position">Global position to spawn the set.</param>
	/// <returns>The instantiated WordsSet node, or null if building failed.</returns>
	// Removed unused 'numberOfWrongOptions' parameter
	public WordsSet BuildWordsSet(GuessBlockWordResource guessBlockInfo, Vector2 position)
	{
		if (!_isInitialized)
		{
			GD.PrintErr($"{Name}: BuildWordsSet called before initialization or initialization failed.");
			return null;
		}

		try
		{
			WordsSet wordsSet = _wordsSetBuilder.BuildWordSet(guessBlockInfo, position);
			return wordsSet;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"{Name}: Error during BuildWordsSet execution. Error: {ex.Message}");
			return null;
		}
	}

	/// <summary>
	/// Builds a new German article WordsSet instance.
	/// </summary>
	/// <param name="guessBlockInfo">Data for the article set.</param>
	/// <param name="position">Global position to spawn the set.</param>
	/// <returns>The instantiated WordsSet node, or null if building failed.</returns>
	// Removed unused 'numberOfWrongOptions' parameter
	public WordsSet BuildArticleSet(GuessBlockWordResource guessBlockInfo, Vector2 position)
	{
		if (!_isInitialized)
		{
			GD.PrintErr($"{Name}: BuildArticleSet called before initialization or initialization failed.");
			return null;
		}

		try
		{
			// Assuming WordsSetBuilder has a distinct method for articles
			WordsSet wordsSet = _wordsSetBuilder.BuildArticleSet(guessBlockInfo, position);
			return wordsSet;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"{Name}: Error during BuildArticleSet execution. Error: {ex.Message}");
			return null;
		}
	}
}