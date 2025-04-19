using System;
using Alfaebeto.Blocks; // Assuming Word and LetterBlockBuilder are here
using AlfaEBetto.Blocks;
using AlfaEBetto.Data.Words;
using Godot;

namespace Alfaebeto.Components; // Corrected namespace based on other files

/// <summary>
/// Component responsible for building Word instances using a configured PackedScene.
/// </summary>
public partial class WordBuilderComponent : Node
{
	[Export] public PackedScene WordPackedScene { get; set; }

	// Instance field - each component gets its own builder
	private WordBuilder _wordBuilder;
	private bool _isInitialized = false; // Flag to track initialization

	public override void _Ready() => InitializeBuilder();

	private void InitializeBuilder()
	{
		if (_isInitialized)
		{
			return; // Prevent double initialization
		}

		// Validate required export
		if (WordPackedScene == null)
		{
			GD.PrintErr($"{Name}: Exported property '{nameof(WordPackedScene)}' is not assigned. WordBuilderComponent cannot function.");
			// Optionally deactivate or queue free
			SetProcess(false);
			SetPhysicsProcess(false);
			return;
		}

		// Create an instance builder, passing the specific PackedScene for this component
		// Assuming WordBuilder constructor takes the PackedScene
		try
		{
			_wordBuilder = new WordBuilder(WordPackedScene);
			_isInitialized = true;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"{Name}: Failed to initialize WordBuilder. Error: {ex.Message}");
			// Deactivate on failure
			SetProcess(false);
			SetPhysicsProcess(false);
		}
	}

	/// <summary>
	/// Builds a new Word instance using the configured builder and scene.
	/// </summary>
	/// <param name="wordInfo">Data for the word.</param>
	/// <param name="position">Global position to spawn the word.</param>
	/// <returns>The instantiated Word node, or null if building failed.</returns>
	public Word BuildWord(DiactricalMarkWordResource wordInfo, Vector2 position)
	{
		// Ensure initialization happened (e.g., if called before _Ready)
		if (!_isInitialized)
		{
			GD.PrintErr($"{Name}: BuildWord called before initialization or initialization failed.");
			// Optionally try initializing now, but usually indicates a logic error elsewhere
			// InitializeBuilder();
			// if (!_isInitialized) return null; // Return null if still not initialized
			return null;
		}

		// Delegate to the instance builder
		try
		{
			// Assuming the builder method handles potential instantiation errors
			Word word = _wordBuilder.BuildWord(wordInfo, position);
			return word;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"{Name}: Error during BuildWord execution. Error: {ex.Message}");
			return null;
		}
	}
}