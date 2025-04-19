using System; // For Exception and ArgumentNullException
using Alfaebeto.Blocks; // Assuming Word is here
using AlfaEBetto.Data.Words;
using Godot;

namespace AlfaEBetto.Blocks; // Corrected namespace based on previous files

/// <summary>
/// Responsible for instantiating and performing initial configuration
/// of Word scene instances.
/// </summary>
public sealed class WordBuilder
{
	private readonly PackedScene _wordBlockPackedScene;

	/// <summary>
	/// Creates a new WordBuilder.
	/// </summary>
	/// <param name="packedScene">The PackedScene resource for the Word scene. Must not be null.</param>
	/// <exception cref="ArgumentNullException">Thrown if packedScene is null.</exception>
	public WordBuilder(PackedScene packedScene) =>
		// Validate the PackedScene immediately on construction
		_wordBlockPackedScene = packedScene ?? throw new ArgumentNullException(nameof(packedScene), "Word PackedScene cannot be null.");

	/// <summary>
	/// Builds a new Word instance.
	/// </summary>
	/// <param name="wordInfo">The data defining the word and its potential diacritical mark.</param>
	/// <param name="startPosition">The initial global position for the Word node.</param>
	/// <returns>The configured Word node, or null if instantiation fails.</returns>
	public Word BuildWord(DiactricalMarkWordResource wordInfo, Vector2 startPosition)
	{
		// Validate input data
		if (wordInfo == null)
		{
			GD.PrintErr($"{nameof(WordBuilder)}: Cannot build Word, provided {nameof(DiactricalMarkWordResource)} is null.");
			return null;
		}

		try
		{
			// Instantiate the scene using the generic method for type safety
			Word wordNode = _wordBlockPackedScene.Instantiate<Word>();

			// Check if instantiation actually returned the expected type
			// Instantiate<T> returns null if the root node is not of type T
			if (wordNode == null)
			{
				GD.PrintErr($"{nameof(WordBuilder)}: Failed to instantiate scene '{_wordBlockPackedScene.ResourcePath}'. The root node is not of type '{nameof(Word)}'.");
				return null;
			}

			// Configure the instantiated node
			wordNode.GlobalPosition = startPosition; // Use GlobalPosition
			wordNode.WordInfo = wordInfo;

			return wordNode;
		}
		catch (Exception ex) // Catch potential errors during instantiation
		{
			GD.PrintErr($"{nameof(WordBuilder)}: Exception during instantiation or setup of scene '{_wordBlockPackedScene.ResourcePath}'. Error: {ex.Message}");
			return null; // Return null on failure
		}
	}
}