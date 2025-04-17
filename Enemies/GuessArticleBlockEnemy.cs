using AlfaEBetto.Blocks;
using AlfaEBetto.Data.Words;
using Godot;
using WordProcessing.Util;

namespace Alfaebeto.Enemies;

public sealed partial class GuessArticleBlockEnemy : BaseGuessEnemy // Inherit from base
{
	protected override GuessBlockWordResource GetGuessResource() =>
		// Fetch German specific data
		GermanArticleResourceProvider.GetRandomResource();

	protected override WordsSet BuildBlockSetInternal(GuessBlockWordResource resource, Vector2 position, int numOptions)
	{
		// Call the specific BuildArticleSet method on the component
		if (WordsSetBuilderComponent == null)
		{
			return null;
		}
		// Note: Assuming BuildArticleSet exists and takes similar parameters.
		// If BuildArticleSet has a different signature or doesn't return WordsSet,
		// you might need to adjust the base class or this implementation.
		// For now, assuming it returns WordsSet for consistency:
		return WordsSetBuilderComponent.BuildArticleSet(resource, position, numOptions);
	}
}

// Moved the static class outside the enemy class for better organization
public static class GermanArticleResourceProvider
{
	public static GuessBlockWordResource GetRandomResource()
	{
		// Assuming GermanGenderResourceProvider and GuessArticleWordResource exist
		// and GermanGenderResourceProvider.GetRandomResource() returns GuessArticleWordResource
		GuessArticleWordResource germanWord = GermanGenderResourceProvider.GetRandomResource();
		if (germanWord == null)
		{
			return null; // Handle potential null return
		}

		return new GuessBlockWordResource // Convert to the common resource type
		{
			AnswerIdx = germanWord.AnswerIdx,
			ShuffledOptions = germanWord.ArticleOptions, // Map correct property
			ToBeGuessed = germanWord.ToBeGuessed
		};
	}
}
