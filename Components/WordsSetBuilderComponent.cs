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
		WordsSet wordsSet = _wordsSetBuilder.BuildWord(guessBlockInfo, position, numberOfWrongOptions);
		return wordsSet;
	}
}
