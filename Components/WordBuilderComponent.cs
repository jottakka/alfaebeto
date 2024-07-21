using Godot;

public partial class WordBuilderComponent : Node
{
	[Export]
	public PackedScene WordPackedScene { get; set; }

	private static WordBuilder _wordBuilder = null;

	public override void _Ready()
	{
		if (_wordBuilder == null)
		{
			_wordBuilder = new WordBuilder(WordPackedScene);
		}
	}

	public Word BuildWord(string wordText, Vector2 position)
	{
		var word = _wordBuilder.BuildWord(wordText, position);
		return word;
	}
}
