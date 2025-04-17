using AlfaEBetto.Data.Words;
using Godot;

namespace AlfaEBetto.Blocks;

public sealed class WordsSetBuilder
{
	private readonly PackedScene _wordsSetPackedScene;

	public WordsSetBuilder(PackedScene packedScene) => _wordsSetPackedScene = packedScene;

	public WordsSet BuildWordSet(GuessBlockWordResource guessBlockWord, Vector2 startPosition)
	{
		WordsSet wordsSetNode = _wordsSetPackedScene.Instantiate<WordsSet>();
		wordsSetNode.Position = startPosition;
		wordsSetNode.GuessBlockInfo = guessBlockWord;
		return wordsSetNode;
	}

	public WordsSet BuildArticleSet(GuessBlockWordResource guessBlockWord, Vector2 startPosition)
	{
		WordsSet wordsSetNode = _wordsSetPackedScene.Instantiate<WordsSet>();
		wordsSetNode.Position = startPosition;
		wordsSetNode.GuessBlockInfo = guessBlockWord;
		wordsSetNode.IsGermanArticle = true;
		return wordsSetNode;
	}
}
