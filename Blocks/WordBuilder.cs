using Godot;
using WordProcessing.Models.DiacriticalMarks;

public sealed class WordBuilder
{
    private PackedScene _wordBlockPackedScene;

    public WordBuilder(PackedScene packedScene)
    {
        _wordBlockPackedScene = packedScene;
    }

    public Word BuildWord(WordInfo word, Vector2 startPosition)
    {
        Word wordNode = _wordBlockPackedScene.Instantiate<Word>();
        wordNode.Position = startPosition;
        wordNode.WordInfo = word;
        return wordNode;
    }
}
