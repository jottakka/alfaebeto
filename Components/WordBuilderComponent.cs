using Godot;

public partial class WordBuilderComponent : Node
{
	[Export]
	public PackedScene WordPackedScene { get; set; }

	public static Word BuildWord(string wordText, Vector2 position)
	{
		var word = new Word();
		word.WordText = wordText;
		word.Position = position;
		return word;
	}
}
