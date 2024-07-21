using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Godot;

public sealed class WordBuilder
{
    private PackedScene _wordBlockPackedScene;

    public WordBuilder(PackedScene packedScene)
    {
        _wordBlockPackedScene = packedScene;
    }

    public Word BuildWord(string word, Vector2 startPosition)
    {
        Word wordNode = _wordBlockPackedScene.Instantiate<Word>();
        wordNode.Position = startPosition;
        wordNode.WordText = word;
        return wordNode;
    }
}
