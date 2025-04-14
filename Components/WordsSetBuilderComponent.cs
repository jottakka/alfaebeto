using Godot;

public partial class WordsSetBuilderComponent : Node
{
	[Export]
	public PackedScene WordsSetPackedScene { get; set; }

	private static WordsSetBuilder _wordsSetBuilder = null;

	public override void _Ready()
	{
		_wordsSetBuilder ??= new WordsSetBuilder(WordsSetPackedScene);
	}

	public WordsSet BuildWordsSet(GuessBlockWordResource guessBlockInfo, Vector2 position, int numberOfWrongOptions)
	{
		WordsSet wordsSet = _wordsSetBuilder.BuildWordSet(guessBlockInfo, position);
		return wordsSet;
	}

	public WordsSet BuildArticleSet(GuessBlockWordResource guessBlockInfo, Vector2 position, int numberOfWrongOptions)
	{
		WordsSet wordsSet = _wordsSetBuilder.BuildArticleSet(guessBlockInfo, position);
		return wordsSet;
	}
}
