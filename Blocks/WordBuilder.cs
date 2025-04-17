using AlfaEBetto.Data.Words;
using Godot;

namespace AlfaEBetto.Blocks;

public sealed class WordBuilder
{
	private readonly PackedScene _wordBlockPackedScene;

	public WordBuilder(PackedScene packedScene) => _wordBlockPackedScene = packedScene;

	public Word BuildWord(DiactricalMarkWordResource word, Vector2 startPosition)
	{
		Word wordNode = _wordBlockPackedScene.Instantiate<Word>();
		wordNode.Position = startPosition;
		wordNode.WordInfo = word;
		return wordNode;
	}
}
