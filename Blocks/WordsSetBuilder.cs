using Godot;

public sealed class WordsSetBuilder
{
	private readonly PackedScene _wordsSetPackedScene;

	public WordsSetBuilder(PackedScene packedScene)
	{
		_wordsSetPackedScene = packedScene;
	}

	public WordsSet BuildWord(GuessBlockWordResource guessBlockWord, Vector2 startPosition, int numberOfWrongOptions)
	{
		WordsSet wordsSetNode = _wordsSetPackedScene.Instantiate<WordsSet>();
		wordsSetNode.Position = startPosition;
		wordsSetNode.GuessBlockInfo = guessBlockWord;
		wordsSetNode.NumberOfWrongOptions = numberOfWrongOptions;
		return wordsSetNode;
	}
}
