using System; // For Exception
using Alfaebeto.Blocks; // Assuming WordsSet is here
using AlfaEBetto.Data.Words;
using Godot;

namespace AlfaEBetto.Blocks; // Assuming this is the correct namespace for the builder too

/// <summary>
/// Responsible for instantiating and performing initial configuration
/// of WordsSet scene instances.
/// </summary>
public sealed class WordsSetBuilder
{
	private readonly PackedScene _wordsSetPackedScene;

	/// <summary>
	/// Creates a new WordsSetBuilder.
	/// </summary>
	/// <param name="packedScene">The PackedScene resource for the WordsSet scene. Must not be null.</param>
	/// <exception cref="ArgumentNullException">Thrown if packedScene is null.</exception>
	public WordsSetBuilder(PackedScene packedScene) =>
		// Validate the PackedScene immediately on construction
		_wordsSetPackedScene = packedScene ?? throw new ArgumentNullException(nameof(packedScene), "WordsSet PackedScene cannot be null.");

	/// <summary>
	/// Builds a standard WordsSet instance.
	/// </summary>
	/// <param name="guessBlockWord">The data defining the word/options.</param>
	/// <param name="startPosition">The initial global position for the WordsSet node.</param>
	/// <returns>The configured WordsSet node, or null if instantiation fails.</returns>
	public WordsSet BuildWordSet(GuessBlockWordResource guessBlockWord, Vector2 startPosition) =>
		// Delegate to the internal builder, setting isArticle to false
		BuildInternal(guessBlockWord, startPosition, isArticle: false);

	/// <summary>
	/// Builds a WordsSet instance configured for German articles.
	/// </summary>
	/// <param name="guessBlockWord">The data defining the articles/options.</param>
	/// <param name="startPosition">The initial global position for the WordsSet node.</param>
	/// <returns>The configured WordsSet node, or null if instantiation fails.</returns>
	public WordsSet BuildArticleSet(GuessBlockWordResource guessBlockWord, Vector2 startPosition) =>
		// Delegate to the internal builder, setting isArticle to true
		BuildInternal(guessBlockWord, startPosition, isArticle: true);

	/// <summary>
	/// Internal helper method to perform the actual instantiation and common setup.
	/// </summary>
	private WordsSet BuildInternal(GuessBlockWordResource guessBlockWord, Vector2 startPosition, bool isArticle)
	{
		// Validate input data
		if (guessBlockWord == null)
		{
			GD.PrintErr($"{nameof(WordsSetBuilder)}: Cannot build WordsSet, provided {nameof(GuessBlockWordResource)} is null.");
			return null;
		}

		try
		{
			// Instantiate the scene using the generic method for type safety
			WordsSet wordsSetNode = _wordsSetPackedScene.Instantiate<WordsSet>();

			// Check if instantiation actually returned the expected type
			// Instantiate<T> returns null if the root node is not of type T
			if (wordsSetNode == null)
			{
				GD.PrintErr($"{nameof(WordsSetBuilder)}: Failed to instantiate scene '{_wordsSetPackedScene.ResourcePath}'. The root node is not of type '{nameof(WordsSet)}'.");
				return null;
			}

			// Configure the instantiated node
			wordsSetNode.GlobalPosition = startPosition; // Use GlobalPosition for consistency
			wordsSetNode.GuessBlockInfo = guessBlockWord;
			wordsSetNode.IsGermanArticle = isArticle; // Set based on parameter

			return wordsSetNode;
		}
		catch (Exception ex) // Catch potential errors during instantiation
		{
			GD.PrintErr($"{nameof(WordsSetBuilder)}: Exception during instantiation or setup of scene '{_wordsSetPackedScene.ResourcePath}'. Error: {ex.Message}");
			return null; // Return null on failure
		}
	}
}