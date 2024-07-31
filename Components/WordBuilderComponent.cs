using Godot;
using WordProcessing.Models.DiacriticalMarks;

public partial class WordBuilderComponent : Node
{
    [Export]
    public PackedScene WordPackedScene { get; set; }

    private static WordBuilder _wordBuilder = null;

    public override void _Ready()
    {
        _wordBuilder ??= new WordBuilder(WordPackedScene);
    }

    public Word BuildWord(WordInfo wordInfo, Vector2 position)
    {
        Word word = _wordBuilder.BuildWord(wordInfo, position);
        return word;
    }
}
