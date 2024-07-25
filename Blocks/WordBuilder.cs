using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
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
